import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, expect, test } from 'vitest'
import type { Deck } from '../deck'
import { COVER_COLOURS, DECK_COLOURS, DOOR_COLOURS } from '../deckStyle'
import { DeckMap } from './DeckMap'
// A deck from GET /decks/freighter?seed=7; it has open, closed and locked doors and an objective.
import fixture from '../fixtures/deck-freighter-7.json'

afterEach(cleanup)

const deck = fixture as unknown as Deck
const securityName = (id: string) => ({ basic: 'Basic', secured: 'Secured', hardened: 'Hardened', military: 'Military' })[id] ?? id
const draw = () => render(<DeckMap deck={deck} securityName={securityName} />).container.querySelector('svg')!

test('draws the board to scale in inches, with a margin', () => {
  const svg = draw()
  // 1.5" margin on every side.
  expect(svg.getAttribute('viewBox')).toBe(`-1.5 -1.5 ${deck.board.width + 3} ${deck.board.height + 3}`)
  expect(screen.getByRole('img', { name: `${deck.title}: map, ${deck.board.width}" by ${deck.board.height}"` })).toBeTruthy()
})

test('every tile is placed with y flipped (north up) and labelled with its id, name and size', () => {
  const svg = draw()
  for (const t of deck.tiles) {
    const g = svg.querySelector(`[data-tile="${t.id}"]`)!
    const r = g.querySelector('rect')!
    expect([r.getAttribute('x'), r.getAttribute('y'), r.getAttribute('width'), r.getAttribute('height')].map(Number))
      .toEqual([t.x, deck.board.height - t.y - t.h, t.w, t.h])
    // Interior 3" grid lines.
    expect(g.querySelectorAll('line')).toHaveLength(t.w / 3 - 1 + (t.h / 3 - 1))
  }
  const labels = [...svg.querySelectorAll('.tile-label')].map((l) => l.textContent)
  for (const t of deck.tiles) expect(labels).toContain(`${t.id}${t.name}${t.size} tile`)
})

test('doors are coloured by state; locked doors are marked (L) and name their security', () => {
  const svg = draw()
  for (const d of deck.doors) {
    const g = svg.querySelector(`[data-door="${d.id}"]`)!
    expect(g.querySelector('line')!.getAttribute('stroke')).toBe(DOOR_COLOURS[d.state])
    expect(g.querySelector('text')!.textContent).toBe(d.state === 'locked' ? `${d.id} (L)` : d.id)
    const title = g.querySelector('title')!.textContent!
    expect(title).toContain(d.note)
    if (d.security) expect(title).toContain(`${securityName(d.security)} security`)
  }
  expect(new Set(deck.doors.map((d) => d.state))).toEqual(new Set(['open', 'closed', 'locked']))
})

test('cover pieces carry their ids; the airlock and objective are drawn', () => {
  const svg = draw()
  for (const c of deck.cover) {
    const g = svg.querySelector(`[data-cover="${c.id}"]`)!
    expect(g.querySelector('rect')!.getAttribute('fill')).toBe(COVER_COLOURS[c.type])
    expect(svg.querySelector(`[data-cover-id="${c.id}"]`)!.textContent).toBe(c.id)
  }
  // Heavy cover is drawn after light, so it sits on top where they meet.
  const order = [...svg.querySelectorAll('[data-cover]')].map((g) => deck.cover.find((c) => c.id === g.getAttribute('data-cover'))!.type)
  expect(order.indexOf('heavy')).toBeGreaterThan(order.lastIndexOf('light'))

  const entry = svg.querySelector('[data-entry] line')!
  expect(entry.getAttribute('stroke')).toBe(DECK_COLOURS.airlock)
  expect(Number(entry.getAttribute('x1'))).toBe(deck.entry.x)
  expect(svg.querySelector('[data-objective] title')!.textContent).toBe(`Objective: ${deck.objective!.label}`)
})
