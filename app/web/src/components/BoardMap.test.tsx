import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, expect, test } from 'vitest'
import type { Board } from '../board'
import type { Deck } from '../deck'
import { BOARD_COLOURS, COVER_COLOURS, COVER_NAMES, COVER_ORDER, DOOR_COLOURS } from '../boardStyle'
import { BoardLegend, BoardMap } from './BoardMap'
// GET /decks/freighter?seed=7: open, closed and locked doors and an objective.
import deckFixture from '../fixtures/deck-freighter-7.json'
// The reference generator's example missions (docs/scenarios/missions/*.yaml).
import { landExample, shipExample } from '../test-missions'

afterEach(cleanup)

const deck = deckFixture as unknown as Deck
const land = landExample
const shipMission = shipExample
const securityName = (id: string) => ({ basic: 'Basic', secured: 'Secured', hardened: 'Hardened', military: 'Military' })[id] ?? id
const draw = (board: Board, title = 'Test board') =>
  render(<BoardMap board={board} title={title} securityName={securityName} troopName={(id) => `Troop ${id}`} />).container.querySelector('svg')!
const num = (el: Element, attr: string) => Number(el.getAttribute(attr))

// ---------- decks

test('draws the board to scale in inches, with a margin', () => {
  const svg = draw(deck, deck.title)
  // 1.5" margin on every side.
  expect(svg.getAttribute('viewBox')).toBe(`-1.5 -1.5 ${deck.board.width + 3} ${deck.board.height + 3}`)
  expect(screen.getByRole('img', { name: `${deck.title}: map, ${deck.board.width}" by ${deck.board.height}"` })).toBeTruthy()
})

test('every tile is placed with y flipped (north up) and labelled with its id, name and size', () => {
  const svg = draw(deck)
  for (const t of deck.tiles) {
    const g = svg.querySelector(`[data-tile="${t.id}"]`)!
    const r = g.querySelector('rect')!
    expect([num(r, 'x'), num(r, 'y'), num(r, 'width'), num(r, 'height')]).toEqual([t.x, deck.board.height - t.y - t.h, t.w, t.h])
    // Interior 3" grid lines.
    expect(g.querySelectorAll('line')).toHaveLength(t.w / 3 - 1 + (t.h / 3 - 1))
  }
  const labels = [...svg.querySelectorAll('.tile-label')].map((l) => l.textContent)
  for (const t of deck.tiles) expect(labels).toContain(`${t.id}${t.name}${t.size} tile`)
  // Aboard ship there is no land grid or sectors.
  expect(svg.querySelector('[data-grid]')).toBeNull()
})

