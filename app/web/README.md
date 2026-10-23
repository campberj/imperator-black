# Imperator Black web app

React + TypeScript (Vite). It fetches `/content` from `Imperator.Api` once and shows it read-only:

- **Units** (`#units`): troop profiles as cards laid out like `docs/print/ImperatorBlack_unit-cards.pdf`, filtered by faction, rank, role and keyword, with a search across names and rules text; then the faction's teams (player) or elements (enemy) in the rulebook's table format.
- **Reference** (`#reference`): gear, skills (levels and Proposed status), weapon and armour keywords, and the weapon, grenade and armour tables, searchable.
- **Behavior cards** (`#behaviors`): the three chapter 10 reference cards and one card per behavior profile, as in `docs/print/ImperatorBlack_behavior-cards.pdf`.
- **Missions** (`#missions?setting=ship&seed=42`, or `#missions?example=mission-ship-42`): a chapter 13 mission from `GET /missions` (setting, mission, tier, vessel or environment, size and seed pickers; "Rolled" leaves a choice to the dice), or one of the example missions in `docs/scenarios/missions/`, read from the YAML itself (Vite `import.meta.glob`, parsed with `yaml` on demand). Laid out like the printed mission sheet: briefing and map (guard, contact chips, objectives, enemy entry), then how it plays (taking it, lockdown, grades, the alert track and clock from content, contact rules, the tier's contact table, the enemy elements with drill), then the ground (rooms and doors, or sectors; terrain; rolls). Prints as three pages. Its **Dashboard** button opens the mission's dashboard.
- **Dashboard** (`#dashboard`, or with a mission's parameters: `#dashboard?setting=ship&seed=42`, `#dashboard?example=mission-ship-42`): the paper mission dashboard (`dashboard_flow` in `tools/print_scenario.py`) as one printable letter page: the alert track (start and thresholds shaded), the clock with a box a minute (check-ins and the limit shaded), rounds played, objectives, contact chips, the guard and reinforcement elements, and the player force (8 rows, 12 for a large mission). `#dashboard` alone is the blank one (30-minute clock, both sizes named). No saved state.
- **Land tables** (`#land?environment=woodland&size=large&seed=59`): a chapter 12 table from `GET /land/{environment}?size=…&seed=N`, drawn with `BoardMap`: sectors, terrain by type, conditions, the sectors and terrain tables, and the roll log. Every environment can be named, Fortified included.
- **Deck maps** (`#decks?vessel=corvette&seed=5`): a chapter 11 deck from `GET /decks/{vessel}?seed=N`, drawn as SVG with the printed deck sheets' conventions (`docs/print/decks/`, `tools/print_scenario.py`): the 3" grid, tiles with ids, names and sizes, cover with ids, doors coloured by state (locked marked (L), security on hover and in the doors table), the airlock and the objective. Rooms, doors and terrain tables sit beside the map. Pick a vessel, type a seed or step to the next one; the hash holds both, so a deck can be shared by link. Printing gives the map on one page and the tables after it.

Every word comes from content `name` and `text` fields; labels and connecting phrases follow the printed cards (`tools/print_sheets.py`) and the rulebook renderer (`tools/render_tables.py`). Views are addressed by the URL hash, so there is no router dependency. Printing a page prints just the cards and tables. Tests render the views from the real `content/dist/content.json` (`src/test-content.ts`), so they fail if the views and content diverge.

```sh
# from the repo root, with the API running (see app/README.md)
npm --prefix app/web install
npm --prefix app/web run dev       # http://localhost:5173, proxies /content, /health, /decks, /land and /missions to http://localhost:5080
npm --prefix app/web run lint      # oxlint
npm --prefix app/web run build     # type-check and production build
```

Set `IMPERATOR_API` to point the dev proxy at a different API address.

### If Units, Fireteams or Force Planner return 404

The web app requires **one current .NET API process** serving all four routes:
`GET /content`, `GET /values`, `GET /fireteams` and `POST /forces/check`.
`/content` alone is not a sufficient check because older API binaries also serve it.

From the repo root, with **both the .NET API and Vite running**, diagnose the actual local processes:

```sh
python3 tools/check_app_endpoints.py
```

It checks the API on port 5080 **and** the Vite proxy on port 5173,
and identifies which side is missing the route. No dependencies beyond Python 3.
If the direct API reports 404, first inspect `lsof -nP -iTCP:5080 -sTCP:LISTEN`
on macOS; terminate the **stale API process**, not unrelated processes.
Start the current API with `dotnet run --project app/src/Imperator.Api`.
If direct API checks pass and only the proxy fails, restart the Vite dev server
and check `IMPERATOR_API`. Both `npm --prefix app/web run dev` and `npm --prefix app/web run preview`
now share the same API proxy; restart the process after pulling the fix.



`src/components/BoardMap.tsx` draws any board in the scenario format (`src/board.ts`): a deck, a land table or a mission. Every part but the board size is optional: tiles, doors and the airlock; the 6" land grid, 12" sectors named from A1 at the player's bottom left, terrain coloured by type (light, heavy, hill, area, ditch, water, wire); deployment; numbered objectives, guard models, contact chips (`?n`) and the enemy entry (`EE`). `BoardLegend` lists what a board shows, with the printed sheets' names. Colours are in `src/boardStyle.ts`.

Tests read the example missions straight from `docs/scenarios/missions/*.yaml` (`src/test-missions.ts`). `src/fixtures/deck-freighter-7.json` is `GET /decks/freighter?seed=7`, saved for the deck map tests (it has open, closed and locked doors and an objective). Refresh it with `curl -s 'localhost:5080/decks/freighter?seed=7' | python3 -m json.tool --indent 2 > app/web/src/fixtures/deck-freighter-7.json` if the deck format changes.
