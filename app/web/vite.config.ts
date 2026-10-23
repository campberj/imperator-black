/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, searchForWorkspaceRoot } from 'vite'

// The dev server proxies API routes to Imperator.Api (dotnet run --project app/src/Imperator.Api).
const api = process.env.IMPERATOR_API ?? 'http://localhost:5080'

// Keep API behaviour identical in the Vite development and production-preview servers.
const apiProxy = {
  '/content': api,
  '/values': api,
  '/fireteams': api,
  '/forces': api,
  '/health': api,
  '/decks': api,
  '/land': api,
  '/missions': api,
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: apiProxy,
    // The mission view reads the example missions from docs/scenarios/missions (outside app/web).
    fs: { allow: [searchForWorkspaceRoot(process.cwd()), '../../docs/scenarios/missions', '../../docs/scenarios/arcs'] },
  },
  preview: { proxy: apiProxy },
  test: {
    environment: 'jsdom',
    // The card-heavy Units regression needs CPU headroom when Planner tests run alongside it.
    maxWorkers: 2,
  },
})
