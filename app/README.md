# Imperator Black app — where to pick up

Handoff for Claude Code (or anyone) working on the application. Read this, then [`../CLAUDE.md`](../CLAUDE.md), then [ADR-001](../docs/architecture/adr-001-architecture.md).

Last updated: 2026-10-20, rules v0.4 (granular profiles).

## How the work is split

- **Design and rules** happen in the claude.ai Project for Imperator Black and land in `docs/`, `rules/` and `content/`.
- **Application code** happens here, in `app/`, ideally in Claude Code on RJ's machine, where NuGet and Docker are available.
- Both commit to `main` on `campberj/imperator-black`. **Pull before starting.** The [decision log](../docs/design/decisions.md) is the authority on game rules; the [roadmap](../docs/roadmap.md) says which phase is current.

## What exists

| Path | What it is | State |
|---|---|---|
| `app/Imperator.slnx` | Solution (.NET 10, new `.slnx` format) | Builds clean, 0 warnings |
| `app/src/Imperator.Core` | Game logic: dice and seeded `Rng`, content loading (`ContentSet`), wound table, shooting (exact odds and sampled), close combat, grenades, `Soldier` state, chapter 11 deck generator (`Layouts/DeckGenerator`, a port of `tools/generate_layout.py`) | Covers rules v0.4 chapters 4–7 math with attribute-based `Soldier` (`FromTroop` builds one from a troop profile; `Generic` makes abstract test soldiers). No movement, board, or mission logic yet |
| `app/src/Imperator.Api` | ASP.NET Core minimal API, EF Core + Npgsql (snake_case tables). Schema: `Player`, `Campaign`, `CampaignMembership`, append-only `CampaignEvent`. Endpoints: `GET /health` (database check + content version), `GET /content` (validated `content.json`, `Imperator-Content-Version` header, ETag), `GET /decks/{vessel}?seed=N` (a generated deck in the scenario format, with seed and content version) | Schema only, no auth. Migration `InitialSchema` |
| `app/src/Imperator.Sim` | Combat simulator: shot odds, duels, frag, open-ground firefights, room clearing, `--compare` for content variants | Working. Output in `docs/analysis/` |
| `app/tests/Imperator.Core.Tests` | xUnit v3 tests (275 cases: the former runner's 72 checks, content loading and validation, and the deck generator held to the Python reference statistics in `Fixtures/layout-reference.json`, written by `tools/layout_reference.py`); run with `dotnet test app/Imperator.slnx` | Passing |
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

### 7. Deck layout generator in Core ✅ done (2026-10-21)

`tools/generate_layout.py` is the reference implementation of rules chapter 11 (`content/layouts.yaml`).

- Port it to `Imperator.Core` (`Layouts/DeckGenerator.cs`), seeded through `Rng`, producing the same deck model (tiles, doors with state and security, cover, entry, objective, hazard, roll count).
- Match the reference statistics: run `python3 tools/generate_layout.py --check --n 2000` and assert the C# generator's compartments, loops and tile use per vessel are within a small tolerance; every deck connected and within 12 of each tile.
- `GET /decks/{vessel}?seed=N` returns the deck as JSON, recording seed and content version.
- **Done when:** tests pass and the endpoint returns a deck for each vessel.

### 8. Deck map in the web app

- Render a deck from task 7 as SVG: the 3" grid, tiles with ids and names, doors coloured by state with security, cover pieces with ids, entry and objective; the same conventions as the printed deck sheets (`docs/print/decks/`).
- Rooms, doors and terrain tables beside the map; a seed box and a vessel picker; print styles.

### 9. Stop and check the roadmap

Beyond this, the app's real features wait on rules:

| App feature | Waits on |
|---|---|
| Layout generator (SVG ship maps from tiles) | Tasks 7 and 8. |
| Land table generator (chapter 12 sector tables) | Ready after task 8, same pattern; RJ to confirm priority. |
| Custom troop and unit builder | Phase 7 point values, then Phase 9 (decision 6.12) |
| Mission generator (briefing, enemy force, contact tables) | Phase 5: missions |
| Enemy guidance cards | Task 6. |
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
