/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The dev server proxies API routes to Imperator.Api (dotnet run --project app/src/Imperator.Api).
const api = process.env.IMPERATOR_API ?? 'http://localhost:5080'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/content': api,
      '/health': api,
      '/decks': api,
    },
  },
  test: {
    environment: 'jsdom',
  },
})
