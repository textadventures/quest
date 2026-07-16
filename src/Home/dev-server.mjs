#!/usr/bin/env node
// Dev server for the Home page — plain static file serving, no build step.
// Run: node dev-server.mjs
// Then open: http://localhost:5176/
//
// Also proxies /player through to the WasmPlayer dev server (port 5175), the
// same trick WebEditor's own vite.config.ts already uses to proxy /player to
// WasmPlayer — so Home's game-card links (player/?id=...) work under one
// origin locally the same way they do in production, without a real reverse
// proxy or extra deps. /editor redirects to Vite's own origin (port 5174)
// instead of proxying: Vite dev mode emits root-absolute module URLs
// (/node_modules/..., /@vite/client) that assume they're served from their
// own true root, so a path-prefix proxy breaks the dynamic import graph —
// confirmed via a "Failed to fetch dynamically imported module" error when
// this was tried. WasmPlayer has no such issue (all its dev-server assets are
// already relative), which is exactly why /player can be proxied but /editor
// can't. dev.sh starts all three together; this is the primary entry point.
//
// /api and /game-resource are also proxied to WasmPlayer's dev server, even
// though they're not under /player: when a game is loaded via ?id=,
// WasmPlayer serves its own dev-only quest-config.js (see its dev-server.mjs)
// pointing textAdventuresApiRoot at the root-relative '/api/' — a leftover
// CORS workaround from before CorsUtility.IsAllowedGamesApiOrigin allowed
// direct localhost calls. That path is resolved against whatever origin the
// browser thinks it's on (Home's, once proxied under /player/), not
// WasmPlayer's own — so without this, /player/?id= 404s fetching the game.
// Home's own catalog fetch doesn't need this: it calls the real
// textadventures.co.uk API directly, which CORS already permits.

import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '../..');
const port = 5176;

const editorOrigin = 'http://localhost:5174';
const wasmPlayerOrigin = 'http://localhost:5175';
const proxies = {
  // /player namespaces WasmPlayer's own root content under a prefix here, so
  // that prefix must be stripped before forwarding (WasmPlayer itself has no
  // concept of "/player/"). /api and /game-resource are WasmPlayer's own
  // literal route names (its dev-server.mjs matches on them directly, e.g.
  // `urlPath.startsWith('/api/')`) — those must be forwarded unchanged.
  '/player': { target: wasmPlayerOrigin, stripPrefix: true },
  '/api': { target: wasmPlayerOrigin, stripPrefix: false },
  '/game-resource': { target: wasmPlayerOrigin, stripPrefix: false },
};

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.svg': 'image/svg+xml',
};

async function proxyRequest(req, res, prefix, { target, stripPrefix }) {
  const rest = stripPrefix ? (req.url.slice(prefix.length) || '/') : req.url;
  try {
    // No need to forward the incoming request's headers (host, cookies, etc.)
    // — these are same-machine GETs for another dev server's own static
    // assets/HTML, not authenticated or content-negotiated requests.
    const upstream = await fetch(`${target}${rest}`);
    const body = Buffer.from(await upstream.arrayBuffer());
    res.writeHead(upstream.status, {
      'Content-Type': upstream.headers.get('content-type') ?? 'application/octet-stream',
    });
    res.end(body);
  } catch (e) {
    res.writeHead(502);
    res.end(`Proxy error (${target}${rest}): ${e.message}`);
  }
}

function serveStatic(req, res) {
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
}

function matchesPrefix(url, prefix) {
  return url === prefix || url.startsWith(`${prefix}/`) || url.startsWith(`${prefix}?`);
}

const server = http.createServer((req, res) => {
  if (matchesPrefix(req.url, '/editor')) {
    res.writeHead(302, { Location: editorOrigin });
    res.end();
    return;
  }

  const prefix = Object.keys(proxies).find(p => matchesPrefix(req.url, p));
  if (prefix) {
    void proxyRequest(req, res, prefix, proxies[prefix]);
    return;
  }
  serveStatic(req, res);
});

server.listen(port, () => {
  console.log(`Home dev server running at http://localhost:${port}/`);
  for (const [prefix, { target, stripPrefix }] of Object.entries(proxies)) {
    console.log(`  proxying ${prefix} -> ${target}${stripPrefix ? ' (prefix stripped)' : ''}`);
  }
  console.log(`  redirecting /editor -> ${editorOrigin}`);
  console.log('Press Ctrl+C to stop.');
});
