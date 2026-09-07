# Amphiteater Task Board

**Last Updated:** 2026-09-01 (arrow-key menus; Godot blocked on net8)

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
- Godot courtyard — needs .NET 8 targeting pack (see `godot/README.md`)
- Wave 2 remaining 4-dir bodies (thraex, retiarius, secutor, household)
- Spy intel that pays (playtest: “nothing of value”)
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
- Kitchen thermopolium lv2 stall + lv3 dishes (2026-09-01)
- Console menus: arrows / tab / Enter / Esc (numbers still work)
- Hosted munus: two pairs, one gate fee

## Blocked / Questions
Playtest **holds**. Thermopolium: lv2 street bowls if cook staffed; lv3 dish pick (*puls*, lentil, *moretum*, posca). Forum rumor can move one dish. Knobs in `production/economy.md`.
Empty purse closes at dusk. Locatio-only `--report` still ~100% ruin by day ~20. AI does not upgrade the kitchen, so stall income is mostly a human path.
Wave 1 tileset lower is brick-ish (use `sample_dirt` for yard fill).
Godot 4.6 C# courtyard is blocked: this SDK is net10-only; Godot still wants net8. Mono zip is in Downloads. See `godot/README.md`.
