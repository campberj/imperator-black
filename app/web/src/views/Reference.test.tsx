import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, expect, test } from 'vitest'
import { content } from '../test-content'
import { lookup } from '../test-content'
import { Reference } from './Reference'

afterEach(cleanup)

const row = (name: string) => screen.getByRole('rowheader', { name }).closest('tr')!

test('lists every gear item, skill and keyword with its text', () => {
  render(<Reference content={content} lookup={lookup} />)
  for (const g of content.gear.gear) expect(row(g.name).textContent).toContain(g.text)
  for (const k of [...content.keywords.weapon, ...content.keywords.armour]) expect(screen.getAllByText(k.text).length).toBeGreaterThan(0)
  for (const s of content.skills.skills) {
    const name = s.levels.length > 1 ? `${s.name} 1–${s.levels.length}` : s.name
    expect(row(name).textContent).toContain(s.status === 'proposed' ? 'Proposed' : '—')
  }
  // Levelled skills show each level's text.
  const marksman = lookup.skills.get('marksman')!
  expect(row('Marksman 1–2').textContent).toContain(`1: ${marksman.levels[0]}`)
  expect(row('Marksman 1–2').textContent).toContain(`2: ${marksman.levels[1]}`)
})

test('search narrows every section to matching names and text', () => {
  render(<Reference content={content} lookup={lookup} />)
  fireEvent.change(screen.getByLabelText('Search'), { target: { value: 'jump' } })
  expect(row('Jump pack')).toBeTruthy()
  expect(screen.queryByRole('rowheader', { name: 'Trauma kit' })).toBeNull()

  fireEvent.change(screen.getByLabelText('Search'), { target: { value: 'zzz no such thing' } })
  expect(screen.getByText('Nothing matches “zzz no such thing”.')).toBeTruthy()
  expect(screen.queryAllByRole('table')).toHaveLength(0)
})
