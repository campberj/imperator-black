# Native PostgreSQL development on macOS (no Docker)

S1 stores solo campaigns in PostgreSQL; the generators, Units and Force Planner continue to work
without the database. The native setup below uses PostgreSQL **18**, matching the CI service
and the existing EF migration. Do not run a second PostgreSQL instance on port 5432.

## 1. Install and start PostgreSQL

From any Terminal window:

```sh
brew install postgresql@18
brew services start postgresql@18
PG_BIN="$(brew --prefix postgresql@18)/bin"
"$PG_BIN/pg_isready" -h localhost -p 5432
```

The service is managed by macOS through Homebrew, independently of the API and web servers.
If port 5432 is already occupied, inspect it before starting another database:
`lsof -nP -iTCP:5432 -sTCP:LISTEN`.

## 2. Create the local development role and database (once)

The existing `appsettings.Development.json` uses
`Host=localhost;Port=5432;Database=imperator;Username=imperator;Password=imperator`.
Use that role only for local development; it is not a production credential.

```sh
PG_BIN="$(brew --prefix postgresql@18)/bin"
"$PG_BIN/psql" -d postgres -tAc "SELECT 1 FROM pg_roles WHERE rolname='imperator'" | grep -q 1 ||
  "$PG_BIN/psql" -d postgres -c "CREATE ROLE imperator WITH LOGIN PASSWORD 'imperator';"
"$PG_BIN/psql" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='imperator'" | grep -q 1 ||
  "$PG_BIN/createdb" -O imperator imperator
PGPASSWORD=imperator "$PG_BIN/psql" -h localhost -U imperator -d imperator -c "SELECT current_user, current_database();"
```

The `imperator` role owns its database. These commands leave an existing role/database alone;
they do **not** overwrite or reset previously saved campaigns. If the role already exists
with a *different* password, do not change it blindly: instead use a suitable
`ConnectionStrings__Imperator` environment override or review your local role setup.

## 3. Apply existing migrations and start the API

From the repository root, in Terminal 1:

```sh
git switch main
git pull --ff-only
dotnet tool restore
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project app/src/Imperator.Api
dotnet run --project app/src/Imperator.Api --launch-profile http
```

Leave the API running on `http://localhost:5080`. In Terminal 2:

```sh
npm --prefix app/web ci
npm --prefix app/web run dev -- --host 127.0.0.1 --strictPort
```

Open `http://localhost:5173` for the existing web tools. S1's campaign API has **no UI
yet**; the first solo game screens belong to the *paper-playtest-gated* S2.

## 4. Verify a persistent solo campaign

In Terminal 3, start with the API running:

```sh
curl -i http://localhost:5080/health
curl -fsS -H "Content-Type: application/json" -d '{"name":"Foundry Moon"}' \
  http://localhost:5080/campaigns
curl -fsS http://localhost:5080/campaigns
```

The POST returns `201 Created` and JSON with `id`, `name`, `contentVersion`, and
`version: 1`. Copy the returned `id` into the commands below:

```sh
ID="<paste-campaign-id>"
curl -fsS "http://localhost:5080/campaigns/$ID"
curl -fsS "http://localhost:5080/campaigns/$ID/events"
curl -fsS -X PATCH "http://localhost:5080/campaigns/$ID/name" \
  -H "Content-Type: application/json" -d '{"name":"Foundry Moon — continuing"}'
```

Stop the API with Control+C; **leave PostgreSQL running**. Restart the API and repeat
the GET requests. You should see the renamed campaign at `version: 2` and exactly
two ordered immutable lifecycle events. S1 intentionally stores *no* mission results,
injuries, XP, movement, resources or outpost rules.

### Back up the database

```sh
PG_BIN="$(brew --prefix postgresql@18)/bin"
PGPASSWORD=imperator "$PG_BIN/pg_dump" -h localhost -U imperator -d imperator -Fc \
  -f "$HOME/imperator-local.backup"
```

To test a restore, create a **separate empty database** (never restore over your
working campaigns) and import the backup there:

```sh
PG_BIN="$(brew --prefix postgresql@18)/bin"
"$PG_BIN/createdb" -O imperator imperator_restore
PGPASSWORD=imperator "$PG_BIN/pg_restore" -h localhost -U imperator -d imperator_restore \
  --no-owner "$HOME/imperator-local.backup"
PGPASSWORD=imperator "$PG_BIN/psql" -h localhost -U imperator -d imperator_restore \
  -c "SELECT name FROM campaigns;"
```

Keep the backup file private; campaign content and future player details are user data.
Delete the **test** database only when no longer needed:
`"$PG_BIN/dropdb" imperator_restore`. Never run `dropdb imperator` as part of setup.

## Security and testing notes

- S1 campaign routes are available **only in ASP.NET Core Development**, using one
  server-configured local player ID. This is *not authentication*. Keep the API bound
  to localhost and do not expose it to other users or networks.
- A different `SoloCampaigns__LocalPlayerId` intentionally selects another local
  player's campaigns; keep the configured ID stable to resume saves.
- PostgreSQL is **not** required for `/content`, `/values`, `/fireteams`,
  `/forces/check`, mission generation or other existing reference screens.
- The Core suite runs without a database. To run the real PostgreSQL integration
  test locally, set `IMPERATOR_TEST_CONNECTION` to an **isolated disposable test database**
  connection string, then run `dotnet test app/Imperator.slnx`. The test runs EF
  migrations and writes test records. CI always runs it against its ephemeral service.
- The eventual hosted and multiplayer APIs need proper authentication and
  per-player authorisation before these routes can be exposed outside Development.

This guide uses `brew services start postgresql@18`, as documented by
[Homebrew's PostgreSQL 18 formula](https://formulae.brew.sh/formula/postgresql@18).
