# Fireteams and attachments: design note

Status: **Proposed, for RJ's review** (2026-10-23). Nothing here is built yet. Decision 6.18.

## The problem

After Gym 2, RJ: troops should be grouped into fireteams (elements, teams) so that a mission, a reinforcement or a campaign force fields them together, not picked à la carte. And: the player should be able either to deploy whole fireteams or to pick individual soldiers, with a campaign setting to switch between them.

## What exists today

| Piece | Where | Used by | Gap |
|---|---|---|---|
| **Teams** (player) | `content/rosters.yaml`: fixed role slots, `gear_choice` | Chapter 9 force building, two-player forces (whole teams, at least half the slots and the Leader) | Player factions only. No minimums or maximums per slot, no troop alternatives, no attachments. |
| **Elements** (enemy) | `content/factions.yaml`: composition, `alternatives`, `tier`, `behavior` | Mission guards, contact tables, reinforcements, opposition tables, the solo opponent | Enemy factions only. The Republic can't be an opposition and the Separatists can't be a player force without duplicating everything. |
| **Attached** | The *Attached* behavior; "Hired gun" and "Golem unit" elements; chapter 7 counts an attached model as part of its fireteam for breaking | Enemy specialists | No player rule. A Jedi, Cody or a marksman can't join a squad. They deploy as loose models. |
| **Picking soldiers** | Chapter 14 (deploy any fit soldiers up to 8 or 12), short arcs (TV and model limit) | Campaigns, short arcs | Purely à la carte. |

The pieces are almost there. They're split by side, and the campaign rules ignore them.

## Proposal

### 1. One fireteam structure for both sides

Teams and elements merge into **fireteams**, in one content file (`content/fireteams.yaml`), used by player and enemy alike.

```yaml
- id: clone_squad
  name: Clone trooper squad
  faction: republic
  summary: "A sergeant, three riflemen and a heavy trooper."
  slots:
    - { troop: rep_clone_sergeant, min: 1, max: 1, leader: true }
    - { troop: rep_clone_trooper,  min: 2, max: 3 }
    - { troop: rep_clone_heavy,    min: 0, max: 1, alternatives: [rep_clone_marksman] }
  # Used when the fireteam is an enemy (solo opponent, opposition tables):
  tier: 2
  behavior: tactical
```

- **Slots** have a minimum and maximum, an optional Leader slot, troop `alternatives` (a different profile in the same slot) and `gear_choice` (as now).
- **Enemy use**: `tier` and `behavior` say how the solo opponent runs it. A fireteam without them is player-only until someone adds them. The Republic gets them for a Separatist-player game later; the Separatists get player slots.
- **The guard, contacts and reinforcements** place a fireteam at its full listed size: its `max` per slot, defaults first. That matches today's elements.
- **Terminology**: rules text says *fireteam* everywhere (chapter 7 already does). "Element" stays as a synonym in chapter 10, where the solo opponent talks about elements, unless RJ prefers to rename it there too.

### 2. Attachments

Some troops are **attachments**: characters, specialists and lone heavy assets. Examples: a Jedi, Cody, a clone marksman, a tactical droid, Grievous, a Golem, a hired gun.

- Content marks them on the troop (`attachment: true`) or lists them per faction. Each lists the fireteams it may join (by role, or "any of its faction").
- **Before deployment, the player chooses for each attachment** (RJ: either, player's choice):
  - **Attached**: it joins one fireteam for the mission. It is part of that fireteam for breaking (chapter 7, as now), Rally and casualty shock. If it is a Leader, it is that fireteam's Leader (drill, initiative). At most one attachment per fireteam (to be confirmed).
  - **Alone**: it is a fireteam of one. It never takes breaking tests (half of one is itself), and nothing rallies it but itself or a Leader.
- **Enemy attachments** work the same way. The *Attached* behavior already says "act as the fireteam it is attached to". A tactical droid attached to a B1 section is the natural Separatist fireteam; Grievous attached to his MagnaGuards is another.

### 3. Force selection: a setting per campaign

Chosen when a campaign or short arc starts, and written on the campaign sheet:

| Setting | Deploying a force | Replacements and growth |
|---|---|---|
| **By fireteam** (default) | Whole fireteams, each with at least its minimums and its Leader, plus attachments, within the mission's TV and model limits. | Replacements fill empty slots in their own fireteam. A new fireteam is bought whole. |
| **By soldier** | Any soldiers fit for duty, within the same limits, as now. Fireteams are formed at the table: before deployment, group the soldiers into fireteams of 2 to 6 for breaking and Rally. | Replacements are any troop the faction fields. |

- **Two-player games are always by fireteam** (chapter 16 already uses whole teams).
- **Short arcs** name a default on the arc sheet. Foundry Moon would default to *by fireteam* (a clone squad plus Ahsoka attached, for example) and say the player may switch.
- The roster sheet groups soldiers under their fireteam, with a column for attachments.

### 4. Deferred

- **Squads and platoons** (two fireteams and a leader, and so on) wait for larger games and vehicles.
- **Fireteam-level activation** (activating a whole fireteam at once) is a separate question. Activation stays per model.

## What would change

| Area | Change |
|---|---|
| Content | New `fireteams.yaml` (from `rosters.yaml` teams and `factions.yaml` elements); `attachment` on troops; opposition tables and missions point at fireteam ids (current element ids kept where possible, so mission files don't break). |
| Rules | Chapter 9 (building a force, attachments), 7 (a lone attachment and breaking), 10 (fireteams and attached models), 13 (wording), 14 and short arcs (the setting, replacements), 16 (minimums). |
| Tools | Validator, table renderer, troop values (fireteam TV), mission generator, scenario and campaign printers (roster grouped by fireteam), cards ("Fireteam:" line instead of "Element:"). |
| App | C# content records (TeamDef and ElementDef become FireteamDef), validator, tests, mission generator; the web app's force views. |

About two to three sessions of work. The content migration and C# records come first, so CI stays green throughout.

## Questions for RJ

1. **Minimum fill (by fireteam):** each slot's minimum plus the Leader, or the two-player rule (at least half the slots and the Leader)?
2. **Attachments per fireteam:** at most one, or one character plus one specialist?
3. **Which troops are attachments?** Proposed: named characters, Jedi, marksmen and snipers, tactical droids, Golems, hired guns, the DSD1, Grievous.
4. **"Element" in chapter 10:** keep it as the solo opponent's word, or rename it to fireteam everywhere?
