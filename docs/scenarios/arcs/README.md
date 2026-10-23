# Short arcs

Authored three-mission stories (rules chapter 14, *Short arcs*): an opening, a second mission chosen by the opening's grade, and a finale. Each arc has a folder with its arc sheet (`arc.yaml`) and one file per mission.

Print an arc sheet (story, mission flow, rules and roster) and its missions (sheet, dashboard and cards):

```sh
python3 tools/print_campaign.py --arc docs/scenarios/arcs/<arc>/arc.yaml
python3 tools/print_scenario.py docs/scenarios/arcs/<arc>/<mission>.yaml
```

PDFs go to `docs/print/arcs/`.

| Arc | Force against | Missions | Boards |
|---|---|---|---|
| [Foundry Moon](foundry-moon/arc.yaml) | Galactic Republic against the Separatist Alliance, solo | 1 Lights Out at Refinery Nine; 2A Kraken's Convoy or 2B Bring Them Home; 3 The Foundry Ship | Industrial table, RJ's desert table (Gym 2), a frigate deck |

## Writing a short arc

- A mission file is a chapter 13 mission (`kind: mission`) with an `arc: { id, name, label }` block. Start from a generated mission (`tools/generate_mission.py ... --opposition <faction>`) or an existing table (the Gym 2 desert map), then write the story into it.
- Fields an authored mission may add: `task`, `lockdown` and `grades` (to replace the mission type's text), `reinforcement` (the element that answers alert 4), `special_rules` (title and text), `next` (what each grade leads to), `map_note`, and extra `briefing` headings such as `Force`.
- Every mission names its force limit (TV and models) in a `Force` briefing heading.
- Every mission's `next` says which mission follows and what edge or setback it carries; the finale's `next` points to the arc's result.
- Units come from the content files only: new profiles follow `docs/guides/profiles-and-cards.md`.
