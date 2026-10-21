# 10. The solo opponent

In solo play the enemy is run by fixed priority lists. Each enemy element has a **behavior profile** (chapter 9 lists which). When an enemy model activates or reacts, read its list from the top and take the first line that applies. When the lists leave a choice, use the tie-breakers at the end of this chapter; nothing else is left to judgment.

Every profile is printed on a behavior card. Lay the cards beside the board with the enemy unit cards.

The lists work aboard ship and on land. They use three terms:

<!-- table:behaviors.terms -->
| Term | Meaning |
|---|---|
| **Area** | Where a model is: a compartment, passage or junction aboard ship; on land, a building, a terrain piece, or the open ground between them. |
| **Approach** | A way an enemy can come into a model's area: a door aboard ship; a door or window of a building; on land, the side of the model's cover that faces the marker. |
| **Exit** | The entry point the mission gives the enemy, a secondary airlock or escape pod aboard ship, or the enemy's board edge on land. |
<!-- /table -->

## Awareness

Each enemy element is in one awareness state at a time. Engaged applies model by model.

<!-- table:behaviors.awareness -->
| State | Meaning |
|---|---|
| **Unaware** | Busy with its work. Does not activate or react. Becomes Alerted when its element sees a player model, hears noise, is hit, or the alarm is raised. |
| **Alerted** | Knows something is wrong but has no target in sight. Activates using its profile's No target list, toward the marker. |
| **Engaged** | Any Alerted model that can see a standing player model when it activates or reacts. Uses its profile's Target in sight list. |
| **At the start** | Elements start Unaware unless the mission says otherwise. Automated models, and models the mission places on Overwatch, start Alerted. |
<!-- /table -->

### Noise

Noise alerts the enemy. It also raises the alert track, when the mission uses one.

<!-- table:behaviors.noise -->
| Noise | Sources | Heard by | Alert track |
|---|---|---|---|
| **Quiet** | Moving, opening and closing doors, hacking, close combat, Silenced shots, smoke grenades | Not heard | — |
| **Loud** | Unsilenced shooting, forcing a door with a breaching tool | Elements with a model within 12" aboard ship, measured through walls; within 24" on land | +1 |
| **Very loud** | Frag grenades, flashbangs, breaching charges, any explosion | Every element on the board | +2 |
<!-- /table -->

### The marker

<!-- table:behaviors.marker -->
- Place or move the marker to the source of the latest noise, or to where an enemy model last saw a player model.
- An Alerted model that ends a Move within 3" of the marker, seeing no player model, removes it. Its element then holds where it is.
- With no marker on the board, Alerted models hold: stay in cover facing the nearest approach and go on Overwatch.
<!-- /table -->

### The alarm

<!-- table:behaviors.alarm -->
- When an element first sees a player model or is hit, it raises the alarm at the end of its next activation: every element on the board becomes Alerted, and the alert track rises by 1.
- If every model of the element is Down, Dead or has surrendered before then, the alarm is not raised.
<!-- /table -->

Putting down a sentry before the end of its next activation, quietly, keeps the rest of the ship, or the position, unaware.

## Drill

Drill is how well an element works together. It depends on the training of its models, and on its leaders.

<!-- table:behaviors.drills -->
| Lowest rank | Drill | Effect |
|---|---|---|
| Green | **No drill** | Each model follows its lists alone. Shoots instead of Aiming, never cracks doors, and throws grenades only where its list says. |
| Regular | **Fire and manoeuvre** | A model ends a Move in the open, in sight of a player model, only while a friend of its element covers it: on Overwatch, or Engaged with the same target. Otherwise it stops in the last cover on its route, or goes on Overwatch instead of moving. Charges are exempt. |
| Veteran | **Bounding and grenades** | As Regular, and: throws a frag at 2 or more player models within its blast radius, with no friend in it; at a closed door (aboard ship or into a building) with the marker or a seen player model beyond, cracks and throws (flashbang first, else frag); the last model of the element to activate each round goes on Overwatch if it has no target. |
| Elite | **Assault drill** | As Veteran, and: focus fire, shooting the player model its element has already hit this round if it can see it; picks Leaders and Auto gunners first at each step of the target list. |
<!-- /table -->

