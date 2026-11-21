// The Play tab's "Open a game file…" in the browser build takes a game together with the
// library files it includes from its own folder (picked-game-files.ts), shows how many
// libraries came with it, and hands them to the player tab along with the game.
//
// Requires AppShell (5174) and WasmPlayer (5175) dev servers running:
//   node src/WasmPlayer/dev-server.mjs &
//   (cd src/AppShell && npx vite dev) &
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const fixtures = join(dirname(fileURLToPath(import.meta.url)), '..', 'fixtures');

test('play-open-file-libraries', async ({ baseURL }) => {
    const browser = await chromium.launch();
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        page.on('pageerror', err => console.log('[appshell] [pageerror]', err.message));

        await page.goto(`${baseURL}/`);
        await page.waitForSelector('button:has-text("Open a game file…")', { timeout: 15000 });
        const [fileChooser] = await Promise.all([
            page.waitForEvent('filechooser'),
            page.click('button:has-text("Open a game file…")'),
        ]);
        if (!fileChooser.isMultiple()) throw new Error('the Play tab file picker should allow several files');
        await fileChooser.setFiles([join(fixtures, 'picked-library.aslx'), join(fixtures, 'picked-library-game.aslx')]);

        await page.waitForSelector('button:has-text("Start")', { timeout: 5000 });
        await page.waitForSelector('text=picked-library-game.aslx', { timeout: 5000 });
        await page.waitForSelector('text=and 1 library file', { timeout: 5000 });
        console.log('PASS: picked game shown with its library count');

        const [popup] = await Promise.all([
            context.waitForEvent('page'),
            page.click('button:has-text("Start")'),
        ]);
        popup.on('pageerror', err => console.log('[player] [pageerror]', err.message));
        await popup.waitForFunction(
            () => document.body?.innerText.includes('The picked library loaded.'), null, { timeout: 60000 });
        const title = await popup.title();
        if (title !== 'Picked Library Game') throw new Error(`expected the game to start; title was "${title}"`);
        console.log('PASS: game started in the player with its library');
    } finally {
        await browser.close();
    }
});
