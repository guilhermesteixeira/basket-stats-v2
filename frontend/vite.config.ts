import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'icons/*.png'],
      manifest: {
        name: 'Basket Stats',
        short_name: 'BasketStats',
        description: 'Basketball statistics recording app',
        theme_color: '#1d4ed8',
        background_color: '#0f172a',
        display: 'standalone',
        icons: [
          { src: 'icons/icon-192.png', sizes: '192x192', type: 'image/png' },
          { src: 'icons/icon-512.png', sizes: '512x512', type: 'image/png' },
        ],
      },
      workbox: {
        runtimeCaching: [
          {
            urlPattern: /\/api\/matches$/,
            handler: 'StaleWhileRevalidate',
            options: { cacheName: 'matches-list', expiration: { maxAgeSeconds: 300 } },
          },
          {
            urlPattern: /\/api\/matches\/.+$/,
            handler: 'StaleWhileRevalidate',
            options: { cacheName: 'match-detail', expiration: { maxAgeSeconds: 30 } },
          },
        ],
      },
    }),
  ],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
})
