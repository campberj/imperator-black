import type { Content, Element, Lookup } from '../content'
import { EXAMPLES, exampleLabel, missionRequest, useMission, type Mission } from '../mission'
import { BoardLegend, BoardMap } from '../components/BoardMap'
import { SeedForm } from '../components/SeedForm'
import { Table } from '../components/Table'
import { navigate, useHashParams } from '../routes'

// A chapter 13 mission, laid out like the printed mission sheet (tools/print_scenario.py, mission_sheet): the briefing
// and map, then how it plays (taking it, lockdown, grades, alert track and clock, contacts, the enemy), then the ground.
// The mission comes from GET /missions, or from an example in docs/scenarios/missions.

const capitalise = (s: string) => s[0].toUpperCase() + s.slice(1)
const inches = (v: number) => `${Number(v.toFixed(2))}"`

export function Missions({ content, lookup }: { content: Content; lookup: Lookup }) {
  const params = useHashParams()
  const { example, asked, seed, url } = missionRequest(params, content)
  const { setting, mission, tier, vessel, environment } = asked
  const size = asked.size || 'small'
  const { value: m, error } = useMission(example, url)
  const show = (change: Partial<typeof asked> & { seed?: number }) => {
    const next = { ...asked, seed, ...change }
    if (next.setting === 'ship') next.environment = next.size = ''
    else next.vessel = ''
    navigate('missions', Object.fromEntries(Object.entries(next).filter(([, v]) => v !== '')) as Record<string, string | number>)
  }
  const select = (label: string, value: string, options: { id: string; name: string }[], onChange: (v: string) => void, rolled = true) => (
    <label>
      {label}
      <select value={value} onChange={(e) => onChange(e.target.value)}>
        {rolled && <option value="">Rolled</option>}
        {options.map((o) => (
          <option key={o.id} value={o.id}>
            {o.name}
          </option>
        ))}
      </select>
    </label>
  )

  return (
    <>
      {example ? (
        <div className="filters">
          <p className="summary">
            Showing <b>{exampleLabel(example)}</b>, read from <code>{example.startsWith('foundry-moon/') ? `docs/scenarios/arcs/${example}.yaml` : `docs/scenarios/missions/${example}.yaml`}</code>.
          </p>
          <button type="button" onClick={() => navigate('missions')}>
            Generate a mission
          </button>
        </div>
      ) : (
        <SeedForm label="Mission" seed={seed} onSeed={(s) => show({ seed: s })}>
          {select('Setting', setting, [{ id: 'ship', name: 'Aboard ship' }, { id: 'land', name: 'On land' }], (v) => show({ setting: v as 'ship' | 'land' }), false)}
          {select('Mission', mission, content.missions.missions, (v) => show({ mission: v }))}
          {select('Opposition', asked.opposition, (content.missions.oppositions ?? []).map((o) => ({ id: o.id, name: o.name })), (v) => show({ opposition: v }))}
          {select('Tier', tier, content.missions.tiers.map((t) => ({ id: String(t.tier), name: `Threat tier ${t.tier}` })), (v) => show({ tier: v }))}
          {setting === 'ship' && select('Vessel', vessel, content.layouts.vessels, (v) => show({ vessel: v }))}
          {setting === 'land' && select('Environment', environment, content.terrain.environments, (v) => show({ environment: v }))}
          {setting === 'land' &&
            select('Size', size, content.missions.clock.sizes.map((z) => ({ id: z.id, name: z.name })), (v) => show({ size: v as 'small' | 'large' }), false)}
        </SeedForm>
      )}
      {!example && EXAMPLES.length > 0 && (
        <p className="meta">
          Or show an example:{' '}
          {EXAMPLES.map((e, i) => (
            <span key={e.id}>
              {i > 0 && ' · '}
              <a href={`#missions?example=${e.id}`}>{exampleLabel(e.id)}</a>
            </span>
          ))}
        </p>
      )}

      {error && (
        <p className="error" role="alert">
          Couldn't {example ? 'load the example' : 'generate the mission'}. ({error})
        </p>
      )}
      {!m && !error && <p className="meta">{example ? 'Loading example…' : 'Generating mission…'}</p>}
      {m && (
        <div className="filters">
          <button type="button" onClick={() => navigate('dashboard', example ? { example } : dashboardParams(asked, seed))}>
            Dashboard
          </button>
          <p className="summary">A one-page dashboard for this mission, to print and mark up during play.</p>
        </div>
      )}
      {m && <MissionSheet mission={m} content={content} lookup={lookup} />}
    </>
  )
}

const dashboardParams = (asked: Record<string, string>, seed: number) =>
  Object.fromEntries([...Object.entries(asked).filter(([, v]) => v), ['seed', seed]]) as Record<string, string | number>

