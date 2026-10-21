# Imperator Black — Design Decision Log

Living record of design decisions. Each entry is **Settled** (build on it), **Leaning** (default unless playtesting says otherwise), **Proposed** (awaiting a call), or **Open** (needs work).

Imperator Black continues the earlier *A War of Worlds* design. Its documents are kept in [a-war-of-worlds/](a-war-of-worlds/): the [design statement and roadmap](a-war-of-worlds/design-statement-and-roadmap.md) and [prototype rules v0.1](a-war-of-worlds/prototype-rules-v0.1.md). Entries marked *(AWoW)* come from there. Where this log and those documents disagree, this log wins.

Last updated: 2026-10-20 (Gym 1 fixes: door sequence, hacking; Phase 3 solo opponent)

---

## 1. Identity and scope

| # | Decision | Status |
|---|----------|--------|
| 1.1 | Imperator Black continues A War of Worlds; everything settled there carries forward unless revised here. | Settled |
| 1.2 | Design pillars and non-goals per [pillars.md](pillars.md) *(AWoW, revised)*. | Settled |
| 1.3 | Model-agnostic: no rule depends on a specific miniature line. *(AWoW)* | Settled |
| 1.4 | One model per soldier; fireteams are sub-units. *(AWoW)* | Settled |
| 1.5 | Scale: fireteam up to platoon, one rules set. PoC targets fireteam / squad. *(AWoW)* | Settled |
| 1.6 | Crunch medium to heavy. Crunch lives where decisions are, not in paperwork. Doctrinal feel, not a simulation. *(AWoW)* | Settled |
| 1.7 | Three layers: Tabletop → Ship/Outpost → Sector. | Settled |
| 1.8 | Play modes: solo is the home mode; co-op and PvP follow. PoC = solo. | Settled |
| 1.9 | Fresh core that borrows proven ideas, not a hack of an existing game. *(AWoW)* | Settled |

## 2. Setting

| # | Decision | Status |
|---|----------|--------|
| 2.1 | Tone sits between *The Expanse* and *Galaxy's Edge*, leaning Galaxy's Edge: grounded military fiction, believable tech, space-opera scope. | Settled |
| 2.2 | The trench-war setting pack from A War of Worlds is **dropped**. | Settled |
| 2.3 | The player leads a force whose latitude **varies by campaign type**: Black Ops campaigns give high-level orders and wide freedom; others give tighter guidance. Revises the AWoW "fully autonomous age-of-sail captain" framing. | Settled |
| 2.4 | Lore reason most fights are small-unit (candidates: ubiquity of autonomous guided munitions; difficulty moving large forces through the void). | Open |

## 3. Core resolution

