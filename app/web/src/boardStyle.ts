// Colours and names of the printed sheets (tools/print_scenario.py), shared by the map and its legend. Presentation only.
import type { CoverType } from './board'

export const DOOR_COLOURS = { open: '#7fb27f', closed: '#8a5a12', locked: '#b4441f' } as const

export const COVER_COLOURS: Record<CoverType, string> = {
  light: '#c8d9b8',
  heavy: '#7d8a6b',
  hill: '#e3cfa4',
  area: '#d9e3c4',
  ditch: '#b98c6a',
  water: '#a9c8e0',
  wire: '#9a9a9a',
}

/** Drawing order: raised ground first, solid cover last. */
export const COVER_ORDER: Record<CoverType, number> = { hill: 0, area: 1, water: 1, ditch: 2, light: 3, wire: 3, heavy: 4 }

/** Legend names for land terrain; aboard ship light and heavy are "Light cover" and "Heavy cover". */
export const COVER_NAMES: Record<CoverType, string> = {
  light: 'Low wall or hedge',
  heavy: 'Solid or building',
  hill: 'Raised ground',
  area: 'Area terrain',
  ditch: 'Crater or trench',
  water: 'Water',
  wire: 'Wire',
}

export const BOARD_COLOURS = {
  player: '#27466b',
  enemy: '#b4441f',
  objective: '#d9a300',
  contact: '#f2f2f2',
  deploy: 'rgb(38 70 107 / 0.18)',
} as const
