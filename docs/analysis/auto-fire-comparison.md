# Automatic fire (Auto N) comparison

2026-10-20. Generated with `Imperator.Sim --compare`, 10,000 runs per cell. Variants: v0.4 single-target fire; Auto N with -1 per extra target (adopted for playtest); Auto N with no penalty; Auto N -1 with LMG Auto 4. The beaten zone is modelled as a 60% chance (in a compartment) or 35% (open ground) that each other enemy is within 3" of the first target.

| Metric | v0.4 (one target) | Auto N, -1 extra | Auto N, no penalty | Auto N -1, LMG Auto 4 |
|---|---|---|---|---|
| Rifle vs Flak, one hit: out | 25% | 25% | 25% | 25% |
| Carbine vs Flak, one hit: out | 25% | 25% | 25% | 25% |
| Carbine vs Void plate (2), one hit: out | 10% | 10% | 10% | 10% |
| Shotgun vs Flak, one hit: out | 63% | 63% | 63% | 63% |
| Sidearm vs no armour, one hit: out | 18% | 18% | 18% | 18% |
| Room clear, dynamic entry: cleared by R3 | 22% | 22% | 22% | 22% |
| Room clear, dynamic entry: attackers out / dead | 1.89 / 0.38 | 1.89 / 0.38 | 1.89 / 0.38 | 1.89 / 0.38 |
| Room clear, flashbang first: cleared by R3 | 41% | 41% | 41% | 41% |
| Room clear, flashbang first: attackers out / dead | 0.88 / 0.23 | 0.88 / 0.23 | 0.88 / 0.23 | 0.88 / 0.23 |
| Room clear, breach + flash: cleared by R3 | 37% | 37% | 37% | 37% |
| 4 Q1 vs 4 Q1 rifles: draws | 2% | 2% | 2% | 2% |
| 4 Q2 vs 4 Q1 rifles: Q2 side wins | 72% | 72% | 72% | 72% |
| 4 Q1 vs 5 Q1 rifles: 4-side wins | 19% | 19% | 19% | 19% |
| Firefight 4v4 Q1: models dead per game | 0.49 | 0.49 | 0.49 | 0.49 |
| Pillar 1: 4 Q2 in cover vs 6 Q0 in the open, Q2 wins | 75% | 75% | 75% | 75% |
| Real VAT team vs asset security on Overwatch, flash: cleared by R3 | 34% | 38% | 40% | 39% |
| Real VAT team vs asset security on Overwatch, flash: VAT out / dead | 1.07 / 0.37 | 1.09 / 0.35 | 1.11 / 0.34 | 1.11 / 0.35 |
| Real VAT team vs asset security on Overwatch, flash: defenders out | 1.34 | 1.44 | 1.49 | 1.46 |
| Real VAT team vs unready asset security, flash: cleared by R3 | 47% | 53% | 52% | 52% |
| Patrol Contact (both sides have LMG + SMG): fireteam / patrol wins | 42% / 51% | 41% / 53% | 42% / 53% | 43% / 52% |
| 3 rifles + LMG vs 4 rifles (all Q1): LMG side wins | 48% | 54% | 57% | 53% |
| Close combat Q1 vs Q1: avg exchanges | 2.8 | 2.8 | 2.8 | 2.8 |
| Close combat Q1 vs Q1: loser dead | 0% | 0% | 0% | 0% |

Exact, one Shoot action by a Q1 (Shoot 1) gunner against Flak in Light cover: a single target is taken out about 10% of the time; a 3-model burst with -1 on extra targets takes someone out about 24% of the time (2.3x), or 31% with no penalty (3x).
