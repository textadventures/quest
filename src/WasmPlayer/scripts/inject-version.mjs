#!/usr/bin/env node
// Splices the repo-root VERSION string into generated/index.html, replacing
// the VERSION_SCRIPT placeholder comment in index.html, so wasm-player.js can
// print its startup banner immediately on page load — without waiting for the
// WASM runtime to boot (which only happens once a game starts loading).
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

const output = html.slice(0, start)
    + `<script>window.QuestVivaVersion = ${JSON.stringify(version)};</script>`
    + html.slice(end + '-->'.length);
fs.writeFileSync(target, output);
console.log(`Wrote generated/index.html with version ${version}`);
