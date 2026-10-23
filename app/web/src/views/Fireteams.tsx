import { useEffect, useState } from 'react'
import type { Content, Lookup } from '../content'
import { Table } from '../components/Table'
import { modelsText } from '../format'

type Fireteam = {
  id: string
  name: string
  faction: string
  type: string
  source: string
  models: { troop: string; count: number }[]
  findings: string[]
  tier?: number
  behavior?: string
}

export function FireteamsView({ content, lookup }: { content: Content; lookup: Lookup }) {
  const [faction, setFaction] = useState('')
  const [standard, setStandard] = useState<Fireteam[]>([])
  const [error, setError] = useState('')
  useEffect(() => {
    const controller = new AbortController()
    fetch('/fireteams', { signal: controller.signal })
      .then(async (response) => {
        if (!response.ok) throw new Error(`GET /fireteams: ${response.status}`)
        const data = (await response.json()) as { fireteams: Fireteam[] }
        if (!controller.signal.aborted) setStandard(data.fireteams)
      })
      .catch((e: unknown) => { if (!controller.signal.aborted) setError(String(e)) })
    return () => controller.abort()
  }, [])
  return (
    <>
      <h2>Fireteams</h2>
      <p>Fireteam and element mean the same thing. A character joins one fireteam or acts alone.</p>
      <div className="filters">
        <label>Faction
          <select value={faction} onChange={(event) => setFaction(event.target.value)}>
            <option value="">All factions</option>
            {content.factions.factions.map((f) => <option key={f.id} value={f.id}>{f.name}</option>)}
          </select>
        </label>
      </div>
      <Table title="Troop classes" headers={['Class', 'Description']}
        rows={(content.rosters.classes ?? []).map((c) => [c.name, c.text])} />
      <Table title="Fireteam types" headers={['Type', 'Leaders', 'Troopers', 'Specialists', 'Most models']}
        rows={(content.rosters.fireteam_types ?? []).map((t) => [t.name, t.leader.join('–'), t.troopers.join('–'), t.specialists.join('–'), t.max_models])} />
      {error && <p role="alert" className="error">Fireteam reference unavailable: {error}</p>}
      {standard.length > 0 && <Table title="Standard fireteams" headers={['Fireteam', 'Type', 'Models', 'Tier', 'Behavior']}
        rows={standard.filter((t) => !faction || t.faction === faction).map((t) => [
          t.name,
          content.rosters.fireteam_types?.find((x) => x.id === t.type)?.name ?? t.type,
          modelsText(t.models, lookup),
          t.tier ?? '—',
          t.behavior ? lookup.behaviors.get(t.behavior)?.name ?? t.behavior : '—',
        ])} />}
    </>
  )
}
