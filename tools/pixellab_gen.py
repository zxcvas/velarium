#!/usr/bin/env python3
"""Offline PixelLab pipeline. Token from pixellab.env or PIXELLAB_API_TOKEN. Never prints the secret."""

from __future__ import annotations

import argparse
import base64
import io
import json
import os
import time
import urllib.error
import urllib.request
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "assets" / "art"
PROMPTS = ART / "prompts"
PALETTE = ART / "tiles" / "palette_nes.png"
API = "https://api.pixellab.ai/v2"

# STYLE.md / ASSET_MAP.md — NES/SNES lock. Seed family = a.u.c. 782 + slot.
PREFIX = (
    "NES SNES era pixel art, 16 color, chunky pixels, 1px outline. "
    "Ochre, soot, iron, dirty linen, Pompeii red, charcoal, one sea-green. "
)
NEG = (
    "not painterly, not 3D, not isometric diorama, not photoreal, "
    "not muscle cuirass, not gold leaf, not neon."
)

# Master strip. Hex locked in STYLE.md. Dirt samples + STYLE pigments; not pixflux.
NES_SWATCHES: list[tuple[str, tuple[int, int, int]]] = [
    ("soot", (0x22, 0x1F, 0x22)),
    ("charcoal_deep", (0x37, 0x2E, 0x31)),
    ("charcoal", (0x44, 0x45, 0x48)),
    ("iron", (0x50, 0x54, 0x55)),
    ("packed_dirt", (0x68, 0x43, 0x35)),
    ("iron_rust", (0x87, 0x6C, 0x57)),
    ("dirty_linen", (0xB7, 0xA1, 0x81)),
    ("pale_linen", (0xE8, 0xDC, 0xC0)),
    ("ochre", (0xBB, 0x87, 0x15)),
    ("pompeii_red", (0xA4, 0x4A, 0x3E)),
    ("dark_pompeii", (0x5B, 0x2D, 0x2D)),
    ("sea_green", (0x4A, 0x8B, 0x7A)),
    ("cold_iron", (0x7F, 0x99, 0xB0)),
    ("bronze", (0xC4, 0x96, 0x4A)),
    ("skin", (0xC8, 0xA8, 0x7A)),
    ("outline", (0x0A, 0x08, 0x08)),
]

WAVE0 = [
    {
        "id": "palette_nes",
        "path": "tiles/palette_nes.png",
        "kind": "palette",
        "seed": 782,
        "w": 64,
        "h": 16,
        "view": None,
        "no_bg": False,
        "description": "Authored 16-swatch NES strip. Not pixflux.",
    },
    {
        "id": "sample_dirt",
        "path": "tiles/sample_dirt.png",
        "kind": "tile",
        "seed": 783,
        "w": 32,
        "h": 32,
        "view": "high top-down",
        "no_bg": False,
        "description": (
            PREFIX
            + "Seamless high top-down packed Capuan courtyard dirt tile. Pale dust, "
            "tiny stones, even density. Pattern continues off every edge. No landmark "
            "rock, no footprints, no directional shadow, no grass. "
            + NEG
        ),
    },
    {
        "id": "sample_murmillo_s",
        "path": "characters/sample_murmillo_s.png",
        "kind": "character4",
        "seed": 784,
        "w": 48,
        "h": 48,
        "view": "low top-down",
        "direction": "south",
        "no_bg": True,
        "description": (
            PREFIX
            + "Murmillo gladiator idle, SNES 3/4 low top-down like A Link to the Past. "
            "Bronze galea with a metal fish-shaped crest (dorsal fish-fin ridge along the helm, "
            "two eye slits). NOT a red horsehair mohawk, NOT a centurion plume. "
            "Big scutum, short gladius, manica on the sword arm, one greave. "
            "Stocky toy soldier, chunky 48px sprite, transparent background. "
            + NEG
        ),
    },
]

