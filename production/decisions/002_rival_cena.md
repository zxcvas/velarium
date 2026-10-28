# 002 — Rival *cena* (v1 lock)

**Date:** 2026-10-24  
**Ask:** banquet for competitors, with table events or a chance to poison guests.  
**Quiz:** all A (Victor).

## Decision
The lanista may invite the **named rival lanista** to a *cena* at the *culina*. Hospitality first; the cup is optional. Console only.

Locked picks:

- Guests are rival lanistae only. Infamia stays a locked door.
- Night-ops wine-at-the-gate stays. The banquet does not replace it and is not a `NightOrder`.
- The cup sickens their next champion (`NextFoePoisoned`) and may cost them the morning editor. The guest does not die at table.
- One named rival. No Campanian circuit this sprint.
- Gate is a staffed kitchen at any level, plus coin and cooldown.
- A pending feast forces night rest. No sneak the same evening.

## Why
- Fama already buys contracts, not dinner with an *augustalis* (night-event line; expansion infamia lock).
- Gate poison is a shipped sneak. Folding it now would retune a loved night verb.
- A guest corpse is Hollywood and collapses the one-rival loop. Death stays on the sand.
- The cook is the house that can feed a guest. Street *thermopolium* is a different register.
- You cannot listen at their porta while they sit in your mess.

Rejected: editors or magistrates at table; delete `NightOrder.Poison`; banquet as a sneak order; killing the guest; 2–3 houses this sprint.

## Consequences
- Spec: `docs/design/04_banquets.md`. Knobs: `production/economy.md` (Rival *cena*).
- Next claims: `banquet-model-save` → `banquet-host` → `banquet-events` → `banquet-poison`.
- Career AI (`--report`) does not host a *cena*.
- Courtyard-morning (draft PR #27) stays parked.
