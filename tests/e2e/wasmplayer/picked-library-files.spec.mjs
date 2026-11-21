// WasmPlayer's start-screen file picker takes a game together with the library files it
// includes from its own folder (wasm-player.js's splitPickedGameFiles), since a picked file
// brings no folder with it. The library is chosen first, so this also checks that the game is
// found by its <asl> root element rather than by the order the files come in. Picking the game
// alone must fail with the engine's "Library file not found" plus a hint to choose the library too.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const fixtures = join(dirname(fileURLToPath(import.meta.url)), '..', 'fixtures');
const game = join(fixtures, 'picked-library-game.aslx');
const library = join(fixtures, 'picked-library.aslx');

test('picked-library-files', async ({ baseURL }) => {
    const browser = await chromium.launch();
    try {
        const page = await browser.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseURL}/`);
        await page.setInputFiles('#qv-file-input', [library, game]);
        await page.waitForFunction(
            () => document.body?.innerText.includes('The picked library loaded.'), null, { timeout: 60000 });
        const title = await page.title();
        if (title !== 'Picked Library Game') throw new Error(`expected the game, not the library, to start; title was "${title}"`);
        console.log('PASS: game picked with its library starts and runs the library function');

        const page2 = await browser.newPage();
        await page2.goto(`${baseURL}/`);
        await page2.setInputFiles('#qv-file-input', [game]);
        await page2.waitForSelector('#qv-error', { state: 'visible', timeout: 60000 });
        const errorText = await page2.$eval('#qv-error', el => el.textContent);
        if (!errorText.includes('Library file not found: picked-library.aslx')) {
            throw new Error(`expected the missing-library error, got: ${errorText}`);
        }
        if (!errorText.includes('This game uses picked-library.aslx. Open the game again, and choose that file too')) {
            throw new Error(`expected the hint to choose the library too, got: ${errorText}`);
        }
        console.log('PASS: game picked alone explains how to add its library');
    } finally {
        await browser.close();
    }
});
