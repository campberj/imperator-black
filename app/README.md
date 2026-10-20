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
| `app/src/Imperator.Core` | Game logic: dice and seeded `Rng`, content loading (`ContentSet`), wound table, shooting (exact odds and sampled), close combat, grenades, `Soldier` state | Covers rules v0.4 chapters 4–7 math with attribute-based `Soldier` (`FromTroop` builds one from a troop profile; `Generic` makes abstract test soldiers). No movement, board, or mission logic yet |
| `app/src/Imperator.Sim` | Combat simulator: shot odds, duels, frag, open-ground firefights, room clearing, `--compare` for content variants | Working. Output in `docs/analysis/` |
| `app/tests/Imperator.Core.Tests` | xUnit v3 tests (198 cases: the former runner's 72 checks plus content loading and validation); run with `dotnet test app/Imperator.slnx` | Passing |
| `content/*.yaml` → `content/dist/content.json` | Game data. Compiled by `tools/build_content.py`. .NET reads only the JSON into typed records (every file, including the chapter 10 solo opponent and its terms, chapter 11 `layouts`, chapter 12 `terrain`, `core.security` and `core.shoot_scale`) and validates it at load (`ContentValidator`, mirroring `tools/validate_content.py`) | `core`, `weapons`, `armour`, `keywords`, `gear`, `troops`, `factions`, `rosters`, `behaviors`. Schema: [content/README.md](../content/README.md) |
| `.github/workflows/ci.yml` | Checks content JSON and rulebook tables are current, builds, runs tests | Green |

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

### 3. API skeleton

- Add `app/src/Imperator.Api`: ASP.NET Core minimal API, EF Core with Npgsql.
- Add a `docker-compose.yml` at the repo root with PostgreSQL for local development.
- Data model per ADR-001 §4–5, **schema only**: `Player`, `Campaign`, `CampaignMembership` (role: Commander / Operator), and `CampaignEvent` (append-only: id, campaign id, sequence, type, JSON payload, created at, created by). No auth yet.
- Endpoints: `GET /health`, `GET /content` (serves `content.json` with its version).
- **Done when:** `docker compose up` plus `dotnet run` serves both endpoints, and an EF migration creates the schema.

### 4. Web skeleton

- Add `app/web`: Vite + React + TypeScript.
- One page that fetches `/content` and renders the weapon and armour tables, as a smoke test of the whole pipeline.
- **Done when:** `npm run dev` shows the tables from live content.

### 5. Stop and check the roadmap

Beyond this, the app's real features wait on rules:

| App feature | Waits on |
|---|---|
| Layout generator (SVG ship maps from tiles) | Ready: Phase 4 tables in `content/layouts.yaml`, rules chapter 11. `tools/generate_layout.py` is the reference implementation: port it to `Imperator.Core` and check the same seed statistics (`--check`). |
| Mission generator (briefing, enemy force, contact tables) | Phase 5: missions |
| Enemy guidance cards | Ready: Phase 3 is done (`content/behaviors.yaml`, rules chapter 10). Can follow task 4 as an in-app behavior-card view. |
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
