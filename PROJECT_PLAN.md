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
- Politics, rival lanistae, and imperial attention — later

## Goals
### Skeleton (M0) — done
- [x] Initialize git
- [x] Define folder structure
- [x] Create project plan + schedule + cycle docs
- [x] Set up organizer agent + subagent placeholders
- [x] Runnable exe
- [x] README + build/run scripts
- [x] Commit initial state

### Ludus slice (M1) — current
- [x] Console engine (Godot deferred)
- [x] Lanista role; hosting as progression
- [x] Playable day loop + combat + save/load
- [ ] Human playtest / balance pass

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
Play the slice. Then pick one: balance, a fuller hosted munus, or *rudis* discharge.

See also: DEVELOPMENT_CYCLE.md, TASK_SCHEDULE.md, docs/design/01_expansion.md, docs/research/ludus_sources.md
