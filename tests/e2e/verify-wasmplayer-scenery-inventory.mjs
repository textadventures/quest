// Regression coverage for issue #905: an object the player is carrying that is
// flagged as scenery appeared in the objects pane but not in the INVENTORY
// command's output, so a carried object could be invisible to the one command
// that lists carried objects.
//
// Settled in favour of the pane: "scenery" means "don't clutter the room
// description with this", not "don't tell the player they are holding it", so
// FormatInventoryList now includes carried scenery while room and container
// listings still exclude it. This asserts the two UIs actually agree, which is
// the reported symptom and the part a unit test on the formatter alone can't
// cover.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function waitUntilCanSendCommand() {
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 20000 });
}

async function sendCommand(command) {
    await waitUntilCanSendCommand();
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
    await waitUntilCanSendCommand();
}

async function paneItems() {
    // playercore.js's updateList() renders each held object as an <li> inside
    // #lstInventory (see the listName == "inventory" branch), not as a link.
    return await page.$$eval('#lstInventory li',
        els => els.map(e => e.textContent.trim()).filter(Boolean));
}

function assertContains(haystack, needle, label) {
    if (!haystack.includes(needle)) {
        throw new Error(`${label}: expected to find "${needle}" in:\n${haystack}`);
    }
    console.log(`PASS: ${label} lists "${needle}"`);
}

function assertOmits(haystack, needle, label) {
    if (haystack.includes(needle)) {
        throw new Error(`${label}: expected NOT to find "${needle}" in:\n${haystack}`);
    }
    console.log(`PASS: ${label} omits "${needle}"`);
}

async function run() {
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/scenery-inventory-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 60000, state: 'attached' });
    await waitUntilCanSendCommand();
    console.log('PASS: game booted');

    // The objects pane, which was already correct and must not regress.
    // canSendCommand flips true before the first list update has landed, so
    // wait for the pane itself rather than reading it straight after boot.
    await page.waitForSelector('#lstInventory li', { timeout: 20000 });
    const pane = (await paneItems()).join(', ');
    if (pane.length === 0) {
        throw new Error('objects pane is empty - the locator is probably stale, not the engine');
    }
    console.log(`objects pane: ${pane}`);
    assertContains(pane, 'lamp', 'objects pane');
    assertContains(pane, 'poster', 'objects pane');
    assertOmits(pane, 'ghost', 'objects pane');

    // The INVENTORY command, which is the half that changed.
    await page.evaluate(() => { document.querySelector('#divOutput').innerHTML = ''; });
    await sendCommand('inventory');
    const inv = (await page.$eval('#divOutput', el => el.innerText)).trim();
    console.log(`inventory output: ${inv.replace(/\n/g, ' | ')}`);
    assertContains(inv, 'lamp', 'INVENTORY command');
    assertContains(inv, 'poster', 'INVENTORY command');
    assertOmits(inv, 'ghost', 'INVENTORY command');

    // Room listings must be unaffected - scenery still stays out of them.
    await page.evaluate(() => { document.querySelector('#divOutput').innerHTML = ''; });
    await sendCommand('look');
    const look = (await page.$eval('#divOutput', el => el.innerText)).trim();
    console.log(`look output: ${look.replace(/\n/g, ' | ')}`);
    assertContains(look, 'rock', 'room description');
    assertOmits(look, 'mural', 'room description');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-scenery-inventory-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
