import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import { content, lookup } from '../test-content'
import { landExample, shipExample } from '../test-missions'
import { Dashboard } from './Dashboard'
import { Missions } from './Missions'

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn(async (url: string) => {
    const q = new URLSearchParams(url.split('?')[1])
    return Response.json({ ...(q.get('setting') === 'land' ? landExample : shipExample), seed: Number(q.get('seed')) })
  }))
})

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
  window.location.hash = ''
})

const boxes = (name: string) => within(screen.getByRole('table', { name })).getAllByRole('row')[0].querySelectorAll('td')
const marked = (name: string, mark: string) => [...boxes(name)].filter((td) => td.dataset.mark === mark).map((td) => td.textContent)
const section = (title: string) => within(screen.getByRole('heading', { name: title }).closest('section')!)
const bodyRows = (title: string) => section(title).getAllByRole('row').slice(1)

// Done when: the dashboard renders for both example missions, with the clock boxes, check-ins and contact rows.
test.each([
  ['mission-ship-42', shipExample],
  ['mission-land-11', landExample],
])('fills in the dashboard for the example %s', async (id, m) => {
  window.location.hash = `#dashboard?example=${id}`
  render(<Dashboard content={content} />)
  expect(await screen.findByText(m.title)).toBeTruthy()

  // Alert track 0–max: the start shaded, the thresholds marked and named.
  expect(boxes('Alert track')).toHaveLength(content.missions.alert.max + 1)
  expect(marked('Alert track', 'start')).toEqual([String(m.alert_start)])
  expect(marked('Alert track', 'threshold')).toEqual(content.missions.alert.thresholds.map((t) => String(t.at)))
  expect(screen.getByText('Lockdown')).toBeTruthy()

  // One box a minute; check-ins before the limit shaded, the limit grey.
  expect(screen.getByRole('heading', { name: `Mission clock: ${m.clock.limit} minutes, clock die ${m.clock.die}` })).toBeTruthy()
  expect(boxes('Mission clock')).toHaveLength(m.clock.limit)
  expect(marked('Mission clock', 'check-in')).toEqual(['10', '15'])
  expect(marked('Mission clock', 'limit')).toEqual([String(m.clock.limit)])
  expect(boxes('Rounds played')).toHaveLength(15)

  // Objectives, contacts (one row a chip, at least six), the guard and the reinforcement.
  const type = content.missions.missions.find((x) => x.id === m.mission)!
  expect(bodyRows(`Objectives: ${type.name}`).map((r) => r.textContent)).toEqual(m.objectives.map((o) => `${o.n}${o.label}□ taken  □ destroyed  □ extracted`))
  const contacts = bodyRows('Contacts')
  expect(contacts).toHaveLength(Math.max(6, m.contacts.length))
  m.contacts.forEach((c, i) => expect(contacts[i].textContent).toBe(`${c.n}${c.where}`))
  expect(contacts.slice(m.contacts.length).every((r) => r.textContent === '')).toBe(true)
  const elements = new Map(content.factions.elements.map((e) => [e.id, e.name]))
  expect(bodyRows('Enemy elements').map((r) => r.textContent).filter(Boolean)).toEqual([
    `${elements.get(m.enemies[0].element)} (guard)U  A  E`,
    `${elements.get(m.reinforcement)} (reinf.)U  A  E`,
  ])

  // A small mission has 8 player rows.
  expect(bodyRows('Player force')).toHaveLength(8)
})

test('the blank dashboard: 30-minute clock with both sizes named', () => {
  window.location.hash = '#dashboard'
  render(<Dashboard content={content} />)
  expect(screen.getByText('Mission: ________________________________')).toBeTruthy()
  expect(fetch).not.toHaveBeenCalled()
  expect(
    screen.getByRole('heading', { name: 'Mission clock: 30 minutes, clock die d4 / d6 (small missions 20 minutes on a d4; large missions 30 minutes on a d6)' }),
  ).toBeTruthy()
  expect(boxes('Mission clock')).toHaveLength(30)
  expect(marked('Mission clock', 'check-in')).toEqual(['10', '15', '20', '25'])
  expect(marked('Alert track', 'start')).toEqual([])
  expect(screen.getByRole('heading', { name: 'Objectives' })).toBeTruthy()
  expect(bodyRows('Objectives')).toHaveLength(2)
  expect(bodyRows('Contacts')).toHaveLength(6)
  expect(bodyRows('Enemy elements')).toHaveLength(6)
  expect(bodyRows('Player force')).toHaveLength(8)
  expect(screen.queryByRole('button', { name: 'Back to the mission' })).toBeNull()
})

test('a large mission has 12 player rows', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => Response.json({ ...landExample, size: 'large', clock: { ...landExample.clock, limit: 30, die: 'd6' } })))
  window.location.hash = '#dashboard?setting=land&size=large&seed=3'
  render(<Dashboard content={content} />)
  await screen.findByText(landExample.title)
  expect(fetch).toHaveBeenLastCalledWith('/missions?setting=land&size=large&seed=3', expect.anything())
  expect(bodyRows('Player force')).toHaveLength(12)
  expect(marked('Mission clock', 'check-in')).toEqual(['10', '15', '20', '25'])
})

test('the mission view opens its dashboard, and the dashboard goes back', async () => {
  window.location.hash = '#missions?setting=ship&seed=42&tier=1'
  render(<Missions content={content} lookup={lookup} />)
  await screen.findByRole('heading', { name: shipExample.title, level: 2 })
  fireEvent.click(screen.getByRole('button', { name: 'Dashboard' }))
  expect(window.location.hash).toBe('#dashboard?setting=ship&tier=1&seed=42')
  cleanup()

  render(<Dashboard content={content} />)
  await screen.findByText(shipExample.title)
  expect(fetch).toHaveBeenLastCalledWith('/missions?setting=ship&tier=1&seed=42', expect.anything())
  fireEvent.click(screen.getByRole('button', { name: 'Back to the mission' }))
  expect(window.location.hash).toBe('#missions?setting=ship&tier=1&seed=42')
})

test('an example mission opens the example dashboard', async () => {
  window.location.hash = '#missions?example=mission-land-11'
  render(<Missions content={content} lookup={lookup} />)
  await screen.findByRole('heading', { name: landExample.title, level: 2 })
  fireEvent.click(screen.getByRole('button', { name: 'Dashboard' }))
  expect(window.location.hash).toBe('#dashboard?example=mission-land-11')
})

test('shows the API error', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => Response.json({ detail: 'Bad seed.' }, { status: 400 })))
  window.location.hash = '#dashboard?seed=1'
  render(<Dashboard content={content} />)
  expect((await screen.findByRole('alert')).textContent).toContain('Bad seed.')
})
