// Formatting shared by the views. Wording follows the printed cards (tools/print_sheets.py)
// and the generated rulebook tables (tools/render_tables.py), so the app adds no rules text of its own.
import type { Content, Element, Lookup, RangedWeapon, Troop } from './content'

/** A keyword's name; an Auto weapon shows its rate ("Auto 3"). Unknown ids fall back to the id. */
export function keywordLabel(id: string, lookup: Lookup, weapon?: RangedWeapon): string {
  if (id === 'auto' && weapon?.auto) return `Auto ${weapon.auto}`
  return lookup.keywords.get(id)?.name ?? id
}

/** Round choices use the same wording as the printed unit cards. */
export function launcherRounds(w: RangedWeapon): string[] {
  const first = w.round ? [`${w.round} above`] : []
  const extras = (w.rounds ?? []).map((r) => `${r.name}: ${r.damage}, AP ${r.ap}${r.explosion ? `, Explosion (${r.explosion})` : ''}`)
  return [...first, ...extras.map((item) => `or ${item.charAt(0).toLowerCase()}${item.slice(1)} round`)]
}

/** A weapon's own range penalty, noted on cards only when it differs from the default. */
export function rangePenaltyNote(w: RangedWeapon): string | undefined {
  return w.range_penalty && `beyond short ${w.range_penalty[0]}, beyond double ${w.range_penalty[1]}`
}

/** An attribute value as printed: "—" for never used, Move in inches. */
export function statText(troop: Troop, attr: string): string {
  const v = troop.stats[attr]
  if (v === null || v === undefined) return '—'
  if (attr === 'move') return v ? `${v}"` : '—'
  return String(v)
}

/** A skill's display name: the level shows only for skills with more than one ("Marksman 2"). */
export function skillName(lookup: Lookup, id: string, level: number): string {
  const skill = lookup.skills.get(id)
  if (!skill) return id
  return skill.levels.length > 1 ? `${skill.name} ${level}` : skill.name
}

/** Rules text for a skill at a level: every level up to it. */
export function skillText(lookup: Lookup, id: string, level: number): string {
  return lookup.skills.get(id)?.levels.slice(0, level).join(' ') ?? ''
}

/** The ranged weapons a troop can carry: its own weapon first, then the options. */
export function troopWeapons(troop: Troop, lookup: Lookup): RangedWeapon[] {
  return [troop.weapon, ...(troop.weapon_options ?? [])]
    .filter((id): id is string => !!id)
    .map((id) => lookup.ranged.get(id))
    .filter((w): w is RangedWeapon => !!w)
}

/** Weapon keywords the troop's gear adds to its first weapon (e.g. a silencer adds Silenced). */
export function gearWeaponKeywords(troop: Troop, lookup: Lookup): string[] {
  return troop.gear.flatMap((g) => lookup.gear.get(g)?.weapon_keywords ?? [])
}

/** Every weapon and armour keyword a troop has, for the keyword filter. */
export function troopKeywords(troop: Troop, lookup: Lookup): Set<string> {
  const ids = [
    ...troopWeapons(troop, lookup).flatMap((w) => w.keywords),
    ...gearWeaponKeywords(troop, lookup),
    ...[troop.armour, ...(troop.armour_options ?? [])].flatMap((a) => lookup.armour.get(a)?.keywords ?? []),
  ]
  return new Set(ids)
}

/** All the names and rules text on a troop's card, for the search box. */
export function troopSearchText(troop: Troop, lookup: Lookup): string[] {
  const gear = [...troop.gear, ...(troop.gear_options ?? [])].map((g) => lookup.gear.get(g))
  return [
    troop.name,
    troop.role,
    troop.miniatures ?? '',
    ...troopWeapons(troop, lookup).map((w) => w.name),
    troop.melee ? (lookup.melee.get(troop.melee)?.name ?? '') : '',
    ...troop.grenades.map((g) => lookup.thrown.get(g)?.name ?? ''),
    ...[troop.armour, ...(troop.armour_options ?? [])].map((a) => lookup.armour.get(a)?.name ?? ''),
    ...gear.flatMap((g) => (g ? [g.name, g.text] : [])),
    ...Object.entries(troop.skills).flatMap(([id, lvl]) => [skillName(lookup, id, lvl), skillText(lookup, id, lvl)]),
  ]
}

/** Case-insensitive match of a search query against any of the texts; an empty query matches everything. */
export function matches(query: string, ...texts: string[]): boolean {
  const q = query.trim().toLowerCase()
  return !q || texts.some((t) => t.toLowerCase().includes(q))
}

/** The enemy elements a troop appears in, as a model or an alternative. */
export function elementsFor(content: Content, troopId: string): Element[] {
  return content.factions.elements.filter(
    (e) => e.composition.some((p) => p.troop === troopId) || (e.alternatives ?? []).includes(troopId),
  )
}

/** "3× Salvage hand, 1× Security gunner", as in the rulebook's team and element tables. */
export function modelsText(parts: { troop: string; count: number }[], lookup: Lookup): string {
  return parts.map((p) => `${p.count}× ${lookup.troops.get(p.troop)?.name ?? p.troop}`).join(', ')
}

// Faction band colours, as on the printed cards (tools/print_sheets.py). Presentation only.
const FACTION_COLOURS: Record<string, string> = {
  tfb: '#27466b',
  freespace: '#8a5a12',
  contractor: '#4d5560',
  directorate: '#7a1f2b',
  republic: '#a34a1a',
  un: '#2f5d62',
}

export const factionColour = (id: string) => FACTION_COLOURS[id] ?? '#3a3f46'
