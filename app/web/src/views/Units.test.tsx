import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, expect, test, vi } from 'vitest'
import { content, lookup } from '../test-content'
import { Units } from './Units'
import { FireteamsView } from './Fireteams'

afterEach(cleanup)

const card = (name: string) => screen.getByRole('article', { name })
const select = (label: string, value: string) => fireEvent.change(screen.getByLabelText(label), { target: { value } })

test('every troop profile renders, faction by faction', () => {
  render(<Units content={content} lookup={lookup} />)
  for (const f of content.factions.factions) {
    select('Faction', f.id)
    const troops = content.troops.troops.filter((t) => t.faction === f.id)
    expect(screen.getAllByRole('article')).toHaveLength(troops.length)
    for (const t of troops) expect(card(t.name)).toBeTruthy()
  }
  select('Faction', '')
  expect(screen.getAllByRole('article')).toHaveLength(content.troops.troops.length)
})

test('one profile per faction matches the content', () => {
  render(<Units content={content} lookup={lookup} />)
  for (const f of content.factions.factions) {
    select('Faction', f.id)
    const t = content.troops.troops.find((x) => x.faction === f.id)!
    const c = within(card(t.name))

    // Testing Library collapses the subtitle's double spaces.
    expect(c.getByText(`${lookup.ranks.get(t.rank)!.name} ${t.role} · ${f.name}`)).toBeTruthy()
    for (const a of content.core.attributes) {
      const v = t.stats[a.id]
      const expected = v === null ? '—' : a.id === 'move' ? (v ? `${v}"` : '—') : String(v)
      expect(card(t.name).querySelector(`td[data-attr="${a.id}"]`)!.textContent).toBe(expected)
    }
    const armour = lookup.armour.get(t.armour)!
    expect(card(t.name).querySelector('td[data-attr="armour"]')!.textContent).toBe(String(armour.value))
    expect(c.getByText(new RegExp(`^Armour: ${armour.name}`))).toBeTruthy()

    if (t.weapon) {
      const w = lookup.ranged.get(t.weapon)!
      const row = card(t.name).querySelector(`tr[data-weapon="${w.id}"]`)!
      expect(row.textContent).toBe(`${w.name}${w.damage}${w.short_range}"${w.ap}`)
    } else {
      expect(c.getByText('No ranged weapon: cannot Shoot')).toBeTruthy()
    }
    if (t.melee) expect(card(t.name).querySelector(`tr[data-weapon="${t.melee}"]`)!.textContent).toContain(lookup.melee.get(t.melee)!.name)
    for (const g of t.gear) expect(c.getByText(lookup.gear.get(g)!.name)).toBeTruthy()
    for (const [id, level] of Object.entries(t.skills)) {
      const s = lookup.skills.get(id)!
      expect(within(c.getByRole('region', { name: 'Skills' })).getByText(s.levels.length > 1 ? `${s.name} ${level}` : s.name)).toBeTruthy()
    }
    if (f.side === 'enemy') expect(c.getByText('Element:')).toBeTruthy()
  }
})

test('weapon notes show Auto N, gear keywords and range penalties', () => {
  render(<Units content={content} lookup={lookup} />)
  select('Faction', '')
  // Silencer adds Silenced to the Liquidator's weapon; the SMG is Auto 2.
  const liq = card(lookup.troops.get('fs_liquidator')!.name)
  expect(liq.textContent).toContain('Silenced')
  const smgUser = content.troops.troops.find((t) => t.weapon === 'smg')!
  expect(card(smgUser.name).textContent).toContain('Auto 2')
  const shotgunUser = content.troops.troops.find((t) => [t.weapon, ...(t.weapon_options ?? [])].includes('shotgun'))!
  expect(card(shotgunUser.name).textContent).toContain('beyond short -2, beyond double -3')
})

test('player factions list teams, enemy factions list elements', () => {
  render(<Units content={content} lookup={lookup} />)
  select('Faction', 'tfb')
  expect(screen.getByRole('heading', { name: 'Teams' })).toBeTruthy()
  expect(screen.queryByRole('heading', { name: 'Elements' })).toBeNull()
  for (const t of content.rosters.teams.filter((x) => x.faction === 'tfb')) expect(screen.getByRole('rowheader', { name: t.name })).toBeTruthy()

  select('Faction', 'freespace')
  expect(screen.queryByRole('heading', { name: 'Teams' })).toBeNull()
  const automated = content.factions.elements.find((e) => e.id === 'automated_defences')!
  const row = screen.getByRole('rowheader', { name: automated.name }).closest('tr')!
  expect(row.textContent).toContain(`(or ${lookup.troops.get(automated.alternatives![0])!.name})`)
  expect(row.textContent).toContain(lookup.behaviors.get(automated.behavior)!.name)
})

