import { useEffect, useState } from 'react'

// Views are addressed by the URL hash (#units, #reference, #behaviors), so links and reloads work without a router.
export const ROUTES = [
  { id: 'units', label: 'Units' },
  { id: 'reference', label: 'Reference' },
  { id: 'behaviors', label: 'Behavior cards' },
] as const

export type Route = (typeof ROUTES)[number]['id']

function current(): Route {
  const hash = window.location.hash.slice(1)
  return ROUTES.find((r) => r.id === hash)?.id ?? 'units'
}

export function useRoute(): Route {
  const [route, setRoute] = useState(current)
  useEffect(() => {
    const onChange = () => setRoute(current())
    window.addEventListener('hashchange', onChange)
    return () => window.removeEventListener('hashchange', onChange)
  }, [])
  return route
}
