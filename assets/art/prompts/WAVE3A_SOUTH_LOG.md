# Wave 3a south — idle / walk / attack

**When:** 2026-09-12 (box).  
**Endpoint:** `POST /animate-character` `mode=v3` `frame_count=4` `keep_first_frame=false`  
**Palette:** `tiles/palette_nes.png` as `color_image` + `force_colors`.  
**Directions:** south only. Household: **no anims** this wave.  
**Balance:** 1997 → 1984 gens (Δ −13).

Victor-approved Wave 2 recreates landed on the same sitting:

| slug | seed | character_id | kit review |
|---|---|---|---|
| `retiarius` | 1793 | `e227393a-96ad-4210-aeb6-afb42ff0ae96` | **soft-fail** (net / galerus / trident read is weak) — accepted for motion |
| `household` | 1795 | `a317ac42-f430-40b0-a295-cf13d6401306` | **PASS** — undyed tunic, no helm / weapon; idle only |

Previous Wave 2 ids (superseded, do not re-animate): retiarius `dfb3e676-0aa6-4a7a-8c37-69af4c8ba290`, household `673a08e7-1037-4f48-8fc5-8a0eb5375c09`.

## 12 animation jobs

| slug | clip | job_id | character_id | out |
|---|---|---|---|---|
| murmillo | idle | `39974873-7a73-4257-a701-fcf386642b00` | `b531be43-c3c1-481d-b7d4-6c2f120e6609` | `characters/murmillo_s_idle_{00..03}.png` |
| murmillo | walk | `073601db-3b8d-4300-9a3b-06e5d0d869cd` | `b531be43-c3c1-481d-b7d4-6c2f120e6609` | `characters/murmillo_s_walk_{00..03}.png` |
| murmillo | attack | `d0e936f1-e5f4-4f73-bbe9-7a48f1d31958` | `b531be43-c3c1-481d-b7d4-6c2f120e6609` | `characters/murmillo_s_attack_{00..03}.png` |
| thraex | idle | `907c3a7e-a80e-451c-a3ee-44cf65fe96c5` | `dc9e6a43-a406-45c6-b2e8-3838f3a77c4c` | `characters/thraex_s_idle_{00..03}.png` |
| thraex | walk | `c0859df0-5a6e-403e-ad96-4e4b1308af9a` | `dc9e6a43-a406-45c6-b2e8-3838f3a77c4c` | `characters/thraex_s_walk_{00..03}.png` |
| thraex | attack | `cadc0eae-267e-4f47-ab67-89b8501f25bb` | `dc9e6a43-a406-45c6-b2e8-3838f3a77c4c` | `characters/thraex_s_attack_{00..03}.png` |
| retiarius | idle | `ccca08ac-9649-46d2-8cda-b09258277d42` | `e227393a-96ad-4210-aeb6-afb42ff0ae96` | `characters/retiarius_s_idle_{00..03}.png` |
| retiarius | walk | `0d160659-563a-410e-b011-2627a2f3b66c` | `e227393a-96ad-4210-aeb6-afb42ff0ae96` | `characters/retiarius_s_walk_{00..03}.png` |
| retiarius | attack | `cefbd92d-5dc5-4aae-993e-be303cc115e2` | `e227393a-96ad-4210-aeb6-afb42ff0ae96` | `characters/retiarius_s_attack_{00..03}.png` |
| secutor | idle | `0ceb4d86-f439-491a-8b9f-a8342b6695ee` | `4fb8a6da-eef8-4c8d-a31a-31ca17b3360e` | `characters/secutor_s_idle_{00..03}.png` |
| secutor | walk | `22a6f499-0918-4dd7-aa91-8d011224975a` | `4fb8a6da-eef8-4c8d-a31a-31ca17b3360e` | `characters/secutor_s_walk_{00..03}.png` |
| secutor | attack | `41e28d27-a063-4a75-be53-1b565374e85c` | `4fb8a6da-eef8-4c8d-a31a-31ca17b3360e` | `characters/secutor_s_attack_{00..03}.png` |

Per-job stubs: `assets/art/prompts/wave3a_{slug}_{clip}.json`.

## Pull (no re-bill)

Character zips already hold rotations + the south clips after the box run.

```
python tools/pixellab_wave3a.py --pull
```

Maps `{Name}/animations/{clip}/south/frame_000.png` … `_003.png` → `{slug}_s_{clip}_{00..03}.png`.  
Replaces 4-dir idles for **retiarius** + **household** only; other rotations only if missing.

Do not `--animate` unless Art Director asks to re-bill. Household has no Wave 3a clips (3b/work later).

## Binaries in this PR

Cloud agent had **no** `PIXELLAB_API_TOKEN` / `PIXELLAB_SECRET` / `pixellab.env`, so zips were not pulled. On-disk 4-dir idles remain the 2026-09-09 Wave 2 stills (old retiarius / household ids). The 48 south animation PNGs are **not** in the tree.

Art Director follow-up: run `python tools/pixellab_wave3a.py --pull` on a box with `pixellab.env`, then commit:

- 48 frames: `{murmillo,thraex,retiarius,secutor}_s_{idle,walk,attack}_{00..03}.png`
- Recreate idles: `retiarius_{n,e,s,w}_idle_00.png` and `household_{n,e,s,w}_idle_00.png`
