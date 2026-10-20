# ADR-001: System Architecture

**Status:** Accepted (2026-10-20)

## Context

Imperator Black is a printed tabletop game first and a web application second. The proof of concept needs an app that generates ship layouts, missions, and enemy forces, and that tracks a solo campaign. Later, the app must host shared multiplayer campaigns on the internet.

Two risks drive this design:

1. **Drift between book and app.** If tables live in two places, the printed rules and the app will disagree within weeks.
2. **A rewrite at multiplayer time.** A single-user app built around "my campaign" will not stretch to shared campaigns without starting over.

## Decision

### 1. Monorepo

```
imperator-black/
├── docs/            Design docs, decision log, roadmap, ADRs, playtest logs
├── rules/           Rulebook chapters (Markdown) → built to PDF and web
├── content/         Game data (YAML) + JSON Schemas: weapons, armor, troop
│                    types, units, enemy profiles, behavior tables, mission
│                    tables, tile inventory, campaign tables
├── tools/           Dice/probability simulator, content validator,
│                    table renderer (content → Markdown tables for the book)
└── app/
    ├── src/Imperator.Core/     Pure C# game logic: content model, seeded RNG,
    │                           generators, campaign rules. No web dependencies.
    ├── src/Imperator.Api/      ASP.NET Core Web API, EF Core, PostgreSQL
    ├── tests/                  xUnit tests for Core
    └── web/                    React + TypeScript (Vite)
```

### 2. Content as data, single source of truth

Every table a player rolls on lives once, in `content/`. The rulebook's tables are **generated** from those files, and the app reads the same files. Paper play and app play always agree, and balance changes happen in one place. Content is versioned with the repo.

### 3. Seeded, reproducible generation

Every generated mission records a **seed and content version**. The same seed reproduces the same layout, mission, and enemies. This supports bug reports, sharing a mission with a friend, and auditable results in multiplayer.

### 4. Campaign state as an event log

Campaign changes are stored as an append-only sequence of events (`TaskingIssued`, `MissionGenerated`, `AfterActionReported`, `CasualtyResolved`, `UpgradePurchased`, …). Current state (roster, ship, sector) is derived from the events.

This gives a campaign history feed for free, makes disputes and corrections in shared campaigns tractable, and maps cleanly onto asynchronous multiplayer turns. It stays deliberately simple: one events table plus projection code in `Imperator.Core`, not a full event-sourcing framework.

### 5. Multiplayer-shaped from day one

`Campaign`, `Player`, and `CampaignMembership` (with a role such as Commander or Operator) exist in the schema from the PoC onward, even with one player. Authentication is deferred. Adding it later (ASP.NET Core Identity or an external OpenID Connect provider) is additive, not a rewrite.

### 6. Printable outputs

The generator produces a **printable mission briefing** and an **SVG map**. The map shows tile placements by size (6×6 / 6×3 / 3×3) on a 3" grid, doors and hatches, numbered token positions, objectives, and entry points. Only tiles listed in the owner's tile inventory (`content/tiles.yaml`) are used.

## Technology choices

| Layer | Choice | Why |
|-------|--------|-----|
| Game logic | C# (.NET 10, current LTS), class library | Strong typing suits rules logic; testable in isolation; matches existing C#/SQL experience |
| API | ASP.NET Core Web API + EF Core | Mature, fast, first-class auth story for later |
| Database | PostgreSQL | Free, cheap to host anywhere; EF Core makes it interchangeable with SQL Server if needed |
| Front end | React + TypeScript (Vite) | Ubiquitous, good SVG/print support |
| Content | YAML, compiled to JSON | Human-editable, diff-friendly; the compiled JSON is what .NET and the web app read, so neither needs a YAML library |
| Rulebook build | Markdown → PDF (Pandoc) and static site | Reuses the same Markdown; tables injected from content |
| Local dev | Docker Compose (Postgres) | One command to run |
| CI | GitHub Actions | Validate content, run tests, build rulebook |

Hosting is deferred to the multiplayer phase. Everything is containerizable, so any container host works.

## Consequences

- Two languages (C# and TypeScript). Accepted for the clean split between rules logic and UI.
- Content schema design is an early, important task (roadmap Phase 2).
- The PoC app is small: a generator and a campaign tracker. Most PoC effort goes into the rules, as it should.

## Amendments

**2026-10-20.** Content is compiled from YAML to `content/dist/content.json` by `tools/build_content.py`; `Imperator.Core` reads it with `System.Text.Json`. The combat simulator lives in `app/src/Imperator.Sim` and uses `Imperator.Core`, so the rules math exists once. Tests are a dependency-free console runner (`app/tests/Imperator.Core.Tests`) so they run without package downloads. CI (`.github/workflows/ci.yml`) checks that content JSON and rulebook tables are current and runs the tests.

**2026-10-20.** The console test runner is replaced by an xUnit v3 project at the same path, run with `dotnet test app/Imperator.slnx`. The statistical checks keep their fixed seeds and tolerances.
