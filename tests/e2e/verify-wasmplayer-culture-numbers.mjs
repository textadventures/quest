// Verifies numbers print the same under a Swedish browser locale, whose decimal separator is ","
// and whose negative sign is U+2212:
// - Quest 5: msg of a bare double prints "100.4", not "100,4"
// - Quest 4: a calculation with a negative result stores and prints -10, not 0
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-culture-numbers.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';

const browser = await chromium.launch();
const context = await browser.newContext({ locale: 'sv-SE' });

async function outputLines(game, waitForText) {
    const page = await context.newPage();
    await page.goto(`${BASE}/?url=${encodeURIComponent(game)}`);
    const output = page.locator('#divOutput');
    await output.getByText(waitForText).first().waitFor({ timeout: 60000 });
    const text = await output.innerText();
    await page.close();
    return text.split('\n').map(l => l.trim()).filter(Boolean);
}

try {
    const probe = await context.newPage();
    const browserNumber = await probe.evaluate(() => (100.4).toLocaleString());
    await probe.close();
    if (browserNumber !== '100,4') {
        throw new Error(`browser locale is not decimal-comma (got ${browserNumber}), so this test proves nothing`);
    }

    const q5 = await outputLines('/e2e-fixtures/culture-numbers-test.aslx', 'Concat:');
    for (const expected of ['100.4', 'Concat: 100.4']) {
        if (!q5.includes(expected)) {
            throw new Error(`Quest 5: expected a line "${expected}", got ${JSON.stringify(q5)}`);
        }
    }
    console.log('PASS: Quest 5 bare double and concatenation print 100.4');

    const q4 = await outputLines('/e2e-fixtures/legacy-culture-numbers-test.asl', 'Negative:');
    const negLine = q4.find(l => l.startsWith('Negative:'));
    if (negLine !== 'Negative: -10') {
        throw new Error(`Quest 4: expected "Negative: -10", got ${JSON.stringify(negLine)}`);
    }
    console.log('PASS: Quest 4 negative calculation prints -10');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
