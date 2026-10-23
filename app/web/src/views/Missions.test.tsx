import { act, cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import { content, lookup } from '../test-content'
import { landExample, shipExample } from '../test-missions'
import { EXAMPLES } from '../mission'
import { Missions } from './Missions'

// The API stand-in returns an example mission for whatever was asked.
beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn(async (url: string) => {
    const q = new URLSearchParams(url.split('?')[1])
    const base = q.get('setting') === 'land' ? landExample : shipExample
    return Response.json({ ...base, seed: Number(q.get('seed')), content_version: content.core.version })
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
const section = (title: string) => within(screen.getByRole('heading', { name: title }).closest('section')!)
const rowOf = (table: string, name: string) => section(table).getByRole('rowheader', { name }).closest('tr')!.textContent

// Done when: both example missions in docs/scenarios/missions/ can be shown.
test('finds both example missions in docs/scenarios/missions', () => {
  expect(EXAMPLES.filter((e) => !e.id.startsWith('foundry-moon/')).map((e) => e.id)).toEqual(['mission-land-11', 'mission-ship-42'])
})

test('shows the ship example: briefing, map, how it plays, ground', async () => {
  window.location.hash = '#missions?example=mission-ship-42'
  render(<Missions content={content} lookup={lookup} />)
  const m = shipExample
  expect(await screen.findByRole('heading', { name: m.title, level: 2 })).toBeTruthy()
  expect(screen.getByText(/docs\/scenarios\/missions\/mission-ship-42\.yaml/)).toBeTruthy()
  expect(screen.getByText(`Imperator Black mission · rules v${m.rules_version} · chapter 13 · aboard ship · threat tier ${m.tier} · seed ${m.seed} · solo`)).toBeTruthy()

  // Briefing paragraphs (Grades are the table on the next page).
  for (const b of m.briefing.filter((x) => x.heading !== 'Grades')) expect(screen.getByText(b.text)).toBeTruthy()

  // The map carries the guard, the contacts, the objective and the enemy entry.
  const svg = screen.getByRole('img', { name: new RegExp(m.title.replace(/[()]/g, '.')) })
  expect(svg.querySelectorAll('[data-enemy]')).toHaveLength(m.enemies.length)
  expect(svg.querySelectorAll('[data-contact]')).toHaveLength(m.contacts.length)
  expect(svg.querySelector('[data-enemy-entry] title')!.textContent).toBe(m.enemy_entry.label)
  // Guard models are named on hover.
  expect(svg.querySelector('[data-enemy="1"] title')!.textContent).toBe(`1: ${lookup.troops.get(m.enemies[0].troop)!.name}, ${m.enemies[0].state}`)

  // How it plays.
  const type = content.missions.missions.find((x) => x.id === m.mission)!
  expect(screen.getByRole('heading', { name: type.name, level: 2 })).toBeTruthy()
  expect(screen.getByText(type.task)).toBeTruthy()
  expect(screen.getByText(type.lockdown)).toBeTruthy()
  expect(rowOf('Grades', 'Decisive')).toBe(`Decisive${type.decisive}`)
  expect(screen.getByText(`Raises:`).closest('p')!.textContent).toContain(`Starts at ${m.alert_start}.`)
  for (const t of content.missions.alert.thresholds) expect(screen.getByText(t.effect)).toBeTruthy()
  for (const r of content.missions.clock.rules) expect(screen.getByText(r)).toBeTruthy()
  for (const c of content.missions.contacts) expect(screen.getByText(c)).toBeTruthy()

  // Contact table, as the printed sheet (tools/print_scenario.py contact_table_rows).
  expect(section('Contact table').getAllByRole('row')).toHaveLength(7)
  expect(rowOf('Contact table', '1')).toBe('1False alarmNothing there: crew who fled, a ventilation fan, a stray animal. Remove the chip.—')
  expect(rowOf('Contact table', '2')).toBe('2Salvage crew3 × Salvage HandCowed')

  // Enemy elements with drill, as the printed sheet (element_drill): the HVT is a Leader, so the guard's drill rises.
  expect(rowOf('Elements', 'Asset security')).toBe(
    `Asset securityGuard: models ${m.enemies.map((e) => e.n).join(', ')}; Contact; Reinforcements (alert 4), at the enemy entrySentry` +
      'Fire and manoeuvre; Bounding and grenades with its Leader within 6"',
  )
  expect(rowOf('Elements', 'Salvage crew')).toContain('No drill')
  expect(rowOf('Elements', 'Recovery captain')).toContain('Bounding and grenades; Assault drill with its Leader within 6"')

  // Ground: rooms, doors, hazard, terrain, rolls.
  const rooms = m.tiles!.filter((t) => /^[CS]/.test(t.id))
  expect(section('Rooms').getAllByRole('row')).toHaveLength(rooms.length + 1)
  expect(section('Doors').getAllByRole('row')).toHaveLength(m.doors!.length + 1)
  const locked = m.doors!.find((d) => d.security)!
  expect(rowOf('Doors', locked.id)).toContain(content.core.security.levels.find((l) => l.id === locked.security)!.name)
  expect(screen.getByText(m.hazard!)).toBeTruthy()
  expect(section('Terrain').getAllByRole('row')).toHaveLength(m.cover.length + 1)
  expect(screen.getByText(`${m.rolls.join('; ')}.`)).toBeTruthy()
})

test('shows the land example: sectors instead of rooms and doors', async () => {
  window.location.hash = '#missions?example=mission-land-11'
  render(<Missions content={content} lookup={lookup} />)
  const m = landExample
  expect(await screen.findByRole('heading', { name: m.title, level: 2 })).toBeTruthy()
  expect(screen.getByText(/chapter 13 · land · threat tier 3/)).toBeTruthy()
  expect(section('Sectors').getAllByRole('row')).toHaveLength(m.sectors!.length + 1)
  expect(screen.queryByRole('heading', { name: 'Rooms' })).toBeNull()
  expect(screen.getByRole('img', { name: new RegExp(m.title) }).querySelectorAll('[data-sector]')).toHaveLength(m.sectors!.length)
  expect(rowOf('Contact table', '1')).toContain('Booby trap')
  expect(rowOf('Elements', 'Borz Bratva team')).toContain('Fire and manoeuvre')
  expect(rowOf('Elements', 'Liquidators')).toContain('Bounding and grenades')
  expect(rowOf('Elements', 'Contract reaction force')).toContain('Reinforcements (alert 4), at the enemy entry')
})

test('generates a mission from the pickers, which live in the hash', async () => {
  window.location.hash = '#missions?setting=ship&seed=42&vessel=freighter&mission=capture&tier=1'
  render(<Missions content={content} lookup={lookup} />)
  await screen.findByRole('heading', { name: shipExample.title, level: 2 })
  expect(fetch).toHaveBeenLastCalledWith('/missions?setting=ship&mission=capture&tier=1&vessel=freighter&seed=42', expect.anything())
  expect(screen.getByText(`· content v${content.core.version}`, { exact: false })).toBeTruthy()

  // Ship missions pick a vessel; land missions an environment and a size.
  expect(screen.getByLabelText('Vessel')).toBeTruthy()
  expect(screen.queryByLabelText('Environment')).toBeNull()
  fireEvent.change(screen.getByLabelText('Setting'), { target: { value: 'land' } })
  expect(window.location.hash).toBe('#missions?setting=land&mission=capture&tier=1&seed=42')

  hashChange('#missions?setting=land&mission=capture&tier=1&seed=42')
  expect(screen.queryByLabelText('Vessel')).toBeNull()
  fireEvent.change(screen.getByLabelText('Environment'), { target: { value: 'desert' } })
  hashChange(window.location.hash)
  fireEvent.change(screen.getByLabelText('Size'), { target: { value: 'large' } })
  expect(window.location.hash).toBe('#missions?setting=land&mission=capture&tier=1&environment=desert&size=large&seed=42')
  hashChange(window.location.hash)
  await screen.findByRole('heading', { name: landExample.title, level: 2 })
  expect(fetch).toHaveBeenLastCalledWith('/missions?setting=land&mission=capture&tier=1&environment=desert&size=large&seed=42', expect.anything())

  // "Rolled" leaves the choice to the dice.
  fireEvent.change(screen.getByLabelText('Tier'), { target: { value: '' } })
  expect(window.location.hash).toBe('#missions?setting=land&mission=capture&environment=desert&size=large&seed=42')
})

test('links to the examples, and back to generating', async () => {
  render(<Missions content={content} lookup={lookup} />)
  await screen.findByRole('heading', { name: shipExample.title, level: 2 })
  expect(screen.getByRole('link', { name: 'Example: ship, seed 42' }).getAttribute('href')).toBe('#missions?example=mission-ship-42')
  hashChange('#missions?example=mission-land-11')
  await screen.findByRole('heading', { name: landExample.title, level: 2 })
  fireEvent.click(screen.getByRole('button', { name: 'Generate a mission' }))
  expect(window.location.hash).toBe('#missions')
})

test('shows the API error', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => Response.json({ detail: 'Decks are small missions.' }, { status: 400 })))
  render(<Missions content={content} lookup={lookup} />)
  expect((await screen.findByRole('alert')).textContent).toContain('Decks are small missions.')
})

test('Foundry Moon authored missions show all four stages and their own rules', async () => {
  const authored = EXAMPLES.filter((e) => e.id.startsWith('foundry-moon/'))
  expect(authored).toHaveLength(4)
  for (const e of authored) {
    hashChange(`#missions?example=${e.id}`)
    const heading = await screen.findByRole('heading', { name: /Foundry Moon \d|Foundry Moon 2[A-B]/i, level: 2 }).catch(() => null)
    // Each authored mission must render without relying on generated fallback text.
    expect(heading).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'The enemy' })).toBeTruthy()
  }
})
