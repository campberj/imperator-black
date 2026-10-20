# Imperator Black — Proof of Concept Roadmap

## PoC definition of done

Using only the printed rules and the app, a solo player can:

1. Receive a tasking and generate a mission (briefing, ship layout map, enemy force).
2. Build the ship on the table from 6"×6", 6"×3", and 3"×3" tiles and place enemy tokens.
3. Play the mission with a Task Force Boone fireteam against enemies run by behavior tables.
4. Log the after-action report in the app and resolve casualties, experience, and awards.
5. Upgrade the ship, advance on the sector map, and receive the next tasking.
6. Repeat this for a **5-mission campaign arc** without needing to invent rules at the table.

## Guiding principles

- **Rules before software.** Every procedure works on paper first. The app automates paper procedures; it never invents its own.
- **One source of truth.** Tables live in `content/` and feed both the book and the app (see [ADR-001](architecture/adr-001-architecture.md)).
- **Playtest gates.** Phases that change how the game feels end with play, not with writing.
- **Thin layers.** The PoC ship/outpost and sector layers are minimal and exist to prove the loop, not to be complete.

---

## Phase 0 — Foundation

Set up the repo and record what is already decided.

- [x] Initial design document in the repo
- [x] A War of Worlds design statement and prototype rules v0.1 imported ([a-war-of-worlds/](design/a-war-of-worlds/))
- [x] Decision log carrying over earlier design work ([decisions.md](design/decisions.md))
- [x] Design pillars and non-goals ([pillars.md](design/pillars.md))
- [x] Architecture decision ([ADR-001](architecture/adr-001-architecture.md))
- [x] Terrain inventory (12 each of 6×6, 6×3, 3×3 tiles; Kill Team / Boarding Actions walls and doors; 3'×5' dry-erase map)
- [x] Opposition force: Freespace Solutions ([opposition-force.md](design/opposition-force.md))
- [x] Glossary of terms ([glossary.md](design/glossary.md))
- [x] Playtest log template ([TEMPLATE.md](playtests/TEMPLATE.md))

**Exit:** Repo structure exists, the decision log is agreed, and the opposition is chosen.

## Phase 1 — Core rules v0.2 and the math

Start from **prototype rules v0.1**, not a blank page. Bring it into `rules/`, apply today's decisions, and close its known gaps.

