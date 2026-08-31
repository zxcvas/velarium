# Amphiteater — PixelLab asset map

**Status:** wave 0 locked. Wave 1 generated 2026-08-31 (32px tileset + six props). Squint before wave 2.  
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

`POST /create-character-with-4-directions`. Persist PixelLab `character_id` in the prompt log (not secrets).

| id | Who | Dirs | Still frames |
|---|---|---|---|
| `char_murmillo` | murmillo | N S E W | 4 |
| `char_thraex` | thraex | 4 | 4 |
| `char_retiarius` | retiarius | 4 | 4 |
| `char_secutor` | secutor | 4 | 4 |
| `char_household` | household slave | 4 | 4 |

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

**Wave 3a (ship):** **south only** — idle, walk, attack. 4 types × 3 clips = **12 anim jobs**.

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

1. Token works. `python tools/pixellab_gen.py` (default wave 0) writes `palette_nes`, `sample_dirt`, `sample_murmillo_s`.
2. Stop and look. If it looks like a painting, tighten palette and size. If it looks like ALttP dirt and a toy soldier, continue.
3. Then tileset, then 4-dir murmillo, then the rest.

Imagine `assets/art/style_lock.png` is mood only — never `color_image`.
