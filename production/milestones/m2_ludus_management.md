# M2 — Ludus Management Stubs

**Date:** 2026-06-26

## Goal
Wire economy, roster interactions, and arena into a playable console loop.

## Deliverables
- `Game/GameState.cs` — denarii (start 500), recruit/train/arena actions
- `Game/GameMenus.cs` — Manage Ludus sub-menu + arena fighter picker
- Recruit stub — 250 denarii, random fighter from name/type pool
- Train stub — 75 denarii, +3–6 to random stat, sets Training condition
- Arena stub — pick healthy fighter, payout by combat rating, +1 victory
- Main menu restructure: Manage Ludus [1], Arena [2], denarii shown

## Verification
- `build.ps1` succeeds
- Recruit deducts denarii and adds fighter
- Train improves stats when affordable
- Arena only accepts Healthy fighters; awards denarii
- Exe runnable end-to-end

## Not in Scope (M2)
- Injury/death after arena
- Persistence
- Real combat resolution

## Next Candidates
- Save/load game state
- Injury system stub
- Ludus facilities / upkeep costs
- Event table / random events