# 13. Missions

A mission turns a board into a fight with a purpose. It works the same aboard ship (chapter 11) and on land (chapter 12): roll a mission type and a threat tier, build the board, place the objective, an open guard and hidden contacts, then play until the force extracts or time runs out. The alert track and the mission clock drive the pressure.

The app's mission generator follows exactly these steps and tables.

## Building a mission

1. **Mission type.** The campaign names it, or roll:

<!-- table:missions.mission_roll -->
| d6 | Mission |
|---|---|
| 1–2 | Seize data or cargo |
| 3 | Recover personnel |
| 4 | Sabotage |
| 5–6 | Capture a high-value target |
<!-- /table -->

2. **Threat tier.** The campaign names it, or roll. Tier 4 is set only by the campaign.

<!-- table:missions.tier_roll -->
| d6 | Threat tier |
|---|---|
| 1–3 | 1 |
| 4–5 | 2 |
| 6 | 3 |
<!-- /table -->

3. **Board.** Build a deck (chapter 11) or a land table (chapter 12).

4. **Complication.** Roll once:

<!-- table:missions.complications -->
| d6 | Complication | Effect |
|---|---|---|
| 1–2 | **None** | No complication. |
| 3 | **Early warning** | The alert track starts at 1. |
| 4 | **Reinforced** | One more contact than the tier lists. |
| 5 | **Tight window** | The time limit is 5 minutes shorter. |
| 6 | **Hazard** | Aboard ship, the vessel's first hazard (chapter 11). On land, Night (chapter 12). |
<!-- /table -->

5. **Placement.**

<!-- table:missions.placement -->
1. Board: a chapter 11 deck aboard ship, a chapter 12 table on land. The mission names the vessel or environment, or roll.
2. Player force: deploys in the docking passage aboard ship, or within 6" of its edge on land. It extracts the same way: a model that leaves by the airlock or its own edge is out of the mission, safely.
3. Objective: as the mission type says. Mark it with the objective token (or two).
4. Guard: roll once on the tier's contact table, rerolling false alarms and booby traps, and place that element openly within 3" of the objective, Unaware. For a capture mission, add the HVT to the guard.
5. Contacts: place the tier's number of contact chips. Aboard ship, one in each compartment that is not the objective, farthest from the airlock first, then in junctions and small rooms, farthest first. On land, one in each sector of the far two rows that is not the objective's, farthest first, never within 12" of the player edge. If there are more contacts than places, go round again, farthest first. Number them.
6. Enemy entry and exit: aboard ship, the spine tile farthest from the airlock; a secondary airlock or escape pod is a second enemy exit. On land, the middle of the enemy's edge.
<!-- /table -->

6. **Size and time.** The mission sets its size (small or large), or the player force does; the size gives the clock die and the time limit (see the mission clock below).

7. **Briefing.** Write it up (see the briefing format below). The alert track starts at 0, and the clock at 0, unless a complication says otherwise.

## Mission types

<!-- table:missions.missions -->
### Seize data or cargo

*Reach the objective, take what it holds, and get it out.*

- **Objective aboard ship:** The heart (chapter 11).
- **Objective on land:** The middle sector of the far row (chapter 12).
- **Taking it:** A model in base contact may Interact to take it: automatic for a model with a hacking rig (data) or Physique 2 or more (cargo); 2d6 + Tech (data) or Physique (cargo), 8+, for anyone else. Data travels with the model that copied it; cargo is carried, and the carrier has −2" Move. If the carrier goes Down, a model in base contact may take it with an Interact.
- **Lockdown (alert 6):** The enemy locks it down: taking the objective now needs two successful Interacts, and a Hack of Secured (9+) first if it is data.

| Grade | Result |
|---|---|
| **Decisive** | Objective extracted and no player model Dead. |
| **Marginal** | Objective extracted. |
| **Failure** | Objective not extracted when time runs out. |

### Recover personnel

*Find a captured friend or a hostage, free them, and bring them out alive.*

- **Objective aboard ship:** The compartment farthest from the airlock by route, other than the heart; the heart if there is no other.
- **Objective on land:** A sector of the far row: roll a die, 1–2 left, 3–4 middle, 5–6 right.
- **Taking it:** The hostage (unit card: Hostage) starts bound in the objective. A player model in base contact frees them with an Interact. From then on the hostage is a player model: it activates after the model that freed it, every round, and must end each activation within 3" of a player model if it can.
- **Lockdown (alert 6):** The enemy moves the hostage: at the end of the round, the nearest enemy model within 6" of a bound hostage moves it up to 6" toward the enemy's exit, with it.

| Grade | Result |
|---|---|
| **Decisive** | Hostage extracted alive and no player model Dead. |
| **Marginal** | Hostage extracted alive. |
| **Failure** | Hostage Dead, still held, or not extracted when time runs out. |

