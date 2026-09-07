#!/usr/bin/env node
// Splices the repo-root VERSION string into generated/index.html, replacing
// the VERSION_SCRIPT placeholder comment in index.html, so wasm-player.js can
// print its startup banner immediately on page load — without waiting for the
// WASM runtime to boot (which only happens once a game starts loading).
//
// Also stamps that same version as a `?v=` query onto every relative asset URL
// in the document. That query is purely a cache key: it changes on every
// release, so play.questviva.com can serve those assets `immutable` (see the
// _headers block in .github/workflows/deploy-play.yml) instead of making the
// browser revalidate ~20 files on every single page load. wasm-player.js does
// the same for the assets it fetches from JS rather than from the markup.
//
// Run via `npm run build`, after build-icons.mjs has produced generated/index.html.

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(__dirname, '..');
const repoRoot = path.resolve(root, '..', '..');

const version = fs.readFileSync(path.join(repoRoot, 'VERSION'), 'utf8').trim();

const target = path.join(root, 'generated', 'index.html');
const html = fs.readFileSync(target, 'utf8');

const placeholder = '<!-- VERSION_SCRIPT';
const start = html.indexOf(placeholder);
if (start === -1) throw new Error('generated/index.html is missing the VERSION_SCRIPT placeholder');
const end = html.indexOf('-->', start);
if (end === -1) throw new Error('VERSION_SCRIPT placeholder comment is not closed with -->');

let output = html.slice(0, start)
    + `<script>window.QuestVivaVersion = ${JSON.stringify(version)};</script>`
    + html.slice(end + '-->'.length);

// Only relative paths ending in a static-asset extension, so this can't touch
// the icon sprite's `<use href="#folder-open">` fragment references or any
// absolute/data URL. The `[^"?#]+` also makes it idempotent — an already
// stamped URL contains a `?` and won't match a second time.
const assetUrl = /\b(src|href)="(?!https?:|\/\/|data:|#)([^"?#]+\.(?:js|css|svg))"/g;
let stampedCount = 0;
output = output.replace(assetUrl, (_match, attr, url) => {
    stampedCount++;
    return `${attr}="${url}?v=${encodeURIComponent(version)}"`;
});
if (stampedCount === 0) throw new Error('generated/index.html has no relative asset URLs to version-stamp');

fs.writeFileSync(target, output);
console.log(`Wrote generated/index.html with version ${version} (${stampedCount} asset URLs stamped)`);
