#!/usr/bin/env node
// Dev server for WasmPlayer — serves the Debug AppBundle.
// Run: node dev-server.mjs
// Then open: http://localhost:5175/?url=/examples/simple.aslx
//
// Proxy routes (dev only — avoids CORS issues during local development):
//   /api/                        → https://textadventures.co.uk/api/
//                                  (rewrites SourceGameUrl in JSON responses)
//   /game-resource/<encoded-url> → fetches the game file from the encoded URL

import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const isRelease = process.argv.includes('--release');
const config = isRelease ? 'Release' : 'Debug';
const appBundleDir = path.resolve(__dirname, `bin/${config}/net10.0/browser-wasm/AppBundle`);
const examplesDir = path.resolve(__dirname, '../../examples');
const e2eFixturesDir = path.resolve(__dirname, '../../tests/e2e/fixtures');
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
  '.svg': 'image/svg+xml',
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

const server = http.createServer(async (req, res) => {
  let urlPath = req.url?.split('?')[0] ?? '/';
  if (urlPath === '/') urlPath = '/index.html';

  if (urlPath.startsWith('/examples/')) {
    const filePath = path.join(examplesDir, urlPath.slice('/examples/'.length));
    if (!filePath.startsWith(examplesDir)) { res.writeHead(403); res.end(); return; }
    serveFile(res, filePath);
    return;
  }

  // Serves e2e regression-test fixture games (tests/e2e/fixtures/*.aslx) the
  // same way /examples/ does. Also checked as a fallback for bare resource
  // requests below (e.g. a fixture's <javascript src="foo.js">) — the
  // ?url= boot path never sets a resourceRoot, so those resolve relative to
  // this server's own origin rather than alongside the .aslx file.
  if (urlPath.startsWith('/e2e-fixtures/')) {
    const filePath = path.join(e2eFixturesDir, urlPath.slice('/e2e-fixtures/'.length));
    if (!filePath.startsWith(e2eFixturesDir)) { res.writeHead(403); res.end(); return; }
    serveFile(res, filePath);
    return;
  }

  // Serve quest-config.js with a dev-local API root so requests route through this proxy.
  // In production the deployed quest-config.js carries the absolute textadventures.co.uk URL.
  if (urlPath === '/quest-config.js') {
    const body = `window.QuestVivaConfig = { textAdventuresApiRoot: '/api/' };\n`;
    res.writeHead(200, { 'Content-Type': 'application/javascript', 'Cache-Control': 'no-cache' });
    res.end(body);
    return;
  }

  // Proxy /api/ → https://textadventures.co.uk/api/.
  // Rewrites SourceGameUrl in JSON responses so the game file also comes through this proxy.
  if (urlPath.startsWith('/api/')) {
    try {
      const target = `https://textadventures.co.uk${urlPath}`;
      const apiRes = await fetch(target);
      const contentType = apiRes.headers.get('content-type') ?? 'application/json';
      let body = await apiRes.text();
      if (contentType.includes('application/json')) {
        try {
          const json = JSON.parse(body);
          if (json.sourceGameUrl) {
            json.sourceGameUrl = `/game-resource/${encodeURIComponent(json.sourceGameUrl)}`;
          }
          if (json.resourceRoot) {
            json.resourceRoot = `/game-resource/${encodeURIComponent(json.resourceRoot)}`;
          }
          body = JSON.stringify(json);
        } catch { /* leave body as-is if JSON parse fails */ }
      }
      res.writeHead(apiRes.status, { 'Content-Type': contentType, 'Cache-Control': 'no-cache' });
      res.end(body);
    } catch (e) {
      res.writeHead(502);
      res.end(`Proxy error: ${e.message}`);
    }
    return;
  }

  // Proxy /game-resource/<encoded-url> → fetch the actual game file.
  // Used because the direct game file URL may also be blocked by CORS in dev.
  if (urlPath.startsWith('/game-resource/')) {
    const targetUrl = decodeURIComponent(urlPath.slice('/game-resource/'.length));
    try {
      const gameRes = await fetch(targetUrl);
      res.writeHead(gameRes.status, {
        'Content-Type': gameRes.headers.get('content-type') ?? 'application/octet-stream',
        'Cache-Control': 'no-cache',
      });
      res.end(Buffer.from(await gameRes.arrayBuffer()));
    } catch (e) {
      res.writeHead(502);
      res.end(`Proxy error: ${e.message}`);
    }
    return;
  }

  // A fixture's own external resources (e.g. restart-test.js) are requested
  // bare, relative to this server's origin — see the /e2e-fixtures/ comment
  // above. Check there before falling back to the AppBundle.
  const fixtureFilePath = path.join(e2eFixturesDir, urlPath);
  if (fixtureFilePath.startsWith(e2eFixturesDir) && fs.existsSync(fixtureFilePath)) {
    serveFile(res, fixtureFilePath);
    return;
  }

  const filePath = path.join(appBundleDir, urlPath);
  if (!filePath.startsWith(appBundleDir)) { res.writeHead(403); res.end(); return; }
  serveFile(res, filePath);
});

server.listen(port, () => {
  console.log(`WasmPlayer dev server (${config}) running at http://localhost:${port}/`);
  console.log(`Serving: ${appBundleDir}`);
  console.log(`Try: http://localhost:${port}/?url=/examples/simple.aslx`);
  console.log('Press Ctrl+C to stop.');
});