| # | Decision | Status |
|---|----------|--------|
| 3.1 | **2d6** hit roll, thrown together with the weapon's **damage dice pair** (2d4 → 2d12). Damage counts only on a hit. *(AWoW)* | Settled |
| 3.2 | Hit target **8+** on 2d6 + Q + modifiers. Natural 2 always misses; natural double 6 is a critical (+1 wound row). *(AWoW prototype)* | Leaning — playtest |
| 3.3 | **Rank** (Green, Regular, Veteran, Elite) is kept as a label only; it never enters a roll. Replaces numeric Quality (v0.4). | Settled |
| 3.4 | Damage total is read against a **ten-row wound table**; **armour steps the row down by whole rows (0–3), less weapon AP**. *(AWoW)* | Settled |
| 3.5 | Wound table bands and results per prototype v0.1 (suppression in rows 1–3, Wounded 4–5, Down/bleeding/critical 6–8, Dead 9–10). Rows 1–3 are the first tuning target. *(AWoW prototype)* | Leaning — playtest |
| 3.6 | **Down recovery:** end of each round, a Down model tests 2d6 + Q at 8+; on a pass it gets up Wounded with 3 Suppression. A *Medic* recovers a Down model automatically. | Leaning — playtest |
| 3.7 | **No fixed weapon ranges.** Each weapon has a **short range**; beyond it -1, beyond double -2 (shotgun -2/-3). *(AWoW)* | Settled |
| 3.8 | Quality may extend how far fire stays effective (Stargrunt-style). *(AWoW teardown)* | Proposed — Phase 1 |
| 3.9 | Using micro-terrain in the open is a **troop-quality test** (Find Cover: 2d6 + Q − Suppression; 7+ Light, 10+ Heavy). *(AWoW)* | Settled |
| 3.10 | Cover: Light (≥⅓ hidden, −1 to hit) and Heavy (≥½ hidden behind solid material, −2). True line of sight. *(AWoW prototype)* | Leaning |
| 3.11 | **Close combat:** started by moving into contact; resolves at once in exchanges of opposed 2d6 + Q (charge +1 first exchange, support +1 each up to +2). The winner rolls melee damage on the wound table; a no-effect result still adds 1 Suppression. Continues until one side is incapacitated or **both** break away. A model at 5 Suppression that loses is overpowered (Down or captured). | Settled |
| 3.12 | **Grenades and demolitions:** Throw action (test 8+, misses land short, natural 2 slips); frag (one damage roll, +1 row in confined compartments), flashbang (+2 Suppression, strips Overwatch, no reactions this round), smoke; breaching charges (place with Interact, detonate as a later action). | Leaning — playtest |
| 3.13 | **Lethality retune (rules v0.3):** re-banded wound table (suppression 2–5, Wounded 6–9, Down 10–11, Bleeding 12–13, Critical 14–15, Dead 16+); sidearm 2d6, SMG and carbine 2d8, shotgun 2d12; knife 2d8, breaching tool 2d10 (unarmed 2d4); frag 2d8. Per [sim findings](../analysis/sim-v0.2-findings.md); passes Pillar 1 at 75%. | Settled — playtest |
| 3.14 | **Granular troop profiles (rules v0.4):** attributes **Move, Shoot, Fight, Physique, Nerve, Tech, Wounds, Size** on the 2d6 engine; each test names its attribute (Shoot: hit rolls; Fight: close combat; Physique: throwing, breaching tool, decompression, Down recovery; Nerve: breaking, Find Cover, Leader initiative; Tech: Medic, hacking, fires). Values usually 0–3, 4 exceptional. | Settled |
| 3.15 | **Move varies by troop** (usually 5–7"), reflecting training, exoskeletons and burden of gear. | Settled |
| 3.16 | **Wounds:** wound rows 4–5 add Wounded markers; any marker −1 to all rolls; more markers than Wounds → Down. Rows 6+ unchanged. A model getting up keeps markers equal to its Wounds. | Leaning — playtest |
| 3.17 | **Size:** 1 small drone, 2 human, 3 large; walkers and vehicles larger. First use: crawlways and small hatches admit Size 2 or less. Stealth, resistances and similar come as special rules, not attributes. | Settled |
| 3.18 | **Skills with levels** replace model keywords (written *Marksman 2*; higher levels include lower). First levelled skill: *Marksman* 1–2. | Settled; Marksman Proposed |
| 3.19 | **Range bands (RJ, Gym 1 replay):** short range +1, up to double short range 0 (optimum), beyond that −1 (effective, table-wide), instead of today's 0 / −1 / −2. Note: this keeps the three bands but shifts every shot +1, the same as lowering the hit target from 8 to 7, so it raises lethality across the board (shotgun penalties would shift too). If the aim is the point-blank feel without more lethality, the alternative is +1 / 0 / −1 with the hit target at 9. Simulate both before deciding. **Held until it recurs in more playtests** (RJ: rules change on repetition, not after every game). Simulated with 3.20 (A, B, A+B): A and B are identical for shooting; A+B is too much (Pillar 1 75% → 68%). **Test A as a playtest variant next game.** Superseded for the next game by 3.21: test Task Force Boone at Shoot 2 alone first (A on top would be +2 for them); bring A back only if misses still dominate. | Open — after 3.21 test |
| 3.20 | **Hit target 8+ may be one too high (RJ, Gym 1 replay):** shots miss a lot once cover, Wounded or Suppression apply; RJ also sees the realism argument. Simulated 7+: fights resolve faster (flash-and-clear vs Overwatch 38% → 57% by round 3; stalemates gone) with balance between sides almost unchanged. Options: keep 8+, go to 7+, or keep 8+ and cap stacked penalties. [Comparison](../analysis/hit-target-comparison.md). Check together with 3.19. Raised again in the Gym 1 replay post-game ("a LOT of shots" at about 3"): now a recurring theme; test a variant next game. | Open — test next game |
| 3.21 | **Shoot scale (RJ):** +0 irregulars and conscripts; +1 trained military; +2 professionals (rangers, Task Force Boone); +3 elite units; +4 or more legendary heroes. A unit's class sets its baseline; a specialist may sit one above it in its speciality; rank is experience within the class. Applied: Task Force Boone line troopers Shoot 1 → 2 (leaders 2, marksman 3 unchanged). Simulator: VAT flash-and-clear against Overwatch 38% → 59% by round 3, VAT models out 1.09 → 0.95. Directorate deniable operators are professionals (2); the handler is elite (3) (RJ). | Settled (RJ) |

## 4. Turn structure, reactions and morale

| # | Decision | Status |
|---|----------|--------|
| 4.1 | Each round starts with an initiative roll (2d6 + leader's Q); winner chooses who activates first. *(AWoW prototype)* | Leaning |
| 4.2 | Players **alternate activating single models**; when one side runs out, the other finishes. *(AWoW)* | Settled |
| 4.3 | **Two actions** per activation: Move, Shoot, Aim, Suppressive Fire, Overwatch, Find Cover, Rally, Medic, Interact. *(AWoW prototype)* | Settled (action list Leaning) |
| 4.4 | **Reaction:** a model that has not yet activated may react when an enemy model's action allows a reaction. Reacting marks it as activated. | Settled |
| 4.5 | **Overwatch:** a model that activates may go on Overwatch. It may then react **up to two times** until its activation comes again next round. Supersedes the prototype's single Overwatch shot. | Settled |
| 4.6 | **Reaction triggers:** an enemy Moving into or within line of sight (interrupts the move), Shooting/Throwing/Suppressive Fire, or Interacting (including doors). **Reaction choices:** one action: Shoot the triggering model at −1, Move up to half to cover or out of sight, or Find Cover. Reactions can't trigger reactions. | Settled |
| 4.11 | **Exposure trigger (from Gym 1):** an enemy model that comes into an Overwatch model's line of sight for any reason (door opened, smoke clears, wall breached) also triggers a reaction. Covers a gunner overwatching a friendly breach. | Leaning — written into chapter 2 |
| 4.12 | **Auto N (from Gym 1):** an Auto weapon's Shoot or Suppressive Fire may engage up to N different models within 3" of the first target, one roll each, −1 for every target after the first; reactions may spread too. SMG Auto 2, compact LMG Auto 3, LMG Auto 3. Simulator: gun team vs riflemen 48% → 54%; bunched targets take about 2.3× the casualties per burst. | Proposed — playtest |
| 4.13 | **Door sequence (from Gym 1):** when a door opens by any means: (1) resolve the opening action; (2) a cracked-in grenade lands; (3) the far side's Overwatch and unactivated models that can now see an enemy react; (4) the opening side's Overwatch models react; (5) the opener carries on. Each model reacts once per door opening. Chapter 3, with worked examples. | Proposed — playtest |
| 4.15 | **Crack and throw:** a model within 1" of a closed, unlocked door opens it and throws a grenade through in **one Interact**, resolved before anyone reacts to the door. A missed throw lands in the doorway. | Proposed — playtest |
| 4.14 | **Hacking security levels:** each lock or system has a level set by the mission: Basic 7+, Secured 9+ (default), Hardened 9+ ×2 successes, Military 10+ ×2. Successes accumulate across models and rounds; each failed attempt on a target gives +1 to later attempts (max +3). Hacking is quiet. Option held back: a fail by 4 or more is noise. | Proposed — playtest |
| 4.7 | **Suppression:** each marker −1 to all rolls (max 5); **Pinned at 3+** (only Rally, Find Cover, Move to cover, Shoot from cover). Two or more Suppression cancels Overwatch. *(AWoW prototype)* | Leaning |
| 4.8 | **Casualty shock** (+1 Suppression when a friend within 6" goes Down/Dead; +2 for the leader) and **breaking** at half a fireteam Down or Dead. *(AWoW prototype)* | Leaning |
| 4.16 | **Breaking clarified (Gym 1 replay):** test at every activation while at half strength; normal test modifiers; attached models count with their team; enemy elements count as fireteams; Automated never test; recovered models count again; leaving by the entry point = fled. | Settled (RJ) |
| 4.9 | Leaders can pass actions to subordinates, as the scaling mechanism to squad and platoon (Stargrunt-style). *(AWoW teardown)* | Proposed — post-PoC |
| 4.10 | No forced unit-cohesion abstractions. *(AWoW)* | Settled |

## 5. Movement, scale and terrain

| # | Decision | Status |
|---|----------|--------|
| 5.1 | Ground scale ≈ **1" = 2 m**. *(AWoW)* | Settled |
| 5.2 | Boards vary from **18" × 18"** (boarding actions) to **9' × 5'**. *(AWoW)* | Settled |
| 5.3 | Move **6"** (5" in Armour 3); climbing or vaulting a low obstacle costs +2". Tuned so models can bound between cover. *(AWoW prototype)* | Leaning |
| 5.4 | Terrain density guideline per board size so a move can always reach cover. *(AWoW)* | Open — Phase 4 |
| 5.5 | **Ship interiors:** doors open/closed/locked; opening is an Interact (or 2" of a Move) and triggers reactions; locked doors yield to breaching charge, breaching tool (test 8+) or a *Tech* hack (test 9+). Compartments ≤ 6"×6" are confined. | Leaning — playtest |
| 5.6 | **Ship hazards are in the PoC:** zero-g (push-off movement, unbraced −1), decompression and vacuum, fire, darkness. | Settled |
| 5.7 | **Deck layouts (Phase 4, RJ):** board size set by vessel type; the dice decide what is built and the player only orients pieces that fit either way; PoC vessels are freighter / ore hauler, salvage tender, corvette / patrol craft and station module; the 3'×5' dry-erase map is overflow for big ships and rooms that don't fit. | Settled |
| 5.8 | **Layout procedure (chapter 11):** docking passage at the middle of the west edge; a spine of junction-and-passage pairs with turn rolls; the heart room at the far end; one branch roll per spine tile; fill to the vessel's minimum; room, second-door, door-state and hazard rolls; cover by room type. About 15–25 d6 rolls. Reference generator `tools/generate_layout.py`: over 2,000 decks per vessel, every deck connected and within 12 of each tile; 47–65% have a loop ([stats](../analysis/layout-stats.md)). | Proposed — playtest (exit: under 10 minutes at the table) |
| 5.9 | **Land battles (chapter 12, pulled forward by RJ for the 78th campaign):** terrain types (area, linear, solid, buildings, bunkers, craters and trenches, hills, water) with cover, movement and sight; difficult ground (each inch costs 2"); height (3" levels, height advantage against linear cover, falling); conditions (rain, haze, night); Loud noise heard within 24" in the open; land table setup by 12" sectors with d6 tables for urban, industrial, woodland, jungle, desert, farmland and fortified positions, and a density check. Solo-opponent lists reworded with area, approach and exit so they work aboard ship and on land. Vehicles stay post-PoC (6.11). | Proposed — playtest |

## 6. Equipment and force building

| # | Decision | Status |
|---|----------|--------|
| 6.1 | Weapons carry distinct profiles: damage pair, short range, AP, keywords (e.g. *Handy*, *Auto*). Starting list per prototype v0.1. *(AWoW)* | Leaning |
| 6.2 | Armour 0–3: None, Flak, Composite plate, Heavy plate. *(AWoW prototype)* | Leaning |
| 6.3 | Tiered equipment so mixed collections share a table. *(AWoW)* | Settled |
| 6.4 | Forces built with a campaign **requisition budget**; points optional. *(AWoW)* | Settled |
| 6.5 | Player forces are built from **teams** (tables of organisation) with fixed role slots and per-model weapon and gear options: VAT fireteam (5), Pathfinder recon team (4), Control team (2). | Proposed |
| 6.6 | PoC player forces run **3 to 8 models**; the default start is the **Boarding element** (one VAT fireteam plus one attached Pathfinder or Marksman, 6 models). Expect wider ranges as the game develops. | Settled |
| 6.7 | Task Force Boone profiles per [troops.yaml](../../content/troops.yaml): VAT and Pathfinder troopers Regular, leaders and marksmen Veteran; VATs Void plate (2), Pathfinders and marksmen Void suits (1); Pathfinders Move 7. | Proposed |
| 6.8 | New weapons: **heavy rifle** (2d8, AP 1; Pathfinders) and **compact LMG** (2d8, short 18", Handy, Auto, Suppressing; VAT gunner). | Leaning — playtest |
| 6.9 | Gear list ([gear.yaml](../../content/gear.yaml)): breaching charge, breaching tool, trauma kit (Medic), hacking rig (Tech), low-light optics, extinguisher, silencer (Silenced). Plus **jump pack** and **jammer**. The list will grow; the app will hold full explanations of gear and special rules. | Settled — playtest |
| 6.10 | **Automated** keyword: no Suppression, breaking, casualty shock or Rally; a wound row of 6+ destroys it. A model with no melee weapon can't start close combat and loses any exchange it's forced into. | Settled |
| 6.11 | Razorback drone (BLKOUT) deferred to the vehicles phase, after the PoC. | Settled |
| 6.12 | **Troop and unit builder (RJ):** players can build custom troop types and units. Needs a troop value (points) per attribute, skill, weapon, armour and gear, calibrated with the simulator, and checks against the Shoot scale (3.21) and content schema. Paper rules in Phase 7 (recruiting and requisition use the same values); app builder screen in Phase 9. | Proposed |

## 7. Campaign and progression

| # | Decision | Status |
|---|----------|--------|
| 7.1 | Awards / veterancy; soldiers, squads and the whole force progress (reputation, unit honors). *(AWoW)* | Settled |
| 7.2 | Hybrid heroes: the player may name a starting leader; others can surpass them. *(AWoW)* | Settled |
| 7.3 | Missions have consequences that carry forward; decisions are tracked across missions. *(AWoW)* | Settled |
| 7.4 | Force-level "doctrines" unlocked by campaign resources (Zone Raiders–style). *(AWoW teardown)* | Proposed — Phase 7 |
| 7.5 | The PoC includes the full campaign loop. | Settled |

## 8. Solo play and AI

| # | Decision | Status |
|---|----------|--------|
| 8.1 | PoC solo AI = **table-driven behavior profiles** per enemy type, building on the prototype's activation-priority list and role priorities. | Settled |
| 8.2 | The app generates enemies and gives **general play guidance** per enemy type; it does not run enemies turn by turn. | Settled |
| 8.3 | **Hidden contacts:** enemy tokens start as unknown contacts, revealed on line of sight or noise, rolled on a contact table per threat tier. *(AWoW teardown)* | Leaning — playtest ([opposition](opposition-force.md)) |
| 8.4 | **Alert track (0–6)** driven by noise; contacts converge at 2, contractor reaction force at 4, lockdown at 6. Adapted from Zero Dark's deck clock. *(AWoW teardown)* | Leaning — playtest ([opposition](opposition-force.md)) |
| 8.6 | Cowed enemies may **surrender**; prisoners are a campaign resource and a tactical burden. | Leaning — playtest |
| 8.5 | Longer term: a more organic, situation-reactive AI. | Leaning |
| 8.7 | **Fixed priority lists (Phase 3):** each behavior profile has top-down lists for Target in sight, No target, Reactions and Close combat; take the first line that applies. Fixed activation order, target choice, movement and tie-breakers leave nothing to judgment. Chapter 10, `content/behaviors.yaml`, behavior cards. | Settled (approach, RJ); lists Proposed — playtest |
| 8.8 | **Awareness and noise:** elements are Unaware (no activation or reactions), Alerted (act toward a single last-known-position marker) or Engaged (a target in sight). Noise is Quiet, Loud (heard within 12") or Very loud (ship-wide). An element that sees a player model or is hit raises the alarm at the end of its next activation unless silenced first. From Gym 1: the enemy ignored noise. | Proposed — playtest |
| 8.9 | **Drill by training:** an element's coordination comes from its lowest rank (Green none; Regular fire and manoeuvre; Veteran bounding, grenades and crack and throw; Elite assault drill and focus fire), raised one step by a friendly Leader within 6". | Settled (approach, RJ); details Proposed — playtest |
| 8.10 | **Close quarters:** an Engaged model with a player model within 6" checks outnumbered (Move away), charge (Fight 2+, Aggressive or Tactical), frag on two or more, then its profile. From Gym 1: judgment calls in the cargo hold. | Proposed — playtest |
| 8.11 | **Surrender:** Cowed and Saboteur models test 2d6 + Nerve, 8+ when Pinned or Wounded with a player model within 6"; on a fail they surrender (prisoners if the mission allows). Implements 8.6. | Proposed — playtest |
| 8.12 | **Recovery under guard and capture (Gym 1 replay):** a Down model in base contact with a standing enemy does not test to recover; if the mission allows prisoners, that enemy may Interact to capture a Down, Bleeding or Critical model. | Settled (RJ) |

## 9. Proof of concept

| # | Decision | Status |
|---|----------|--------|
| 9.1 | PoC = **printed rules and tables** played with miniatures, plus an app that generates layout, mission and enemies. | Settled |
| 9.2 | First environment: **ship / station boarding actions**. | Settled |
| 9.3 | Terrain inventory: **12 each** of 6"×6", 6"×3" and 3"×3" tiles; walls and doors/hatches from Kill Team and 40k Boarding Actions; a 3' × 5' dry-erase map. Enemy placement via tokens / poker chips. | Settled |
| 9.4 | Player faction: **Task Force Boone** (BLKOUT miniatures). Opposition from BLKOUT and BPRE miniatures. | Settled |
| 9.5 | Opposition: **Freespace Solutions**, a privatized debt and salvage corporation that unofficially conducts grey-zone operations for paying patrons. Three layers (patron, corporate core, contractors), element profiles, a miniature mapping across all BLKOUT/BPRE kits, and four threat tiers ([opposition-force.md](opposition-force.md)). | Settled |
| 9.6 | Freespace's current patron is the **Meridian Directorate**, a technocratic state run through its intelligence ministries. | Settled |
| 9.8 | Freespace, contractor and Directorate troop profiles and element compositions per [troops.yaml](../../content/troops.yaml) and [factions.yaml](../../content/factions.yaml). | Proposed |
| 9.7 | Campaign thread: intelligence from Freespace operations builds toward **attributing** Freespace to the Meridian Directorate, which can escalate into a deniable war with the Directorate (peer naval-infantry foe). | Leaning |

## 10. Software

| # | Decision | Status |
|---|----------|--------|
| 10.1 | The repo stores everything: rules (Markdown), game content data, tools and application code. | Settled |
| 10.2 | Architecture per [ADR-001](../architecture/adr-001-architecture.md). | Settled |
| 10.3 | The app must eventually serve many players over the internet with shared campaign state. The PoC is single-user but modeled for multiplayer from day one. | Settled |
| 10.4 | The combat simulator is C# (`app/src/Imperator.Sim`) and uses the same game-logic library as the app, so the rules math exists once. | Settled |
| 10.5 | Content YAML is compiled to `content/dist/content.json` (`tools/build_content.py`); .NET reads JSON with the built-in serializer, so the app has no YAML dependency. Rulebook tables are generated from the same YAML (`tools/render_tables.py`). | Settled |
| 10.6 | Target .NET 10 (current LTS). Tests are a dependency-free console runner; CI checks content, tables and tests on every push. | Settled |
