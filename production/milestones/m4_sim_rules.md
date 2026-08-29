# M4 — Sim rules extract + headless report (PR1.5)

**Date:** 2026-08-29

## Goal
Game rules live in `Amphiteater.Sim`, not in the console host. A headless runner measures lethality and cashflow.

## Deliverables
- `src/Amphiteater.Sim/Ludus.cs` — Start, market, locatio bout, EndDay, treat/buy
- `src/Amphiteater.Sim/CareerSim.cs` — locatio-first AI, aggregate stats
- `src/Velarium/Game.cs` — UI only
- `Amphiteater.exe --report [n]`
- `src/Amphiteater.Sim.Tests`

## Verification
- `.\build.ps1` (build + tests)
- New game → locatio → end day → save still works
- `--report 50` prints Ville/Gaius table

## Not in scope
Rooms, household slaves, night ops, Godot, PixelLab, Esc menu, balance retune.

## Next
M2 console: rooms + household + spy/poison/sabotage. Then PixelLab for those rooms.
