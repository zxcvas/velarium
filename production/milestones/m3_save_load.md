# M3 — Save / Load

**Date:** 2026-06-26

## Goal
Persist denarii and full roster state between sessions.

## Deliverables
- `Game/SaveData.cs` — JSON-serializable save format (version 1)
- `Game/GameSaveService.cs` — save/load to `saves/velarium_save.json`
- Startup prompt when save exists: New career or Load
- Main menu `[5] Save Game`, `[6] Load Game`
- `saves/` added to `.gitignore`

## Save Format
Human-readable JSON with denarii + fighter list (name, type, stats, condition, victories).

## Verification
- Save after arena win → file written with correct denarii/victories
- Restart → Load restores 664 denarii, Marcus at 4 wins
- Build 0.3.0 passes

## Not in Scope (M3)
- Multiple save slots
- Autosave
- Save on exit prompt

## Next Candidates
- Injury/death after arena
- Ludus upkeep costs
- Event table