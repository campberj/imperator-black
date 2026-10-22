import type { Content } from '../content'
import type { LandTable } from '../land'
import { BoardLegend, BoardMap } from '../components/BoardMap'
import { SeedForm } from '../components/SeedForm'
import { Table } from '../components/Table'
import { seedFrom, useGenerated } from '../generated'
import { navigate, useHashParams } from '../routes'

const SIZES = [
  { id: 'small', name: 'Small, 36" × 36"' },
  { id: 'large', name: 'Large, 48" × 48"' },
] as const
const inches = (v: number) => `${Number(v.toFixed(2))}"`

/** A chapter 12 land table from GET /land, drawn with the board map, with its sectors and terrain. */
export function Land({ content }: { content: Content }) {
  const params = useHashParams()
  const environments = content.terrain.environments
  const environment = environments.find((e) => e.id === params.get('environment'))?.id ?? environments[0]?.id ?? ''
  const size = SIZES.find((s) => s.id === params.get('size'))?.id ?? 'small'
  const seed = seedFrom(params)
  const { value: table, error } = useGenerated<LandTable>(
    environment ? `/land/${encodeURIComponent(environment)}?size=${size}&seed=${seed}` : undefined,
  )
  const show = (e: string, z: string, s: number) => navigate('land', { environment: e, size: z, seed: s })

  return (
    <>
      <SeedForm label="Table" seed={seed} onSeed={(s) => show(environment, size, s)}>
        <label>
          Environment
          <select value={environment} onChange={(e) => show(e.target.value, size, seed)}>
            {environments.map((e) => (
              <option key={e.id} value={e.id}>
                {e.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Size
          <select value={size} onChange={(e) => show(environment, e.target.value, seed)}>
            {SIZES.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name}
              </option>
            ))}
          </select>
        </label>
      </SeedForm>

      {error && (
        <p className="error" role="alert">
          Couldn't generate the table. ({error})
        </p>
      )}
      {!table && !error && <p className="meta">Generating table…</p>}
      {table && <LandSheet table={table} content={content} />}
    </>
  )
}

function LandSheet({ table: t, content }: { table: LandTable; content: Content }) {
  const conditions = content.terrain.conditions.find((c) => c.id === t.conditions)
  const types = new Map(content.terrain.types.map((x) => [x.id, x.name]))
  const cols = t.board.width / 12
  return (
    <article className="deck" aria-labelledby="land-title">
      <header>
        <h2 id="land-title">{t.title}</h2>
        <p className="meta">
          Imperator Black land table · rules v{t.rules_version} · chapter 12 · seed {t.seed} · {t.rolls.filter((r) => !r.startsWith('Density')).length} d6
          rolls · content v{t.content_version}
        </p>
        {conditions && (
          <p>
            <b>Conditions: {conditions.name}.</b> {conditions.text}
          </p>
        )}
      </header>

      <div className="deck-layout">
        <figure className="deck-figure">
          <BoardMap board={t} title={t.title} />
          <BoardLegend board={t} />
          <figcaption className="small">
            Scale: each grid square is 6"; sectors are 12". Board {t.board.width}" × {t.board.height}" ({cols} × {cols} sectors). The player's
            edge is at the bottom: sector A1 is the player's bottom left.
          </figcaption>
        </figure>

        <div className="deck-tables">
          <Table title="Sectors" headers={['Sector', 'Ground']} rows={t.sectors.map((s) => [s.name, s.text])} />
          <Table
            title="Terrain"
            headers={['Map', 'Piece', 'Terrain', 'Footprint', 'Sector']}
            rows={t.cover.map((c) => [c.id, c.label, types.get(c.terrain) ?? c.terrain, `${inches(c.w)} × ${inches(c.h)}`, c.sector])}
          />
          <details className="rolls">
            <summary>Rolls</summary>
            <ol>
              {t.rolls.map((r, i) => (
                <li key={i}>{r}</li>
              ))}
            </ol>
          </details>
        </div>
      </div>
    </article>
  )
}
