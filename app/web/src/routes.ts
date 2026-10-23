import { useEffect, useState } from 'react'

// Views are addressed by the URL hash (#units, #reference, #behaviors, #decks?vessel=corvette&seed=5, #land?…),
// so links and reloads work without a router.
export const ROUTES = [
  { id: 'units', label: 'Units' },
  { id: 'fireteams', label: 'Fireteams' },
  { id: 'reference', label: 'Reference' },
  { id: 'behaviors', label: 'Behavior cards' },
  { id: 'decks', label: 'Deck maps' },
  { id: 'land', label: 'Land tables' },
  { id: 'missions', label: 'Missions' },
  { id: 'dashboard', label: 'Dashboard' },
] as const

export type Route = (typeof ROUTES)[number]['id']

function parse(): { route: Route; params: URLSearchParams } {
  const [path, query = ''] = window.location.hash.slice(1).split('?', 2)
  return { route: ROUTES.find((r) => r.id === path)?.id ?? 'units', params: new URLSearchParams(query) }
}

function useHash() {
  const [hash, setHash] = useState(() => window.location.hash)
  useEffect(() => {
    const onChange = () => setHash(window.location.hash)
    window.addEventListener('hashchange', onChange)
    return () => window.removeEventListener('hashchange', onChange)
  }, [])
  return hash
}

export function useRoute(): Route {
  useHash()
  return parse().route
}

/** The query part of the hash (#decks?vessel=…&seed=…). */
export function useHashParams(): URLSearchParams {
  useHash()
  return parse().params
}

export function navigate(route: Route, params?: Record<string, string | number>) {
  const query = params ? '?' + new URLSearchParams(Object.entries(params).map(([k, v]) => [k, String(v)])).toString() : ''
  window.location.hash = route + query
}
