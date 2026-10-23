import { useEffect, useRef, useState } from 'react'
import type { Content, Lookup } from '../content'
import { UnitCard } from '../components/UnitCard'
import { Table } from '../components/Table'

type FireteamReference = {
  pinning: { troop: string; text: string | null }[]
  fireteams: { id: string; name: string; faction: string; type: string; source: string; models: { troop: string; count: number }[]; findings: string[]; tier?: number; behavior?: string }[]
}
import { matches, modelsText, troopKeywords, troopSearchText } from '../format'

/** Troop profiles as cards, with the faction's teams (player) or elements (enemy). */
export function Units({ content, lookup }: { content: Content; lookup: Lookup }) {
  const [values, setValues] = useState<Map<string, { tv: number; requisition: number }>>(new Map())
  const [valueError, setValueError] = useState('')
  const [fireteamReference, setFireteamReference] = useState<FireteamReference>()
  const [fireteamError, setFireteamError] = useState('')
  useEffect(() => {
    const controller = new AbortController()
    fetch('/fireteams', { signal: controller.signal })
      .then(async (response) => {
        if (!response.ok) throw new Error(`GET /fireteams: ${response.status}`)
        const data = (await response.json()) as FireteamReference
        if (!controller.signal.aborted) setFireteamReference(data)
      })
      .catch((error: unknown) => { if (!controller.signal.aborted) setFireteamError(String(error)) })
    return () => controller.abort()
  }, [])
  useEffect(() => {
    const controller = new AbortController()
    fetch('/values', { signal: controller.signal })
      .then(async (response) => {
        if (!response.ok) throw new Error(`GET /values: ${response.status}`)
        const rows = (await response.json()) as { troop: string; tv: number; requisition: number }[]
        if (!controller.signal.aborted) setValues(new Map(rows.map((row) => [row.troop, row])))
      })
      .catch((error: unknown) => { if (!controller.signal.aborted) setValueError(String(error)) })
    return () => controller.abort()
  }, [])
  const searchCache = useRef(new Map<string, string[]>())
  const keywordCache = useRef(new Map<string, Set<string>>())
  const factions = content.factions.factions
  const [faction, setFaction] = useState(factions[0]?.id ?? '')
  const [rank, setRank] = useState('')
  const [role, setRole] = useState('')
  const [keyword, setKeyword] = useState('')
  const [query, setQuery] = useState('')

  const inFaction = content.troops.troops.filter((t) => !faction || t.faction === faction)
  const roles = [...new Set(inFaction.map((t) => t.role))].sort()
  const troops = inFaction.filter(
    (t) =>
      (!rank || t.rank === rank) &&
      (!role || t.role === role) &&
      (!keyword || (keywordCache.current.get(t.id) ?? (() => { const v = troopKeywords(t, lookup); keywordCache.current.set(t.id, v); return v })()).has(keyword)) &&
      (!query.trim() || matches(query, ...(searchCache.current.get(t.id) ?? (() => { const v = troopSearchText(t, lookup); searchCache.current.set(t.id, v); return v })()))),
  )
  const shown = (id: string) => !faction || id === faction
  const teams = content.rosters.teams.filter((t) => shown(t.faction))
  const elements = content.factions.elements.filter((e) => shown(e.faction))
  const keywords = [...content.keywords.weapon, ...content.keywords.armour]
  const pins = new Map(fireteamReference?.pinning.map((p) => [p.troop, p.text]) ?? [])

  return (
    <>
      <div className="filters" role="search">
        <label>
          Faction
          <select
            value={faction}
            onChange={(e) => {
              setFaction(e.target.value)
              setRole('')
            }}
          >
            <option value="">All factions</option>
            {factions.map((f) => (
              <option key={f.id} value={f.id}>
                {f.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Rank
          <select value={rank} onChange={(e) => setRank(e.target.value)}>
            <option value="">Any rank</option>
            {content.core.ranks.map((r) => (
              <option key={r.id} value={r.id}>
                {r.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Role
          <select value={role} onChange={(e) => setRole(e.target.value)}>
            <option value="">Any role</option>
            {roles.map((r) => (
              <option key={r} value={r}>
                {r}
              </option>
            ))}
          </select>
        </label>
        <label>
          Keyword
          <select value={keyword} onChange={(e) => setKeyword(e.target.value)}>
            <option value="">Any keyword</option>
            {keywords.map((k) => (
              <option key={k.id} value={k.id}>
                {k.name}
              </option>
            ))}
          </select>
        </label>
        <label className="grow">
          Search
          <input type="search" value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Names and rules text" />
        </label>
      </div>

      {faction && <p className="summary">{lookup.factions.get(faction)?.summary}</p>}
      <p className="meta" aria-live="polite">
        {troops.length} of {inFaction.length} troop profiles
      </p>
      {valueError && <p className="error" role="alert">Troop Values unavailable: {valueError}</p>}
      {fireteamError && <p className="error" role="alert">Fireteams unavailable: {fireteamError}</p>}
      <div className="cards">
        {troops.map((t) => (
          <UnitCard key={t.id} troop={t} content={content} lookup={lookup} value={values.get(t.id)} pinning={pins.get(t.id)} />
        ))}
      </div>

      {(content.rosters.classes?.length ?? 0) > 0 && <Table title="Troop classes" headers={['Class', 'Description']} rows={content.rosters.classes!.map((c) => [c.name, c.text])} />}
      {(content.rosters.fireteam_types?.length ?? 0) > 0 && <Table title="Fireteam types" headers={['Type', 'Leaders', 'Troopers', 'Specialists', 'Most models']} rows={content.rosters.fireteam_types!.map((t) => [t.name, t.leader.join('–'), t.troopers.join('–'), t.specialists.join('–'), t.max_models])} />}
      {fireteamReference && <Table title="Standard fireteams" headers={['Fireteam', 'Type', 'Models', 'Tier', 'Behavior']} rows={fireteamReference.fireteams.filter((t) => shown(t.faction)).map((t) => [t.name, content.rosters.fireteam_types?.find((f) => f.id === t.type)?.name ?? t.type, modelsText(t.models, lookup), t.tier ?? '—', t.behavior ? lookup.behaviors.get(t.behavior)?.name ?? t.behavior : '—'])} />}
      {teams.length > 0 && (
        <Table
          title="Teams"
          headers={['Team', 'Models', 'Role']}
          rows={teams.map((t) => [t.name, modelsText(t.slots, lookup), t.summary])}
        />
      )}
      {elements.length > 0 && (
        <Table
          title="Elements"
          headers={['Element', 'Faction', 'Tier', 'Models', 'Behavior']}
          rows={elements.map((e) => [
            e.name,
            lookup.factions.get(e.faction)?.name ?? e.faction,
            <abbr key="tier" title={lookup.threatTiers.get(e.tier)?.name}>
              {e.tier}
            </abbr>,
            modelsText(e.composition, lookup) +
              (e.alternatives?.length ? ` (or ${e.alternatives.map((a) => lookup.troops.get(a)?.name ?? a).join(', ')})` : ''),
            (lookup.behaviors.get(e.behavior)?.name ?? e.behavior) + (e.reaction_force ? ' — reaction force' : ''),
          ])}
        />
      )}
    </>
  )
}
