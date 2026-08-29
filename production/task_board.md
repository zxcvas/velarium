# Amphiteater Task Board

**Last Updated:** 2026-08-29 (PR1.5 sim rules)

## Now
- [x] Engine: keep console for first demo
- [x] Role: lanista first, host munera as progression
- [x] Playable day loop (familia, drill, forum, locatio, hosting unlock)
- [x] Roman names, Capua, civil calendar, Gaius-style purses
- [x] Save / load
- [x] Extract `Amphiteater.Sim` classlib (data, combat, calendar, save)
- [x] PR1.5: remaining rules in `Ludus` + headless `--report`

## Next Up
- M2 on **console** (no Godot / PixelLab until the verbs exist):
  - Unlock/upgrade rooms (cellae, kitchen, medicus, porta)
  - Household slaves assigned to rooms (not gladiators)
  - Night ops vs one rival camp: spy / poison / sabotage
- Playtest / balance pass using `--report` vs Ville ~10% and Gaius cashflow
- A second fight in a hosted munus
- Discharge (*rudis*) after enough palmae
- PixelLab style sheet + tiles for the M2 rooms (after M2)
- Godot courtyard, then Android export

## Done this cycle
- M0 skeleton (2026-06-21)
- GitHub June path: roster / ludus stubs / save-load (M1–M3, 2026-06-26) — superseded by the playable slice
- M1 ludus slice (2026-08-26)
- Sim extract + expansion design (2026-08-28)
- PR1.5 Ludus rules + career report (2026-08-29)

## Blocked / Questions
None. M2 is the next code after this lands.

`--report 50` snapshot (locatio-first AI, always mitte on own man): 16% ruined by day 60; combatant death 0.13 (Ville ~0.10) but **own deaths 0** (AI never yields); purse 621 → 75 by day 30; hosting unlocked 100% by day 30; mean fama ~81. Not unplayable. Balance pass should SimRolls on own fallen before retuning lethality.
