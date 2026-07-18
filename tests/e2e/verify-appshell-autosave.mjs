// Ad-hoc manual verification for the AppShell autosave change: the explicit
// Save button was removed in favor of debounced autosave-on-edit. This
// confirms the status pill reflects the autosave lifecycle (no "Saved" pill
// stuck at page load, "Saving…" appears after an edit, then "Saved"), and
// that the edit actually lands in OPFS storage (survives a reload), not just
// held in the WASM model.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5176';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);

    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', 'Autosave Test');
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    const saveButtonCount = await page.locator('button:has-text("💾 Save")').count();
    if (saveButtonCount !== 0) throw new Error('Explicit Save button is still present');
    console.log('PASS: no explicit Save button in the toolbar');

    await page.waitForSelector('text=Saved', { timeout: 10000 });
    console.log('PASS: toolbar shows "Saved" once the initial draft is written');

    // Select the game node (already selected on load) and edit its
    // description via the PropertyEditor, then blur it — this should commit
    // into the WASM model (onchange) and kick off the debounced autosave.
    const descBox = page.locator('textarea, input[type="text"]').first();
    await descBox.waitFor({ timeout: 10000 });
    const marker = `Autosave marker ${Date.now()}`;
    await descBox.fill(marker);
    await page.click('.toolbar-divider'); // blur onto something inert

    await page.waitForSelector('text=Saving…', { timeout: 3000 });
    console.log('PASS: "Saving…" pill appeared after edit, without clicking any Save button');

    await page.waitForSelector('text=Saved', { timeout: 10000 });
    console.log('PASS: pill returned to "Saved" after the autosave debounce fired');

    // Reopen via /open (a bare reload of "/" just redirects to /open anyway,
    // since isLoaded is in-memory SPA state) — if the edit only lived in the
    // in-memory WASM model, it would be gone; if autosave actually persisted
    // it to OPFS, it survives.
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('text=Your local drafts', { timeout: 10000 });
    await page.click('button:has-text("Autosave Test.aslx")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    await page.waitForSelector('text=Saved', { timeout: 10000 });
    const persistedValue = await page.locator('textarea, input[type="text"]').first().inputValue();
    console.log('value after reload:', persistedValue);
    if (persistedValue !== marker) throw new Error(`Edit did not survive reload — expected "${marker}", got "${persistedValue}"`);
    console.log('PASS: autosaved edit survived a full page reload (persisted to OPFS)');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-autosave-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
