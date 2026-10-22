# imperator-black
A multi domain, narrative driven wargame experience

Imperator Black is a 28mm science fiction narrative campaign wargame spanning three layers: tabletop boarding actions and skirmishes, the player's ship or outpost, and a sector-level campaign map. A companion web application generates missions and layouts and, in time, hosts shared multiplayer campaigns.

## Status

Proof of concept in progress: solo ship and station boarding actions with a full campaign loop. See the [roadmap](docs/roadmap.md).

## Rules

- [Rules v0.5 (playtest draft)](rules/README.md)
- [Printable unit cards and quick reference](docs/print/) (PDF)
- [Scenarios](docs/scenarios/README.md): Gym 1: First Breach

## Documents

- [Initial design document](docs/design/initial-design-document.md)
- [Design pillars and non-goals](docs/design/pillars.md)
- [Design decision log](docs/design/decisions.md)
- [Glossary](docs/design/glossary.md)
- [Playtest log template](docs/playtests/TEMPLATE.md)
- [PoC opposition force: Freespace Solutions](docs/design/opposition-force.md)
- A War of Worlds (predecessor design): [design statement and roadmap](docs/design/a-war-of-worlds/design-statement-and-roadmap.md), [prototype rules v0.1](docs/design/a-war-of-worlds/prototype-rules-v0.1.md)
- [Proof of concept roadmap](docs/roadmap.md)
- [Simulator findings, rules v0.2 → v0.3](docs/analysis/sim-v0.2-findings.md)
- [Simulator findings, rules v0.4 profiles](docs/analysis/sim-v0.4-findings.md)
- [ADR-001: System architecture](docs/architecture/adr-001-architecture.md)

## Repository layout

| Folder | Contents |
|--------|----------|
| `docs/` | Design documents, decision log, roadmap, architecture decisions, playtest logs |
| `rules/` | Rulebook chapters in Markdown |
| `content/` | Game data (YAML) shared by the rulebook and the app |
| `tools/` | Dice simulator, content validator, table renderer |
| `app/` | Application code (C# game logic and API, React front end) |

## Working with the repo

Requires Python 3 with PyYAML, the .NET 10 SDK, Docker (for the API's PostgreSQL), and Node.js 24+ (for the web app). A local virtualenv keeps the Python tools self-contained: `python3 -m venv .venv && .venv/bin/pip install pyyaml reportlab`, then run the tools with `.venv/bin/python` (`.venv/` is gitignored).

```sh
python3 tools/validate_content.py   # check content cross-references
python3 tools/build_content.py      # compile content/*.yaml to content/dist/content.json
python3 tools/render_tables.py      # regenerate rulebook tables from content/
python3 tools/print_sheets.py       # regenerate unit cards and quick reference (needs reportlab)
dotnet test app/Imperator.slnx                                                        # tests
dotnet run -c Release --project app/src/Imperator.Sim -- docs/analysis/sim-v0.4-results.md   # simulator report
```

Running the API and web app locally is described in [app/README.md](app/README.md).
