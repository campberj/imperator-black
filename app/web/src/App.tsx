import { useEffect, useState, type ReactNode } from 'react'
import { fetchContent, rangePenalty, type Keyword, type LoadedContent } from './content'

// Smoke test of the content pipeline: YAML -> content.json -> Imperator.Api /content -> this page.
// Columns follow the generated rulebook tables (tools/render_tables.py).

export default function App() {
  const [loaded, setLoaded] = useState<LoadedContent>()
  const [error, setError] = useState<string>()

  useEffect(() => {
    const abort = new AbortController()
    fetchContent(abort.signal)
      .then(setLoaded)
      .catch((e: unknown) => {
        if (!abort.signal.aborted) setError(e instanceof Error ? e.message : String(e))
      })
    return () => abort.abort()
  }, [])

  return (
    <main>
      <header>
        <h1>Imperator Black</h1>
        <p className="meta">{loaded ? `Content v${loaded.version}, live from /content` : error ? '' : 'Loading content…'}</p>
      </header>
      {error && (
        <p className="error" role="alert">
          Couldn't load content. Is the API running? ({error})
        </p>
      )}
      {loaded && <Tables {...loaded} />}
    </main>
  )
}

function Tables({ content }: LoadedContent) {
  const { ranged, melee, thrown } = content.weapons
  const weaponKw = new Map(content.keywords.weapon.map((k) => [k.id, k]))
  const armourKw = new Map(content.keywords.armour.map((k) => [k.id, k]))

  return (
    <>
      <Table
        title="Ranged weapons"
        headers={['Weapon', 'Damage', 'Short range', 'Beyond / double', 'AP', 'Keywords']}
        rows={ranged.map((w) => {
          const [beyond, double] = rangePenalty(w)
          return [w.name, w.damage, `${w.short_range}"`, `${beyond} / ${double}`, w.ap, <Keywords key="kw" ids={w.keywords} defs={weaponKw} rate={w.auto} />]
        })}
      />
      <Table
        title="Melee weapons"
        headers={['Melee weapon', 'Damage', 'AP']}
        rows={melee.map((w) => [w.name, w.damage, w.ap])}
      />
      <Table
        title="Grenades"
        headers={['Grenade', 'Throw range', 'Radius', 'Damage / effect']}
        rows={thrown.map((g) => [
          g.name,
          `${g.throw_range}"`,
          `${g.blast_radius}"`,
          g.damage ? `${g.damage} (AP ${g.ap ?? 0})` : (g.effect ?? ''),
        ])}
      />
      <Table
        title="Armour"
        headers={['Armour', 'Value', 'Keywords']}
        rows={content.armour.armour.map((a) => [a.name, a.value, <Keywords key="kw" ids={a.keywords} defs={armourKw} />])}
      />
    </>
  )
}

function Table({ title, headers, rows }: { title: string; headers: string[]; rows: ReactNode[][] }) {
  return (
    <section>
      <h2>{title}</h2>
      <div className="scroll">
        <table>
          <thead>
            <tr>
              {headers.map((h) => (
                <th key={h} scope="col">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((cells, r) => (
              <tr key={r}>
                {cells.map((c, i) => (i === 0 ? <th key={i} scope="row">{c}</th> : <td key={i}>{c}</td>))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}

/**
 * Keyword names, with the rules text on hover. As in the rulebook renderer, an Auto weapon shows its rate
 * ("Auto 3") and unknown ids fall back to the id.
 */
function Keywords({ ids, defs, rate }: { ids: string[]; defs: Map<string, Keyword>; rate?: number }) {
  if (ids.length === 0) return <>—</>
  return (
    <>
      {ids.map((id, i) => (
        <span key={id}>
          {i > 0 && ', '}
          <abbr title={defs.get(id)?.text}>{id === 'auto' && rate ? `Auto ${rate}` : (defs.get(id)?.name ?? id)}</abbr>
        </span>
      ))}
    </>
  )
}
