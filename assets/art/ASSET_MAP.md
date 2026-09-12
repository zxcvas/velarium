# Amphiteater — PixelLab asset map

**Status:** wave 0 locked. Wave 1 generated 2026-08-31 (32px tileset + six props). Wave 2 4-dir idle bodies **generated** (thraex / secutor 2026-09-09; retiarius seed 1793 + household seed 1795 Victor-approved recreate 2026-09-12). Wave 3a **shipped south** — 12 idle/walk/attack jobs logged; PNG pull is `python tools/pixellab_wave3a.py --pull`. Household: idles only, no 3a anims.  
**Engine:** PixelLab v2 only for production art.  
**Consumers:** later Godot; console stays text.  
**Style:** `STYLE.md` — NES/SNES, cartoon KO, Pompeii kit.

PixelLab `GET /balance` **200** (2026-08-31): token valid. USD 0; Tier 1 subscription generations pay for pixflux. Run `python tools/pixellab_gen.py --check`.

Session Imagine stills are reference, not the pack.

---

## Constraints we are designing around

| PixelLab | We do |
|---|---|
| 4-dir character (48×48 / 64×64) | 4-dir **idles** for five bodies. No 8-dir. |
| Animate-with-text: even frames 4–16; 32–64px → more frames | Generate **4 frames**. Engine may loop 2. |
| Tileset 16 or 32 px, two terrains + transition | Dirt + plaster portico. |
| Credits cost | South-facing combat clips first; other dirs later. |

NES walk is 2 frames; SNES often 4. We **ask PixelLab for 4**, keep all or subsample.

---

## What the sim actually needs (so we don't draw unused rooms)

**On the courtyard (Godot later, same data as M2):** palus, cellae, kitchen, porta, medicus stall; household posted 1:1; gladiators in the yard.

**On the sand:** one pair, four *armaturae*, outcomes palma / stans / missio / mors.

**At night:** household (or a gladiator) at a rival porta — spy / poison / sabotage. Reuse walk + a night porta tile.

Not in wave 1: lanista avatar, aedile, hoplomachus, second storey, Ludus Magnus, wax-tablet UI, 8-dir.

---

## Sizes

| Kind | Native | Godot |
|---|---|---|
| Ground / wall tiles | 16×16 (fallback 32×32) | ×4 |
| Map props (palus, hearth, porta) | 32×32, no background | ×4 |
| Characters | 32×32 or 48×48, 4-dir, no background | ×4 |
| Combat / KO FX | 32×32, no background | ×4 |

---

## Wave 0 — lock (2–3 calls)

Prove palette and chunkiness **before** a tileset.

| id | PixelLab | Size | Out |
|---|---|---|---|
| `palette_nes` | **authored** 16-bar strip (pixflux made a scene) | 64×16 | `tiles/palette_nes.png` |
| `sample_dirt` | pixflux, high top-down, seamless dirt | 32×32 | `tiles/sample_dirt.png` |
| `sample_murmillo_s` | **4-dir** (`POST /create-character-with-4-directions`), south copied to lock | 48×48 | `characters/sample_murmillo_s.png` + `murmillo_{n,e,s,w}_idle_00.png` |

**Stop and look.** If it looks like a painting, tighten palette and size. If it looks like ALttP dirt and a toy soldier, continue.

---

## Wave 1 — courtyard (map)

| id | Kind | PixelLab | Frames | Notes |
|---|---|---|---|---|
| `tileset_ludus` | tileset | `POST /create-tileset` **32px** (16px rejected the 32px dirt reference) | static | Lower should be packed dirt; PixelLab trends brick. Prefer `sample_dirt` as yard fill. Upper is plaster floor. |
| `prop_palus` | object 1-dir | pixflux / map-object | 1 | Wooden stake. |
| `prop_hearth` | object 1-dir | same | 1 | Sooty brick + pot. No text on sacks. |
| `prop_porta` | object 1-dir | same | 1 | One gate, iron bands. |
| `prop_porta_night` | object 1-dir | edit of porta, night palette | 1 | Same gate, torch. Night ops. |
| `prop_cellae` | object or tile | straw + plaster cell | 1 | Interior when you click a cell. |
| `prop_medicus` | object 1-dir | stall / bag | 1 | Unbuilt = missing sprite. |

No worker-in-kitchen unique building interiors in wave 1 — the posted slave is the character sprite on the tile.

---

## Wave 2 — bodies (4-dir idle only)

`POST /create-character-with-4-directions`. 48×48 request, PixelLab 68×68 canvas (same as wave 0 murmillo), low top-down, cartoon mannequin, palette `tiles/palette_nes.png`. Idle only. Persist PixelLab `character_id` in the prompt log (not secrets).

Murmillo 4-dir idle shipped in wave 0 (`murmillo_{n,e,s,w}_idle_00.png`). Remaining jobs pulled `--from-character-id` (do not re-bill create):

| id | Seed | character_id | Who | Out |
|---|---|---|---|---|
| `char_thraex` | 792 | `dc9e6a43-a406-45c6-b2e8-3838f3a77c4c` | thraex: griffin helm, parmula, sica, long greaves | `characters/thraex_{n,e,s,w}_idle_00.png` |
| `char_retiarius` | 1793 | `e227393a-96ad-4210-aeb6-afb42ff0ae96` | retiarius recreate: bare head, galerus, net+trident, tunic. **Soft-fail kit** accepted for motion | `characters/retiarius_{n,e,s,w}_idle_00.png` |
| `char_secutor` | 794 | `4fb8a6da-eef8-4c8d-a31a-31ca17b3360e` | secutor: smooth two-eyehole helm, scutum, no fish crest | `characters/secutor_{n,e,s,w}_idle_00.png` |
| `char_household` | 1795 | `a317ac42-f430-40b0-a295-cf13d6401306` | household recreate **PASS**: undyed tunic, no helm/weapon | `characters/household_{n,e,s,w}_idle_00.png` |

