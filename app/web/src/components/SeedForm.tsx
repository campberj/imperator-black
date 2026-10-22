import type { FormEvent, ReactNode } from 'react'
import { MAX_SEED } from '../generated'

/** The pickers (children), a seed box, and Show / Previous seed / Next seed. */
export function SeedForm({ label, seed, onSeed, children }: {
  label: string
  seed: number
  onSeed: (seed: number) => void
  children: ReactNode
}) {
  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const s = Number(new FormData(e.currentTarget).get('seed'))
    if (Number.isInteger(s) && s >= 0 && s <= MAX_SEED) onSeed(s)
  }
  return (
    <form className="filters" role="search" onSubmit={onSubmit} aria-label={label}>
      {children}
      <label>
        Seed
        <input key={seed} name="seed" type="number" min={0} max={MAX_SEED} step={1} defaultValue={seed} required />
      </label>
      <button type="submit">Show {label.toLowerCase()}</button>
      <button type="button" onClick={() => onSeed(Math.max(0, seed - 1))} disabled={seed === 0}>
        Previous seed
      </button>
      <button type="button" onClick={() => onSeed(Math.min(MAX_SEED, seed + 1))}>
        Next seed
      </button>
    </form>
  )
}
