# The campaign game: design note

Status: **Proposed** (2026-10-23; RJ's answers 2026-10-27). Decisions 1.11, 7.14 and 7.15. Nothing here is built yet.

## The vision (RJ)

The campaign plays like a video game: players read the story, make strategic decisions and build up an outpost in the app, then play the combat missions on the tabletop. This is the original design document's vision. Outside combat, the app runs everything: the crew, travel, narrative events, communication with command. In single-player campaigns the app acts as commander; in multiplayer, players plot courses and fight over bases and planets.

RJ's calls (2026-10-23):

- **The app may have things paper doesn't** (decision 1.11): branching story text, automated commander decisions, outpost management. The tabletop missions stay printed-rules play. Every rule the app applies is still written down in `rules/` or `content/`; the app never invents one.
- **Outposts are a separate layer** (decision 7.14): an established outpost the player builds up with facilities, distinct from the ship. These are new rules, to be designed.
- **Two sources of campaigns** (decision 7.15): **bespoke** campaigns, authored like Foundry Moon, and **procedurally generated adventure** campaigns, built from tables like chapter 14's arcs. Story text comes from whichever the campaign uses.

## What already exists to build on

| Piece | Where | Becomes |
|---|---|---|
| Campaign turn, arcs, Standing, Requisition, Intel, Evidence, events, injuries, XP, advances, ship upgrades | Chapter 14, `content/campaign.yaml` | The engine of procedural adventure campaigns |
| Short arcs: authored missions, branches by grade, the arc sheet | Chapter 14 *Short arcs*, `docs/scenarios/arcs/` | The first bespoke campaign format |
| Missions, boards, opposition tables | Chapters 11–13, generators in Core | The tabletop half of every campaign turn |
| Fireteams, troop values, force selection, force planner | Chapters 9, 14, 15; Core | Deploying the force for each mission |
| Event log, Campaign, Player, Membership | ADR-001 §4–5, the database | Saved campaign state; history feed |

## The loop, as a player sees it

1. **Hub:** the outpost and the ship. Roster, injuries and recovery, facilities, resources, messages from command.
2. **The situation:** the sector or arc map, with text for each place, and command's tasking (in solo, the app is the commander).
3. **A decision:** which node to strike, what to spend, what to build, how to answer an event. Each choice shows its cost and its rule.
4. **Deployment:** choose the force (force planner rules) and support; the app generates or loads the mission and prints its sheet and cards.
5. **The tabletop:** play the mission on paper, with the printed dashboard or, later, the live one.
6. **Debrief:** enter the grade and casualties; the app rolls injuries and XP, applies rewards, and writes the aftermath text.
7. **Consequences:** events, the next turn, the story moves on.

## Content the app needs that doesn't exist yet

- **A campaign definition** (bespoke or procedural): its start, its map or arc structure, its story nodes, how it ends. Short arcs become one kind of bespoke campaign.
- **Story text:** per node, per mission outcome, per event, per message from command. Bespoke campaigns author it; procedural campaigns assemble it from tables (Five Parsecs style), with slots filled from the campaign state (places, names, the enemy).
- **Outpost rules:** facilities, what they cost and do, upkeep, how outposts are founded, threatened and lost. New rules chapter or a chapter 14 section.
- **Choices with consequences:** an event or story node offers options; each option has a cost, a roll or a result written as content (no rule in code).

## Proposed order

1. **Design first:** RJ answers the questions below; then an outpost rules draft and a campaign content schema (bespoke and procedural), in `content/`, validated like everything else.
2. **App S1:** persistence and a one-player campaign (create, save, load, replay).
3. **App S2:** Foundry Moon as the first bespoke campaign in the app: story text, branches, deployment, debrief, injuries and XP. Screens gated on the paper Foundry Moon playthrough.
4. **App S3:** a procedural adventure campaign (chapter 14 arcs) with generated story text, the outpost hub, events with choices.
5. **Later:** multiplayer (decision 1.10), a larger sector layer, the live mission dashboard.

## RJ's answers (2026-10-27)

1. **The base is a land outpost or a ship, by campaign.** Each campaign gives the force one home base: an outpost on the ground or a ship that travels. Chapter 14's ship upgrade tracks become the ship kind of base; a land outpost gets its own facilities.
2. **The base is on the map and can be attacked.** It is a place on the arc or sector map, and an attack on it is a tabletop mission (a defence mission type, still to write).
3. **The commander's latitude varies by campaign.** A campaign definition sets it, from chapter 14's Black Ops latitude (choose targets while Standing holds) to tight orders.
4. **Procedural story text is short military reports for now:** taskings, briefings, sitreps and after-action summaries.

## Still to design

- **Base rules:** facilities for each kind of base, costs, upkeep, founding or assignment, what an attack threatens, losing and rebuilding.
- **A defence mission** (chapter 13): the enemy attacks the base; the player holds it.
- **The campaign definition schema:** start, base kind, command latitude, map or arc structure, story nodes and text, end conditions.
- **Report templates** for procedural text: taskings, briefings, sitreps, after-action summaries, with slots filled from the campaign state.
