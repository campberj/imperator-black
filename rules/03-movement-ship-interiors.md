# 3. Movement and ship interiors

## Moving

A **Move** action lets a model move up to its **Move** attribute in inches (usually 5–7"), in any direction, changing direction freely. Move reflects training and kit: exoskeletons raise it, heavy armour and heavy weapons lower it.

- Climbing or vaulting a low obstacle costs **2"** extra.
- Models cannot move through other models or through walls and closed doors.
- Moving into base contact with an enemy starts **close combat** (chapter 5).
- Moving into or within an enemy's line of sight can trigger **reactions** (chapter 2).

At Move 6, two Moves cover 12", which is two compartments. A model can bound from one piece of cover to the next and still shoot, or cross a compartment and stack on the far door.

## Ship interiors

Boarding actions are fought on tiles: 6"×6" compartments, 6"×3" passages and 3"×3" junctions, joined by doors.

### Walls and bulkheads

Walls block movement and line of sight. Most interior walls are **thin bulkheads**, which breaching charges can destroy (chapter 6). The mission may mark some walls as **hull** (outer walls) or **armoured bulkheads**; these cannot be breached in the proof of concept, and damaging hull can cause decompression (chapter 8).

### Compartments

A **compartment** is an area fully enclosed by walls and doors. A compartment 6"×6" or smaller counts as **confined** for blast weapons.

**Crawlways and small hatches**, where a mission marks them, admit only models of **Size 2 or less**.

### Doors and hatches

A door or hatch is either **open** or **closed**, and a closed door may be **locked**.

| Door state | Movement | Line of sight |
|---|---|---|
| Open | Pass through | Through the opening only |
| Closed | Blocked | Blocked |

**Opening and closing.** A model within 1" may open or close an unlocked door with an **Interact** action. It may also open or close a door it passes during a Move, at a cost of 2". Either way, this is an Interact. Opening a door starts the **door sequence** (below); closing one triggers reactions from enemies who can see the door.

**Locked doors.** A locked door can be opened by:

| Method | How | Noise |
|---|---|---|
| **Breaching charge** | Place it with Interact, detonate it later (chapter 6). Destroys the door. | Breach |
| **Breaching tool** | Interact with the tool: test 2d6 + Physique, 8+. On a pass, the door is forced open; on a fail, it holds. | Breach |
| **Hack** | A model with a hacking rig uses Interact at a console or the door: test 2d6 + Tech against the lock's security level (below). When it has the successes it needs, the door unlocks and opens. | None |

Noise matters to the enemy (chapter 10).

### Hacking

Every lock and system has a **security level**, set by the mission; when the mission says nothing, it is Secured. A hack is an **Interact** by a model with a hacking rig: test 2d6 + Tech against the level's target number.

<!-- table:core.security -->
| Security level | Hack test | Successes needed | Typical |
|---|---|---|---|
| Basic | 2d6 + Tech, 7+ | 1 | Storage, cargo hatches, crew quarters |
| Secured | 2d6 + Tech, 9+ | 1 | Most locked doors, ship's systems |
| Hardened | 2d6 + Tech, 9+ | 2 | Command decks, armouries, data cores |
| Military | 2d6 + Tech, 10+ | 2 | Warship systems, patron-grade encryption |
<!-- /table -->

- **Successes add up.** A level that needs two successes needs two passed tests, by the same or different models, in any rounds. Mark each success beside the door or console.
- **Working the lock.** Each failed attempt on a target gives **+1** to every later attempt on it, by anyone, to a maximum of +3.
- **Hacking is quiet:** it is never noise.
- An enemy *Jammer* within 6" of the hacker gives −2.

### Cover aboard ship

Consoles, crates, frames and machinery give **Light** cover. Structural columns, half-height bulkheads and heavy machinery give **Heavy** cover. A model standing beside a door frame, out of the line through the opening, is simply out of sight; use true line of sight.

## The door sequence

Opening a door is one of the most dangerous moments aboard ship. Whenever a door or hatch opens, by any means (an Interact, a Move, a hack, a breaching tool or a breaching charge), resolve these steps in order:

1. **The opening.** Resolve the action that opened it, including a breaching charge's damage and Suppression (chapter 6).
2. **Crack and throw.** If the opening model cracked the door and threw a grenade (below), the grenade lands and takes effect now.
3. **The far side reacts.** Each model of the side that did **not** open the door that can now see an enemy model, and is on Overwatch or not yet activated, may react once (chapter 2).
4. **The near side reacts.** Each Overwatch model of the side that opened the door that can now see an enemy model may react once.
5. **Carry on.** The model that opened the door continues its activation.

A door opening is a single trigger: each model reacts to it at most once, whether it was the Interact or the sudden view through the doorway that set it off.

### Crack and throw

A model within 1" of a **closed, unlocked** door may open it and throw a grenade through it as **one Interact action**. Pick a point through the opening, in a straight line from the thrower, within the grenade's throw range. Take the normal Throw test (2d6 + Physique, 8+). On a miss, the grenade lands in the doorway instead, and affects models on both sides that can see it.

The grenade resolves at step 2, before anyone reacts to the door opening. A locked door cannot be cracked; open it first.

### Examples

**Crack and throw.** A VAT assaulter stacks beside closed door D1, with the VAT gunner on Overwatch behind her.
- *Her activation:* action 1, Move to the door; action 2, crack and throw a flashbang into the hold. She passes the Throw test.
- *Step 2:* the flashbang lands. Guards within 3" of it gain 2 Suppression, lose Overwatch and cannot react this round.
- *Step 3:* a guard 5" from the flashbang, out of its radius but now able to see the assaulter, reacts with Shoot at −1.
- *Step 4:* the gunner can now see two guards through the doorway. He reacts with an *Auto 3* burst at both, at −1, and −1 more on the second.
- *Step 5:* the assaulter's activation is over (she used both actions). The next VAT model activates and goes through the door.

**Breaching charge.** Door D3 is locked. The breacher places a charge (action 1) and detonates it (action 2).
- *Step 1:* the door is destroyed. Models within 1" take 2d6 damage; enemy models within 2" on the far side gain 2 Suppression and cannot react this round.
- *Step 3:* an enemy gunner across the room was more than 2" away and is on Overwatch. He can react, but only at a model he can see; the VAT team is stacked beside the frame, out of his line of sight, so he has no target and keeps his Overwatch for the first model through.
- *Next activation:* a VAT model throws a flashbang through the breach (Throw), then follows it in.

**Hack.** The Data Specialist hacks Secured door D3 (2d6 + Tech 2, 9+). The first attempt fails; the second gets +1 and passes. The door unlocks and opens, and the door sequence runs from step 3. Nothing was noise, so unaware enemies stay unaware.

## Stacking and entering

These follow from the rules above:

- Models stacked beside a closed door are out of sight of anyone inside.
- Opening the door starts the door sequence: anyone inside on Overwatch or still to activate may react to whoever they can see.
- Models on Overwatch inside can react twice, to the first models through.
- A flashbang, cracked in or thrown through first, strips Overwatch and stops reactions, which is why breach-and-clear teams use them.
