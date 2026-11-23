// Checks adding a font to the game from the author's own font files, in the Fonts dialog: the
// way to use a font that isn't on Fontsource without pasting it into the stylesheet as base64.
//
// - A file that isn't a font is refused, naming it.
// - Two WOFF2 files, whose names say their weights and styles (Grimoire-Regular, Grimoire-
//   BoldItalic), become one font called Grimoire, regular and bold italic.
// - Add more files… adds a TrueType file to it, whose weight, style and family are read from its
//   own tables (OS/2 and name), not its file name.
// - Add to game writes grimoire.font.css with an @font-face rule for each file, of the kind
//   chosen, lists it in game.fontfiles and makes it the text font, which the Preview's player loads.
//
// Nothing here needs the internet: the dialog is used with the browser offline.
//
// Requires the AppShell dev server (5174), with WasmPlayer on the next port.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const woff2 = readFileSync(fileURLToPath(new URL('../../../src/WasmPlayer/wwwroot/fonts/vt323/latin-400-normal.woff2', import.meta.url)));

// The smallest TrueType file the dialog can read: a name table naming its family, and an OS/2
// table with its weight and the italic bit. The browser can't draw with it, which doesn't matter.
function tinyTtf(family, weight, italic) {
    const os2 = Buffer.alloc(96);
    os2.writeUInt16BE(4, 0);
    os2.writeUInt16BE(weight, 4);
    os2.writeUInt16BE(italic ? 1 : 0, 62);
    const text = Buffer.from(family, 'utf16le').swap16();
    const name = Buffer.alloc(18 + text.length);
    name.writeUInt16BE(0, 0);
    name.writeUInt16BE(1, 2);
    name.writeUInt16BE(18, 4);
    [3, 1, 0x409, 1, text.length, 0].forEach((v, i) => name.writeUInt16BE(v, 6 + i * 2));
    text.copy(name, 18);
    const tables = [['OS/2', os2], ['name', name]];
    const header = Buffer.alloc(12 + tables.length * 16);
    header.writeUInt32BE(0x00010000, 0);
    header.writeUInt16BE(tables.length, 4);
    let offset = header.length;
    tables.forEach(([tag, data], i) => {
        header.write(tag, 12 + i * 16, 'latin1');
        header.writeUInt32BE(offset, 12 + i * 16 + 8);
        header.writeUInt32BE(data.length, 12 + i * 16 + 12);
        offset += data.length;
    });
    return Buffer.concat([header, ...tables.map(([, data]) => data)]);
}

