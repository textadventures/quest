// Verifies undo after "undo, then another command". The command used to put the turn before the undone one
// on the undo stack a second time, so a later undo undid that turn twice.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-undo-after-command.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';
const GAME = '/e2e-fixtures/undo-after-command-test.aslx';

const browser = await chromium.launch();
const page = await browser.newPage();

async function send(command) {
    const output = page.locator('#divOutput');
    const before = (await output.innerText()).length;
    const input = page.locator('#txtCommand');
    await input.fill(command);
    await input.press('Enter');
    await page.waitForFunction(
        n => document.querySelector('#divOutput').innerText.length > n, before, { timeout: 15000 });
    // Let the turn finish printing
    await page.waitForTimeout(300);
}

try {
    await page.goto(`${BASE}/?url=${encodeURIComponent(GAME)}`);
    await page.locator('#divOutput').getByText('You are in a room.').first().waitFor({ timeout: 60000 });

    for (const command of ['first', 'second', 'undo', 'third', 'undo', 'undo', 'undo', 'showcount']) {
        await send(command);
    }

    const lines = (await page.locator('#divOutput').innerText()).split('\n').map(l => l.trim());
    const undoLines = lines.filter(l => l.startsWith('Undo:') || l === 'Nothing to undo!');
    const expected = ['Undo: second', 'Undo: third', 'Undo: first', 'Nothing to undo!'];
    if (JSON.stringify(undoLines) !== JSON.stringify(expected)) {
        throw new Error(`expected undo output ${JSON.stringify(expected)}, got ${JSON.stringify(undoLines)}`);
    }
    if (!lines.includes('Final count: 0')) {
        throw new Error(`expected "Final count: 0", got ${JSON.stringify(lines)}`);
    }
    console.log('PASS: each turn is undone once, back to the start');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
