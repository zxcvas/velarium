#!/usr/bin/env python3
"""Wave 3a south idle / walk / attack (4 frames) for the four armaturae.

Imports helpers from pixellab_gen. Token from pixellab.env or PIXELLAB_API_TOKEN.
Never prints the secret.

Usage:
  python tools/pixellab_wave3a.py --list
  python tools/pixellab_wave3a.py --pull
  python tools/pixellab_wave3a.py --animate
  python tools/pixellab_wave3a.py --check
"""

from __future__ import annotations

import argparse
import io
import json
import re
import sys
import zipfile
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
if str(TOOLS) not in sys.path:
    sys.path.insert(0, str(TOOLS))

from pixellab_gen import (  # noqa: E402
    ART,
    DIR_SHORT,
    PROMPTS,
    api,
    api_bytes,
    palette_body,
    poll_job,
    print_balance,
    save_image,
    token,
    write_log,
)

CLIPS = ("idle", "walk", "attack")
FRAME_COUNT = 4

ACTIONS = {
    "idle": (
        "breathing idle, tiny weight shift, feet planted, SNES 4-frame loop, "
        "facing south, keep kit silhouette, transparent background"
    ),
    "walk": (
        "walking in place south, SNES 4-frame loop, limbs and kit move, "
        "keep silhouette, transparent background"
    ),
    "attack": (
        "south-facing attack, 4 frames, commit the swing, not a loop, "
        "keep kit silhouette, transparent background"
    ),
}

FIGHTERS = [
    {
        "slug": "murmillo",
        "character_id": "b531be43-c3c1-481d-b7d4-6c2f120e6609",
        "wave2_id": "sample_murmillo_s",
        "seed_base": 1800,
    },
    {
        "slug": "thraex",
        "character_id": "dc9e6a43-a406-45c6-b2e8-3838f3a77c4c",
        "wave2_id": "char_thraex",
        "seed_base": 1810,
    },
    {
        "slug": "retiarius",
        "character_id": "e227393a-96ad-4210-aeb6-afb42ff0ae96",
        "wave2_id": "char_retiarius",
        "seed_base": 1820,
    },
    {
        "slug": "secutor",
        "character_id": "4fb8a6da-eef8-4c8d-a31a-31ca17b3360e",
        "wave2_id": "char_secutor",
        "seed_base": 1830,
    },
]

HOUSEHOLD = {
    "slug": "household",
    "character_id": "a317ac42-f430-40b0-a295-cf13d6401306",
    "wave2_id": "char_household",
}

# Jobs already billed on box (2026-09-12). --animate skips these unless --force.
SHIPPED_JOBS: dict[tuple[str, str], str] = {
    ("murmillo", "idle"): "39974873-7a73-4257-a701-fcf386642b00",
    ("murmillo", "walk"): "073601db-3b8d-4300-9a3b-06e5d0d869cd",
    ("murmillo", "attack"): "d0e936f1-e5f4-4f73-bbe9-7a48f1d31958",
    ("thraex", "idle"): "907c3a7e-a80e-451c-a3ee-44cf65fe96c5",
    ("thraex", "walk"): "c0859df0-5a6e-403e-ad96-4e4b1308af9a",
    ("thraex", "attack"): "cadc0eae-267e-4f47-ab67-89b8501f25bb",
    ("retiarius", "idle"): "ccca08ac-9649-46d2-8cda-b09258277d42",
    ("retiarius", "walk"): "0d160659-563a-410e-b011-2627a2f3b66c",
    ("retiarius", "attack"): "cefbd92d-5dc5-4aae-993e-be303cc115e2",
    ("secutor", "idle"): "0ceb4d86-f439-491a-8b9f-a8342b6695ee",
    ("secutor", "walk"): "22a6f499-0918-4dd7-aa91-8d011224975a",
    ("secutor", "attack"): "41e28d27-a063-4a75-be53-1b565374e85c",
}

ALWAYS_REPLACE_IDLES = {"retiarius", "household"}


