# Pinning threshold: flat 3 or set by Nerve (2026-10-23)

RJ, after Gym 2: should a model be Pinned when its Suppression exceeds its Nerve, rather than at a flat 3? Four variants, 20,000 games per metric, seed 1, with the simulator's abstract firefight (`app/src/Imperator.Sim`, `--compare`; content variants set `core.suppression.pinned_by_nerve` and `pinned_at_least`).

| Variant | Nerve 0 | Nerve 1 | Nerve 2 | Nerve 3 | Nerve 4 |
|---|---|---|---|---|---|
| **Flat 3** (current) | 3 | 3 | 3 | 3 | 3 |
| **Above Nerve** (Nerve + 1) | 1 | 2 | 3 | 4 | 5 |
| **Nerve + 2** | 2 | 3 | 4 | 5 | never |
| **Above Nerve, at least 3** | 3 | 3 | 3 | 4 | 5 |

Nerve across the 74 troops that have one: 0 (3 troops, civilians and crew), 1 (13, including Task Force Boone's VATs and the generic troopers), 2 (35), 3 (22), 4 (1).

| Metric | Flat 3 | Above Nerve | Nerve+2 | Above Nerve, at least 3 |
|---|---|---|---|---|
| 4 Q2 vs 4 Q1 rifles: Q2 side wins | 73% | 71% | 73% | 73% |
| Pillar 1: 4 Q2 in cover vs 6 Q0 in the open, Q2 wins | 75% | 78% | 75% | 75% |
| Pinning: 4v4 Q1 (Nerve 1): activations spent Pinned | 9% | 16% | 9% | 9% |
| Pinning: 4v4 Q2 (Nerve 2): activations spent Pinned | 13% | 13% | 2% | 13% |
| Pinning: 4 Q2 vs 4 Q1: Q2 side wins | 73% | 71% | 73% | 73% |
| Real: clone squad vs UN reserve fireteam: clones win / draw | 45% / 4% | 45% / 4% | 46% / 5% | 45% / 4% |
| Real: clone squad vs UN reserve fireteam: clone / UN activations Pinned | 9% / 26% | 9% / 26% | 1% / 15% | 9% / 26% |
| Real: VAT fireteam vs asset security + captain: VAT win / draw | 95% / 2% | 95% / 2% | 95% / 2% | 95% / 2% |
| Real: VAT fireteam vs asset security + captain: VAT / security activations Pinned | 11% / 23% | 18% / 32% | 9% / 20% | 11% / 23% |
| Real: VAT fireteam vs UN reserve fireteam: VAT win / draw | 67% / 7% | 68% / 7% | 68% / 7% | 67% / 7% |
| Real: generic squad (Nerve 1) vs clone squad (Nerve 2): generic win / draw | 32% / 4% | 34% / 4% | 32% / 5% | 32% / 4% |
| Real: Harlow veterans (Nerve 3) vs clone squad: Harlow win / draw | 26% / 3% | 25% / 4% | 24% / 4% | 25% / 4% |
| Real: Ahsoka + clone squad vs UN reserve fireteam: Republic win | 51% / 3% | 51% / 3% | 52% / 3% | 51% / 3% |

## What it shows

- **Who wins barely moves.** No real matchup shifts by more than 2 points under any variant. Damage decides these fights; pinning changes what a model does while it is losing, not whether it loses.
- **How often models are Pinned moves a lot.** Above Nerve nearly doubles it for Nerve 1 troops (VATs 11% → 18% of activations; asset security 23% → 32%) and leaves Nerve 2 alone. Nerve + 2 all but removes it for professionals (clones 9% → 1%; 4v4 Nerve 2 from 13% to 2%).
- **The floored variant changes nothing measurable** here: only Nerve 3 and 4 troops gain, and in these fights they are rarely pushed to 3 anyway.

## Limits

The firefight has no board: no movement, objectives or reactions. At the table, Pinned matters most for tempo (a Pinned model cannot advance or react except to Find Cover), which these numbers do not capture. Read "activations Pinned" as how often the table feels suppressed, not as a balance figure.
