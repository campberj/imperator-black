import { act, cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import type { Deck } from '../deck'
import { content } from '../test-content'
import { Decks } from './Decks'
import fixture from '../fixtures/deck-freighter-7.json'

const deck = fixture as unknown as Deck

// The API stand-in returns the fixture with whatever vessel and seed were asked for.
beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn(async (url: string) => {
    const m = /^\/decks\/(\w+)\?seed=(\d+)$/.exec(url)
    if (!m) return new Response('', { status: 404 })
    if (m[1] === 'nowhere') return Response.json({ detail: "Unknown vessel 'nowhere'." }, { status: 404 })
    return Response.json({ ...deck, vessel: m[1], seed: Number(m[2]) })
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

test('defaults to the first vessel, seed 1, and shows the deck sheet', async () => {
  render(<Decks content={content} />)
  expect(await screen.findByRole('heading', { name: deck.title, level: 2 })).toBeTruthy()
  expect(fetch).toHaveBeenCalledWith(`/decks/${content.layouts.vessels[0].id}?seed=1`, expect.anything())
  expect(screen.getByText(new RegExp(`chapter 11 · seed 1 · ${deck.rolls} d6 rolls`))).toBeTruthy()
  expect(screen.getByRole('img', { name: new RegExp(deck.title) })).toBeTruthy()
})

test('reads the vessel and seed from the hash, and the picker and seed box write them back', async () => {
  window.location.hash = '#decks?vessel=corvette&seed=5'
  render(<Decks content={content} />)
  await screen.findByRole('heading', { name: deck.title, level: 2 })
  expect(fetch).toHaveBeenLastCalledWith('/decks/corvette?seed=5', expect.anything())

  const picker = screen.getByLabelText('Vessel') as HTMLSelectElement
  expect([...picker.options].map((o) => o.value)).toEqual(content.layouts.vessels.map((v) => v.id))
  fireEvent.change(picker, { target: { value: 'station_module' } })
  expect(window.location.hash).toBe('#decks?vessel=station_module&seed=5')

  hashChange('#decks?vessel=station_module&seed=5')
  fireEvent.change(screen.getByLabelText('Seed'), { target: { value: '42' } })
  fireEvent.click(screen.getByRole('button', { name: 'Show deck' }))
  expect(window.location.hash).toBe('#decks?vessel=station_module&seed=42')

  hashChange('#decks?vessel=station_module&seed=42')
  await screen.findByText(/seed 42 ·/)
  expect(fetch).toHaveBeenLastCalledWith('/decks/station_module?seed=42', expect.anything())
  fireEvent.click(screen.getByRole('button', { name: 'Next seed' }))
  expect(window.location.hash).toBe('#decks?vessel=station_module&seed=43')
})

test('rooms, doors and terrain tables list every room tile, door and cover piece', async () => {
  render(<Decks content={content} />)
  await screen.findByRole('heading', { name: deck.title, level: 2 })
  const table = (title: string) => within(screen.getByRole('heading', { name: title }).closest('section')!)

  const rooms = deck.tiles.filter((t) => t.kind === 'compartment' || t.kind === 'small')
  expect(table('Rooms').getAllByRole('row')).toHaveLength(rooms.length + 1)
  for (const t of rooms) {
    const text = content.layouts.rooms.find((r) => r.id === t.room)?.text
    if (text) expect(table('Rooms').getByText(text)).toBeTruthy()
  }

  expect(table('Doors').getAllByRole('row')).toHaveLength(deck.doors.length + 1)
  const locked = deck.doors.find((d) => d.state === 'locked')!
  const row = table('Doors').getByRole('rowheader', { name: locked.id }).closest('tr')!
  const level = content.core.security.levels.find((l) => l.id === locked.security)!
  expect(row.textContent).toBe(`${locked.id}${locked.note}Locked${level.name}`)

  expect(table('Terrain').getAllByRole('row')).toHaveLength(deck.cover.length + 1)
  const c = deck.cover[0]
  expect(table('Terrain').getByRole('rowheader', { name: c.id }).closest('tr')!.textContent).toContain(`${c.w}" × ${c.h}"`)

  expect(screen.getByText(new RegExp(`^${deck.tile_counts.compartment} × 6"×6"`))).toBeTruthy()
})

test('a bad seed in the hash falls back to seed 1', async () => {
  window.location.hash = '#decks?vessel=freighter&seed=-3'
  render(<Decks content={content} />)
  await screen.findByRole('heading', { name: deck.title, level: 2 })
  expect(fetch).toHaveBeenLastCalledWith('/decks/freighter?seed=1', expect.anything())
  expect((screen.getByRole('button', { name: 'Previous seed' }) as HTMLButtonElement).disabled).toBe(false)
})

test('shows the API error when a deck cannot be generated', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => Response.json({ detail: 'A seed is required.' }, { status: 400 })))
  render(<Decks content={content} />)
  expect((await screen.findByRole('alert')).textContent).toContain('A seed is required.')
})
