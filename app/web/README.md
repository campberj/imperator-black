# Imperator Black web app

React + TypeScript (Vite). Right now it is a smoke test of the content pipeline: it fetches `/content` from `Imperator.Api` and renders the weapon and armour tables, with the same columns as the generated rulebook tables.

```sh
# from the repo root, with the API running (see app/README.md)
npm --prefix app/web install
npm --prefix app/web run dev       # http://localhost:5173, proxies /content and /health to http://localhost:5080
npm --prefix app/web run lint      # oxlint
npm --prefix app/web run build     # type-check and production build
```

Set `IMPERATOR_API` to point the dev proxy at a different API address.
