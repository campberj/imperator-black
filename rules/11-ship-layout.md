# 11. Ship and station layouts

This chapter builds a deck to fight over from your tiles, with a handful of d6 rolls. The dice decide what is there; you decide only which way a piece faces when it could go either way. A deck takes 15 to 25 rolls and should be on the table in under ten minutes.

The app's layout generator follows exactly these steps and tables, so a deck from the app is one the paper procedure could have rolled.

## Tiles and the grid

A deck is built on a **3" grid** from three tile sizes:

- **Compartments:** 6"×6" rooms.
- **Passages:** 6"×3" corridors.
- **Junctions** and **small rooms:** 3"×3".

Tiles join edge to edge. Where two spine tiles meet, or a side passage meets its junction, the gap is an open archway; every other link between tiles is a **door** (chapter 3). The space around the tiles is outside the hull.

If a deck needs more of a tile than you own, or a room will not fit, draw it on the dry-erase map at the same scale. The same goes for big ships that need a second deck.

## The procedure

### 1. The vessel

The mission names the vessel. If it does not, roll:

<!-- table:layouts.vessel_roll -->
| d6 | Vessel |
|---|---|
| 1–2 | Freighter / ore hauler |
| 3 | Salvage tender |
| 4 | Corvette / patrol craft |
| 5–6 | Station module |
<!-- /table -->

<!-- table:layouts.vessels -->
| Vessel | Board | Spine | Compartments | Heart | Locks | Hazards (5 / 6) |
|---|---|---|---|---|---|---|
| **Freighter / ore hauler** | 36" × 18" | 3 pairs | 4–6 | Bridge (Secured) | Secured | Darkness / Zero-g |
| **Salvage tender** | 24" × 24" | 3 pairs | 3–5 | Bridge (Basic) | Basic | Fire / Darkness |
| **Corvette / patrol craft** | 30" × 18" | 2 pairs | 3–5 | Bridge (Military) | Secured | Hull breach risk / Darkness |
| **Station module** | 24" × 24" | 3 pairs | 3–5 | Operations centre (Hardened) | Secured | Darkness / Fire |
<!-- /table -->

- **Board:** the area the deck must fit in. Long boards suit the dry-erase map.
- **Spine:** how many junction-and-passage pairs follow the docking passage.
- **Compartments:** the fewest and most compartments on the deck, counting the heart.
- **Heart:** the room at the far end of the spine. Missions put their objective here unless they say otherwise. Its doors are always locked, at the security level shown.
- **Locks:** the security level of other locked doors (chapter 3), unless the room has its own.
- **Station modules** add 1 to junction branch rolls: stations are junction-heavy.

### 2. The airlock and the spine

1. Place the **docking passage** against the middle of the board's west edge, running inward. The airlock in that edge is the boarding element's way in and out.
2. Lay the **spine**: a junction, then a passage, repeated as many times as the vessel's spine says.
3. At every junction **after the first**, roll for the turn:

<!-- table:layouts.spine_turn -->
| d6 | Result |
|---|---|
| 1–4 | Straight on. |
| 5–6 | Turn left or right, toward the side with more open board (your choice if equal). |
<!-- /table -->

4. If the next tile will not fit (the board edge or another tile is in the way), turn toward the side with more open board. If it fits nowhere, the spine ends there.

### 3. The heart

Place the vessel's **heart** compartment against the far end of the spine, with a door. If it will not fit, put it beside the last spine tile that has room.

### 4. Branches

Walk the spine from the airlock outward (skip the docking passage) and roll once for each tile.

**Each passage:**

<!-- table:layouts.passage_branch -->
| d6 | Result |
|---|---|
| 1 | Plain walls. |
| 2–4 | A compartment on one side (the side with more open board; your choice if equal). |
| 5 | A compartment on each side. |
| 6 | A compartment on one side and a small room on the other. |
<!-- /table -->

**Each junction**, on its open sides (sides the spine does not use):

<!-- table:layouts.junction_branch -->
| d6 | Result |
|---|---|
| 1–3 | Plain walls. |
| 4–5 | A small room on one open side. |
| 6 | A side passage (one passage tile) off one open side, ending in a compartment. |
<!-- /table -->

