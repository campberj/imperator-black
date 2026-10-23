# Fireteams and attachments: design note

Status: **Built** (2026-10-23), Proposed — playtest. Decision 6.18. RJ approved revision 2: the type numbers stand, no cap on characters beyond TV, robots stay as they are (specialists for now; they may move to the vehicle rules). Two adjustments to fit the existing fireteams: every type takes 0 or 1 Leader (droid sections, crews and contractor teams have none), and Support takes 1 to 3 specialists (a lone Hound, turret or DSD1 is a support fireteam of one).

## The problem

After Gym 2, RJ: troops should be grouped into fireteams (elements, teams) so that a mission, a reinforcement or a campaign force fields them together, not picked à la carte. The player should be able either to deploy whole fireteams or to pick individual soldiers, with a campaign setting to switch between them. And (revision 2): **no heavy army-building system**. There should be structure where it helps, and room for players to customize their teams and their force.

## RJ's calls (2026-10-23)

- **A fireteam fields at least half its slots, plus its Leader** (as two-player games do now).
- **Characters are separate from specialists.** Characters (Jedi, Cody, Grievous, a tactical droid) attach to a fireteam or deploy alone, the player's choice. Specialists are fireteam members.
- **The kind of fireteam sets how many specialists it takes.** A line fireteam takes one; a VAT team takes several (gunner, data specialist).
- **Each army decides who counts as a specialist.** Typically gunners, medics, data specialists and hackers, engineers, drone operators, anti-armour troopers and marksmen.
- **"Fireteam" and "element" mean the same thing** in the rules.

## The proposal: one rule, a tag per troop, a few fireteam types per army

### 1. Every troop has a class

One field on each troop profile, shown on its card:

| Class | Who | Examples |
|---|---|---|
| **Leader** | Leads a fireteam | Clone sergeant, VAT team leader, OOM-free B1s have none |
| **Trooper** | The fireteam's core | Clone trooper, VAT assaulter, B1, Pointman |
| **Specialist** | A fireteam member with a job | Clone heavy, VAT gunner, data specialist, medic, marksman, B1 heavy, AT specialist |
| **Character** | Joins a fireteam or acts alone | Jedi, Cody, Grievous, tactical droids, a Golem, a hired gun |

Each army's chapter 9 section lists its classes in the troop table, so who counts as what is the army's call.

### 2. Each army has a few fireteam types

A fireteam type is a single line, not a roster:

| Type | Leader | Troopers | Specialists | Size |
|---|---|---|---|---|
| **Line** | 0 or 1 | 2 to 5 | up to 1 | up to 6 |
| **Assault** (VAT, ARC, commandos, BX) | 0 or 1 | 1 to 3 | up to 2 | up to 5 |
| **Support** (marksmen, gun teams, robots) | 0 or 1 | 0 to 1 | 1 to 3 | up to 3 |

The numbers are a starting point; an army may change them, or have only some of the types. Fireteams without a Leader (marksman pairs, droid squads) are allowed where the army says so.

**Building a fireteam:** pick a type and fill it from the army's troops by class. That's the whole rule. The fireteams listed today (clone squad, VAT fireteam, UN reserve fireteam, B1 section) stay in the content as **standard fireteams**: ready-made examples that follow their type, used by missions, the solo opponent and quick setup. A player may field a standard fireteam as it is, or build their own to the type.

### 3. Characters

- Before deployment, each character either **joins a fireteam** (at most one character per fireteam) or **acts alone**.
- **Joined:** it is part of that fireteam for breaking, Rally and casualty shock. A Leader character leads it (drill, initiative, the +1 to breaking tests).
- **Alone:** a fireteam of one. It never takes breaking tests.
- Enemies work the same way. The *Attached* behavior is already "act as the fireteam it joined".

### 4. Picking a force: a setting per campaign

| Setting | Deploying | Replacements |
|---|---|---|
| **By fireteam** (default) | Fireteams with at least half their slots and their Leader, plus characters, within the mission's TV and model limits. | Fill empty slots in the soldier's own fireteam. |
| **By soldier** | Any soldiers fit for duty, within the same limits. Before deployment, group them into fireteams of 2 to 6 for breaking and Rally. | Any troop the army fields. |

Two-player games are always by fireteam. A short arc sheet names its default; the player may switch at the start.

### 5. The enemy

Missions, contacts, reinforcements and opposition tables keep placing standard fireteams at full size, as elements do now. Nothing changes for the solo opponent except the word, which may be either.

## What would change

| Area | Change |
|---|---|
| Content | A `class` on every troop (leader, trooper, specialist, character); `fireteam_types` per faction; teams and elements become standard fireteams with a `type` (current ids kept, so missions don't break). |
| Rules | Chapter 9: classes, fireteam types, building a fireteam, characters (about one page). Chapter 7: a lone character and breaking. Chapters 14, 16 and the short arc rules: the setting and replacements. |
| Tools and app | Validator (standard fireteams must fit their type), troop tables (class column), cards (class in the subtitle), campaign and arc rosters grouped by fireteam, C# records and tests. |

## Still to settle

1. **Type limits.** Are the Line / Assault / Support numbers above about right?
2. **Characters per force.** Is any limit needed beyond TV and models (for example, one Jedi per fireteam already holds; a force cap of two characters)?
3. **Robots and drones** (a Hound, a ZQ44 pair, a DSD1): characters, or specialists in a support fireteam?
