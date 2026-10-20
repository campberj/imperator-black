# Simulator findings — rules v0.2

2026-10-20. Full numbers: [sim-v0.2-results.md](sim-v0.2-results.md). Simulator: `app/src/Imperator.Sim`.

## Summary

Rules v0.2 works for open-ground firefights, but **boarding actions stall**. The close-quarters weapons that dominate ship fights can barely take anyone out of the fight, and close combat never kills. The simulator also shows that the current wound table **fails the Pillar 1 test**: veterans in cover lose to a larger green squad in the open.

Two content changes, tested together, fix the boarding problem and pass Pillar 1. Neither changes a single rule; both are edits to `content/`. They are **proposed, not adopted**, pending RJ's call.

## What works

- **Quality matters as intended.** +1 Q is worth about +23 points of win rate in an even 4v4 (prototype: about +20).
- **Parity is fair.** Identical 4v4 teams split evenly.
- **Doctrine beats bravado.** In every variant, a flashbang or breach-and-flash entry beats simply opening the door and walking in, with roughly half the attacker casualties. The reaction and Overwatch rules make defenders dangerous at the door, which is the feel we want.

## Problems

### 1. Close-quarters weapons can't take anyone out

One hit from a carbine or SMG (2d6) on a Flak-armoured target takes it out of the fight **0%** of the time. A sidearm cannot wound an armoured target at all. To take someone out, it needs two separate Wounded results. Inside a ship, where carbines, SMGs and shotguns are the main weapons, fights grind on with no result.

**Room clearing, current rules:** four elite Void Assault troopers against three guards clear the room **0%** of the time within three rounds, and take out an average of 0.06–0.3 defenders.

### 2. Rows 1–3 absorb most hits

Damage totals 2–7 only add Suppression, and Down starts at 12. A 2d6 weapon reaches 12 only on a perfect roll, before armour. The prototype predicted rows 1–3 would be the first thing to tune; the simulator confirms it.

### 3. Pillar 1 fails

"A veteran squad in cover beats a larger green squad in the open most of the time." Under the current table, 4 Q2 in cover beat 6 Q0 in the open only **40%** of the time. Hits are so rarely decisive that numbers win.

### 4. Close combat never kills and runs long

A duel takes about 5.6 exchanges, which is a lot of rolling at the table, and the loser is never dead: melee damage dice can't reach rows 9–10. Losers are Down or overpowered (and so capturable), which feeds the campaign injury table rather than the body count. Decisive-blow bonuses (+1 row on winning by 3 or 4) barely change this.

### 5. Frag grenades are weak against armour

A frag in a confined room takes out an unarmoured target 17% of the time, and an armoured one almost never.

## Tested fixes

| Variant | Change |
|---|---|
| **A: re-banded wound table** | Suppression rows compressed to totals 2–5; Wounded at 6–9, Down at 10–11, Bleeding 12–13, Critical 14–15, Dead 16+. File: [variants/a-rebanded.json](variants/a-rebanded.json) |
| **B: bigger close-quarters weapons** | Sidearm 2d6, SMG 2d8, carbine 2d8, shotgun 2d12. Rifle, LMG and DMR unchanged. File: [variants/b-cqb-weapons.json](variants/b-cqb-weapons.json) |
| **A + B** | Both. File: [variants/a-plus-b.json](variants/a-plus-b.json) |

Also tried: a flat close-quarters bonus of +1 or +2 wound rows for shots inside a compartment. +2 rows clears a room 20–40% of the time but leaves the open-ground game and Pillar 1 untouched, and it adds a rule to remember, so it is not recommended (section 7a of the results).

| Metric (10,000 runs each) | Current (v0.2) | A | B | A + B |
|---|---|---|---|---|
| Rifle vs Flak, one hit: out | 10% | 25% | 10% | 25% |
| Carbine vs Flak, one hit: out | 0% | 4% | 10% | 25% |
| Carbine vs Void plate (2), one hit: out | — | 0% | 2% | 10% |
| Shotgun vs Flak, one hit: out | 29% | 46% | 47% | 63% |
| Sidearm vs no armour, one hit: out | — | 0% | 4% | 18% |
| Room clear, dynamic entry: cleared by R3 | 0% | 5% | 8% | 23% |
| Room clear, dynamic entry: attackers out / dead | 0.30 / 0.11 | 0.84 / 0.23 | 1.04 / 0.30 | 1.85 / 0.37 |
| Room clear, flashbang first: cleared by R3 | 0% | 9% | 15% | 41% |
| Room clear, flashbang first: attackers out / dead | 0.15 / 0.06 | 0.38 / 0.13 | 0.49 / 0.17 | 0.86 / 0.23 |
| Room clear, breach + flash: cleared by R3 | 0% | 6% | 11% | 37% |
| 4 Q1 vs 4 Q1 rifles: draws | 21% | 2% | 21% | 2% |
| 4 Q2 vs 4 Q1 rifles: Q2 side wins | 63% | 73% | 63% | 73% |
| 4 Q1 vs 5 Q1 rifles: 4-side wins | 8% | 20% | 8% | 20% |
| Firefight 4v4 Q1: models dead per game | 0.21 | 0.50 | 0.21 | 0.50 |
| Pillar 1: 4 Q2 in cover vs 6 Q0 in the open, Q2 wins | 40% | 74% | 40% | 74% |
| Close combat Q1 vs Q1: avg exchanges | 5.6 | 4.5 | 5.6 | 4.5 |
| Close combat Q1 vs Q1: loser dead | — | — | — | — |

## Recommendation

**Adopt A + B as rules v0.3** for the first table playtests.

- **A** fixes the open-ground game: Pillar 1 passes (74%), draws almost vanish, and Q matters more. Deaths stay modest (0.5 per 4v4 game), so most casualties still reach the post-battle injury table.
- **B** gives close-quarters weapons a real job aboard ship without touching long arms. Shotguns become the door-kicker's weapon, which suits pillar 4.
- Together, entry method matters a great deal: a flashbang entry clears the room about twice as often as walking in, at under half the cost. That is the doctrinal feel we're after.

Rooms are still hard: four elite troopers clear three guards in three rounds 37–41% of the time. That suits a game where a boarding action is a series of rooms and the player decides how much risk to take at each door. The simulator's attackers are also clumsy (no aiming, no positioning, no supporting fire into the room), so real players should do better.

## Open questions for RJ

1. Adopt A + B for v0.3?
2. Close combat: is it right that close combat almost never kills outright (losers are Down or captured), or should melee be deadlier? Either way it is slow at about 5 exchanges. One option is that after the third exchange, the winner of each exchange adds +1 row.
3. Frag grenades: with A + B in place, should frags step up to 2d8?

## Simulator limits

- Firefights are abstract: no board, cover as a state, range sampled per shot, both sides use the same simple policy. Reactions are modelled only in the room-clearing simulation.
- Room clearing has no positions inside the room; every defender sees every attacker who has entered; attackers do not react or aim.
- The Patrol Contact scenario comes out at about 29% / 47% (fireteam / patrol) under the current rules, lower than the prototype's estimate of 43–52%. The policies differ, so treat table playtests as the authority.
