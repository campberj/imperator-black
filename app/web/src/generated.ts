import { useEffect, useState } from 'react'

// Shared by the views of generated boards (decks, land tables, missions): seeds from the URL hash, fetching from the
// API with its problem details as errors, and a result keyed by the request so a stale one never shows.

export const MAX_SEED = 2_147_483_647

/** The seed in the hash, or 1 when it is missing or not a whole number from 0 to MAX_SEED. */
export function seedFrom(params: URLSearchParams): number {
  const asked = Number(params.get('seed'))
  return params.has('seed') && Number.isInteger(asked) && asked >= 0 && asked <= MAX_SEED ? asked : 1
}

export async function fetchJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(url, { signal })
  if (!response.ok) {
    const problem = await response.json().catch(() => undefined)
    throw new Error(problem?.detail ?? `GET ${url} failed: ${response.status} ${response.statusText}`)
  }
  return (await response.json()) as T
}

/** Fetches url whenever it changes; returns the result for this url only. */
export function useGenerated<T>(url: string | undefined): { value?: T; error?: string } {
  const [result, setResult] = useState<{ url: string; value?: T; error?: string }>()
  useEffect(() => {
    if (!url) return
    const abort = new AbortController()
    fetchJson<T>(url, abort.signal)
      .then((value) => setResult({ url, value }))
      .catch((e: unknown) => {
        if (!abort.signal.aborted) setResult({ url, error: e instanceof Error ? e.message : String(e) })
      })
    return () => abort.abort()
  }, [url])
  return result && result.url === url ? { value: result.value, error: result.error } : {}
}
