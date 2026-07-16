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
// No API proxy is needed for the catalog fetch itself (unlike WasmPlayer's
// dev-server.mjs): its CORS policy already allows any http://localhost/
// 127.0.0.1 origin (see CorsUtility.IsAllowedGamesApiOrigin in
// textadventures.co.uk), specifically so this page can call it directly from
// local dev.

import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '../..');
const port = 5176;

const editorOrigin = 'http://localhost:5174';
const proxies = {
  '/player': 'http://localhost:5175',
};

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.svg': 'image/svg+xml',
};

async function proxyRequest(req, res, prefix, targetOrigin) {
  const rest = req.url.slice(prefix.length) || '/';
  try {
    // No need to forward the incoming request's headers (host, cookies, etc.)
    // — these are same-machine GETs for another dev server's own static
    // assets/HTML, not authenticated or content-negotiated requests.
    const upstream = await fetch(`${targetOrigin}${rest}`);
    const body = Buffer.from(await upstream.arrayBuffer());
    res.writeHead(upstream.status, {
      'Content-Type': upstream.headers.get('content-type') ?? 'application/octet-stream',
    });
    res.end(body);
  } catch (e) {
    res.writeHead(502);
    res.end(`Proxy error (${targetOrigin}${rest}): ${e.message}`);
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
  console.log(`  proxying /player -> ${proxies['/player']}`);
  console.log(`  redirecting /editor -> ${editorOrigin}`);
  console.log('Press Ctrl+C to stop.');
});
