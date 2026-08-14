// Ad-hoc manual verification: WasmPlayer's cmdSave button gets disabled while
// the engine is suspended inside wait()/get input(), per user feedback that
// saving+reloading in either state silently loses/corrupts the pending turn
// (only world-model data is serialized, not the pending script continuation).
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function isSaveDisabled() {
    return await page.$eval('#cmdSave', el => el.disabled || el.classList.contains('ui-state-disabled'));
}

async function assertSaveDisabled(expected, label) {
    const actual = await isSaveDisabled();
    if (actual !== expected) {
        throw new Error(`${label}: expected Save disabled=${expected}, got ${actual}`);
    }
    console.log(`PASS: ${label} (disabled=${actual})`);
}

async function run() {
    await page.goto(`${baseUrl}/?url=/examples/test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000 });
    console.log('PASS: game booted');

    await assertSaveDisabled(false, 'Save enabled before any command');

    // wait2 triggers a script-level wait { ... }
    await page.fill('#txtCommand', 'wait2');
    await page.press('#txtCommand', 'Enter');
    await page.waitForTimeout(500);
    await assertSaveDisabled(true, 'Save disabled during wait()');

    await page.click('#endWaitLink');
    await page.waitForTimeout(500);
    await assertSaveDisabled(false, 'Save enabled after wait() ends');

    // input2 triggers a script-level get input { ... }
    await page.fill('#txtCommand', 'input2');
    await page.press('#txtCommand', 'Enter');
    await page.waitForTimeout(500);
    await assertSaveDisabled(true, 'Save disabled during get input()');

    await page.fill('#txtCommand', 'some answer');
    await page.press('#txtCommand', 'Enter');
    await page.waitForTimeout(500);
    await assertSaveDisabled(false, 'Save enabled after get input() answered');

    // Sanity: Save still works normally at rest
    await page.click('#cmdSave');
    await page.waitForTimeout(500);
    const dialogVisible = await page.$eval('#qv-saves', el => el.open).catch(() => null);
    if (dialogVisible !== true) {
        throw new Error(`Save dialog did not open normally at rest: expected true, got ${dialogVisible}`);
    }
    console.log('PASS: Save dialog opened normally at rest');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-save-disabled-during-wait-input-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
