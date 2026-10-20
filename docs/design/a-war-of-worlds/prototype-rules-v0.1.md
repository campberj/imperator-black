# Prototype Rules v0.1: Fireteam Test

2026-10-15 · RJ

> **Imported from A War of Worlds**, the predecessor design to Imperator Black. This is the historical record of the first prototype. Where it conflicts with [decisions.md](../decisions.md), the decision log wins. Known errata are marked **[Errata]**.

## What you need

This prototype tests whether the settled core plays well: 2d6 hit roll thrown with weapon damage dice, a ten-row wound table, two actions per model, alternating single-model activation, and no weapon ranges. One fireteam of four fights a five-model enemy force run by the solo rules. Scale is 1 inch = 2 metres.

- **Models:** 9 (4 yours, 5 enemy), one per soldier, on any 25 to 32 mm bases.
- **Board:** 36 x 36 inches, with about 10 terrain pieces: 4 light cover (crates, low walls), 3 heavy cover (wall sections, bunker), 3 micro-terrain (rubble, craters).
- **Dice:** two d6 in one colour (hit dice); a pair each of d4, d6, d8, d10 and d12 in a second colour (damage dice).
- **Tokens:** Suppression markers (about 20), Wounded markers, Overwatch markers, Down markers, one objective token.
- **Also:** a ruler and the roster sheet below.

## Core rules

### Model stats

Every model has **Quality (Q)** from 0 to 3 (Green 0, Regular 1, Veteran 2, Elite 3), **Armour** from 0 to 3, a **weapon**, and **Move 6 inches** (5 in Armour 3).

### The round

1. **Initiative.** Each side rolls 2d6 and adds its leader's Q. The winner chooses who activates first.
2. **Activations.** Sides alternate, activating one model at a time. Each model activates once per round. When one side has no models left to activate, the other finishes its remaining models.
3. **End of round.** Each model that was not hit this round and is in cover removes 1 Suppression. Dying models check as the wound table says.

### Actions

A model takes **two actions** per activation, in any order, and may repeat an action unless noted.

| Action | Effect |
|---|---|
| Move | Up to Move inches. Climbing or vaulting a low obstacle costs 2 extra inches. |
| Shoot | Fire the weapon at a visible target (hit roll below). Shooting after a Move this activation is -1 unless the weapon is Handy. |
| Aim | +1 to the next Shoot this activation. |
| Suppressive Fire | Auto weapons only. Pick a visible target and make a hit roll. On a hit the target gains 1 Suppression, or 2 if you beat the target number by 3 or more. No damage dice are used. |
| Overwatch | Place a marker; must be the last action. When an enemy model Moves or Shoots in your line of sight before your next activation, you may fire one Shoot at it at -1, then remove the marker. Two or more Suppression cancels it. |
| Find Cover | A model in the open within 1 inch of micro-terrain rolls 2d6 + Q (minus Suppression): 7+ gains Light cover until it moves, 10+ gains Heavy cover. |
| Rally | Remove 1 Suppression, or 2 if your leader is within 6 inches in line of sight. |
| Medic | Touch a Down model. Roll 2d6 + Q (+2 if Medic role): 8+ stops bleeding (row 7); 10+ stabilises a Critical (row 8). |
| Interact | Use an objective token within 1 inch. |

### Shooting

1. **Throw together** the 2d6 hit dice and the weapon's damage dice. If the shot misses, ignore the damage dice.
2. **Hit roll:** 2d6 + Q + modifiers must reach **8**. A natural 2 always misses. A natural double 6 is a critical hit: add 1 row to the damage result.
3. **Damage:** add the damage dice and find the row on the wound table. Subtract the target's Armour (reduced by the weapon's AP) from the row. At row 0 or below there is no effect.

| Hit modifier | Value |
|---|---|
| Aim | +1 |
| Target in Light cover | -1 |
| Target in Heavy cover | -2 |
| Each Suppression on shooter | -1 |
| Shooter Wounded | -1 |
| Moved earlier this activation | -1 |
| Overwatch shot | -1 |
| Beyond the weapon's short range | per weapon table |

