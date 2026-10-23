# 14. Campaign

*Draft (Phase 7). Every table here is Proposed until a campaign has been played through.*

A campaign links missions into a story the force carries with it. Soldiers are wounded and recover, or don't. They earn experience and grow, and the ones who walk off the table are not the ones who walked on. The force answers to command, which judges it on results. Between missions it spends what it has won on replacements, recovery and its ship.

A campaign is played in **arcs**. An arc is a small map of enemy sites, about six missions deep, ending at a target. Each **campaign turn** is one mission. This chapter uses the Black Ops campaign type: command names the target and leaves the route to the force, as long as it keeps command's confidence.

The examples follow Task Force Boone against Freespace Solutions. Any player faction can run a campaign.

## Starting a campaign

<!-- table:campaign.start_text -->
- Force: the starting force for the faction (for Task Force Boone, the boarding element: one VAT fireteam and one attached specialist). Name every soldier.
- Roster: up to 12 soldiers. Up to 8 deploy on a small mission (chapter 13), 12 on a large one.
- Resources: 6 Requisition, 1 Intel, Standing 3, Evidence 0.
- Arc: generate the first arc map. The campaign turn marker starts at 1.
<!-- /table -->

Keep the campaign on the campaign sheets: the force roster, and the campaign status sheet with the arc map (`ImperatorBlack_campaign-sheets.pdf`).

## Short arcs

A **short arc** is a story of three missions, written in advance: the opening, a second mission that depends on how the opening went, and a finale. It uses the force rules of this chapter (injuries, experience, advances) without the arc map, resources or ship, so it plays in an evening or two. Short arcs are written for one faction against one opposition (chapter 13); their sheets are under `docs/scenarios/arcs/`.

