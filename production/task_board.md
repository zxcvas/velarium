# Amphiteater Task Board

**Last Updated:** 2026-10-08 (`sprint-courtyard-morning`)

Now / Next / Blocked here is the queue. Do not treat `TASK_SCHEDULE.md` or agent memory as a second board.

## Now

## Next Up

Sprint **courtyard-morning** (planned 2026-10-08 from `master` `f9e3093`). Claim **one** bullet. Console stays the playable host. No virtus / vigor / palmae retune. No PixelLab. Wave 3b stays blocked until `yard-walk-hook` is Done.

- `yard-familia` — Place every **living** gladiator on the yard from the loaded save. South idle frame 0 only: `characters/{murmillo,thraex,retiarius,secutor}_s_idle_00.png` (murmillo may fall back to `sample_murmillo_s.png`). Skip a man whose PNG is missing. Dead men and household stay off the yard. No new art, no animation, no new `Ludus` verbs. End Day stays `Ludus.EndDay` + `Save.Write`. Editor unzip is not required. Verify: `dotnet test src/Amphiteater.Sim.Tests` and `dotnet build godot/Amphiteater.Godot.csproj -f net8.0`.
- `yard-day-order` — Set `Gladiator.Order` to Palus, Sparring, or Requies (the field `FamiliaScreen` already writes; Latin via `Content.OrderLat`). Click the man, or a HUD cycle if hit-testing needs the editor. No new order, no combat, no forum. End Day still goes through `Ludus.EndDay`, which consumes the order. HUD shows the order. Same verify as `yard-familia`.
- `yard-walk-hook` — While `Order` is Palus or Sparring, play the wave-3a south walk already on disk: `characters/{slug}_s_walk_00.png` … `_03.png`. Requies and None stay on idle frame 0. After End Day the order clears and the sprite returns to idle. Missing walk frames fall back to idle. This is the hook in `assets/art/ASSET_MAP.md`. Do not generate Wave 3b. Do not change `Combat`.
- `forum-locatio-320` — Docs only. `docs/design/04_forum_locatio.md` locks Forum and Locatio at the Godot viewport (320×180, integer 4×, roles in `docs/design/03_ui_tokens.md`). Stalls from `Game.ForumScreen` (catasta, household, arms, medicus, rumors); locatio from `Game.LocatioScreen`. The 2026-10-01 canvas was 1280×720 and private — this doc replaces it as the spec. Harena stays out. No Godot scenes, no C#.

Human gates — do not claim:

- Godot courtyard editor run — unzip `Godot_v4.6.3-stable_mono_*.zip` to `tools/godot-editor/` (gitignored) and open `godot/project.godot`. C# restore / net8 shipped in PR #12. See `godot/README.md`.
- Enable GitHub Pages Actions for the marketing site (Settings → Pages → Source = GitHub Actions) so `master` deploys `marketing/site/`.