**Cover:** a target has Light cover if at least a third of its body is hidden behind an obstacle within 1 inch of it, and Heavy cover if at least half is hidden behind solid material such as concrete or a vehicle. Use true line of sight from the shooter's eye.

### Suppression and morale

Each Suppression marker is -1 to all of that model's rolls (maximum 5). At **3 or more** the model is **Pinned**: it may only Rally, Find Cover, Move to cover, or Shoot from cover.

**Casualty shock:** when a friendly model within 6 inches goes Down or Dead, the model gains 1 Suppression (2 if the casualty was the leader).

**Breaking:** when half of a fireteam (rounded up) is Down or Dead, each remaining model rolls 2d6 + Q - Suppression at the start of its activation (+1 if its leader is within 6 inches). On 8+ it fights on; on a fail it spends both actions Moving away from the enemy.

## Weapons and armour

Damage dice are thrown with the hit dice. **Short range** is the distance in inches; beyond it the shot takes -1, and beyond double it -2. Weapons have no maximum range.

| Weapon | Damage | Short range | AP | Keywords |
|---|---|---|---|---|
| Sidearm | 2d4 | 8 | 0 | Handy |
| SMG | 2d6 | 12 | 0 | Handy, Auto |
| Carbine | 2d6 | 18 | 0 | Handy |
| Shotgun | 2d10 | 10 | 0 | Handy; beyond short range -2, beyond double -3 |
| Rifle | 2d8 | 30 | 0 | none |
| LMG | 2d8 | 30 | 0 | Auto; a hit also adds 1 Suppression |
| DMR | 2d10 | 48 | 0 | none |
| AT launcher | 2d12 | 36 | 1 | one shot per round; -1 more when firing after a Move |

**Keywords:** *Handy* weapons ignore the Move-then-Shoot penalty. *Auto* weapons may use Suppressive Fire.

| Armour | Value | Notes |
|---|---|---|
| None | 0 | |
| Flak | 1 | Standard issue |
| Composite plate | 2 | |
| Heavy plate | 3 | Move 5 inches |

Armour steps the wound-table row down by its value, less the weapon's AP. Armour 3 against a rifle is nearly immune, so heavy targets need AT launchers or criticals.

> **[Errata]** The original text said a rifle tops out at row 5 against Armour 3. That is wrong: a rifle (2d8, max 16) can reach row 8 before armour, though it usually lands on rows 3–5.

## Wound table

Add the damage dice, find the row, then step down by armour (less AP). Add 1 row first if the hit was a critical (double 6).

| Row | Damage total | Result |
|---|---|---|
| 1 | 2-3 | Shaken: +1 Suppression |
| 2 | 4-5 | Rattled: +2 Suppression |
| 3 | 6-7 | Grazed: +1 Suppression |
| 4 | 8-9 | Wounded: place a marker (-1 to all rolls). Already Wounded: Down |
| 5 | 10-11 | Wounded and +1 Suppression. Already Wounded: Down |
| 6 | 12-13 | Down: removed from play; recovers with Medic |
| 7 | 14-15 | Down and bleeding: dies at the end of the third round unless a Medic action gets 8+ |
| 8 | 16-17 | Critical: dies at the end of the next round unless a Medic action gets 10+ |
| 9 | 18-20 | Dead |
| 10 | 21-24 | Dead; adjacent friends gain 1 Suppression |

Rows above 10 count as 10. Rows 0 or lower do nothing. A Down model counts toward the breaking test.

> **[Errata]** Row 6 "Down" has no clear recovery rule. Until fixed, treat a row 6 Down model as out of the game.

## Solo opponent

The enemy force follows these rules. Whenever the rules leave a choice, pick the option that is nearest to the enemy's current objective, then the lowest-numbered model.

### Choosing who activates

When it is the enemy's turn, activate the first eligible model from this list:

1. A model with 3 or more Suppression (it Rallies).
2. A model in the open with a target in sight.
3. A model with a target in sight in cover.
4. A model with no target in sight, nearest the objective first.

### What each model does

Work down the list for each action and take the first that applies:

