# Imperator Black — Design Decision Log

Living record of design decisions. Each entry is **Settled** (build on it), **Leaning** (default unless playtesting says otherwise), **Proposed** (awaiting a call), or **Open** (needs work).

Imperator Black continues the earlier *A War of Worlds* design. Its documents are kept in [a-war-of-worlds/](a-war-of-worlds/): the [design statement and roadmap](a-war-of-worlds/design-statement-and-roadmap.md) and [prototype rules v0.1](a-war-of-worlds/prototype-rules-v0.1.md). Entries marked *(AWoW)* come from there. Where this log and those documents disagree, this log wins.

Last updated: 2026-10-20 (rules v0.3)

---

## 1. Identity and scope

| # | Decision | Status |
|---|----------|--------|
| 1.1 | Imperator Black continues A War of Worlds; everything settled there carries forward unless revised here. | Settled |
| 1.2 | Design pillars and non-goals per [pillars.md](pillars.md) *(AWoW, revised)*. | Settled |
| 1.3 | Model-agnostic: no rule depends on a specific miniature line. *(AWoW)* | Settled |
| 1.4 | One model per soldier; fireteams are sub-units. *(AWoW)* | Settled |
| 1.5 | Scale: fireteam up to platoon, one rules set. PoC targets fireteam / squad. *(AWoW)* | Settled |
| 1.6 | Crunch medium to heavy. Crunch lives where decisions are, not in paperwork. Doctrinal feel, not a simulation. *(AWoW)* | Settled |
| 1.7 | Three layers: Tabletop → Ship/Outpost → Sector. | Settled |
| 1.8 | Play modes: solo is the home mode; co-op and PvP follow. PoC = solo. | Settled |
| 1.9 | Fresh core that borrows proven ideas, not a hack of an existing game. *(AWoW)* | Settled |

## 2. Setting

| # | Decision | Status |
|---|----------|--------|
| 2.1 | Tone sits between *The Expanse* and *Galaxy's Edge*, leaning Galaxy's Edge: grounded military fiction, believable tech, space-opera scope. | Settled |
| 2.2 | The trench-war setting pack from A War of Worlds is **dropped**. | Settled |
| 2.3 | The player leads a force whose latitude **varies by campaign type**: Black Ops campaigns give high-level orders and wide freedom; others give tighter guidance. Revises the AWoW "fully autonomous age-of-sail captain" framing. | Settled |
| 2.4 | Lore reason most fights are small-unit (candidates: ubiquity of autonomous guided munitions; difficulty moving large forces through the void). | Open |

## 3. Core resolution

| # | Decision | Status |
|---|----------|--------|
| 3.1 | **2d6** hit roll, thrown together with the weapon's **damage dice pair** (2d4 → 2d12). Damage counts only on a hit. *(AWoW)* | Settled |
| 3.2 | Hit target **8+** on 2d6 + Q + modifiers. Natural 2 always misses; natural double 6 is a critical (+1 wound row). *(AWoW prototype)* | Leaning — playtest |
| 3.3 | **Quality (Q)** 0–3: Green 0, Regular 1, Veteran 2, Elite 3. Added to hit rolls and tests. Simulator: each +1 Q ≈ +20 points of win rate. *(AWoW prototype)* | Leaning |
| 3.4 | Damage total is read against a **ten-row wound table**; **armour steps the row down by whole rows (0–3), less weapon AP**. *(AWoW)* | Settled |
| 3.5 | Wound table bands and results per prototype v0.1 (suppression in rows 1–3, Wounded 4–5, Down/bleeding/critical 6–8, Dead 9–10). Rows 1–3 are the first tuning target. *(AWoW prototype)* | Leaning — playtest |
| 3.6 | **Down recovery:** end of each round, a Down model tests 2d6 + Q at 8+; on a pass it gets up Wounded with 3 Suppression. A *Medic* recovers a Down model automatically. | Leaning — playtest |
| 3.7 | **No fixed weapon ranges.** Each weapon has a **short range**; beyond it -1, beyond double -2 (shotgun -2/-3). *(AWoW)* | Settled |
| 3.8 | Quality may extend how far fire stays effective (Stargrunt-style). *(AWoW teardown)* | Proposed — Phase 1 |
| 3.9 | Using micro-terrain in the open is a **troop-quality test** (Find Cover: 2d6 + Q − Suppression; 7+ Light, 10+ Heavy). *(AWoW)* | Settled |
| 3.10 | Cover: Light (≥⅓ hidden, −1 to hit) and Heavy (≥½ hidden behind solid material, −2). True line of sight. *(AWoW prototype)* | Leaning |
| 3.11 | **Close combat:** started by moving into contact; resolves at once in exchanges of opposed 2d6 + Q (charge +1 first exchange, support +1 each up to +2). The winner rolls melee damage on the wound table; a no-effect result still adds 1 Suppression. Continues until one side is incapacitated or **both** break away. A model at 5 Suppression that loses is overpowered (Down or captured). | Settled |
| 3.12 | **Grenades and demolitions:** Throw action (test 8+, misses land short, natural 2 slips); frag (one damage roll, +1 row in confined compartments), flashbang (+2 Suppression, strips Overwatch, no reactions this round), smoke; breaching charges (place with Interact, detonate as a later action). | Leaning — playtest |
| 3.13 | **Lethality retune (rules v0.3):** re-banded wound table (suppression 2–5, Wounded 6–9, Down 10–11, Bleeding 12–13, Critical 14–15, Dead 16+); sidearm 2d6, SMG and carbine 2d8, shotgun 2d12; knife 2d8, breaching tool 2d10 (unarmed 2d4); frag 2d8. Per [sim findings](../analysis/sim-v0.2-findings.md); passes Pillar 1 at 75%. | Settled — playtest |

