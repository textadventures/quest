#!/usr/bin/env node
// Extracts a curated subset of Lucide's <symbol> icons (from lucide-static's
// ~470KB, ~1500-icon sprite.svg) and splices just those into generated/
// index.html, replacing the `ICONS_SPRITE` placeholder comment in index.html.
//
// Spliced in as a hidden inline <svg><defs>, not shipped as a separate file
// referenced via <use href="icons.svg#id"> — cross-document SVG <use> isn't
// reliably supported by browsers (notably Chrome), so the sprite defs must
// live in the same document as the <use> elements that reference them.
//
// Add new icon names to ICONS below as needed — they must match a filename
// in lucide-static/icons/ (e.g. "trash-2" -> icons/trash-2.svg).
//
// Run via `npm run build`, not directly.

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(__dirname, '..');

const ICONS = ['x', 'trash-2', 'folder-open'];

const sprite = fs.readFileSync(path.join(root, 'node_modules/lucide-static/sprite.svg'), 'utf8');

const symbols = ICONS.map((name) => {
    const match = sprite.match(new RegExp(`<symbol id="${name}"[\\s\\S]*?</symbol>`));
    if (!match) throw new Error(`Icon "${name}" not found in lucide-static/sprite.svg`);
    return match[0];
});

const spriteBlock = `<svg style="display:none">\n  <defs>\n${symbols.map((s) => '    ' + s).join('\n')}\n  </defs>\n</svg>`;

const placeholder = '<!-- ICONS_SPRITE:';
const html = fs.readFileSync(path.join(root, 'index.html'), 'utf8');
const start = html.indexOf(placeholder);
if (start === -1) throw new Error('index.html is missing the ICONS_SPRITE placeholder comment');
const end = html.indexOf('-->', start);
if (end === -1) throw new Error('ICONS_SPRITE placeholder comment is not closed with -->');

const output = html.slice(0, start) + spriteBlock + html.slice(end + '-->'.length);

const outDir = path.join(root, 'generated');
fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(path.join(outDir, 'index.html'), output);
console.log(`Wrote generated/index.html with ${ICONS.length} spliced icon(s): ${ICONS.join(', ')}`);
