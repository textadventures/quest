// Checks the Text section of the game's Appearance tab (#2902): the font pickers, and the Fonts
// dialog copying a font into the game as one file, which the player then loads.
//
// - The text font is a picker (FontPicker.svelte), not a box to type in, and there's no Web font
//   box or Google Fonts link for a new game.
// - A font built into the player (Literata) can be chosen, and the editor shows it in that font.
// - The interface font has Default, which empties the setting, and Custom… shows a box to type in.
// - Fonts… adds a font from Fontsource, as <id>.font.css with the font and its licence in it,
//   lists it in game.fontfiles and makes it the game's text font, which the Preview's player loads.
// - Removing it deletes the file and puts the text font back to the fonts after it.
//
// Fontsource and jsDelivr are stood in for with page.route, serving one of the player's own
// built-in font files, so this needs no internet and always gets the same answers.
//
// Requires the AppShell dev server (5174), with WasmPlayer on the next port.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const woff2 = readFileSync(fileURLToPath(new URL('../../../src/WasmPlayer/wwwroot/fonts/vt323/latin-400-normal.woff2', import.meta.url)));
const fileUrl = (name) => `https://cdn.jsdelivr.net/fontsource/fonts/lora@latest/${name}.woff2`;
const LORA = {
    id: 'lora', family: 'Lora', category: 'serif', subsets: ['cyrillic', 'latin', 'latin-ext'], weights: [400, 500, 700],
    styles: ['normal', 'italic'], license: 'OFL-1.1', npmVersion: '5.3.0',
    unicodeRange: { latin: 'U+0000-00FF', 'latin-ext': 'U+0100-024F', cyrillic: 'U+0400-045F' },
    variants: Object.fromEntries([400, 500, 700].map(w => [String(w), Object.fromEntries(['normal', 'italic'].map(s => [s,
        Object.fromEntries(['cyrillic', 'latin', 'latin-ext'].map(sub => [sub, { url: { woff2: fileUrl(`${sub}-${w}-${s}`) } }]))]))])),
};

