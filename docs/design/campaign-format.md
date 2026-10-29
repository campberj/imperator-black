# Campaign definitions: format

Status: **Proposed** (2026-10-29). Decision 7.15. The format the campaign game (roadmap Phase 9; app stage S2) reads, and the paper tools print. Validated by `tools/validate_content.py`.

A **campaign definition** is one YAML file. It says what the campaign is (who fights whom, from what base, under what command) and how it moves on (authored branches, or generated arcs). Everything it applies is a rule in `rules/` or `content/`. The definition only chooses among them and supplies the story text.

## Where files live

| Kind | Where | Example |
|---|---|---|
| **Bespoke** (`source: bespoke`): authored missions and branches | `docs/scenarios/arcs/<id>/arc.yaml`, with one mission file per mission beside it | [Foundry Moon](../scenarios/arcs/foundry-moon/arc.yaml) |
| **Procedural** (`source: procedural`): generated arcs | `docs/scenarios/campaigns/<id>.yaml` | [Outer Reach Patrol](../scenarios/campaigns/outer-reach-patrol.yaml) |

## Fields every campaign has

| Field | Meaning |
|---|---|
| `id`, `name`, `summary` | Identity and the opening text. |
| `kind: campaign` | Marks the file as a campaign definition. |
| `format` | `short_arc` (three authored missions, chapter 14 *Short arcs*), `arcs` (chapter 14 arcs) or `story` (a longer authored graph of missions). |
| `source` | `bespoke` or `procedural`. |
| `faction`, `opposition` | The player faction; the opposition id from `missions.yaml` `oppositions`, or `null` for the default tiers. |
| `rules_version` | The rules the campaign was written for. |
| `force_selection` | `fireteam` or `soldier` (chapter 14): the default; the player may switch at the start. |
| `command` | The commander's latitude, an id from `campaign.yaml` `latitudes`: `black_ops`, `orders` or `free_hand`. |
| `base` | `null` (no base, as in short arcs), or `{ kind: outpost | ship, name, facilities: { facility id: level } }` (chapter 14 *The base*). |
| `attacks` | Campaign triggers for base attacks beyond the threat track: `{ when: { … }, text }`. |

## Bespoke campaigns

| Field | Meaning |
|---|---|
| `start` | The label of the first mission. |
| `missions` | `{ label, file, board, force }`: each mission file is a chapter 13 mission with an `arc` block (see `docs/scenarios/arcs/README.md`). |
| `results` | The campaign's result text by grade (`Decisive`, `Marginal`, `Failure`). |

Each mission file's **`next`** list holds the branches, one per grade:

```yaml
next:
  - grade: Decisive
    to: 2A                       # a mission label, or end
    effects: { initiative: player }
    report: "SITREP. Both uplink masts down. …"   # short military report, shown after the mission
    text: "Mission 2A, Kraken's Convoy. The grid is down …"   # the printed sheet's wording
```

**Effects** carry over to the next mission or to the campaign:

| Effect | Means |
|---|---|
| `initiative: player` | The player wins initiative in round 1 of the next mission. |
| `alert_start: n` | The next mission's alert track starts *n* higher. |
| `force_tv: n` | The next mission's TV limit changes by *n*. |
| `remove_contacts: n` | Before the next mission's setup, the player removes *n* contact chips. |
| `requisition`, `intel`, `standing`, `evidence`, `threat: n` | Campaign resources change by *n* (chapter 14). |
| `flag: name` | Sets a named flag a later mission's text may check. |

`to: end` finishes the campaign. A `short_arc` works out its result by the short arc rule; other formats use `results` directly by the last grade.

## Procedural campaigns

| Field | Meaning |
|---|---|
| `start_force` | A starting force id from `rosters.yaml` `starting_forces`. |
| `resources` | Starting Requisition, Intel, Standing, Evidence and Threat. |
| `generator` | `arcs` (how many arcs, at most), `node_table` (`campaign`: chapter 14's node table), `environments` and `vessels` (lists that replace the nodes' own when given). |
| `reports` | `default` (the templates in `campaign.yaml` `reports`) or the id of a campaign's own set. |
| `end` | The campaign's results: `{ result, when, text }`. |

Generated text uses the **report templates** (`campaign.yaml` `reports`): short military reports with `{slots}` filled from the campaign state (`{force}`, `{node}`, `{mission_name}`, `{tier}`, `{opposition}`, `{losses}`, `{standing}`, and so on).

## Checks

`tools/validate_content.py` checks every campaign file:
- the faction, opposition, command latitude and start force exist;
- a bespoke campaign's mission files exist;
- every mission's `next` covers the three grades;
- every `to` is a mission label or `end`;
- every effect is one listed above.
