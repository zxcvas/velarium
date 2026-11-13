#!/usr/bin/env python3
"""Dispatch one PixelLab batch with a few jobs in flight.

One artist agent owns one manifest. A second process that opens the same
manifest skips jobs already claimed, so it does not submit them again.

Token: pixellab.env or PIXELLAB_API_TOKEN. This file never prints it.

Dry-run is the default. --run is what spends. A manifest whose gate is
wave-3b, wave-3c, or wave-4 refuses --run until --hold-lifted.
"""

from __future__ import annotations

import argparse
import json
import os
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from pixellab_gen import PROMPTS, api, palette_body, print_balance  # noqa: E402

HELD_GATES = {
    "wave-3b": "Wave 3b stays held until a human lifts it (Godot walk/palus hook).",
    "wave-3c": "Wave 3c stays held until a human lifts it.",
    "wave-4": "Wave 4 stays held until a human lifts it.",
}
MAX_WORKERS = 4
CLAIM_ROOT = PROMPTS / "batch-claims"
TERMINAL_OK = {"completed", "complete", "success"}
TERMINAL_BAD = {"failed", "error"}
SECRET_KEYS = {"authorization", "token", "api_key", "apikey"}


def load_manifest(path: Path) -> dict:
    return load_manifest_data(json.loads(path.read_text(encoding="utf-8")))


def load_manifest_data(data: object) -> dict:
    if not isinstance(data, dict):
        raise SystemExit("manifest must be an object")
    batch_id = str(data.get("id") or "").strip()
    if not batch_id or any(c.isspace() for c in batch_id) or "/" in batch_id or "\\" in batch_id:
        raise SystemExit("manifest id must be a single path segment")
    jobs = data.get("jobs")
    if not isinstance(jobs, list) or not jobs:
        raise SystemExit("manifest jobs must be a non-empty list")
    seen: set[str] = set()
    for job in jobs:
        if not isinstance(job, dict):
            raise SystemExit("each job must be an object")
        jid = str(job.get("id") or "").strip()
        if not jid or jid in seen or "/" in jid or "\\" in jid:
            raise SystemExit(f"bad or duplicate job id: {jid!r}")
        seen.add(jid)
        route = str(job.get("path") or "")
        if not route.startswith("/") or route.startswith("//"):
            raise SystemExit(f"{jid}: path must start with a single /")
        body = job.get("body")
        if not isinstance(body, dict):
            raise SystemExit(f"{jid}: body must be an object")
        for key in body:
            if str(key).lower() in SECRET_KEYS:
                raise SystemExit(f"{jid}: body must not carry a secret field")
        pull = job.get("pull")
        if pull is not None:
            if not isinstance(pull, dict):
                raise SystemExit(f"{jid}: pull must be an object")
            for need in ("slug", "clip", "character_id"):
                if not pull.get(need):
                    raise SystemExit(f"{jid}: pull.{need} is required")
    workers = int(data.get("workers") or 3)
    if workers < 1:
        raise SystemExit("workers must be >= 1")
    data["id"] = batch_id
    data["workers"] = min(workers, MAX_WORKERS)
    data["gate"] = data.get("gate") or None
    return data


def claim_path(root: Path, batch_id: str, job_id: str) -> Path:
    return root / batch_id / f"{job_id}.json"


def read_claim(root: Path, batch_id: str, job_id: str) -> dict | None:
    path = claim_path(root, batch_id, job_id)
    if not path.is_file():
        return None
    return json.loads(path.read_text(encoding="utf-8"))


def create_claim(root: Path, batch_id: str, job_id: str) -> dict | None:
    """Exclusive create. None means another process already owns the job."""
    path = claim_path(root, batch_id, job_id)
    path.parent.mkdir(parents=True, exist_ok=True)
    payload = {"id": job_id, "status": "started", "job_id": "", "error": None}
    try:
        fd = os.open(path, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o644)
    except FileExistsError:
        return None
    with os.fdopen(fd, "w", encoding="utf-8") as handle:
        json.dump(payload, handle)
    return payload


def write_claim(root: Path, batch_id: str, job_id: str, payload: dict) -> None:
    path = claim_path(root, batch_id, job_id)
    path.parent.mkdir(parents=True, exist_ok=True)
    tmp = path.with_suffix(".tmp")
    tmp.write_text(json.dumps(payload), encoding="utf-8")
    tmp.replace(path)


def gate_block(manifest: dict, hold_lifted: bool) -> str | None:
    gate = manifest.get("gate")
    if not gate or hold_lifted:
        return None
    return HELD_GATES.get(str(gate))


