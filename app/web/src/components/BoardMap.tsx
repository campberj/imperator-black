import type { Board, CoverType } from '../board'
import type { DeckDoor } from '../deck'
import { BOARD_COLOURS, COVER_COLOURS, COVER_NAMES, COVER_ORDER, DOOR_COLOURS } from '../boardStyle'

// A board drawn to scale in inches, with the printed sheets' conventions (tools/print_scenario.py). Every part is
// optional, so one component draws decks, land tables and missions:
// - ship: tiles with a 3" grid and their id, name and size; doors coloured by state, locked marked (L); the airlock
// - land: a 6" reference grid; 12" sector lines named A1 (the player's bottom left) onward; terrain by type
// - both: cover with ids, deployment, objectives, the enemy entry (EE), contact chips (?n) and numbered guard models
// Board y grows north, SVG y grows down, so every y is flipped.

const MARGIN = 1.5

export function BoardMap({ board: b, title, securityName = (id) => id, troopName = (id) => id }: {
  board: Board
  title: string
  securityName?: (id: string) => string
  troopName?: (id: string) => string
}) {
  const { width: W, height: H } = b.board
  const fy = (y: number) => H - y
  const rect = (x: number, y: number, w: number, h: number) => ({ x, y: fy(y + h), width: w, height: h })
  const tiles = b.tiles ?? []
  const land = tiles.length === 0
  const cover = [...(b.cover ?? [])].sort((p, q) => COVER_ORDER[p.type] - COVER_ORDER[q.type])
  const objectives = b.objectives ?? (b.objective ? [{ n: 1, ...b.objective }] : [])

  return (
    <svg className="board-map" viewBox={`${-MARGIN} ${-MARGIN} ${W + 2 * MARGIN} ${H + 2 * MARGIN}`} role="img"
      aria-label={`${title}: map, ${W}" by ${H}"`}>
      <rect {...rect(0, 0, W, H)} className="board" />

      {land && (
        <g data-grid="6">
          {range(6, W, 6).map((x) => <line key={`x${x}`} x1={x} x2={x} y1={0} y2={H} className="grid" />)}
          {range(6, H, 6).map((y) => <line key={`y${y}`} x1={0} x2={W} y1={fy(y)} y2={fy(y)} className="grid" />)}
        </g>
      )}
      {land && b.sectors && (
        <g data-sectors="">
          {range(12, W, 12).map((x) => <line key={`x${x}`} x1={x} x2={x} y1={0} y2={H} className="sector-line" />)}
          {range(12, H, 12).map((y) => <line key={`y${y}`} x1={0} x2={W} y1={fy(y)} y2={fy(y)} className="sector-line" />)}
          {b.sectors.map((s) => {
            const col = 'ABCDE'.indexOf(s.name[0])
            const row = Number(s.name.slice(1)) - 1
            return (
              <text key={s.name} data-sector={s.name} x={col * 12 + 0.3} y={fy(row * 12 + 11)} className="sector-name">
                <title>{`Sector ${s.name}: ${s.text}`}</title>
                {s.name}
              </text>
            )
          })}
        </g>
      )}

      {tiles.map((t) => (
        <g key={t.id} data-tile={t.id}>
          <rect {...rect(t.x, t.y, t.w, t.h)} className="tile" />
          {range(t.x + 3, t.x + t.w, 3).map((gx) => <line key={`x${gx}`} x1={gx} x2={gx} y1={fy(t.y)} y2={fy(t.y + t.h)} className="grid" />)}
          {range(t.y + 3, t.y + t.h, 3).map((gy) => <line key={`y${gy}`} x1={t.x} x2={t.x + t.w} y1={fy(gy)} y2={fy(gy)} className="grid" />)}
        </g>
      ))}

      {(b.deploy ?? []).map((d, i) => (
        <rect key={i} data-deploy="" {...rect(d.x, d.y, d.w, d.h)} fill={BOARD_COLOURS.deploy} stroke={BOARD_COLOURS.player} className="deploy" />
      ))}

      {cover.map((c, i) => (
        <g key={c.id ?? i} data-cover={c.id ?? ''} data-type={c.type}>
          <title>{`${c.id ? `${c.id}: ` : ''}${c.label}${c.piece ? `. ${c.piece}` : ''}`}</title>
          <rect {...rect(c.x, c.y, c.w, c.h)} fill={COVER_COLOURS[c.type]} className="cover" />
        </g>
      ))}

      {tiles.map((t) => (
        <text key={t.id} x={t.x + 0.2} y={fy(t.y + t.h) + 0.6} className="tile-label">
          <tspan className="tile-id">{t.id}</tspan>
          <tspan x={t.x + 0.2} dy={0.48}>{t.name}</tspan>
          <tspan x={t.x + 0.2} dy={0.42} className="tile-size">{t.size} tile</tspan>
        </text>
      ))}
      {(b.deploy ?? []).map((d, i) => (
        <text key={i} x={d.x + d.w / 2} y={fy(d.y + 0.35)} textAnchor="middle" className="deploy-label">
          Deploy: {d.label}
        </text>
      ))}

      {/* Cover ids go above the tile labels: the terrain table refers to them. */}
      {cover.filter((c) => c.id).map((c) => (
        <text key={c.id} data-cover-id={c.id} x={c.x + c.w / 2} y={fy(c.y + c.h / 2)} className={`cover-id ${c.type}`}
          textAnchor="middle" dominantBaseline="central">
          {c.id}
        </text>
      ))}

      {(b.doors ?? []).map((d) => <Door key={d.id} door={d} fy={fy} securityName={securityName} />)}
      {b.entry && (
        <g data-entry="">
          <title>{b.entry.label}</title>
          <DoorBar x={b.entry.x} y={fy(b.entry.y)} orientation={b.entry.orientation} colour={BOARD_COLOURS.player} />
        </g>
      )}

      {objectives.map((o) => (
        <g key={o.n} data-objective={o.n}>
          <title>{`Objective${objectives.length > 1 ? ` ${o.n}` : ''}: ${o.label}`}</title>
          <polygon points={diamond(o.x, fy(o.y), 0.45)} fill={BOARD_COLOURS.objective} className="objective" />
          {objectives.length > 1 && (
            <text x={o.x} y={fy(o.y)} textAnchor="middle" dominantBaseline="central" className="marker-label">{o.n}</text>
          )}
        </g>
      ))}

      {b.enemy_entry && (
        <g data-enemy-entry="">
          <title>{b.enemy_entry.label}</title>
          <rect x={b.enemy_entry.x - 0.6} y={fy(b.enemy_entry.y) - 0.6} width={1.2} height={1.2} stroke={BOARD_COLOURS.enemy} className="enemy-entry" />
          <text x={b.enemy_entry.x} y={fy(b.enemy_entry.y)} textAnchor="middle" dominantBaseline="central" fill={BOARD_COLOURS.enemy}
            className="enemy-entry-label">EE</text>
        </g>
      )}

      {(b.contacts ?? []).map((c) => (
        <g key={c.n} data-contact={c.n}>
          <title>{`Contact ?${c.n}${c.where ? ` (${c.where})` : ''}`}</title>
          <circle cx={c.x} cy={fy(c.y)} r={0.55} fill={BOARD_COLOURS.contact} className="contact" />
          <text x={c.x} y={fy(c.y)} textAnchor="middle" dominantBaseline="central" className="contact-label">?{c.n}</text>
        </g>
      ))}

      {(b.enemies ?? []).map((e) => (
        <g key={e.n} data-enemy={e.n}>
          <title>{`${e.n}: ${e.troop ? troopName(e.troop) : 'Enemy model'}${e.state ? `, ${e.state}` : ''}`}</title>
          <circle cx={e.x} cy={fy(e.y)} r={0.45} fill={BOARD_COLOURS.enemy} className="enemy" />
          <text x={e.x} y={fy(e.y)} textAnchor="middle" dominantBaseline="central" className="enemy-label">{e.n}</text>
        </g>
      ))}
    </svg>
  )
}

