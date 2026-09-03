# Amphiteater — economy knobs

Tweak these during playtest. Code is the authority; this file is the sheet. All amounts are **denarii**.

Sources: `src/Amphiteater.Sim/Ludus.cs`, `Ludus.House.cs`, `Models.cs`, `Combat.cs`, `CareerSim.cs`.

## Start

| Knob | Value | Code |
|---|---|---|
| Starting purse | 620 | `Ludus.StartDenarii` |
| Starting fama | 3 | `Ludus.StartFama` |
| Date | Kalends of May, a.u.c. 782 | `StartYearAuc` / `StartMonth` / `StartDay` |
| Starter roster | 3 tiros (murmillo, thraex, retiarius) | `Ludus.Start` |

## Upkeep (dusk)

| Knob | Value | Code |
|---|---|---|
| Roof | 10 | `UpkeepRoof` |
| Per fighter | 6 | `UpkeepPerMouth` |
| Per household slave | 3 | `WorkerUpkeep` |
| Staffed kitchen | fighter upkeep `max(4, 6 − kitchen level)` | `Ludus.Upkeep` |
| Idle 3 fighters, no cook | **28** / day | `10 + 3×6` |

Empty purse at dusk: vigor −2, 40% a creditor takes the cheapest living man, purse clamped to 0, then **ludus closes** (`CheckEnd`).

## Locatio (rental)

Offer appears most mornings (always days 0–1; after that 68% if rival did not miss).

| Knob | Range | Code |
|---|---|---|
| *Pro sudore* (sweat) | 22–37 | `RefreshOffer` |
| *Pro occiso* (corpse) | 360–519 | `RefreshOffer` |
| Wrong *armatura* sweat | sweat − 8 (min 12) | `Sudore` |
| Wrong *armatura* corpse | corpse − 40 (min 80) | `Occisus` |
| Palma (rental win) | sweat + 25 + 2×palmae | `SettleBout` |
| Missio (rental, spared) | ⅔ sweat | `SettleBout` |
| Stans (rental) | sweat | `SettleBout` |

Playtest note: ~120/fight was likely a **hosted** palma (160–280) or stans (90–140), not locatio sweat (~22–65).

## Hosted munus

| Knob | Value | Code |
|---|---|---|
| Unlock | fama ≥ 16 **and** a living palma | `HostFamaNeed` |
| Gate cost | 220 | `HostCost` |
| Palma pay | 160–279 + 40 if *iugula* + 30 if spectacular | `SettleBout` |
| Stans pay | 90–139 | |
| Missio pay | 70–119 | |
| Own death (hosted) | 40–89 (no occisus compensation) | |

## Forum prices

Gladiator sale (`Gladiator.Value`):

`max(80, 180 + 22×virtus + 35×palmae + 8×fama + 4×vigorMax)`  
×0.6 if vulneratus, ×0.7 if aeger, +40 if *auctoratus*.

Household (`Worker.Value`): `max(28, 28 + 2×vigorMax)` → typically **~44–54**.

| Knob | Value | Code |
|---|---|---|
| Medicus treat | 15; staffed stall `max(8, 15 − 3 − level)` | `MedicusFee` / `TreatFee` |
| Treat heal | 6 + medicus level if staffed | `Treat` |
| Cell cap (start) | 8 | `CellCap` |
| Household cap | 6 | `HouseholdCap` |

## Room upgrades (`UpgradeCost`)

Max level 3. Kitchen/porta/medicus need a worker assigned (cellae/palus do not). Finishes at dusk.

| Room | 0→1 | 1→2 | 2→3 |
|---|---|---|---|
| Medicus | 90 (unbuilt) | 110 | 140 |
| Cellae | starts 1 | 120 | 160 |
| Kitchen | starts 1 | 90 | 110 |
| Porta | starts 1 | 80 | 100 |
| Palus | starts 1 | 75 | 100 |

Beds = `max(8, 6 + 2×cellae level)`.

## Combat (not denarii, but knobs)

| Knob | Value |
|---|---|
| Tiro virtus | 4–7 |
| Market veteran virtus | 7–11 |
| VigorMax (tiro) | 14–18 |
| Score | virtus + vigor/4 + 2d6 |
| Rounds | max 6 |
| Ville target | ~0.10 deaths / combatant / bout |

## Career AI (`--report` only)

| Knob | Value |
|---|---|
| Buy cook if purse | > 120 |
| Keep cushion | 50 |
| Never hosts; *mitte* own fallen | — |

## Market prices already in code

There is **no** food/ingredient market. The only “price” rolls are locatio *pro sudore* / *pro occiso* (`RefreshOffer`) and hosted gate gifts (`SettleBout`). Rumors in the forum do not move those numbers.

## Thermopolium (implemented)

Street window on the ludus wall. Staffed cook required. Settles at dusk **before** upkeep. Must not out-earn a locatio palma.

| Knob | Value | Code |
|---|---|---|
| Stall unlock | kitchen level ≥ 2 | `StallOpen` |
| Dish menu | kitchen level ≥ 3 | day menu item |
| Bowls cap | 4 × kitchen level | `StallBowlsPerLevel` |
| Base clients | 2 × kitchen level + fama/4 ± rumor demand | `StallBaseClientsPerLevel` |
| Lv2 anonymous bowl | cost 1, sale 3 | `StallAnonCost` / `StallAnonSale` |
| *Puls* | cost 1, sale 3 | `Content.Dishes` |
| Lentil stew | cost 2, sale 5 | |
| *Moretum* | cost 2, sale 6 | |
| *Posca* | cost 1, sale 3 | |
| Food rumor | 40% of mornings; demand −2..+3, price −1..+1 on one dish | `RefreshFoodRumor` |
| Unstaffed | no sales (and no diet bonus) | |

Empty kitchen = shuttered. If purse < cost×clients, sell only what grain you can buy.

## Other playtest wishes (not implemented)

- ASCII / arrow-key / tab UI (numbers stay until then)
- Spy that always returns usable intel (success still has a “dogs, barred porta” miss)