# Wave 1: Wang tileset (dirt ↔ plaster) then courtyard props.
WAVE1 = [
    {
        "id": "tileset_ludus",
        "path": "tiles/ludus/lower.png",
        "kind": "tileset",
        "seed": 785,
        "w": 32,
        "h": 32,
        "view": "high top-down",
        "no_bg": False,
        "lower": PREFIX
        + "Seamless packed-earth COURTYARD GROUND tile, dusty brown dirt, tiny pebbles, even lighting. "
        "Not brick, not cobblestone, not a wall, not a building. Pattern continues off every edge. "
        + NEG,
        "upper": PREFIX
        + "Seamless cracked-plaster FLOOR tile, pale dirty linen, hairline cracks, even lighting. "
        "A floor you walk on, not a wall, not a building, no columns. Pattern continues off every edge. "
        + NEG,
        "transition": PREFIX
        + "Packed dirt ground meeting cracked plaster floor, small stones, no walls, no grass. "
        + NEG,
        "description": "Wang tileset: courtyard dirt (lower) to cracked plaster portico (upper).",
    },
    {
        "id": "prop_palus",
        "path": "props/prop_palus.png",
        "kind": "prop",
        "seed": 786,
        "w": 32,
        "h": 32,
        "view": "low top-down",
        "no_bg": True,
        "description": PREFIX
        + "Isolated wooden palus training stake, scarred post, SNES low top-down. "
        "No ground shadow, no dirt tile, no people. Transparent background. "
        + NEG,
    },
    {
        "id": "prop_hearth",
        "path": "props/prop_hearth.png",
        "kind": "prop",
        "seed": 787,
        "w": 32,
        "h": 32,
        "view": "low top-down",
        "no_bg": True,
        "description": PREFIX
        + "Isolated sooty brick kitchen hearth with a bronze pot. No text on sacks. "
        "No ground shadow, no people. Transparent background. "
        + NEG,
    },
    {
        "id": "prop_porta",
        "path": "props/prop_porta.png",
        "kind": "prop",
        "seed": 788,
        "w": 32,
        "h": 32,
        "view": "low top-down",
        "no_bg": True,
        "description": PREFIX
        + "Isolated single ludus gate, iron bands, stone jambs, closed. "
        "No ground shadow, no people. Transparent background. "
        + NEG,
    },
    {
        "id": "prop_porta_night",
        "path": "props/prop_porta_night.png",
        "kind": "prop",
        "init": "props/prop_porta.png",
        "init_strength": 420,
        "seed": 789,
        "w": 32,
        "h": 32,
        "view": "low top-down",
        "no_bg": True,
        "description": PREFIX
        + "The same isolated ludus gate at night, one torch on the jamb, darker soot palette. "
        "Keep the gate shape. No people. Transparent background. "
        + NEG,
    },
    {
        "id": "prop_cellae",
        "path": "props/prop_cellae.png",
        "kind": "prop",
        "seed": 790,
        "w": 32,
        "h": 32,
        "view": "low top-down",
        "no_bg": True,
        "description": PREFIX
        + "Isolated straw and cracked plaster gladiator cell interior, empty, no bed frame. "
        "No ground shadow, no people. Transparent background. "
        + NEG,
    },
    {
        "id": "prop_medicus",
        "path": "props/prop_medicus.png",
        "kind": "prop",
        "seed": 791,
        "w": 32,
        "h": 32,
        "view": "low top-down",
        "no_bg": True,
        "description": PREFIX
        + "Isolated medicus stall: linen bag, bowl, roll of cloth. "
        "No ground shadow, no people, no text. Transparent background. "
        + NEG,
    },
]

WAVES: dict[int, list[dict]] = {0: WAVE0, 1: WAVE1}


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
            if k.strip() in {"PIXELLAB_API_TOKEN", "PIXELLAB_SECRET", "PIXELLAB_TOKEN"}:
                return v.strip().strip('"').strip("'")
            continue
        return line
    raise SystemExit("pixellab.env has no token")


def _redact(text: str) -> str:
    tok = token()
    return text.replace(tok, "[token]") if tok else text


def api(method: str, path: str, body: dict | None = None, pending_ok: bool = False) -> dict:
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
            raw = r.read().decode("utf-8")
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        if pending_ok and e.code == 423:
            e.read()
            return {"_pending": True}
        err = _redact(e.read().decode("utf-8", errors="replace"))
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


