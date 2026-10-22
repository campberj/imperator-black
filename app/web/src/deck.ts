// A generated deck from GET /decks/{vessel}?seed=N (Imperator.Core/Layouts), in the scenario format of
// docs/scenarios/decks/*.yaml. Distances are inches from the board's south-west corner; y grows north.

export interface DeckTile {
  id: string
  name: string
  size: string
  x: number
  y: number
  w: number
  h: number
  kind: 'passage' | 'junction' | 'compartment' | 'small'
  room?: string
}

export interface DeckDoor {
  id: string
  x: number
  y: number
  /** h lies along an east-west wall, v along a north-south wall. */
  orientation: 'h' | 'v'
  state: 'open' | 'closed' | 'locked'
  security?: string
  /** "C1 to P2" */
  note: string
}

export interface DeckCover {
  id: string
  x: number
  y: number
  w: number
  h: number
  type: 'light' | 'heavy'
  label: string
  piece: string
  room: string
}

export interface Deck {
  id: string
  title: string
  rules_version: string
  content_version: string
  kind: 'deck'
  vessel: string
  seed: number
  situation: string
  board: { width: number; height: number }
  tiles: DeckTile[]
  doors: DeckDoor[]
  entry: { x: number; y: number; orientation: 'h' | 'v'; label: string }
  cover: DeckCover[]
  objective?: { x: number; y: number; label: string }
  hazard: string
  rolls: number
  notes: string[]
  tile_counts: Record<string, number>
}

export async function fetchDeck(vessel: string, seed: number, signal?: AbortSignal): Promise<Deck> {
  const response = await fetch(`/decks/${encodeURIComponent(vessel)}?seed=${seed}`, { signal })
  if (!response.ok) {
    const problem = await response.json().catch(() => undefined)
    throw new Error(problem?.detail ?? `GET /decks/${vessel} failed: ${response.status} ${response.statusText}`)
  }
  return (await response.json()) as Deck
}