test('filters by rank, role and keyword, and searches names and rules text', () => {
  render(<Units content={content} lookup={lookup} />)
  select('Faction', '')
  const count = () => screen.queryAllByRole('article').length

  select('Rank', 'elite')
  expect(count()).toBe(content.troops.troops.filter((t) => t.rank === 'elite').length)
  select('Rank', '')

  select('Keyword', 'silenced')
  expect(card(lookup.troops.get('fs_liquidator')!.name)).toBeTruthy()
  expect(count()).toBeLessThan(content.troops.troops.length)
  select('Keyword', '')

  // "Medic" is only in rules text: the trauma kit grants it.
  fireEvent.change(screen.getByLabelText('Search'), { target: { value: 'medic skill' } })
  const withKit = content.troops.troops.filter((t) => [...t.gear, ...(t.gear_options ?? [])].includes('trauma_kit'))
  expect(count()).toBeGreaterThanOrEqual(withKit.length)
  for (const t of withKit) expect(card(t.name)).toBeTruthy()

  fireEvent.change(screen.getByLabelText('Search'), { target: { value: 'zzz no such thing' } })
  expect(count()).toBe(0)
  expect(screen.getByText(`0 of ${content.troops.troops.length} troop profiles`)).toBeTruthy()
})

test('changing faction clears a role that no longer applies', () => {
  render(<Units content={content} lookup={lookup} />)
  select('Faction', 'tfb')
  select('Role', 'Leader')
  select('Faction', 'freespace')
  expect((screen.getByLabelText('Role') as HTMLSelectElement).value).toBe('')
})

test('AT launcher card shows both anti-armour and high explosive rounds', () => {
  const launcher = content.weapons.ranged.find((w) => w.rounds?.length && w.round)!
  const troop = content.troops.troops.find((t) => [t.weapon, ...(t.weapon_options ?? [])].includes(launcher.id))!
  render(<Units content={content} lookup={lookup} />)
  select('Faction', troop.faction)
  const c = card(troop.name)
  expect(c.textContent).toContain(`${launcher.round} above`)
  for (const round of launcher.rounds!) {
    expect(c.textContent).toContain(`or ${round.name.toLowerCase()} round: ${round.damage}, AP ${round.ap}`)
    if (round.explosion) expect(c.textContent).toContain(`Explosion (${round.explosion})`)
  }
  expect(c.textContent).toContain('Choose the round before each Shoot')
})

test('unit database displays TV and Requisition supplied by GET /values', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => Response.json([
    { troop: 'rep_clone_trooper', tv: 41, requisition: 5 },
  ])))
  try {
    render(<Units content={content} lookup={lookup} />)
    select('Faction', 'republic')
    const clone = within(card(lookup.troops.get('rep_clone_trooper')!.name))
    expect(await clone.findByText('Troop Value (TV): 41')).toBeTruthy()
    expect(clone.getByText(/Requisition: 5/)).toBeTruthy()
    expect(fetch).toHaveBeenCalledWith('/values', expect.anything())
  } finally {
    vi.unstubAllGlobals()
  }
})

test('clone trooper, Obi-Wan and VAT fireteam show class, pinning and type', async () => {
  const prior = globalThis.fetch
  vi.stubGlobal('fetch', vi.fn(async (url: string) => {
    if (url === '/fireteams') return Response.json({
      pinning: [
        { troop: 'rep_clone_trooper', text: 'Pinned at 4 Suppression' },
        { troop: 'rep_obi_wan', text: 'Never Pinned' },
      ],
      fireteams: [{
        id: 'vat_fireteam', name: 'VAT fireteam', faction: 'tfb', type: 'assault',
        source: 'team', models: content.rosters.teams.find((t) => t.id === 'vat_fireteam')!.slots, findings: [],
      }],
    })
    return Response.json([])
  }))
  try {
    render(<Units content={content} lookup={lookup} />)
    select('Faction', 'republic')
    const clone = within(card(lookup.troops.get('rep_clone_trooper')!.name))
    expect(clone.getByText('Trooper')).toBeTruthy()
    expect(await clone.findByText('Pinned at 4 Suppression')).toBeTruthy()
    const obiWan = within(card(lookup.troops.get('rep_obi_wan')!.name))
    expect(obiWan.getByText('Never Pinned')).toBeTruthy()
    cleanup()
    render(<FireteamsView content={content} lookup={lookup} />)
    select('Faction', 'tfb')
    const vatRow = (await screen.findByRole('rowheader', { name: 'VAT fireteam' })).closest('tr')!
    expect(vatRow.textContent).toContain('Assault')
  } finally {
    vi.stubGlobal('fetch', prior)
    vi.unstubAllGlobals()
  }
})
