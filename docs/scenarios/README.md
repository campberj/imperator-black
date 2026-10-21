# Scenarios

Fixed scenarios for playtesting. Each is a YAML file holding the layout, forces with their loadouts, special rules and victory conditions. Cover pieces carry a map id and a suggested terrain piece, printed in the sheet's Terrain table, and `walls_and_doors` says what to build walls and doors from. Each enemy model names its element, which sets its behavior profile (rules chapter 10). The mission generator (Phase 5) will grow out of this format.

Print a scenario's sheet and cards with:

```sh
python3 tools/print_scenario.py docs/scenarios/<scenario>.yaml
```

This writes `docs/print/<id>-sheet.pdf` (scenario sheet with a to-scale map) and `docs/print/<id>-cards.pdf` (one card per model, with its chosen loadout, plus the solo opponent reference cards and a behavior card for each enemy profile in play). Profiles come from `content/`, so reprinting picks up any rules changes.

| Scenario | Purpose | Print |
|---|---|---|
| [Gym 1: First Breach](gym-01-first-breach.yaml) | Phase 2 exit playtest: Boarding element vs four Freespace asset security on five tiles; doors, a locked door, Overwatch, close quarters, an extraction | [sheet](../print/gym-01-first-breach-sheet.pdf), [cards](../print/gym-01-first-breach-cards.pdf) |