| Role | Priority |
|---|---|
| All | If Pinned: Rally, then Find Cover or Move to nearest cover. If in the open and cover is within one Move: Move to it. |
| Gunner (LMG) | Suppressive Fire at the nearest player model if two or more are in sight; otherwise Shoot at the nearest. |
| Marksman | Aim, then Shoot at the player model that is not in cover, else the leader. |
| Rifleman, SMG | If target is within short range: Shoot. Otherwise Move to cover that is nearer the target, then Shoot. |
| Leader | Rally the nearest Suppressed ally, then act as a Rifleman. |

**Fire and manoeuvre:** the enemy never moves every model in the same round. At least one model that has a target in sight stays put and fires while the others advance.

**Target choice:** the nearest visible player model not in cover; if all are in cover, the one with the fewest cover points; ties by dice.

**Withdrawal:** when the enemy breaks (half Down or Dead), surviving models Move away from the nearest player model and leave the board edge they entered from. The game ends when all have left or are Down.

**Optional:** hold two enemy models off table. At the start of round 3, roll 2d6 for each: on 7+ it enters from a random board edge.

## Test scenario: Patrol Contact

Your fireteam is sweeping a ruined village when it meets an enemy patrol at a crossroads.

**Setup:** 36 x 36 inch board. Place the objective token (a radio relay) in the centre. Scatter the ten terrain pieces so that no line of sight across the board is longer than 18 inches. You enter from the south edge within 6 inches of a corner; the enemy enters from the north edge, 6 inches in.

**Goal:** at the end of round 8, hold the token (a Down model cannot hold it, and an enemy model within 3 inches contests it). Or force the enemy to break before then. The enemy wins by the same conditions, or if your team breaks.

| Your fireteam | Q | Armour | Weapon |
|---|---|---|---|
| Team leader | 2 | 2 | Rifle |
| Gunner | 1 | 1 | LMG |
| Medic | 1 | 1 | Rifle (+2 on Medic rolls) |
| Marksman | 1 | 1 | DMR |

| Enemy patrol | Q | Armour | Weapon |
|---|---|---|---|
| Leader | 1 | 1 | Rifle |
| Gunner | 1 | 1 | LMG |
| Rifleman | 1 | 1 | Rifle |
| Rifleman | 1 | 1 | Rifle |
| Trooper | 1 | 1 | SMG |

The enemy has one more model to offset your better kit and leader. Expect a fair fight with the solo rules. If it plays too easy or hard, adjust by changing one enemy model's Q by 1.

## Playtest log and questions

Play the scenario three times. After each game, record: winner and round the game ended; models Down / Dead / Critical on each side; real time taken; rules you had to look up or invented.

**Questions to answer:**

1. Did the 2d6 hit roll feel too swingy or too safe? Is 8+ the right target?
2. Did throwing hit and damage dice together speed play, or was it fiddly?
3. Did armour stepping rows down matter? Did anyone feel invulnerable?
4. Was the wound table too lethal, or did models rarely die?
5. Did suppression and Pinned create pauses, or just slow the game?
6. Were two actions per model enough to bound between cover?
7. Did the enemy rules feel like an opponent? Where did they act stupidly?
8. Without ranges, did long shots ever feel wrong? Was the short-range penalty noticed at all?
9. Did the scenario last 8 rounds, and was it close?

## Simulator results and known gaps

A Python Monte Carlo (thousands of games per case) with a simple abstract policy tuned these numbers. It models cover as a random value and ignores positioning, so treat it as a rough check only.

| Finding | Result |
|---|---|
| Quality | Each +1 Q is worth about 20 percentage points of win rate |
| Numbers | Four against five wins about 17% of games, against 79% for the five |
| Scenario | Your fireteam with better kit against five enemies: about 43% to 52% (rest are draws or mutual breaks) |
| Solo AI | Weaker than the abstract policy, so expect your side to win about 50 to 60% |
| Armour 3 | Needs AP or criticals; only 2d12 AP launchers hurt it reliably |
| Deaths | Rare from rifles, so rows 7 and 8 and the Medic action supply the danger |
| LMG | Underperformed until hits added Suppression; now near parity |

**Known gaps, not yet tested at the table:** vehicles, grenades and blast, melee, morale for larger forces, leaders other than the team leader, campaign advancement, equipment costs, and Overwatch and Find Cover (not simulated). Expect the wound table's rows 1 to 3 (suppression only) to be the first thing you tune.