def write_log(
    item: dict,
    usage: dict | None,
    used_palette: bool,
    endpoint: str | None = None,
    extra: dict | None = None,
) -> None:
    PROMPTS.mkdir(parents=True, exist_ok=True)
    log = {
        "id": item["id"],
        "endpoint": endpoint or "/create-image-pixflux",
        "seed": item["seed"],
        "image_size": {"width": item["w"], "height": item["h"]},
        "view": item.get("view"),
        "direction": item.get("direction"),
        "no_background": item["no_bg"],
        "outline": "single color black outline",
        "shading": "basic shading",
        "detail": "low detail",
        "color_image": used_palette,
        "description": item["description"],
        "usage": usage,
    }
    if item.get("kind") == "palette":
        log["swatches"] = [
            {"name": name, "hex": f"#{r:02X}{g:02X}{b:02X}"}
            for name, (r, g, b) in NES_SWATCHES
        ]
    if extra:
        log.update(extra)
    (PROMPTS / f"{item['id']}.json").write_text(json.dumps(log, indent=2) + "\n", encoding="utf-8")


def write_authored_palette(dest: Path) -> None:
    from PIL import Image

    width, height = 64, 16
    bar = width // len(NES_SWATCHES)
    im = Image.new("RGB", (width, height))
    px = im.load()
    assert px is not None
    for i, (_name, rgb) in enumerate(NES_SWATCHES):
        x0 = i * bar
        for x in range(x0, x0 + bar):
            for y in range(height):
                px[x, y] = rgb
    dest.parent.mkdir(parents=True, exist_ok=True)
    im.save(dest)


def _b64_png(path: Path) -> dict:
    return {"type": "base64", "base64": base64.b64encode(path.read_bytes()).decode("ascii"), "format": "png"}


def palette_body() -> dict | None:
    if not PALETTE.is_file():
        return None
    return _b64_png(PALETTE)


def write_murmillo_south_init(dest: Path) -> None:
    """ALttP-ish south 3/4 block-in: big helm, short body, scutum in front."""
    from PIL import Image, ImageDraw

    im = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    o = (0x0A, 0x08, 0x08, 255)
    bronze = (0xC4, 0x96, 0x4A, 255)
    pompeii = (0xA4, 0x4A, 0x3E, 255)
    linen = (0xB7, 0xA1, 0x81, 255)
    iron = (0x50, 0x54, 0x55, 255)
    wood = (0x68, 0x43, 0x35, 255)
    # short legs, feet toward bottom
    d.rectangle((19, 36, 24, 45), fill=linen, outline=o)
    d.rectangle((26, 36, 31, 45), fill=linen, outline=o)
    # compact torso
    d.rectangle((18, 22, 32, 37), fill=linen, outline=o)
    # scutum as a disc in front of the chest (south)
    d.ellipse((12, 20, 30, 40), fill=pompeii, outline=o)
    d.rectangle((19, 24, 23, 36), fill=wood)
    # manica + short blade on our right
    d.rectangle((31, 24, 36, 33), fill=iron, outline=o)
    d.rectangle((34, 18, 37, 28), fill=bronze, outline=o)
    # bronze galea: dome seen from 3/4 (top of skull visible)
    d.ellipse((15, 8, 34, 26), fill=bronze, outline=o)
    d.ellipse((17, 10, 32, 18), fill=(0x87, 0x6C, 0x57, 255))
    # two visor slits
    d.rectangle((20, 16, 23, 18), fill=o)
    d.rectangle((26, 16, 29, 18), fill=o)
    # fish-crest: dorsal ridge along the crown, fish head toward the back (up)
    d.rectangle((23, 4, 27, 12), fill=bronze, outline=o)
    d.polygon([(25, 1), (30, 7), (20, 7)], fill=bronze, outline=o)
    dest.parent.mkdir(parents=True, exist_ok=True)
    im.save(dest)


DIR_SHORT = {"south": "s", "east": "e", "north": "n", "west": "w"}