<!-- table:campaign.short_arc -->
1. A short arc is three authored missions: an opening, a second mission chosen by the opening's grade, and a finale. The arc sheet lists them and what each grade leads to.
2. Roster: any troops of the faction you own. Name every soldier the first time they deploy; they carry their XP, advances and injuries through the arc.
3. Force: each mission sets a Troop Value limit (chapter 15) and a model limit. Before it, choose who deploys from the soldiers fit for duty. Count each soldier at its profile's TV; advances are free.
4. After each mission: casualties and injuries, XP and advances as in a full campaign (chapter 14). An injury that puts a soldier out for turns puts them out for that many missions. A soldier left behind Down is captured and out for the rest of the arc, unless the arc gives a chance to bring them back.
5. Replacements: a Dead, captured or injured soldier of a line profile (a clone trooper, not a named character) may be replaced by a new soldier of the same profile with no XP. A named character who dies is gone for the arc.
6. Grades lead on: the arc sheet says which mission comes next and any edge or setback it carries (a bigger or smaller force, the alert track's start, contacts removed or added).
7. The arc's result is the finale's grade, one step better if the first two missions were both Decisive, one step worse if either was a Failure (Decisive, Marginal, Failure; never past either end).
<!-- /table -->

## The campaign turn

<!-- table:campaign.turn -->
| Step | What happens |
|---|---|
| **1. Tasking** | Choose a node to strike: any revealed node linked to the ship or to a taken node. At Standing 2 or less, command chooses for you: the available node with the highest threat tier (ties: your pick). |
| **2. Preparation** | Collect income: Requisition equal to the Standing table. Spend Requisition (replacements, recovery, ship upgrades) and Intel (node reveals and mission edges). Pick the support assets Standing allows. Choose the soldiers who deploy from those fit for duty. |
| **3. Mission** | Build the node's mission with chapter 13, using its mission type, vessel or environment and threat tier. Play it. |
| **4. After-action** | In order: casualties and injuries, prisoners, the grade's rewards and the node's outcome, XP and the citation, then advances. |
| **5. Upkeep** | Each recovering soldier ticks down one turn. Ship upgrades bought this turn are finished. Move the turn marker on; if the arc has run out of turns, it ends. |
| **6. Events** | Roll 2d6 on the events table. |
<!-- /table -->

## Resources

<!-- table:campaign.resources -->
| Resource | Track | What it does |
|---|---|---|
| **Requisition** | No limit | Buys replacements, faster recovery and ship upgrades. Unspent Requisition carries over. |
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
<!-- /table -->

## The arc map

<!-- table:campaign.arc_procedure -->
1. Draw the ship on the left, then three node boxes in a column, two in the next and the target on the right. Number them 1 to 6, top to bottom, left to right; the target is node 6.
2. Link the ship to nodes 1, 2 and 3. Link nodes 4 and 5 to the target.
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
- A node can be struck if it is revealed and linked to the ship or to a taken node.
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
| +1 | Medical bay level 2 or 3 |
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

## The ship

The force's ship is its home between missions. Three upgrade tracks improve it. A level is bought in Preparation and is ready from the next turn. Levels are bought in order, and each includes the ones below it.

<!-- table:campaign.ship -->
| Upgrade | Level | Requisition | Effect |
|---|---|---|---|
| **Medical bay** | 1 | 4 | Out-of-action times are 1 turn shorter (0 at least). |
|  | 2 | 6 | +1 to injury rolls. |
|  | 3 | 8 | Once per turn, re-roll one injury roll. |
| **Armoury** | 1 | 4 | Each mission, one soldier may carry one extra gear item from the gear list. |
|  | 2 | 6 | Each mission, one soldier may swap its weapon for an LMG, DMR or AT launcher. |
|  | 3 | 8 | Both of the above for two soldiers each mission. |
| **Intelligence cell** | 1 | 4 | +1 Intel after each mission graded Decisive or Marginal. |
|  | 2 | 6 | One Intel use each mission is free. |
|  | 3 | 8 | At the start of each turn, reveal one hidden node. |
<!-- /table -->

## Requisition

The force is built like a table of organisation, not from a points list. Requisition replaces soldiers within the force's teams and pays for recovery and the ship. A replacement costs its Requisition: its Troop Value ÷ 10, rounded up (chapter 15).

<!-- table:campaign.costs -->
| Item | Requisition |
|---|---|
| Replacement soldier (any troop in the force's teams, 0 XP) | Its Requisition (chapter 15) |
| Fast-track recovery: one recovering soldier is fit a turn sooner | 2 |
| Ship upgrade | 4 / 6 / 8 by level |
<!-- /table -->

A replacement joins with the troop profile's attributes and kit, its listed rank and 0 XP. Name them.

## Events

At the end of each campaign turn, roll 2d6.

<!-- table:campaign.events -->
| 2d6 | Event | Effect |
|---|---|---|
| 2 | **Leak** | The enemy knows you are coming: the target's threat tier is 1 higher (4 at most). |
| 3 | **Supply shortfall** | Lose 2 Requisition (0 at least). |
| 4 | **Enemy reinforces** | The next mission's threat tier is 1 higher (4 at most). |
| 5 | **Questions from command** | Lose 1 Standing, unless you spend 2 Requisition. |
| 6 | **Downtime** | One recovering soldier is fit a turn sooner. |
| 7 | **Quiet** | Nothing happens. |
| 8 | **Informant** | +1 Intel. |
| 9 | **Supply drop** | +2 Requisition. |
| 10 | **Volunteers** | One free Regular replacement from the force's teams, if the roster has room. |
| 11 | **Opportunity** | A new node joins the map: roll its type; threat tier 1; linked to the ship or to any taken node; revealed. |
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

**Turn 2.** Standing 3 gives 3 Requisition (13). The assaulter is out. The force spends 6 on a replacement VAT Assaulter (TV 53, Requisition 6) to keep the fireteam full, and 4 on the medical bay (level 1), so injuries from turn 3 on heal a turn faster. That leaves 3. Then it picks its next strike: the safe house (node 5), for the Evidence.
