#!/usr/bin/env node
// Dev server for WasmPlayer — serves the Debug AppBundle with required COOP/COEP headers.
// Run: node dev-server.mjs
// Then open: http://localhost:5175/?game=/examples/simple.aslx

import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const isRelease = process.argv.includes('--release');
const config = isRelease ? 'Release' : 'Debug';
const appBundleDir = path.resolve(__dirname, `bin/${config}/net10.0/browser-wasm/AppBundle`);
const examplesDir = path.resolve(__dirname, '../../examples');
const port = 5175;

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.mjs': 'application/javascript',
  '.wasm': 'application/wasm',
  '.json': 'application/json',
  '.css': 'text/css',
  '.png': 'image/png',
  '.dat': 'application/octet-stream',
  '.blat': 'application/octet-stream',
  '.aslx': 'application/xml',
  '.asl': 'text/plain',
};

function serveFile(res, filePath) {
  fs.readFile(filePath, (err, data) => {
    if (err) {
      res.writeHead(404);
      res.end(`Not found: ${filePath}`);
      return;
    }
    const ext = path.extname(filePath);
    res.writeHead(200, {
      'Content-Type': mimeTypes[ext] ?? 'application/octet-stream',
      'Cache-Control': 'no-cache, no-store, must-revalidate',
    });
    res.end(data);
  });
}

const server = http.createServer((req, res) => {
  // Required for SharedArrayBuffer used by the .NET WASM runtime
  res.setHeader('Cross-Origin-Opener-Policy', 'same-origin');
  res.setHeader('Cross-Origin-Embedder-Policy', 'require-corp');

  let urlPath = req.url?.split('?')[0] ?? '/';
  if (urlPath === '/') urlPath = '/index.html';

  if (urlPath.startsWith('/examples/')) {
    const filePath = path.join(examplesDir, urlPath.slice('/examples/'.length));
    if (!filePath.startsWith(examplesDir)) { res.writeHead(403); res.end(); return; }
    serveFile(res, filePath);
    return;
  }

  const filePath = path.join(appBundleDir, urlPath);
  if (!filePath.startsWith(appBundleDir)) { res.writeHead(403); res.end(); return; }
  serveFile(res, filePath);
});

server.listen(port, () => {
  console.log(`WasmPlayer dev server (${config}) running at http://localhost:${port}/`);
  console.log(`Serving: ${appBundleDir}`);
  console.log(`Try: http://localhost:${port}/?game=/examples/simple.aslx`);
  console.log('Press Ctrl+C to stop.');
});