- A compartment fits flush against a passage's long side, or against a junction side, offset either way.
- A room that will not fit is not placed.
- Once the deck has its most compartments, further compartment results are ignored; a side passage then ends in a small room instead.

### 5. Fill to the minimum

If the deck has fewer compartments than the vessel's minimum, add one beside each spine tile, working back from the far end, until it has enough. If it still falls short, draw the rest on the dry-erase map.

### 6. Rooms

Roll on the vessel's column for each compartment except the heart. A room marked † is unique: if it comes up again, take the next line down.

<!-- table:layouts.vessel_rooms -->
| d6 | Freighter / ore hauler | Salvage tender | Corvette / patrol craft | Station module |
|---|---|---|---|---|
| 1 | Cargo hold | Salvage bay | Armoury † | Laboratory |
| 2 | Cargo hold | Salvage bay | Crew quarters | Habitation |
| 3 | Cargo hold | Workshop | Sensors and comms † | Storage |
| 4 | Crew quarters | Crew quarters | Engineering † | Life support † |
| 5 | Mess | Mess | Mess | Medbay † |
| 6 | Engineering † | Engineering † | Medbay † | Data core † |
<!-- /table -->

Roll for each small room:

<!-- table:layouts.small_rooms -->
| d6 | Small room | Notes |
|---|---|---|
| 1 | Store | — |
| 2 | Locker room | — |
| 3 | Maintenance access | A crawlway hatch leads off the deck: Size 2 or less only. Missions may use it as a second way in or out. |
| 4 | Utility closet | — |
| 5 | Escape pod | Enemy models may flee by it: a model that ends its activation here leaves the board. |
| 6 | Secondary airlock | A second way in or out for either side; mission reinforcements may arrive here. |
<!-- /table -->

### 7. Second doors

For each compartment, check every other tile it touches that it has no door to (not small rooms). Roll for each:

<!-- table:layouts.loop_door -->
| d6 | Result |
|---|---|
| 1–3 | No second door. |
| 4–6 | A second door into the other tile. Second doors make loops: more than one way round. |
<!-- /table -->

### 8. Doors

Spine and side-passage joints are open archways. Every other door is rolled:

<!-- table:layouts.door_state -->
| d6 | Result |
|---|---|
| 1–2 | Open. |
| 3–5 | Closed. |
| 6 | Locked, at the room's security level. |
<!-- /table -->

Doors into the heart are always locked, at the heart's security level. Another locked door uses the security level of the room it leads into, if it has one, otherwise the vessel's.

### 9. Cover

Place each room's cover from the table below.

- Heavy pieces go in the middle of the room.
- Light pieces go in the corners farthest from the doors.
- Keep a clear lane in front of every door.

<!-- table:layouts.rooms -->
| Room | Tile | Cover | Notes |
|---|---|---|---|
| **Cargo hold** | 6"×6" | Container (heavy): A shipping container or crate stack taller than a model; Crates (light): Two or three crates, waist-high; Crates (light): Two or three crates, waist-high | — |
| **Salvage bay** | 6"×6" | Salvage (heavy): A large piece of wreckage or a dismantled engine; Scrap (light): A pile of scrap, plating or barrels; Scrap (light): A pile of scrap, plating or barrels | — |
| **Workshop** | 6"×6" | Machine (heavy): A heavy machine: lathe, press or fabricator; Bench (light): A workbench or tool rack | — |
| **Crew quarters** | 6"×6" | Bunks (light): Bunks or lockers; Bunks (light): Bunks or lockers | — |
| **Mess** | 6"×6" | Tables (light): Tables and benches; Counter (light): A galley counter | — |
| **Engineering** | 6"×6" | Reactor housing (heavy): A reactor, drive housing or large machinery block; Console (light): A control console | Unique |
| **Bridge** | 6"×6" | Console bank (light): A bank of consoles; Console bank (light): A bank of consoles | Unique |
| **Armoury** | 6"×6" | Weapon racks (light): Weapon racks or ammunition lockers; Weapon racks (light): Weapon racks or ammunition lockers | Locks: Hardened, Unique |
| **Sensors and comms** | 6"×6" | Consoles (light): Consoles and display screens; Server stack (heavy): A floor-to-ceiling equipment stack | Unique |
| **Medbay** | 6"×6" | Beds (light): Medical beds or a treatment station; Cabinets (light): Supply cabinets | Unique |
| **Laboratory** | 6"×6" | Equipment (heavy): A large piece of lab equipment; Benches (light): Lab benches | — |
| **Habitation** | 6"×6" | Furniture (light): Furniture, bunks or partitions; Lockers (light): Lockers | — |
| **Storage** | 6"×6" | Shelving (heavy): Tall shelving or a container; Crates (light): Crates, waist-high | — |
| **Life support** | 6"×6" | Scrubbers (heavy): Air scrubbers or tanks; Pipework (light): Low pipework or a pump | Unique |
| **Data core** | 6"×6" | Core (heavy): A floor-to-ceiling data core column; Consoles (light): Access consoles | Locks: Hardened, Unique |
| **Operations centre** | 6"×6" | Console bank (light): A bank of consoles; Plot table (light): A plotting table or holo-display | Locks: Hardened, Unique |
| **Store** | 3"×3" | Crates (light): A crate | — |
| **Locker room** | 3"×3" | None | — |
| **Maintenance access** | 3"×3" | None | — |
| **Escape pod** | 3"×3" | None | — |
| **Utility closet** | 3"×3" | Pipework (light): Pipes or a pump | — |
| **Secondary airlock** | 3"×3" | None | — |
<!-- /table -->

