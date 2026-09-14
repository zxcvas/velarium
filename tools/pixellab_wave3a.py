#!/usr/bin/env python3
"""Wave 3a: south-only idle/walk/attack animations for 4 armaturae via PixelLab."""

from __future__ import annotations

import io
import json
import re
import shutil
import time
import zipfile
from datetime import datetime, timezone
from pathlib import Path

from pixellab_gen import (
    ART,
    PROMPTS,
    api,
    api_bytes,
    palette_body,
    print_balance,
    save_image,
    token,  # noqa: F401 — load/validate token path exists
)

# Confirm token loads without printing it.
_ = token()

BACKUP_DIR = Path("/workspace/velarium/wave3a_idle_still_backup")
ENDPOINT = "/animate-character"
FRAME_COUNT = 4
POLL_TIMEOUT_S = 600  # ≥ 10 min
STYLE_PREFIX = (
    "NES SNES era pixel art, 16 color, chunky pixels, 1px outline, "
    "low top-down south facing. "
)

ARMATURAE = [
    {
        "slug": "murmillo",
        "character_id": "b531be43-c3c1-481d-b7d4-6c2f120e6609",
        "seed_base": 784,
        "attack": "one short blade swing attack, not a loop, readable windup and strike",
    },
    {
        "slug": "thraex",
        "character_id": "dc9e6a43-a406-45c6-b2e8-3838f3a77c4c",
        "seed_base": 792,
        "attack": "one short blade swing attack, not a loop, readable windup and strike",
    },
    {
        "slug": "retiarius",
        "character_id": "e227393a-96ad-4210-aeb6-afb42ff0ae96",
        "seed_base": 1793,
        "attack": "one trident thrust attack, not a loop, readable windup and strike",
    },
    {
        "slug": "secutor",
        "character_id": "4fb8a6da-eef8-4c8d-a31a-31ca17b3360e",
        "seed_base": 794,
        "attack": "one short blade swing attack, not a loop, readable windup and strike",
    },
]

CLIPS = {
    "idle": "subtle breathing idle, tiny bob, loopable, no foot travel",
    "walk": "four-frame walk cycle in place, south facing, clear leg motion",
    "attack": None,  # per-armatura
}

SEED_OFFSET = {"idle": 0, "walk": 10, "attack": 20}


def action_for(arm: dict, clip: str) -> str:
    body = CLIPS[clip] if clip != "attack" else arm["attack"]
    return STYLE_PREFIX + body


def backup_idle_stills() -> list[str]:
    BACKUP_DIR.mkdir(parents=True, exist_ok=True)
    copied: list[str] = []
    for arm in ARMATURAE:
        src = ART / "characters" / f"{arm['slug']}_s_idle_00.png"
        if src.is_file():
            dest = BACKUP_DIR / src.name
            shutil.copy2(src, dest)
            copied.append(src.name)
            print(f"backup {src.name} -> {dest}")
    return copied


def poll_job_soft(job_id: str, timeout_s: int = POLL_TIMEOUT_S) -> dict:
    deadline = time.time() + timeout_s
    last = ""
    while time.time() < deadline:
        job = api("GET", f"/background-jobs/{job_id}")
        status = (job.get("status") or "").lower()
        if status != last:
            print(f"  job {job_id[:8]}… {status}")
            last = status
        if status in {"completed", "complete", "success"}:
            return job
        if status in {"failed", "error"}:
            raise RuntimeError(f"job failed: {json.dumps(job.get('last_response'))[:400]}")
        time.sleep(5)
    raise TimeoutError(f"job timed out after {timeout_s}s: {job_id}")


def _frame_index(name: str) -> int | None:
    m = re.search(r"frame[_-]?(\d+)\.png$", name.lower())
    return int(m.group(1)) if m else None


