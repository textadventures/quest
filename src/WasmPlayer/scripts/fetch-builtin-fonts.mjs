#!/usr/bin/env node
// Downloads the fonts the player has built in (the themes' fonts) from Fontsource into
// wwwroot/fonts/, with their licences, and writes wwwroot/fonts/fonts.css with their @font-face
// rules. index.html loads that stylesheet, so a game can use these fonts by name, in any font
// setting, without its own copy, and nothing is ever asked of Google. A browser only downloads a
// font's files once a page uses it, and only the subsets its text needs (unicode-range).
//
// Not part of the build: the files are checked in. Run it again only to add a font or update
// them. A font that a theme in CoreThemes.aslx or a published game names has to stay here, since
// a published game's Core is frozen (see CLAUDE.md), so only ever add fonts.
//
//   node scripts/fetch-builtin-fonts.mjs
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const FONTS = ['courier-prime', 'literata', 'nunito', 'special-elite', 'vt323'];

const API = 'https://api.fontsource.org/v1/fonts';
const out = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', 'wwwroot', 'fonts');

// The regular weight and a bold one, as the editor's Fonts dialog chooses (AppShell lib/fonts.ts)
function chooseWeights(weights) {
    const regular = [...weights].sort((a, b) => Math.abs(a - 400) - Math.abs(b - 400) || a - b)[0];
    const heavier = weights.filter(w => w >= regular + 200);
    const bold = heavier.includes(700) ? 700 : heavier.length ? Math.max(...heavier) : undefined;
    return bold === undefined ? [regular] : [regular, bold];
}

async function get(url, as) {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`${url} answered ${response.status}`);
    return as === 'json' ? response.json() : as === 'text' ? response.text() : new Uint8Array(await response.arrayBuffer());
}

const css = [
    '/* The fonts built into the player, from Fontsource (https://fontsource.org). Each one\'s',
    '   licence is in its folder. Written by scripts/fetch-builtin-fonts.mjs: don\'t edit. */',
    '',
];
fs.rmSync(out, { recursive: true, force: true });
for (const id of FONTS) {
    const font = await get(`${API}/${id}`, 'json');
    const folder = path.join(out, id);
    fs.mkdirSync(folder, { recursive: true });
    fs.writeFileSync(path.join(folder, 'LICENSE.txt'),
        await get(`https://cdn.jsdelivr.net/npm/@fontsource/${id}@${font.npmVersion}/LICENSE`, 'text'));
    css.push(`/* ${font.family}: ${font.license} */`);
    for (const weight of chooseWeights(font.weights)) {
        for (const style of ['normal', 'italic']) {
            const files = font.variants[String(weight)]?.[style];
            if (!files) continue;
            // Every named subset: numbered ones are for fonts with thousands of characters
            for (const subset of Object.keys(files).filter(s => !/^\d+$/.test(s))) {
                const file = `${subset}-${weight}-${style}.woff2`;
                fs.writeFileSync(path.join(folder, file), await get(files[subset].url.woff2));
                css.push(
                    '@font-face {',
                    `    font-family: "${font.family}";`,
                    `    font-style: ${style};`,
                    `    font-weight: ${weight};`,
                    '    font-display: swap;',
                    `    src: url("${id}/${file}") format("woff2");`,
                    ...(font.unicodeRange[subset] ? [`    unicode-range: ${font.unicodeRange[subset]};`] : []),
                    '}',
                );
            }
        }
    }
    css.push('');
    console.log(`${font.family}: done`);
}
fs.writeFileSync(path.join(out, 'fonts.css'), css.join('\n'));