## 4. Turn structure, reactions and morale

| # | Decision | Status |
|---|----------|--------|
| 4.1 | Each round starts with an initiative roll (2d6 + leader's Q); winner chooses who activates first. *(AWoW prototype)* | Leaning |
| 4.2 | Players **alternate activating single models**; when one side runs out, the other finishes. *(AWoW)* | Settled |
| 4.3 | **Two actions** per activation: Move, Shoot, Aim, Suppressive Fire, Overwatch, Find Cover, Rally, Medic, Interact. *(AWoW prototype)* | Settled (action list Leaning) |
| 4.4 | **Reaction:** a model that has not yet activated may react when an enemy model's action allows a reaction. Reacting marks it as activated. | Settled |
| 4.5 | **Overwatch:** a model that activates may go on Overwatch. It may then react **up to two times** until its activation comes again next round. Supersedes the prototype's single Overwatch shot. | Settled |
| 4.6 | **Reaction triggers:** an enemy Moving into or within line of sight (interrupts the move), Shooting/Throwing/Suppressive Fire, or Interacting (including doors). **Reaction choices:** one action: Shoot the triggering model at −1, Move up to half to cover or out of sight, or Find Cover. Reactions can't trigger reactions. | Settled |
| 4.7 | **Suppression:** each marker −1 to all rolls (max 5); **Pinned at 3+** (only Rally, Find Cover, Move to cover, Shoot from cover). Two or more Suppression cancels Overwatch. *(AWoW prototype)* | Leaning |
| 4.8 | **Casualty shock** (+1 Suppression when a friend within 6" goes Down/Dead; +2 for the leader) and **breaking** at half a fireteam Down or Dead. *(AWoW prototype)* | Leaning |
| 4.9 | Leaders can pass actions to subordinates, as the scaling mechanism to squad and platoon (Stargrunt-style). *(AWoW teardown)* | Proposed — post-PoC |
| 4.10 | No forced unit-cohesion abstractions. *(AWoW)* | Settled |

## 5. Movement, scale and terrain

| # | Decision | Status |
|---|----------|--------|
| 5.1 | Ground scale ≈ **1" = 2 m**. *(AWoW)* | Settled |
| 5.2 | Boards vary from **18" × 18"** (boarding actions) to **9' × 5'**. *(AWoW)* | Settled |
| 5.3 | Move **6"** (5" in Armour 3); climbing or vaulting a low obstacle costs +2". Tuned so models can bound between cover. *(AWoW prototype)* | Leaning |
| 5.4 | Terrain density guideline per board size so a move can always reach cover. *(AWoW)* | Open — Phase 4 |
| 5.5 | **Ship interiors:** doors open/closed/locked; opening is an Interact (or 2" of a Move) and triggers reactions; locked doors yield to breaching charge, breaching tool (test 8+) or a *Tech* hack (test 9+). Compartments ≤ 6"×6" are confined. | Leaning — playtest |
| 5.6 | **Ship hazards are in the PoC:** zero-g (push-off movement, unbraced −1), decompression and vacuum, fire, darkness. | Settled |

## 6. Equipment and force building

| # | Decision | Status |
|---|----------|--------|
| 6.1 | Weapons carry distinct profiles: damage pair, short range, AP, keywords (e.g. *Handy*, *Auto*). Starting list per prototype v0.1. *(AWoW)* | Leaning |
| 6.2 | Armour 0–3: None, Flak, Composite plate, Heavy plate. *(AWoW prototype)* | Leaning |
| 6.3 | Tiered equipment so mixed collections share a table. *(AWoW)* | Settled |
| 6.4 | Forces built with a campaign **requisition budget**; points optional. *(AWoW)* | Settled |

## 7. Campaign and progression

| # | Decision | Status |
|---|----------|--------|
| 7.1 | Awards / veterancy; soldiers, squads and the whole force progress (reputation, unit honors). *(AWoW)* | Settled |
| 7.2 | Hybrid heroes: the player may name a starting leader; others can surpass them. *(AWoW)* | Settled |
| 7.3 | Missions have consequences that carry forward; decisions are tracked across missions. *(AWoW)* | Settled |
| 7.4 | Force-level "doctrines" unlocked by campaign resources (Zone Raiders–style). *(AWoW teardown)* | Proposed — Phase 7 |
| 7.5 | The PoC includes the full campaign loop. | Settled |

## 8. Solo play and AI

| # | Decision | Status |
|---|----------|--------|
| 8.1 | PoC solo AI = **table-driven behavior profiles** per enemy type, building on the prototype's activation-priority list and role priorities. | Settled |
| 8.2 | The app generates enemies and gives **general play guidance** per enemy type; it does not run enemies turn by turn. | Settled |
| 8.3 | **Hidden contacts:** enemy tokens start as unknown contacts, revealed on line of sight or noise, rolled on a contact table per threat tier. *(AWoW teardown)* | Leaning — playtest ([opposition](opposition-force.md)) |
| 8.4 | **Alert track (0–6)** driven by noise; contacts converge at 2, contractor reaction force at 4, lockdown at 6. Adapted from Zero Dark's deck clock. *(AWoW teardown)* | Leaning — playtest ([opposition](opposition-force.md)) |
| 8.6 | Cowed enemies may **surrender**; prisoners are a campaign resource and a tactical burden. | Leaning — playtest |
| 8.5 | Longer term: a more organic, situation-reactive AI. | Leaning |

## 9. Proof of concept

| # | Decision | Status |
|---|----------|--------|
| 9.1 | PoC = **printed rules and tables** played with miniatures, plus an app that generates layout, mission and enemies. | Settled |
| 9.2 | First environment: **ship / station boarding actions**. | Settled |
| 9.3 | Terrain inventory: **12 each** of 6"×6", 6"×3" and 3"×3" tiles; walls and doors/hatches from Kill Team and 40k Boarding Actions; a 3' × 5' dry-erase map. Enemy placement via tokens / poker chips. | Settled |
| 9.4 | Player faction: **Task Force Boone** (BLKOUT miniatures). Opposition from BLKOUT and BPRE miniatures. | Settled |
| 9.5 | Opposition: **Freespace Solutions**, a privatized debt and salvage corporation that unofficially conducts grey-zone operations for paying patrons. Three layers (patron, corporate core, contractors), element profiles, a miniature mapping across all BLKOUT/BPRE kits, and four threat tiers ([opposition-force.md](opposition-force.md)). | Settled |
| 9.6 | Freespace's current patron is the **Meridian Directorate**, a technocratic state run through its intelligence ministries. | Settled |
| 9.7 | Campaign thread: intelligence from Freespace operations builds toward **attributing** Freespace to the Meridian Directorate, which can escalate into a deniable war with the Directorate (peer naval-infantry foe). | Leaning |

## 10. Software

| # | Decision | Status |
|---|----------|--------|
| 10.1 | The repo stores everything: rules (Markdown), game content data, tools and application code. | Settled |
| 10.2 | Architecture per [ADR-001](../architecture/adr-001-architecture.md). | Settled |
| 10.3 | The app must eventually serve many players over the internet with shared campaign state. The PoC is single-user but modeled for multiplayer from day one. | Settled |
| 10.4 | The combat simulator is C# (`app/src/Imperator.Sim`) and uses the same game-logic library as the app, so the rules math exists once. | Settled |
| 10.5 | Content YAML is compiled to `content/dist/content.json` (`tools/build_content.py`); .NET reads JSON with the built-in serializer, so the app has no YAML dependency. Rulebook tables are generated from the same YAML (`tools/render_tables.py`). | Settled |
| 10.6 | Target .NET 10 (current LTS). Tests are a dependency-free console runner; CI checks content, tables and tests on every push. | Settled |