/** The legend for what this board shows, with the printed sheets' names. */
export function BoardLegend({ board: b }: { board: Board }) {
  const land = (b.tiles ?? []).length === 0
  const types = [...new Set((b.cover ?? []).map((c) => c.type))].sort((p, q) => COVER_ORDER[p] - COVER_ORDER[q])
  const states = new Set((b.doors ?? []).map((d) => d.state))
  const items: { name: string; colour: string; outline?: boolean }[] = [
    ...(['open', 'closed', 'locked'] as const)
      .filter((s) => states.has(s))
      .map((s) => ({ name: s === 'locked' ? 'Locked door (L)' : `${s[0].toUpperCase()}${s.slice(1)} door`, colour: DOOR_COLOURS[s] })),
    ...types.map((t: CoverType) => ({
      name: land ? COVER_NAMES[t] : t === 'light' ? 'Light cover' : t === 'heavy' ? 'Heavy cover' : COVER_NAMES[t],
      colour: COVER_COLOURS[t],
    })),
  ]
  if (b.entry) items.push({ name: 'Airlock', colour: BOARD_COLOURS.player })
  if ((b.enemies ?? []).length) items.push({ name: 'Enemy model', colour: BOARD_COLOURS.enemy })
  if ((b.deploy ?? []).length) items.push({ name: 'Deployment', colour: BOARD_COLOURS.deploy })
  if (b.objective || (b.objectives ?? []).length) items.push({ name: 'Objective', colour: BOARD_COLOURS.objective })
  if ((b.contacts ?? []).length) items.push({ name: 'Contact chip (?n)', colour: BOARD_COLOURS.contact, outline: true })
  if (b.enemy_entry) items.push({ name: 'Enemy entry (EE)', colour: '#fff', outline: true })
  return (
    <ul className="legend" aria-label="Legend">
      {items.map((i) => (
        <li key={i.name}>
          <span className={i.outline ? 'swatch outline' : 'swatch'} style={{ background: i.colour }} />
          {i.name}
        </li>
      ))}
    </ul>
  )
}

