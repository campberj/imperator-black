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

export interface Rank {
  id: string
  name: string
  typical: string
  text: string
}

export interface Attribute {
  id: string
  name: string
  short: string
  text: string
}

export interface Gear {
  id: string
  name: string
  text: string
  status: 'settled' | 'proposed'
  grants_skills?: Record<string, number>
  weapon_keywords?: string[]
  melee?: string
}

export interface Skill {
  id: string
  name: string
  status: 'settled' | 'proposed'
  /** levels[0] is level 1; a higher level includes everything below it. */
  levels: string[]
}

export interface Troop {
  id: string
  faction: string
  name: string
  role: string
  rank: string
  /** Attribute id to value; null means never used (Automated models only). */
  stats: Record<string, number | null>
  armour: string
  armour_options?: string[]
  weapon: string | null
  weapon_options?: string[]
  melee: string | null
  grenades: string[]
  gear: string[]
  gear_options?: string[]
  skills: Record<string, number>
  miniatures?: string
}

export interface Faction {
  id: string
  name: string
  side: 'player' | 'enemy'
  summary: string
}

export interface Element {
  id: string
  name: string
  faction: string
  tier: number
  behavior: string
  composition: { troop: string; count: number }[]
  alternatives?: string[]
  reaction_force?: boolean
}

export interface ThreatTier {
  tier: number
  name: string
  summary: string
}

export interface Team {
  id: string
  name: string
  faction: string
  summary: string
  slots: { troop: string; count: number; gear_choice?: number }[]
}

export interface Behavior {
  id: string
  name: string
  summary: string
  status: 'settled' | 'proposed'
  surrenders: boolean
  close_quarters: boolean
  investigate: string
  engaged: string[]
  alerted: string[]
  reactions: string[]
  close_combat: string
}

export interface Behaviors {
  awareness: { id: string; name: string; text: string }[]
  marker: string[]
  alarm: string[]
  noise: { id: string; name: string; sources: string; heard: string; alert: number }[]
  drills: { id: string; rank: string; name: string; text: string }[]
  activation_order: string[]
  targets: string[]
  moving: string[]
  close_quarters: string[]
  surrender: string[]
  tie_breakers: string[]
  terms?: { term: string; text: string }[]
  behaviors: Behavior[]
}

export interface Vessel {
  id: string
  name: string
  board: [number, number]
  text: string
}

export interface Room {
  id: string
  name: string
  size: 'compartment' | 'small'
  text?: string
  security?: string
}

export interface SecurityLevel {
  id: string
  name: string
  target: number
  successes: number
  examples: string
}

export interface Content {
  core: { version: string; ranks: Rank[]; attributes: Attribute[]; security: { levels: SecurityLevel[] } }
  weapons: { ranged: RangedWeapon[]; melee: MeleeWeapon[]; thrown: ThrownWeapon[] }
  armour: { armour: Armour[] }
  keywords: { weapon: Keyword[]; armour: Keyword[] }
  gear: { gear: Gear[] }
  skills: { skills: Skill[] }
  troops: { troops: Troop[] }
  factions: { factions: Faction[]; elements: Element[]; threat_tiers: ThreatTier[] }
  rosters: { teams: Team[] }
  behaviors: Behaviors
  layouts: { vessels: Vessel[]; rooms: Room[] }
  terrain: {
    types: { id: string; name: string; kind: string }[]
    environments: { id: string; name: string; fill: string }[]
    conditions: { id: string; name: string; text: string }[]
  }
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

/** Content sections keyed by id. The API validates every reference at startup, so lookups by a content id succeed. */
export interface Lookup {
  ranged: Map<string, RangedWeapon>
  melee: Map<string, MeleeWeapon>
  thrown: Map<string, ThrownWeapon>
  armour: Map<string, Armour>
  gear: Map<string, Gear>
  skills: Map<string, Skill>
  /** Weapon and armour keywords together, as on the printed cards. */
  keywords: Map<string, Keyword>
  ranks: Map<string, Rank>
  troops: Map<string, Troop>
  factions: Map<string, Faction>
  behaviors: Map<string, Behavior>
  threatTiers: Map<number, ThreatTier>
}

const byId = <T extends { id: string }>(items: T[]) => new Map(items.map((x) => [x.id, x]))

export function indexContent(c: Content): Lookup {
  return {
    ranged: byId(c.weapons.ranged),
    melee: byId(c.weapons.melee),
    thrown: byId(c.weapons.thrown),
    armour: byId(c.armour.armour),
    gear: byId(c.gear.gear),
    skills: byId(c.skills.skills),
    keywords: byId([...c.keywords.weapon, ...c.keywords.armour]),
    ranks: byId(c.core.ranks),
    troops: byId(c.troops.troops),
    factions: byId(c.factions.factions),
    behaviors: byId(c.behaviors.behaviors),
    threatTiers: new Map(c.factions.threat_tiers.map((t) => [t.tier, t])),
  }
}

// Same default as tools/render_tables.py and RangedWeapon in Imperator.Core; content lists only exceptions.
const DEFAULT_RANGE_PENALTY: [number, number] = [-1, -2]

export const rangePenalty = (w: RangedWeapon) => w.range_penalty ?? DEFAULT_RANGE_PENALTY
