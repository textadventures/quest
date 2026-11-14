// Verifies a game shipping a malformed dynamic template still loads and plays. Quest 5.0 and 5.1 didn't
// check a template's quotes at load, so games published with them can carry one (e.g. Nearco II).
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: npx playwright test --project=wasmplayer malformed-dynamic-template

import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('malformed-dynamic-template', async ({ baseURL }) => {
    const BASE = baseURL;
    const GAME = '/e2e-fixtures/malformed-dynamic-template-test.aslx';

    const browser = await chromium.launch();
    const page = await browser.newPage();

    try {
        await page.goto(`${BASE}/?url=${encodeURIComponent(GAME)}`);

        const output = page.locator('#divOutput');
        await output.getByText('Game started.').first().waitFor({ timeout: 60000 });

        const input = page.locator('#txtCommand');
        await input.fill('look');
        await input.press('Enter');
        await output.getByText('You are in a room.').nth(1).waitFor({ timeout: 15000 });

        const text = await output.innerText();
        if (text.includes('Missing quote character')) {
            throw new Error(`template error reported although the template was never used: ${text}`);
        }
        console.log('PASS: game with a malformed, unused dynamic template loads and plays');
    } catch (e) {
        console.error('FAIL:', e.message);
        throw e;
    } finally {
        await browser.close();
    }
});