function MissionSheet({ mission: m, content, lookup }: { mission: Mission; content: Content; lookup: Lookup }) {
  const type = content.missions.missions.find((x) => x.id === m.mission)
  const brief = new Map((m.briefing ?? []).map((b) => [b.heading, b.text]))
  const para = (heading: string) =>
    brief.has(heading) && (
      <p key={heading}>
        <b>{heading}.</b> {brief.get(heading)}
      </p>
    )
  const security = new Map(content.core.security.levels.map((l) => [l.id, l.name]))
  const securityName = (id: string) => security.get(id) ?? capitalise(id)
  const troopName = (id: string) => lookup.troops.get(id)?.name ?? id
  const alert = content.missions.alert

  return (
    <article className="deck mission" aria-labelledby="mission-title">
      <header>
        <h2 id="mission-title">{m.title}</h2>
        <p className="meta">
          Imperator Black mission · rules v{m.rules_version} · chapter 13 · {m.setting === 'ship' ? 'aboard ship' : 'land'} · threat tier {m.tier} · seed{' '}
          {m.seed} · solo{m.content_version && ` · content v${m.content_version}`}
        </p>
        {para('Tasking')}
        {para('Situation')}
        {m.arc && <p className="meta">{m.arc.name} · {m.arc.label}</p>}
      </header>

      <div className="deck-layout">
        <figure className="deck-figure">
          <BoardMap board={m} title={m.title} securityName={securityName} troopName={troopName} />
          <BoardLegend board={m} />
          <figcaption className="small">{m.map_note && <p>{m.map_note}</p>}
            {m.setting === 'ship'
              ? `Scale: each grid square is 3". Board ${m.board.width}" × ${m.board.height}". The blue bar is the airlock: the way in and out. ` +
                'Enemy models (numbered) are the guard, placed openly. Contact chips (?n) are hidden contacts: roll for them when revealed.'
              : `Each grid square is 6"; heavier lines mark the 12" sectors (A1 bottom left). Board ${m.board.width}" × ${m.board.height}". ` +
                'The player edge is the bottom. Pieces are footprints from the sector rolls: place your own terrain to match. Contact chips (?n) are hidden contacts.'}
          </figcaption>
        </figure>
        <div className="deck-tables">{['Objective', 'Opposition', 'Ground', 'Complication', 'Extraction and time', 'Force'].map(para)}</div>
      </div>

      <section className="sheet-page" aria-labelledby="plays">
        <h2 id="plays">{type?.name ?? m.mission}</h2>
        {type && (
          <>
            <p>
              <b>Taking it.</b> {m.task ?? type.task}
            </p>
            <p>
              <b>Lockdown (alert 6).</b> {m.lockdown ?? type.lockdown}
            </p>
            <Table
              title="Grades"
              headers={['Grade', 'Result']}
              rows={[
                ['Decisive', m.grades?.decisive ?? type.decisive],
                ['Marginal', m.grades?.marginal ?? type.marginal],
                ['Failure', m.grades?.failure ?? type.failure],
              ]}
            />
          </>
        )}

        {(m.next?.length ?? 0) > 0 && <Table title="After the mission" headers={['Grade', 'Next']} rows={m.next!.map((n) => [n.grade, n.text])} />}
        {(m.special_rules?.length ?? 0) > 0 && <section><h2>Special rules</h2>{m.special_rules!.map((r) => <p key={r.title}><b>{r.title}.</b> {r.text}</p>)}</section>}
        <h2>Alert track and clock</h2>
        <p>
          <b>Raises:</b> {alert.raises.map((r) => `${r.source} +${r.amount}`).join('; ')}. Starts at {m.alert_start}.
        </p>
        {alert.thresholds.map((t) => (
          <p key={t.at}>
            <b>At {t.at}:</b> {t.effect}
          </p>
        ))}
        <p>
          <b>Clock:</b> {m.clock.limit} minutes, clock die {m.clock.die}; check-ins at minute {m.clock.first_check_in} and every {m.clock.check_in_every} after.
        </p>
        <ul>
          {content.missions.clock.rules.map((r) => (
            <li key={r}>{r}</li>
          ))}
        </ul>

        <h2>Contacts (threat tier {m.tier})</h2>
        <ol>
          {content.missions.contacts.map((c) => (
            <li key={c}>{c}</li>
          ))}
        </ol>
        <ContactTable mission={m} content={content} lookup={lookup} />
        <EnemyTable mission={m} content={content} lookup={lookup} />
      </section>

      <section className="sheet-page" aria-label="Ground">
        {m.setting === 'ship' ? (
          <>
            <Table
              title="Rooms"
              headers={['Tile', 'Room', 'Notes']}
              rows={(m.tiles ?? [])
                .filter((t) => t.id.startsWith('C') || t.id.startsWith('S'))
                .map((t) => [t.id, t.name, content.layouts.rooms.find((r) => r.name === t.name)?.text ?? ''])}
            />
            <Table
              title="Doors"
              headers={['Door', 'Between', 'State', 'Security']}
              rows={(m.doors ?? []).map((d) => [d.id, d.note, capitalise(d.state), d.security ? securityName(d.security) : '—'])}
            />
            <h2>Hazard</h2>
            <p>{m.hazard || 'None'}</p>
            {(m.deck_notes ?? []).map((n) => (
              <p key={n}>{n}</p>
            ))}
          </>
        ) : (
          <Table title="Sectors" headers={['Sector', 'Roll result']} rows={(m.sectors ?? []).map((s) => [s.name, s.text])} />
        )}
        <Table
          title="Terrain"
          headers={['Map', 'Piece', 'Cover', 'Footprint', 'Suggested terrain']}
          rows={m.cover.map((c) => [c.id, c.label, capitalise(c.type), `${inches(c.w)} × ${inches(c.h)}`, c.piece ?? 'Any suitable scenery.'])}
        />
        <p className="small">
          Light cover: at least a third of the model hidden. Heavy cover: at least half hidden behind solid material. Exact pieces don't matter; keep
          the footprint and height close.
        </p>
        <h2>Rolls</h2>
        <p className="small">{(m.rolls ?? []).join('; ')}.</p>
      </section>
    </article>
  )
}

