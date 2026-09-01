# Amphiteater — Task Schedule

This document will evolve. For the skeleton we only list major tracks.

## Tracks
- **Foundation & Tooling**
- **Game Systems** (roster, economy, combat, time)
- **Presentation** (UI, text, menus, events)
- **Content & Narrative**
- **Art & Audio**
- **Quality & Release**
- **Meta / Agents & Workflow**

## Current High-Level Backlog (all pending post-skeleton)

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
- [ ] Politics meters / patrons
- [ ] Rudis / discharge
- [ ] Hosted munus with more than one pair

### Presentation
- [-] Main menu flow
- [x] Roster screen (console table)
- [ ] Show planning screen
- [ ] Event log / narrative text
- [ ] Later: graphical / Godot / custom engine?

### Content & Narrative
- [ ] Starting scenario
- [ ] NPC / rival lanistas
- [ ] Imperial figures
- [ ] Random event table
- [ ] Multiple endings / legacies

### Art & Audio
- [x] Style guide (NES/SNES lock)
- [-] Core sprites (wave 0–1 in; wave 2 bodies next)
- [ ] Sound design direction
- [ ] Music direction

### Quality & Release
- [x] Test harness / scenarios (`Amphiteater.Sim.Tests` + `--report`)
- [-] Balancing passes (Ville ~0.12 on `--report 200`; cash drain noted; wait on human playtest)
- [ ] Documentation
- [ ] Steam / distribution prep (later)

### Meta / Agents & Workflow
- [ ] Exercise organizer + subagents on first real task
- [ ] Refine DEVELOPMENT_CYCLE.md based on use
- [ ] Populate production/ with actual milestones

**Status Legend:**  
- [ ] Not started  
- [-] In progress (by agents)  
- [x] Done

This file will be updated by the organizer agent regularly.
