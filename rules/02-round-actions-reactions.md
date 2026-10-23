# 2. The round, actions and reactions

## The round

1. **Initiative.** Each side rolls 2d6 and adds the Nerve of its *Leader* with the highest Nerve. The winner chooses who activates first. Reroll ties.
2. **Activations.** Sides alternate, activating one model at a time. Mark each model Activated when it finishes. When one side has no models left to activate, the other side activates its remaining models in turn.
3. **End of round.** Resolve in this order:
   1. Bleeding and Critical models that have run out of time die (chapter 7).
   2. Down models test to recover (chapter 7).
   3. Each model that was not hit this round and is in cover removes 1 Suppression.
   4. Hazards update: fire spreads, smoke clears, decompression ends (chapter 8).
   5. Mission effects resolve (alert track, reinforcements).
   6. Remove Activated tokens. **Overwatch markers stay** until their model next activates.

## Actions

An activating model takes **two actions**, in any order. It may repeat an action unless noted.

| Action | Effect | Chapter |
|---|---|---|
| **Move** | Up to the model's Move in inches. Moving into base contact with an enemy starts close combat. | 3, 5 |
| **Shoot** | Fire a ranged weapon at a visible target. | 4 |
| **Aim** | +1 to the next Shoot this activation. | 4 |
| **Suppressive Fire** | *Auto* weapons only. No damage; ignores cover; a hit adds Suppression and stops the target reacting. | 4 |
| **Throw** | Throw a grenade. | 6 |
| **Overwatch** | Must be the last action. Place an Overwatch marker (see Reactions). | 2 |
| **Find Cover** | Use micro-terrain in the open: test 2d6 + Nerve, 7+ Light cover, 10+ Heavy cover, until the model moves. | 4 |
| **Rally** | Remove 1 Suppression from this model or a friend within 6" in line of sight (2 near a Leader). | 7 |
| **Medic** | Treat a Down model in base contact. | 7 |
| **Interact** | Use an objective, door, console or charge within 1". | 3, 6 |

## Reactions

A reaction is a single action taken out of turn, in response to an enemy action.

### Who can react

- **A model that has not yet activated this round.** Reacting uses up its activation: mark it Activated. It gives up two actions for one.
- **A model with an Overwatch marker.** It may react **once** before its next activation; then the marker is removed. An Overwatch reaction does not mark the model Activated. When the model next activates, remove the marker; an unused reaction is lost.

A model reacts at most once between its activations: a model on Overwatch cannot also react as an unactivated model. An *Auto* weapon's reaction Shoot still spreads across up to N targets, each at −1 (chapter 4).

### Triggers

A model may react when an enemy model, in its line of sight:

| Trigger | When the reaction happens |
|---|---|
| **Moves** into or within its line of sight | At any point during the move, interrupting it. The mover then finishes its move, unless the reaction put it Down. |
| **Shoots, throws or uses Suppressive Fire** | After the action resolves. |
| **Interacts**, including closing a door or placing a charge the reactor can see | After the action resolves. |
| **A door opens** (by either side) | Follow the door sequence (chapter 3): the far side reacts first, then the opening side's Overwatch models. |
| **Comes into view** of a model on Overwatch for any reason (smoke clears, a door opens, a wall is breached) | Immediately. |

### Reaction choices

A reacting model takes **one** of these actions:

- **Shoot** at the triggering model, at −1.
- **Move** up to half its Move, ending in cover or out of the triggering model's line of sight.
- **Find Cover.**

### Limits

- A reaction cannot trigger reactions.
- Each model may react only once to any one trigger. Any number of models may react to the same trigger; the reacting side chooses the order and resolves them one at a time. Once the triggering model is Down or Dead, the remaining reactions are not used and not spent.
- A **Pinned** model may only react with Find Cover.
- A model with **2 or more Suppression** loses its Overwatch marker.
- *Cumbersome* weapons cannot fire as reactions. *One per round* weapons count reaction shots toward their limit.
- Close combat resolves at once and cannot be interrupted by reactions (chapter 5).

### Timing

Reactions are never simultaneous. The triggering action resolves in full first: hit rolls, damage, Wounded markers, Down, Suppression and casualty shock. Only then do reactions happen, and only by models still able to take them. There is no comparing of rolls to see who fires first: the model that acts has the initiative, and the reactor fires second, at −1.

**Example: shot, then return fire.** The VAT gunner (compact LMG, *Auto 3*) activates in round 2. Two Freespace sentries stand together in the hold; the sentry gunner, unactivated, is 5" behind them.

1. *First action, Shoot.* The burst targets the nearer sentry and spreads to the shotgun sentry 2" away (−1 on that roll). The first roll misses; the second hits and puts the shotgun sentry Down.
2. *Resolve in full.* The sentry gunner is within 6" of a friend who went Down: casualty shock, +1 Suppression.
3. *Reactions to the first Shoot.* The sentry gunner may react now if it is eligible and its profile says so (on Overwatch it would Shoot; unactivated and in cover, the Sentry profile does nothing). Here it does not react.
4. *Second action, Shoot* at the sentry gunner. The roll hits: a Wounded marker and +1 Suppression. It now has 2 Suppression, so it loses any Overwatch marker.
5. *Reaction to the second Shoot.* Had it been put Down, there would be no reaction. Had it reached 3 Suppression (its Nerve 1 + 2), it would be Pinned and could only Find Cover. It is neither, so if its profile allows, it Shoots back at the VAT gunner at −1 (reaction), −2 (Suppression) and −1 (Wounded); its *Auto 3* burst may spread to VAT models within 3" of the gunner. If it reacted as an unactivated model, it is now Activated.

### Solo opponent

Enemy models react as their behavior profile says (chapter 10).
