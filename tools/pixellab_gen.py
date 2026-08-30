#!/usr/bin/env python3
"""Offline PixelLab pipeline. Token from pixellab.env or PIXELLAB_API_TOKEN. Never prints the secret."""

from __future__ import annotations

import argparse
import base64
import json
import os
import time
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "assets" / "art"
PROMPTS = ART / "prompts"
API = "https://api.pixellab.ai/v2"

STYLE = (
    "16-bit pixel art, Pompeii fresco palette: ochre, soot, iron rust, dirty linen, "
    "charcoal, faded sea-green. Single black outline, basic shading. Gritty Capuan ludus. "
    "No muscle cuirass, no comic abs, no lorica, no neon, no modern gym body."
)

# Locked seeds (a.u.c. 782 + slot).
PACK = [
    {
        "id": "style_lock",
        "path": "style_lock.png",
        "kind": "palette",
        "seed": 782,
        "w": 128,
        "h": 64,
        "view": "high top-down",
        "no_bg": False,
        "description": (
            STYLE
            + " A horizontal strip of Roman pigments on plaster: ochre yellow, soot black, "
            "iron brown, dirty linen white, fresco red, charcoal grey. Flat swatches, no figures."
        ),
    },
    {
        "id": "courtyard_dirt",
        "path": "tiles/courtyard_dirt.png",
        "kind": "tile",
        "seed": 783,
        "w": 64,
        "h": 64,
        "view": "high top-down",
        "no_bg": False,
        "description": (
            STYLE
            + " Seamless top-down courtyard dirt tile. Packed Capuan earth, pale dust, tiny stones, "
            "even density. Pattern continues off every edge. No landmark rock, no footprints, "
            "no directional shadow."
        ),
    },
    {
        "id": "cell_interior",
        "path": "tiles/cell_interior.png",
        "kind": "tile",
        "seed": 784,
        "w": 64,
        "h": 64,
        "view": "low top-down",
        "no_bg": False,
        "description": (
            STYLE
            + " Low top-down gladiator cell floor: straw on packed earth, cracked plaster edge, "
            "no bed frame. Pattern continues off the earth edges. Empty cell, no people."
        ),
    },
    {
        "id": "kitchen_hearth",
        "path": "props/kitchen_hearth.png",
        "kind": "prop",
        "seed": 785,
        "w": 64,
        "h": 64,
        "view": "low top-down",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated kitchen hearth of a Capuan ludus: brick and soot, bronze cauldron, "
            "barley sack, no people. Low top-down, transparent background."
        ),
    },
    {
        "id": "porta",
        "path": "props/porta.png",
        "kind": "prop",
        "seed": 786,
        "w": 64,
        "h": 80,
        "view": "low top-down",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated single guarded porta of a small ludus: heavy wooden gate, iron bands, "
            "stone jambs. Low top-down, transparent background, no people."
        ),
    },
    {
        "id": "palus",
        "path": "props/palus.png",
        "kind": "prop",
        "seed": 787,
        "w": 48,
        "h": 64,
        "view": "low top-down",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated wooden palus training stake in a dirt yard, scarred post, no people. "
            "Low top-down, transparent background."
        ),
    },
    {
        "id": "murmillo_idle",
        "path": "characters/murmillo_idle.png",
        "kind": "character",
        "seed": 788,
        "w": 48,
        "h": 64,
        "view": "side",
        "direction": "south",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated murmillo gladiator idle, south-facing side view. Fish-crest bronze helm, "
            "large scutum, short gladius, manica on the sword arm, greave. Stocky, not a bodybuilder. "
            "Transparent background."
        ),
    },
    {
        "id": "thraex_idle",
        "path": "characters/thraex_idle.png",
        "kind": "character",
        "seed": 789,
        "w": 48,
        "h": 64,
        "view": "side",
        "direction": "south",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated thraex gladiator idle, south-facing. Griffin-crest helm, small parmula, "
            "curved sica, long greaves. Lighter than a murmillo. Transparent background."
        ),
    },
    {
        "id": "retiarius_idle",
        "path": "characters/retiarius_idle.png",
        "kind": "character",
        "seed": 790,
        "w": 48,
        "h": 64,
        "view": "side",
        "direction": "south",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated retiarius idle, south-facing. Bare head, galerus on the left shoulder, "
            "net and fuscina trident, tunic, no helm. Wiry, not a bodybuilder. Transparent background."
        ),
    },
    {
        "id": "secutor_idle",
        "path": "characters/secutor_idle.png",
        "kind": "character",
        "seed": 791,
        "w": 48,
        "h": 64,
        "view": "side",
        "direction": "south",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated secutor idle, south-facing. Smooth round helm with two tiny eye-holes, "
            "scutum, short gladius, made to hunt the net-man. Transparent background."
        ),
    },
    {
        "id": "household_idle",
        "path": "characters/household_idle.png",
        "kind": "character",
        "seed": 792,
        "w": 48,
        "h": 64,
        "view": "side",
        "direction": "south",
        "no_bg": True,
        "description": (
            STYLE
            + " Isolated household slave of a ludus, south-facing. Undyed dirty linen tunic, "
            "bare feet, no helm, no weapon. Thin kitchen hand. Transparent background."
        ),
    },
    {
        "id": "night_rival_gate",
        "path": "scenes/night_rival_gate.png",
        "kind": "scene",
        "seed": 793,
        "w": 128,
        "h": 96,
        "view": "side",
        "no_bg": False,
        "description": (
            STYLE
            + " Night scene: two small figures at a rival ludus porta. Torch, barred wooden gate, "
            " Capuan street. Spy beat, not a fight. Side view, no sexual content, no children."
        ),
    },
]