Prompt logs: `assets/art/prompts/char_{thraex,retiarius,secutor,household}.json`. Re-pull: `python tools/pixellab_gen.py --wave 2 --only char_thraex --from-character-id <uuid>` (skip murmillo).

Foes on the sand **reuse** the four *armaturae* (palette swap later if we want rival tint).

**Not yet:** player lanista, editors, women of graffiti, horses.

---

## Wave 3 — motion (south first)

`POST /animate-with-text` or `animate-character` on the saved id. **Always 4 frames, 32 or 48 px, no background.** Engine: 8–12 fps, loop idle/walk.

### Per gladiator (×4 armaturae)

| Clip | Frames in | Frames used | Loop | When |
|---|---|---|---|---|
| `idle` | 4 | 2 (1 and 3) | yes | yard + sand wait |
| `walk` | 4 | 4 | yes | courtyard path |
| `palus` | 4 | 4 | yes | DayOrder.Palus |
| `attack` | 4 | 4 | no | harena swing |
| `hit` | 4 | 2 | no | took a beat |
| `down` | 4 | 2 | hold last | missio / stans fall |
| `ko` | 4 | 4 | hold last | mors — cartoon |

**Wave 3a (shipped south):** idle, walk, attack. 4 types × 3 clips = **12 anim jobs**. Job table + pull notes: `prompts/WAVE3A_SOUTH_LOG.md`. Tooling: `python tools/pixellab_wave3a.py --pull`.

**Wave 3b:** hit, down, ko south. 4 × 3 = 12.

**Wave 3c:** walk east (mirror for west); skip north walk or reuse south. NES did this.

### Household (×1)

| Clip | Frames in | Used | When |
|---|---|---|---|
| `idle` | 4 | 2 | posted at a room |
| `walk` | 4 | 4 | night ops approach |
| `work` | 4 | 4 | optional; idle is enough for v1 |

No household attack/ko in v1 (if sent as night actor and killed: vanish + KO puff).

### Shared FX (not per character)

| id | Frames in | Used | Look |
|---|---|---|---|
| `fx_clash` | 4 | 4 | bronze spark, no blood |
| `fx_ko` | 4–8 | 4 | red puff + stars, X-eyes |
| `fx_missio` | 4 | 4 | wooden staff / white flag pixel |
| `fx_palma` | 4 | 4 | palm leaf, tiny |

Gore lives **here**, not on the body sheet.

---

## Wave 4 — harena floor + night (still)

| id | Kind | Frames |
|---|---|---|
| `tile_sand` | 16×16 sand, seamless | 1 |
| `tile_podium` | strip of first seats | 1 |
| `scene_night_gate` | optional 64×48 still if porta_night + walk is not enough | 1 |

Combat staging: two character sprites + `tile_sand` + FX. No full-screen painted bout.

---

## Frame budget (if we stop after 3a)

```
Wave 0:     3 stills
Wave 1:     1 tileset + 6 props
Wave 2:     5 × 4-dir idles
Wave 3a:    12 south anims (4 frames each)
FX:         4 clips
────────────────
~30 PixelLab jobs before Godot
```

Full 3b+3c is another ~20. Do not animate 4 dirs × 7 clips × 5 bodies (140 jobs).

---

## Godot clip names (later)

`{role}_{dir}_{clip}_{frame:02}`  
Example: `murmillo_s_walk_02.png`

4-dir: `n` `e` `s` `w`. Mirror `e` → `w` if we never generate west.

---

## Prompt rules (every PixelLab call)

1. Prefix: `NES SNES era pixel art, 16 color, chunky pixels, 1px outline, transparent background.`
2. Attach authored `tiles/palette_nes.png` as `color_image`. Never the Imagine `style_lock.png`.
3. Size ≤ 48 for characters; 16 or 32 for tiles.
4. Negative in description: `not painterly, not 3D, not isometric diorama, not photoreal, not muscle cuirass.`
5. Log JSON next to PNG: endpoint, seed, size, character_id, frames requested vs kept.
6. Seed family: **782 + slot** (same as the civil year).

---

## Cartoon KO (so iugula stays a game)

| Outcome | Picture |
|---|---|
| Palma | attack last frame + `fx_palma` |
| Stans | both `down` frame 1, then stand |
| Missio | `down` + `fx_missio` |
| Mors | `ko` 4 frames + `fx_ko` red puff. No wound drawing. |

Crowd is a **tile** of heads, not a sim of pollice verso.

---

## Out of pack

- 8-dir, 128px sheets, isometric courtyard
- Unique sprite per named gladiator (armatura is the identity)
- Sexual content, children
- Imagine as production (mood board only)
- Generating before wave 0 passes the squint test

---

## Next action (human)

1. Wave 0–2 idles are generated (murmillo wave 0; thraex / secutor wave 2; retiarius + household recreate 2026-09-12).
2. Wave 3a south idle/walk/attack jobs shipped (12 ids in `WAVE3A_SOUTH_LOG.md`). **PNGs still need `--pull`** if this tree has no token — Art Director pushes the 48 frames + recreate idles.
3. Re-pull zips (no create): `python tools/pixellab_wave3a.py --pull`. Wave 2 still: `python tools/pixellab_gen.py --wave 2 --only char_<id> --from-character-id <uuid>`.

Imagine `assets/art/style_lock.png` is mood only — never `color_image`.
