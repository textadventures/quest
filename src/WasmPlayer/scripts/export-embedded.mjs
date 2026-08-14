#!/usr/bin/env node
// Generates a single self-contained HTML file for a .quest/.aslx game: the
// WasmPlayer runtime loads from jsDelivr (the `@textadventures/quest-viva-
// wasmplayer` package published by npm-publish.yml) via a <base href> tag,
// and the game bytes are inlined as base64 — nothing else to host or upload.
// See docs/wasmplayer-single-file-export.md for the full design.
//
// Usage: node scripts/export-embedded.mjs <game-file> [version] [output-file]
//   game-file    Path to a .quest or .aslx file.
//   version      npm package version to pin to (defaults to repo VERSION).
//   output-file  Defaults to <game-file-basename>.html next to the input.
//
// Requires generated/index.html to already exist (`npm run build`) — this
// script reuses that exact document (icon sprite + version already spliced
// in by build-icons.mjs/inject-version.mjs) as its starting point, so the
// two flavors of index.html can't drift apart from each other.

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(__dirname, '..');
const repoRoot = path.resolve(root, '..', '..');

const [, , gameFileArg, versionArg, outFileArg] = process.argv;
if (!gameFileArg) {
    console.error('Usage: node scripts/export-embedded.mjs <game-file> [version] [output-file]');
    process.exit(1);
}

const gameFile = path.resolve(gameFileArg);
if (!fs.existsSync(gameFile)) {
    console.error(`Game file not found: ${gameFile}`);
    process.exit(1);
}

const version = versionArg || fs.readFileSync(path.join(repoRoot, 'VERSION'), 'utf8').trim();

const templatePath = path.join(root, 'generated', 'index.html');
if (!fs.existsSync(templatePath)) {
    console.error(`Missing ${templatePath} — run "npm run build" in src/WasmPlayer first.`);
    process.exit(1);
}
const template = fs.readFileSync(templatePath, 'utf8');

const gameBytes = fs.readFileSync(gameFile);
const gameBase64 = gameBytes.toString('base64');
const gameFilename = path.basename(gameFile);

const cdnBase = `https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@${version}/`;

let html = template;

// 1. <html class="qv-booting"> so the file/URL pickers never paint even for a
//    frame — unlike the ?id=/?url= flavors, this document is exclusively for
//    an embedded game, so there's no need to detect that at runtime.
html = html.replace('<html lang="en">', '<html lang="en" class="qv-booting">');

// 2. <base href> as the very first thing in <head>, so every existing
//    relative <script src>/<link href>/fetch() in the document resolves
//    against the CDN instead of this file's own (nonexistent) sibling
//    assets. See docs/wasmplayer-single-file-export.md §3 for why this
//    alone is sufficient — no per-tag rewriting needed.
html = html.replace('<head>', `<head>\n    <base href="${cdnBase}" />`);

// 3. quest-config.js is local-hosting config (API root, defaultGameUrl) —
//    nothing in this flavor is local, so drop the tag entirely rather than
//    let it resolve to the CDN's own (unused) copy.
html = html.replace('    <script type="text/javascript" src="quest-config.js"></script>\n', '');

// 4. Embed the game bytes + original filename (its extension selects
//    .quest vs .aslx vs Legacy .asl/.cas parsing — see
//    WasmPlayerBridge.Initialise) right before wasm-player.js, which checks
//    for QuestVivaEmbeddedGame first thing in its boot IIFE. No extra
//    attributes needed on the wasm-player.js tag itself — it resolves its
//    own absolute URL via document.currentScript to work around a
//    cross-origin dynamic-import quirk, see the comment there.
const embedScript = `<script type="text/javascript">\n`
    + `        window.QuestVivaEmbeddedGame = ${JSON.stringify(gameBase64)};\n`
    + `        window.QuestVivaEmbeddedGameFilename = ${JSON.stringify(gameFilename)};\n`
    + `    </script>\n`
    + `    <script type="text/javascript" src="wasm-player.js"></script>`;
html = html.replace('<script type="text/javascript" src="wasm-player.js"></script>', embedScript);

const outFile = outFileArg
    ? path.resolve(outFileArg)
    : path.join(path.dirname(gameFile), path.basename(gameFile, path.extname(gameFile)) + '.html');

fs.writeFileSync(outFile, html);
const sizeKb = (fs.statSync(outFile).size / 1024).toFixed(1);
console.log(`Wrote ${outFile} (${sizeKb} KB) — game: ${gameFilename}, runtime: ${cdnBase}`);
