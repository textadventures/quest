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

await page.goto(`${baseUrl}/?url=/examples/test.aslx`);
await page.waitForSelector('#txtCommand', { timeout: 30000 });
console.log('Game booted.');

async function isSaveDisabled() {
    return await page.$eval('#cmdSave', el => el.disabled || el.classList.contains('ui-state-disabled'));
}

console.log('Save disabled before any command (expect false):', await isSaveDisabled());

// wait2 triggers a script-level wait { ... }
await page.fill('#txtCommand', 'wait2');
await page.press('#txtCommand', 'Enter');
await page.waitForTimeout(500);
console.log('Save disabled during wait() (expect true):', await isSaveDisabled());

await page.click('#endWaitLink');
await page.waitForTimeout(500);
console.log('Save disabled after wait() ends (expect false):', await isSaveDisabled());

// input2 triggers a script-level get input { ... }
await page.fill('#txtCommand', 'input2');
await page.press('#txtCommand', 'Enter');
await page.waitForTimeout(500);
console.log('Save disabled during get input() (expect true):', await isSaveDisabled());

await page.fill('#txtCommand', 'some answer');
await page.press('#txtCommand', 'Enter');
await page.waitForTimeout(500);
console.log('Save disabled after get input() answered (expect false):', await isSaveDisabled());

// Sanity: Save still works normally at rest
await page.click('#cmdSave');
await page.waitForTimeout(500);
const dialogVisible = await page.$eval('#qv-saves', el => el.open).catch(() => null);
console.log('Save dialog opened normally at rest (expect true):', dialogVisible);

await browser.close();