## Done this cycle
- Sprint plan courtyard-morning (`sprint-courtyard-morning`, 2026-10-08) — Next sequenced from an empty Now. No sim or scene code.
- UI tokens v1 (`ui-tokens`, 2026-10-01) — `docs/design/03_ui_tokens.md`; semantic roles in `marketing/site/styles.css`; Godot `ui/UiTokens.cs` + `ui/VelariumTheme.cs` on the courtyard HUD. Contrast fixes: secondary text off `iron_rust` (3.4:1), primary CTA label large/bold (4.3:1). No new palette colours. HUD pixel font: Silkscreen (OFL) Regular/Bold in `godot/fonts/`, AA off — Press Start 2P overflowed the status label.
- PixelLab wave 3a south (`art-wave3a-south`, 2026-09-14, PR #16) — 12 south idle/walk/attack jobs + 48 PNG frames for murmillo/thraex/retiarius/secutor; Wave 2 retiarius/household recreate 4-dir idles; tooling `tools/pixellab_wave3a.py`. Soft-fail retiarius kit + secutor crest notes stay. Household idles only (no 3a anims). No Infected/Latifundium. Hold Wave 3b until Godot walk/palus hook.
- Forum equipment sprint shipped as PRs #18–#23 (design → model → catalog → forum buy → assign → combat).
- Additive equipment combat (`equip-combat`, 2026-09-14, PR #23) — `Combat.Score` tier bonuses / +4 cap / retiarius −2 / mismatch −1; locatio culture dock reuses `LocatioWrongSudoreDock` / `LocatioWrongOccisusDock` (no stack on wrong *armatura*). No virtus / vigor / palmae retune.
- Assign / unequip from Armory (`equip-assign`, 2026-09-14, PR #22) — `Ludus.Assign` / `Unequip`; occupied slot returns the old piece first; death and *rudis* strip loadout → Armory; familia inspect UI. No `Combat.Score`.
- Forum equipment buy (`equip-forum-buy`, 2026-09-14, PR #21) — console forum stall: browse by culture/slot, buy into the Ludus Armory with purse check; resale from unequipped Armory at `ResalePrice`. No assign UI.
- Static equipment catalog (`equip-catalog`, 2026-09-14, PR #20) — 36 templates Punic/Greek/Roman × Helmet/Armor/Shield/Weapon × T1/T2/T3; `EquipmentCatalog.BuyPrice` / `ResalePrice` / `CreateInstance`; `Content.EquipmentNom` (Roman Weapon = rete+fuscina). No forum UI.
- Ludus Armory + loadout model/save (`equip-model-save`, 2026-09-14, PR #19) — `EquipmentItem` / Armory on `GameState` / optional slots on `Gladiator`; round-trip in `amphiteater_save.json`
- Forum equipment design lock (`equip-design-lock`, 2026-09-14, PR #18) — `docs/design/02_equipment.md` + Forum equipment knobs in `production/economy.md`. No C#.
- Board hygiene + store name lock (`board-hygiene-velarium-name`, 2026-09-12, PR #17) — product/store name **Velarium**; Amphiteater stays historical flavor / subtitle only
- Marketing site shell (`marketing-site-shell`, 2026-09-12, PR #14; CI fix #15) — static `marketing/site/` landing, Steam wishlist stub, `marketing/BRIEF.md`; Pages from `marketing/site/`
- Godot courtyard net8 (`godot-courtyard-net8`, 2026-09-12, PR #12) — Sim `net8.0;net10.0`, cloud env SDKs 8+10, restorable Godot 4.6 C# project. Console host stays net10.
- Board hygiene after PR #12 (`board-hygiene-godot-net8`, 2026-09-12) — closed the in-flight courtyard claim; Now empty; Next is editor unzip only
- Board hygiene (`board-hygiene`, 2026-09-11) — Now cleared of finished claims; locatio-survival results recorded; Godot stays Next/Blocked
- Locatio-only career survival (`locatio-survival`, 2026-09-11, PR #7) — `--report --locatio` rest/skip/host; day-21 ruin ~6% (was ~100%); default UpgradeStall fine
- CareerSim kitchen upgrade / stall AI (`career-kitchen-ai`, 2026-09-10, PR #6) — `--report` hires, upgrades culina, picks dishes
- Discharge / *rudis* (`rudis-discharge`, 2026-09-10, PR #5) — five palmae, fama 20, two-thirds value; man leaves the roster; console + tests
- Spy intel that pays (`spy-intel-pays`, 2026-09-09, PR #4) — success seeds dawn locatio kit, thin purse + lift, missed editor, or fessus foe; miss/caught stay
- PixelLab wave 2 4-dir idle bodies (`art-wave2-bodies`, 2026-09-09, PR #3) — thraex, retiarius, secutor, household; murmillo skipped
- PixelLab wave 2 tooling (`pixellab-wave2-tooling`, 2026-09-08, PR #2) — WAVE2 jobs registered; no generate in that PR
- Root `AGENTS.md` (`agents-md-pointer`, 2026-09-08, PR #1) — cycle pointer; this board is SoT, not `TASK_SCHEDULE.md`
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
Playtest **holds** (`production/playtest_m2.md`): no combat virtus / vigor / palmae retune this sitting; no engine switch before the console loop is loved. Equipment (2026-09-14) is **additive** knobs only — see `docs/design/02_equipment.md`.
Wave 3b (hit / down / ko) stays blocked until `yard-walk-hook` is Done (`assets/art/ASSET_MAP.md`). Sprint courtyard-morning does not generate it.
Thermopolium: lv2 street bowls if cook staffed; lv3 dish pick (*puls*, lentil, *moretum*, posca). Forum rumor can move one dish. Knobs in `production/economy.md`.
Empty purse closes at dusk. `--report --locatio` day-21 ruin ~6% (PR #7; was ~100%). Default `--report` (UpgradeStall) is fine — kitchen stall income. See `production/economy.md`.
Wave 1 tileset lower is brick-ish (use `sample_dirt` for yard fill).
Godot 4.6 **editor** is still a local unzip: `Godot_v4.6.3-stable_mono_*.zip` → `tools/godot-editor/` (gitignored). C# restore / net8 targeting shipped in PR #12. See `godot/README.md`.
