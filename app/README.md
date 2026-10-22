# Imperator Black app — where to pick up

Handoff for Claude Code (or anyone) working on the application. Read this, then [`../CLAUDE.md`](../CLAUDE.md), then [ADR-001](../docs/architecture/adr-001-architecture.md).

Last updated: 2026-10-22, rules v0.5 (chapters 10–13: solo opponent, ship layouts, land battles, missions).

## How the work is split

- **Design and rules** happen in the claude.ai Project for Imperator Black and land in `docs/`, `rules/` and `content/`.
- **Application code** happens here, in `app/`, ideally in Claude Code on RJ's machine, where NuGet and Docker are available.
- Both commit to `main` on `campberj/imperator-black`. **Pull before starting.** The [decision log](../docs/design/decisions.md) is the authority on game rules; the [roadmap](../docs/roadmap.md) says which phase is current.

## What exists

| Path | What it is | State |
|---|---|---|
| `app/Imperator.slnx` | Solution (.NET 10, new `.slnx` format) | Builds clean, 0 warnings |
| `app/src/Imperator.Core` | Game logic: dice and seeded `Rng`, content loading (`ContentSet`), wound table, shooting (exact odds and sampled), close combat, grenades, `Soldier` state, chapter 11 deck generator (`Layouts/DeckGenerator`, a port of `tools/generate_layout.py`; the deck keeps its graph: links, spine, heart, with `RouteFrom` for route distances) | Covers rules v0.4 chapters 4–7 math with attribute-based `Soldier` (`FromTroop` builds one from a troop profile; `Generic` makes abstract test soldiers). No movement, board, or mission logic yet |
| `app/src/Imperator.Api` | ASP.NET Core minimal API, EF Core + Npgsql (snake_case tables). Schema: `Player`, `Campaign`, `CampaignMembership`, append-only `CampaignEvent`. Endpoints: `GET /health` (database check + content version), `GET /content` (validated `content.json`, `Imperator-Content-Version` header, ETag), `GET /decks/{vessel}?seed=N` (a generated deck in the scenario format, with seed and content version) | Schema only, no auth. Migration `InitialSchema` |
| `app/src/Imperator.Sim` | Combat simulator: shot odds, duels, frag, open-ground firefights, room clearing, `--compare` for content variants | Working. Output in `docs/analysis/` |
| `app/tests/Imperator.Core.Tests` | xUnit v3 tests (310 cases: the former runner's 72 checks, content loading and validation, and the deck generator held to the Python reference statistics in `Fixtures/layout-reference.json`, written by `tools/layout_reference.py`); run with `dotnet test app/Imperator.slnx` | Passing |
| `app/tests/Imperator.Api.Tests` | xUnit v3 + `WebApplicationFactory` (17 cases): endpoints (including `/decks`) and schema, no database needed | Passing |
| `content/*.yaml` → `content/dist/content.json` | Game data. Compiled by `tools/build_content.py`. .NET reads only the JSON into typed records (every file, including the chapter 10 solo opponent and its terms, chapter 11 `layouts`, chapter 12 `terrain`, `core.security` and `core.shoot_scale`) and validates it at load (`ContentValidator`, mirroring `tools/validate_content.py`) | `core`, `weapons`, `armour`, `keywords`, `gear`, `troops`, `factions`, `rosters`, `behaviors`. Schema: [content/README.md](../content/README.md) |
| `app/web` | Vite + React + TypeScript, read from `/content` (dev server proxies to the API). Unit database (`#units`: troop cards per faction with filters and search, teams, elements), reference (`#reference`: gear, skills, keywords, weapon and armour tables, searchable), behavior cards (`#behaviors`). Vitest + Testing Library, oxlint. See [web/README.md](web/README.md) | Read-only |
| `docker-compose.yml` (repo root) | PostgreSQL 18 for local development | |
| `.github/workflows/ci.yml` | Checks content JSON and rulebook tables are current, builds, runs tests, checks EF migrations match the model and apply to PostgreSQL; a `web` job lints, tests and builds `app/web` | Green |

**NuGet:** the cloud workspace where this started blocks `api.nuget.org`, which is why content is compiled YAML → JSON (System.Text.Json is built in) and why the tests began as a console runner. Locally, NuGet works. Keep the YAML → JSON step regardless; the web front end will read the same JSON.

**`global.json`** (repo root) pins SDK 10 and opts `dotnet test` into Microsoft.Testing.Platform, which xUnit v3 needs on the .NET 10 SDK.

## Next tasks, in order

These are foundations that don't depend on unfinished rules. Each should leave CI green.

### 1. Move tests to xUnit ✅ done (2026-10-20)

- Replace the console runner with an xUnit project at the same path; port all 72 checks.
- Keep the statistical checks (exact vs sampled odds, Down recovery rate) with their tolerances and fixed seeds.
- Update CI to `dotnet test app/Imperator.slnx`.
- **Done when:** `dotnet test` passes with the same coverage, and CI runs it.

### 2. Finish content loading ✅ done (2026-10-20)

`ContentSet` already loads `core`, `weapons`, `armour`, `gear`, `skills` and `troops` (`TroopDef`, `TroopStats`), and `Soldier.FromTroop` builds a model from a profile. Still to do: load `factions` (factions, elements, threat tiers), `rosters` (teams, starting forces), `behaviors` (the chapter 10 solo opponent: general procedures plus per-profile priority lists, all text for now) and `keywords` into typed records, plus `core.security` (hacking security levels), and validate every referenced id at load time (mirror `tools/validate_content.py`).

- **Done when:** a test loads every element and team and resolves every id; loading fails loudly on a bad id.

### 3. API skeleton ✅ done (2026-10-20)

- Add `app/src/Imperator.Api`: ASP.NET Core minimal API, EF Core with Npgsql.
- Add a `docker-compose.yml` at the repo root with PostgreSQL for local development.
- Data model per ADR-001 §4–5, **schema only**: `Player`, `Campaign`, `CampaignMembership` (role: Commander / Operator), and `CampaignEvent` (append-only: id, campaign id, sequence, type, JSON payload, created at, created by). No auth yet.
- Endpoints: `GET /health`, `GET /content` (serves `content.json` with its version).
- **Done when:** `docker compose up` plus `dotnet run` serves both endpoints, and an EF migration creates the schema.

Running it locally (Docker via Docker Desktop, OrbStack or Colima):

```sh
docker compose up -d --wait                                    # PostgreSQL on localhost:5432 (dev credentials in docker-compose.yml)
dotnet tool restore                                            # dotnet-ef, pinned in dotnet-tools.json
dotnet ef database update --project app/src/Imperator.Api      # apply migrations
dotnet run --project app/src/Imperator.Api                     # http://localhost:5080/health and /content
```

After changing an entity: `dotnet ef migrations add <Name> --project app/src/Imperator.Api --output-dir Data/Migrations`. CI fails if the model and migrations disagree.

### 4. Web skeleton ✅ done (2026-10-20)

- Add `app/web`: Vite + React + TypeScript.
- One page that fetches `/content` and renders the weapon and armour tables, as a smoke test of the whole pipeline.
- **Done when:** `npm run dev` shows the tables from live content.

With the API running: `npm --prefix app/web install && npm --prefix app/web run dev`, then open http://localhost:5173.

### 6. Unit database and reference (web, read-only) ✅ done (2026-10-21)

The content already holds the unit database: every faction, troop profile, weapon, armour, gear item, skill, keyword, team, enemy element and behavior profile (`content/*.yaml`, served by `GET /content`). Build the screens to browse it. No new rules; every word comes from the `text` and `name` fields.

- **Units:** pick a faction, see its troop profiles as cards (the same layout as `docs/print/ImperatorBlack_unit-cards.pdf`: attributes, armour, weapons and options, kit, skills with their text), its teams (player factions) or elements (enemy factions, with threat tier and behavior).
- **Reference:** gear, skills (with levels and Proposed status), weapon and armour keywords, searchable.
- **Behavior cards:** the chapter 10 reference cards and one card per behavior profile.
- Filters: faction, rank, role, keyword; a search box across names and text.
- **Done when:** every troop in `content/troops.yaml` renders, a Vitest test checks one profile per faction against the content, and lint, tests and build pass.

### 7. Missions content in Core ✅ done (2026-10-22)

`content/missions.yaml` (rules chapter 13) is in `content.json`, but `ContentSet.Parse` doesn't load it yet. `content/terrain.yaml` now carries generator footprints: `pieces` on each sector roll and `fill_pieces` on each environment. The records `TerrainPieceDef`, `SectorRollRow.Pieces` and `EnvironmentDef.FillPieces` already exist in `TerrainContent.cs`.

- Add typed records for every section of `missions.yaml`: `mission_roll`, `missions`, `tier_roll`, `tiers` (contacts, reinforcement, hvt, d6 table), `contact_results`, `alert` (raises, thresholds), `complications`, `clock` (sizes, check-ins, rules), `placement`, `contacts`, `briefing`. Load them in `ContentSet.Parse` as a required section.
- Port the mission and terrain-piece checks from `tools/validate_content.py` to `ContentValidator`: every d6 table covers 1–6; tier table results are element ids or `false_alarm` / `booby_trap`; reinforcements are element ids; HVTs are troop ids; clock sizes have a die and a limit past the first check-in; every sector roll has `pieces` with known terrain types that fit a 12" sector; every fill has cover.
- Tests: section counts (4 missions, 4 tiers, 2 clock sizes), one negative test per new check.
- **Done when:** CI is green and `GET /content` still validates.

### 8. Deck layout generator in Core ✅ done (2026-10-22)

`tools/generate_layout.py` is the reference implementation of rules chapter 11 (`content/layouts.yaml`).

- Port it to `Imperator.Core` (`Layouts/DeckGenerator.cs`), seeded through `Rng`, producing the same deck model (tiles, doors with state and security, cover, entry, objective, hazard, roll count). Keep the deck graph (which tiles connect, and the spine) in the model: task 11 routes over it.
- Match the reference statistics: run `python3 tools/generate_layout.py --check --n 2000` and assert the C# generator's compartments, loops and tile use per vessel are within a small tolerance; every deck connected and within 12 of each tile.
- `GET /decks/{vessel}?seed=N` returns the deck as JSON, recording seed and content version.
- **Done when:** tests pass and the endpoint returns a deck for each vessel.

### 9. Board map in the web app

- Render a deck from task 8 as SVG: the 3" grid, tiles with ids and names, doors coloured by state with security, cover pieces with ids, entry and objective; the same conventions as the printed deck sheets (`docs/print/decks/`).
- Rooms, doors and terrain tables beside the map; a seed box and a vessel picker; print styles.
- Build the map as one component that takes a board model (tiles optional), so tasks 10 and 12 reuse it for land tables and mission markers: 12" sector lines with names (A1 at the player's bottom left), terrain footprints coloured by type (`tools/print_scenario.py` has the colours), guard models, contact chips (`?n`), objectives and the enemy entry.

### 10. Land table generator in Core

The reference is `Mission.build_land` in `tools/generate_mission.py` (rules chapter 12, "Setting up a land table").

- `Land/LandTableGenerator.cs`: environment (named or rolled; fortified only when named), board by size (small 36" × 36", large 48" × 48"), one d6 per 12" sector, the density check (three sectors in a line with no `pieces` → the middle one becomes the fill, repeated until none), the conditions roll.
- Place each roll's `pieces` near the sector's middle as the reference does: the first piece centred; rocks on a hill placed on it; others offset clear of earlier pieces and inside the sector. Which way a piece faces is the player's choice in the rules, so here it comes from `Rng`.
- `GET /land/{environment}?size=small&seed=N`; show it in the web app with the task 9 map.
- **Done when:** a test checks that no line of three empty sectors remains over 1,000 seeds per environment, every piece lies inside its sector, and the endpoint returns a table.

### 11. Mission generator in Core

The reference is `tools/generate_mission.py`; the procedure is rules chapter 13, "Building a mission".

- `Missions/MissionGenerator.cs`, seeded, in the paper order: mission type and threat tier (named or rolled), board (task 8 deck or task 10 table), complication, objectives, guard, contact chips, enemy entry, clock, briefing.
- Aboard ship, "farthest" means farthest from the airlock by route over the deck graph (fewest tile steps); ties go to the straight-line distance from the airlock. On land, "farthest" means farthest from the middle of the player's edge.
- Complications change the mission: Early warning sets the alert start to 1; Reinforced adds a contact; Tight window takes 5 minutes off; Hazard gives the vessel's first hazard aboard ship, or Night on land in place of the conditions roll.
- Guard: one roll on the tier table, rerolling false alarm and booby trap, models placed within 3" of the first objective and clear of cover; a capture mission adds the HVT to the guard unless it is already in it.
- The model records everything the printed sheet shows: seed, content version, the roll log, briefing headings in the `briefing` order, objectives, guard models, contact chips, enemy entry and any second enemy exit, clock (die, limit, check-ins), alert start, reinforcement element and HVT.
- `GET /missions?setting=ship&seed=N` (optional `mission`, `tier`, `vessel`, `environment`, `size`).
- **Done when:** over 2,000 seeds per setting, the mission and tier frequencies match their d6 tables within tolerance; every contact chip and guard model stands inside the board, clear of solid cover; objective counts match the mission type; the endpoint returns a mission.

### 12. Mission view in the web app

- A mission page: the briefing, then the task 9 map with objectives, guard models (numbered), contact chips and the enemy entry. Below it: the mission type's taking-it, lockdown and grades text; the alert track and clock rules; the tier's contact table with element compositions and behaviors; the enemy elements with drill (the drill rules are in the chapter 10 content; `tools/print_scenario.py` `element_drill` is the reference).
- Rooms and doors (ship) or sectors and terrain (land) tables; a seed box and pickers for setting, mission, tier, vessel or environment, and size.
- Print styles that come close to `docs/print/missions/ImperatorBlack_mission-*-sheet.pdf`.
- Out of scope: the live dashboard (it needs saved game state; it comes with the Phase 7 campaign tracker).
- **Done when:** both example missions in `docs/scenarios/missions/` can be shown, and lint, tests and build pass.

### 13. Stop and check the roadmap

Beyond this, the app's real features wait on rules:

| App feature | Waits on |
|---|---|
| Live mission dashboard (alert, clock, contacts, models during play) | Saved game state; built with the Phase 7 campaign tracker (RJ, 2026-10-22). The paper dashboard is `docs/print/ImperatorBlack_mission-dashboard.pdf`. |
| Custom troop and unit builder | Phase 7 point values, then Phase 9 (decision 6.12) |
| Campaign tracker | Phase 7: campaign loop |

Planned app feature (RJ, 2026-10-20): an in-app reference that explains every gear item, keyword and special rule, read from `content/` (the list will grow). Build it from the same `text` fields the rulebook tables use.

Unit cards and the quick reference are generated by `tools/print_sheets.py` (Python, reportlab) into `docs/print/`, not by the app, for now.

## Rules for app code

- **Never invent game rules in code.** If the app needs a rule that isn't in `rules/` or `content/`, stop and record it as Open in the decision log.
- **All game math lives in `Imperator.Core`**, with no web or database dependencies.
- **All randomness goes through `Rng`**, seeded. Generated artifacts record their seed and the content version.
- **Never hand-edit** `content/dist/content.json` or the generated tables in `rules/`.
- After editing any `content/*.yaml`, run `python3 tools/build_content.py && python3 tools/render_tables.py`.
- Commit messages describe the change; record architecture changes as an amendment to ADR-001 or a new ADR.