test('fonts', async ({ baseURL }) => {
    const browser = await chromium.launch();
    try {
        const context = await browser.newContext({ viewport: { width: 1280, height: 900 } });
        const requested = [];
        await context.route('https://api.fontsource.org/**', route => {
            const url = route.request().url();
            requested.push(url);
            if (url.endsWith('/fonts')) {
                return route.fulfill({ json: [{ id: 'lora', family: 'Lora', category: 'serif', subsets: LORA.subsets, weights: LORA.weights, styles: LORA.styles }] });
            }
            if (url.endsWith('/fonts/lora')) return route.fulfill({ json: LORA });
            return route.fulfill({ status: 404, body: '' });
        });
        await context.route('https://cdn.jsdelivr.net/**', route => {
            const url = route.request().url();
            requested.push(url);
            if (url.endsWith('/LICENSE')) return route.fulfill({ contentType: 'text/plain', body: 'SIL Open Font License, Version 1.1 */ (a test licence)' });
            return route.fulfill({ contentType: 'font/woff2', body: woff2 });
        });
        // Nothing should ever go to Google
        await context.route(/googleapis\.com|gstatic\.com/, route => {
            requested.push(route.request().url());
            return route.abort();
        });

        const page = await context.newPage();
        const errors = [];
        page.on('pageerror', err => errors.push(err.message));
        const expect = (what, ok, detail) => { if (!ok) throw new Error(`${what}: ${detail}`); };

        await page.goto(`${baseURL}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Fonts Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 30000 });
        await page.locator('[data-value="game"][data-part="branch-control"]').click();
        await page.getByRole('button', { name: 'Appearance', exact: true }).click();

        const textFont = page.getByRole('button', { name: /^Font: / });
        const uiFont = page.getByRole('button', { name: /^Interface font: / });
        await textFont.waitFor({ timeout: 10000 });
        expect('the text font picker', (await textFont.getAttribute('aria-label')) === 'Font: Georgia', await textFont.getAttribute('aria-label'));
        expect('a new game\'s Web font box', await page.locator('[data-webfont-control]').count() === 0, 'it shows');
        expect('a Google Fonts link', await page.locator('a[href*="fonts.google.com"]').count() === 0, 'it shows');
        console.log('PASS: a new game\'s Text section has a font picker, and no Web font box or Google Fonts link');

        const sampleFont = () => page.locator('[data-appearance-scheme] .appearance-page').first().evaluate(el => getComputedStyle(el).fontFamily);
        const choose = async (picker, option) => {
            await picker.click();
            await page.getByRole('listbox').getByRole('option', { name: option, exact: true }).click();
            await page.waitForTimeout(300);
        };

        // A font built into the player
        await choose(textFont, 'Literata');
        expect('the text font after choosing Literata', (await textFont.getAttribute('aria-label')) === 'Font: Literata', await textFont.getAttribute('aria-label'));
        expect('the preview\'s font', (await sampleFont()).startsWith('Literata'), await sampleFont());
        const literata = await page.evaluate(async () => { await document.fonts.load('16px Literata'); return document.fonts.check('16px Literata'); });
        expect('the editor\'s copy of the built-in Literata', literata, 'it isn\'t loaded');
        console.log('PASS: a font built into the player can be chosen, and the editor shows it');

        // The interface font: Default and Custom…
        expect('a new game\'s interface font', (await uiFont.getAttribute('aria-label')) === 'Interface font: Default', await uiFont.getAttribute('aria-label'));
        await choose(uiFont, 'Nunito');
        expect('the interface font after choosing Nunito', (await uiFont.getAttribute('aria-label')) === 'Interface font: Nunito', await uiFont.getAttribute('aria-label'));
        await choose(uiFont, 'Custom…');
        const custom = page.locator('[data-font-picker-custom]');
        expect('the custom font box', await custom.count() === 1, `${await custom.count()} of them`);
        await custom.fill("'Book Antiqua', Palatino, serif");
        await custom.press('Enter');
        await custom.blur();
        await page.waitForTimeout(300);
        expect('the custom font', (await custom.inputValue()) === "'Book Antiqua', Palatino, serif", await custom.inputValue());
        await choose(uiFont, 'Default');
        expect('the interface font back to Default', (await uiFont.getAttribute('aria-label')) === 'Interface font: Default', await uiFont.getAttribute('aria-label'));
        console.log('PASS: the interface font has Default and Custom…');

        // Adding a font with the Fonts dialog, opened from a font picker's Add fonts…
        await textFont.click();
        await page.getByRole('listbox').getByRole('option', { name: 'Add fonts…', exact: true }).click();
        const dialog = page.locator('[data-fonts-modal]');
        await dialog.waitFor({ timeout: 5000 }).catch(() => { throw new Error('Add fonts… doesn\'t open the Fonts dialog'); });
        await dialog.getByRole('searchbox', { name: 'Search fonts' }).fill('lor');
        await dialog.locator('[data-font-result="Lora"]').click();
        await dialog.getByText('Letters for').waitFor();
        const ticked = await dialog.locator('fieldset input[type=checkbox]').evaluateAll(boxes => boxes.map(b => b.checked));
        expect('the subsets ticked to start with (Cyrillic, Latin, Latin Extended)', JSON.stringify(ticked) === '[false,true,true]', JSON.stringify(ticked));
        await dialog.getByRole('button', { name: 'Add to game' }).click();
        await dialog.locator('[data-game-font-row="Lora"]').waitFor({ timeout: 10000 });
        const row = await dialog.locator('[data-game-font-row="Lora"]').innerText();
        expect('the added font\'s row', row.includes('lora.font.css') && row.includes("the game's text"), row);
        // Regular and bold, normal and italic, in Latin and Latin Extended (each font in the list is
        // shown in its own font too, from its latin-400-normal file)
        const downloads = [...new Set(requested.filter(u => u.includes('/lora@') && u.endsWith('.woff2')).map(u => u.split('/').pop()))].sort();
        expect('the files downloaded', JSON.stringify(downloads) === JSON.stringify([
            'latin-400-italic.woff2', 'latin-400-normal.woff2', 'latin-700-italic.woff2', 'latin-700-normal.woff2',
            'latin-ext-400-italic.woff2', 'latin-ext-400-normal.woff2', 'latin-ext-700-italic.woff2', 'latin-ext-700-normal.woff2',
        ]), JSON.stringify(downloads));
        await dialog.getByRole('button', { name: 'Close' }).click();

        expect('the text font after adding Lora', (await textFont.getAttribute('aria-label')) === 'Font: Lora', await textFont.getAttribute('aria-label'));
        expect('the preview\'s font with Lora', (await sampleFont()).startsWith('Lora'), await sampleFont());
        const css = await page.evaluate(() => document.head.querySelector('style[data-game-font="lora.font.css"]')?.textContent ?? '');
        expect('the font file', css.startsWith('/* Lora, a serif font from Fontsource') && css.includes('a test licence') && !css.includes('*/ (a test')
            && (css.match(/@font-face/g) ?? []).length === 8 && css.includes('src: url(data:font/woff2;base64,'), css.slice(0, 400));
        await textFont.click();
        const gameOptions = await page.getByRole('listbox').locator('[data-font-option]').allInnerTexts();
        await page.keyboard.press('Escape');
        expect('the picker\'s list', gameOptions[0] === 'Lora' && gameOptions.includes('Literata') && gameOptions.includes('Georgia'), JSON.stringify(gameOptions));
        console.log('PASS: Fonts… copies a font into the game as one file with its licence, and makes it the text font');

        // The Preview's player loads it from the game's own file
        const [player] = await Promise.all([context.waitForEvent('page'), page.click('button[title="Preview game"]')]);
        await player.waitForFunction(() => [...document.fonts].some(f => f.family.replace(/["']/g, '') === 'Lora' && f.status === 'loaded'),
            null, { timeout: 60000 }).catch(async () => {
            const families = await player.evaluate(() => [...document.fonts].map(f => `${f.family}:${f.status}`));
            throw new Error(`the Preview never loaded Lora: ${JSON.stringify(families)}`);
        });
        await player.close();
        console.log('PASS: the Preview\'s player loads the game\'s own font');

        // Removing it
        await page.getByRole('button', { name: 'Fonts…', exact: true }).click();
        await dialog.locator('[data-game-font-row="Lora"]').getByRole('button', { name: 'Remove' }).click();
        await page.getByRole('button', { name: 'Remove', exact: true }).last().click();
        await dialog.locator('[data-game-font-row="Lora"]').waitFor({ state: 'detached', timeout: 10000 });
        await dialog.getByRole('button', { name: 'Close' }).click();
        expect('the text font after removing Lora', (await textFont.getAttribute('aria-label')) === 'Font: Georgia', await textFont.getAttribute('aria-label'));
        console.log('PASS: removing a font deletes it and the text font falls back on the fonts after it');

        const google = requested.filter(u => /google/.test(u));
        expect('requests to Google', google.length === 0, JSON.stringify(google));
        expect('page errors', errors.length === 0, errors.join('\n'));
        console.log('PASS: nothing asked Google for anything');
    } finally {
        await browser.close();
    }
});
