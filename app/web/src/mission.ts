// A mission in the scenario format: from GET /missions (Imperator.Core/Missions) or one of the example missions in
// docs/scenarios/missions/ (made by tools/generate_mission.py). Both have the same shape.
import type { Board, BoardCover } from './board'

export interface Mission extends Board {
  id: string
  title: string
  kind: 'mission'
  rules_version: string
  /** Only on generated missions. */
  content_version?: string
  setting: 'ship' | 'land'
  seed: number
  /** The mission type id. */
  mission: string
  tier: number
  size: string
  clock: { die: string; limit: number; check_in_every: number; first_check_in: number }
  alert_start: number
  complication: string
  situation: string
  briefing: { heading: string; text: string }[]
  objectives: { n: number; x: number; y: number; tile: string; label: string; prose: string }[]
  enemy_entry: { x: number; y: number; tile: string; label: string }
  enemies: { n: number; troop: string; element: string; x: number; y: number; state: string }[]
  contacts: { n: number; x: number; y: number; where: string }[]
  reinforcement: string
  hvt: string | null
  rolls: string[]
  cover: (BoardCover & { id: string })[]
  // Ship
  vessel?: string
  hazard?: string
  deck_notes?: string[]
  // Land
  environment?: string
  conditions?: string
}

// The example missions, read from the YAML files themselves so docs/scenarios/missions stays the one source.
const RAW = import.meta.glob('../../../docs/scenarios/missions/*.yaml', { query: '?raw', import: 'default' }) as Record<string, () => Promise<string>>

export const EXAMPLES = Object.keys(RAW)
  .map((path) => ({ id: path.slice(path.lastIndexOf('/') + 1, -'.yaml'.length), path }))
  .sort((a, b) => a.id.localeCompare(b.id))

/** "mission-ship-42" → "Example: ship, seed 42". */
export function exampleLabel(id: string): string {
  const m = /^mission-(\w+)-(\d+)$/.exec(id)
  return m ? `Example: ${m[1]}, seed ${m[2]}` : `Example: ${id}`
}

export async function loadExample(id: string): Promise<Mission> {
  const example = EXAMPLES.find((e) => e.id === id)
  if (!example) throw new Error(`No example mission '${id}'`)
  const [{ parse }, text] = await Promise.all([import('yaml'), RAW[example.path]()])
  return parse(text) as Mission
}
