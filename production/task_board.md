# Amphiteater Task Board

**Last Updated:** 2026-09-10 (rudis-discharge claimed)

## Now
- [ ] Discharge (*rudis*) — claimed `rudis-discharge` (`cursor/rudis-discharge-085f`)
- [x] Spy intel that pays — claimed `spy-intel-pays` (`cursor/spy-intel-pays-f234`)
- [x] Wave 2 remaining 4-dir idle bodies (thraex, retiarius, secutor, household) — claimed `art-wave2-bodies` (`cursor/art-wave2-bodies-7dba`)
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

## Done this cycle
- Spy intel that pays (2026-09-09) — success seeds dawn locatio kit, thin purse + lift, missed editor, or fessus foe; miss/caught stay
- PixelLab wave 2 4-dir idle bodies (2026-09-09) — thraex, retiarius, secutor, household; murmillo skipped
- PixelLab wave 2 tooling (2026-09-08) — WAVE2 jobs registered; no generate in that PR
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
