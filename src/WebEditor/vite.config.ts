import { sveltekit } from '@sveltejs/kit/vite'
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath } from 'node:url'
import { join, extname } from 'node:path'
import { readFile } from 'node:fs/promises'
import { readFileSync } from 'node:fs'

// Set WASM_CONFIG=Release to serve the AOT-compiled AppBundle instead of the Debug/interpreter one
// (e.g. for profiling, where AOT gives per-method native frames instead of one opaque interpreter loop).
const wasmConfig = process.env.WASM_CONFIG === 'Release' ? 'Release' : 'Debug'
const appBundleDir = fileURLToPath(
  new URL(`../WasmEditor/bin/${wasmConfig}/net10.0/browser-wasm/AppBundle`, import.meta.url)
)

const mimeTypes: Record<string, string> = {
  '.js': 'application/javascript',
  '.wasm': 'application/wasm',
  '.json': 'application/json',
  '.dll': 'application/octet-stream',
  '.dat': 'application/octet-stream',
  '.blat': 'application/octet-stream',
  '.pdb': 'application/octet-stream',
}

// Set VITE_API_PROXY=http://localhost:5043 (or your local textadventures.co.uk URL) to proxy
// /api requests during development. Not needed in production (same-origin).
const apiProxy = process.env.VITE_API_PROXY

// CI workflows set PUBLIC_WEBEDITOR_VERSION explicitly (to github.sha or github.ref_name);
// locally it's blank, so fall back to the repo-root VERSION file — the same source
// WasmPlayer's inject-version.mjs uses — so `npm run dev`/plain `npm run build` show a real version too.
if (!process.env.PUBLIC_WEBEDITOR_VERSION) {
  try {
    const versionFile = fileURLToPath(new URL('../../VERSION', import.meta.url))
    process.env.PUBLIC_WEBEDITOR_VERSION = readFileSync(versionFile, 'utf8').trim()
  } catch {
    // no VERSION file to read — banner falls back to "dev"
  }
}

export default defineConfig({
  // Vite clears the terminal on startup (and on file changes) by default,
  // which wipes out dev.sh's own printed URLs (Home/WasmPlayer/WasmEditor)
  // once Vite finishes starting last — this is the one place those URLs are
  // visible, so keep them on screen instead.
  clearScreen: false,
  plugins: [
    tailwindcss(),
    sveltekit(),
    {
      name: 'wasm-appbundle',
      configureServer(server) {
        server.middlewares.use('/AppBundle', async (req, res, next) => {
          try {
            const filePath = join(appBundleDir, req.url?.split('?')[0] ?? '')
            const data = await readFile(filePath)
            const ext = extname(filePath)
            res.setHeader('Content-Type', mimeTypes[ext] ?? 'application/octet-stream')
            res.end(data)
          } catch {
            next()
          }
        })
      }
    }
  ],
  server: {
    port: 5174,
    proxy: {
      // Proxy WasmPlayer through the same origin so BroadcastChannel works between editor and player tabs.
      '/player': {
        target: 'http://localhost:5175',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/player/, ''),
      },
      ...(apiProxy ? { '/api': { target: apiProxy, changeOrigin: true } } : {}),
    },
  }
})