1. **Rebuild the dice simulator** in `tools/` (the A War of Worlds simulator isn't in the repo). It should give exact probabilities per shot and Monte Carlo results per fight, and read weapons and armour from `content/`.
2. **Lethality targets:** confirm or revise the prototype's (deaths rare from rifles, danger from rows 7–8, ~50–60% player win rate against the solo AI).
3. Rules chapters v0.2 in `rules/`:
   - Core concepts: Q, tests, measuring, line of sight, cover
   - Turn sequence: initiative, alternating activation, two actions, **reactions by unactivated models, Overwatch with up to two reactions**
   - Movement and terrain: bounding, Find Cover test, **doors, hatches and bulkheads**
   - Shooting: hit roll with damage pair, wound table, armour steps, short-range keywords
   - Close combat (**new**; boarding actions need it)
   - Grenades and blast in confined spaces (**new**)
   - Suppression, casualty shock, breaking; fix the row-6 Down recovery gap
4. **The gym:** 2v2 then 4v4 on a few 6×6 tiles, focused on reactions, Overwatch and doorways.

**Progress (2026-10-20):**
- [x] Rules v0.2 chapters 1–8 in [`rules/`](../rules/), with tables generated from `content/`
- [x] Simulator rebuilt in C# (`app/src/Imperator.Sim`), sharing the game-logic library with the app
- [x] First simulator pass: [findings](analysis/sim-v0.2-findings.md) (lethality retune proposed)
- [x] Lethality retune adopted; rules v0.3 published ([results](analysis/sim-v0.3-results.md))
- [ ] Gym playtests at the table

**Exit:** Gym fights play cleanly; the simulator output matches the lethality targets.

## Phase 2 — Forces, equipment, and content schema

**Progress (2026-10-20):**
- [x] Content schema for gear, troops, factions and elements, rosters, behaviors ([content/README.md](../content/README.md)), with a cross-reference validator in CI
- [x] Task Force Boone teams and starting force; Freespace, contractor and Directorate troops and elements (proposed, awaiting RJ's review)
- [x] Rules chapter 9: [forces and equipment](../rules/09-forces-equipment.md)
- [x] Printable [unit cards and quick reference](print/)
- [x] RJ review: force size 3–8, gear approved, Automated settled, Razorback deferred
- [x] Granular profiles (rules v0.4): attributes, ranks, Wounds, Size, levelled skills ([findings](analysis/sim-v0.4-findings.md))
- [x] Gym scenario: [Gym 1: First Breach](scenarios/README.md), with sheet, map and curated cards
- [x] RJ played Gym 1 ([log](playtests/2026-10-20-gym-01-first-breach.md)): game fully playable from cards and quick reference; door reactions, enemy noise response and same-room enemy orders need work. **Phase 2 exit met.**

1. Define the YAML schemas: weapon, armor, gear, troop type, unit, keyword.
2. **Task Force Boone** roster: fireteam of 4–6 plus a leader, with roles (rifleman, gunner, breacher, medic, marksman, leader).
3. **Opposing force:** build out the chosen opposition's elements, mapped to the BLKOUT / BPRE miniatures you own ([opposition-force.md](design/opposition-force.md)).
4. Boarding gear: breaching charges, door controls, grenades in confined spaces, drones (if in scope).
5. Printable unit cards and a quick reference sheet.

**Exit:** Every model on the table has a card; all stats live in `content/`.

## Phase 3 — Enemy behavior profiles

1. Extend the prototype's activation-priority list and role priorities into one profile per enemy element (proposed: *Cowed*, *Sentry*, *Aggressive*, *Tactical*, *Saboteur*, *Automated*).
2. For each: a short decision table read on activation, target priority, and when it reacts or overwatches.
3. Contact tables per threat tier, contact movement, and the alert track (see [opposition-force.md](design/opposition-force.md)).
4. Tie-breakers for choices the tables don't cover (the prototype's "nearest the objective, then lowest-numbered model").

**Exit:** The player makes no more than one judgment call per turn on the enemy's behalf.

## Phase 4 — Ship and station layout procedure

1. Grid convention: 3" cells, 18"×18" board = 6×6 cells. The inventory (12 of each tile, about 756 square inches) also supports larger decks such as 24"×24" or a long 36"×18" ship.
2. Room types by tile size (6×6 compartments, 6×3 corridors / passages, 3×3 junctions, airlocks, closets); walls and doors from the Kill Team / Boarding Actions sets.
3. Paper procedure: roll the vessel type, then place an entry point, rooms, connections, doors, objectives, and enemy tokens.
4. Boarding environment rules: doors and hatches, breaching, cramped spaces, optional hazards (decompression, fire, power loss).

**Exit:** The paper procedure produces a sensible, playable layout in under 10 minutes using only owned tiles.

## Phase 5 — Missions

1. Four mission types for the PoC, e.g. *Breach and Clear*, *Recover Personnel*, *Seize Data / Cargo*, *Sabotage*.
2. Objectives, victory conditions, and **outcome grades** (decisive / marginal / failure) that feed the campaign.
3. Tables: complications, atmospherics, enemy force size and composition by threat level, reinforcements.
4. Mission briefing format.

**Exit:** Each mission type played at least once.

## Phase 6 — Playtest block 1 (gate)

- Play 4–6 solo missions using paper procedures only. Log each game with the playtest template.
- Revise to rules **v0.2**.

**Exit:** No known rules holes in a standard mission; lethality and tempo feel right.

## Phase 7 — Campaign loop (paper)

1. **Campaign turn:** tasking → preparation → mission → after-action → upkeep → events.
2. **Post-battle:** casualty and injury table, experience, awards and veterancy, promotions, unit honors, force reputation.
3. **Ship / outpost layer (lite):** the player's ship with 2–3 upgrade tracks (e.g. medical bay, armory, intelligence), requisition points, recovery times.
4. **Sector layer (lite):** a small node map (6–8 nodes) for one campaign arc, using the **Black Ops** campaign type (high-level orders, wide latitude).
5. Campaign events and narrative tables.

**Exit:** A 3-mission paper campaign plays through without rule gaps.

## Phase 8 — App MVP, part 1: generators

*Can start in parallel with Phases 6–7 once Phase 4–5 content stabilizes.*

1. Scaffold the monorepo app: `Imperator.Core`, `Imperator.Api`, `web/`, Docker Compose for Postgres, GitHub Actions CI.
2. Content loader and schema validator.
3. Seeded generators: layout, mission, enemy force. Implements exactly the paper tables.
4. Outputs: printable briefing and SVG map with tiles, doors, numbered tokens, objectives, and entry points; enemy guidance cards.
5. Rulebook build: Markdown plus generated tables to PDF.

**Exit:** One click produces a printable briefing and a map that matches what the paper procedure could produce.

## Phase 9 — App MVP, part 2: campaign tracker

1. Data model: Campaign, Player, CampaignMembership, Roster, Soldier, Ship, SectorNode, and the event log.
2. Screens: roster, after-action report entry, post-battle resolution (using the same tables), ship upgrades, sector map, next tasking.
3. Campaign history feed from the event log.

**Exit:** The whole campaign loop runs in the app for one player.

## Phase 10 — PoC validation

- Play a **5-mission campaign arc** end to end with rules and app.
- Record findings; publish rules v0.3 and the PoC app build.

**Exit:** The definition of done at the top of this document is met.

---

## Sequencing

```
P0 → P1 → P2 → P3 ─┐
          P4 ───────┼→ P5 → P6 → P7 → P10
                    └→ P8 ──────→ P9 ─┘
```

Phase 4 can begin alongside Phase 2. Phase 8 starts once layout and mission tables exist.

## After the PoC

- Multiplayer: accounts, shared campaigns, Commander/Operator roles, asynchronous campaign turns
- Full ship/outpost and sector layers; additional campaign types with tighter command guidance
- Planetside battles and larger boards
- Vehicles, squad- and platoon-scale play
- PvP rules
- More organic solo AI
