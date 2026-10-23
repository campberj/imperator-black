import { useEffect, useState } from 'react'
import { parse } from 'yaml'
import type { Content } from '../content'
import { Table } from '../components/Table'
import { useHashParams } from '../routes'
import { loadExample, type Mission } from '../mission'

export interface ShortArc {
  id: string
  name: string
  kind: 'arc'
  faction: string
  opposition: string
  rules_version: string
  force_selection: string
  summary: string
  missions: { label: string; file: string; board: string; force: string }[]
  results: { grade: string; text: string }[]
}

const RAW_ARCS = import.meta.glob('../../../docs/scenarios/arcs/*/*.yaml', { query: '?raw', import: 'default' }) as Record<string, () => Promise<string>>

export const ARCS = Object.entries(RAW_ARCS).filter(([path]) => path.endsWith('/arc.yaml')).map(([path, load]) => ({
  id: path.split('/').at(-2)!,
  load,
})).sort((a, b) => a.id.localeCompare(b.id))

export function Arcs({ content }: { content: Content }) {
  const params = useHashParams()
  const requested = params.get('arc')
  const selected = ARCS.find((a) => a.id === requested) ?? ARCS[0]
  const [data, setData] = useState<{ id: string; arc: ShortArc; missions: Mission[] }>()
  const [error, setError] = useState('')

  useEffect(() => {
    if (!selected) return
    let active = true
    selected.load()
      .then(async (text) => {
        const arc = parse(text) as ShortArc
        const missions = await Promise.all(arc.missions.map((m) => loadExample(`${arc.id}/${m.file.replace(/\.yaml$/, '')}`)))
        if (active) { setData({ id: selected.id, arc, missions }); setError('') }
      })
      .catch((e: unknown) => { if (active) { setData(undefined); setError(e instanceof Error ? e.message : String(e)) } })
    return () => { active = false }
  }, [selected])

  const arc = data?.id === selected?.id ? data.arc : undefined
  const missions = data?.id === selected?.id ? data.missions : []
  const missionByLabel = new Map(missions.map((m, i) => [arc?.missions[i].label, m]))
  const href = (m: ShortArc['missions'][number]) => `#missions?example=${encodeURIComponent(`${arc!.id}/${m.file.replace(/\.yaml$/, '')}`)}`
  const link = (label: string) => {
    const entry = arc?.missions.find((m) => m.label === label)
    return entry ? <a href={href(entry)}>{missionByLabel.get(label)?.title ?? `Mission ${label}`}</a> : label
  }

  return (
    <>
      <h2>Short arcs</h2>
      <div className="filters">
        <label>Arc
          <select value={selected?.id ?? ''} onChange={(e) => { window.location.hash = `arcs?arc=${encodeURIComponent(e.target.value)}` }}>
            {ARCS.map((a) => <option key={a.id} value={a.id}>{a.id}</option>)}
          </select>
        </label>
      </div>
      {error && <p className="error" role="alert">Couldn't load the arc. ({error})</p>}
      {arc && <>
        <h2>{arc.name}</h2>
        <p>{arc.summary}</p>
        <p className="meta">{content.factions.factions.find((f) => f.id === arc.faction)?.name ?? arc.faction} against {content.missions.oppositions?.find((o) => o.id === arc.opposition)?.name ?? arc.opposition} · Force selection: {arc.force_selection}</p>
        <section aria-label="Mission flow">
          <h2>Mission flow</h2>
          <p>{link('1')} → Decisive or Marginal → {link('2A')} → {link('3')}</p>
          <p>{link('1')} → Failure → {link('2B')} → {link('3')}</p>
        </section>
        <Table title="Missions" headers={['Mission', 'Board', 'Opposition / tier', 'Force limit']}
          rows={arc.missions.map((m, i) => [
            <a key={m.file} href={href(m)}>{missionByLabel.get(m.label)?.title ?? `Mission ${m.label}`}</a>,
            m.board,
            `${content.missions.oppositions?.find((o) => o.id === missions[i]?.opposition)?.name ?? missions[i]?.opposition ?? arc.opposition}, tier ${missions[i]?.tier ?? '—'}`,
            missions[i]?.force_limit ? `${missions[i].force_limit!.tv} TV, ${missions[i].force_limit!.models} models` : m.force,
          ])} />
        <Table title="What each grade leads to" headers={['Mission', 'Grade', 'Next']}
          rows={arc.missions.flatMap((m, i) => (missions[i]?.next ?? []).map((n) => [
            m.label, n.grade, n.text,
          ]))} />
        <section>
          <h2>Short arc rules</h2>
          <ol>{(content.campaign?.short_arc?.text ?? []).map((rule, i) => <li key={i}>{rule}</li>)}</ol>
        </section>
        <Table title="Arc results" headers={['Result', 'The story']} rows={arc.results.map((r) => [r.grade, r.text])} />
      </>}
    </>
  )
}