test('font files', async ({ baseURL }) => {
    const browser = await chromium.launch();
    try {
        const context = await browser.newContext({ viewport: { width: 1280, height: 900 } });
        const page = await context.newPage();
        const errors = [];
        page.on('pageerror', err => errors.push(err.message));
        const expect = (what, ok, detail) => { if (!ok) throw new Error(`${what}: ${detail}`); };

        await page.goto(`${baseURL}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Font Files Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 30000 });
        await page.locator('[data-value="game"][data-part="branch-control"]').click();
        await page.getByRole('button', { name: 'Appearance', exact: true }).click();
        const textFont = page.getByRole('button', { name: /^Font: / });
        await textFont.waitFor({ timeout: 10000 });

        // Offline: adding a font from a file mustn't need the internet
        await context.setOffline(true);
        await page.getByRole('button', { name: 'Fonts…', exact: true }).click();
        const dialog = page.locator('[data-fonts-modal]');
        await dialog.waitFor({ timeout: 5000 });
        const input = dialog.locator('[data-font-file-input]');

        await input.setInputFiles({ name: 'notes.txt', mimeType: 'text/plain', buffer: Buffer.from('not a font at all') });
        const alert = dialog.getByRole('alert');
        await alert.waitFor({ timeout: 5000 });
        expect('the error for a file that isn\'t a font', (await alert.innerText()).includes('notes.txt'), await alert.innerText());
        console.log('PASS: a file that isn\'t a font is refused, naming it');

        // Its button opens the (hidden) file input, which is filled in directly here
        await dialog.getByRole('button', { name: 'Add from font files…' }).waitFor({ timeout: 5000 });
        await input.setInputFiles([
            { name: 'Grimoire-Regular.woff2', mimeType: 'font/woff2', buffer: woff2 },
            { name: 'Grimoire-BoldItalic.woff2', mimeType: 'font/woff2', buffer: woff2 },
        ]);
        const panel = dialog.locator('[data-local-font]');
        await panel.waitFor({ timeout: 5000 });
        const faces = () => panel.locator('[data-local-face]').evaluateAll(rows => rows.map(row =>
            `${row.dataset.localFace}:${row.querySelector('select').value}:${row.querySelector('input[type=checkbox]').checked ? 'italic' : 'normal'}`));
        const name = panel.getByRole('textbox', { name: 'Name' });
        expect('the font\'s name', (await name.inputValue()) === 'Grimoire', await name.inputValue());
        expect('the files read from their names', JSON.stringify(await faces()) === JSON.stringify([
            'Grimoire-Regular.woff2:400:normal', 'Grimoire-BoldItalic.woff2:700:italic']), JSON.stringify(await faces()));
        console.log('PASS: WOFF2 files make one font, with weights and styles from their names');

        await input.setInputFiles({ name: 'gr-semi.ttf', mimeType: 'font/ttf', buffer: tinyTtf('Something Else', 600, true) });
        await panel.locator('[data-local-face="gr-semi.ttf"]').waitFor({ timeout: 5000 });
        expect('the faces after adding a TrueType file', JSON.stringify(await faces()) === JSON.stringify([
            'Grimoire-Regular.woff2:400:normal', 'Grimoire-BoldItalic.woff2:700:italic', 'gr-semi.ttf:600:italic']), JSON.stringify(await faces()));
        expect('the font\'s name after adding another file', (await name.inputValue()) === 'Grimoire', await name.inputValue());
        console.log('PASS: Add more files… reads a TrueType file\'s weight and style from its own tables');

        await panel.getByRole('combobox', { name: 'Kind' }).selectOption('sans-serif');
        await panel.getByRole('button', { name: 'Add to game' }).click();
        const row = dialog.locator('[data-game-font-row="Grimoire"]');
        await row.waitFor({ timeout: 10000 });
        const rowText = await row.innerText();
        expect('the added font\'s row', rowText.includes('grimoire.font.css') && rowText.includes("the game's text"), rowText);
        await dialog.getByRole('button', { name: 'Close' }).click();
        await context.setOffline(false);

        expect('the text font', (await textFont.getAttribute('aria-label')) === 'Font: Grimoire', await textFont.getAttribute('aria-label'));
        const css = await page.evaluate(() => document.head.querySelector('style[data-game-font="grimoire.font.css"]')?.textContent ?? '');
        const rules = css.match(/@font-face \{[^}]*\}/g) ?? [];
        expect('the font file\'s first line', css.startsWith("/* Grimoire, a sans-serif font from the author's own files: Grimoire-Regular.woff2, Grimoire-BoldItalic.woff2, gr-semi.ttf"), css.slice(0, 200));
        expect('the font file\'s rules', rules.length === 3
            && /font-weight: 400;/.test(rules[0]) && /font-style: normal;/.test(rules[0]) && rules[0].includes('data:font/woff2;base64,') && rules[0].includes('format("woff2")')
            && /font-weight: 700;/.test(rules[1]) && /font-style: italic;/.test(rules[1])
            && /font-weight: 600;/.test(rules[2]) && rules[2].includes('data:font/ttf;base64,') && rules[2].includes('format("truetype")')
            && rules.every(r => r.includes("font-family: 'Grimoire';")), rules.map(r => r.replace(/base64,[^)]*/, 'base64,…')).join('\n'));
        const fontSetting = await page.evaluate(() => document.querySelector('[data-appearance-scheme] .appearance-page') && getComputedStyle(document.querySelector('[data-appearance-scheme] .appearance-page')).fontFamily);
        expect('the preview\'s font', String(fontSetting).includes('Grimoire') && String(fontSetting).includes('sans-serif'), fontSetting);
        console.log('PASS: Add to game writes one file with a rule for each file, of the kind chosen, and makes it the text font');

        // The Preview's player loads it from the game's own file
        const [player] = await Promise.all([context.waitForEvent('page'), page.click('button[title="Preview game"]')]);
        await player.waitForFunction(() => [...document.fonts].some(f => f.family.replace(/["']/g, '') === 'Grimoire' && f.status === 'loaded'),
            null, { timeout: 60000 }).catch(async () => {
            const families = await player.evaluate(() => [...document.fonts].map(f => `${f.family}:${f.weight}:${f.status}`));
            throw new Error(`the Preview never loaded Grimoire: ${JSON.stringify(families)}`);
        });
        await player.close();
        console.log('PASS: the Preview\'s player loads the font');

        expect('page errors', errors.length === 0, errors.join('\n'));
    } finally {
        await browser.close();
    }
});
