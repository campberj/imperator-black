# 4. Shooting

## The shot

1. **Choose a target** the shooter can see. A model cannot shoot at a model in close combat.
2. **Throw together** the 2d6 hit dice and the weapon's damage dice. If the shot misses, ignore the damage dice.
3. **Hit roll:** 2d6 + **Shoot** + modifiers must reach **8**.
   - A natural 2 always misses.
   - A natural double 6 is a **critical hit**: add 1 row to the wound result.
4. **Wound:** add the damage dice and find the row on the wound table. If the hit was critical, add 1 row. Then step the row down by the target's Armour, less the weapon's AP (the step can't go below 0). A final row above 10 counts as 10; at row 0 or below there is no effect.

<!-- table:core.hit_modifiers -->
| Hit modifier | Value |
|---|---|
| Aim | +1 |
| Target in Light cover | -1 |
| Target in Heavy cover | -2 |
| Each Suppression on shooter | -1 |
| Shooter has any Wounded markers | -1 |
| Moved earlier this activation (not Handy) | -1 |
| Reaction or Overwatch shot | -1 |
| Unbraced in zero-g (not Void-trained) | -1 |
| Target in darkness beyond 6" (no low-light gear) | -2 |
<!-- /table -->

## Range

Weapons have **no maximum range**. Each has a **short range**: beyond it the shot takes −1, and beyond double it −2. Some weapons have harsher penalties, listed in the table.

<!-- table:weapons.ranged -->
| Weapon | Damage | Short range | Beyond / double | AP | Keywords |
|---|---|---|---|---|---|
| Sidearm | 2d6 | 8" | -1 / -2 | 0 | Handy |
| SMG | 2d8 | 12" | -1 / -2 | 0 | Handy, Auto 2 |
| Carbine | 2d8 | 18" | -1 / -2 | 0 | Handy |
| Shotgun | 2d12 | 10" | -2 / -3 | 0 | Handy |
| Rifle | 2d8 | 30" | -1 / -2 | 0 | — |
| Heavy rifle | 2d8 | 30" | -1 / -2 | 1 | — |
| LMG | 2d8 | 30" | -1 / -2 | 0 | Auto 3, Suppressing |
| Compact LMG | 2d8 | 18" | -1 / -2 | 0 | Handy, Auto 3, Suppressing |
| DMR | 2d10 | 48" | -1 / -2 | 0 | — |
| AT launcher | 2d12 | 36" | -1 / -2 | 1 | One per round, Cumbersome |
<!-- /table -->

<!-- table:keywords.weapon -->
| Keyword | Effect |
|---|---|
| *Handy* | Ignores the moved-this-activation penalty. |
| *Auto* | Auto N: may use Suppressive Fire. Shoot or Suppressive Fire may engage up to N models within 3" of the first target, one roll each, -1 after the first. |
| *Suppressing* | A hit also adds 1 Suppression. |
| *One per round* | May fire once per round, including reactions. |
| *Cumbersome* | An extra -1 when firing after a Move; cannot fire as a reaction. |
| *Blast* | Affects every model within the blast radius with line of sight to the point of impact. |
| *Confined* | +1 wound row to models in an enclosed compartment of 6" x 6" or smaller. |
| *Breach* | Can destroy doors, hatches and thin bulkheads. |
| *Silenced* | Shots do not count as noise for the alert track. |
<!-- /table -->

## Cover

A target has **Light cover** if at least a third of its body is hidden behind an obstacle within 1" of it, or if another model is between it and the shooter. It has **Heavy cover** if at least half its body is hidden behind solid material such as a bulkhead, column or heavy machinery.

**Find Cover:** a model in the open within 1" of micro-terrain (debris, a fallen crate, a body) may take the Find Cover action: test 2d6 + Nerve. On 7+ it gains Light cover, on 10+ Heavy cover, until it moves.

## Aim

Aim gives +1 to the next Shoot in the same activation.

## Automatic fire

A weapon with **Auto N** (for example *Auto 3*) can engage several models with one action.

- **Shoot:** choose the first target as normal. Then choose up to N − 1 more **different** models that are within **3"** of the first target and visible to the shooter. Roll a separate hit roll, with damage dice, for each. Every target after the first takes **−1** to hit.
- **Suppressive Fire** spreads in the same way (see below).
- **Reactions:** a reaction Shoot with an *Auto* weapon may spread too; the triggering model is the first target, and the usual −1 for a reaction applies to every roll.
- A model can be engaged only once by the same burst.

## Suppressive Fire

*Auto* weapons only. Pick a visible target and make a hit roll, without damage dice. On a hit the target gains 1 Suppression, or 2 if the roll beats 8 by 3 or more. An *Auto N* weapon may spread Suppressive Fire across up to N models within 3" of the first target, rolling for each, with −1 for every target after the first.

## Wound table

<!-- table:core.wound_table -->
| Row | Damage total | Result |
|---|---|---|
| 1 | 2–3 | Shaken: +1 Suppression |
| 2 | 4–4 | Rattled: +2 Suppression |
| 3 | 5–5 | Grazed: +1 Suppression |
| 4 | 6–7 | Wounded: add a Wounded marker. More markers than Wounds: Down |
| 5 | 8–9 | Wounded and +1 Suppression. More markers than Wounds: Down |
| 6 | 10–11 | Down: may recover (end-of-round test 8+, or Medic) |
| 7 | 12–13 | Down and bleeding: dies at end of third round unless Medic 8+ |
| 8 | 14–15 | Critical: dies at end of next round unless Medic 10+ |
| 9 | 16–18 | Dead |
| 10 | 19+ | Dead; friends within 1" gain 1 Suppression |
<!-- /table -->

## Armour

<!-- table:armour.armour -->
| Armour | Value | Keywords |
|---|---|---|
| None | 0 | — |
| Flak | 1 | — |
| Void suit | 1 | Sealed |
| Composite plate | 2 | — |
| Void plate | 2 | Sealed |
| Heavy plate | 3 | Sealed, Heavy |
<!-- /table -->

Armour steps the row down by its value, less AP. A rifle or carbine (2d8) can reach row 9 before armour, but it usually lands on rows 4–6; against Armour 3 that becomes suppression. Heavy targets need AP, criticals, shotguns at close range, close combat, or explosives.
