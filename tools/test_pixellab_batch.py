#!/usr/bin/env python3
"""Claim, gate, and in-flight tests for the PixelLab batch dispatcher. No network."""

from __future__ import annotations

import json
import subprocess
import sys
import tempfile
import threading
import unittest
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
ROOT = TOOLS.parent
sys.path.insert(0, str(TOOLS))

import pixellab_batch as batch  # noqa: E402


def job(jid: str, **extra) -> dict:
    body = {
        "id": jid,
        "path": "/animate-character",
        "body": {"character_id": "abc", "animation_name": "hit", "frame_count": 4},
    }
    body.update(extra)
    return body


def manifest(jobs: list[dict], **extra) -> dict:
    data = {"id": "batch-test", "workers": 2, "jobs": jobs}
    data.update(extra)
    return batch.load_manifest_data(data)


class LoadTests(unittest.TestCase):
    def test_rejects_duplicate_secret_and_bad_path(self) -> None:
        with self.assertRaises(SystemExit):
            batch.load_manifest_data(
                {"id": "a", "jobs": [job("one"), job("one")]}
            )
        with self.assertRaises(SystemExit):
            batch.load_manifest_data(
                {"id": "a", "jobs": [job("one", body={"token": "nope"})]}
            )
        with self.assertRaises(SystemExit):
            batch.load_manifest_data(
                {"id": "a", "jobs": [job("one", path="animate-character")]}
            )

    def test_caps_workers(self) -> None:
        loaded = batch.load_manifest_data(
            {"id": "a", "workers": 12, "jobs": [job("one")]}
        )
        self.assertEqual(loaded["workers"], batch.MAX_WORKERS)

    def test_pull_requires_slug(self) -> None:
        with self.assertRaises(SystemExit):
            batch.load_manifest_data(
                {"id": "a", "jobs": [job("one", pull={"clip": "hit"})]}
            )


class GateTests(unittest.TestCase):
    def test_held_gate_refuses_run(self) -> None:
        loaded = batch.load_manifest_data(
            {"id": "wave3b-south", "gate": "wave-3b", "jobs": [job("murmillo-hit")]}
        )
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaises(SystemExit) as raised:
                batch.run_batch(loaded, claim_root=Path(tmp), pause=lambda _s: None)
        self.assertIn("Wave 3b", str(raised.exception))
        self.assertEqual(list(Path(tmp).rglob("*.json")), [])

    def test_hold_lifted_allows_submit(self) -> None:
        loaded = batch.load_manifest_data(
            {"id": "wave3b-south", "gate": "wave-3b", "workers": 1, "jobs": [job("murmillo-hit")]}
        )
        seen: list[str] = []

        def submit(item: dict) -> dict:
            seen.append(item["id"])
            return {"background_job_ids": ["job-1"], "status": "completed"}

        with tempfile.TemporaryDirectory() as tmp:
            rows = batch.run_batch(
                loaded,
                claim_root=Path(tmp),
                hold_lifted=True,
                submit=submit,
                poll=lambda _job: {"status": "completed"},
                pull=lambda _pull: {},
                pause=lambda _s: None,
            )
        self.assertEqual(seen, ["murmillo-hit"])
        self.assertEqual(rows[0]["status"], "completed")


