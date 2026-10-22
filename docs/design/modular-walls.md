# Modular walls: building rooms on a plain mat

Status: **notes only, waiting on prints** (RJ, 2026-10-22). RJ is printing a set of modular wall sections sculpted by Sable Badger 3D. They give a second way to build boards: walls stood on a plain mat instead of floor tiles. This applies to both ship decks (chapter 11) and land buildings (chapter 12). The rules and generator changes wait until a set is printed and the sizes are confirmed on the table.

## The kit (RJ's measurements)

| Piece | Size |
|---|---|
| Wall sections | 50, 75, 100 and 125 mm long; about 10 mm thick |
| Door sections | Same lengths as the walls. Single door about 50 mm wide; double door 85–100 mm |
| Junctions | About 20 mm, one between every two sections (and at corners) |

## How the sizes fit our 3" grid

Junctions add 20 mm per joint, so a wall run is longer than its sections. Measured centre to centre of the junctions, one section plus one junction gives:

| Section | Section + junction | In inches | Grid fit |
|---|---|---|---|
| 50 mm | 70 mm | 2.8" | One 3" cell |
| 75 mm | 95 mm | 3.7" | One 3" cell, a little long; or a 4" side |
| 100 mm | 120 mm | 4.7" | Between cells; best for land buildings |
| 125 mm | 145 mm | 5.7" | One 6" side (two cells) |
| 125 + 75 mm | 240 mm | 9.4" | One 9" side (three cells) |
| 125 + 125 mm | 290 mm | 11.4" | One 12" side |

So the chapter 11 room sizes map to sections with about 5% error, which the table won't notice:

- **6" × 6" compartment:** one 125 mm section per side (four junctions at the corners). Inside, wall face to wall face, it measures about 5.3".
- **6" × 3" passage:** 125 mm sides and 50 mm ends.
- **3" × 3" junction or small room:** 50 mm sections.
- **Doors:** a 125 mm side with a door uses a 125 mm door section. A single door (about 50 mm, 2") matches the door width the deck sheets draw.

On a plain mat there are no tiles to run out of, so the "within 12 of each tile" limit in chapter 11 would become a limit on wall sections and junctions instead. A deck sheet would then need a **cut list** saying how many of each section, door section and junction to set out.

## Plan, once the walls are printed

1. RJ measures a built 6" room and a passage on the mat, to confirm the table above.
2. **Ship decks:** chapter 11 gains a "walls on a mat" option. The procedure and dice stay the same; only the build changes. Each tile becomes a walled room, and touching rooms share a wall. `tools/generate_layout.py` and the C# deck generator add the cut list, and the deck sheet draws wall runs instead of tile outlines.
3. **Land buildings:** the chapter 12 building results (small building 6" × 6", large 6" × 9", warehouse, bunker, outpost) gain a wall recipe from the same sections, with door sections where the generator puts the doors.
4. **Count the inventory:** RJ's printed counts go into `content/layouts.yaml`, the way the tile inventory is now, so the generator never asks for more sections than exist.

## Open questions

- How many sections, door sections and junctions of each size will be printed?
- Do the junctions also make T and cross joints, so one wall can be shared between two rooms?
- Are there windows, or half-height (Light cover) walls? Those would map to the land rules for buildings and low walls.
- Single storey only, or can the sections stack for a second level?
