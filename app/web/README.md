# Imperator Black web app

React + TypeScript (Vite). It fetches `/content` from `Imperator.Api` once and shows it read-only:

- **Units** (`#units`): troop profiles as cards laid out like `docs/print/ImperatorBlack_unit-cards.pdf`, filtered by faction, rank, role and keyword, with a search across names and rules text; then the faction's teams (player) or elements (enemy) in the rulebook's table format.
- **Reference** (`#reference`): gear, skills (levels and Proposed status), weapon and armour keywords, and the weapon, grenade and armour tables, searchable.
- **Behavior cards** (`#behaviors`): the three chapter 10 reference cards and one card per behavior profile, as in `docs/print/ImperatorBlack_behavior-cards.pdf`.

Every word comes from content `name` and `text` fields; labels and connecting phrases follow the printed cards (`tools/print_sheets.py`) and the rulebook renderer (`tools/render_tables.py`). Views are addressed by the URL hash, so there is no router dependency. Printing a page prints just the cards and tables. Tests render the views from the real `content/dist/content.json` (`src/test-content.ts`), so they fail if the views and content diverge.

```sh
# from the repo root, with the API running (see app/README.md)
npm --prefix app/web install
npm --prefix app/web run dev       # http://localhost:5173, proxies /content and /health to http://localhost:5080
npm --prefix app/web run lint      # oxlint
npm --prefix app/web run build     # type-check and production build
```

Set `IMPERATOR_API` to point the dev proxy at a different API address.
