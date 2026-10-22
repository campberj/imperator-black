// A board to draw: the scenario format shared by decks (GET /decks), land tables and missions
// (docs/scenarios/). Every part but the board size is optional. Distances are inches from the
// board's south-west corner; y grows north.
import type { DeckDoor, DeckTile } from './deck'

/** How a terrain piece is drawn (tools/print_scenario.py): light/heavy cover, raised ground, area terrain and so on. */
export type CoverType = 'light' | 'heavy' | 'hill' | 'area' | 'ditch' | 'water' | 'wire'

export interface BoardCover {
  id?: string
  x: number
  y: number
  w: number
  h: number
  type: CoverType
  label: string
  piece?: string
  /** Land: the chapter 12 terrain type id. */
  terrain?: string
  /** Land: the sector it stands in. Ship: the room tile is in `room`. */
  sector?: string
  room?: string
}

export interface Board {
  board: { width: number; height: number }
  tiles?: DeckTile[]
  doors?: DeckDoor[]
  /** The player's airlock aboard ship. */
  entry?: { x: number; y: number; orientation: 'h' | 'v'; label: string }
  cover?: BoardCover[]
  /** A deck's single objective (its heart). */
  objective?: { x: number; y: number; label: string }
  /** A mission's numbered objectives. */
  objectives?: { n: number; x: number; y: number; label: string }[]
  /** Land: the 12" sectors, named A1 (the player's bottom left) onward. */
  sectors?: { name: string; text: string }[]
  deploy?: { x: number; y: number; w: number; h: number; label: string }[]
  enemy_entry?: { x: number; y: number; label: string }
  contacts?: { n: number; x: number; y: number; where?: string }[]
  /** Guard models, numbered. */
  enemies?: { n: number; x: number; y: number; troop?: string; state?: string }[]
}