def token() -> str:
    env = os.environ.get("PIXELLAB_API_TOKEN", "").strip()
    if env:
        return env
    path = ROOT / "pixellab.env"
    if not path.is_file():
        raise SystemExit("No PIXELLAB_API_TOKEN and no pixellab.env")
    for raw in path.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        if line.lower().startswith("authorization:"):
            return line.split("Bearer", 1)[-1].strip()
        if "=" in line:
            k, v = line.split("=", 1)
            if k.strip() in {"PIXELLAB_API_TOKEN", "PIXELLAB_SECRET"}:
                return v.strip().strip('"')
    raise SystemExit("pixellab.env has no Bearer token")


def api(method: str, path: str, body: dict | None = None) -> dict:
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(
        API + path,
        data=data,
        method=method,
        headers={
            "Authorization": "Bearer " + token(),
            "Content-Type": "application/json",
            "Accept": "application/json",
        },
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as r:
            return json.loads(r.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise SystemExit(f"PixelLab HTTP {e.code} on {path}: {err[:500]}") from e


def b64_to_png(blob: str) -> bytes:
    if "base64," in blob:
        blob = blob.split("base64,", 1)[1]
    return base64.b64decode(blob)


def save_image(rel: str, png: bytes) -> Path:
    out = ART / rel
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_bytes(png)
    return out


def write_log(item: dict, usage: dict | None, used_palette: bool) -> None:
    PROMPTS.mkdir(parents=True, exist_ok=True)
    log = {
        "id": item["id"],
        "endpoint": "/create-image-pixflux",
        "seed": item["seed"],
        "image_size": {"width": item["w"], "height": item["h"]},
        "view": item.get("view"),
        "direction": item.get("direction"),
        "no_background": item["no_bg"],
        "outline": "single color black outline",
        "shading": "basic shading",
        "detail": "medium detail",
        "color_image": used_palette,
        "description": item["description"],
        "usage": usage,
    }
    (PROMPTS / f"{item['id']}.json").write_text(json.dumps(log, indent=2) + "\n", encoding="utf-8")


def palette_body() -> dict | None:
    p = ART / "style_lock.png"
    if not p.is_file():
        return None
    b64 = base64.b64encode(p.read_bytes()).decode("ascii")
    return {"type": "base64", "base64": b64, "format": "png"}


def generate(item: dict, force: bool) -> None:
    dest = ART / item["path"]
    if dest.is_file() and not force:
        print(f"skip {item['id']} (exists)")
        return
    body: dict = {
        "description": item["description"],
        "image_size": {"width": item["w"], "height": item["h"]},
        "text_guidance_scale": 10,
        "outline": "single color black outline",
        "shading": "basic shading",
        "detail": "medium detail",
        "view": item.get("view"),
        "no_background": item["no_bg"],
        "seed": item["seed"],
    }
    if item.get("direction"):
        body["direction"] = item["direction"]
    pal = None if item["id"] == "style_lock" else palette_body()
    if pal:
        body["color_image"] = pal
    print(f"gen  {item['id']} {item['w']}x{item['h']} seed={item['seed']}")
    resp = api("POST", "/create-image-pixflux", body)
    img = resp.get("image") or {}
    png = b64_to_png(img.get("base64") or "")
    save_image(item["path"], png)
    write_log(item, resp.get("usage"), pal is not None)
    time.sleep(0.4)


def write_manifest(items: list[dict]) -> None:
    lines = [
        "# First PixelLab pack",
        "",
        "Offline. Game never calls this API.",
        "",
        "| id | file | seed | size | kind |",
        "|---|---|---|---|---|",
    ]
    for it in items:
        lines.append(
            f"| `{it['id']}` | `{it['path']}` | {it['seed']} | {it['w']}×{it['h']} | {it['kind']} |"
        )
    lines.append("")
    (ART / "MANIFEST.md").write_text("\n".join(lines), encoding="utf-8")


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--force", action="store_true")
    ap.add_argument("--only", nargs="*")
    args = ap.parse_args()
    bal = api("GET", "/balance")
    credits = (bal.get("credits") or {}).get("usd")
    print(f"PixelLab credits USD: {credits}")
    items = PACK
    if args.only:
        want = set(args.only)
        items = [i for i in PACK if i["id"] in want]
    ART.mkdir(parents=True, exist_ok=True)
    for it in items:
        generate(it, args.force)
    write_manifest(PACK)
    print("done")


if __name__ == "__main__":
    main()