def dry_plan(manifest: dict) -> list[str]:
    gate = manifest.get("gate") or "-"
    lines = [
        f"batch {manifest['id']} jobs={len(manifest['jobs'])} "
        f"workers={manifest['workers']} gate={gate}"
    ]
    for job in manifest["jobs"]:
        pull = job.get("pull") or {}
        extra = f" pull={pull.get('slug')}/{pull.get('clip')}" if pull else ""
        lines.append(f"  {job['id']} POST {job['path']}{extra}")
    lines.append("dry-run: no claim, no POST")
    return lines


def job_id_from(resp: dict) -> str:
    ids = resp.get("background_job_ids") or []
    if ids:
        return str(ids[0])
    if resp.get("background_job_id"):
        return str(resp["background_job_id"])
    return ""


def retryable(exc: BaseException) -> bool:
    text = str(exc)
    return "HTTP 429" in text or "HTTP 503" in text


def call_submit(job: dict, submit, pause) -> dict:
    delay = 0.4
    for attempt in range(4):
        try:
            return submit(job)
        except KeyboardInterrupt:
            raise
        except BaseException as exc:
            if not retryable(exc) or attempt == 3:
                raise
            pause(delay)
            delay *= 2
    raise RuntimeError("submit retries exhausted")  # pragma: no cover


def prepare_body(job: dict) -> dict:
    body = dict(job["body"])
    if job.get("attach_palette"):
        pal = palette_body()
        if pal:
            body["color_image"] = pal
            body["force_colors"] = True
    return body


def default_submit(job: dict) -> dict:
    return api("POST", job["path"], prepare_body(job))


def default_poll(job_id: str, timeout_s: int = 600) -> dict:
    deadline = time.time() + timeout_s
    while time.time() < deadline:
        found = api("GET", f"/background-jobs/{job_id}")
        status = (found.get("status") or "").lower()
        if status in TERMINAL_OK or status in TERMINAL_BAD:
            return found
        time.sleep(5)
    raise TimeoutError(f"job timed out: {job_id}")


def default_pull(pull: dict) -> dict[str, str]:
    import pixellab_wave3a

    return pixellab_wave3a.pull_and_save(pull["slug"], pull["clip"], pull["character_id"])


def run_batch(
    manifest: dict,
    *,
    claim_root: Path,
    hold_lifted: bool = False,
    resume: bool = False,
    submit=None,
    poll=None,
    pull=None,
    pause=time.sleep,
) -> list[dict]:
    blocked = gate_block(manifest, hold_lifted)
    if blocked:
        raise SystemExit(blocked)
    submit = submit or default_submit
    poll = poll or default_poll
    pull = pull or default_pull
    workers = int(manifest["workers"])
    sem = threading.Semaphore(workers)
    peak = {"n": 0, "max": 0}
    lock = threading.Lock()
    results: list[dict] = []
    results_lock = threading.Lock()

    def finish(row: dict) -> dict:
        with results_lock:
            results.append(row)
        return row

    def poll_existing(job: dict, existing: dict, remote: str) -> dict:
        with sem:
            with lock:
                peak["n"] += 1
                peak["max"] = max(peak["max"], peak["n"])
            try:
                final = poll(remote)
                status = (final.get("status") or "").lower()
                if status in TERMINAL_BAD:
                    existing["status"] = "failed"
                    existing["error"] = json.dumps(final.get("last_response"))[:400]
                    write_claim(claim_root, manifest["id"], job["id"], existing)
                    return finish(
                        {
                            "id": job["id"],
                            "status": "failed",
                            "job_id": remote,
                            "error": existing["error"],
                        }
                    )
                files = pull(job["pull"]) if job.get("pull") else {}
                existing["status"] = "completed"
                existing["files"] = files
                existing["error"] = None
                write_claim(claim_root, manifest["id"], job["id"], existing)
                return finish(
                    {"id": job["id"], "status": "completed", "job_id": remote, "files": files}
                )
            except KeyboardInterrupt:
                raise
            except BaseException as exc:
                existing["error"] = str(exc)[:500]
                write_claim(claim_root, manifest["id"], job["id"], existing)
                return finish(
                    {
                        "id": job["id"],
                        "status": "failed",
                        "job_id": remote,
                        "error": existing["error"],
                    }
                )
            finally:
                with lock:
                    peak["n"] -= 1

    def one(index: int, job: dict) -> dict:
        existing = read_claim(claim_root, manifest["id"], job["id"])
        if existing:
            status = existing.get("status")
            remote = existing.get("job_id") or ""
            if status == "completed":
                return finish(
                    {"id": job["id"], "status": "skipped-completed", "job_id": remote}
                )
            if resume and remote:
                return poll_existing(job, existing, remote)
            if resume and not remote:
                return finish(
                    {
                        "id": job["id"],
                        "status": "blocked",
                        "error": "claim started without a PixelLab job id; not submitting again",
                    }
                )
            return finish({"id": job["id"], "status": "skipped-claimed", "job_id": remote})
        if create_claim(claim_root, manifest["id"], job["id"]) is None:
            return finish({"id": job["id"], "status": "skipped-claimed"})
        pause(0.4 * min(index, workers))
        with sem:
            with lock:
                peak["n"] += 1
                peak["max"] = max(peak["max"], peak["n"])
            remote = ""
            try:
                resp = call_submit(job, submit, pause)
                remote = job_id_from(resp)
                payload = {
                    "id": job["id"],
                    "status": "started",
                    "job_id": remote,
                    "error": None,
                }
                write_claim(claim_root, manifest["id"], job["id"], payload)
                final = poll(remote) if remote else resp
                status = (final.get("status") or "completed").lower()
                if status in TERMINAL_BAD:
                    payload["status"] = "failed"
                    payload["error"] = json.dumps(final.get("last_response"))[:400]
                    write_claim(claim_root, manifest["id"], job["id"], payload)
                    return finish(
                        {
                            "id": job["id"],
                            "status": "failed",
                            "job_id": remote,
                            "error": payload["error"],
                        }
                    )
                files = pull(job["pull"]) if job.get("pull") else {}
                payload["status"] = "completed"
                payload["files"] = files
                write_claim(claim_root, manifest["id"], job["id"], payload)
                return finish(
                    {"id": job["id"], "status": "completed", "job_id": remote, "files": files}
                )
            except KeyboardInterrupt:
                raise
            except BaseException as exc:
                prior = read_claim(claim_root, manifest["id"], job["id"]) or {}
                kept = prior.get("job_id") or remote
                payload = {
                    "id": job["id"],
                    "status": "failed",
                    "job_id": kept,
                    "error": str(exc)[:500],
                }
                write_claim(claim_root, manifest["id"], job["id"], payload)
                return finish(
                    {
                        "id": job["id"],
                        "status": "failed",
                        "job_id": kept,
                        "error": payload["error"],
                    }
                )
            finally:
                with lock:
                    peak["n"] -= 1

    with ThreadPoolExecutor(max_workers=workers) as pool:
        futures = [pool.submit(one, i, job) for i, job in enumerate(manifest["jobs"])]
        for fut in as_completed(futures):
            fut.result()
    manifest["_peak_inflight"] = peak["max"]
    order = {job["id"]: i for i, job in enumerate(manifest["jobs"])}
    results.sort(key=lambda row: order.get(row["id"], 0))
    return results


