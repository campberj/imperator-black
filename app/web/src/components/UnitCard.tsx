import { memo } from 'react'
import type { Content, Lookup, Troop } from '../content'
import {
  elementsFor,
  factionColour,
  gearWeaponKeywords,
  keywordLabel,
  launcherRounds,
  rangePenaltyNote,
  skillName,
  skillText,
  statText,
  troopWeapons,
} from '../format'
import { Card, Proposed } from './Card'

/** One troop profile, laid out like docs/print/ImperatorBlack_unit-cards.pdf (tools/print_sheets.py). */
export const UnitCard = memo(function UnitCard({ troop, content, lookup, value, pinning }: { troop: Troop; content: Content; lookup: Lookup; value?: { tv: number; requisition: number }; pinning?: string | null }) {
  const faction = lookup.factions.get(troop.faction)
  const rank = lookup.ranks.get(troop.rank)?.name ?? troop.rank
  const armour = lookup.armour.get(troop.armour)
  const weapons = troopWeapons(troop, lookup)
  const extraKeywords = gearWeaponKeywords(troop, lookup)
  const melee = troop.melee ? lookup.melee.get(troop.melee) : undefined
  const elements = faction?.side === 'enemy' ? elementsFor(content, troop.id) : []

  return (
    <Card
      title={troop.name}
      subtitle={`${rank} ${troop.role}  ·  ${faction?.name ?? troop.faction}`}
      colour={factionColour(troop.faction)}
      corner={content.rosters.classes?.find((c) => c.id === troop.class)?.name ?? troop.class}
      footer={
        <>
          <span className="minis">{troop.miniatures}</span>
          <span>Imperator Black · content v{content.core.version}</span>
        </>
      }
    >
      {value && <p className="small"><b>Troop Value (TV): {value.tv}</b> · Requisition: {value.requisition}</p>}
      <table className="stats">
        <thead>
          <tr>
            {content.core.attributes.map((a) => (
              <th key={a.id} scope="col">
                <abbr title={a.name}>{a.short}</abbr>
              </th>
            ))}
            <th scope="col" className="arm">
              <abbr title="Armour">ARM</abbr>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            {content.core.attributes.map((a) => (
              <td key={a.id} data-attr={a.id}>
                {statText(troop, a.id)}
              </td>
            ))}
            <td className="arm" data-attr="armour">
              {armour?.value}
            </td>
          </tr>
        </tbody>
      </table>
      {pinning && <p className="small">{pinning}</p>}
      <p className="small">
        Armour: {armour?.name}
        {(troop.armour_options ?? []).map((id) => {
          const a = lookup.armour.get(id)
          return ` or ${a?.name} (${a?.value})`
        })}
      </p>

      <table className="weapons">
        <thead>
          <tr>
            <th scope="col">Weapon</th>
            <th scope="col">Dmg</th>
            <th scope="col">Short</th>
            <th scope="col">AP</th>
          </tr>
        </thead>
        <tbody>
          {weapons.map((w, i) => {
            const keywords = [...w.keywords, ...(i === 0 ? extraKeywords : [])]
            const notes = keywords.map((k) => keywordLabel(k, lookup, w))
            const penalty = rangePenaltyNote(w)
            if (penalty) notes.push(penalty)
            if (w.explosion) notes.push(`Explosion (${w.explosion})`)
            return [
              <tr key={w.id} data-weapon={w.id}>
                <td>{i === 0 ? <b>{w.name}</b> : <i>or {w.name}</i>}</td>
                <td>{w.damage}</td>
                <td>{w.short_range}"</td>
                <td>{w.ap}</td>
              </tr>,
              launcherRounds(w).length > 0 && (
                <tr key={`${w.id}-rounds`} className="note">
                  <td colSpan={4}>{launcherRounds(w).join(', ')}. Choose the round before each Shoot.</td>
                </tr>
              ),
              notes.length > 0 && (
                <tr key={`${w.id}-notes`} className="note">
                  <td colSpan={4}>{notes.join(', ')}</td>
                </tr>
              ),
            ]
          })}
          {weapons.length === 0 && (
            <tr className="note">
              <td colSpan={4}>
                <i>No ranged weapon: cannot Shoot</i>
              </td>
            </tr>
          )}
          {melee && (
            <tr data-weapon={melee.id}>
              <td>
                <b>{melee.name}</b> (close combat)
              </td>
              <td>{melee.damage}</td>
              <td>—</td>
              <td>{melee.ap}</td>
            </tr>
          )}
        </tbody>
      </table>

      {troop.grenades.length > 0 && (
        <p>
          <b>Grenades:</b> {troop.grenades.map((g) => lookup.thrown.get(g)?.name ?? g).join(', ')}
        </p>
      )}

      {(troop.gear.length > 0 || (troop.gear_options ?? []).length > 0) && (
        <section aria-label="Gear">
          <h4>Gear</h4>
          {troop.gear.map((id) => {
            const g = lookup.gear.get(id)
            return (
              <p key={id}>
                <b>{g?.name ?? id}</b>
                {g?.status === 'proposed' && <> <Proposed /></>}: {g?.text}
              </p>
            )
          })}
          {(troop.gear_options ?? []).length > 0 && (
            <p>
              <b>Choose one:</b> {(troop.gear_options ?? []).map((g) => lookup.gear.get(g)?.name ?? g).join(' / ')}
            </p>
          )}
        </section>
      )}

      {/* Skills granted by gear are already explained in the gear text. */}
      {Object.keys(troop.skills).length > 0 && (
        <section aria-label="Skills">
          <h4>Skills</h4>
          {Object.entries(troop.skills).map(([id, level]) => (
            <p key={id}>
              <b>{skillName(lookup, id, level)}</b>
              {lookup.skills.get(id)?.status === 'proposed' && <> <Proposed /></>}: {skillText(lookup, id, level)}
            </p>
          ))}
        </section>
      )}

      {elements.length > 0 && (
        <p>
          <b>Element:</b>{' '}
          {elements
            .map((e) => `${e.name} (tier ${e.tier}, ${lookup.behaviors.get(e.behavior)?.name ?? e.behavior})`)
            .join('; ')}
        </p>
      )}
    </Card>
  )
})
