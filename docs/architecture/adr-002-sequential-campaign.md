# ADR-002: Campaign Game Infrastructure and Shared Sequential Turns

**Status:** Proposed — for RJ's architecture review  
**Date:** 2026-10-23

## Context

The app is the **campaign game**, not merely a tabletop tracker (settled decisions [1.11, 7.14 and 7.15](../design/decisions.md)). Players experience authored or procedurally generated story, decisions, taskings and outpost development in the app; combat missions are played with the printed tabletop rules. See the authoritative [campaign-game design note](../design/campaign-game.md). Every gameplay rule automated by the app must be specified in `rules/` or `content/`; no new gameplay mechanics are approved by this ADR.

[ADR-001 §§4–5](adr-001-architecture.md) **already establish** append-only campaign events, Core projections, PostgreSQL, the multiplayer-shaped Player/Campaign/Membership schema and authentication deferred until later. This ADR does not restate or supersede those choices.

## Decisions and proposals

1. **Sequential turns — already settled (decision 1.10).** Only **shared multiplayer campaigns** use sequential strategic player turns, with one server-held active actor at a time. Solo campaigns continue to use the Chapter 14 campaign turn; they do **not** use shared multiplayer turn machinery. Who may act, which orders are legal, turn order, ending conditions and idle-player handling remain to be designed before multiplayer implementation.
2. **Local PostgreSQL — proposed implementation requirement.** PostgreSQL-backed saves must be usable with a native or managed development database **without installing Docker**. Docker Compose remains an optional convenience, not a prerequisite.
3. **Command idempotency — proposed implementation requirement for shared campaigns.** Clients need a persistent, campaign-scoped idempotency key for retryable commands. This **requires a new column/unique index or another explicitly reviewed durable schema change**; it does not exist in the current event model.

The existing `campaign_events` **unique (campaign, sequence) index** already provides optimistic append concurrency. A command based on version `n` attempts to append at `sequence = n + 1`; a unique-key collision means another writer won. The API must handle that case safely. This mechanism and the existing event log originate in ADR-001, not a newly invented turn rule.

## Development gates

The [single project roadmap](../roadmap.md) and [campaign-game design note](../design/campaign-game.md) control sequencing:

- **S1:** Persistent **one-player** campaign create, list, load, projection/replay and restart verification.
- **S2:** Playable **bespoke Foundry Moon campaign**—story text, grade-based branches, force deployment, debrief, injuries, XP and consequences. **Complete and review the paper Foundry Moon playthrough before S2 gameplay screens.**
- **S3:** A **procedural adventure campaign**, drawing on Chapter 14, with generated story text, outpost hub and choices. **Outpost rules and a bespoke/procedural campaign content schema must first be designed and approved**; the [design note's open questions](../design/campaign-game.md) remain with RJ.
- **Later, after the solo game and PoC validation:** Player identity, shared memberships, sequential active-actor handoff and idempotency, followed by expanded strategic assets/maps and encounters when their rules exist.

The initial strategic map builds on Chapter 14's **six-node arc map** (links, revealed/hidden nodes), not an invented sector geography. Outposts are **distinct from ships** (decision 7.14); their facilities, economy, placement and defences are not yet specified.

**Fully automated digital tabletop combat is out of scope** under decision 8.2 and the tabletop-first design pillars; RJ may reopen that separately. A richer *campaign* video game does not mean an automated tabletop combat engine.

## Unresolved strategic design questions

| ID | Unsettled design question | Gate |
|---|---|---|
| D1 | How a shared strategic turn relates to campaign time and tabletop aftermath (not the solo Chapter 14 turn) | Multiplayer |
| D2 | Shared campaign faction/actor eligibility, turn order and player authority | Multiplayer |
| D3 | Valid strategic orders, action budgets, interrupts and end-turn criteria | Multiplayer commands |
| D4 | Extensions **beyond the approved six-node arc map**, including larger-sector visibility, ownership and geography | Large sector |
| D5 | Ship/ground force movement, travel, supply and detection | Strategic orders |
| D6 | Encounter generation and return from a tabletop mission | Shared encounters |
| D7 | Review, correction and dispute procedures for submitted battle outcomes | Shared outcomes |
| D8 | Strategic solo commander decision rules and what information it may use; decision 1.11 sets the **role**, not its algorithm | Procedural adventures |
| D9 | Idle players, delegation, timing and administrator intervention | Asynchronous multiplayer |
| D10 | Strategic campaign end conditions and victory/resource effects | Larger campaign |

Additionally, the design note leaves **outpost-versus-ship start, map placement, solo command latitude and procedural narration style** for RJ to settle. These are not invented in app code. Former D11 (**fully digital tabletop combat**) is **settled out of scope**, not an open design task.

## Approval

This ADR remains **Proposed**. RJ has already settled decisions 1.10, 1.11, 7.14 and 7.15; review of ADR-002 covers only these technical extensions and their integration. No app work or new game mechanics follow automatically from this proposal. Each implementation slice requires its own scoped task, tests, green CI and review.
