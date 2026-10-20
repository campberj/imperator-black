# Sci-Fi Skirmish Game: Design Statement and Roadmap

2026-10-11 · RJ

> **Imported from A War of Worlds**, the predecessor design to Imperator Black. Kept as the historical record. Its pillars, non-goals and settled decisions carry forward; where it conflicts with [decisions.md](../decisions.md) (for example the trench-war setting pack, which is dropped, and its twelve-step roadmap, replaced by [the PoC roadmap](../../roadmap.md)), the decision log wins.

## Design statement

**Working title: TBD.** This is a military sci-fi skirmish game for 28mm collections, scaling from a single fireteam to a platoon, in which a solo commander leads named soldiers through a campaign and feels the weight of doctrine, equipment and loss. At a high level, the player represents and independent, autonomous leader, similar to captains of ships in the age of sail. The game should provide heavy narrative choices and missions that the player must react to.

A game turn should feel like a junior officer's problem. Where do I fix the enemy, who carries the support weapon, when do I commit the reserve? Weapons reach across the board, so cover, movement and fire support matter more than who charges first.

The game promises three things. It plays like a firefight rather than a duel. It builds like a table of organization and equipment rather than a point-buy list. It remembers like a campaign, so the soldiers who walk off the table are not the ones who walked on.

*Status: draft for your review. Everything below is a proposal built from your stated goals; change anything that doesn't ring true.*

## Design pillars

Eight pillars, each with the test a rule or table must pass to stay in the game. When two pillars collide, the earlier one wins.

| # | Pillar | What it means in play | Test it must pass |
|---|---|---|---|
| 1 | Soldiers, not superheroes | Troops behave like trained infantry: they take cover, suppress, fall back and break. Experience and leadership matter more than raw stats. | A veteran squad in cover beats a larger green squad in the open most of the time. |
| 2 | Collection first | Rules are model-agnostic and setting-neutral. Any 28mm infantry, vehicle or walker can be statted from its look and kit. | A new miniature line can be statted in under ten minutes with no special rules. |
| 3 | Cover is tactics | Weapons have no fixed ranges. Keywords such as sidearm or shotgun take penalties past a distance set for each keyword. Cover and movement are the main decisions. | A squad can bound from cover to cover in a turn, and crossing open ground under fire is costly. |
| 4 | Equipment is the system | Weapons, armor and gear carry real, distinct profiles. Loadouts change how a soldier is used. | Two soldiers with different loadouts should rarely do the same job. |
| 5 | Structure is a choice | Squad size, composition and chain of command are the player's to build, from fireteam up to platoon. | The same rules run a 6-model patrol and a 40-model platoon without changing the core. |
| 6 | Everyone can earn a name | Soldiers, squads and the whole force progress. Leaders can arise, but a rifleman can surpass the starting captain. | After five missions, the roster tells a story no one planned. |
| 7 | The campaign remembers | Missions have consequences: casualties, supply, reputation, objectives won or lost carry forward. | A lost mission changes what the next one looks like. |
| 8 | Solo is the home mode | The opponent is a built-in system, not a patch on a two-player game, and it behaves like an enemy force, not a random dice-roller. | A full mission can be played solo with no judgment calls beyond the rules. |

One rule sits under all eight: crunch lives where decisions are, not in paperwork. Depth is welcome; bookkeeping that doesn't change a decision is not.

## Non-goals

The game is deliberately not a combat simulation, and saying no early keeps the design coherent.

- **Not a simulation.** Doctrinal feel, not modeled ballistics or realistic paperwork.
- **Not tied to a setting or miniature line.** No faction lore needed to play; a setting can be layered on.
- **No occult, demonic or supernatural content** in the core rules or examples.
- **Not tournament-first.** Balance is a nice-to-have; campaign play and scenarios come first.
- **Not forced-cohesion abstraction.** No rigid unit blobs that move and die as a single token.
- **Not an RPG.** Soldiers have identity and history, but no character sheets that take longer than the battle.

## Constraints and assets

These are the fixed edges of the design. Anything marked *open* is decided in the roadmap, not here.

| Area | Constraint |
|---|---|
| Scale | 28mm miniatures |
| Collection | Several unrelated sci-fi lines; infantry plus a good number of vehicles and walkers; existing sci-fi terrain |
| Force size | Fireteam to platoon, one rules set (squad to platoon is the sweet spot) |
| Activation | Alternating single models (settled) |
| Resolution | 2d6 *(**settled), two actions per model* |
| Complexity | Medium to heavy crunch, depth as the goal rather than complexity for its own sake |
| Primary mode | Solo, with an opponent AI designed as part of the game |
| Campaign | Required: missions, consequences, soldier, squad and force progression |
| Table size and ground scale | *Variable boards, from 18x18 inches (boarding actions) to 9x5 feet (large battles); 1 inch = 2 metres* |

## Reference games

No single game meets all eight pillars, so the plan is to take proven pieces and fix the gaps. This table is a starting list; step 2 of the roadmap turns each row into notes.

