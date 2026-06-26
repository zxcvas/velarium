# M1 — Gladiator Roster

**Date:** 2026-06-26

## Goal
First real game system: a basic Gladiator data model and a viewable ludus roster in the console.

## Deliverables
- `Models/Gladiator.cs` — name, type, STR/AGI/END, condition, victories, combat rating
- `Models/GladiatorType.cs` — Murmillo, Retiarius, Secutor, Thraex, Hoplomachus
- `Models/GladiatorCondition.cs` — Healthy, Training, Injured, Dead
- `Game/LudusRoster.cs` — starter roster (3 fighters) + formatted table output
- Main menu option `[2] View Ludus Roster`

## Verification
- `build.ps1` succeeds
- Menu choice 2 prints roster table with 3 starter gladiators
- Exe remains runnable; arena still stubbed

## Not in Scope (M1)
- Recruiting, training, injuries, persistence
- Combat resolution
- Economy

## Next Candidates
- Roster interactions (recruit stub, train stub)
- Simple economy / denarii counter
- Arena combat stub wired to roster fighters