# Subagent: Artist

## Role
Dispatches PixelLab asset batches. One artist, one manifest. Several artists may run at the same time when each has a different manifest id.

## Command
From the repo root:

```bash
python3 tools/pixellab_batch.py --check
python3 tools/pixellab_batch.py --manifest assets/art/batches/<id>.json
python3 tools/pixellab_batch.py --manifest assets/art/batches/<id>.json --run
```

Dry-run is the default. `--run` is what spends. `--check` reads `GET /balance` and does not generate.

## Parallel work
- Inside one manifest the process keeps at most 4 jobs in flight (default 3).
- The organizer may dispatch one artist per manifest in parallel.
- Do not put two artists on the same manifest. If it happens, the claim file under `assets/art/prompts/batch-claims/` (gitignored) makes the second process skip jobs it does not own.
- A claim is taken before the POST. `--resume` polls a claim that already has a PixelLab job id. It does not submit again. A claim with no job id stays blocked so a lost response cannot be billed twice.

## Holds
`wave-3b`, `wave-3c`, and `wave-4` manifests refuse `--run` until the human lifts the hold on the task board. Do not pass `--hold-lifted` on your own. Wave 3b also waits on the Godot walk/palus hook (`assets/art/ASSET_MAP.md`).

The queued batch is `assets/art/batches/wave3b-south.json` (hit / down / ko, south, four *armaturae*). Dry-run it. Do not run it while the gate is held.

## Constraints
- Token is `PIXELLAB_API_TOKEN` or gitignored `pixellab.env`. Never print it, never commit it.
- Palette is `assets/art/tiles/palette_nes.png` via `attach_palette`. Never `style_lock.png`.
- Do not retune combat. Do not switch the playable host off the console.
- Do not start a second wave in the same dispatch. Household has no 3b clips.
- Log the summary JSON the tool writes. Do not invent frames that were not pulled.

## Inputs
- Manifest path and whether the board says that gate is lifted.
- Acceptance: dry-run lists every job; `--run` (only if lifted) writes frames named `{slug}_s_{clip}_{00..03}.png` and a summary with no failures.

## Outputs
- PNGs under `assets/art/characters/` when a run was allowed.
- `assets/art/prompts/batch_<id>.json` summary.
- A short note: balance before and after, completed count, any blocked claim.
