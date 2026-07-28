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

await page.goto(`${baseUrl}/?url=/examples/test.aslx`);
await page.waitForSelector('#txtCommand', { timeout: 30000 });
console.log('Game booted.');

// ExpressionOwner.ShowMenu() function form, via test.aslx's "menufn" command.
await page.fill('#txtCommand', 'menufn');
await page.press('#txtCommand', 'Enter');
await page.waitForSelector('#dialogOptions option', { timeout: 10000 });

const selectedBeforeClick = await page.$eval('#dialogOptions', el => el.value);
console.log('Select value before clicking any option (expect non-empty, e.g. "One"):', selectedBeforeClick);

// Reproduce the original bug: click "Select" WITHOUT first clicking an option,
// exactly as a player would if they trusted the visually-shown default.
await page.click('button:has-text("Select")');
await page.waitForTimeout(300);

console.log('Page errors after clicking Select with no manual option click (expect none):', pageErrors.length === 0 ? 'none' : pageErrors);

const output = await page.$eval('#divOutput', el => el.textContent).catch(() => null);
console.log('Output tail (expect "You chose: One"):', output?.slice(-60));

const dialogStillOpen = await page.$eval('#dialog', el => $(el).dialog('isOpen')).catch(() => null);
console.log('Dialog still open after Select (expect false):', dialogStillOpen);

await browser.close();

if (pageErrors.length > 0) {
    console.log('FAILED: page errors were thrown');
    process.exit(1);
}
