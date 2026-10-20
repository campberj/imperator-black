# imperator-black
A multi domain, narrative driven wargame experience

Imperator Black is a 28mm science fiction narrative campaign wargame spanning three layers: tabletop boarding actions and skirmishes, the player's ship or outpost, and a sector-level campaign map. A companion web application generates missions and layouts and, in time, hosts shared multiplayer campaigns.

## Status

Proof of concept in progress: solo ship and station boarding actions with a full campaign loop. See the [roadmap](docs/roadmap.md).

## Rules

- [Rules v0.3 (playtest draft)](rules/README.md)

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

Requires Python 3 with PyYAML, and the .NET 10 SDK.

```sh
python3 tools/build_content.py      # compile content/*.yaml to content/dist/content.json
python3 tools/render_tables.py      # regenerate rulebook tables from content/
dotnet run --project app/tests/Imperator.Core.Tests                                   # tests
dotnet run -c Release --project app/src/Imperator.Sim -- docs/analysis/sim-v0.3-results.md   # simulator report
```