function Door({ door: d, fy, securityName }: { door: DeckDoor; fy: (y: number) => number; securityName: (id: string) => string }) {
  const colour = DOOR_COLOURS[d.state]
  const y = fy(d.y)
  const label = d.id + (d.state === 'locked' ? ' (L)' : '')
  const [lx, ly] = d.orientation === 'v' ? [d.x + 0.2, y + 1.2] : [d.x + 0.95, y - 0.2]
  const state = d.state[0].toUpperCase() + d.state.slice(1)
  return (
    <g data-door={d.id} data-state={d.state}>
      <title>{`${d.id}, ${d.note}: ${state}` + (d.security ? `, ${securityName(d.security)} security` : '')}</title>
      <DoorBar x={d.x} y={y} orientation={d.orientation} colour={colour} />
      <text x={lx} y={ly} fill={colour} className="door-label">{label}</text>
    </g>
  )
}

function DoorBar({ x, y, orientation, colour }: { x: number; y: number; orientation: 'h' | 'v'; colour: string }) {
  const half = 0.75
  return orientation === 'v' ? (
    <line x1={x} x2={x} y1={y - half} y2={y + half} stroke={colour} className="door" />
  ) : (
    <line x1={x - half} x2={x + half} y1={y} y2={y} stroke={colour} className="door" />
  )
}

/** from, from + step, … below to. */
const range = (from: number, to: number, step: number) => {
  const out: number[] = []
  for (let v = from; v < to; v += step) out.push(v)
  return out
}

const diamond = (x: number, y: number, r: number) => `${x},${y - r} ${x + r},${y} ${x},${y + r} ${x - r},${y}`