def write_summary(manifest: dict, rows: list[dict]) -> Path:
    PROMPTS.mkdir(parents=True, exist_ok=True)
    path = PROMPTS / f"batch_{manifest['id']}.json"
    payload = {
        "id": manifest["id"],
        "gate": manifest.get("gate"),
        "workers": manifest["workers"],
        "peak_inflight": manifest.get("_peak_inflight"),
        "jobs": [
            {
                "id": row.get("id"),
                "status": row.get("status"),
                "job_id": row.get("job_id") or "",
                "files": row.get("files") or {},
                "error": row.get("error"),
            }
            for row in rows
        ],
    }
    path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")
    return path


def main(argv: list[str] | None = None) -> None:
    parser = argparse.ArgumentParser(description="Dispatch one PixelLab asset batch.")
    parser.add_argument("--check", action="store_true", help="GET /balance and exit")
    parser.add_argument("--manifest", type=Path)
    parser.add_argument("--run", action="store_true", help="Submit the batch. Default is dry-run.")
    parser.add_argument("--resume", action="store_true", help="Poll claims that already have a job id.")
    parser.add_argument("--hold-lifted", action="store_true")
    parser.add_argument("--workers", type=int)
    parser.add_argument("--claim-root", type=Path, default=CLAIM_ROOT)
    args = parser.parse_args(argv)
    if args.check:
        bal = api("GET", "/balance")
        print_balance(bal)
        return
    if args.manifest is None:
        raise SystemExit("pass --manifest, or --check")
    manifest = load_manifest(args.manifest)
    if args.workers is not None:
        if args.workers < 1:
            raise SystemExit("workers must be >= 1")
        manifest["workers"] = min(args.workers, MAX_WORKERS)
    if not args.run:
        print("\n".join(dry_plan(manifest)))
        return
    rows = run_batch(
        manifest,
        claim_root=args.claim_root,
        hold_lifted=args.hold_lifted,
        resume=args.resume,
    )
    summary = write_summary(manifest, rows)
    failed = 0
    for row in rows:
        err = f" {row['error']}" if row.get("error") else ""
        print(f"{row['status']:20} {row['id']}{err}")
        if row["status"] in {"failed", "blocked"}:
            failed += 1
    print(f"summary -> {summary}")
    if failed:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
