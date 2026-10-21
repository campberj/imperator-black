# Hit target: 8+ against 7+

From the Gym 1 replay (2026-10-21), RJ: "8+ as the target hit number may be one too high; a lot of shots start missing as soon as cover, injury, or any suppression matter. That said, I could see the argument that it is realistic."

Held as an observation (decisions 3.19 and 3.20): rules change when a finding repeats, not after one game. These are the numbers for when it comes up again.

## Hit chance by situation

Exact odds: 2d6 + Shoot + modifiers; a natural 2 always misses and a double 6 always hits.

| Shooter and situation | Net modifier | 8+ (now) | 7+ | 9+ |
|---|---|---|---|---|
| Shoot 1, clean | +1 | 58% | 72% | 42% |
| Shoot 1, Light cover | +0 | 42% | 58% | 28% |
| Shoot 1, Heavy cover | −1 | 28% | 42% | 17% |
| Shoot 1, Light cover + 1 Suppression | −1 | 28% | 42% | 17% |
| Shoot 1, reaction into Light cover | −1 | 28% | 42% | 17% |
| Shoot 1, Heavy cover + Wounded | −2 | 17% | 28% | 8% |
| Shoot 1, Heavy + Wounded + 1 Suppression | −3 | 8% | 17% | 3% |
| Shoot 2 (Veteran), Light cover | +1 | 58% | 72% | 42% |

On 2d6 near the middle, every −1 costs 14 to 16 points. At 8+ a Regular falls from 58% to 28% with just two modifiers, which matches what RJ saw at the table: the first bit of cover or Suppression makes a big difference.

## Simulator: 8+ against 7+

`dotnet run -c Release --project app/src/Imperator.Sim -- --compare "Hit 8+ (now)=…" "Hit 7+=…" --n 20000`, seed fixed. The 7+ variant changes only `core.hit_target` (which also sets the Throw and Suppressive Fire targets).

| Metric | Hit 8+ (now) | Hit 7+ |
|---|---|---|
| Room clear, dynamic entry: cleared by R3 | 22% | 22% |
| Room clear, dynamic entry: attackers out / dead | 1.90 / 0.38 | 2.50 / 0.36 |
| Room clear, flashbang first: cleared by R3 | 41% | 54% |
| Room clear, breach + flash: cleared by R3 | 37% | 50% |
| 4 Q1 vs 4 Q1 rifles: draws | 2% | 0% |
| 4 Q2 vs 4 Q1 rifles: Q2 side wins | 73% | 71% |
| 4 Q1 vs 5 Q1 rifles: 4-side wins | 19% | 21% |
| Firefight 4v4 Q1: models dead per game | 0.49 | 0.38 |
| Pillar 1: 4 Q2 in cover vs 6 Q0 in the open, Q2 wins | 75% | 73% |
| Real VAT team vs asset security on Overwatch, flash: cleared by R3 | 38% | 57% |
| Real VAT team vs asset security on Overwatch, flash: VAT out / dead | 1.09 / 0.35 | 1.18 / 0.31 |
| Real VAT team vs asset security on Overwatch, flash: defenders out | 1.45 | 1.92 |
| Real VAT team vs unready asset security, flash: cleared by R3 | 53% | 70% |
| Patrol Contact: fireteam / patrol wins | 42% / 53% | 42% / 57% |
| 3 rifles + LMG vs 4 rifles: LMG side wins | 53% | 56% |

## Reading

- **Tempo, not balance.** 7+ makes fights resolve faster (rooms cleared by round 3 rise by 13 to 19 points; stalemates disappear) while the balance between sides barely moves (quality edge 73% → 71%, Pillar 1 75% → 73%).
- **Not much bloodier.** Deaths per firefight fall slightly at 7+, because suppression and breaking end fights sooner. Lethality per hit is unchanged; the wound table does that work.
- **Breaching gets easier.** Flash-and-clear against Overwatch rises from 38% to 57%. Dynamic entry without a flash stays at 22%, so the flashbang matters more at 7+, not less.
- **Realism.** 8+ makes cover and Suppression decisive, which suits the boarding-action pillar; the cost is more turns of missing.

## Options when this comes back

1. **Keep 8+.** Cover and Suppression stay decisive; accept slower fights.
2. **7+.** Faster fights, same balance; a one-line content change.
3. **Keep 8+, soften stacking:** for example cap the total negative modifier at −3, or have Wounded and Suppression not stack. Keeps clean shots as they are and helps only the badly degraded ones.
4. **RJ's range bands (3.19):** +1 at short range is 7+ at short range only; check together with option 2.
