// Verifies a Quest 4 game whose start script prompts with "enter" shows that prompt.
// Begin() doesn't return until the player answers, so the output only appears if the
// legacy engine raises TurnSuspended for the player to flush on.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-legacy-start-enter.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';
const GAME = '/e2e-fixtures/legacy-start-enter-test.asl';

const browser = await chromium.launch();
const page = await browser.newPage();

try {
    await page.goto(`${BASE}/?url=${encodeURIComponent(GAME)}`);

    const output = page.locator('#divOutput');
    await output.getByText('What is your name?').first().waitFor({ timeout: 60000 });

    const input = page.locator('#txtCommand');
    await input.fill('Alex');
    await input.press('Enter');
    await output.getByText('Hello Alex').first().waitFor({ timeout: 15000 });

    console.log('PASS: start-script enter prompt is shown and answered');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
