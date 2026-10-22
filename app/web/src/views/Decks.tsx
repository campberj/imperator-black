import type { Content } from '../content'
import type { Deck } from '../deck'
import { BoardLegend, BoardMap } from '../components/BoardMap'
import { SeedForm } from '../components/SeedForm'
import { Table } from '../components/Table'
import { seedFrom, useGenerated } from '../generated'
import { navigate, useHashParams } from '../routes'

const capitalise = (s: string) => s[0].toUpperCase() + s.slice(1)
const inches = (v: number) => `${Number(v.toFixed(2))}"`

/** A chapter 11 deck from GET /decks, drawn like the printed deck sheets, with its rooms, doors and terrain. */
export function Decks({ content }: { content: Content }) {
  const params = useHashParams()
  const vessels = content.layouts.vessels
  const vessel = vessels.find((v) => v.id === params.get('vessel'))?.id ?? vessels[0]?.id ?? ''
  const seed = seedFrom(params)
  const { value: current, error } = useGenerated<Deck>(vessel ? `/decks/${encodeURIComponent(vessel)}?seed=${seed}` : undefined)
  const show = (v: string, s: number) => navigate('decks', { vessel: v, seed: s })
  const rooms = new Map(content.layouts.rooms.map((r) => [r.id, r]))
  const security = new Map(content.core.security.levels.map((l) => [l.id, l.name]))
  const securityName = (id: string) => security.get(id) ?? capitalise(id)

  return (
    <>
      <SeedForm label="Deck" seed={seed} onSeed={(s) => show(vessel, s)}>
        <label>
          Vessel
          <select value={vessel} onChange={(e) => show(e.target.value, seed)}>
            {vessels.map((v) => (
              <option key={v.id} value={v.id}>
                {v.name}
              </option>
            ))}
          </select>
        </label>
      </SeedForm>

      {error && (
        <p className="error" role="alert">
          Couldn't generate the deck. ({error})
        </p>
      )}
      {!current && !error && <p className="meta">Generating deck…</p>}
      {current && <DeckSheet deck={current} rooms={rooms} securityName={securityName} />}
    </>
  )
}

function DeckSheet({ deck, rooms, securityName }: {
  deck: Deck
  rooms: Map<string, { name: string; text?: string }>
  securityName: (id: string) => string
}) {
  const counts = deck.tile_counts
  return (
    <article className="deck" aria-labelledby="deck-title">
      <header>
        <h2 id="deck-title">{deck.title}</h2>
        <p className="meta">
          Imperator Black deck layout · rules v{deck.rules_version} · chapter 11 · seed {deck.seed} · {deck.rolls} d6 rolls · content v
          {deck.content_version}
        </p>
        <p>{deck.situation.trim()}</p>
      </header>

      <div className="deck-layout">
        <figure className="deck-figure">
          <BoardMap board={deck} title={deck.title} securityName={securityName} />
          <BoardLegend board={deck} />
          <figcaption className="small">
            Scale: each grid square is 3". Board {deck.board.width}" × {deck.board.height}". The blue bar on the west edge is the airlock:
            the way in and out. Spine joints are open; other links are doors.
          </figcaption>
        </figure>

        <div className="deck-tables">
          <section>
            <h3>Tiles</h3>
            <p>
              {counts.compartment ?? 0} × 6"×6", {counts.passage ?? 0} × 6"×3", {counts.junction ?? 0} × 3"×3" (junctions and small rooms).
            </p>
            <h3>Hazard</h3>
            <p>{deck.hazard || 'None'}</p>
            {deck.notes.map((n) => (
              <p key={n}>{n}</p>
            ))}
          </section>
          <Table
            title="Rooms"
            headers={['Tile', 'Room', 'Notes']}
            rows={deck.tiles
              .filter((t) => t.kind === 'compartment' || t.kind === 'small')
              .map((t) => [t.id, t.name, (t.room && rooms.get(t.room)?.text) ?? ''])}
          />
          <Table
            title="Doors"
            headers={['Door', 'Between', 'State', 'Security']}
            rows={deck.doors.map((d) => [d.id, d.note, capitalise(d.state), d.security ? securityName(d.security) : '—'])}
          />
          <Table
            title="Terrain"
            headers={['Map', 'Piece', 'Cover', 'Footprint', 'Suggested terrain']}
            rows={deck.cover.map((c) => [c.id, c.label, capitalise(c.type), `${inches(c.w)} × ${inches(c.h)}`, c.piece])}
          />
          <p className="small">
            Light cover: at least a third of the model hidden. Heavy cover: at least half hidden behind solid material. Exact pieces don't
            matter; keep the footprint and height close.
          </p>
        </div>
      </div>
    </article>
  )
}
