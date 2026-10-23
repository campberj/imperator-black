import { act, cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, expect, test, vi } from 'vitest'
import { content } from '../test-content'
import { Planner } from './Planner'

afterEach(() => { cleanup(); vi.unstubAllGlobals(); window.location.hash = '' })

const response = (plan: { fireteams: { id: string; slots: { troop: string; count: number }[] }[]; characters: { troop: string }[]; soldiers: { troop: string }[] }) => {
  const values: Record<string, number> = { rep_clone_sergeant: 50, rep_clone_trooper: 41, rep_clone_heavy: 48, rep_ahsoka: 84 }
  const count = plan.fireteams.reduce((n, t) => n + t.slots.reduce((m, s) => m + s.count, 0), 0) + plan.characters.length + plan.soldiers.length
  const tv = plan.fireteams.reduce((n, t) => n + t.slots.reduce((m, s) => m + (values[s.troop] ?? 0) * s.count, 0), 0) + plan.characters.reduce((n, c) => n + (values[c.troop] ?? 0), 0)
  return { tv, models: count, fireteams: plan.fireteams.length, characters: plan.characters.length, findings: [], team_totals: plan.fireteams.map(t => ({ id: t.id, models: t.slots.reduce((n, s) => n + s.count, 0), tv: t.slots.reduce((n, s) => n + (values[s.troop] ?? 0) * s.count, 0) })), character_totals: plan.characters.map((c, i) => ({ index: i, tv: values[c.troop] ?? 0 })), troop_values: values }
}

test('builds Gym 2 force, displays API totals, changes selection and prints', async () => {
  vi.stubGlobal('fetch', vi.fn(async (_url: string, init?: RequestInit) => Response.json(response(JSON.parse(String(init?.body))))))
  vi.stubGlobal('print', vi.fn())
  render(<Planner content={content} />)
  fireEvent.click(screen.getByRole('button', { name: 'Add standard fireteam' }))
  fireEvent.click(screen.getByRole('button', { name: 'Add character' }))
  const deployment = screen.getByLabelText('Deployment')
  fireEvent.change(deployment, { target: { value: 'team-1' } })
  await act(async () => { await new Promise(resolve => setTimeout(resolve, 500)) })
  expect(screen.getByText(/305 TV/)).toBeTruthy()
  expect(screen.getByText(/6 models/)).toBeTruthy()
  expect(window.location.hash).toContain('plan=')
  fireEvent.change(screen.getByLabelText('Force selection'), { target: { value: 'soldier' } })
  fireEvent.click(screen.getByRole('button', { name: 'Add soldier' }))
  expect(screen.getByLabelText('Group')).toBeTruthy()
  fireEvent.click(screen.getByRole('button', { name: 'Print force list' }))
  expect(window.print).toHaveBeenCalled()
})
