import { useState } from 'react'
import type { Content, Lookup } from '../content'
import { rangePenalty } from '../content'
import { Keywords, Table } from '../components/Table'
import { launcherRounds, matches } from '../format'

/** Gear, skills, keywords and the weapon and armour tables, with the rulebook's columns; searchable. */
export function Reference({ content, lookup }: { content: Content; lookup: Lookup }) {
  const [query, setQuery] = useState('')
  const has = (...texts: string[]) => matches(query, ...texts)
  const kwNames = (ids: string[]) => ids.map((id) => lookup.keywords.get(id)?.name ?? id)
  const status = (s: string) => (s === 'proposed' ? <b key="s">Proposed</b> : '—')

  const gear = content.gear.gear.filter((g) => has(g.name, g.text))
  const skills = content.skills.skills.filter((s) => has(s.name, ...s.levels))
  const weaponKw = content.keywords.weapon.filter((k) => has(k.name, k.text))
  const armourKw = content.keywords.armour.filter((k) => has(k.name, k.text))
  const ranged = content.weapons.ranged.filter((w) => has(w.name, ...kwNames(w.keywords), ...launcherRounds(w)))
  const melee = content.weapons.melee.filter((w) => has(w.name))
  const thrown = content.weapons.thrown.filter((g) => has(g.name, g.effect ?? '', ...kwNames(g.keywords)))
  const armour = content.armour.armour.filter((a) => has(a.name, ...kwNames(a.keywords)))
  const total = gear.length + skills.length + weaponKw.length + armourKw.length + ranged.length + melee.length + thrown.length + armour.length

  return (
    <>
      <div className="filters" role="search">
        <label className="grow">
          Search
          <input type="search" value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Names and rules text" />
        </label>
      </div>
      {total === 0 && <p className="meta">Nothing matches “{query}”.</p>}

      {gear.length > 0 && (
        <Table title="Gear" headers={['Gear', 'Effect', 'Status']} rows={gear.map((g) => [g.name, g.text, status(g.status)])} />
      )}
      {skills.length > 0 && (
        <Table
          title="Skills"
          headers={['Skill', 'Effect', 'Status']}
          rows={skills.map((s) => [
            <i key="n">{s.name + (s.levels.length > 1 ? ` 1–${s.levels.length}` : '')}</i>,
            s.levels.length === 1 ? (
              s.levels[0]
            ) : (
              <span key="l">
                {s.levels.map((text, i) => (
                  <span key={i}>
                    {i > 0 && ' '}
                    <b>{i + 1}:</b> {text}
                  </span>
                ))}
              </span>
            ),
            status(s.status),
          ])}
        />
      )}
      {weaponKw.length > 0 && (
        <Table title="Weapon keywords" headers={['Keyword', 'Effect']} rows={weaponKw.map((k) => [<i key="n">{k.name}</i>, k.text])} />
      )}
      {armourKw.length > 0 && (
        <Table title="Armour keywords" headers={['Keyword', 'Effect']} rows={armourKw.map((k) => [<i key="n">{k.name}</i>, k.text])} />
      )}
      {ranged.length > 0 && (
        <Table
          title="Ranged weapons"
          headers={['Weapon', 'Damage', 'Short range', 'Beyond / double', 'AP', 'Keywords']}
          rows={ranged.map((w) => {
            const [beyond, double] = rangePenalty(w)
            return [w.name, w.damage, `${w.short_range}"`, `${beyond} / ${double}`, w.ap, <span key="kw"><Keywords ids={w.keywords} lookup={lookup} weapon={w} />{w.explosion ? `${w.keywords.length ? ', ' : ''}Explosion (${w.explosion})` : ''}{launcherRounds(w).length > 0 && <small className="meta"><br />{launcherRounds(w).join(', ')}. Choose the round before each Shoot.</small>}</span>]
          })}
        />
      )}
      {melee.length > 0 && (
        <Table title="Melee weapons" headers={['Melee weapon', 'Damage', 'AP']} rows={melee.map((w) => [w.name, w.damage, w.ap])} />
      )}
      {thrown.length > 0 && (
        <Table
          title="Grenades"
          headers={['Grenade', 'Throw range', 'Radius', 'Damage / effect']}
          rows={thrown.map((g) => [
            g.name,
            `${g.throw_range}"`,
            `${g.blast_radius}"`,
            g.damage ? `${g.damage} (AP ${g.ap ?? 0})` : (g.effect ?? ''),
          ])}
        />
      )}
      {armour.length > 0 && (
        <Table
          title="Armour"
          headers={['Armour', 'Value', 'Keywords']}
          rows={armour.map((a) => [a.name, a.value, <Keywords key="kw" ids={a.keywords} lookup={lookup} />])}
        />
      )}
    </>
  )
}
