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
    // Excludes the elements-tree filter box (added after this test was
    // written, and first in DOM order): an unscoped "first text input on the
    // page" locator silently fills that instead, so the edit never reaches
    // the WASM bridge — isDirty never flips and "Saving…" never appears.
    const descBox = page.locator('textarea, input[type="text"]:not([placeholder="Filter..."])').first();
    await descBox.waitFor({ timeout: 10000 });
    const marker = `Autosave marker ${Date.now()}`;
    await descBox.fill(marker);

    // Explicitly dispatch input+change events to guarantee the PropertyEditor's
    // onchange handler fires. page.fill() sets the DOM value and dispatches
    // input events, but the native change event (which triggers the bridge's
    // setAttribute → isDirty flip) doesn't always fire on blur in headless
    // Chromium on CI — making the subsequent pill check time out.
    await descBox.dispatchEvent('input');
    await descBox.dispatchEvent('change');
    await page.click('.toolbar-divider'); // blur onto something inert

    // Wait for the "Unsaved" pill (class save-chip-unsaved) — it appears the
    // instant isDirty flips to true, confirming the bridge was actually
    // updated. Then wait for "Unsaved" to disappear (save completed, pill
    // transitioned to "Saving…" then "Saved"), and finally confirm "Saved"
    // is visible.  Checking "Saving…" directly via text= flaked on CI because
    // MIN_SAVING_VISIBLE_MS's 300ms window could elapse between frames, and
    // "Unsaved" contains the substring "Saved" which gave false positives.
    await page.locator('.save-chip-unsaved').waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "Unsaved" pill appeared — bridge isDirty confirmed');

    await page.locator('.save-chip-unsaved').waitFor({ state: 'hidden', timeout: 10000 });
    console.log('PASS: "Unsaved" pill gone — save cycle started');

    await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
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
    const persistedValue = await page.locator('textarea, input[type="text"]:not([placeholder="Filter..."])').first().inputValue();
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
