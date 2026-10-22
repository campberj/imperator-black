import { useEffect, useMemo, useState } from 'react'
import { fetchContent, indexContent, type LoadedContent } from './content'
import { ROUTES, useRoute } from './routes'
import { Behaviors } from './views/Behaviors'
import { Decks } from './views/Decks'
import { Land } from './views/Land'
import { Missions } from './views/Missions'
import { Reference } from './views/Reference'
import { Units } from './views/Units'

// The unit database and reference: everything on these pages is read from /content
// (content/*.yaml compiled to content.json and served by Imperator.Api).

export default function App() {
  const [loaded, setLoaded] = useState<LoadedContent>()
  const [error, setError] = useState<string>()
  const route = useRoute()
  const lookup = useMemo(() => loaded && indexContent(loaded.content), [loaded])

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
      <header className="top">
        <div>
          <h1>Imperator Black</h1>
          <p className="meta">{loaded ? `Content v${loaded.version}, live from /content` : error ? '' : 'Loading content…'}</p>
        </div>
        <nav aria-label="Sections">
          {ROUTES.map((r) => (
            <a key={r.id} href={`#${r.id}`} aria-current={route === r.id ? 'page' : undefined}>
              {r.label}
            </a>
          ))}
        </nav>
      </header>
      {error && (
        <p className="error" role="alert">
          Couldn't load content. Is the API running? ({error})
        </p>
      )}
      {loaded && lookup && (
        <>
          {route === 'units' && <Units content={loaded.content} lookup={lookup} />}
          {route === 'reference' && <Reference content={loaded.content} lookup={lookup} />}
          {route === 'behaviors' && <Behaviors content={loaded.content} lookup={lookup} />}
          {route === 'decks' && <Decks content={loaded.content} />}
          {route === 'land' && <Land content={loaded.content} />}
          {route === 'missions' && <Missions content={loaded.content} lookup={lookup} />}
        </>
      )}
    </main>
  )
}