/** "3 × Salvage hand, Security gunner", as on the printed sheet. */
function composition(el: Element, lookup: Lookup) {
  return el.composition.map((c) => (c.count > 1 ? `${c.count} × ${lookup.troops.get(c.troop)?.name}` : lookup.troops.get(c.troop)?.name)).join(', ')
}

function ContactTable({ mission: m, content, lookup }: { mission: Mission; content: Content; lookup: Lookup }) {
  const tier = (content.missions.oppositions?.find((o) => o.id === m.opposition)?.tiers ?? content.missions.tiers).find((t) => t.tier === m.tier)
  const elements = new Map(content.factions.elements.map((e) => [e.id, e]))
  const special = new Map(content.missions.contact_results.map((r) => [r.id, r]))
  return (
    <Table
      title="Contact table"
      headers={['d6', 'Contact', 'Models', 'Behavior']}
      rows={(tier?.table ?? []).map((r) => {
        const s = special.get(r.result)
        if (s) return [r.roll, s.name, s.text, '—']
        const el = elements.get(r.result)!
        return [r.roll, el.name, composition(el, lookup), lookup.behaviors.get(el.behavior)?.name ?? el.behavior]
      })}
    />
  )
}

/**
 * Every element the mission can put on the table (guard, contact table, reinforcement), with its drill: set by the
 * lowest rank in the element, raised one step by its Leader within 6" (element_drill in tools/print_scenario.py).
 */
function EnemyTable({ mission: m, content, lookup }: { mission: Mission; content: Content; lookup: Lookup }) {
  const tier = content.missions.tiers.find((t) => t.tier === m.tier)
  const elements = new Map(content.factions.elements.map((e) => [e.id, e]))
  const guardId = m.enemies[0]?.element
  const ids = [guardId, ...(tier?.table ?? []).map((r) => r.result), m.reinforcement ?? tier?.reinforcement].filter((id): id is string => !!id && elements.has(id))
  const ranks = content.core.ranks.map((r) => r.id)
  const drillByRank = new Map(content.behaviors.drills.map((d) => [d.rank, d.name]))
  const isLeader = (troop: string) => 'leader' in (lookup.troops.get(troop)?.skills ?? {})
  const hvtLeads = !!m.hvt && isLeader(m.hvt)

  const drill = (el: Element, extraLeader: boolean) => {
    const low = Math.min(...el.composition.map((c) => ranks.indexOf(lookup.troops.get(c.troop)!.rank)))
    let text = drillByRank.get(ranks[low]) ?? ranks[low]
    if ((el.composition.some((c) => isLeader(c.troop)) || extraLeader) && low < ranks.length - 1)
      text += `; ${drillByRank.get(ranks[low + 1])} with its Leader within 6"`
    return text
  }

  return (
    <section>
      <h2>The enemy</h2>
      <p>Run the enemy with chapter 10 and the behavior cards. The enemy's objective, for tie-breakers, is the mission objective.</p>
      <Table
        title="Elements"
        headers={['Element', 'When', 'Behavior', 'Drill']}
        rows={[...new Set(ids)].map((id) => {
          const el = elements.get(id)!
          const when = []
          if (id === guardId) when.push(`Guard: models ${m.enemies.map((e) => e.n).join(', ')}`)
          if (tier?.table.some((r) => r.result === id)) when.push('Contact')
          if (id === tier?.reinforcement) when.push('Reinforcements (alert 4), at the enemy entry')
          return [el.name, when.join('; '), lookup.behaviors.get(el.behavior)?.name ?? el.behavior, drill(el, id === guardId && hvtLeads)]
        })}
      />
    </section>
  )
}