An element's drill is set by the **lowest rank** among its models. A friendly model with the *Leader* skill within 6" of the activating model raises the drill **one step**, to Elite at most. Kill or pin the leader and the element gets sloppier.

Drill changes *how* a model carries out its list, not what it is trying to do: an Aggressive element is always aggressive, but a Green one charges in alone while a Veteran one cracks the door with a flashbang first.

## The enemy turn

### Activation order

When it is the enemy's turn to activate a model, take the first model that fits:

<!-- table:behaviors.activation_order -->
1. A Leader with a friend within 6" that has 2 or more Suppression.
2. Engaged models in the open.
3. Other Engaged models, most Suppression first.
4. Alerted models, nearest the marker first.
5. Unaware models do not activate.
<!-- /table -->

### Choosing targets

<!-- table:behaviors.targets -->
1. Ignore Down, Dead and surrendered models unless nothing else is in sight.
2. A player model it can hit without a cover penalty, nearest first.
3. Otherwise the player model in the least cover, nearest first.
4. Auto weapons spread to every other player model they can engage.
<!-- /table -->

### Moving

<!-- table:behaviors.moving -->
- To cover: the nearest position in cover within the Move, Heavy before Light. If the line says "with a target", it must still see one; otherwise it ends out of sight of every player model if it can.
- Toward the marker or a target: by the shortest route (through doors aboard ship or in buildings; on land, from cover to cover where the route allows), ending in cover if it can. Enemy models open locked doors on their own vessel or ground with an Interact.
- Away: by the shortest route out of sight of every player model, ending in cover.
<!-- /table -->

### Reactions

Enemy models react only as their profile's **Reactions** lines say; a reacting model takes one action, as normal (chapter 2). When a door opens, follow the door sequence (chapter 3): the enemy's eligible models react in activation order.

### Close quarters

<!-- table:behaviors.close_quarters -->
Used first by an Engaged model with a player model within 6", unless its profile says otherwise. Read top-down.

