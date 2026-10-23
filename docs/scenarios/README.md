# Scenarios

Fixed scenarios for playtesting. Each is a YAML file holding the layout, forces with their loadouts, special rules and victory conditions. Cover pieces carry a map id and a suggested terrain piece, printed in the sheet's Terrain table, and `walls_and_doors` says what to build walls and doors from. Each enemy model names its element, which sets its behavior profile (rules chapter 10). The mission generator (Phase 5) will grow out of this format.

Print a scenario's sheet and cards with:

```sh
python3 tools/print_scenario.py docs/scenarios/<scenario>.yaml
```

This writes `docs/print/ImperatorBlack_<id>-sheet.pdf` (scenario sheet with a to-scale map) and `docs/print/ImperatorBlack_<id>-cards.pdf` (one card per model, with its chosen loadout, plus the solo opponent reference cards and a behavior card for each enemy profile in play). Profiles come from `content/`, so reprinting picks up any rules changes.

| Scenario | Purpose | Print |
|---|---|---|
| [Gym 1: First Breach](gym-01-first-breach.yaml) | Phase 2 exit playtest: Boarding element vs four Freespace asset security on five tiles; doors, a locked door, Overwatch, close quarters, an extraction | [sheet](../print/ImperatorBlack_gym-01-first-breach-sheet.pdf), [cards](../print/ImperatorBlack_gym-01-first-breach-cards.pdf) |
| [Gym 2: Silence the Relay](gym-02-silence-the-relay.yaml) | First land test (chapter 12), built from RJ's 35" × 43" desert table: a Republic clone squad and Ahsoka Tano against a UN reserve fireteam holding a mesa relay, with Pointmen as the reaction force | [sheet](../print/ImperatorBlack_gym-02-silence-the-relay-sheet.pdf), [cards](../print/ImperatorBlack_gym-02-silence-the-relay-cards.pdf) |
| [Gym 3: Wipe Race](gym-03-wipe-race.yaml) | Ship test on a chapter 11 deck (station module, seed 11): Boarding element against Freespace salvage techs wiping a data core, Borz contractors, and a Hardened door | [sheet](../print/ImperatorBlack_gym-03-wipe-race-sheet.pdf), [cards](../print/ImperatorBlack_gym-03-wipe-race-cards.pdf) |

To print a scenario together with every other unit card in one run, add `--with-unit-cards`: it writes `docs/print/ImperatorBlack_<id>-and-unit-cards.pdf`, the scenario's cards first, then the rest of the unit cards, four to a page with no troop printed twice. Identical enemy models share one card ("1, 2. Salvage Tech"). Current set: [Gym 3 and unit cards](../print/ImperatorBlack_gym-03-wipe-race-and-unit-cards.pdf) (57 cards, 15 pages).

Land scenarios set `tiles: []` and describe terrain as typed footprints (`hill`, `area`, `ditch`, `light`, `heavy`); the map draws a 6" grid instead of tiles.

Short arcs (three authored missions, chapter 14) are in [`arcs/`](arcs/README.md). They are the first bespoke campaigns for the campaign game (decision 7.15).
