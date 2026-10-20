# Content

All game data lives here as YAML. It is the single source of truth: the rulebook tables, the printable cards and the app all read it. Edit the YAML, then run:

```sh
python3 tools/validate_content.py   # every referenced id exists; ids are unique
python3 tools/build_content.py      # compile to dist/content.json (read by .NET and the web app)
python3 tools/render_tables.py      # refresh tables in rules/*.md
python3 tools/print_sheets.py       # refresh docs/print/*.pdf
```

CI fails if `dist/content.json` or the rulebook tables are stale, or if validation fails.

## Files

| File | Top-level keys | What it holds |
|---|---|---|
| `core.yaml` | `hit_target`, `quality`, `hit_modifiers`, `wound_table`, `suppression`, `movement`, … | Core resolution numbers |
| `weapons.yaml` | `ranged`, `melee`, `thrown`, `demolitions` | Weapon profiles: damage dice, short range, AP, keywords |
| `armour.yaml` | `armour` | Armour types: value and keywords |
| `keywords.yaml` | `weapon`, `armour`, `model` | Keyword names and rules text |
| `gear.yaml` | `gear` | Equipment: rules text, keywords granted, status (`settled` or `proposed`) |
| `troops.yaml` | `troops` | Troop types: one profile per kind of soldier, for every faction |
| `factions.yaml` | `factions`, `elements`, `threat_tiers` | Factions; enemy elements (troop compositions, tier, behavior); threat tiers |
| `rosters.yaml` | `teams`, `starting_forces` | Player tables of organisation and the PoC starting force |
| `behaviors.yaml` | `behaviors` | Enemy behavior profile ids and intent (decision tables come in Phase 3) |

## Troop type fields

| Field | Required | Meaning |
|---|---|---|
| `id`, `name`, `role` | yes | Identity. `role` is descriptive (Leader, Gunner, …). |
| `faction` | yes | Id from `factions.yaml`. |
| `q` | yes | Quality 0–3. |
| `armour`, `armour_options` | yes / no | Armour id, and alternatives. |
| `weapon`, `weapon_options` | yes / no | Ranged weapon id, and alternatives. |
| `melee` | yes | Melee weapon id, or `null` (Automated models only). |
| `grenades` | yes | Up to two thrown ids. |
| `gear`, `gear_options` | yes / no | Gear ids carried, and per-model choices. |
| `keywords` | yes | Model keyword ids. |
| `move` | no | Overrides the default (6", or 5" in Heavy armour). `0` cannot move. |
| `miniatures` | no | Suggested models from the collection. |

## Conventions

- Ids are `snake_case` and never change once used; rename the `name`, not the `id`.
- Rules text in YAML uses straight quotes and `\"` for inches.
- New rules introduced through content (gear effects, keywords) start as `status: proposed` (gear) and are logged as **Proposed** in `docs/design/decisions.md` until playtested.
