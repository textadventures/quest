// Verifies Quest 4 behaviour that relied on VB6 object records being value types:
// - "clone" copies the object instead of renaming the original
// - reading object id 0 ("no such object") no longer gives "[An internal error occurred]"
// - taking an object from a container removes it from the container first
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-legacy-records.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';
const GAME = '/e2e-fixtures/legacy-records-test.asl';

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

    await send('clonetwice', 'Colours:');
    await send('removeloose', 'Removed.');
    await send('take ball', 'You pick it up.');

    const output = await page.locator('#divOutput').innerText();
    const lines = output.split('\n').map(l => l.trim());
    if (lines.includes('[An internal error occurred]')) {
        throw new Error(`internal error in output: ${JSON.stringify(lines)}`);
    }
    for (const expected of ['Colours: red green red', '(first removing it from box)']) {
        if (!lines.includes(expected)) {
            throw new Error(`expected a line "${expected}", got ${JSON.stringify(lines)}`);
        }
    }
    console.log('PASS: clone copies, id 0 reads are harmless, take removes from container first');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
