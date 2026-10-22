import { useId, type ReactNode } from 'react'

/** A card with a coloured title band, as on the printed unit and behavior cards. */
export function Card({ title, subtitle, colour, footer, children }: {
  title: string
  subtitle: string
  colour: string
  footer?: ReactNode
  children: ReactNode
}) {
  const id = useId()
  return (
    <article className="card" aria-labelledby={id}>
      <header className="card-band" style={{ background: colour }}>
        <h3 id={id}>{title}</h3>
        <p>{subtitle}</p>
      </header>
      <div className="card-body">{children}</div>
      {footer && <footer className="card-foot">{footer}</footer>}
    </article>
  )
}

export function Proposed() {
  return <span className="proposed">[proposed]</span>
}

export function Numbered({ lines }: { lines: string[] }) {
  return (
    <ol>
      {lines.map((l) => (
        <li key={l}>{l}</li>
      ))}
    </ol>
  )
}

export function Bulleted({ lines }: { lines: string[] }) {
  return (
    <ul>
      {lines.map((l) => (
        <li key={l}>{l}</li>
      ))}
    </ul>
  )
}
