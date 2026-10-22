import type { ReactNode } from 'react'
import type { Lookup, RangedWeapon } from '../content'
import { keywordLabel } from '../format'

/** A titled table; the first cell of each row is its row header. */
export function Table({ title, headers, rows }: { title: string; headers: string[]; rows: ReactNode[][] }) {
  return (
    <section>
      <h2>{title}</h2>
      <div className="scroll">
        <table className="data">
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

/** Keyword names, with the rules text on hover. As in the rulebook renderer, an Auto weapon shows its rate. */
export function Keywords({ ids, lookup, weapon }: { ids: string[]; lookup: Lookup; weapon?: RangedWeapon }) {
  if (ids.length === 0) return <>—</>
  return (
    <>
      {ids.map((id, i) => (
        <span key={id}>
          {i > 0 && ', '}
          <abbr title={lookup.keywords.get(id)?.text}>{keywordLabel(id, lookup, weapon)}</abbr>
        </span>
      ))}
    </>
  )
}
