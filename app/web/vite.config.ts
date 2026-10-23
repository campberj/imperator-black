/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, searchForWorkspaceRoot } from 'vite'

// The dev server proxies API routes to Imperator.Api (dotnet run --project app/src/Imperator.Api).
const api = process.env.IMPERATOR_API ?? 'http://localhost:5080'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/content': api,
      '/values': api,
      '/fireteams': api,
      '/health': api,
      '/decks': api,
      '/land': api,
      '/missions': api,
    },
    // The mission view reads the example missions from docs/scenarios/missions (outside app/web).
    fs: { allow: [searchForWorkspaceRoot(process.cwd()), '../../docs/scenarios/missions', '../../docs/scenarios/arcs/foundry-moon'] },
  },
  test: {
    environment: 'jsdom',
  },
})
