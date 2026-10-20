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

## Commands

```sh
python3 tools/validate_content.py     # after any content/*.yaml edit
python3 tools/build_content.py        # after any content/*.yaml edit
python3 tools/render_tables.py        # after any content/*.yaml edit
python3 tools/print_sheets.py         # after content changes that affect cards (needs reportlab)
python3 tools/print_scenario.py docs/scenarios/<file>.yaml   # scenario sheet + curated cards
dotnet build app/Imperator.slnx
dotnet run --project app/tests/Imperator.Core.Tests                     # must print "All N checks passed."
dotnet run -c Release --project app/src/Imperator.Sim -- docs/analysis/sim-v0.4-results.md
dotnet run -c Release --project app/src/Imperator.Sim -- --compare Label=a.json Label2=b.json   # compare content variants
```

CI (`.github/workflows/ci.yml`) fails if content JSON or rulebook tables are stale, or if tests fail.

## Stack

.NET 10 (C#) for game logic, simulator and (later) ASP.NET Core API with EF Core + PostgreSQL; React + TypeScript (Vite) front end. See `docs/architecture/adr-001-architecture.md`. The test project is currently a dependency-free console runner; it can move to xUnit once NuGet is available in the working environment.
