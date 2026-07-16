#!/usr/bin/env node
// Dev server for the Home page — plain static file serving, no build step.
// Run: node dev-server.mjs
// Then open: http://localhost:5176/
//
// No API proxy is needed here (unlike WasmPlayer's dev-server.mjs): the
// catalog API's CORS policy already allows any http://localhost/127.0.0.1
// origin (see CorsUtility.IsAllowedGamesApiOrigin in textadventures.co.uk),
// specifically so this page can call it directly from local dev.

import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '../..');
const port = 5176;

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.svg': 'image/svg+xml',
};

const server = http.createServer((req, res) => {
  let urlPath = req.url?.split('?')[0] ?? '/';
  if (urlPath === '/') urlPath = '/index.html';

  // The real deploy copies the repo-root VERSION file alongside Home's static
  // assets (see .github/workflows/deploy-play.yml) — mirror that here so
  // home.js's fetch('VERSION') behaves the same in dev.
  const filePath = urlPath === '/VERSION'
    ? path.join(repoRoot, 'VERSION')
    : path.join(__dirname, urlPath);

  fs.readFile(filePath, (err, data) => {
    if (err) {
      res.writeHead(404);
      res.end(`Not found: ${filePath}`);
      return;
    }
    res.writeHead(200, {
      'Content-Type': mimeTypes[path.extname(filePath)] ?? 'text/plain',
      'Cache-Control': 'no-cache, no-store, must-revalidate',
    });
    res.end(data);
  });
});

server.listen(port, () => {
  console.log(`Home dev server running at http://localhost:${port}/`);
  console.log('Press Ctrl+C to stop.');
});
