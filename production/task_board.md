# Amphiteater Task Board

**Last Updated:** 2026-09-01 (M2 playtest filled; empty-purse ruin)

## Now
- [x] Engine: keep console for first demo
- [x] Role: lanista first, host munera as progression
- [x] Playable day loop (familia, drill, forum, locatio, hosting unlock)
- [x] Roman names, Capua, civil calendar, Gaius-style purses
- [x] Save / load
- [x] Extract `Amphiteater.Sim` classlib
- [x] PR1.5: `Ludus` + `--report`
- [x] M2: rooms, household slaves, night ops (spy / poison / sabotage)

## Next Up
- Godot courtyard (verdict **holds**) — after empty-purse close is in
- Kitchen as *thermopolium* (playtest: income after lv2, dish menu after lv3) — not this patch
- ASCII / arrow-key UI
- Wave 2 remaining 4-dir bodies (thraex, retiarius, secutor, household)
- A second fight in a hosted munus
- Discharge (*rudis*)

## Done this cycle
- M0 skeleton (2026-06-21)
- GitHub June path — superseded by the playable slice
- M1 ludus slice (2026-08-26)
- Sim extract + expansion design (2026-08-28)
- PR1.5 Ludus rules + career report (2026-08-29)
- M2 rooms / household / night ops (2026-08-29)
- PixelLab NES wave 0–1 (2026-08-31)
- CareerSim kitchen AI + `--report 200` refresh (2026-09-01)
- Human 12-day M2 playtest filled (holds; 0 denarii did not end)

## Blocked / Questions
Playtest **holds**. Spy often “nothing useful”. ~120/fight was hosted palma, not locatio sweat (22–37). Knobs: `production/economy.md` (thermopolium proposed, **no food market in code yet**).
Empty purse **closes at dusk**. `--report 200` after that: **100% ruin**, mean **20 days**, death 0.120, kitchen 100%, hosting 91% by day 30. The locatio-only AI always drains to 0; a human who hosts can last longer. Arrow-key UI not this patch.
Wave 1 tileset lower is brick-ish (use `sample_dirt` for yard fill).
