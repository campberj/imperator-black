import { useEffect, useState } from 'react'
import type { Content, Troop } from '../content'
import { loadExample, EXAMPLES } from '../mission'
import { Table } from '../components/Table'
import { useHashParams } from '../routes'

type Slot = { troop: string; count: number; weapon?: string; armour?: string; gear?: string[] }
type Team = { id: string; name: string; type: string; standard?: string; slots: Slot[] }
type Character = { troop: string; joins?: string; weapon?: string; armour?: string; gear?: string[] }
type Soldier = { troop: string; group: number; weapon?: string; armour?: string; gear?: string[] }
type Plan = { name: string; faction: string; setting: 'fireteam' | 'soldier'; source: string; sourceId?: string; tvLimit?: number; modelLimit?: number; fireteams: Team[]; characters: Character[]; soldiers: Soldier[] }
type Result = { tv: number; models: number; fireteams: number; characters: number; findings: { code: string; subject?: string; message: string }[]; team_totals: { id: string; tv: number; models: number }[]; character_totals: { index: number; tv: number }[]; troop_values: Record<string, number> }
const example = (): Plan => ({ name: 'My force', faction: 'republic', setting: 'fireteam', source: 'custom', tvLimit: 350, fireteams: [], characters: [], soldiers: [] })
export function Planner({ content }: { content: Content }) {
  const params = useHashParams()
  const encoded = params.get('plan')
  const [plan, setPlan] = useState<Plan>(() => { try { return encoded ? JSON.parse(encoded) as Plan : example() } catch { return example() } })
  const [result, setResult] = useState<Result>()
  const [error, setError] = useState('')
  const [newTeam, setNewTeam] = useState('')
  const [newType, setNewType] = useState('line')
  const [newTroop, setNewTroop] = useState('rep_clone_trooper')
  const [newCharacter, setNewCharacter] = useState('rep_ahsoka')
  const troops = content.troops.troops.filter(t => t.faction === plan.faction)
  const characters = troops.filter(t => t.class === 'character')
  const standard = [...content.rosters.teams.filter(t => t.faction === plan.faction), ...content.factions.elements.filter(e => e.faction === plan.faction)]
  const troop = (id: string) => content.troops.troops.find(t => t.id === id)
  const names = (id: string) => troop(id)?.name ?? id
  const update = (f: (p: Plan) => Plan) => setPlan(p => f(p))
  useEffect(() => { window.history.replaceState(null, '', `#planner?plan=${encodeURIComponent(JSON.stringify(plan))}`) }, [plan])
  useEffect(() => {
    const abort = new AbortController()
    const timer = window.setTimeout(() => {
      fetch('/forces/check', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(plan), signal: abort.signal })
        .then(async r => { if (!r.ok) throw new Error(`Force check: ${r.status}`); return await r.json() as Result })
        .then(v => { if (!abort.signal.aborted) { setResult(v); setError('') } })
        .catch(e => { if (!abort.signal.aborted) setError(String(e)) })
    }, 180)
    return () => { window.clearTimeout(timer); abort.abort() }
  }, [plan])
  const setLimit = async (source: string, sourceId: string) => {
    if (source === 'versus') {
      const size = content.versus?.sizes.find(s => s.id === sourceId)
      update(p => ({ ...p, source, sourceId: sourceId, setting: 'fireteam', tvLimit: size?.tv, modelLimit: undefined }))
    } else if (source === 'mission') {
      const size = content.missions.clock.sizes.find(s => s.id === sourceId)
      update(p => ({ ...p, source, sourceId: sourceId, modelLimit: size?.id === 'small' ? 8 : 12, tvLimit: undefined }))
    } else if (source === 'arc') {
      const mission = await loadExample(sourceId)
      update(p => ({ ...p, source, sourceId: sourceId, modelLimit: mission.force_limit?.models, tvLimit: mission.force_limit?.tv }))
    } else update(p => ({ ...p, source: 'custom', sourceId: undefined }))
  }
  const addStandard = () => {
    const team = standard.find(t => t.id === newTeam) ?? standard[0]
    if (!team) return
    const slots = 'slots' in team ? team.slots : team.composition
    update(p => ({ ...p, fireteams: [...p.fireteams, { id: `team-${p.fireteams.length + 1}`, name: team.name, type: team.type ?? 'line', standard: team.id, slots: slots.map(s => ({ troop: s.troop, count: s.count })) }] }))
  }
  const addCustom = () => update(p => ({ ...p, fireteams: [...p.fireteams, { id: `team-${p.fireteams.length + 1}`, name: 'Built fireteam', type: newType, slots: [] }] }))
  const updateTeam = (index: number, fn: (t: Team) => Team) => update(p => ({ ...p, fireteams: p.fireteams.map((t, i) => i === index ? fn(t) : t) }))
  const addCharacter = () => update(p => ({ ...p, characters: [...p.characters, { troop: newCharacter }] }))
  const modelOptions = (t: Troop, what: 'weapon' | 'armour') => [t[what], ...(what === 'weapon' ? t.weapon_options ?? [] : t.armour_options ?? [])].filter((x): x is string => x !== null && x !== undefined)
  const kit = (slot: Slot | Character | Soldier, change: (s: typeof slot) => void) => {
    const t = troop(slot.troop)
    if (!t) return null
    return <span className="planner-kit">
      {(['weapon', 'armour'] as const).map(k => <label key={k}>{k === 'weapon' ? 'Weapon' : 'Armour'}
        <select value={slot[k] ?? t[k] ?? ''} onChange={e => change({ ...slot, [k]: e.target.value })}>
          {modelOptions(t, k).length ? modelOptions(t, k).map(id => <option key={id} value={id}>{k === 'weapon' ? content.weapons.ranged.find(w => w.id === id)?.name : content.armour.armour.find(a => a.id === id)?.name}</option>) : <option value="">None</option>}
        </select>
      </label>)}
      {(t.gear_options?.length ?? 0) > 0 && <label>Gear
        <select value={(slot.gear ?? t.gear)[0] ?? ''} onChange={e => change({ ...slot, gear: e.target.value ? [...t.gear, e.target.value] : [...t.gear] })}>
          <option value="">Profile gear</option>{t.gear_options!.map(g => <option key={g} value={g}>{content.gear.gear.find(x => x.id === g)?.name ?? g}</option>)}
        </select>
      </label>}
    </span>
  }
  const addSoldier = () => update(p => ({ ...p, soldiers: [...p.soldiers, { troop: newTroop, group: 1 }] }))
  const kitSummary = (selection: Slot | Character | Soldier) => {
    const profile = troop(selection.troop)
    if (!profile) return ''
    const weapon = selection.weapon ?? profile.weapon
    const armour = selection.armour ?? profile.armour
    const gear = selection.gear ?? profile.gear
    return [weapon ? content.weapons.ranged.find(w => w.id === weapon)?.name ?? weapon : 'Unarmed', content.armour.armour.find(a => a.id === armour)?.name ?? armour, ...gear.map(g => content.gear.gear.find(x => x.id === g)?.name ?? g)].join(', ')
  }
  return <section className="planner">
    <div className="planner-heading"><h2>Force planner</h2><button type="button" onClick={() => window.print()}>Print force list</button></div>
    <p className="meta">A planning calculator. Warnings do not prevent play. The plan stays in the URL; nothing is saved.</p>
    <div className="filters planner-settings">
      <label>Force name<input value={plan.name} onChange={e => update(p => ({ ...p, name: e.target.value }))} /></label>
      <label>Faction<select value={plan.faction} onChange={e => update(p => ({ ...p, faction: e.target.value, fireteams: [], characters: [], soldiers: [] }))}>{content.factions.factions.filter(f => f.side === 'player').map(f => <option key={f.id} value={f.id}>{f.name}</option>)}</select></label>
      <label>Limits from<select value={plan.source === 'custom' ? 'custom' : `${plan.source}:${plan.sourceId ?? ''}`} onChange={e => { const [source, ...id] = e.target.value.split(':'); void setLimit(source, id.join(':')) }}>
        <option value="custom">Custom</option>
        {(content.versus?.sizes ?? []).map(s => <option key={s.id} value={`versus:${s.id}`}>{s.name} (two-player)</option>)}
        {content.missions.clock.sizes.map(s => <option key={s.id} value={`mission:${s.id}`}>{s.name}</option>)}
        {EXAMPLES.filter(e => e.id.includes('/')).map(e => <option key={e.id} value={`arc:${e.id}`}>{e.id}</option>)}
      </select></label>
      <label>TV limit<input type="number" value={plan.tvLimit ?? ''} onChange={e => update(p => ({ ...p, tvLimit: e.target.value ? Number(e.target.value) : undefined }))} /></label>
      <label>Model limit<input type="number" value={plan.modelLimit ?? ''} onChange={e => update(p => ({ ...p, modelLimit: e.target.value ? Number(e.target.value) : undefined }))} /></label>
      <label>Force selection<select value={plan.setting} disabled={plan.source === 'versus'} onChange={e => update(p => ({ ...p, setting: e.target.value as Plan['setting'] }))}>
        <option value="fireteam">By fireteam</option><option value="soldier">By soldier</option>
      </select></label>
    </div>
    <div className="planner-summary" aria-live="polite">{result ? <>
      <strong>{result.tv} TV</strong> · {plan.setting === 'fireteam' ? 'By fireteam' : 'By soldier'} · {result.models} models · {result.fireteams} fireteams · {result.characters} characters
      {plan.tvLimit !== undefined && ` · TV limit ${plan.tvLimit}`}
      {plan.modelLimit !== undefined && ` · Model limit ${plan.modelLimit}`}
    </> : 'Checking force…'}</div>
    {error && <p role="alert" className="error">{error}</p>}
    <h2>Findings</h2>
    {result?.findings.length ? <ul>{result.findings.map((f, i) => <li key={i}>{f.message}</li>)}</ul> : <p>No findings.</p>}
    {plan.setting === 'fireteam' ? <>
      <h2>Fireteams</h2>
      <div className="planner-actions">
        <label>Standard fireteam<select value={newTeam} onChange={e => setNewTeam(e.target.value)}>{standard.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}</select></label>
        <button type="button" onClick={addStandard}>Add standard fireteam</button>
        <label>Build to type<select value={newType} onChange={e => setNewType(e.target.value)}>{(content.rosters.fireteam_types ?? []).map(t => <option key={t.id} value={t.id}>{t.name}</option>)}</select></label>
        <button type="button" onClick={addCustom}>Build fireteam</button>
      </div>
      {plan.fireteams.map((team, i) => <section key={team.id} className="planner-team">
        <h3>{team.name} · {team.type} · {result?.team_totals.find(x => x.id === team.id)?.tv ?? '—'} TV</h3>
        {team.slots.map((slot, j) => <div className="planner-model" key={j}>
          <a href="#units">{names(slot.troop)}</a>
          <label>Models<input type="number" min="0" value={slot.count} onChange={e => updateTeam(i, t => ({ ...t, slots: t.slots.map((x,k) => k === j ? { ...x, count: Number(e.target.value) } : x) }))} /></label>
          {kit(slot, x => updateTeam(i, t => ({ ...t, slots: t.slots.map((v,k) => k === j ? x as Slot : v) })))}
          {!team.standard && <button type="button" onClick={() => updateTeam(i, t => ({ ...t, slots: t.slots.filter((_,k) => k !== j) }))}>Remove model</button>}
        </div>)}
        {!team.standard && <div className="planner-actions"><select aria-label={`Troop for ${team.name}`} value={newTroop} onChange={e => setNewTroop(e.target.value)}>{troops.filter(t => t.class !== 'character').map(t => <option key={t.id} value={t.id}>{t.name}</option>)}</select><button type="button" onClick={() => updateTeam(i, t => ({ ...t, slots: [...t.slots, { troop: newTroop, count: 1 }] }))}>Add model</button></div>}
        <button type="button" onClick={() => update(p => ({ ...p, fireteams: p.fireteams.filter((_,k) => k !== i) }))}>Remove fireteam</button>
      </section>)}
    </> : <>
      <h2>Soldiers</h2>
      <div className="planner-actions"><select aria-label="Soldier to add" value={newTroop} onChange={e => setNewTroop(e.target.value)}>{troops.filter(t => t.class !== 'character').map(t => <option key={t.id} value={t.id}>{t.name}</option>)}</select><button type="button" onClick={addSoldier}>Add soldier</button></div>
      {plan.soldiers.map((s,i) => <div className="planner-model" key={i}>
        <a href="#units">{names(s.troop)}</a>
        <label>Group<input type="number" min="1" value={s.group} onChange={e => update(p => ({ ...p, soldiers: p.soldiers.map((x,k) => k === i ? { ...x, group: Number(e.target.value) } : x) }))} /></label>
        {kit(s, v => update(p => ({ ...p, soldiers: p.soldiers.map((x,k) => k === i ? v as Soldier : x) })))}
        <button type="button" onClick={() => update(p => ({ ...p, soldiers: p.soldiers.filter((_,k) => k !== i) }))}>Remove</button>
      </div>)}
    </>}
    <h2>Characters</h2>
    <div className="planner-actions"><select aria-label="Character to add" value={newCharacter} onChange={e => setNewCharacter(e.target.value)}>{characters.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}</select><button type="button" onClick={addCharacter}>Add character</button></div>
    {plan.characters.map((c,i) => <div className="planner-model" key={i}>
      <a href="#units">{names(c.troop)}</a>
      <label>Deployment<select value={c.joins ?? ''} onChange={e => update(p => ({ ...p, characters: p.characters.map((x,k) => k === i ? { ...x, joins: e.target.value || undefined } : x) }))}>
        <option value="">Alone</option>
        {(plan.setting === 'fireteam' ? plan.fireteams.map(t => ({ id: t.id, name: t.name })) : [...new Set(plan.soldiers.map(s => s.group))].map(n => ({ id: String(n), name: `Group ${n}` }))).map(t => <option value={t.id} key={t.id}>Joins {t.name}</option>)}
      </select></label>
      {kit(c, v => update(p => ({ ...p, characters: p.characters.map((x,k) => k === i ? v as Character : x) })))}
      <button type="button" onClick={() => update(p => ({ ...p, characters: p.characters.filter((_,k) => k !== i) }))}>Remove</button>
    </div>)}
    <section className="planner-print">
      <Table title={plan.name} headers={['Fireteam / group', 'Models and loadout', 'TV']}
        rows={[...plan.fireteams.map(t => [`${t.name} (${t.type})`, t.slots.filter(s => s.count > 0).map(s => `${s.count} × ${names(s.troop)} (${kitSummary(s)})`).join('; '), result?.team_totals.find(x => x.id === t.id)?.tv ?? '—']), ...plan.soldiers.map(s => [`Group ${s.group}`, `${names(s.troop)} (${kitSummary(s)})`, result?.troop_values[s.troop] ?? '—']), ...plan.characters.map((c,i) => [c.joins ? `Joins ${c.joins}` : 'Alone', `${names(c.troop)} (${kitSummary(c)})`, result?.character_totals.find(x => x.index === i)?.tv ?? '—'])]} />
    </section>
  </section>
}
