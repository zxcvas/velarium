# M5 — Domus, household, night ops (M2)

**Date:** 2026-08-29

## Goal
The ludus is a house: rooms to raise, household slaves to post, and a night action against one rival camp. Console menus, same JSON save.

## Deliverables
- Rooms: palus, cellae, kitchen, porta (lv 1); medicus stall unbuilt
- Household market at the forum; assign 1:1 to kitchen / porta / medicus
- Upgrades finish at EndDay (cellae add beds)
- Night: rest / spy / poison / sabotage vs a named rival
- Kitchen staffed cuts barley; medicus staffed cuts treat fee; porta staffed lowers catch chance

## Verification
- `dotnet test src/Amphiteater.Sim.Tests`
- New game → Forum → household slave → Domus assign cook → End day (upkeep line names household)
- Domus night spy with a slave
- Old saves load (`EnsureHouse`)

## Not in scope
Godot, PixelLab generation, Android, *rudis*, second hosted pair.
