import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, expect, test } from 'vitest'
import { content, lookup } from '../test-content'
import { Behaviors } from './Behaviors'

afterEach(cleanup)

test('three reference cards, then one card per behavior profile', () => {
  render(<Behaviors content={content} lookup={lookup} />)
  const cards = screen.getAllByRole('article')
  expect(cards).toHaveLength(3 + content.behaviors.behaviors.length)
  expect(within(cards[0]).getByRole('heading', { name: 'Enemy awareness' })).toBeTruthy()

  for (const bh of content.behaviors.behaviors) {
    const c = within(screen.getByRole('article', { name: bh.name }))
    for (const line of [...bh.engaged, ...bh.alerted, ...bh.reactions]) expect(c.getAllByText(line).length).toBeGreaterThan(0)
    expect(c.getAllByText(bh.investigate).length).toBeGreaterThan(0)
    for (const e of content.factions.elements.filter((x) => x.behavior === bh.id)) expect(c.getByText(new RegExp(`Used by:.*${e.name}`))).toBeTruthy()
  }
})

test('reference cards carry the general procedures', () => {
  render(<Behaviors content={content} lookup={lookup} />)
  const turn = within(screen.getByRole('article', { name: 'Enemy turn' }))
  for (const line of [...content.behaviors.activation_order, ...content.behaviors.tie_breakers]) expect(turn.getByText(line)).toBeTruthy()
  for (const t of content.behaviors.terms ?? []) expect(turn.getByText(t.text)).toBeTruthy()
  const drill = within(screen.getByRole('article', { name: 'Drill and close quarters' }))
  for (const d of content.behaviors.drills) expect(drill.getByText(d.text)).toBeTruthy()
})
