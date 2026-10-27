# 14. Campaign

*Draft (Phase 7). Every table here is Proposed until a campaign has been played through.*

A campaign links missions into a story the force carries with it. Soldiers are wounded and recover, or don't. They earn experience and grow, and the ones who walk off the table are not the ones who walked on. The force answers to command, which judges it on results. Between missions it spends what it has won on replacements, recovery and its base: an outpost or a ship.

A campaign is played in **arcs**. An arc is a small map of enemy sites, about six missions deep, ending at a target. Each **campaign turn** is one mission. This chapter uses the Black Ops campaign type: command names the target and leaves the route to the force, as long as it keeps command's confidence.

The examples follow Task Force Boone against Freespace Solutions. Any player faction can run a campaign.

## Starting a campaign

<!-- table:campaign.start_text -->
- Force: the starting force for the faction (for Task Force Boone, the boarding element: one VAT fireteam and one attached specialist). Name every soldier.
- Roster: up to 12 soldiers, grouped by fireteam. Up to 8 deploy on a small mission (chapter 13), 12 on a large one.
- Force selection: by fireteam (the default) or by soldier. Choose now; it holds for the campaign.
- Base: the campaign names its kind, an outpost or a ship (Task Force Boone's arcs: its ship). Every facility starts at level 0; the threat track starts at 0.
- Resources: 6 Requisition, 1 Intel, Standing 3, Evidence 0.
- Arc: generate the first arc map. The campaign turn marker starts at 1.
<!-- /table -->

### Force selection

A campaign picks its force one of two ways, chosen at the start (decision 6.18). Two-player games always go by fireteam.

<!-- table:campaign.force_selection -->
| Setting | Deploying a force | Replacements |
|---|---|---|
| **By fireteam** (default) | Fireteams (standard, or built to their type: chapter 9), each fielding at least half its slots and its Leader, plus characters, each joining a fireteam or acting alone. Within the mission's TV and model limits. | A replacement fills an empty slot in its own fireteam. New fireteams join whole. |
| **By soldier** | Any soldiers fit for duty, within the mission's TV and model limits. Before deployment, group them into fireteams of up to 6 for breaking and Rally; characters join one or act alone. | A replacement may be any troop the force fields. |
<!-- /table -->

Keep the campaign on the campaign sheets: the force roster, and the campaign status sheet with the arc map (`ImperatorBlack_campaign-sheets.pdf`).

## Short arcs

A **short arc** is a story of three missions, written in advance: the opening, a second mission that depends on how the opening went, and a finale. It uses the force rules of this chapter (injuries, experience, advances) without the arc map, resources or base, so it plays in an evening or two. Short arcs are written for one faction against one opposition (chapter 13); their sheets are under `docs/scenarios/arcs/`.

<!-- table:campaign.short_arc -->
1. A short arc is three authored missions: an opening, a second mission chosen by the opening's grade, and a finale. The arc sheet lists them and what each grade leads to.
2. Roster: any troops of the faction you own. Name every soldier the first time they deploy; they carry their XP, advances and injuries through the arc.
3. Force: each mission sets a Troop Value limit (chapter 15) and a model limit. Before it, choose who deploys from the soldiers fit for duty, by fireteam or by soldier as the arc's force-selection setting says (the arc sheet names a default; choose at the start). Count each soldier at its profile's TV; advances are free.
4. After each mission: casualties and injuries, XP and advances as in a full campaign (chapter 14). An injury that puts a soldier out for turns puts them out for that many missions. A soldier left behind Down is captured and out for the rest of the arc, unless the arc gives a chance to bring them back.
5. Replacements: a Dead, captured or injured soldier who is not a character may be replaced by a new soldier of the same profile with no XP. A character who dies is gone for the arc.
6. Grades lead on: the arc sheet says which mission comes next and any edge or setback it carries (a bigger or smaller force, the alert track's start, contacts removed or added).
7. The arc's result is the finale's grade, one step better if the first two missions were both Decisive, one step worse if either was a Failure (Decisive, Marginal, Failure; never past either end).
<!-- /table -->

## The campaign turn

<!-- table:campaign.turn -->
| Step | What happens |
|---|---|
| **1. Tasking** | Choose a node to strike: any revealed node linked to the base or to a taken node. If the base is under attack, the mission is its defence instead (see The base). At Standing 2 or less, command chooses for you: the available node with the highest threat tier (ties: your pick). |
| **2. Preparation** | Collect income: Requisition equal to the Standing table. Spend Requisition (replacements, recovery, facilities) and Intel (node reveals and mission edges). Pick the support assets Standing allows. Choose the soldiers who deploy from those fit for duty. |
| **3. Mission** | Build the node's mission with chapter 13, using its mission type, vessel or environment and threat tier. Play it. |
| **4. After-action** | In order: casualties and injuries, prisoners, the grade's rewards and the node's outcome, XP and the citation, then advances. |
| **5. Upkeep** | Each recovering soldier ticks down one turn. Facility levels bought this turn are finished. Pay the base's upkeep. If the threat track is at its attack level, the base is attacked. Move the turn marker on; if the arc has run out of turns, it ends. |
| **6. Events** | Roll 2d6 on the events table. |
<!-- /table -->

## Resources

<!-- table:campaign.resources -->
| Resource | Track | What it does |
|---|---|---|
| **Requisition** | No limit | Buys replacements, faster recovery and base facilities. Unspent Requisition carries over. |
| **Threat** | 0–6 | How close the enemy is to finding and hitting the base. It rises with loud and failed missions and falls with quiet ones; at 4 or more the base is attacked (see The base). |
| **Intel** | 0–6 | Spent to reveal nodes and to shape a mission before it starts. Gained from data, prisoners and some nodes. |
| **Standing** | 0–6 | Command's confidence in the force. Sets income, support and latitude. At 0 the force is recalled and the campaign ends. |
| **Evidence** | 0–6 | The case that Freespace answers to the Meridian Directorate (decision 9.7). It is never spent. |
<!-- /table -->

### Standing

Standing is command's confidence in the force. It sets the Requisition the force draws each turn and the support command sends with a mission. It also decides who chooses where to strike: at low Standing, command stops trusting the force's judgment and picks the taskings itself.

<!-- table:campaign.standing -->
| Standing |  | Income | Support assets | Taskings |
|---|---|---|---|---|
| **0** | Recalled | 0 | — | The force is pulled out. The campaign ends. |
| **1** | Under review | 1 | — | Command chooses each tasking. |
| **2** | Doubted | 2 | — | Command chooses each tasking. |
| **3** | Trusted | 3 | — | The force chooses its taskings. |
| **4** | Valued | 3 | 1 | The force chooses its taskings. |
| **5** | Favoured | 4 | 1 | The force chooses its taskings. |
| **6** | Command's own | 4 | 2 | The force chooses its taskings. |
<!-- /table -->

Support assets are picked in Preparation, up to the number Standing allows. Each can be used once per mission.

<!-- table:campaign.support -->
| Support asset | Effect |
|---|---|
| **Insertion window** | The mission clock's time limit is 5 minutes longer. |
| **Signals cover** | The first check-in does not raise the alert track. |
| **Attached trooper** | One extra Regular soldier from the force's teams deploys on loan. Injuries apply to it, but it gains no XP and returns to command after the mission. |
| **Intel package** | 1 Intel to spend on this mission only. |
<!-- /table -->

### Intel

Intel is spent in Preparation, before the mission is built. Node reveals can be bought at any time in Preparation; each other use, once per mission.

<!-- table:campaign.intel_uses -->
| Intel | Use | Effect |
|---|---|---|
| 1 | **Reveal a node** | A hidden node shows its type. |
| 1 | **Their pattern** | The first check-in comes at minute 15, not 10. |
| 1 | **Eyes on a contact** | At setup, roll one contact chip's result and place that element openly, Unaware. |
| 1 | **Re-roll the complication** | Roll the mission's complication again; the second roll stands. |
| 2 | **Misdirection** | The threat track falls by 1 (once a turn). |
<!-- /table -->

## The arc map

<!-- table:campaign.arc_procedure -->
1. Draw the base on the left, then three node boxes in a column, two in the next and the target on the right. Number them 1 to 6, top to bottom, left to right; the target is node 6.
2. Link the base to nodes 1, 2 and 3. Link nodes 4 and 5 to the target.
3. For each of nodes 1, 2 and 3 roll a die: 1–3 link it to node 4, 4–5 to node 5, 6 to both. If node 4 or 5 has no link from the left, link it to the nearest node in column 1.
4. Roll each of nodes 1–5 on the node table; the target is an operations hub. Column 1 is threat tier 1, column 2 tier 2, the target tier 3.
5. Nodes 1–3 are revealed. Nodes 4, 5 and the target are hidden: mark them '?' and keep their rolls face down (or roll when revealed). A node is revealed when a node linked to it is taken, or for 1 Intel.
<!-- /table -->

<!-- table:campaign.nodes -->
| d6 | Node | Board | Mission | Reward when taken |
|---|---|---|---|---|
| 1 | **Salvage yard** | Salvage tender | Seize data or cargo | +2 Requisition. |
| 2 | **Freight run** | Freighter / ore hauler | Seize data or cargo | +1 Requisition and +1 Intel. |
| 3 | **Relay station** | Station module | Sabotage | Reveal every node on the map; +1 Intel. |
| 4 | **Safe house** | Urban (land) | Capture a high-value target | +1 Intel and +1 Evidence. |
| 5 | **Patrol corvette** | Corvette / patrol craft | Capture a high-value target | +1 Standing. |
| 6 | **Forward depot** | Industrial (land) | Sabotage | +2 Requisition; the next mission's threat tier is 1 lower (1 at least). |
| — | **Operations hub** | Roll a die: 1–3 ship (station module), 4–6 land (fortified position). | Capture a high-value target | The arc is won (see the end of the arc). |
| — | **Holding site** | As the node where the soldier was captured. | Recover personnel | The soldier comes home: roll for injury, +1. |
<!-- /table -->

A node's mission is built with chapter 13, with the mission type, the vessel or environment and the threat tier named, not rolled. If the vessel or environment doesn't suit the table you have, use another. Only the mission type and tier matter to the campaign.

### Taking nodes

<!-- table:campaign.arc_taking -->
- A node is taken when its mission is graded Decisive or Marginal. Collect the node's reward.
- A Failure leaves the node standing; its threat tier rises by 1 (to 4 at most). It may be struck again.
- A node can be struck if it is revealed and linked to the base or to a taken node.
<!-- /table -->

### The end of an arc

An arc lasts **8 campaign turns**.

<!-- table:campaign.arc_end -->
- The arc is won when the target is taken: Decisive if its mission was Decisive, otherwise Marginal. Gain 2 Evidence and 1 Standing.
- The arc is lost if the turn marker passes the last turn: the network relocates. Lose 1 Standing. Evidence stays.
- Between arcs: soldiers keep XP, advances, injuries and recovery; resources carry over; generate a new map. A campaign is usually one to three arcs.
<!-- /table -->

## After-action

Work through the after-action step in this order.

### 1. Casualties and injuries

<!-- table:campaign.casualties -->
- Dead: the soldier's name goes on the force's roll of honour.
- Down at any point in the mission, and alive at the end: roll on the injury table.
- Left behind Down: captured. A holding site node joins the map, linked to the node where it happened, at the same threat tier, revealed. Strike it to bring the soldier home.
- Left behind standing: the soldier makes their own way back and is out for 1 turn.
- Wounded markers alone leave no lasting injury.
<!-- /table -->

<!-- table:campaign.injury_modifiers -->
| Modifier | When |
|---|---|
| -1 | The soldier was Bleeding or Critical during the mission |
| +1 | A Medic action saved or treated the soldier during the mission (chapter 7) |
| +1 | Medical facility level 2 or 3 |
<!-- /table -->

<!-- table:campaign.injuries -->
| 2d6 | Injury | Out | Effect |
|---|---|---|---|
| 2 | **Dies of wounds** | — | The soldier dies. Roll of honour. |
| 3 | **Crippled** | 3 turns | Permanently −1 to an attribute: roll a die, 1 Move (−1"), 2 Shoot, 3 Fight, 4 Physique, 5 Nerve, 6 Tech (0 at least). Out 3 turns. |
| 4–5 | **Severe wound** | 2 turns | Out 2 turns. |
| 6–8 | **Wounded** | 1 turn | Out 1 turn. |
| 9–10 | **Patched up** | 0 | Fit for the next mission. |
| 11–12 | **Close call** | 0 | Fit for the next mission, and +1 XP. |
<!-- /table -->

A total of 2 or less reads as 2, and 12 or more as 12. A soldier who is **out** misses that many campaign turns, counting down at Upkeep. A soldier out for 1 turn misses the next mission.

### 2. Prisoners and rewards

<!-- table:campaign.rewards -->
| Result | Reward |
|---|---|
| **Decisive** | +3 Requisition, +1 Standing. The node is taken. |
| **Marginal** | +1 Requisition. The node is taken. |
| **Failure** | −1 Standing. The node stands, its threat tier 1 higher (4 at most). |
| **HVT extracted as a prisoner** | +2 Intel and +1 Evidence. |
| **Each other prisoner extracted** | +1 Intel (2 at most per mission). |
| **Data seized (seize mission)** | +1 Intel; +1 Evidence as well at threat tier 3 or 4. |
| **Two or more soldiers Dead** | −1 Standing. |
<!-- /table -->

The node's own reward (the node table) comes on top when the node is taken.

### 3. Experience

Each soldier who deployed earns XP:

<!-- table:campaign.xp -->
| XP | For |
|---|---|
| +1 | Deployed on the mission |
| +1 | Did the objective's action: took it, freed the hostage, set a charge that destroyed a target, or took the HVT |
| +1 | Put one or more enemy models Down or worse, or made one surrender |
| +1 | The mission was graded Decisive |
| +1 | The citation: one soldier per mission, your pick, for the deed of the mission (record it) |
<!-- /table -->

### 4. Advances

<!-- table:campaign.advances -->
- Each time a soldier reaches 5, 10, 15 (and so on) XP, roll a die on the advance table. If the result is already at its cap, choose another row.
- Rank follows advances: a Regular becomes a Veteran at 3 advances and an Elite at 6 (a Green soldier becomes Regular at 1). Rank is a label (chapter 1).

| d6 | Advance |
|---|---|
| 1 | +1 Shoot (4 at most; 4 is legendary) |
| 2 | +1 Fight (4 at most) |
| 3 | +1 Physique (3 at most) |
| 4 | +1 Nerve (4 at most) |
| 5 | +1 Tech (3 at most), or +1" Move (once per soldier) |
| 6 | A skill: Leader, Medic, Marksman (next level), Scout or Low-light; or choose any row above |
<!-- /table -->

### Citations and honours

<!-- table:campaign.honours -->
| Honour | How |
|---|---|
| **Medal** | A soldier with three citations is decorated: +1 Standing (once per soldier), and the name goes on the honour roll. |
| **Unit honour** | A team earns a mark when all its fit soldiers deploy and the mission is Decisive. At 3 marks it takes one honour from the list below, for good. |

| Unit honour | Effect |
|---|---|
| **Breachers** | +1 to the team's tests to set charges and open or force doors. |
| **Ghosts** | The first Loud noise the team makes in a mission does not raise the alert track. |
| **Steady** | +1 to the team's breaking tests. |
| **Hunters** | +1 to hit Unaware models. |
<!-- /table -->

## The base

The force has one home base, chosen by the campaign: an **outpost** on the ground or a **ship** that carries it (decision 7.14). The base is a place on the map, and the enemy can find it and attack it.

<!-- table:campaign.base_kinds -->
| Base | What it is |
|---|---|
| **Outpost** | A base on the ground. It stays where it is on the map. It is defended on a land table of the campaign's environment. |
| **Ship** | A ship that carries the force. It is the map's starting point wherever the arc is; it is defended aboard its own deck, against boarders. |
<!-- /table -->

<!-- table:campaign.base_rules -->
- The base is a place on the map: the arc map's starting point. Nodes linked to it can be struck.
- It has six facilities of three levels each. A level costs 4, 6 or 8 Requisition, is bought in Preparation and works from the next turn. Levels are bought in order; each includes the ones below it.
- Upkeep: at Upkeep, pay 1 Requisition for every 3 facility levels the base has (rounded down). If you can't, choose one facility: it doesn't work until the upkeep is paid.
- An outpost's facilities and a ship's are the same; on a ship, Defences are its internal security.
<!-- /table -->

### Facilities

<!-- table:campaign.facilities -->
| Facility | Level | Requisition | Effect |
|---|---|---|---|
| **Medical** | 1 | 4 | Out-of-action times are 1 turn shorter (0 at least). |
|  | 2 | 6 | +1 to injury rolls. |
|  | 3 | 8 | Once per turn, re-roll one injury roll. |
| **Armoury** | 1 | 4 | Each mission, one soldier may carry one extra gear item from the gear list. |
|  | 2 | 6 | Each mission, one soldier may swap its weapon for an LMG, DMR or AT launcher. |
|  | 3 | 8 | Both of the above for two soldiers each mission. |
| **Intelligence and comms** | 1 | 4 | +1 Intel after each mission graded Decisive or Marginal. |
|  | 2 | 6 | One Intel use each mission is free. |
|  | 3 | 8 | At the start of each turn, reveal one hidden node. |
| **Workshop** | 1 | 4 | Facility levels cost 1 Requisition less (3, 5 and 7). |
|  | 2 | 6 | Salvage: an Automated soldier (a robot or drone) destroyed in a mission rolls on the injury table as if it had gone Down; Dies of wounds means it is scrapped. |
|  | 3 | 8 | After a defence or recapture mission, rebuild one lost facility level free. |
| **Barracks** | 1 | 4 | The roster holds up to 14 soldiers. |
|  | 2 | 6 | Replacements join with 2 XP. |
|  | 3 | 8 | The roster holds up to 16 soldiers, and fast-track recovery costs 1 Requisition. |
| **Defences** | 1 | 4 | Defence missions: before the enemy arrives, set up two pieces of low wall or sandbags (aboard ship: barricades in passages), each within 6" of an objective. |
|  | 2 | 6 | Defence missions: an Automated turret (Freespace turret profile) on Overwatch guards each objective; it is a player model. |
|  | 3 | 8 | Early warning: the base is attacked at threat 5 or more instead of 4, and the first attackers arrive at the end of round 2 instead of round 1. |
<!-- /table -->

### The threat track

The threat track (0 to 6) is how close the enemy is to finding and hitting the base. It starts at 0.

<!-- table:campaign.threat -->
| Threat track (0–6) | Change |
|---|---|
| A mission ends with the alert track at 4 or more | +1 |
| A mission graded Failure | +1 |
| Each soldier captured | +1 |
| The Leak event (2) | +1 |
| A mission graded Decisive with the alert track at 1 or less | −1 |
| Misdirection: spend 2 Intel in Preparation (once a turn) | −1 |
<!-- /table -->

### Attacks on the base

<!-- table:campaign.base_attacks -->
- Threat: at Upkeep, if the threat track is at 4 or more (5 with Defences 3), the enemy attacks the base.
- Events: some events raise the threat track; a campaign may add events that attack the base outright.
- Campaign: a bespoke campaign may name points where the base is attacked, whatever the threat track says.
- An attack makes the next campaign turn's mission the base's defence: there is no tasking choice. Its threat tier is the highest tier among nodes linked to the base (1 at least), 1 higher if the threat track is 6 (4 at most).
- After the defence, set the threat track by the result (below).
<!-- /table -->

### Defending the base

The defence is a tabletop mission with its own setup. Everything not changed here works as in chapter 13.

<!-- table:campaign.defence -->
**Board.**

- Outpost: a small land table of the campaign's environment (chapter 12), rolled as usual, except sectors A1, B1 and C1: the outpost. Put a small building (the command post) in the middle of B1, and a second piece of the player's choice (a hangar, a mast, a store) in A1 or C1. Each is an objective.
- Ship: a deck of the ship's vessel (chapter 11). The enemy boards at the airlock. The objectives are the heart and the compartment farthest from it by route.

**Deployment.**

- The player deploys first: every model within 6" of an objective (aboard ship: in an objective's compartment or a tile next to it), with any Defences.
- Doors aboard ship start as the deck rolls them; the player may close any door next to a model.

**The enemy.**

- The attack: roll twice on the threat tier's contact table (chapter 13; reroll false alarm and booby trap). Both elements arrive at the end of round 1 at the enemy entry (land: the middle of the far edge; ship: the airlock), Alerted, with the marker on the nearest objective.
- Waves: at each check-in on the clock, roll once more; that element arrives the same way.
- Attackers whose profile is Sentry, Cowed or Saboteur use the Aggressive profile instead. The enemy's objective (for tie-breakers and the marker) is the nearest objective still held.
- There is no alert track: the alarm has already sounded. The clock is a small mission's: 20 minutes on a d4 (chapter 13).

**Losing an objective.** An objective is lost at the end of a round in which an enemy model is in base contact with it (aboard ship: in its compartment) and no standing player model is within 3" of it (aboard ship: in that compartment). A player model retakes a lost objective the same way: in base contact at the end of a round, with no standing enemy model within 3" (aboard ship: in that compartment).

| Grade | Result |
|---|---|
| **Decisive** | Both objectives held when time runs out or the attack is broken, and no player model Dead. |
| **Marginal** | At least one objective held. |
| **Failure** | Both objectives lost, or the force breaks. |
<!-- /table -->

<!-- table:campaign.base_outcomes -->
| Defence | Afterwards |
|---|---|
| **Decisive** | The attack is beaten. Threat track to 0; +1 Standing. |
| **Marginal** | The base holds, damaged: it loses one facility level (its highest; ties, your choice). Threat track to 1. |
| **Failure** | The base is overrun: -1 Standing. No facility works and no Requisition can be spent on the base until it is retaken. The next campaign turn's mission is its recapture. |
<!-- /table -->

### Recapture

<!-- table:campaign.recapture -->
- The recapture is a seize mission (chapter 13) at the attack's threat tier, on the base's own board: the objective is the command post (outpost) or the heart (ship), the guard is rolled on the tier's table as usual, and the alert track starts at 1.
- Decisive: the base is retaken intact. Threat track to 1.
- Marginal: retaken, but it loses one more facility level. Threat track to 1.
- Failure: the base is lost: -1 Standing. Command assigns a new base of the same kind with every facility at level 0 (an outpost on any taken node, or the map's starting point; a replacement ship). Threat track to 0.
<!-- /table -->

## Requisition

The force is built like a table of organisation, not from a points list. Requisition replaces soldiers within the force's fireteams and pays for recovery and the base. A replacement costs its Requisition: its Troop Value ÷ 10, rounded up (chapter 15).

<!-- table:campaign.costs -->
| Item | Requisition |
|---|---|
| Replacement soldier (0 XP): by fireteam, a troop that fills an empty slot in its fireteam; by soldier, any troop the force fields | Its Requisition (chapter 15) |
| New fireteam (a standard fireteam, or one built to its type, chapter 9), at least half its slots and its Leader | Its soldiers' Requisition |
| Fast-track recovery: one recovering soldier is fit a turn sooner | 2 |
| Base facility level | 4 / 6 / 8 by level (1 less each with a Workshop) |
<!-- /table -->

A replacement joins with the troop profile's attributes and kit, its listed rank and 0 XP. Name them.

## Events

At the end of each campaign turn, roll 2d6.

<!-- table:campaign.events -->
| 2d6 | Event | Effect |
|---|---|---|
| 2 | **Leak** | The enemy knows you are coming: the target's threat tier is 1 higher (4 at most), and the threat track rises 1. |
| 3 | **Supply shortfall** | Lose 2 Requisition (0 at least). |
| 4 | **Enemy reinforces** | The next mission's threat tier is 1 higher (4 at most). |
| 5 | **Questions from command** | Lose 1 Standing, unless you spend 2 Requisition. |
| 6 | **Downtime** | One recovering soldier is fit a turn sooner. |
| 7 | **Quiet** | Nothing happens. |
| 8 | **Informant** | +1 Intel. |
| 9 | **Supply drop** | +2 Requisition. |
| 10 | **Volunteers** | One free Regular replacement for an empty slot in one of the force's fireteams (any troop the force fields, if picking by soldier), if the roster has room. |
| 11 | **Opportunity** | A new node joins the map: roll its type; threat tier 1; linked to the base or to any taken node; revealed. |
| 12 | **Commendation** | +1 Standing. |
<!-- /table -->

## Evidence

<!-- table:campaign.evidence -->
- Evidence comes from the after-action rewards, safe houses and winning an arc. It is never spent.
- At 6 Evidence, the Directorate is exposed: the next arc is against the Meridian Directorate. Its column 1 is threat tier 2, column 2 tier 3, and the target tier 4.
<!-- /table -->

## Example

Task Force Boone begins with the boarding element: a VAT fireteam (leader, two assaulters, gunner, data specialist) and a Marksman, six soldiers. It has 6 Requisition, 1 Intel and Standing 3.

**The map.** For nodes 1, 2 and 3 the link rolls are 2, 6 and 5: node 1 links to node 4, node 2 to both, node 3 to node 5. The node rolls are 2, 6 and 3 in column 1: a freight run, a forward depot and a relay station, all threat tier 1 and revealed. Nodes 4 and 5 (tier 2) and the operations hub (tier 3) are hidden.

**Turn 1.** Standing 3 gives 3 Requisition (9 in all). The force strikes the relay station: if taken, it reveals the whole map. Chapter 13 builds a sabotage mission on a station module at threat tier 1. The force wins it Marginal: one target destroyed. In the fight one assaulter went Down and was Bleeding until the other assaulter's Medic action, with its trauma kit, saved it. Its injury roll is 2d6 −1 for Bleeding +1 for the Medic: 5, a severe wound, out 2 turns. The relay is taken: +1 Requisition for the Marginal grade, +1 Intel from the node, and every node is revealed. Node 5 is a safe house. XP: every soldier gets +1 for deploying. The gunner, whose charge took out the target, gets +1 more, and +1 more again for the citation. The events roll is 8: an informant, +1 Intel.

**Turn 2.** Standing 3 gives 3 Requisition (13). The assaulter is out. The force spends 6 on a replacement VAT Assaulter (TV 53, Requisition 6) to keep the fireteam full, and 4 on the ship's Medical facility (level 1), so injuries from turn 3 on heal a turn faster. That leaves 3. Then it picks its next strike: the safe house (node 5), for the Evidence.
