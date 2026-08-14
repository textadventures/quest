// Ad-hoc manual verification: showMenu() in player.js (shared by WasmPlayer
// and WebPlayer) populates the menu dialog's <select> without marking any
// <option> as selected. A <select> visually shows its first option as
// selected by default, but jQuery's .val() returns null (not that option's
// value) unless one is explicitly marked - so clicking "Select" without
// first manually clicking an option threw
// "TypeError: Cannot read properties of null (reading 'length')" in
// dialogSelect() and silently did nothing. Fixed by explicitly marking the
// first appended <option> selected.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
const pageErrors = [];
page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
page.on('pageerror', err => { pageErrors.push(err.message); console.log('[pageerror]', err.message); });

async function run() {
    await page.goto(`${baseUrl}/?url=/examples/test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000 });
    console.log('PASS: game booted');

    // ExpressionOwner.ShowMenu() function form, via test.aslx's "menufn" command.
    await page.fill('#txtCommand', 'menufn');
    await page.press('#txtCommand', 'Enter');
    await page.waitForSelector('#dialogOptions option', { timeout: 10000 });

    const selectedBeforeClick = await page.$eval('#dialogOptions', el => el.value);
    if (!selectedBeforeClick) {
        throw new Error(`Expected a non-empty default selection, got ${JSON.stringify(selectedBeforeClick)}`);
    }
    console.log(`PASS: first option is pre-selected ("${selectedBeforeClick}")`);

    // Reproduce the original bug: click "Select" WITHOUT first clicking an option,
    // exactly as a player would if they trusted the visually-shown default.
    await page.click('button:has-text("Select")');
    await page.waitForTimeout(300);

    if (pageErrors.length > 0) {
        throw new Error(`Page errors were thrown after clicking Select with no manual option click: ${pageErrors.join('; ')}`);
    }
    console.log('PASS: no page errors after clicking Select with no manual option click');

    const output = await page.$eval('#divOutput', el => el.textContent).catch(() => null);
    if (!output?.includes('You chose: One')) {
        throw new Error(`Expected output to contain "You chose: One", got tail: ${output?.slice(-100)}`);
    }
    console.log('PASS: menu choice was submitted ("You chose: One")');

    const dialogStillOpen = await page.$eval('#dialog', el => $(el).dialog('isOpen')).catch(() => null);
    if (dialogStillOpen !== false) {
        throw new Error(`Expected dialog to be closed after Select, got isOpen=${dialogStillOpen}`);
    }
    console.log('PASS: dialog closed after Select');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-showmenu-select-default-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