def api_bytes(path: str) -> bytes:
    req = urllib.request.Request(
        API + path,
        method="GET",
        headers={
            "Authorization": "Bearer " + token(),
            "Accept": "application/zip, application/octet-stream, */*",
        },
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as r:
            return r.read()
    except urllib.error.HTTPError as e:
        err = _redact(e.read().decode("utf-8", errors="replace"))
        raise SystemExit(f"PixelLab HTTP {e.code} on {path}: {err[:500]}") from e


def poll_job(job_id: str, timeout_s: int = 180) -> dict:
    deadline = time.time() + timeout_s
    last = ""
    while time.time() < deadline:
        job = api("GET", f"/background-jobs/{job_id}")
        status = (job.get("status") or "").lower()
        if status != last:
            print(f"job  {status}")
            last = status
        if status in {"completed", "complete", "success"}:
            return job
        if status in {"failed", "error"}:
            raise SystemExit(f"PixelLab job failed: {json.dumps(job.get('last_response'))[:400]}")
        time.sleep(5)
    raise SystemExit(f"PixelLab job timed out after {timeout_s}s")


def pull_character4(item: dict, char_id: str, job: dict | None = None) -> None:
    raw = api_bytes(f"/characters/{char_id}/zip")
    zf = zipfile.ZipFile(io.BytesIO(raw))
    names = [n.replace("\\", "/") for n in zf.namelist()]
    print("zip", names)
    files: dict[str, str] = {}
    for direction, short in DIR_SHORT.items():
        member = next(
            (
                n
                for n in names
                if n.lower().endswith(f"/{direction}.png") or n.lower() == f"{direction}.png"
            ),
            None,
        )
        if not member:
            continue
        png = zf.read(member)
        rel = f"characters/murmillo_{short}_idle_00.png"
        save_image(rel, png)
        files[direction] = rel
        if direction == "south":
            save_image(item["path"], png)
        print(f"save {rel}")
    if "south" not in files:
        raise SystemExit(f"zip had no south.png; members={names}")
    write_log(
        item,
        (job or {}).get("usage"),
        palette_body() is not None,
        endpoint="/create-character-with-4-directions",
        extra={
            "character_id": char_id,
            "background_job_id": (job or {}).get("id"),
            "files": files,
            "force_colors": True,
            "proportions": "cartoon",
        },
    )


def generate_character4(item: dict, from_character_id: str | None = None) -> None:
    if from_character_id:
        print(f"pull {item['id']} character_id={from_character_id}")
        pull_character4(item, from_character_id)
        return
    pal = palette_body()
    body: dict = {
        "description": item["description"],
        "image_size": {"width": item["w"], "height": item["h"]},
        "text_guidance_scale": 12,
        "outline": "single color black outline",
        "shading": "basic shading",
        "detail": "low detail",
        "view": item.get("view") or "low top-down",
        "isometric": False,
        "force_colors": True,
        "template_id": "mannequin",
        "proportions": {"type": "preset", "name": "cartoon"},
        "seed": item["seed"],
    }
    if pal:
        body["color_image"] = pal
    else:
        print(f"warn {item['id']}: no tiles/palette_nes.png yet, generating without color_image")
        body["force_colors"] = False
    print(f"gen4 {item['id']} {item['w']}x{item['h']} seed={item['seed']}")
    resp = api("POST", "/create-character-with-4-directions", body)
    char_id = resp["character_id"]
    job_id = resp["background_job_id"]
    print(f"     character_id={char_id}")
    job = poll_job(job_id)
    pull_character4(item, char_id, job)


def tile_stem(tile: dict) -> str:
    corners = tile.get("corners") or {}
    vals = [corners.get(k) for k in ("NW", "NE", "SW", "SE")]
    if vals and all(v == "lower" for v in vals):
        return "lower"
    if vals and all(v == "upper" for v in vals):
        return "upper"
    name = (tile.get("name") or tile.get("id") or "tile")[:40]
    return "t_" + "".join(ch if ch.isalnum() else "_" for ch in name)


def write_2x2(src: Path, dest: Path) -> None:
    from PIL import Image

    tile = Image.open(src).convert("RGBA")
    w, h = tile.size
    sheet = Image.new("RGBA", (w * 2, h * 2))
    for y in range(2):
        for x in range(2):
            sheet.paste(tile, (x * w, y * h))
    dest.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(dest)


def save_tileset_images(item: dict, payload: dict, tileset_id: str, usage: dict | None) -> None:
    from PIL import Image

    tiles = ((payload.get("tileset") or {}).get("tiles")) or []
    if not tiles:
        raise SystemExit(f"tileset {tileset_id} has no tiles")
    out_dir = ART / "tiles" / "ludus"
    out_dir.mkdir(parents=True, exist_ok=True)
    files: dict[str, str] = {}
    placed: list[tuple[int, int, Image.Image]] = []
    for tile in tiles:
        img = tile.get("image") or {}
        png = b64_to_png(img.get("base64") or "")
        if not png:
            continue
        stem = tile_stem(tile)
        rel = f"tiles/ludus/{stem}.png"
        save_image(rel, png)
        files[stem] = rel
        pos = tile.get("original_position") or {}
        placed.append((int(pos.get("row") or 0), int(pos.get("col") or 0), Image.open(io.BytesIO(png)).convert("RGBA")))
        if stem == "lower":
            save_image(item["path"], png)
    if "lower" not in files:
        raise SystemExit(f"tileset {tileset_id} has no all-lower tile")
    write_2x2(ART / files["lower"], ART / "tiles/ludus/lower_2x2.png")
    if files.get("upper"):
        write_2x2(ART / files["upper"], ART / "tiles/ludus/upper_2x2.png")
    if placed:
        tw, th = placed[0][2].size
        rows = max(p[0] for p in placed) + 1
        cols = max(p[1] for p in placed) + 1
        sheet = Image.new("RGBA", (cols * tw, rows * th), (0, 0, 0, 0))
        for row, col, im in placed:
            sheet.paste(im, (col * tw, row * th))
        sheet.save(out_dir / "sheet.png")
        files["sheet"] = "tiles/ludus/sheet.png"
    meta = payload.get("metadata") or {}
    write_log(
        item,
        usage,
        True,
        endpoint="/create-tileset",
        extra={
            "tileset_id": tileset_id,
            "files": files,
            "tile_count": len(tiles),
            "view": meta.get("view"),
            "transition_size": meta.get("transition_size"),
        },
    )
    print(f"     tileset_id={tileset_id} tiles={len(tiles)}")


def generate_tileset(item: dict) -> None:
    pal = palette_body()
    body: dict = {
        "lower_description": item["lower"],
        "upper_description": item["upper"],
        "transition_description": item.get("transition") or "",
        "tile_size": {"width": item["w"], "height": item["h"]},
        "mode": "standard",
        "shape_style": "square",
        "enhance": False,
        "text_guidance_scale": 10,
        "outline": "single color black outline",
        "shading": "basic shading",
        "detail": "low detail",
        "view": item.get("view") or "high top-down",
        "transition_size": 0.0,
        "seed": item["seed"],
    }
    if pal:
        body["color_image"] = pal
    dirt = ART / "tiles" / "sample_dirt.png"
    if dirt.is_file():
        body["lower_reference_image"] = _b64_png(dirt)
    print(f"genT {item['id']} {item['w']}x{item['h']} seed={item['seed']}")
    resp = api("POST", "/create-tileset", body)
    tileset_id = resp.get("tileset_id")
    if resp.get("tileset") and (resp["tileset"].get("tiles")):
        save_tileset_images(item, resp, tileset_id or "sync", resp.get("usage"))
        return
    job_id = resp.get("background_job_id")
    if not tileset_id or not job_id:
        raise SystemExit(f"create-tileset returned no job: {list(resp)}")
    print(f"     tileset_id={tileset_id}")
    job = poll_job(job_id, timeout_s=300)
    payload = {"_pending": True}
    for _ in range(12):
        payload = api("GET", f"/tilesets/{tileset_id}", pending_ok=True)
        if not payload.get("_pending"):
            break
        time.sleep(5)
    if payload.get("_pending"):
        raise SystemExit(f"tileset {tileset_id} still 423 after job completed")
    save_tileset_images(item, payload, tileset_id, job.get("usage") or resp.get("usage"))


def generate_edit(item: dict) -> None:
    src = ART / item["edit_from"]
    if not src.is_file():
        raise SystemExit(f"{item['id']} needs {item['edit_from']}")
    pal = palette_body()
    body: dict = {
        "description": item["description"],
        "image": _b64_png(src),
        "image_size": {"width": item["w"], "height": item["h"]},
        "width": item["w"],
        "height": item["h"],
        "no_background": item["no_bg"],
        "text_guidance_scale": 8,
        "seed": item["seed"],
    }
    if pal:
        body["color_image"] = pal
    desc = item["description"]
    if len(desc) > 500:
        desc = desc[:500]
        body["description"] = desc
    print(f"edit {item['id']} from {item['edit_from']} seed={item['seed']}")
    resp = api("POST", "/edit-image", body)
    img = resp.get("image") or {}
    usage = resp.get("usage")
    if not (img.get("base64")):
        job_id = resp.get("background_job_id")
        if not job_id:
            raise SystemExit(f"edit-image returned no image and no job for {item['id']}")
        job = poll_job(job_id)
        last = job.get("last_response") or {}
        img = last.get("image") or {}
        usage = job.get("usage") or usage
    png = b64_to_png(img.get("base64") or "")
    if not png:
        raise SystemExit(f"empty image for {item['id']}: {list((job if 'job' in locals() else resp).keys())}")
    save_image(item["path"], png)
    write_log(item, usage, pal is not None, endpoint="/edit-image")
    time.sleep(0.4)


def generate(item: dict, force: bool) -> None:
    dest = ART / item["path"]
    if dest.is_file() and not force:
        print(f"skip {item['id']} (exists)")
        return
    if item.get("kind") == "palette":
        write_authored_palette(dest)
        write_log(item, None, False, endpoint="authored")
        print(f"authored {item['id']} {item['w']}x{item['h']}")
        return
    if item.get("kind") == "character4":
        generate_character4(item, from_character_id=item.get("_from_character_id"))
        return
    if item.get("kind") == "tileset":
        generate_tileset(item)
        return
    if item.get("kind") == "edit":
        generate_edit(item)
        return
    body: dict = {
        "description": item["description"],
        "image_size": {"width": item["w"], "height": item["h"]},
        "text_guidance_scale": 12 if item.get("kind") == "character" else 10,
        "outline": "single color black outline",
        "shading": "basic shading",
        "detail": "low detail",
        "no_background": item["no_bg"],
        "isometric": False,
        "seed": item["seed"],
    }
    if item.get("view"):
        body["view"] = item["view"]
    if item.get("direction"):
        body["direction"] = item["direction"]
    pal = palette_body()
    if pal:
        body["color_image"] = pal
    else:
        print(f"warn {item['id']}: no tiles/palette_nes.png yet, generating without color_image")
    if item.get("init"):
        init_path = ART / item["init"]
        if not init_path.is_file():
            raise SystemExit(f"missing init image {item['init']}")
        body["init_image"] = _b64_png(init_path)
        body["init_image_strength"] = int(item.get("init_strength") or 350)
    print(f"gen  {item['id']} {item['w']}x{item['h']} seed={item['seed']}")
    resp = api("POST", "/create-image-pixflux", body)
    img = resp.get("image") or {}
    png = b64_to_png(img.get("base64") or "")
    if not png:
        raise SystemExit(f"empty image for {item['id']}")
    save_image(item["path"], png)
    write_log(item, resp.get("usage"), pal is not None)
    time.sleep(0.4)


def print_balance(bal: dict) -> tuple[float, float]:
    usd = float((bal.get("credits") or {}).get("usd") or 0)
    sub = bal.get("subscription") or {}
    gens = float(sub.get("generations") or 0)
    plan = sub.get("plan") or "none"
    status = sub.get("status") or "unknown"
    print(f"PixelLab USD: {usd} | {plan} ({status}) | generations left: {gens}")
    return usd, gens


def select_items(wave: int, only: list[str] | None) -> list[dict]:
    if only:
        want = set(only)
        pool = [i for w in WAVES.values() for i in w]
        items = [i for i in pool if i["id"] in want]
        missing = want - {i["id"] for i in items}
        if missing:
            raise SystemExit(f"unknown id(s): {sorted(missing)}")
        return items
    if wave not in WAVES:
        raise SystemExit(f"unknown wave {wave}; have {sorted(WAVES)}")
    return list(WAVES[wave])


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true", help="GET /balance and exit")
    ap.add_argument("--force", action="store_true")
    ap.add_argument("--wave", type=int, default=0)
    ap.add_argument("--only", nargs="*")
    ap.add_argument(
        "--from-character-id",
        help="Re-pull an existing 4-dir character (no new generation)",
    )
    args = ap.parse_args()
    bal = api("GET", "/balance")
    usd, gens = print_balance(bal)
    if args.check:
        return
    if args.from_character_id:
        items = select_items(args.wave, args.only)
        ART.mkdir(parents=True, exist_ok=True)
        for it in items:
            it = dict(it)
            it["_from_character_id"] = args.from_character_id
            generate_character4(it, from_character_id=args.from_character_id)
        print("done")
        return
    if usd <= 0 and gens <= 0:
        raise SystemExit("PixelLab has no USD credits and no subscription generations")
    items = select_items(args.wave, args.only)
    ART.mkdir(parents=True, exist_ok=True)
    for it in items:
        generate(it, args.force)
    print("done")


if __name__ == "__main__":
    main()