1. Outnumbered (more standing player models than friends within 6" of it) and not Aggressive: Move away, then Shoot if it still has a target.
2. Fight 2 or more, a target within its Move, and Aggressive or Tactical: charge it (close combat).
3. Two or more player models within a frag's blast radius of one point, no friend in it, and it carries frag: Throw it, then Move to cover.
4. Otherwise: use its profile's Target in sight list.
<!-- /table -->

### Close combat

Each profile says when its models **Break**. A model that does not break declares Continue.

### Surrender

<!-- table:behaviors.surrender -->
- A model whose profile allows it tests when it activates Pinned or Wounded with a player model within 6" in its line of sight: 2d6 + Nerve, 8+, or it surrenders.
- A surrendered model lies down, takes no actions, cannot react and is not a target. It is a prisoner if the mission allows it.
<!-- /table -->

### Tie-breakers

When a list leaves a choice, take, in order:

<!-- table:behaviors.tie_breakers -->
1. The option nearest the enemy's objective, or the marker if there is no objective.
2. The lowest-numbered enemy model, or the player model nearest the enemy's objective.
3. Roll a die.
<!-- /table -->

## Behavior profiles

Read each list top-down and take the first line that applies. **Target in sight** is used by an Engaged model; **No target** by an Alerted one.

<!-- table:behaviors.behaviors -->
### Cowed

*Holds its ground, falls back under Suppression, may surrender.* Used by: Salvage crew. **Proposed.**

**Target in sight**

1. Pinned or Wounded, with a player model within 6" in sight: test to surrender, then carry on down the list if it passes.
2. In the open: Move to cover, then Move away.
3. Otherwise: Shoot, Shoot.

**No target**

1. If it can see an approach toward the marker: Move away, out of sight of that approach.
2. Otherwise: stay put.

**Investigates:** None. Cowed models never move toward the marker.

**Reactions**

- Unactivated: when shot at in the open, Move to cover.
- Never goes on Overwatch.

**Close combat:** Breaks after any lost exchange. Uses only its own lists, even in close quarters. May surrender.

### Sentry

*Holds chokepoints, overwatches approaches, raises the alarm.* Used by: Asset security. **Proposed.**

**Target in sight**

1. In the open: Move to cover with a target, then Shoot.
2. Auto weapon with two or more targets in sight: Suppressive Fire, then Shoot.
3. A target within short range: Aim, then Shoot.
4. Otherwise: Shoot, then Overwatch.

**No target**

1. The investigator: Move toward the marker, twice.
2. Already in cover covering the approach to its area nearest the marker: Overwatch.
3. Others: Move to cover covering that approach, then Overwatch.

**Investigates:** One model: the lowest-numbered model without an Auto weapon. The rest hold.

**Reactions**

- Overwatch: Shoot at the first trigger it can.
- Unactivated: when shot at in the open, Move to cover.

**Close combat:** Breaks after a lost exchange if Wounded or with 2 or more Suppression. Uses the close-quarters list first.

### Aggressive

*Closes distance, flanks through side passages, seeks close combat.* Used by: Collections enforcers, Repossession team, Contract reaction force. **Proposed.**

**Target in sight**

1. Fight 2 or more and a target within its Move: charge it.
2. A target within short range: Shoot, Shoot.
3. Otherwise: Move toward the nearest target, then Shoot.

**No target**

1. Move toward the marker, twice. Prefer a route that does not pass through a door or open ground a player model can see (flanking).

**Investigates:** Every model.

**Reactions**

- Overwatch: Shoot at the first trigger it can.
- Unactivated: when a player model comes within short range in its sight, Shoot.

**Close combat:** Never breaks while it has fewer than 3 Suppression. Uses the close-quarters list first.

### Tactical

*Fire and manoeuvre: one element fixes while another moves; uses Overwatch and grenades.* Used by: Liquidators, Contract boarding team, Directorate deniable operators. **Proposed.**

**Target in sight**

1. In the open: Move to cover with a target, then Shoot.
2. No friend of its element is covering the target: Shoot, then Overwatch.
3. A friend is covering and the target is beyond short range: Move to cover closer to the target, out of its line of sight on the way if it can, then Shoot.
4. Otherwise: Aim, then Shoot.

**No target**

1. First of its element this round: Move toward the marker, then Overwatch.
2. Otherwise: Move to cover covering the friend ahead, then Overwatch.

**Investigates:** Every model, in bounds: the first model to activate Moves, the next covers it from Overwatch, alternating.

**Reactions**

- Overwatch: Shoot at the first trigger it can.
- Unactivated: when shot at in the open, Move to cover; when a player model comes within short range in its sight, Shoot.

**Close combat:** Breaks after a lost exchange if Wounded or with 2 or more Suppression. Uses the close-quarters list first.

### Saboteur

*Avoids fights; works consoles, rigs charges, seals doors, destroys evidence.* Used by: Salvage techs. **Proposed.**

**Target in sight**

1. Pinned or Wounded, with a player model within 6" in sight: test to surrender, then carry on down the list if it passes.
2. Within 1" of its task: Interact (the mission says what each Interact achieves), then Interact again.
3. In the open: Move away, then Throw smoke between itself and the target if it has one.
4. Otherwise: Shoot, then Move away.

**No target**

1. An open door between it and the marker within its Move: Move to it, close it and lock it (Interact). No such door (in the open): skip this line.
2. Otherwise: Move to its task and Interact.

**Investigates:** None. Saboteurs work their task and seal doors.

**Reactions**

- Unactivated: when shot at, Move away.
- Never goes on Overwatch.

**Close combat:** Breaks after any lost exchange. Uses only its own lists, even in close quarters. May surrender.

### Automated

*Permanent Overwatch on its field of fire, or triggers on proximity.* Used by: Automated defences. **Proposed.**

**Target in sight**

1. Drone with no target within short range: Move to within short range with a target, then Shoot.
2. Otherwise: Shoot, then Overwatch. A turret only shoots into the field of fire the mission gives it.

**No target**

1. Turret: Overwatch.
2. Drone: Move toward the marker, then Overwatch.

**Investigates:** Drones only. Turrets never move.

**Reactions**

- Overwatch: Shoot at every trigger it can, up to two.
- Never Unaware: it does not hear noise, and starts on Overwatch.

**Close combat:** Never breaks; it has no melee weapon, so it loses every exchange it is forced into. Uses only its own lists, even in close quarters.

### Leader

*Rallies the most suppressed friend nearby, then acts with its element.* Used by: Recovery captain. **Proposed.**

**Target in sight**

1. A friend within 6" with 2 or more Suppression: Rally the most suppressed, then take one action from the line below.
2. Otherwise act as the profile of the nearest friendly element (Sentry if it is alone).

**No target**

1. A friend within 6" with 2 or more Suppression: Rally it.
2. Otherwise act as the profile of the nearest friendly element (Sentry if it is alone).

**Investigates:** Goes with the nearest element; raises its drill one step while within 6".

**Reactions**

- As the nearest friendly element.

**Close combat:** Breaks after a lost exchange if Wounded. Uses the close-quarters list first.

### Handler

*Independent elite; protects the patron's interests first, the mission second.* Used by: Directorate handler. **Proposed.**

**Target in sight**

1. A player model within 6" of the patron's asset: Shoot it, Shoot it (or charge it, if within its Move).
2. Wounded or with 2 or more Suppression, and the asset safe or lost: Move away, toward the nearest exit or board edge. It escapes if it leaves the board.
3. Otherwise: Aim, then Shoot, picking Leaders first.

**No target**

1. Move toward the patron's asset; within 3" of it, Overwatch.

**Investigates:** Moves to the patron's asset, never toward the marker.

**Reactions**

- Overwatch: Shoot at the first trigger it can.
- Unactivated: when a player model comes within short range in its sight, Shoot.

**Close combat:** Breaks after a lost exchange if Wounded. Uses the close-quarters list first.

### Attached

*Uses the profile of the element it is attached to.* Used by: Hired gun. **Proposed.**

**Target in sight**

1. Act as the profile of the element it is attached to (Tactical if it is alone).

**No target**

1. Act as the profile of the element it is attached to (Tactical if it is alone).

**Investigates:** As the element it is attached to.

**Reactions**

- As the element it is attached to.

**Close combat:** As the element it is attached to. Uses the close-quarters list first.
<!-- /table -->

## Example

Gym 1. The VAT team is stacked at D1. Asset security officers 1 and 2 are in the cargo hold, Unaware; the gunner (3) is on Overwatch in the control room, so it starts Alerted; the recovery captain (4) guards the data core, Unaware.

- **Crack and throw.** The assaulter cracks D1 and throws a flashbang beside officer 1. It is **Very loud**: every element becomes Alerted, and the marker goes on the flashbang.
- **Door sequence.** Officer 1 is within the flashbang's radius: 2 Suppression, and no reactions. Officer 2 is outside it and can now see the assaulter, but its Sentry Reactions line only lets an unactivated model Move to cover when shot at, so it holds. The VAT gunner on Overwatch behind the assaulter can see officer 2 and Shoots.
- **Officer 2 activates** first: it is Engaged and in the open. Asset security is all Regular, and the captain is not within 6", so its drill is Fire and manoeuvre. Close quarters: one VAT model within 6" and one friend, so it is not outnumbered; it has Fight 1 and no frag, so it uses the Sentry list: in the open, it Moves to cover with a target (behind the crates), then Shoots the assaulter. At the end of the activation asset security raises the alarm: the alert track rises by 1.
- **The gunner activates** later. It has no target and is already covering D3, the control room door nearest the marker, so it stays on Overwatch.
- **The captain activates.** No friend within 6" has 2 or more Suppression, so it acts as the nearest element, asset security (Sentry). It is not the investigator, so it Moves to cover covering D3, then goes on Overwatch. The control room now has two guns on D3, and the service passage door (D5) is the way round.