test('doors are coloured by state; locked doors are marked (L) and name their security', () => {
  const svg = draw(deck)
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

test('cover pieces carry their ids; the airlock and the deck objective are drawn', () => {
  const svg = draw(deck)
  for (const c of deck.cover) {
    expect(svg.querySelector(`[data-cover="${c.id}"] rect`)!.getAttribute('fill')).toBe(COVER_COLOURS[c.type])
    expect(svg.querySelector(`[data-cover-id="${c.id}"]`)!.textContent).toBe(c.id)
  }
  const entry = svg.querySelector('[data-entry] line')!
  expect(entry.getAttribute('stroke')).toBe(BOARD_COLOURS.player)
  expect(num(entry, 'x1')).toBe(deck.entry.x)
  // A deck's single objective is not numbered.
  expect(svg.querySelector('[data-objective="1"] title')!.textContent).toBe(`Objective: ${deck.objective!.label}`)
  expect(svg.querySelector('[data-objective="1"] text')).toBeNull()
})

// ---------- land tables

test('land boards get a 6" grid, 12" sector lines and sector names from A1 at the bottom left', () => {
  const svg = draw(land)
  const { width: W, height: H } = land.board
  expect(svg.querySelectorAll('[data-grid] line')).toHaveLength(W / 6 - 1 + (H / 6 - 1))
  expect(svg.querySelectorAll('[data-sectors] line')).toHaveLength(W / 12 - 1 + (H / 12 - 1))
  for (const s of land.sectors!) {
    const t = svg.querySelector(`[data-sector="${s.name}"]`)!
    expect(t.textContent).toContain(s.name)
    const col = 'ABCDE'.indexOf(s.name[0])
    const row = Number(s.name.slice(1)) - 1
    expect([num(t, 'x'), num(t, 'y')]).toEqual([col * 12 + 0.3, H - (row * 12 + 11)])
  }
  // A1 is at the player's bottom left: its name sits low on the SVG (y near the bottom).
  expect(num(svg.querySelector('[data-sector="A1"]')!, 'y')).toBeGreaterThan(H - 12)
})

test('terrain is coloured by type and layered raised ground first, solid cover last', () => {
  const svg = draw(land)
  const drawn = [...svg.querySelectorAll('[data-cover]')]
  expect(drawn).toHaveLength(land.cover!.length)
  for (const g of drawn) expect(g.querySelector('rect')!.getAttribute('fill')).toBe(COVER_COLOURS[g.getAttribute('data-type') as keyof typeof COVER_COLOURS])
  const order = drawn.map((g) => COVER_ORDER[g.getAttribute('data-type') as keyof typeof COVER_ORDER])
  expect(order).toEqual([...order].sort((a, b) => a - b))
})

test('deployment, guard models, contact chips, objectives and the enemy entry are drawn', () => {
  const svg = draw(land)
  const H = land.board.height
  const deploy = svg.querySelector('[data-deploy]')!
  expect(num(deploy, 'y')).toBeCloseTo(H - (land.deploy![0].y + land.deploy![0].h))
  expect(screen.getByText(`Deploy: ${land.deploy![0].label}`)).toBeTruthy()

  for (const e of land.enemies!) {
    const g = svg.querySelector(`[data-enemy="${e.n}"]`)!
    expect([num(g.querySelector('circle')!, 'cx'), num(g.querySelector('circle')!, 'cy')]).toEqual([e.x, H - e.y])
    expect(g.querySelector('text')!.textContent).toBe(String(e.n))
    expect(g.querySelector('title')!.textContent).toBe(`${e.n}: Troop ${e.troop}, ${e.state}`)
  }
  for (const c of land.contacts!) {
    const g = svg.querySelector(`[data-contact="${c.n}"]`)!
    expect(g.querySelector('text')!.textContent).toBe(`?${c.n}`)
    expect(g.querySelector('title')!.textContent).toBe(`Contact ?${c.n} (${c.where})`)
  }
  const ee = svg.querySelector('[data-enemy-entry]')!
  expect(ee.querySelector('text')!.textContent).toBe('EE')
  expect(ee.querySelector('title')!.textContent).toBe(land.enemy_entry!.label)
  expect(svg.querySelectorAll('[data-objective]')).toHaveLength(land.objectives!.length)
})

test('several objectives are numbered', () => {
  const sabotage: Board = { ...land, objectives: [land.objectives![0], { ...land.objectives![0], n: 2, x: 6, label: 'Sector A3' }] }
  const svg = draw(sabotage)
  expect(svg.querySelector('[data-objective="1"] text')!.textContent).toBe('1')
  expect(svg.querySelector('[data-objective="2"] title')!.textContent).toBe('Objective 2: Sector A3')
})

test('mission markers sit on a deck as well', () => {
  const svg = draw(shipMission)
  expect(svg.querySelectorAll('[data-tile]')).toHaveLength(shipMission.tiles!.length)
  expect(svg.querySelectorAll('[data-enemy]')).toHaveLength(shipMission.enemies!.length)
  expect(svg.querySelectorAll('[data-contact]')).toHaveLength(shipMission.contacts!.length)
  expect(svg.querySelector('[data-enemy-entry]')).toBeTruthy()
  expect(svg.querySelector('[data-grid]')).toBeNull()
})

// ---------- legend

test('the legend lists what the board shows, with the printed names', () => {
  render(<BoardLegend board={land} />)
  const items = within(screen.getByRole('list', { name: 'Legend' })).getAllByRole('listitem').map((li) => li.textContent)
  const types = [...new Set(land.cover!.map((c) => c.type))].sort((a, b) => COVER_ORDER[a] - COVER_ORDER[b])
  expect(items).toEqual([...types.map((t) => COVER_NAMES[t]), 'Enemy model', 'Deployment', 'Objective', 'Contact chip (?n)', 'Enemy entry (EE)'])
  cleanup()

  render(<BoardLegend board={deck} />)
  const deckItems = within(screen.getByRole('list', { name: 'Legend' })).getAllByRole('listitem').map((li) => li.textContent)
  expect(deckItems).toEqual(['Open door', 'Closed door', 'Locked door (L)', 'Light cover', 'Heavy cover', 'Airlock', 'Objective'])
})