def wave3a_jobs(only: list[str] | None = None) -> list[dict]:
    want = None if only is None else set(only)
    jobs: list[dict] = []
    for fighter in FIGHTERS:
        if want is not None and fighter["slug"] not in want:
            continue
        for i, clip in enumerate(CLIPS):
            jobs.append(
                {
                    "id": f"wave3a_{fighter['slug']}_{clip}",
                    "slug": fighter["slug"],
                    "clip": clip,
                    "character_id": fighter["character_id"],
                    "wave2_id": fighter["wave2_id"],
                    "seed": fighter["seed_base"] + i,
                    "w": 48,
                    "h": 48,
                    "view": "low top-down",
                    "direction": "south",
                    "no_bg": True,
                    "description": ACTIONS[clip],
                    "job_id": SHIPPED_JOBS[(fighter["slug"], clip)],
                }
            )
    return jobs


def _zip_names(zf: zipfile.ZipFile) -> list[str]:
    return [n.replace("\\", "/") for n in zf.namelist() if not n.endswith("/")]


def _find_rotation(names: list[str], direction: str) -> str | None:
    tail = f"rotations/{direction}.png"
    for n in names:
        low = n.lower()
        if low.endswith(tail) or low == f"{direction}.png":
            return n
    return None


def _find_anim_frame(names: list[str], clip: str, direction: str, index: int) -> str | None:
    pats = (
        re.compile(
            rf"(?:^|/)animations/{re.escape(clip)}/{re.escape(direction)}/frame_{index:03}\.png$",
            re.I,
        ),
        re.compile(
            rf"(?:^|/){re.escape(clip)}/{re.escape(direction)}/frame_{index:03}\.png$",
            re.I,
        ),
        re.compile(
            rf"(?:^|/)animations/{re.escape(clip)}/{re.escape(direction)}/{index:02}\.png$",
            re.I,
        ),
    )
    for n in names:
        if any(p.search(n) for p in pats):
            return n
    return None


def unpack_rotations(zf: zipfile.ZipFile, slug: str, replace: bool) -> dict[str, str]:
    names = _zip_names(zf)
    files: dict[str, str] = {}
    for direction, short in DIR_SHORT.items():
        rel = f"characters/{slug}_{short}_idle_00.png"
        dest = ART / rel
        if dest.is_file() and not replace:
            files[direction] = rel
            continue
        member = _find_rotation(names, direction)
        if not member:
            continue
        save_image(rel, zf.read(member))
        files[direction] = rel
        print(f"save {rel}")
    return files


def unpack_south_anims(zf: zipfile.ZipFile, slug: str, clips: tuple[str, ...] = CLIPS) -> dict[str, list[str]]:
    names = _zip_names(zf)
    out: dict[str, list[str]] = {}
    for clip in clips:
        frames: list[str] = []
        for i in range(FRAME_COUNT):
            member = _find_anim_frame(names, clip, "south", i)
            if not member:
                print(f"miss {slug} {clip} south frame_{i:03} (zip has {len(names)} files)")
                continue
            rel = f"characters/{slug}_s_{clip}_{i:02}.png"
            save_image(rel, zf.read(member))
            frames.append(rel)
            print(f"save {rel}")
        if frames:
            out[clip] = frames
    return out


def write_wave3a_stub(job: dict, extra: dict | None = None) -> None:
    item = {
        "id": job["id"],
        "seed": job["seed"],
        "w": job["w"],
        "h": job["h"],
        "view": job["view"],
        "direction": job["direction"],
        "no_bg": True,
        "description": job["description"],
    }
    payload = {
        "slug": job["slug"],
        "clip": job["clip"],
        "character_id": job["character_id"],
        "background_job_id": job.get("job_id"),
        "mode": "v3",
        "frame_count": FRAME_COUNT,
        "keep_first_frame": False,
        "force_colors": True,
        "directions": ["south"],
        "files": [
            f"characters/{job['slug']}_s_{job['clip']}_{i:02}.png" for i in range(FRAME_COUNT)
        ],
    }
    if extra:
        payload.update(extra)
    write_log(item, None, palette_body() is not None, endpoint="/animate-character", extra=payload)


def pull_character(slug: str, character_id: str, *, idles: bool, replace_idles: bool, anims: bool) -> None:
    print(f"zip  {slug} {character_id}")
    raw = api_bytes(f"/characters/{character_id}/zip")
    zf = zipfile.ZipFile(io.BytesIO(raw))
    names = _zip_names(zf)
    print("     members", len(names))
    if idles:
        unpack_rotations(zf, slug, replace=replace_idles)
    if anims:
        unpack_south_anims(zf, slug)


