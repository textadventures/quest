// Ad-hoc manual verification: legacy (.asl) save/load parity fix on
// WebPlayer's LoadSaveFromFile (Game.razor) — confirms downloading a legacy
// save then re-uploading it is accepted (no longer always rejected as
// "different game"). Requires WebPlayer running locally in Development mode
// with Dev:Enabled (see CLAUDE.md / dotnet run --configuration Release
// --no-launch-profile --urls http://localhost:5099, ASPNETCORE_ENVIRONMENT=Development).
import { chromium } from 'playwright';
import path from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5099';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
page.on('pageerror', err => console.log('[pageerror]', err.message));

await page.goto(`${baseUrl}/dev/open`);
await page.waitForTimeout(1000);
const fileInput = await page.$('input[type=file]');
await fileInput.setInputFiles(path.resolve('../../examples/test.asl'));

await page.waitForSelector('#txtCommand', { timeout: 30000 });
console.log('Legacy game booted via /dev/open.');

await page.fill('#txtCommand', 'look');
await page.press('#txtCommand', 'Enter');
await page.waitForTimeout(500);

await page.click('#cmdSave');
await page.waitForTimeout(500);

await page.fill('input[placeholder="Save name"]', 'legacy-slot-1');
await page.click('button:has-text("Save new")');
await page.waitForTimeout(500);
console.log('Slot saved (sanity check for existing slot-based flow).');

const [download] = await Promise.all([
    page.waitForEvent('download'),
    page.click('button:has-text("Save to file")'),
]);
const savedPath = await download.path();
console.log('Downloaded legacy save to', savedPath, 'suggested name:', download.suggestedFilename());

const loadFileInput = await page.$('input[type=file][accept=".quest-save"]');
await loadFileInput.setInputFiles(savedPath);
await page.waitForTimeout(1000);

const errorEl = await page.$('.alert-danger');
const errorText = errorEl ? await errorEl.textContent() : null;
console.log('Mismatch error shown after re-upload (expect null):', errorText);

await page.fill('#txtCommand', 'look');
await page.press('#txtCommand', 'Enter');
await page.waitForTimeout(500);
const outputText = await page.$eval('#divOutput', el => el.textContent).catch(() => null);
console.log('Output tail after reload+command:', outputText?.slice(-300));

await browser.close();

if (errorText) {
    console.error('FAIL: mismatch error was shown for a legitimate same-game legacy save re-upload.');
    process.exit(1);
}
console.log('PASS');
