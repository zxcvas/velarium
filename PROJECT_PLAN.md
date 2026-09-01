# Amphiteater — Project Plan

**Project Name:** Amphiteater  
**Theme:** Roman Empire — Amphitheater games, gladiators, spectacle, politics, and survival.  
**Working Directory:** `amphiteater/`

## Vision (High Level)
You are a **lanista** in Capua: you own a *ludus* and a *familia gladiatoria*. You train, feed, and rent gladiators to magistrates and rich *editores*. If the school earns *fama*, you may later stage *munera* yourself. Infamia closes the curia; the harena does not.

Core loops:
- Recruit, train, and treat gladiators
- Pay the ludus (barley, oil, roof)
- Lease men (*locatio*) under sweat-vs-death terms
- Unlock and edit a munus of your own
- (M2) Expand the house; assign household slaves; night ops vs a rival camp
- Politics and imperial attention — later

## Goals
### Skeleton (M0) — done
- [x] Initialize git
- [x] Define folder structure
- [x] Create project plan + schedule + cycle docs
- [x] Set up organizer agent + subagent placeholders
- [x] Runnable exe
- [x] README + build/run scripts
- [x] Commit initial state

### Ludus slice (M1) — done
- [x] Console engine (Godot deferred)
- [x] Lanista role; hosting as progression
- [x] Playable day loop + combat + save/load
- [x] Rules in `Amphiteater.Sim.Ludus` + `--report` harness
- [ ] Human playtest / balance pass (use `--report` too)

### House and night (M2) — current
- [x] Rooms: unlock/upgrade cellae, kitchen, medicus, porta
- [x] Household slaves assigned to rooms
- [x] Night ops vs a named rival: spy / poison / sabotage
- [x] PixelLab wave 0–1 (palette, dirt, 4-dir murmillo, courtyard props)
- [ ] Human playtest of the M2 console loop (`production/playtest_m2.md`)

## High-Level Phases
1. **Foundation** — setup, console loop, data models (M0–M1)
2. **Core Simulation** — economy, roster, combat, time (M1 in)
3. **Presentation** — better text, edicta, events
4. **Depth** — politics, rival familiae, multiple show types, *rudis*
5. **Polish & Content** — art/audio only after the loop is loved
6. **Release Prep**

## Non-Goals (for now)
- No engine switch until the console slice has been played.
- No major art or audio in M1.

## Next
Play 12 days (`production/playtest_m2.md`). `--report 200` is off-band on cash, not unplayable; no retune until that verdict. Godot waits.

Headless: `.\dist\Amphiteater.exe --report 200`

See also: DEVELOPMENT_CYCLE.md, TASK_SCHEDULE.md, docs/design/01_expansion.md, docs/research/ludus_sources.md
