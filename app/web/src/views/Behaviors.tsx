import type { ReactNode } from 'react'
import type { Behavior, Content, Lookup } from '../content'
import { Bulleted, Card, Numbered } from '../components/Card'

// The chapter 10 cards, as in docs/print/ImperatorBlack_behavior-cards.pdf (tools/print_sheets.py): three reference cards
// for running the enemy, then one card per behavior profile.
const COLOUR = '#3a3f46'

export function Behaviors({ content, lookup }: { content: Content; lookup: Lookup }) {
  const b = content.behaviors
  const footer = <span>Imperator Black · content v{content.core.version} · chapter 10</span>
  return (
    <div className="cards">
      <Card title="Enemy awareness" subtitle="Solo opponent reference 1 of 3" colour={COLOUR} footer={footer}>
        <h4>Awareness</h4>
        {b.awareness.map((a) => (
          <p key={a.id}>
            <b>{a.name}:</b> {a.text}
          </p>
        ))}
        <h4>Noise</h4>
        {b.noise.map((n) => (
          <p key={n.id}>
            <b>{n.name}</b> ({n.sources}): {n.alert === 0 ? n.heard.toLowerCase() : n.heard}
            {n.alert ? `; alert +${n.alert}.` : '.'}
          </p>
        ))}
        <h4>The marker</h4>
        <Bulleted lines={b.marker} />
        <h4>The alarm</h4>
        <Bulleted lines={b.alarm} />
      </Card>

      <Card title="Enemy turn" subtitle="Solo opponent reference 2 of 3" colour={COLOUR} footer={footer}>
        <h4>Who activates next</h4>
        <Numbered lines={b.activation_order} />
        <h4>Targets</h4>
        <Numbered lines={b.targets} />
        <h4>Moving</h4>
        <Bulleted lines={b.moving} />
        <h4>Surrender</h4>
        <Bulleted lines={b.surrender} />
        <h4>Tie-breakers</h4>
        <Numbered lines={b.tie_breakers} />
        {(b.terms ?? []).length > 0 && (
          <>
            <h4>Terms</h4>
            {(b.terms ?? []).map((t) => (
              <p key={t.term}>
                <b>{t.term}:</b> {t.text}
              </p>
            ))}
          </>
        )}
      </Card>

      <Card title="Drill and close quarters" subtitle="Solo opponent reference 3 of 3" colour={COLOUR} footer={footer}>
        <p>Drill: set by the element's lowest rank; a friendly Leader within 6" raises it one step.</p>
        {b.drills.map((d) => (
          <p key={d.id}>
            <b>
              {lookup.ranks.get(d.rank)?.name ?? d.rank}, {d.name}:
            </b>{' '}
            {d.text}
          </p>
        ))}
        <h4>Close quarters</h4>
        <p>{b.close_quarters[0]}</p>
        <Numbered lines={b.close_quarters.slice(1)} />
      </Card>

      {b.behaviors.map((bh) => (
        <BehaviorCard key={bh.id} behavior={bh} content={content} footer={footer} />
      ))}
    </div>
  )
}

function BehaviorCard({ behavior: bh, content, footer }: { behavior: Behavior; content: Content; footer: ReactNode }) {
  const users = content.factions.elements.filter((e) => e.behavior === bh.id).map((e) => e.name)
  return (
    <Card
      title={bh.name}
      subtitle={'Enemy behavior profile' + (bh.status === 'proposed' ? '  ·  proposed' : '')}
      colour={COLOUR}
      footer={footer}
    >
      <p>
        <i>{bh.summary}</i>
      </p>
      {users.length > 0 && <p className="small">Used by: {users.join(', ')}.</p>}
      {bh.close_quarters && (
        <p>
          <b>Player model within 6":</b> close quarters first (reference card).
        </p>
      )}
      <h4>Target in sight (Engaged)</h4>
      <Numbered lines={bh.engaged} />
      <h4>No target (Alerted)</h4>
      <Numbered lines={bh.alerted} />
      <p>
        <b>Investigates:</b> {bh.investigate}
      </p>
      <h4>Reactions</h4>
      <Bulleted lines={bh.reactions} />
      <h4>Close combat</h4>
      <p>{bh.close_combat + (bh.surrenders ? ' May surrender.' : '')}</p>
      <p className="small">Read each list top-down; take the first line that applies. Unaware models do not activate or react.</p>
    </Card>
  )
}