def animate_job(job: dict, force: bool) -> None:
    dest0 = ART / f"characters/{job['slug']}_s_{job['clip']}_00.png"
    if dest0.is_file() and not force:
        print(f"skip {job['id']} (exists)")
        return
    pal = palette_body()
    body: dict = {
        "character_id": job["character_id"],
        "animation_name": job["clip"],
        "action_description": job["description"],
        "mode": "v3",
        "frame_count": FRAME_COUNT,
        "keep_first_frame": False,
        "directions": ["south"],
        "isometric": False,
        "force_colors": bool(pal),
        "seed": job["seed"],
        "enhance_prompt": False,
    }
    if pal:
        body["color_image"] = pal
    print(f"anim {job['id']} seed={job['seed']} char={job['character_id']}")
    resp = api("POST", "/animate-character", body)
    job_ids = resp.get("background_job_ids") or []
    if not job_ids:
        raise SystemExit(f"animate-character returned no jobs for {job['id']}: {list(resp)}")
    job_id = job_ids[0]
    print(f"     job={job_id}")
    poll_job(job_id, timeout_s=300)
    pull_character(job["slug"], job["character_id"], idles=False, replace_idles=False, anims=True)
    write_wave3a_stub(job, extra={"background_job_id": job_id, "usage": resp.get("usage")})


def main() -> None:
    ap = argparse.ArgumentParser(description="Wave 3a south idle/walk/attack for four armaturae")
    ap.add_argument("--check", action="store_true", help="GET /balance and exit")
    ap.add_argument("--list", action="store_true", help="Print the 12 shipped jobs (no token)")
    ap.add_argument("--pull", action="store_true", help="Download character zips; unpack south frames + approved idles")
    ap.add_argument("--animate", action="store_true", help="POST /animate-character (bills generations)")
    ap.add_argument("--force", action="store_true")
    ap.add_argument("--only", nargs="*", help="Limit to slug(s): murmillo thraex retiarius secutor household")
    args = ap.parse_args()

    if args.only:
        known = {f["slug"] for f in FIGHTERS} | {HOUSEHOLD["slug"]}
        missing = set(args.only) - known
        if missing:
            raise SystemExit(f"unknown slug(s): {sorted(missing)}")
    if args.only is None:
        jobs = wave3a_jobs()
    else:
        jobs = wave3a_jobs([s for s in args.only if s != HOUSEHOLD["slug"]])

    if args.list or not (args.check or args.pull or args.animate):
        print("Wave 3a south — 12 jobs (idle/walk/attack × 4 armaturae), household idles only")
        for job in wave3a_jobs():
            print(f"  {job['slug']:10} {job['clip']:6} job={job['job_id']} char={job['character_id']}")
        print(f"  {HOUSEHOLD['slug']:10} idle   (no wave3a anims) char={HOUSEHOLD['character_id']}")
        if not (args.check or args.pull or args.animate):
            return

    # Token only when talking to the API.
    token()
    bal = api("GET", "/balance")
    print_balance(bal)
    if args.check:
        return

    if args.pull:
        ART.mkdir(parents=True, exist_ok=True)
        want = set(args.only) if args.only else None
        for fighter in FIGHTERS:
            if want and fighter["slug"] not in want:
                continue
            pull_character(
                fighter["slug"],
                fighter["character_id"],
                idles=True,
                replace_idles=fighter["slug"] in ALWAYS_REPLACE_IDLES,
                anims=True,
            )
        if want is None or HOUSEHOLD["slug"] in want:
            pull_character(
                HOUSEHOLD["slug"],
                HOUSEHOLD["character_id"],
                idles=True,
                replace_idles=True,
                anims=False,
            )
        for job in jobs:
            write_wave3a_stub(job)
        print("done pull")
        return

    if args.animate:
        ART.mkdir(parents=True, exist_ok=True)
        for job in jobs:
            animate_job(job, args.force)
        print("done animate")
        return


if __name__ == "__main__":
    main()
