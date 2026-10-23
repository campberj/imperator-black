# Content

All game data lives here as YAML. It is the single source of truth: the rulebook tables, the printable cards and the app all read it. Edit the YAML, then run:

```sh
python3 tools/validate_content.py   # every referenced id exists; ids are unique
python3 tools/build_content.py      # compile to dist/content.json (read by .NET and the web app)
python3 tools/render_tables.py      # refresh tables in rules/*.md
python3 tools/print_sheets.py       # refresh docs/print/*.pdf
```

CI fails if `dist/content.json` or the rulebook tables are stale, or if validation fails.

The .NET loader (`ContentSet.Parse` in `app/src/Imperator.Core`) runs the same cross-reference checks (`ContentValidator`, same messages) and is also strict about shape: a missing required field, a `null` where one isn't allowed, or an unknown key in `stats` fails the load. Content that fails one fails the other; where the Python check is about a missing or mistyped value (a behavior's `summary`, `investigate`, `close_combat`, `surrenders` or `close_quarters`), the .NET error comes from the deserializer and names the field instead of repeating the Python wording. The layout generator check (200 decks per vessel, connected and within the tile inventory) runs the C# port of `tools/generate_layout.py` in `Imperator.Core/Layouts`; its statistics are held to the Python reference by `tools/layout_reference.py` and the Core tests. When you add a check to `tools/validate_content.py`, add it to `ContentValidator` too.

## Files

| File | Top-level keys | What it holds |
|---|---|---|
| `core.yaml` | `hit_target`, `ranks`, `attributes`, `hit_modifiers`, `wound_table`, `suppression`, `movement`, … | Core resolution numbers, ranks and attribute definitions |
| `weapons.yaml` | `ranged`, `melee`, `thrown`, `demolitions` | Weapon profiles: damage dice, short range, AP, keywords |
| `armour.yaml` | `armour` | Armour types: value and keywords |
| `keywords.yaml` | `weapon`, `armour` | Weapon and armour keyword names and rules text |
| `skills.yaml` | `skills` | Model skills, with per-level rules text and status |
| `gear.yaml` | `gear` | Equipment: rules text, skills granted, status (`settled` or `proposed`) |
| `troops.yaml` | `troops` | Troop types: one profile per kind of soldier, for every faction |
| `factions.yaml` | `factions`, `elements`, `threat_tiers` | Factions; enemy elements (troop compositions, tier, behavior); threat tiers |
| `rosters.yaml` | `teams`, `starting_forces` | Player tables of organisation and the PoC starting force |
| `layouts.yaml` | `tiles`, `vessel_roll`, d6 tables, `rooms`, `small_rooms`, `vessels`, `hazards` | Ship and station deck layouts (rules chapter 11) |
| `terrain.yaml` | `types`, `height`, `noise`, `conditions`, `environments`, `environment_roll`, `boards` | Land terrain and table setup (rules chapter 12) |
| `behaviors.yaml` | `awareness`, `marker`, `alarm`, `noise`, `drills`, `activation_order`, `targets`, `moving`, `close_quarters`, `surrender`, `tie_breakers`, `behaviors` | The solo opponent (rules chapter 10): general procedures, and one behavior profile per element type with fixed priority lists (`engaged`, `alerted`, `reactions`, `close_combat`, `investigate`, `surrenders`, `close_quarters`) |
| `missions.yaml` | `mission_roll`, `missions`, `tier_roll`, `tiers`, `contact_results`, `alert`, `complications`, `clock`, `placement`, `contacts`, `briefing` | Missions, threat tiers, the alert track and the mission clock (rules chapter 13) |
| `campaign.yaml` | `start`, `turn`, `resources`, `standing`, `support`, `intel_uses`, `arc`, `nodes`, `special_nodes`, `rewards`, `casualties`, `injuries`, `xp`, `advances`, `honours`, `ship`, `costs`, `events`, `evidence` | The campaign (rules chapter 14 draft) |
| `values.yaml` | `baseline`, `multiplier`, `attribute_costs`, `weapon_formula`, `melee_scale`, `gear`, `grenades`, `skills`, survival and Automated factors | Troop Value and Requisition (rules chapter 15); computed by `tools/troop_values.py` |
| `versus.yaml` | `sizes`, `forces`, `mission_roll`, `missions`, `raid`, `blinds`, `casualty_points`, `end`, `even_grades`, `changes` | Two players (rules chapter 16 draft) |

## Troop type fields

| Field | Required | Meaning |
|---|---|---|
| `id`, `name`, `role` | yes | Identity. `role` is descriptive (Leader, Gunner, …). |
| `faction` | yes | Id from `factions.yaml`. |
| `rank` | yes | `green`, `regular`, `veteran` or `elite`. A label; never used in rolls. |
| `stats` | yes | `move`, `shoot`, `fight`, `physique`, `nerve`, `tech`, `wounds`, `size`. Rolled attributes usually 0–3 (−1 to 4 allowed); `null` only on Automated models. |
| `armour`, `armour_options` | yes / no | Armour id, and alternatives. |
| `weapon`, `weapon_options` | yes / no | Ranged weapon id (or `null` for a model with only a melee weapon: it cannot Shoot), and alternatives. |
| `melee` | yes | Melee weapon id, or `null` (Automated models only). |
| `grenades` | yes | Up to two thrown ids. |
| `gear`, `gear_options` | yes / no | Gear ids carried, and per-model choices. |
| `skills` | yes | Map of skill id to level, e.g. `{ leader: 1, marksman: 2 }`. |
| `miniatures` | no | Suggested models from the collection. |

## Conventions

- **Adding miniatures:** follow [`docs/guides/profiles-and-cards.md`](../docs/guides/profiles-and-cards.md) (profiles, weapons, costs, teams and elements, cards).

- Ids are `snake_case` and never change once used; rename the `name`, not the `id`.
- Rules text in YAML uses straight quotes and `\"` for inches.
- New rules introduced through content (gear effects, skills) start as `status: proposed` and are logged as **Proposed** in `docs/design/decisions.md` until playtested.