### 10. Hazard

Roll once for the deck:

<!-- table:layouts.hazard_roll -->
| d6 | Result |
|---|---|
| 1–4 | No hazard. |
| 5 | The vessel's first hazard. |
| 6 | The vessel's second hazard. |
<!-- /table -->

<!-- table:layouts.hazards -->
| Hazard | Effect |
|---|---|
| **Darkness** | Power failure: the whole deck is in darkness (chapter 8). |
| **Zero-g** | Gravity is off on the whole deck (chapter 8). |
| **Fire** | Place a fire marker in the middle of one compartment: the workshop, salvage bay or life support if there is one, otherwise the one nearest the heart (chapter 8). |
| **Hull breach risk** | Every outer wall is hull: a breaching charge or frag that damages it causes decompression (chapter 8). |
<!-- /table -->

## Missions on the deck

Phase 5 adds the mission tables: objectives, where the enemy starts, contacts and the alert track. Until then, use the heart as the objective, and place enemy models in the compartments, at least one compartment away from the docking passage.

## Example

A freighter ([deck sheet](../docs/print/decks/deck-freighter-42.pdf)). The board is 36" × 18", the spine is three pairs, and it needs four to six compartments.

1. **Spine.** The docking passage (P1) goes against the middle of the west edge, then junction J1 and passage P2. At J2 the turn roll is 3, straight on: passage P3. At J3 it is 1, straight: passage P4, which stops 3" short of the east edge.
2. **Heart.** There is no room for the bridge beyond P4, so it goes beside P4. Both sides fit; you choose the south (C1).
3. **Branches.**
   - J1 rolls 1: plain walls.
   - P2 rolls 5: a compartment on each side (C2 south, C3 north).
   - J2 rolls 6: a side passage. It goes north (P5), but at its end there is only 3" of board left, so it ends in a small room (S1).
   - P3 rolls 4: a compartment on one side. Both sides fit; you choose the south (C4).
   - J3 rolls 1: plain walls.
   - P4 rolls 4: a compartment on one side. The bridge is on the south side, so it goes north (C5).

   That is five compartments, so no filling is needed.
4. **Rooms.** C2 rolls 3, a cargo hold; C3 rolls 1, a cargo hold; S1 rolls 1, a store; C4 rolls 4, crew quarters; C5 rolls 1, a cargo hold.
5. **Second doors.** C3 touches the side passage P5 with no door between them: the roll is 3, so no second door.
6. **Doors.** The bridge door is locked (Secured). P2's two doors roll 3 and 5, both closed; the store door rolls 3, closed; C4's door rolls 1, open; C5's door rolls 6, locked (Secured). The side passage joins J2 by an open archway.
7. **Cover and hazard.** Each hold gets a container and two crate stacks; the quarters two sets of bunks; the bridge two console banks. The hazard roll is 4: none.

Twenty rolls in all.