| Game | Take | Leave or fix |
|---|---|---|
| Five Parsecs: Tactics | Campaign flow and a feel that often seems right | Weapon variety and lethality, short movement and ranges, thin morale, command and cohesion |
| Stargrunt II | Long ranges with no weapons table, troop quality dice, suppression, motivation, squad-level structure | Built for 15mm or 25mm, no progression or solo found, heavy weapons underpowered by design |
| Zone Raiders | Large equipment arsenal, doctrine upgrades across a campaign, AI missions for solo and co-op | Survivor-scavenger setting, not a military force |
| Horizon Wars: Zero Dark | Card-driven enemy AI, campaign progression | No weapons tables, which conflicts with pillar 4 |
| Eldfall Chronicles | Tactical mechanics already adapted in the ICC project | To be reviewed for what carries over |

## Reference teardown (step 2)

How each reference game handles the five things we care about. Built from published descriptions, designer posts and reviews, not from a full read of every rulebook, so treat details as pointers to check against the books you own.

| Game | Activation | Resolution | Solo opponent | Progression and equipment |
|---|---|---|---|---|
| Five Parsecs: Tactics | Scenario-driven, scaled up from squad skirmishes to platoons, tanks and monsters | D6-based, inherited from Five Parsecs From Home | Built-in AI mechanics for the enemy, plus solo campaign rules | Random tables for the unexpected; the Field Commander's Guide adds a War Generator, Force Events and Battlefield Conditions |
| Stargrunt II | Alternating by unit; leaders can pass actions down to subordinates | Quality die per squad (Untrained to Elite); the opposing side rolls a range die adjusted for cover; impact die against armour die | None built in | Quality can be improved; squad cards and mission cards; no points values |
| Zone Raiders | Two actions per model; shooting ends the activation | Defence and Survival stats, armour must be beaten before damage | Adversarial Intelligences: hidden patrol contacts that deploy when found, with brief behaviour rules per model | Over 150 weapons, armour and gear; Doctrines unlocked with campaign resources; individual level-ups |
| Horizon Wars: Zero Dark | One activation, then a flip of the Control Deck | D12 stat checks | A 54-card deck: number cards run grunts, face cards run elites, jokers trigger complications; the deck doubles as a mission clock | XP and levelling, unlimited upgrades per hero; no weapon tables |
| Eldfall Chronicles | Activation points per model; the other side can react by spending points | Dice vs attribute value | Hostile Behaviour rules let any model be run by the game | Quest log campaign with decisions tracked across missions (fantasy setting) |
| Forbidden Psalm | One model at a time, alternating; monsters always go last | D20 against a flat target, modifiers added | Scripted monsters acting last | Ten linked scenarios; reviewers call the campaign tie thin |

### Adopt, adapt or reject

Each row is a mechanic worth carrying into the design, or turning down, with the reason.

| Mechanic | Source | Verdict | Why |
|---|---|---|---|
| Quality tier sets how far fire stays effective | Stargrunt II | Adapt | Fits the no-ranges decision: better troops keep their fire effective at distance, while short-range keywords still take penalties |
| Suppression that blocks actions until cleared | Stargrunt II | Adopt | Makes soldiers behave like soldiers (pillar 1); cap the markers so it never locks the game |
| Leader passes actions to subordinates; fireteams as sub-units | Stargrunt II | Adopt | The mechanism for scaling from fireteam to platoon (pillar 5) |
| Wounded more likely than killed; morale cost for abandoning wounded | Stargrunt II | Adopt | Feeds campaign injuries and the weight of loss |
| Control deck as the enemy brain | Zero Dark | Adapt | The deck now drives only the enemy, since players alternate single models; the mission clock is worth keeping as an option |
| Hidden patrol contacts revealed on discovery | Zone Raiders | Adopt | Gives a solo opponent surprise and stealth without a human hiding things |
| Short behaviour rules per enemy model | Zone Raiders | Adopt | Easy to write for any unit type, and the base for the solo layer |
| Doctrines unlocked by campaign resources | Zone Raiders | Adapt | Becomes force-level progression tied to unit honors and reputation |
| Armour that grants stats and movement options | Zone Raiders | Adapt | Fits tiered equipment for powered armour and heavier gear |
| Activation points with reactions | Eldfall | Adapt | Gives overwatch and bounding a rules home; decide against two-action models in step 4 |
| Decision tracking across missions | Eldfall | Adapt | Supports the independent-captain campaign with consequences that return |
| War Generator, Force Events, Battlefield Conditions | Five Parsecs: Tactics | Adapt | Starting point for the mission and event generator in step 9 |
| Flat D20 target number | Forbidden Psalm | Reject | Too coarse for the equipment detail in pillar 4 |
| No weapon tables | Zero Dark | Reject | Pillar 4 needs distinct weapon profiles, though ranges stay out |

### What this leaves open

- **Resolution system.** Settled: 2d6. Troop quality becomes a modifier or target number, tuned in step 4.
- **Action economy.** Settled: two actions per model, so a soldier can bound and fire, or spend both on one complex action. Reactions such as overwatch are still to be designed.
- **Verification.** Several rows come from descriptions, not full rules. Before step 4, check the Five Parsecs: Tactics and Eldfall rules you own for the exact activation and reaction wording.