def extract_south_frames(zf: zipfile.ZipFile, animation_name: str) -> list[tuple[int, bytes]]:
    names = [n.replace("\\", "/") for n in zf.namelist()]
    # Prefer animations/{name}/south/frame_*.png (case-insensitive).
    needle = f"animations/{animation_name}/south/"
    candidates = [
        n
        for n in names
        if needle.lower() in n.lower() and n.lower().endswith(".png")
    ]
    if not candidates:
        # Fallback: any path with animation_name + south + frame
        candidates = [
            n
            for n in names
            if animation_name.lower() in n.lower()
            and "south" in n.lower()
            and "frame" in n.lower()
            and n.lower().endswith(".png")
        ]
    frames: list[tuple[int, bytes]] = []
    for n in candidates:
        idx = _frame_index(n)
        if idx is None:
            continue
        frames.append((idx, zf.read(n)))
    frames.sort(key=lambda t: t[0])
    # Deduplicate by index (keep first)
    seen: set[int] = set()
    unique: list[tuple[int, bytes]] = []
    for idx, png in frames:
        if idx in seen:
            continue
        seen.add(idx)
        unique.append((idx, png))
    return unique


def pull_and_save(slug: str, clip: str, character_id: str) -> dict[str, str]:
    raw = api_bytes(f"/characters/{character_id}/zip")
    zf = zipfile.ZipFile(io.BytesIO(raw))
    names = [n.replace("\\", "/") for n in zf.namelist()]
    print(f"  zip members ({len(names)}): sample={names[:12]}")
    frames = extract_south_frames(zf, clip)
    if len(frames) < FRAME_COUNT:
        # List animation-ish paths for debug
        anim_paths = [n for n in names if "anim" in n.lower() or "frame" in n.lower()]
        raise RuntimeError(
            f"expected ≥{FRAME_COUNT} {clip}/south frames, got {len(frames)}; "
            f"anim_paths={anim_paths[:40]}"
        )
    # Take first FRAME_COUNT in sorted order; reindex 00..03 for Godot.
    files: dict[str, str] = {}
    for out_i, (_src_i, png) in enumerate(frames[:FRAME_COUNT]):
        rel = f"characters/{slug}_s_{clip}_{out_i:02}.png"
        save_image(rel, png)
        files[f"{clip}_{out_i:02}"] = rel
        print(f"  save {rel} ({len(png)} bytes)")
    return files


def submit_animate(arm: dict, clip: str) -> tuple[str, dict, bool]:
    action = action_for(arm, clip)
    seed = arm["seed_base"] + SEED_OFFSET[clip]
    body: dict = {
        "character_id": arm["character_id"],
        "mode": "v3",
        "directions": ["south"],
        "frame_count": FRAME_COUNT,
        "animation_name": clip,
        "action_description": action,
        "async_mode": True,
        "keep_first_frame": False,  # store exactly frame_count generated frames
        "seed": seed,
    }
    pal = palette_body()
    used_palette = False
    if pal:
        body["color_image"] = pal
        body["force_colors"] = True
        used_palette = True
    print(
        f"POST {ENDPOINT} {arm['slug']}/{clip} "
        f"char={arm['character_id'][:8]}… seed={seed} palette={used_palette}"
    )
    resp = api("POST", ENDPOINT, body)
    job_ids = resp.get("background_job_ids") or []
    if not job_ids:
        # Some responses may use singular
        jid = resp.get("background_job_id")
        if jid:
            job_ids = [jid]
    if not job_ids:
        raise RuntimeError(f"no job id in response keys={list(resp)}")
    return job_ids[0], {"request": {k: v for k, v in body.items() if k != "color_image"}, "response_status": resp.get("status"), "directions": resp.get("directions"), "action_description": action, "seed": seed}, used_palette


def write_prompt_json(
    slug: str,
    clip: str,
    character_id: str,
    job_id: str,
    action: str,
    seed: int,
    files: dict[str, str],
    used_palette: bool,
    job: dict | None,
    status: str,
    error: str | None = None,
) -> Path:
    PROMPTS.mkdir(parents=True, exist_ok=True)
    log = {
        "id": f"wave3a_{slug}_{clip}",
        "endpoint": ENDPOINT,
        "character_id": character_id,
        "animation_name": clip,
        "directions": ["south"],
        "frame_count": FRAME_COUNT,
        "mode": "v3",
        "keep_first_frame": False,
        "action_description": action,
        "seed": seed,
        "background_job_id": job_id,
        "color_image": used_palette,
        "force_colors": used_palette,
        "status": status,
        "files": files,
        "usage": (job or {}).get("usage"),
        "error": error,
        "finished_at_utc": datetime.now(timezone.utc).isoformat(),
    }
    path = PROMPTS / f"wave3a_{slug}_{clip}.json"
    path.write_text(json.dumps(log, indent=2) + "\n", encoding="utf-8")
    return path


