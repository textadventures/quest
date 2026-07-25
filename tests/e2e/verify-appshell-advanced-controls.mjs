// Verifies workstream 1 of docs/editor-progressive-disclosure.md: controls
// flagged <advanced/> in the ASLX editor definitions are folded into a
// collapsed "Advanced" expander at the bottom of their tab instead of being
// rendered inline, and still round-trip edits once expanded.
//
// Target: the game element's "Interface" tab, whose "Set a custom display
// width" checkbox (EditorGameSetacustom) is <advanced/> with no onlydisplayif,
// so it's always present without extra game setup.
//
// Run: node src/WasmEditor... build first (Debug), then
//   node tests/e2e/verify-appshell-advanced-controls.mjs [baseUrl]
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Advanced Controls Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // "game" is auto-selected on load but that doesn't push into the mobile
    // properties pane on narrow viewports; tap it explicitly to be safe.
    await page.locator('[data-value="game"][data-part="branch-control"]').click();

    await page.getByRole('button', { name: 'Interface', exact: true }).click();

    const advancedCheckbox = page.getByText('Set a custom display width', { exact: true });
    const nonAdvancedCheckbox = page.getByText('Show border', { exact: true });

    await nonAdvancedCheckbox.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: non-advanced control ("Show border") renders inline');

    const isAdvancedVisibleBeforeExpand = await advancedCheckbox.isVisible().catch(() => false);
    if (isAdvancedVisibleBeforeExpand) throw new Error('Advanced control should be hidden until the expander is opened');
    console.log('PASS: advanced control is not visible before expanding');

    const summary = page.locator('summary', { hasText: 'Advanced' });
    await summary.waitFor({ state: 'visible', timeout: 5000 });
    await summary.click();

    await advancedCheckbox.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: advanced control appears after expanding "Advanced"');

    // Round-trip: toggle the checkbox and confirm the attribute is persisted.
    const checkboxInput = advancedCheckbox.locator('xpath=ancestor::label').locator('input[type=checkbox]');
    const wasChecked = await checkboxInput.isChecked();
    await checkboxInput.click();
    await page.waitForTimeout(200);
    const nowChecked = await checkboxInput.isChecked();
    if (nowChecked === wasChecked) throw new Error('Checkbox state did not toggle');

    // Switch tabs away and back, then re-open the expander, to confirm the edit persisted.
    await page.getByRole('button', { name: 'Setup', exact: true }).click();
    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    const summary2 = page.locator('summary', { hasText: 'Advanced' });
    await summary2.waitFor({ state: 'visible', timeout: 5000 });
    // The <details> DOM node is reused (not recreated) across this tab-away-and-back,
    // so it's still open from the earlier expand — only click if it isn't.
    const alreadyOpen = await summary2.evaluate(el => el.closest('details')?.open);
    if (!alreadyOpen) await summary2.click();
    await advancedCheckbox.waitFor({ state: 'visible', timeout: 5000 });
    const persistedChecked = await checkboxInput.isChecked();
    if (persistedChecked !== nowChecked) throw new Error('Checkbox edit did not persist after tab switch');
    console.log('PASS: advanced control edit round-trips after tab switch');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/advanced-controls-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