### Sabotage

*Destroy two targets and get out.*

- **Objective aboard ship:** The heart, and the compartment farthest from it by route.
- **Objective on land:** Two sectors of the far row: the left and right ones.
- **Taking it:** A model in base contact sets a charge with an Interact (models without breaching charges use the target's own systems: same Interact). The target is destroyed at the end of that round (Very loud), unless an enemy model is in base contact with it then, which disarms the charge.
- **Lockdown (alert 6):** The enemy guards the targets: place one new contact within 3" of each target still standing.

| Grade | Result |
|---|---|
| **Decisive** | Both targets destroyed and the force extracted with no player model Dead. |
| **Marginal** | One target destroyed and the force extracted. |
| **Failure** | No target destroyed, or the force breaks. |

### Capture a high-value target

*Take a named enemy alive and bring them out.*

- **Objective aboard ship:** The heart.
- **Objective on land:** The middle sector of the far row.
- **Taking it:** The high-value target (HVT) is the tier's HVT model, placed at the objective with the guard. Take it by overpowering it in close combat, capturing it while Down (chapter 7), or when it surrenders: the HVT tests to surrender (chapter 10) whenever it is Pinned or Wounded with a player model within 6" and no friend within 6". A prisoner moves with a player model in base contact, which halves that model's Move.
- **Lockdown (alert 6):** The HVT runs: from now on it activates first each enemy turn, using the Handler profile's escape line toward the enemy's exit. If it leaves the board, it has escaped.

| Grade | Result |
|---|---|
| **Decisive** | HVT extracted as a prisoner and no player model Dead. |
| **Marginal** | HVT extracted as a prisoner, or Dead (with the body or its data recovered: an Interact in base contact). |
| **Failure** | HVT escaped or still at large when time runs out. |
<!-- /table -->

## The enemy

Each tier draws its guard and its contacts from one d6 table. The elements, their troop profiles and their behavior are in chapter 9; the solo opponent runs them as chapter 10 says.

<!-- table:missions.tiers -->
| d6 | Tier 1 | Tier 2 | Tier 3 | Tier 4 |
|---|---|---|---|---|
| 1 | False alarm | False alarm | Booby trap | Booby trap |
| 2 | Salvage crew | Salvage crew | Collections enforcers | Liquidators |
| 3 | Salvage crew | Asset security | Repossession team | Directorate deniable operators |
| 4 | Asset security | Collections enforcers | Liquidators | UN reserve fireteam |
| 5 | Asset security | Salvage techs | Borz Bratva team | UN Pointmen |
| 6 | Recovery captain | Automated defences | Harlow veterans | Raid Force Alpha fireteam |
| Contacts | 3 | 4 | 4 | 5 |
| Reinforcement | Asset security | Collections enforcers | Contract reaction force | Raid Force Alpha fireteam |
| HVT | Recovery Captain | Recovery Captain | Ubiytsa Controller | Directorate Handler |
<!-- /table -->

<!-- table:missions.contact_results -->
| Result | Effect |
|---|---|
| **False alarm** | Nothing there: crew who fled, a ventilation fan, a stray animal. Remove the chip. |
| **Booby trap** | A tripwire charge: resolve a frag grenade centred on the chip (2d8, radius 2"). It is Very loud. Remove the chip. |
<!-- /table -->

### Contacts

Contacts are numbered chips (poker chips work). Until revealed, a contact is only a possibility: it blocks nothing, cannot be shot, and never reacts.

<!-- table:missions.contacts -->
1. A contact is revealed when a player model can see it, or ends a Move within 3" of it. Roll on the tier's contact table.
2. Place the element with its first model on the chip and the rest within 2", in cover where they can be; then remove the chip.
3. A revealed element is Unaware while the alert track is below 2, Alerted from 2 on. Either way, newly placed models cannot react until the current activation ends.
4. A contact that moves into sight of a player model is revealed at once.
<!-- /table -->

The high-value target (HVT) is used by capture missions. A *Leader* HVT raises its guard's drill (chapter 10).

## The alert track

Keep the alert track on the mission dashboard (or beside the board, 0 to 6, with a die or a cube). Noise from chapter 10 raises it, along with the alarm and missed check-ins on the mission clock.

<!-- table:missions.alert_raises -->
| Raises the alert track | By |
|---|---|
| Loud noise (chapter 10) | +1 |
| Very loud noise | +2 |
| An element raises the alarm | +1 |
| Each check-in the clock reaches or passes (a check-in is missed) | +1 |
<!-- /table -->

Each threshold takes effect once, the first time the track reaches it:

<!-- table:missions.alert_thresholds -->
| At | Effect |
|---|---|
| **2** | Contacts move: at the end of each round, every contact moves 6" toward the marker (or toward the player entry if there is no marker), by the shortest route. Unrevealed elements become Alerted when revealed. |
| **4** | Reinforcements: at the end of the round, the tier's reinforcement element arrives at the enemy's entry, Alerted. |
| **6** | Lockdown: the mission type's lockdown happens at the end of the round. |
<!-- /table -->

Noise still alerts elements directly (chapter 10). The track is the wider ship or position waking up: contacts closing in, help arriving, and the enemy locking down what you came for.

## The mission clock

There is no fixed number of rounds. Time passes unevenly: a round may be a minute of careful stacking at a door or several of crawling through ducts.

<!-- table:missions.clock_sizes -->
| Mission size | Player force | Clock die | Time limit |
|---|---|---|---|
| **Small mission** | Up to 8 player models | d4 | 20 minutes |
| **Large mission** | 9 or more player models | d6 | 30 minutes |
<!-- /table -->

<!-- table:missions.clock -->
- The mission sets its size, or the size of the player force does. The size gives the clock die and the time limit.
- The clock starts at 0. At the end of each round, roll the clock die: that many minutes pass. Then resolve the alert track's effects, including any check-in just passed.
- Check-ins fall every 5 minutes from minute 10 (10, 15, 20 and so on). Each check-in the clock reaches or passes raises the alert track by 1.
- When the clock reaches the time limit, time has run out: the mission ends.
<!-- /table -->

A small mission lasts about eight rounds, but the clock die decides: a run of high rolls can end it after five. Each round's end runs in this order: the clock roll and any check-ins, then the alert track's effects, then the end of the mission if time has run out.

## The mission dashboard

The dashboard is a one-page status sheet (`ImperatorBlack_mission-dashboard.pdf`, or the last page of a generated mission sheet). Slip it into a dry-erase sleeve. It holds the alert track and its thresholds, the clock with its check-ins marked, the objectives, the contact chips and what each turned out to be, the enemy elements, and the player models' state.

## Extraction and the end

- A player model that leaves by its airlock or its own edge is **extracted**. It takes no further part, and whatever it carries (data, cargo, a hostage or a prisoner in base contact) leaves with it.
- The mission ends when every player model is extracted, Down, Dead or captured, or when time runs out.
- Player models still on the board at the end are **left behind**. In a campaign (Phase 7), the enemy holds the field: models left behind Down are captured.
- Grade the mission with its type's table: **Decisive**, **Marginal** or **Failure**.

## The briefing

Every mission, rolled or written, gets a briefing in this order. Scenario sheets follow it.

<!-- table:missions.briefing -->
| Heading | Contents |
|---|---|
| **Tasking** | Mission type, and who gives the order. |
| **Situation** | Two or three sentences: where, who holds it, why now. |
| **Objective** | What to do, where it is, and how to take it. |
| **Opposition** | Faction and threat tier; the guard (known); the number of contacts (unknown). |
| **Ground** | Vessel and deck seed, or environment and table; conditions or hazard. |
| **Complication** | If any. |
| **Extraction and time** | The way out, the time limit and clock die, and the alert track start. |
| **Grades** | Decisive, Marginal and Failure. |
<!-- /table -->

## Example

A capture mission aboard a freighter at threat tier 1. It is printed as `ImperatorBlack_mission-ship-42-sheet.pdf` in `docs/print/missions/`, made by `tools/generate_mission.py ship --seed 42 --vessel freighter --mission capture --tier 1`.

1. **Type and tier:** the campaign names a capture mission at tier 1. The HVT is a recovery captain.
2. **Board:** the freighter deck, seed 42 (chapter 11). The heart is the bridge (C1).
3. **Complication:** the roll is 6, Hazard: the freighter's first hazard, a power failure. The whole deck is dark.
4. **Objective and guard:** the bridge. The guard roll is 5, asset security. Two officers and a gunner go within 3" of the objective with the captain, all Unaware. The captain is a Leader, so asset security drills one step better while the captain is within 6".
5. **Contacts:** tier 1 has three. They go in the compartments other than the bridge, farthest from the airlock first: C5, C4, then C3.
6. **Size and time:** the boarding element is six models, so this is a small mission: 20 minutes on a d4. Check-ins fall at minutes 10 and 15.
7. **Entry and exit:** the enemy's entry is the far end of the spine (P4). The boarding element deploys in the docking passage.

In play, the first unsilenced burst raises the alert track from 0 to 1. Clock rolls of 3, 4 and 3 bring it to minute 10, a check-in, which raises the alert to 2: the contacts start moving 6" a round toward the marker. A breaching charge on the bridge door is Very loud, which raises the alert to 4: a second asset security team arrives at P4 at the end of that round. If the track reaches 6 before the captain is taken, the captain runs for P4.