def write_summary(results: list[dict], bal_before: tuple[float, float], bal_after: tuple[float, float], backups: list[str]) -> Path:
    PROMPTS.mkdir(parents=True, exist_ok=True)
    lines = [
        "# Wave 3a south animations log",
        "",
        f"## {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M:%S')} UTC",
        "",
        "- Scope: idle / walk / attack × south only × murmillo, thraex, retiarius, secutor (household skipped)",
        f"- Endpoint: `POST {ENDPOINT}` mode=v3 frame_count={FRAME_COUNT} keep_first_frame=false",
        "- Palette: NES strip attached via color_image + force_colors when supported (is supported)",
        f"- Idle still backup: `{BACKUP_DIR}` ({len(backups)} files: {', '.join(backups) or 'none'})",
        f"- Balance before: USD={bal_before[0]} gens={bal_before[1]}",
        f"- Balance after:  USD={bal_after[0]} gens={bal_after[1]}",
        "",
        "## Jobs",
        "",
        "| slug | clip | character_id | job_id | status | files |",
        "|---|---|---|---|---|---|",
    ]
    ok = 0
    for r in results:
        nfiles = len(r.get("files") or {})
        if r["status"] == "completed":
            ok += 1
        lines.append(
            f"| {r['slug']} | {r['clip']} | `{r['character_id']}` | `{r['job_id']}` | "
            f"{r['status']} | {nfiles} |"
        )
        if r.get("error"):
            lines.append(f"|  |  |  |  | error | {r['error'][:120]} |")
    lines.extend(
        [
            "",
            f"## Summary",
            "",
            f"- Completed: {ok}/{len(results)}",
            f"- Expected PNGs: 48; written: {sum(len(r.get('files') or {}) for r in results)}",
            "",
        ]
    )
    path = PROMPTS / "WAVE3A_SOUTH_LOG.md"
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return path


def main() -> None:
    print("=== Wave 3a south animations ===")
    bal0 = api("GET", "/balance")
    usd0, gens0 = print_balance(bal0)
    backups = backup_idle_stills()

    results: list[dict] = []
    for arm in ARMATURAE:
        for clip in ("idle", "walk", "attack"):
            row: dict = {
                "slug": arm["slug"],
                "clip": clip,
                "character_id": arm["character_id"],
                "job_id": "",
                "status": "pending",
                "files": {},
                "error": None,
            }
            try:
                job_id, meta, used_pal = submit_animate(arm, clip)
                row["job_id"] = job_id
                print(f"  queued job_id={job_id}")
                job = poll_job_soft(job_id)
                files = pull_and_save(arm["slug"], clip, arm["character_id"])
                row["files"] = files
                row["status"] = "completed"
                write_prompt_json(
                    arm["slug"],
                    clip,
                    arm["character_id"],
                    job_id,
                    meta["action_description"],
                    meta["seed"],
                    files,
                    used_pal,
                    job,
                    "completed",
                )
            except Exception as e:
                row["status"] = "failed"
                row["error"] = str(e)[:500]
                print(f"  FAIL {arm['slug']}/{clip}: {e}")
                write_prompt_json(
                    arm["slug"],
                    clip,
                    arm["character_id"],
                    row["job_id"] or "none",
                    action_for(arm, clip),
                    arm["seed_base"] + SEED_OFFSET[clip],
                    row["files"],
                    palette_body() is not None,
                    None,
                    "failed",
                    error=row["error"],
                )
            results.append(row)
            # Gentle pause between jobs to reduce 429 risk
            time.sleep(2)

    bal1 = api("GET", "/balance")
    usd1, gens1 = print_balance(bal1)
    summary = write_summary(results, (usd0, gens0), (usd1, gens1), backups)
    print(f"summary -> {summary}")
    completed = sum(1 for r in results if r["status"] == "completed")
    pngs = sum(len(r.get("files") or {}) for r in results)
    print(f"DONE completed={completed}/{len(results)} pngs={pngs}")
    if completed < len(results):
        raise SystemExit(1)


if __name__ == "__main__":
    main()
