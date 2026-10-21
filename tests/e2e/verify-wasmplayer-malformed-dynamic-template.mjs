// Verifies a game shipping a malformed dynamic template still loads and plays. Quest 5.0 and 5.1 didn't
// check a template's quotes at load, so games published with them can carry one (e.g. Nearco II).
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-malformed-dynamic-template.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';
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
    process.exitCode = 1;
} finally {
    await browser.close();
}
