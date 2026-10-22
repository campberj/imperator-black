# Imperator Black — notes for Claude

A 28mm sci-fi narrative campaign wargame (printed rules first) plus a companion app. Read `docs/design/decisions.md` before changing rules or architecture; it is the authority, and `docs/roadmap.md` says what phase we're in.

**Working on the app? Start with [`app/README.md`](app/README.md)**: current state and the next tasks in order.

## Ground rules

- **Content is the single source of truth.** Game numbers live in `content/*.yaml`. Never hand-edit tables in `rules/*.md` between `<!-- table:… -->` markers, or `content/dist/content.json`.
- **Rules before software.** The app automates paper procedures; it never invents its own.
- **Rules math exists once**, in `app/src/Imperator.Core`. The simulator and app both use it.
- Record design decisions in `docs/design/decisions.md` (Settled / Leaning / Proposed / Open).
- Rules prose: plain, direct, second-person-free rules text; British "armour" spelling in rules and content; inches for distances.
- Setting content: no occult, demonic or supernatural elements (see `docs/design/pillars.md`, non-goals).
- **Every PDF is named `ImperatorBlack_<name>.pdf`** (RJ). The print tools add the prefix (`PREFIX` in `tools/print_sheets.py`); any new PDF output must too.

## Commands

```sh
python3 tools/validate_content.py     # after any content/*.yaml edit
python3 tools/build_content.py        # after any content/*.yaml edit
python3 tools/render_tables.py        # after any content/*.yaml edit
python3 tools/print_sheets.py         # after content changes that affect cards (needs reportlab)
python3 tools/print_scenario.py docs/scenarios/<file>.yaml   # scenario sheet + curated cards (or a deck sheet)
python3 tools/generate_layout.py <vessel> --seed N --out docs/scenarios/decks/x.yaml   # chapter 11 deck
python3 tools/generate_layout.py --check     # layout statistics over many seeds
python3 tools/layout_reference.py            # after editing layouts.yaml: reference stats for the C# deck generator's tests
dotnet build app/Imperator.slnx
dotnet test app/Imperator.slnx                                         # xUnit; must pass
docker compose up -d --wait && dotnet tool restore                    # local PostgreSQL + dotnet-ef
dotnet ef database update --project app/src/Imperator.Api             # apply migrations
dotnet run --project app/src/Imperator.Api                            # API on http://localhost:5080
npm --prefix app/web run dev                                          # web on http://localhost:5173 (proxies to the API)
npm --prefix app/web run lint && npm --prefix app/web test && npm --prefix app/web run build
dotnet run -c Release --project app/src/Imperator.Sim -- docs/analysis/sim-v0.4-results.md
dotnet run -c Release --project app/src/Imperator.Sim -- --compare Label=a.json Label2=b.json   # compare content variants
```

CI (`.github/workflows/ci.yml`) fails if content JSON or rulebook tables are stale, if tests fail, if EF migrations don't match the model, or if the web app fails lint, tests or build.

## Stack

.NET 10 (C#) for game logic, simulator and (later) ASP.NET Core API with EF Core + PostgreSQL; React + TypeScript (Vite) front end. See `docs/architecture/adr-001-architecture.md`. Tests are xUnit v3 (`app/tests/Imperator.Core.Tests`).
