// Verifies workstream 2 of docs/editor-progressive-disclosure.md: the
// advanced tree category headers (Functions, Timers, Walkthrough, and the
// "Advanced" group's children: Included Libraries, Templates, Dynamic
// Templates, Object Types, Javascript) are always present in the tree data
// (added unconditionally by EditorController.AddTreeHeader) but should be
// hidden in the UI while empty, since an empty header is noise. "Objects"
// must never be hidden, and adding the first element of a hidden type via
// the "Advanced" node's own "+ Add" buttons in the properties panel (the
// only entry point once its own sub-headers are hidden — see
// PropertyEditor's ADVANCED_ADDERS) should make its header reappear with the
// new element selected.
//
// Note: a fresh text-adventure game already includes two default libraries
// (English.aslx, Core.aslx), so "Included Libraries" — and therefore the
// "Advanced" group header itself — is visible even on a brand-new game;
// Templates/Dynamic Templates/Object Types/Javascript are the ones actually
// empty by default.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Empty Advanced Cats Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    const tree = page.locator('.overflow-y-auto.p-1.text-xs');
    await tree.locator('text=Objects').waitFor({ state: 'visible', timeout: 10000 });

    for (const label of ['Functions', 'Timers', 'Walkthrough']) {
        const visible = await tree.getByText(label, { exact: true }).isVisible().catch(() => false);
        if (visible) throw new Error(`Expected "${label}" header to be hidden while empty, but it's visible`);
    }
    console.log('PASS: empty Functions/Timers/Walkthrough headers are hidden on a fresh game');

    const objectsVisible = await tree.getByText('Objects', { exact: true }).isVisible();
    if (!objectsVisible) throw new Error('"Objects" header must never be hidden, even though it starts with just the default room');
    console.log('PASS: "Objects" header stays visible');

    // "Advanced" itself stays visible because "Included Libraries" (a child) already
    // has the two default libraries — expand it and confirm the genuinely-empty
    // children (Templates/Dynamic Templates/Object Types/Javascript) are hidden
    // while "Included Libraries" is not.
    const advancedSummary = tree.getByText('Advanced', { exact: true });
    await advancedSummary.waitFor({ state: 'visible', timeout: 5000 });
    // "_advanced" starts collapsed like every other branch now (#827), so
    // expand it via its own caret (there are several collapsed branches — room
    // and Included Libraries too — so target the Advanced row specifically).
    await advancedSummary.locator('xpath=ancestor::*[@data-part="branch-control"]').locator('button[aria-label="Expand"]').click();
    await page.waitForTimeout(300);

    await tree.getByText('Included Libraries', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "Advanced" is visible and shows "Included Libraries" (non-empty by default)');

    for (const label of ['Templates', 'Dynamic Templates', 'Object Types', 'Javascript']) {
        const visible = await tree.getByText(label, { exact: true }).isVisible().catch(() => false);
        if (visible) throw new Error(`Expected "${label}" to be hidden while empty, but it's visible`);
    }
    console.log('PASS: empty Templates/Dynamic Templates/Object Types/Javascript are hidden inside "Advanced"');

    // Add the first Function via the "Advanced" node's own "+ Add Function" button
    // and confirm the "Functions" header appears with the new element selected.
    await tree.getByText('Advanced', { exact: true }).click();
    await page.getByRole('button', { name: '+ Add Function', exact: true }).click();
    await page.fill('#element-name', 'MyFunction');
    await page.getByRole('button', { name: 'Add Function', exact: true }).click();

    await tree.getByText('Functions', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "Functions" header appears after adding the first function');

    const functionNodeVisible = await tree.getByText('MyFunction', { exact: true }).isVisible();
    if (!functionNodeVisible) throw new Error('New function element should be visible in the tree');
    console.log('PASS: new function element is visible in the tree');

    // Other still-empty headers remain hidden.
    for (const label of ['Timers', 'Walkthrough', 'Javascript']) {
        const visible = await tree.getByText(label, { exact: true }).isVisible().catch(() => false);
        if (visible) throw new Error(`"${label}" should still be hidden — nothing of that type was added`);
    }
    console.log('PASS: other still-empty categories remain hidden');

    // "+ Add JavaScript" lives under the "Advanced" node too and should make the
    // "Javascript" header reappear, same as Functions did above, once its confirm
    // modal (see AddJavascriptModal) is accepted with the pre-filled filename.
    // Selection moved to the new function above, so re-select "Advanced" first.
    await tree.getByText('Advanced', { exact: true }).click();
    await page.getByRole('button', { name: '+ Add JavaScript', exact: true }).click();
    await page.getByRole('button', { name: 'Add JavaScript', exact: true }).click();
    await tree.getByText('Javascript', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "Javascript" header appears after adding via the "Advanced" node');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/empty-advanced-categories-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
