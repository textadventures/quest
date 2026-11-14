// Verifies Quest 4 "goto" and "move" with an array-indexed room name: cell[n] with n = 2 is the room cell2.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: npx playwright test --project=wasmplayer legacy-room-array

import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('legacy-room-array', async ({ baseURL }) => {
    const BASE = baseURL;
    const GAME = '/e2e-fixtures/legacy-room-array-test.asl';

    const browser = await chromium.launch();
    const page = await browser.newPage();

    async function send(command, waitForText) {
        const input = page.locator('#txtCommand');
        await input.fill(command);
        await input.press('Enter');
        await page.locator('#divOutput').getByText(waitForText).first().waitFor({ timeout: 15000 });
    }

    try {
        await page.goto(`${BASE}/?url=${encodeURIComponent(GAME)}`);
        await page.locator('#txtCommand').waitFor({ timeout: 60000 });

        await send('gocell', 'You are in cell2.');
        await send('movecoin', 'There is coin here.');

        console.log('PASS: goto and move resolve cell[n] to cell2');
    } catch (e) {
        console.error('FAIL:', e.message);
        throw e;
    } finally {
        await browser.close();
    }
});