Step 3 needs one input from this teardown: quality as the thing that sets reach. The action economy is now settled.

## Scope overview

The game is built in four layers, each resting on the one before it, which fixes the order of work.

1. **Tactical layer.** Table, scale, activation, movement, shooting, suppression, morale and damage. Nothing else works until this does.
2. **Force layer.** Soldier and unit profiles, weapons and equipment, squad and platoon structure, vehicles and walkers.
3. **Solo layer.** The opponent AI: how it activates, moves, targets and decides, plus how it gets objectives.
4. **Campaign layer.** Mission generation, between-mission sequence, casualties and supply, and progression for soldiers, squads and the force.

The solo layer sits third on purpose. An AI built before the tactical rules are stable has to be rebuilt each time they change. A trench-war sci-fi setting, with its own equipment tier, scenarios and mission types, is an optional setting pack built after the core is stable.

## Open design decisions

Each has a recommended default so work can start; any can be overturned by a prototype.

| Decision | Options | Recommended default | Why |
|---|---|---|---|
| Foundation | Hack an existing game, or write a fresh core | Fresh core that borrows proven ideas | Your complaints (lethality, ranges, cohesion, morale) sit in the core, so a hack means rewriting it anyway |
| Resolution | Settled: 2d6 | 2d6 to hit, thrown together with the weapon's damage dice (2d4 to 2d12); damage counts only on a hit and is read against a wound table after armour steps it down; troop quality as a modifier or target number, tuned in step 4 | A 2d6 bell curve carries fine weapon and quality modifiers, and promotion becomes a simple bonus |
| Activation | Settled: two actions per model, players alternate activating single models | Alternating single models; the solo opponent gets its own activation-order rule | Keeps the two-sided game simple; the AI ordering rule is designed in step 8 |
| Model-to-man ratio | One model per soldier, or abstracted | One model per soldier, with fireteams as sub-units | Supports named soldiers and flexible structure |
| Damage model | Wound counts, or states such as suppressed, wounded, down | A wound table of up to ten rows keyed to the damage roll, with varied effects; armour steps the result down by whole rows (0 to 3) | Gives variety of effects and feeds campaign injuries; small weapons live in the low rows, only heavy weapons reach the top |
| Force building | Points, or a campaign requisition budget | Requisition budget, with points optional | Fits the campaign and avoids point-balance work |
| Technology | One fixed tech level, or tiers | Tiered equipment so different lines fit | Lets mixed collections share a table |
| Table size and ground scale | Settled: variable boards from 18x18 inches to 9x5 feet; 1 inch = 2 metres | No weapon ranges; short-range weapon keywords take penalties past a distance set per keyword | A real rifle outranges every board at this scale, so cover, movement and micro-terrain decide fights instead |

## Development roadmap

Twelve steps, in order. Each ends with a gate: a thing you can hold or play before the next step starts.

1. **Design statement and pillars** (this document). *Gate: you approve or edit it.*
2. **Reference teardown.** One page per reference game on what to take and what to leave. *Gate: a short list of mechanics we will adopt, adapt or reject.*
3. **Physical framework.** Movement rates matched to typical cover gaps, cover rules, a troop-quality test for using micro terrain in the open, and per-keyword penalty distances, all sized for boards from 18x18 inches to 9x5 feet at 1 inch = 2 metres, plus definitions of a trooper, fireteam, squad and platoon. *Gate: a one-page scale sheet.*
4. **Core resolution and activation prototype.** 2d6 hit roll thrown with weapon damage dice, wound table, two-action turns, alternating single-model activation, shooting, cover, suppression, morale. *Gate: two fireteams fight a clean solo test with no AI.*
5. **Damage and casualty model.** States, recovery and what carries into the campaign. *Gate: the test fight produces believable casualties.*
6. **Weapons and equipment framework.** Profile format, range keywords, traits, armor, gear and tech tiers, starting with about twenty weapons. *Gate: loadouts visibly change how a soldier plays.*
7. **Force building.** Troop types, squad and platoon templates, requisition budget. Vehicles and walkers follow in step 11. *Gate: you can build three different platoons from your collection.*
8. **Solo opponent.** Behavior rules tied to the activation deck, targeting, and mission objectives. *Gate: a full mission is playable solo without judgment calls.*
9. **Missions and scenarios.** A mission generator and six to eight starting scenarios. *Gate: you play three different missions back to back.*
10. **Campaign layer.** Between-mission sequence, casualties, supply, soldier, squad and force progression, unit honors. *Gate: a five-mission campaign with a roster that visibly changes.*
11. **Vehicles, walkers and support.** Armor, weapons, crew, artillery and air support. *Gate: a combined-arms mission works.*
12. **Playtest and tooling.** Reference sheets, roster and tracking sheets, revisions from play. *Gate: one clean, rules-only campaign start to finish.*

**Where to start next:** move to steps 2 and 3 with movement and cover as the first problem, since table size, ground scale and the no-ranges approach are now settled. Short ranges and short moves are where your current games fall short, so they set the order of work.
