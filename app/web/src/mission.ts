// A mission in the scenario format: from GET /missions (Imperator.Core/Missions) or one of the example missions in
// docs/scenarios/missions/ (made by tools/generate_mission.py). Both have the same shape.
import { useEffect, useState } from 'react'
import type { Board, BoardCover } from './board'
import type { Content } from './content'
import { seedFrom, useGenerated } from './generated'

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

/**
 * The mission the hash asks for (#missions?… and #dashboard?…): an example, or the pickers and seed for GET /missions.
 * Unknown picker values fall back to rolled.
 */
export function missionRequest(params: URLSearchParams, content: Content) {
  const example = EXAMPLES.find((e) => e.id === params.get('example'))?.id
  const setting = params.get('setting') === 'land' ? 'land' : 'ship'
  const pick = (key: string, ids: string[]) => (ids.includes(params.get(key) ?? '') ? params.get(key)! : '')
  const mission = pick('mission', content.missions.missions.map((m) => m.id))
  const tier = pick('tier', content.missions.tiers.map((t) => String(t.tier)))
  const vessel = setting === 'ship' ? pick('vessel', content.layouts.vessels.map((v) => v.id)) : ''
  const environment = setting === 'land' ? pick('environment', content.terrain.environments.map((e) => e.id)) : ''
  const size = setting === 'land' && params.get('size') === 'large' ? 'large' : 'small'
  const seed = seedFrom(params)
  const asked = { setting, mission, tier, vessel, environment, size: setting === 'land' ? size : '' }
  const query = new URLSearchParams(Object.entries({ ...asked, seed: String(seed) }).filter(([, v]) => v)).toString()
  return { example, asked, seed, url: `/missions?${query}` }
}

/** The example mission, or the generated one from url; nothing when both are undefined. */
export function useMission(example: string | undefined, url: string | undefined): { value?: Mission; error?: string } {
  const generated = useGenerated<Mission>(example ? undefined : url)
  const fromExample = useExample(example)
  return example ? fromExample : generated
}

function useExample(id: string | undefined): { value?: Mission; error?: string } {
  const [result, setResult] = useState<{ id: string; value?: Mission; error?: string }>()
  useEffect(() => {
    if (!id) return
    let live = true
    loadExample(id)
      .then((value) => live && setResult({ id, value }))
      .catch((e: unknown) => live && setResult({ id, error: e instanceof Error ? e.message : String(e) }))
    return () => {
      live = false
    }
  }, [id])
  return result && result.id === id ? { value: result.value, error: result.error } : {}
}