class ClaimTests(unittest.TestCase):
    def test_second_runner_does_not_submit(self) -> None:
        loaded = batch.load_manifest_data(
            {"id": "batch-test", "workers": 2, "jobs": [job("a"), job("b")]}
        )
        calls: list[str] = []

        def submit(item: dict) -> dict:
            calls.append(item["id"])
            return {"status": "completed"}

        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            first = batch.run_batch(
                loaded,
                claim_root=root,
                submit=submit,
                poll=lambda _job: {"status": "completed"},
                pause=lambda _s: None,
            )
            second = batch.run_batch(
                loaded,
                claim_root=root,
                submit=submit,
                poll=lambda _job: {"status": "completed"},
                pause=lambda _s: None,
            )
        self.assertEqual(sorted(calls), ["a", "b"])
        self.assertTrue(all(row["status"] == "completed" for row in first))
        self.assertTrue(all(row["status"] == "skipped-completed" for row in second))

    def test_exclusive_create(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            winners: list[dict | None] = []
            lock = threading.Lock()

            def race() -> None:
                won = batch.create_claim(root, "batch-test", "same")
                with lock:
                    winners.append(won)

            threads = [threading.Thread(target=race) for _ in range(8)]
            for thread in threads:
                thread.start()
            for thread in threads:
                thread.join()
        self.assertEqual(sum(1 for item in winners if item), 1)

    def test_resume_polls_and_does_not_post_again(self) -> None:
        loaded = batch.load_manifest_data(
            {"id": "batch-test", "workers": 1, "jobs": [job("a")]}
        )

        def submit(_item: dict) -> dict:
            raise AssertionError("resume must not POST")

        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            batch.write_claim(
                root,
                "batch-test",
                "a",
                {"id": "a", "status": "started", "job_id": "remote-9", "error": None},
            )
            rows = batch.run_batch(
                loaded,
                claim_root=root,
                resume=True,
                submit=submit,
                poll=lambda job_id: {"status": "completed", "id": job_id},
                pause=lambda _s: None,
            )
        self.assertEqual(rows[0]["status"], "completed")
        self.assertEqual(rows[0]["job_id"], "remote-9")

    def test_resume_without_job_id_does_not_post(self) -> None:
        loaded = batch.load_manifest_data(
            {"id": "batch-test", "workers": 1, "jobs": [job("a")]}
        )

        def submit(_item: dict) -> dict:
            raise AssertionError("blocked claim must not POST")

        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            batch.write_claim(
                root,
                "batch-test",
                "a",
                {"id": "a", "status": "started", "job_id": "", "error": "lost response"},
            )
            rows = batch.run_batch(
                loaded,
                claim_root=root,
                resume=True,
                submit=submit,
                pause=lambda _s: None,
            )
        self.assertEqual(rows[0]["status"], "blocked")

    def test_retryable_submit_then_success(self) -> None:
        loaded = batch.load_manifest_data(
            {"id": "batch-test", "workers": 1, "jobs": [job("a")]}
        )
        attempts = {"n": 0}

        def submit(_item: dict) -> dict:
            attempts["n"] += 1
            if attempts["n"] < 3:
                raise SystemExit("PixelLab HTTP 429 on /animate-character")
            return {"status": "completed"}

        with tempfile.TemporaryDirectory() as tmp:
            rows = batch.run_batch(
                loaded,
                claim_root=Path(tmp),
                submit=submit,
                pause=lambda _s: None,
            )
        self.assertEqual(attempts["n"], 3)
        self.assertEqual(rows[0]["status"], "completed")

    def test_inflight_stays_within_workers(self) -> None:
        jobs = [job(f"j{i}") for i in range(6)]
        loaded = batch.load_manifest_data({"id": "batch-test", "workers": 2, "jobs": jobs})
        current = {"n": 0, "max": 0}
        lock = threading.Lock()

        def submit(item: dict) -> dict:
            with lock:
                current["n"] += 1
                current["max"] = max(current["max"], current["n"])
            try:
                time_hold()
                return {"status": "completed", "id": item["id"]}
            finally:
                with lock:
                    current["n"] -= 1

        def time_hold() -> None:
            import time

            time.sleep(0.05)

        with tempfile.TemporaryDirectory() as tmp:
            batch.run_batch(
                loaded,
                claim_root=Path(tmp),
                submit=submit,
                pause=lambda _s: None,
            )
        self.assertLessEqual(current["max"], 2)
        self.assertGreaterEqual(loaded["_peak_inflight"], 1)
        self.assertLessEqual(loaded["_peak_inflight"], 2)


class CliTests(unittest.TestCase):
    def test_dry_run_and_held_run(self) -> None:
        manifest_path = ROOT / "assets" / "art" / "batches" / "wave3b-south.json"
        dry = subprocess.run(
            [sys.executable, str(TOOLS / "pixellab_batch.py"), "--manifest", str(manifest_path)],
            cwd=ROOT,
            capture_output=True,
            text=True,
            check=False,
        )
        self.assertEqual(dry.returncode, 0, dry.stderr)
        self.assertIn("jobs=12", dry.stdout)
        self.assertIn("dry-run: no claim, no POST", dry.stdout)
        self.assertNotIn("Bearer", dry.stdout)

        held = subprocess.run(
            [
                sys.executable,
                str(TOOLS / "pixellab_batch.py"),
                "--manifest",
                str(manifest_path),
                "--run",
                "--claim-root",
                str(ROOT / "assets" / "art" / "prompts" / "batch-claims-test"),
            ],
            cwd=ROOT,
            capture_output=True,
            text=True,
            check=False,
        )
        self.assertNotEqual(held.returncode, 0)
        self.assertIn("Wave 3b", held.stderr + held.stdout)
        self.assertFalse((ROOT / "assets" / "art" / "prompts" / "batch-claims-test").exists())


if __name__ == "__main__":
    unittest.main()
