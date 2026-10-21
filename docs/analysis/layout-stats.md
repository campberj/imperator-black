# Deck layout statistics

`python3 tools/generate_layout.py --check --n 2000` (2,000 decks per vessel, seeds 1–2000), content v0.5.0, 2026-10-21.

| Vessel | Compartments, mean (range) | Below minimum | With a loop | d6 rolls | Most tiles used (6x6 / 6x3 / 3x3) | Disconnected |
|---|---|---|---|---|---|---|
| Freighter / ore hauler | 4.1 (4–6) | 0% | 65% | 21 | 6 / 7 / 8 | 0 |
| Salvage tender | 3.3 (3–5) | 0% | 47% | 16 | 5 / 7 / 7 | 0 |
| Corvette / patrol craft | 3.2 (3–5) | 0% | 60% | 14 | 5 / 5 / 6 | 0 |
| Station module | 3.3 (3–5) | 0% | 58% | 17 | 5 / 7 / 8 | 0 |

- **Below minimum** counts decks that could not reach the vessel's minimum compartments on the tiles (they would use the dry-erase map). **With a loop** counts decks with more than one route between some rooms. **Most tiles used** is the largest count of each tile in any deck; you own 12 of each.
- Compartment counts sit near each vessel's minimum: rooms come mostly from the spine's passages, and the boards are tight. About a quarter to a half of decks use the fill-to-minimum step.
