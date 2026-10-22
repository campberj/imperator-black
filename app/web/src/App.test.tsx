import { act, cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, expect, test, vi } from 'vitest'
import App from './App'
// The real compiled content, so the test fails if the tables stop matching it.
import content from '../../../content/dist/content.json'

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
  window.location.hash = ''
})

function serveContent() {
  vi.stubGlobal('fetch', vi.fn(async () =>
    new Response(JSON.stringify(content), {
      headers: { 'Content-Type': 'application/json', 'Imperator-Content-Version': content.core.version },
    }),
  ))
}

const row = (name: string) => screen.getByRole('rowheader', { name }).closest('tr')!

test('renders weapon and armour tables from /content', async () => {
  serveContent()
  window.location.hash = '#reference'
  render(<App />)

  expect(await screen.findByText(`Content v${content.core.version}, live from /content`)).toBeTruthy()
  expect(fetch).toHaveBeenCalledWith('/content', expect.anything())

  for (const title of ['Ranged weapons', 'Melee weapons', 'Grenades', 'Armour'])
    expect(screen.getByRole('heading', { name: title })).toBeTruthy()

  for (const w of content.weapons.ranged) expect(row(w.name)).toBeTruthy()
  for (const a of content.armour.armour) expect(row(a.name)).toBeTruthy()

  // Same formatting as the generated rulebook tables (tools/render_tables.py).
  expect(within(row('Shotgun')).getByText('-2 / -3')).toBeTruthy()
  expect(within(row('Rifle')).getByText('-1 / -2')).toBeTruthy()
  expect(within(row('Rifle')).getByText('—')).toBeTruthy()
  expect(row('Compact LMG').textContent).toContain('Handy, Auto 3, Suppressing')
  expect(row('SMG').textContent).toContain('Handy, Auto 2')
  expect(within(row('Frag grenade')).getByText('2d8 (AP 0)')).toBeTruthy()
  expect(row('Flashbang').textContent).toContain('+2 Suppression')
  expect(row('Heavy plate').textContent).toContain('Sealed, Heavy')

  // Keyword rules text is on hover.
  const auto = content.keywords.weapon.find((k) => k.id === 'auto')!
  expect(screen.getAllByTitle(auto.text)).toHaveLength(content.weapons.ranged.filter((w) => w.keywords.includes('auto')).length)
})

test('opens on the unit database and navigates by hash', async () => {
  serveContent()
  render(<App />)

  expect(await screen.findByRole('heading', { name: content.troops.troops[0].name, level: 3 })).toBeTruthy()
  expect(screen.getByRole('link', { name: 'Units' }).getAttribute('aria-current')).toBe('page')

  act(() => {
    window.location.hash = '#behaviors'
    window.dispatchEvent(new HashChangeEvent('hashchange'))
  })
  expect(await screen.findByRole('heading', { name: 'Enemy awareness' })).toBeTruthy()
  expect(screen.getByRole('link', { name: 'Behavior cards' }).getAttribute('aria-current')).toBe('page')
})

test('shows an error when the API is unavailable', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => new Response('', { status: 503, statusText: 'Service Unavailable' })))
  render(<App />)

  expect((await screen.findByRole('alert')).textContent).toContain('503')
})
