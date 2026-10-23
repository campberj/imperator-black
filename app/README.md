# Imperator Black app — where to pick up

Handoff for Claude Code (or anyone) working on the application. Read this, then [`../CLAUDE.md`](../CLAUDE.md), then [ADR-001](../docs/architecture/adr-001-architecture.md).

Last updated: 2026-10-23, rules v0.5 (chapters 10–16: solo opponent, ship layouts, land battles, missions, campaign and short arcs, troop values, two players; fireteams and Pinned at Nerve + 2).

## How the work is split

- **Design and rules** happen in the claude.ai Project for Imperator Black and land in `docs/`, `rules/` and `content/`.
- **Application code** happens here, in `app/`, ideally in Claude Code on RJ's machine, where NuGet and Docker are available.
- Both commit to `main` on `campberj/imperator-black`. **Pull before starting.** The [decision log](../docs/design/decisions.md) is the authority on game rules; the [roadmap](../docs/roadmap.md) says which phase is current.

## What exists

| Path | What it is | State |
|---|---|---|
| `app/Imperator.slnx` | Solution (.NET 10, new `.slnx` format) | Builds clean, 0 warnings |
| `app/src/Imperator.Core` | Game logic: dice and seeded `Rng`, content loading (`ContentSet`), wound table, shooting (exact odds and sampled), close combat, grenades, `Soldier` state, chapter 11 deck generator (`Layouts/DeckGenerator`, a port of `tools/generate_layout.py`; the deck keeps its graph: links, spine, heart, with `RouteFrom` for route distances), chapter 12 land table generator (`Land/LandTableGenerator`, a port of `build_land` in `tools/generate_mission.py`), chapter 13 mission generator (`Missions/MissionGenerator`, a port of `tools/generate_mission.py`) | Covers rules v0.4 chapters 4–7 math with attribute-based `Soldier` (`FromTroop` builds one from a troop profile; `Generic` makes abstract test soldiers). No movement, board, or mission logic yet |
| `app/src/Imperator.Api` | ASP.NET Core minimal API, EF Core + Npgsql (snake_case tables). Schema: `Player`, `Campaign`, `CampaignMembership`, append-only `CampaignEvent`. Endpoints: `GET /health` (database check + content version), `GET /content` (validated `content.json`, `Imperator-Content-Version` header, ETag), `GET /decks/{vessel}?seed=N` (a generated deck in the scenario format, with seed and content version), `GET /land/{environment}?size=small|large&seed=N` (a generated land table), `GET /missions?setting=ship|land&seed=N` (a generated mission; optional `mission`, `tier`, `vessel`, `environment`, `size`) | Schema only, no auth. Migration `InitialSchema` |
| `app/src/Imperator.Sim` | Combat simulator: shot odds, duels, frag, open-ground firefights, room clearing, `--compare` for content variants, `--values` for the troop value harness (`Values.cs`) | Working. Output in `docs/analysis/` |
| `app/tests/Imperator.Core.Tests` | xUnit v3 tests (399 cases: the former runner's 72 checks, content loading and validation, and the deck generator held to the Python reference statistics in `Fixtures/layout-reference.json`, written by `tools/layout_reference.py`); run with `dotnet test app/Imperator.slnx` | Passing |
| `app/tests/Imperator.Api.Tests` | xUnit v3 + `WebApplicationFactory` (40 cases): endpoints (including `/decks`) and schema, no database needed | Passing |
| `app/tests/Imperator.Sim.Tests` | xUnit v3 (4 cases): the troop value harness, including the reference trooper scoring 50% against itself | Passing |
| `content/*.yaml` → `content/dist/content.json` | Game data. Compiled by `tools/build_content.py`. .NET reads only the JSON into typed records (every file, including the chapter 10 solo opponent and its terms, chapter 11 `layouts`, chapter 12 `terrain`, `core.security` and `core.shoot_scale`) and validates it at load (`ContentValidator`, mirroring `tools/validate_content.py`) | `core`, `weapons`, `armour`, `keywords`, `gear`, `troops`, `factions`, `rosters`, `behaviors`. Schema: [content/README.md](../content/README.md) |
| `app/web` | Vite + React + TypeScript, read from `/content` (dev server proxies to the API). Unit database (`#units`: troop cards per faction with filters and search, teams, elements), reference (`#reference`: gear, skills, keywords, weapon and armour tables, searchable), behavior cards (`#behaviors`), missions (`#missions?setting=…&seed=…`, or an example from `docs/scenarios/missions`), land tables (`#land?environment=…&size=…&seed=…`), deck maps (`#decks?vessel=…&seed=…`: SVG map of a generated deck with rooms, doors and terrain tables), the printable mission dashboard (`#dashboard`, blank or for a mission). Vitest + Testing Library, oxlint. See [web/README.md](web/README.md) | Read-only |
| `docker-compose.yml` (repo root) | PostgreSQL 18 for local development | |
| `.github/workflows/ci.yml` | Checks content JSON and rulebook tables are current, builds, runs tests, checks EF migrations match the model and apply to PostgreSQL; a `web` job lints, tests and builds `app/web` | Green |

**NuGet:** the cloud workspace where this started blocks `api.nuget.org`, which is why content is compiled YAML → JSON (System.Text.Json is built in) and why the tests began as a console runner. Locally, NuGet works. Keep the YAML → JSON step regardless; the web front end will read the same JSON.

**`global.json`** (repo root) pins SDK 10 and opts `dotnet test` into Microsoft.Testing.Platform, which xUnit v3 needs on the .NET 10 SDK.

## Next tasks, in order

These are foundations that don't depend on unfinished rules. Each should leave CI green.

**Open tasks, suggested order: 15 (in PR #14), 17, 16, 18, 19, 20, 21, then 22.** Tasks marked *chatgpt authorized* may be picked up by another agent without asking RJ first: they have a reference implementation or a fixed spec, and tests decide when they are done. Unmarked tasks need RJ's say-so (or a design pass in the claude.ai Project) before starting.

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

### 9. Board map in the web app ✅ done (2026-10-22)

- Render a deck from task 8 as SVG: the 3" grid, tiles with ids and names, doors coloured by state with security, cover pieces with ids, entry and objective; the same conventions as the printed deck sheets (`docs/print/decks/`).
- Rooms, doors and terrain tables beside the map; a seed box and a vessel picker; print styles.
- Build the map as one component that takes a board model (tiles optional), so tasks 10 and 12 reuse it for land tables and mission markers: 12" sector lines with names (A1 at the player's bottom left), terrain footprints coloured by type (`tools/print_scenario.py` has the colours), guard models, contact chips (`?n`), objectives and the enemy entry.

### 10. Land table generator in Core ✅ done (2026-10-22)

The reference is `Mission.build_land` in `tools/generate_mission.py` (rules chapter 12, "Setting up a land table").

- `Land/LandTableGenerator.cs`: environment (named or rolled; fortified only when named), board by size (small 36" × 36", large 48" × 48"), one d6 per 12" sector, the density check (three sectors in a line with no `pieces` → the middle one becomes the fill, repeated until none), the conditions roll.
- Place each roll's `pieces` near the sector's middle as the reference does: the first piece centred; rocks on a hill placed on it; others offset clear of earlier pieces and inside the sector. Which way a piece faces is the player's choice in the rules, so here it comes from `Rng`.
- `GET /land/{environment}?size=small&seed=N`; show it in the web app with the task 9 map.
- **Done when:** a test checks that no line of three empty sectors remains over 1,000 seeds per environment, every piece lies inside its sector, and the endpoint returns a table.

### 11. Mission generator in Core ✅ done (2026-10-22)

The reference is `tools/generate_mission.py`; the procedure is rules chapter 13, "Building a mission".

- `Missions/MissionGenerator.cs`, seeded, in the paper order: mission type and threat tier (named or rolled), board (task 8 deck or task 10 table), complication, objectives, guard, contact chips, enemy entry, clock, briefing.
- Aboard ship, "farthest" means farthest from the airlock by route over the deck graph (fewest tile steps); ties go to the straight-line distance from the airlock. On land, "farthest" means farthest from the middle of the player's edge.
- Complications change the mission: Early warning sets the alert start to 1; Reinforced adds a contact; Tight window takes 5 minutes off; Hazard gives the vessel's first hazard aboard ship, or Night on land in place of the conditions roll.
- Guard: one roll on the tier table, rerolling false alarm and booby trap, models placed within 3" of the first objective and clear of cover; a capture mission adds the HVT to the guard unless it is already in it.
- The model records everything the printed sheet shows: seed, content version, the roll log, briefing headings in the `briefing` order, objectives, guard models, contact chips, enemy entry and any second enemy exit, clock (die, limit, check-ins), alert start, reinforcement element and HVT.
- `GET /missions?setting=ship&seed=N` (optional `mission`, `tier`, `vessel`, `environment`, `size`).
- **Done when:** over 2,000 seeds per setting, the mission and tier frequencies match their d6 tables within tolerance; every contact chip and guard model stands inside the board, clear of solid cover; objective counts match the mission type; the endpoint returns a mission.

### 12. Mission view in the web app ✅ done (2026-10-22)

- A mission page: the briefing, then the task 9 map with objectives, guard models (numbered), contact chips and the enemy entry. Below it: the mission type's taking-it, lockdown and grades text; the alert track and clock rules; the tier's contact table with element compositions and behaviors; the enemy elements with drill (the drill rules are in the chapter 10 content; `tools/print_scenario.py` `element_drill` is the reference).
- Rooms and doors (ship) or sectors and terrain (land) tables; a seed box and pickers for setting, mission, tier, vessel or environment, and size.
- Print styles that come close to `docs/print/missions/ImperatorBlack_mission-*-sheet.pdf`.
- Out of scope: the live dashboard (it needs saved game state; it comes with the Phase 7 campaign tracker).
- **Done when:** both example missions in `docs/scenarios/missions/` can be shown, and lint, tests and build pass.

### 13. Troop value harness in the simulator ✅ done (2026-10-22)

Report: [`docs/analysis/troop-values-v0.5.md`](../docs/analysis/troop-values-v0.5.md). Each duel activation is Aim then Shoot (Rally in place of Aim when suppressed), with initiative each round, so Marksman and Leader count; a draw counts as half a win, so the reference scores 50% against itself. Core gained the Proposed skills the duels need: Augmented (first Suppression each round), Jedi discipline (Pinned at 4), Shield (Light cover when it has no other), Marksman 2 (an Aimed shot ignores Light cover) and Twin blades; `Soldier.StartRound` resets per-round state. Not modelled: Laser detector (no reactions), Deflect, Self-destruct and the grenade launchers.

Phase 7 needs a points value for every troop (decision 6.12, roadmap Phase 7 item 6), calibrated with the simulator. Build the data first; the points formula is a design decision that comes after.

- New simulator mode `--values` (`app/src/Imperator.Sim/Values.cs`): every troop profile in `content/troops.yaml`, with its default loadout, against a **reference trooper** (Task Force Boone VAT Assaulter, `tfb_vat_assaulter`).
- Three measures, each with a fixed seed and at least 20,000 runs: **duel at 12"** (open ground, alternate Shoot actions, first to put the other Down or worse wins); **duel at 24" with the target in Light cover**; **close combat** (the existing exchange rules). Also the expected wound rows inflicted per Shoot action against Armour 0, 2 and 3, so Auto, AP and the new heavy weapons show up.
- Report: a Markdown table per faction to `docs/analysis/troop-values-v0.5.md`, sorted by an overall score (the mean of the three win rates), with Move, Shoot, Wounds and Armour beside it. Mark Automated models and profiles with Proposed skills.
- Handle models with no ranged weapon (Jedi: close combat only; their ranged duels are N/A), Automated models (no Suppression, destroyed on row 6+), and Wounds 2–3 (more markers before Down).
- Skills that change the math are modelled where Core already supports them (Marksman, Augmented, Laser detector's no-penalty reactions if reactions are simulated); list the ones that are not modelled under the table.
- **Done when:** the report covers all 73 troop types and a test checks the reference trooper scores 50% (±2%) against itself.

### 14. Mission dashboard in the web app (print) ✅ done (2026-10-22)

`app/web/src/views/Dashboard.tsx`; checked in headless Chromium: the blank, a small and a large mission each print on one letter page.

The paper dashboard (`docs/print/ImperatorBlack_mission-dashboard.pdf`, drawn by `dashboard_flow` in `tools/print_scenario.py`) as a printable page for a generated mission. No saved state.

- From the mission view (task 12), a **Dashboard** button opens a print-ready page: the alert track 0–6 (start value shaded, thresholds 2/4/6 marked and named), the clock with one box per minute up to the limit (check-ins and the limit shaded), a rounds-played row of 15, the objectives with status boxes, the contact chips with "revealed as" and "state" columns, the enemy elements (the guard and the reinforcement, with U / A / E awareness), and a player force table (8 rows for a small mission, 12 for a large one).
- Also a blank dashboard at `#dashboard` (30-minute clock, labels for both sizes), matching the blank PDF.
- One letter page in print; the print CSS hides the app's navigation.
- **Done when:** a Vitest test renders the dashboard for both example missions in `docs/scenarios/missions/` and checks the clock box count, the check-in boxes and the contact rows, and lint, tests and build pass.

### 15. Launcher rounds and Explosion (X) in the web app ✅ done (2026-10-23)

Content now has half-step damage dice (`d8+d10`; `DiceSpec` parses them, with an `Extra` die) and two new ranged-weapon fields (decisions 6.14 and 6.15): `explosion` (the X in Explosion (X)) and, for launchers, `round` (the weapon's own round) and `rounds` (extra rounds: id, name, damage, AP, explosion). The *Blast* keyword is now *Explosion*; grenades keep `blast_radius` as their X.

- Unit database and reference: show each launcher's rounds under the weapon (as the printed cards do: "Anti-armour above, or high explosive round: d8+d10, AP 0, Explosion (2). Choose the round before each Shoot."), and show Explosion (X) beside the keywords.
- Simulator: model a high-explosive round in the troop value harness where a profile carries a launcher (use the better round against each target), and note it under the table.
- **Done when:** a Vitest test renders the AT launcher's two rounds, and lint, tests and build pass.

### 16. Troop values in Core ✅ done (2026-10-23)

Rules chapter 15 (decision 6.16): every troop has a Troop Value (TV) and a Requisition cost. `tools/troop_values.py` is the reference implementation; `content/values.yaml` holds every number.

- `Imperator.Core/Values/TroopValue.cs`: load `values` into typed records (`ContentSet`), and compute weapon base values (lethality averaged over Armour 0–2, range, Auto, Explosion, keywords, best round), melee base values, the survival multiplier (expected 2d8 hits to Down, to the power 0.4, rounded to 2 decimals), and a troop's TV and Requisition for its default loadout or a given one. Round exactly as the Python does (weapon and melee costs to 0.1, survival to 0.01, TV to the nearest whole number).
- Tests against the fixture `app/tests/Imperator.Core.Tests/Fixtures/troop-values-reference.json` (written by `python3 tools/troop_values.py --reference`; CI fails if it is stale): every troop's TV and Requisition, every weapon base and every survival value must match.
- `GET /values` returns every troop's TV and Requisition; the unit database shows them on each card.
- Nerve now costs 3 per point (decision 4.19; the fixture already reflects it).
- **Done when:** tests pass and the unit database shows TV and Requisition. The builder screen (pick attributes and kit within the chapter 15 limits, see the TV update live) is Phase 9, after this.

### 17. Bring the C# mission generator level with the Python one ✅ done (2026-10-23)

**Do this first: the two have drifted.** The reference is `tools/generate_mission.py`; the printed sheets come from `tools/print_scenario.py`. Changes since task 11 (2026-10-22 and 23):

- **Oppositions** (decision 8.14, chapter 13 *Other oppositions*): `missions.yaml` has `oppositions` (first: `separatist`), each with its own four tiers. `MissionsContent.Oppositions` and `ContentSet.Tier(tier, opposition)` already exist in Core. Add an `opposition` option to `MissionGenerator` and `GET /missions?opposition=separatist`, record it in the mission model, and use the opposition's tiers for the guard, contacts, reinforcement and HVT, and its faction name in the briefing.
- **Awareness** (decision 8.14): only elements with the *Automated* behavior profile start Alerted. A guard from such an element is placed "Alerted", and the briefing's Opposition line says "Alerted" instead of "Unaware".
- **Briefing text:** "an industrial position" (a/an by the environment's first letter).
- **Authored missions** (short arcs, chapter 14): mission files may carry `arc` (`id`, `name`, `label`), `task`, `lockdown` and `grades` (replacing the mission type's text), `reinforcement` (replacing the tier's), `special_rules` (title, text), `next` (grade, text), `map_note`, extra `briefing` headings (such as `Force`), and land boards with no `sectors` or `rolls`. Add them to the mission model as optional fields, and show them in the mission view (task 12): taking it, lockdown and grades from the mission when given, then *After the mission* and *Special rules*, as `mission_sheet` in `tools/print_scenario.py` does.
- **Done when:** tests check that a Separatist mission draws only Separatist elements (tier 4 HVT `sep_grievous`); a mission whose guard has the Automated profile places it Alerted; the web mission view shows all four Foundry Moon missions in `docs/scenarios/arcs/foundry-moon/`; lint, tests and build pass.

### 18. Fireteams in the web unit database ✅ done (2026-10-23)

Decision 6.18, rules chapter 9 *Building a force*, design note `docs/design/fireteams.md`.

- **Core:** load `rosters.fireteam_types` and `rosters.classes` (records `FireteamTypeDef` and `TroopClassDef` exist; `ContentValidator` already checks every standard fireteam). Move the fit check into a reusable `Fireteams.Check(type, troops)` in Core that the validator, the web app (through the API or a ported copy, see below) and task 20 all use: leaders, troopers and specialists within the type's ranges, at most `max_models` (characters not counted), at most one character; `character` as a type means one character acting alone.
- **Unit cards:** each troop's class (top right of the card header, as the printed cards) and its **Pinned threshold** under the stat line ("Pinned at 4 Suppression", "Never Pinned"; Automated models show none). The threshold is Nerve + `core.suppression.pinned_by_nerve`, never above `core.suppression.max` (decision 4.19).
- **Fireteams view:** the class table and fireteam types table (chapter 9), then the standard fireteams of the chosen faction: teams and enemy elements in one list, each with its type, models and (for enemies) tier and behavior. "Fireteam" and "element" mean the same thing.
- If the web app checks fits itself, port the rule in TypeScript with a test that agrees with the C# one on every standard fireteam.
- **Done when:** a Vitest test renders a clone trooper card with "Trooper" and "Pinned at 4 Suppression", Obi-Wan with "Never Pinned", and the VAT fireteam as Assault; lint, tests and build pass.

### 19. Short arc viewer (web, read-only) ✅ done (2026-10-23)

Rules chapter 14 *Short arcs* (decision 7.13). The paper version is `tools/print_campaign.py --arc` (`ImperatorBlack_arc-foundry-moon.pdf`).

- `#arcs`: the arcs in `docs/scenarios/arcs/` (bundled like the example missions). For one arc: its summary; the mission flow (mission 1, then 2A on Decisive or Marginal and 2B on Failure, then 3); the missions table (board, opposition and tier, force limit); *What each grade leads to*, from each mission's `next`; the short arc rules (`content/campaign.yaml` `short_arc`, not yet loaded: read it from `/content`); the arc's results.
- Each mission opens in the mission view (task 17 makes the authored fields show).
- **Done when:** a Vitest test renders Foundry Moon's flow with all four missions and the 2B branch; lint, tests and build pass.

### 20. Force planner (web, no saved state) ✅ done (2026-10-23)

Needs tasks 16 (TV in Core) and 18 (fireteam check in Core) merged first. RJ's brief: structure where it helps, room to customize, **no heavy army building**. The planner is a calculator that shows the rules; it never blocks the player.

**Where the rules live.** Every check and every total is computed in `Imperator.Core` (CLAUDE.md: rules math exists once), exposed by the API, and shown by the web app. No force rule in TypeScript.

- **Core:** `Forces/ForcePlan.cs` (the plan: faction, setting, limits, fireteams with their models and loadout choices, characters with "joins fireteam N" or "alone", for by-soldier a flat list of models with a group number) and `Forces/ForceCheck.cs` (returns the total TV, the model count, and a list of findings, each with a code, the fireteam or model it concerns, and a message).
- **API:** `POST /forces/check` takes a plan as JSON and returns the totals and findings, with the content version.
- **Web:** `#planner`, calling the API as the plan changes (debounced). The plan lives in the URL (encoded in the hash) so a force can be bookmarked or shared; no database, no accounts.

**Limits.** Pick one of: a two-player size (`versus.yaml` `sizes`: Patrol 350 TV, Strike force 600 TV); a mission size (`missions.yaml` `clock.sizes`: up to 8 or 12 models); a short arc mission (each mission file in `docs/scenarios/arcs/` has `force_limit: { tv, models }`); or custom numbers. The limits can always be edited by hand (Foundry Moon mission 3 is 50 TV higher or lower after 2B).

**The setting** (chapter 14 *Force selection*, `campaign.yaml` `force_selection`): **by fireteam** (default) or **by soldier**. Two-player sizes force by fireteam.

**By fireteam** (chapter 9 *Building a force*):
- Add a **standard fireteam** of the faction (its `teams` for player factions, its `elements` for enemy ones), then adjust each slot's count from 0 up to the standard count. Or **build one to a type**: pick Line, Assault or Support and add the faction's troops by class.
- Characters: add any troop of class `character`; choose "joins" (pick a fireteam) or "alone".
- Loadouts: choose each model's weapon, armour and gear options from its profile. The TV shown is the troop's listed TV whatever its loadout (chapter 16's rule; exact loadout costs are the Phase 9 builder).

**By soldier:** add any troops of the faction; assign each to a group (fireteams of up to 6); characters join a group or act alone.

**Findings** (warnings, never blocking; wording plain and taken from the rules):

| Code | When | Message (example) |
|---|---|---|
| `over_tv` | Total TV above the limit | "312 TV: 12 over the 300 limit." |
| `over_models` | Models above the limit | "8 models: 1 over the limit of 7." |
| `half_slots` | A fireteam fields fewer than half its slots, rounded up (by fireteam) | "Clone trooper squad: 2 of 5 slots; at least 3." |
| `no_leader` | A fireteam whose standard version has a Leader fields none | "Clone trooper squad: its Leader must be fielded." |
| `type_limits` | A fireteam breaks its type's class ranges or most models (task 18's check) | "Line: 2 specialists; at most 1." |
| `two_characters` | More than one character joins the same fireteam | "VAT fireteam: two characters; at most one." |
| `group_size` | A by-soldier group above 6 | "Group 2: 7 models; at most 6." |
| `wrong_faction` | A troop from another faction | "Kraken is not a Galactic Republic troop." |

**Output.** Running totals (TV, models, fireteams, characters) beside the plan; the findings list; a **print** view: one letter page with the force name, setting, limits, each fireteam (type, models, loadouts, TV) and each character (joined or alone), in the style of the printed sheets. Link each troop to its unit card (`#units`).

**Not in scope:** saving forces, the campaign roster, custom troops (attributes and kit), exact loadout TV, the enemy's forces.

**Done when:**
- Core tests cover each finding, including the rounding of half slots (5 slots: 3; 4 slots: 2), a joined Leader character leading a leaderless fireteam, and a lone character never breaking a rule by being alone.
- An API test posts Gym 2's force (Ahsoka joined to a clone squad) and gets its TV from task 16, with no findings at 350 TV and `over_tv` at 300.
- A Vitest test builds that force in the planner, sees the totals, switches to by soldier, and prints.
- Lint, tests and build pass; RJ reviews the screen before the PR is merged.

### 21. Campaign and two-player content in Core ✅ done (2026-10-23)

`content/campaign.yaml` (chapter 14, including `short_arc` and `force_selection`) and `content/versus.yaml` (chapter 16) are in `content.json` but not loaded by `ContentSet.Parse`.

- Typed records for every section of both files, loaded as required sections; port their checks from `tools/validate_content.py` (campaign: d6 and 2d6 tables cover their ranges, node missions and settings exist, the start force exists; versus: whatever `tools/validate_content.py` checks for it), with one negative test per check.
- No screens: this is the groundwork for the campaign tracker.
- **Done when:** tests load both sections and CI is green.

### 23. S1: native PostgreSQL and solo campaign persistence ✅ done (2026-10-23)

**Scope (approved S0 roadmap):** a database-backed single-player campaign lifecycle; no Foundry Moon gameplay screens, strategic turns, multiplayer identity, game rules, or outpost mechanics. The current development API uses one stable server-configured *local* player, and is available only in Development until authentication exists.

- Homebrew PostgreSQL 18 guide: [`app/docs/native-postgresql-macos.md`](docs/native-postgresql-macos.md). Docker is optional. Use the existing EF schema and apply the existing migration, without generated migration edits.
- A pure `Imperator.Core` lifecycle projection (`DecideCreate`, `DecideRename`, `Apply`, `Replay`) and immutable creation/rename events; events are not fabricated game-rule outcomes.
- Development-only `POST /campaigns`, `GET /campaigns`, `GET /campaigns/{id}`, `GET /campaigns/{id}/events`, `PATCH /campaigns/{id}/name`. The event log is authoritative and membership filters each read.
- Reuse Player, Campaign, CampaignMembership and CampaignEvent tables; no required schema changes in S1.

**Done when:**
1. On macOS with native PostgreSQL (no Docker), the documented setup creates/connects a development database, applies migrations, and the API health check reports healthy.
2. A single development player can create, list, load and rename a solo campaign; the immutable event log records creation and rename, and all reads are scoped to that player.
3. `Imperator.Core` replays the event history deterministically, rejects gaps, duplicates, unknown events and invalid names, and the state remains identical after a fresh API instance loads it.
4. A real PostgreSQL integration test covers create → rename → API restart → load/list/event replay, including isolation between local player IDs. CI tests use its ephemeral PostgreSQL service.
5. Native backup/restore instructions preserve data without resetting the working database. Web lint/tests/build, .NET tests and migration checks are green. No unapproved campaign mechanics or S2 screens.

**S1 implementation complete:** native macOS setup is documented; the real PostgreSQL/API restart and event-replay checks pass in CI. Local Homebrew installation remains for RJ to verify after merging.

---

### 22. Stop and check the roadmap

Beyond this, the app's real features wait on rules:

| App feature | Waits on |
|---|---|
| Live mission dashboard (alert, clock, contacts, models during play) | Saved game state; built with the Phase 7 campaign tracker (RJ, 2026-10-22). The paper dashboard is `docs/print/ImperatorBlack_mission-dashboard.pdf`. |
| Custom troop and unit builder | Ready after tasks 16, 18 and 20: chapter 15 limits and costs; Phase 9 (decision 6.12) |
| Campaign tracker | Phase 7: chapter 14 is drafted (loaded by task 21); build after a paper campaign (Foundry Moon) has been played, with saved game state and the live dashboard. |

Planned app feature (RJ, 2026-10-20): an in-app reference that explains every gear item, keyword and special rule, read from `content/` (the list will grow). Build it from the same `text` fields the rulebook tables use.

Unit cards and the quick reference are generated by `tools/print_sheets.py` (Python, reportlab) into `docs/print/`, not by the app, for now.

## Pull requests

For any agent working from this list:

- **One task per pull request**, named after its task ("Task 15: launcher rounds in the web app"). Don't bundle tasks: a draft holding seven tasks can't be reviewed or merged piece by piece.
- **CI green before asking for review.** Run `npm --prefix app/web run lint && npm --prefix app/web test && npm --prefix app/web run build` and `dotnet test app/Imperator.slnx` locally first. A changed screen often breaks an older test (a cell's accessible name, a row count): fix the cause, don't loosen the test.
- **Quote the task's "Done when" in the PR description** and say how each point is met.
- **When a task merges**, mark it ✅ done (with the date) in this README in the same PR.
- **Don't change** `content/`, `rules/`, `docs/design/` or `docs/scenarios/` in an app PR. If the app needs something there, say so in the PR and RJ (or the design project) adds it.
- Rebase on `main` before review; main moves daily.

## Rules for app code

- **Never invent game rules in code.** If the app needs a rule that isn't in `rules/` or `content/`, stop and record it as Open in the decision log.
- **All game math lives in `Imperator.Core`**, with no web or database dependencies.
- **All randomness goes through `Rng`**, seeded. Generated artifacts record their seed and the content version.
- **Never hand-edit** `content/dist/content.json` or the generated tables in `rules/`.
- After editing any `content/*.yaml`, run `python3 tools/build_content.py && python3 tools/render_tables.py`.
- Commit messages describe the change; record architecture changes as an amendment to ADR-001 or a new ADR.
