import type { Content } from '../content'
import { missionRequest, useMission, type Mission } from '../mission'
import { Table } from '../components/Table'
import { navigate, useHashParams } from '../routes'

// The mission dashboard, one letter page to print and mark up during play, as the paper dashboard
// (docs/print/ImperatorBlack_mission-dashboard.pdf, dashboard_flow in tools/print_scenario.py). #dashboard alone is the
// blank one; with a mission's hash parameters (#dashboard?setting=ship&seed=42, #dashboard?example=…) it is filled in.
// No saved state: the live dashboard comes with the campaign tracker.

const ROUNDS = 15
const FORM_ROWS = 6
const STATUS = '□ taken  □ destroyed  □ extracted'

export function Dashboard({ content }: { content: Content }) {
  const params = useHashParams()
  const blank = [...params.keys()].length === 0
  const { example, url } = missionRequest(params, content)
  const { value: m, error } = useMission(blank ? undefined : example, blank ? undefined : url)

  return (
    <>
      <div className="filters">
        {!blank && (
          <button type="button" onClick={() => navigate('missions', Object.fromEntries(params))}>
            Back to the mission
          </button>
        )}
        <button type="button" onClick={() => window.print()}>
          Print
        </button>
        <p className="summary">{blank ? 'The blank dashboard.' : 'Prints on one letter page.'}</p>
      </div>
      {error && (
        <p className="error" role="alert">
          Couldn't load the mission. ({error})
        </p>
      )}
      {!blank && !m && !error && <p className="meta">Loading mission…</p>}
      {(blank || m) && <DashboardSheet mission={m} content={content} />}
    </>
  )
}

type Mark = 'start' | 'threshold' | 'check-in' | 'limit'

/** A row of numbered boxes, shaded by mark, with an optional label under each. */
function Boxes({ label, boxes }: { label: string; boxes: { text: string; mark?: Mark; note?: string }[] }) {
  const notes = boxes.some((b) => b.note)
  return (
    <table className="boxes" aria-label={label}>
      <tbody>
        <tr>
          {boxes.map((b, i) => (
            <td key={i} className={b.mark} data-mark={b.mark}>
              {b.text}
            </td>
          ))}
        </tr>
        {notes && (
          <tr className="notes">
            {boxes.map((b, i) => (
              <td key={i}>{b.note}</td>
            ))}
          </tr>
        )}
      </tbody>
    </table>
  )
}

/** Rows, then blank rows up to at least min. */
const padded = (rows: string[][], width: number, min: number) => [
  ...rows,
  ...Array.from({ length: Math.max(0, min - rows.length) }, () => Array<string>(width).fill('')),
]

function DashboardSheet({ mission: m, content }: { mission?: Mission; content: Content }) {
  const { alert, clock } = content.missions
  const type = m && content.missions.missions.find((x) => x.id === m.mission)
  const elements = new Map(content.factions.elements.map((e) => [e.id, e.name]))
  const named = new Map(alert.thresholds.map((t) => [t.at, t.effect.split(':')[0]]))

  // Alert track: the start shaded yellow, thresholds red.
  const start = m?.alert_start
  const alertBoxes = Array.from({ length: alert.max + 1 }, (_, i) => ({
    text: String(i),
    mark: (i === start ? 'start' : named.has(i) ? 'threshold' : undefined) as Mark | undefined,
    note: [i === start ? 'start' : '', named.get(i) ?? ''].filter(Boolean).join(' · '),
  }))

  // Clock: one box a minute; check-ins red, the limit grey. The blank one is the longest, with both sizes named.
  const longest = clock.sizes.reduce((a, b) => (b.limit > a.limit ? b : a))
  const ck = m?.clock ?? {
    limit: longest.limit,
    die: clock.sizes.map((z) => z.die).join(' / '),
    first_check_in: clock.first_check_in,
    check_in_every: clock.check_in_every,
  }
  const checkIn = (minute: number) => minute >= ck.first_check_in && minute < ck.limit && (minute - ck.first_check_in) % ck.check_in_every === 0
  const clockBoxes = Array.from({ length: ck.limit }, (_, i) => ({
    text: String(i + 1),
    mark: (i + 1 === ck.limit ? 'limit' : checkIn(i + 1) ? 'check-in' : undefined) as Mark | undefined,
  }))
  const sizes = clock.sizes.map((z) => `${z.name.toLowerCase()}s ${z.limit} minutes on a ${z.die}`).join('; ')

  const objectives = m ? m.objectives.map((o) => [String(o.n), o.label, STATUS]) : []
  const contacts = m ? m.contacts.map((c) => [String(c.n), c.where, '', '']) : []
  const enemy = m
    ? [
        [`${elements.get(m.enemies[0]?.element) ?? 'Guard'} (guard)`, 'U  A  E', ''],
        ...(m.reinforcement ? [[`${elements.get(m.reinforcement) ?? m.reinforcement} (reinf.)`, 'U  A  E', '']] : []),
      ]
    : []
  const force = m?.size === 'large' ? 12 : 8

  return (
    <article className="dashboard" aria-labelledby="dashboard-title">
      <header>
        <h2 id="dashboard-title">Mission dashboard</h2>
        <p>
          <b>{m ? m.title : 'Mission: ________________________________'}</b>
        </p>
        <p className="small">Imperator Black · chapter 13 · in a dry-erase sleeve, mark the tracks and fill the boxes as you play.</p>
      </header>

      <section aria-labelledby="dashboard-alert">
        <h3 id="dashboard-alert">Alert track</h3>
        <Boxes label="Alert track" boxes={alertBoxes} />
        <p className="small">
          Raises: {alert.raises.map((r) => `${r.source} +${r.amount}`).join(' · ')}. Thresholds take effect once, at the end of the round.
        </p>
        <Boxes label="Thresholds reached" boxes={alert.thresholds.map((t) => ({ text: `□ ${t.at}: ${named.get(t.at)}` }))} />
      </section>

      <section aria-labelledby="dashboard-clock">
        <h3 id="dashboard-clock">
          Mission clock: {ck.limit} minutes, clock die {ck.die}
          {!m && ` (${sizes})`}
        </h3>
        <Boxes label="Mission clock" boxes={clockBoxes} />
        <p className="small">
          Shaded red: check-ins (alert +1 when reached or passed). Grey: time runs out. End of each round: roll {ck.die}, move the clock, then
          resolve the alert track.
        </p>
        <p className="small">Rounds played</p>
        <Boxes label="Rounds played" boxes={Array.from({ length: ROUNDS }, (_, i) => ({ text: String(i + 1) }))} />
      </section>

      <div className="form">
        <Table title={type ? `Objectives: ${type.name}` : 'Objectives'} headers={['#', 'Where', 'Status']} rows={padded(objectives, 3, m ? 0 : 2)} />
      </div>
      <div className="dashboard-two form">
        <Table title="Contacts" headers={['Chip', 'Where', 'Revealed as', 'State']} rows={padded(contacts, 4, FORM_ROWS)} />
        <Table title="Enemy elements" headers={['Element', 'Awareness', 'Down / Dead / surrendered']} rows={padded(enemy, 3, FORM_ROWS)} />
      </div>
      <div className="form">
        <Table
          title="Player force"
          headers={['Model', 'Wounds', 'Pinned', 'Wounded', 'Down', 'Carrying', 'Extracted']}
          rows={padded([], 7, force)}
        />
      </div>
      <p>Notes: _______________________________________________________________________________________</p>
    </article>
  )
}
