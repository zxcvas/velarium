# Amphiteater — Task Schedule

Long-horizon **track list**. Now / Next / Blocked is [`production/task_board.md`](production/task_board.md). Do not treat this file, or agent memory, as a second queue.

## Tracks
- **Foundation & Tooling**
- **Game Systems** (roster, economy, combat, time)
- **Presentation** (UI, text, menus, events)
- **Content & Narrative**
- **Art & Audio**
- **Quality & Release**
- **Meta / Agents & Workflow**

## Track status (not a dispatch list)

### Foundation & Tooling
- [x] Finalize language/engine choice (C# console for M1; Godot deferred)
- [x] Core project structure / data loading
- [x] Persistence (save/load)
- [x] Headless career report (`--report`)
- [x] Sim test project
- [ ] Logging / debug tools
- [ ] Build & CI pipeline (local `dotnet test` in `build.ps1`)

### Game Systems
- [x] Gladiator model + attributes
- [x] Ludus / familia (thin)
- [x] Economy & market (denarii, locatio, upkeep)
- [x] Event / calendar system (Roman civil date + night events)
- [x] Combat / spectacle (one pair, four armaturae)
- [x] Reputation (fama ludi + personal fama)
- [x] Rules extracted to `Ludus` (console is a host)
- [x] Ludus rooms + upgrades
- [x] Household slave workforce
- [x] Night ops (spy / poison / sabotage) vs a rival camp
- [x] Rudis / discharge
- [x] Hosted munus with more than one pair (two pairs, one gate)
- [ ] Politics meters / patrons — later; not on the board

### Presentation
- [x] Main menu flow
- [x] Roster screen (console table)
- [x] Arrow / tab / Enter / Esc menus (numbers still work)
- [ ] Show planning screen / edictum
- [ ] Event log / narrative text
- [ ] Later: graphical / Godot / custom engine? (board Next; blocked on net8)

### Content & Narrative
- [ ] Starting scenario
- [ ] NPC / rival lanistas (one named rival exists for night ops)
- [ ] Imperial figures
- [ ] Random event table (night events exist)
- [ ] Multiple endings / legacies

### Art & Audio
- [x] Style guide (NES/SNES lock)
- [x] Core sprites (wave 0–2 4-dir idles in; wave 3 motion waits on a human silhouette squint — `assets/art/ASSET_MAP.md`)
- [ ] Sound design direction
- [ ] Music direction

### Quality & Release
- [x] Test harness / scenarios (`Amphiteater.Sim.Tests` + `--report`)
- [-] Balancing passes (playtest **holds** — no virtus / vigor / palmae retune this sitting)
- [ ] Documentation
- [ ] Steam / distribution prep (later)

### Meta / Agents & Workflow
- [x] Root `AGENTS.md` cycle pointer
- [x] Live board is SoT (`production/task_board.md`)
- [ ] Refine DEVELOPMENT_CYCLE.md based on use

**Status Legend:**  
- [ ] Not started  
- [-] Held / parked  
- [x] Done

This file is a map of tracks. Dispatch only from the board.
