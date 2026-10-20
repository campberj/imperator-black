// Types for the parts of content/dist/content.json the web app reads, and the API call that fetches it.
// Field names match the JSON (snake_case). The full schema is in content/README.md and Imperator.Core/Content.cs.

export interface RangedWeapon {
  id: string
  name: string
  damage: string
  short_range: number
  ap: number
  keywords: string[]
  range_penalty?: [number, number]
  /** Rate of fire for Auto weapons: the N in Auto N. */
  auto?: number
}

export interface MeleeWeapon {
  id: string
  name: string
  damage: string
  ap: number
}

export interface ThrownWeapon {
  id: string
  name: string
  throw_range: number
  blast_radius: number
  damage?: string
  ap?: number
  effect?: string
  keywords: string[]
}

export interface Armour {
  id: string
  name: string
  value: number
  keywords: string[]
}

export interface Keyword {
  id: string
  name: string
  text: string
}

export interface Content {
  core: { version: string }
  weapons: { ranged: RangedWeapon[]; melee: MeleeWeapon[]; thrown: ThrownWeapon[] }
  armour: { armour: Armour[] }
  keywords: { weapon: Keyword[]; armour: Keyword[] }
}

export interface LoadedContent {
  content: Content
  /** From the Imperator-Content-Version response header. */
  version: string
}

export async function fetchContent(signal?: AbortSignal): Promise<LoadedContent> {
  const response = await fetch('/content', { signal })
  if (!response.ok) throw new Error(`GET /content failed: ${response.status} ${response.statusText}`)
  const content = (await response.json()) as Content
  return { content, version: response.headers.get('Imperator-Content-Version') ?? content.core.version }
}

// Same default as tools/render_tables.py and RangedWeapon in Imperator.Core; content lists only exceptions.
const DEFAULT_RANGE_PENALTY: [number, number] = [-1, -2]

export const rangePenalty = (w: RangedWeapon) => w.range_penalty ?? DEFAULT_RANGE_PENALTY
