import { sveltekit } from '@sveltejs/kit/vite'
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath } from 'node:url'
import { join, extname } from 'node:path'
import { readFile } from 'node:fs/promises'

const appBundleDir = fileURLToPath(
  new URL('../WasmEditor/bin/Debug/net10.0/browser-wasm/AppBundle', import.meta.url)
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

export default defineConfig({
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
            // Required for SharedArrayBuffer used by the .NET WASM runtime
            res.setHeader('Cross-Origin-Opener-Policy', 'same-origin')
            res.setHeader('Cross-Origin-Embedder-Policy', 'require-corp')
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
    headers: {
      'Cross-Origin-Opener-Policy': 'same-origin',
      'Cross-Origin-Embedder-Policy': 'require-corp',
    }
  }
})
