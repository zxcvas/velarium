# M1 — Ludus slice (first working demo)

**Date:** 2026-08-26

## Goal
A playable Capuan lanista game in the console: survive days, rent men, feel the money/risk loop, unlock hosting.

## Deliverables
- New game with Roman identity (presets or *tria nomina*)
- Familia of three *tiros* (murmillo, thraex, retiarius)
- Day loop: familia, exercitia, forum, locatio, end day
- Combat with type pairings and *palma / stans / missio / mors*
- Editor contracts (*pro sudore* / *occisus*)
- Hosting locked behind fama + a palma
- Save / load
- Game over when the ludus is empty and broke

## Verification
- `.\build.ps1` succeeds
- `.\run.ps1` reaches a day menu
- Locatio can be taken on day one
- Calendar prints a real Roman date
- Save file written; continue works
