# ADR-002: Sequential Turns and Local Campaign Infrastructure

**Status:** Proposed (RJ review required) · **Date:** 2026-10-23

## Context

[ADR-001](adr-001-architecture.md) already settles the append-only campaign event log (section 4), Core projections and multiplayer-shaped Player/Campaign/Membership records (section 5). Do not re-decide them here. [Decision 1.10](../design/decisions.md) settles **RJ's choice that sequential player turns apply to shared multiplayer campaigns only**. Solo campaigns continue to use the Chapter 14 campaign turn, and the **solo Phase 9 tracker must precede multiplayer**.

## Proposed additions

1. **Multiplayer sequential-turn authority.** The server holds the active actor for a shared campaign and must reject strategic commands from an inactive participant. The sequence of eligible actors, command allowance, turn-end triggers and idle-player policy remain **Open** rules decisions. Solo campaigns do **not** use this multiplayer turn machinery (settled decision 1.10).
2. **PostgreSQL development without mandatory Docker.** PostgreSQL remains authoritative under ADR-001. Developers may use native or managed PostgreSQL; local setup and migration checks must not require Docker. Docker Compose remains optional.
3. **Idempotent submitted commands.** A client can safely retry a submitted command using a durable campaign-scoped idempotency key. **A new column and uniqueness constraint (or an equivalently reviewed persistent data structure) are required**; this is not yet implemented. The event schema and API contracts require separate design and code review.

### Existing optimistic concurrency

The existing `campaign_events` table already has a **unique (campaign, sequence) index** and prohibits update/delete. An append expecting version `n` attempts to insert at `sequence = n + 1`; a uniqueness violation indicates a lost race. Transactions must handle that failure explicitly. This is an implementation expectation based on ADR-001, not a new strategic game rule.

## Scope and sequencing

- **S1–S2: Solo first.** Demonstrate database-backed solo campaign create/load/list and replay, then the Chapter 14 Foundry Moon campaign tracker. The paper Foundry Moon playthrough gates tracker gameplay screens.
- **S3+: Multiplayer later.** Only after the solo campaign loop works, add identity/membership enforcement and server-held turn handoffs. Preserve ADR-001's Core/API/PostgreSQL/React boundaries.
- Chapter 14's solo campaign turn and multiplayer sequential player turns remain **distinct**.
- The first strategic map is the existing Chapter 14 **six-node arc map**, with links and revealed/hidden status in `content/campaign.yaml`. Expanding it into a larger sector map requires further design.
- Fully automated digital tactical combat is **out of scope under decision 8.2 and existing pillars**; RJ may reopen that scope separately.

See the single [project roadmap](../roadmap.md) for Phase 9 and post-PoC stages.

## Open strategic decisions (not implemented by this ADR)

| ID | Open question | Dependency |
|---|---|---|
| D1 | How multiplayer strategic turns relate to campaign time and completed tabletop missions | Multiplayer time/aftermath |
| D2 | Multiplayer actor order, faction representation and eligibility | Multiplayer turn authority |
| D3 | Strategic commands, allowances, interruptions and end-turn criteria | Multiplayer orders |
| D4 | Extensions beyond the approved Chapter 14 arc-map geography, visibility and location control | Larger sector layer |
| D5 | Movement, travel, supply and detection for forces/vessels | Ship/force movement |
| D6 | When a strategic encounter generates a tabletop mission and how turns resume | Encounter integration |
| D7 | Result submissions, disputes, corrections and who can confirm an outcome | Shared outcomes |
| D8 | Computer-controlled *strategic* opponent behavior, distinct from existing tabletop guidance | Extended solo AI |
| D9 | Idle participants, deadlines, delegation and administrators | Asynchronous multiplayer |
| D10 | Larger strategic victory/end conditions and resource effects | Full strategic campaign |

**Not open (formerly D11):** Fully digital tactical combat is currently **Settled: out of scope** (decision 8.2 and the tabletop-first pillars); RJ may explicitly reopen it.

## Review gate

ADR-002 remains **Proposed**, including the local PostgreSQL/idempotency additions. Decision 1.10 is already settled. Approval of this document does not approve new game mechanics or authorise app implementation; each stage needs separate tasks, acceptance criteria and green CI.
