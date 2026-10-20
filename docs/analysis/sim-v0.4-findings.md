# Simulator findings — rules v0.4 (granular profiles)

2026-10-20. Full numbers: [sim-v0.4-results.md](sim-v0.4-results.md), sections 5 and 8.

## Summary

Splitting Quality into attributes keeps the core balance intact and shows real differences between troop types. It also confirms that **room clearing against prepared defenders is hard for a starting force**, which puts the burden on mission design (Phase 5): contacts, alert level and threat tier must decide how often the player meets defenders on Overwatch.

## Core balance unchanged

Abstract matchups (every attribute set to one rating) match v0.3:

| Metric | v0.3 | v0.4 |
|---|---|---|
| Pillar 1: 4 rated 2 in cover vs 6 rated 0 in the open | 75% | 75% |
| Even 4v4: draws | 2% | 2% |
| +1 rating, 4v4 | 72% | 73% |
| Patrol Contact (fireteam / patrol) | 42% / 51% | 42% / 51% |

## Real profiles: the Boarding element's VAT fireteam

Five VATs (leader, two assaulters, gunner, data specialist) clear one 6"×6" compartment, within three rounds:

| Defenders | On Overwatch: cleared (best entry) | VATs out | Unready: cleared | VATs out |
|---|---|---|---|---|
| Asset security, 3 (tier 1) | 50% (frag first) | 1.1 | 49% | 0.4 |
| Asset security + recovery captain | 37% (frag first) | 1.6 | 15% | 0.6 |
| Repossession team, 3 (tier 3) | 16% (frag first) | 2.8 | 5% | 1.1 |
| Contract boarding team, 4 (tier 3) | 16% (frag first) | 2.7 | 1% | 0.9 |

"Unready" rooms aren't often cleared within three rounds, because armoured models trade slowly, but they cost the VATs about a third of the casualties. At the table the player can also take more rounds, use cover inside the room, and bring the attached model. The simulator does none of these.

**Close combat by profile** (attacker charging):

| Matchup | Attacker wins |
|---|---|
| VAT Assaulter vs Salvage Hand | 94% |
| VAT Assaulter vs Asset Security Officer | 76% |
| VAT Assaulter vs Repossession Breacher (Wounds 2) | 43% |
| Repossession Breacher vs VAT Assaulter | 63% |
| Contract Boarder vs VAT Assaulter | 53% |
| Directorate Deniable Operator vs VAT Team Leader | 77% |
| Control Team Marksman vs Collections Enforcer | 24% |

The profiles read as intended. Crew and guards lose at close quarters; repossession teams are close-combat specialists the VATs should shoot, not fight; Directorate operators are a story-tier threat; marksmen must not get caught.

## Implications

1. **Mission design (Phase 5):** a starting Boarding element should mostly meet **tier 1–2** enemies, with only one or two alert, prepared rooms per mission. Tier 3 elements belong to later campaign turns, or appear as the reaction force when the alert track runs high, which gives the player a reason to keep it low.
2. **Contacts and the alert track carry the balance.** Unready defenders cost about a third of the casualties, so the stealth, flashbang and breach tools matter. That is the doctrinal feel we're after.
3. **Wounds 2 is strong** on Fight 2 troops. Watch repossession teams and contract heavies at the table.
4. **Progression has room:** Regular VATs at Shoot 1 leave obvious veterancy upgrades (Shoot 2, Fight 3, Marksman, more Wounds).
