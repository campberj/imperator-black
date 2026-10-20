# Imperator Black — Rules v0.2

Playtest draft. This version continues [prototype rules v0.1](../docs/design/a-war-of-worlds/prototype-rules-v0.1.md) from A War of Worlds. It adds reactions, close combat, grenades and breaching, ship interiors, and ship hazards.

Tables between `<!-- table:… -->` markers are generated from the YAML files in [`content/`](../content). Edit the YAML, not the table, then run `python3 tools/render_tables.py`.

## Chapters

1. [Core concepts](01-core-concepts.md)
2. [The round, actions and reactions](02-round-actions-reactions.md)
3. [Movement and ship interiors](03-movement-ship-interiors.md)
4. [Shooting](04-shooting.md)
5. [Close combat](05-close-combat.md)
6. [Grenades and demolitions](06-grenades-demolitions.md)
7. [Suppression, morale and casualties](07-suppression-morale-casualties.md)
8. [Ship hazards](08-ship-hazards.md)

Not yet written: solo opponent and behavior profiles (Phase 3), ship layout procedure (Phase 4), missions, contacts and the alert track (Phase 5), campaign (Phase 7). Until then, use the solo rules and test scenario in prototype v0.1.

## Changes from prototype v0.1

- **Reactions:** unactivated models may react to enemy actions; Overwatch now allows up to two reactions until the model next activates.
- **Close combat:** opposed 2d6 + Q exchanges that continue until one side is incapacitated or both break away.
- **Grenades and demolitions:** frag, flashbang, smoke and breaching charges.
- **Ship interiors:** doors, locked doors, bulkheads and compartments.
- **Ship hazards:** zero-g, decompression, vacuum, fire and darkness.
- **Down recovery:** an end-of-round test fixes the row-6 gap.
- **Errata:** the Armour 3 note is corrected.
