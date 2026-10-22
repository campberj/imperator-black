import type { Deck, DeckDoor } from '../deck'
import { COVER_COLOURS, DECK_COLOURS, DOOR_COLOURS } from '../deckStyle'

// The deck drawn to scale in inches, with the printed deck sheets' conventions (tools/print_scenario.py):
// tiles with a 3" grid and their id, name and size; cover with ids; doors coloured by state, locked doors
// marked (L); the airlock as a blue bar on the west edge; the objective as a gold diamond.
// Deck y grows north, SVG y grows down, so every y is flipped.

const MARGIN = 1.5

export function DeckMap({ deck, securityName }: { deck: Deck; securityName: (id: string) => string }) {
  const { width: W, height: H } = deck.board
  const fy = (y: number) => H - y
  const rect = (x: number, y: number, w: number, h: number) => ({ x, y: fy(y + h), width: w, height: h })
  const cover = [...deck.cover].sort((a, b) => (a.type === b.type ? 0 : a.type === 'light' ? -1 : 1))

  return (
    <svg
      className="deck-map"
      viewBox={`${-MARGIN} ${-MARGIN} ${W + 2 * MARGIN} ${H + 2 * MARGIN}`}
      role="img"
      aria-label={`${deck.title}: map, ${W}" by ${H}"`}
    >
      <rect {...rect(0, 0, W, H)} className="board" />

      {deck.tiles.map((t) => (
        <g key={t.id} data-tile={t.id}>
          <rect {...rect(t.x, t.y, t.w, t.h)} className="tile" />
          {range(t.x + 3, t.x + t.w, 3).map((gx) => (
            <line key={`x${gx}`} x1={gx} x2={gx} y1={fy(t.y)} y2={fy(t.y + t.h)} className="grid" />
          ))}
          {range(t.y + 3, t.y + t.h, 3).map((gy) => (
            <line key={`y${gy}`} x1={t.x} x2={t.x + t.w} y1={fy(gy)} y2={fy(gy)} className="grid" />
          ))}
        </g>
      ))}

      {cover.map((c) => (
        <g key={c.id} data-cover={c.id}>
          <title>{`${c.id}: ${c.label} (${c.type} cover), ${c.w}" × ${c.h}"`}</title>
          <rect {...rect(c.x, c.y, c.w, c.h)} fill={COVER_COLOURS[c.type]} className="cover" />
        </g>
      ))}

      {deck.tiles.map((t) => (
        <text key={t.id} x={t.x + 0.2} y={fy(t.y + t.h) + 0.6} className="tile-label">
          <tspan className="tile-id">{t.id}</tspan>
          <tspan x={t.x + 0.2} dy={0.48}>
            {t.name}
          </tspan>
          <tspan x={t.x + 0.2} dy={0.42} className="tile-size">
            {t.size} tile
          </tspan>
        </text>
      ))}

      {/* Cover ids go above the tile labels: the terrain table refers to them. */}
      {cover.map((c) => (
        <text
          key={c.id}
          data-cover-id={c.id}
          x={c.x + c.w / 2}
          y={fy(c.y + c.h / 2)}
          className={`cover-id ${c.type}`}
          textAnchor="middle"
          dominantBaseline="central"
        >
          {c.id}
        </text>
      ))}

      {deck.doors.map((d) => (
        <Door key={d.id} door={d} fy={fy} securityName={securityName} />
      ))}

      <g data-entry="">
        <title>{deck.entry.label}</title>
        <DoorBar x={deck.entry.x} y={fy(deck.entry.y)} orientation={deck.entry.orientation} colour={DECK_COLOURS.airlock} />
      </g>

      {deck.objective && (
        <g data-objective="">
          <title>{`Objective: ${deck.objective.label}`}</title>
          <polygon
            points={diamond(deck.objective.x, fy(deck.objective.y), 0.45)}
            fill={DECK_COLOURS.objective}
            className="objective"
          />
        </g>
      )}
    </svg>
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
      <text x={lx} y={ly} fill={colour} className="door-label">
        {label}
      </text>
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
