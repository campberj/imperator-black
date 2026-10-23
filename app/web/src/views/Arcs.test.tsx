import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, expect, test } from 'vitest'
import { content } from '../test-content'
import { Arcs, ARCS } from './Arcs'

afterEach(() => { cleanup(); window.location.hash = '' })

test('Foundry Moon shows both branches, all four missions, and authored short arc rules', async () => {
  expect(ARCS.some((a) => a.id === 'foundry-moon')).toBe(true)
  window.location.hash = '#arcs?arc=foundry-moon'
  render(<Arcs content={content} />)
  expect(await screen.findByRole('heading', { name: 'Foundry Moon' })).toBeTruthy()
  const flow = screen.getByRole('region', { name: 'Mission flow' })
  expect(within(flow).getAllByRole('link')).toHaveLength(6)
  expect(flow.textContent).toContain('Failure')
  expect(flow.textContent).toContain('2B')
  const missions = screen.getByRole('heading', { name: 'Missions' }).closest('section')!
  expect(within(missions).getAllByRole('row')).toHaveLength(5)
  const links = within(missions).getAllByRole('link')
  expect(links).toHaveLength(4)
  for (const link of links) expect(link.getAttribute('href')).toMatch(/^#missions\?example=foundry-moon%2F/)
  const branch = links.find((l) => l.textContent?.includes('2B'))!
  expect(branch.textContent).toContain('Bring Them Home')
  expect(within(missions).getAllByText('300 TV, 7 models')).toHaveLength(2)
  expect(screen.getByRole('heading', { name: 'Short arc rules' })).toBeTruthy()
  expect(screen.getByText(content.campaign!.short_arc!.text[0])).toBeTruthy()
  expect(screen.getByRole('heading', { name: 'What each grade leads to' })).toBeTruthy()
  expect(screen.getByRole('heading', { name: 'Arc results' })).toBeTruthy()
})
