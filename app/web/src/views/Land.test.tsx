import { act, cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import type { LandTable } from '../land'
import { content } from '../test-content'
import { Land } from './Land'
// GET /land/woodland?size=large&seed=59: two density-check fills, night conditions.
import fixture from '../fixtures/land-woodland-large-59.json'

const table = fixture as unknown as LandTable

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn(async (url: string) => {
    const m = /^\/land\/(\w+)\?size=(\w+)&seed=(\d+)$/.exec(url)
    if (!m) return new Response('', { status: 404 })
    return Response.json({ ...table, environment: m[1], size: m[2], seed: Number(m[3]) })
  }))
})

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
  window.location.hash = ''
})

const hashChange = (hash: string) =>
  act(() => {
    window.location.hash = hash
    window.dispatchEvent(new HashChangeEvent('hashchange'))
  })

test('defaults to the first environment, small, seed 1', async () => {
  render(<Land content={content} />)
  await screen.findByRole('heading', { name: table.title, level: 2 })
  expect(fetch).toHaveBeenCalledWith(`/land/${content.terrain.environments[0].id}?size=small&seed=1`, expect.anything())
})

test('the environment, size and seed live in the hash', async () => {
  window.location.hash = '#land?environment=woodland&size=large&seed=59'
  render(<Land content={content} />)
  await screen.findByRole('heading', { name: table.title, level: 2 })
  expect(fetch).toHaveBeenLastCalledWith('/land/woodland?size=large&seed=59', expect.anything())

  const env = screen.getByLabelText('Environment') as HTMLSelectElement
  // Every environment can be named, Fortified included (it is never rolled).
  expect([...env.options].map((o) => o.value)).toEqual(content.terrain.environments.map((e) => e.id))
  fireEvent.change(env, { target: { value: 'fortified' } })
  expect(window.location.hash).toBe('#land?environment=fortified&size=large&seed=59')

  hashChange('#land?environment=fortified&size=large&seed=59')
  fireEvent.change(screen.getByLabelText('Size'), { target: { value: 'small' } })
  expect(window.location.hash).toBe('#land?environment=fortified&size=small&seed=59')
  hashChange('#land?environment=fortified&size=small&seed=59')
  fireEvent.click(screen.getByRole('button', { name: 'Next seed' }))
  expect(window.location.hash).toBe('#land?environment=fortified&size=small&seed=60')
})

test('shows the map, conditions, sectors and terrain', async () => {
  window.location.hash = '#land?environment=woodland&size=large&seed=59'
  render(<Land content={content} />)
  await screen.findByRole('heading', { name: table.title, level: 2 })

  expect(screen.getByRole('img', { name: new RegExp(table.title) })).toBeTruthy()
  const night = content.terrain.conditions.find((c) => c.id === table.conditions)!
  expect(screen.getByText(`Conditions: ${night.name}.`)).toBeTruthy()

  const section = (title: string) => within(screen.getByRole('heading', { name: title }).closest('section')!)
  expect(section('Sectors').getAllByRole('row')).toHaveLength(table.sectors.length + 1)
  // The density check's fills are marked in the sector text.
  for (const s of table.sectors.filter((x) => x.text.endsWith('(density check)')))
    expect(section('Sectors').getByRole('rowheader', { name: s.name }).closest('tr')!.textContent).toContain('(density check)')

  expect(section('Terrain').getAllByRole('row')).toHaveLength(table.cover.length + 1)
  const c = table.cover[0]
  const type = content.terrain.types.find((x) => x.id === c.terrain)!.name
  expect(section('Terrain').getByRole('rowheader', { name: c.id }).closest('tr')!.textContent).toBe(`${c.id}${c.label}${type}${c.w}" × ${c.h}"${c.sector}`)

  expect(screen.getByText(`Imperator Black land table · rules v${table.rules_version} · chapter 12 · seed 59 · 17 d6 rolls · content v${table.content_version}`)).toBeTruthy()
})

test('shows the API error', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => Response.json({ detail: "Unknown size 'huge'." }, { status: 400 })))
  render(<Land content={content} />)
  expect((await screen.findByRole('alert')).textContent).toContain("Unknown size 'huge'.")
})
