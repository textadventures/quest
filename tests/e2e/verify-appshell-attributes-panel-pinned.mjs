// Regression test for follow-up feedback: after the first scroll fix, the
// Assignment panel was reachable but rendered *after* the whole attributes
// list in one flowing scroll column — so on a long list, selecting a row
// near the top meant scrolling past every remaining row to reach the panel.
// Fixed by giving the (stacked) list and panel each a real, simultaneously-
// visible share of the split area's height once a row is selected, instead
// of one flowing column. This verifies: (1) the panel is visible without
// having scrolled anywhere, immediately after selecting a row near the top
// of a long list, and (2) part of the list stays visible alongside it.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Attrs Panel Pinned Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

try {
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const page = await createDraftAndGetPage(mobileCtx);
    await page.click('text=room');
    await page.getByRole('button', { name: /^Attributes$/ }).click();
    await page.waitForSelector('text=Inherited types', { timeout: 5000 });

    // Add a bunch of custom attributes so the list is longer than the viewport.
    // Scoped near the input — a plain `button:has-text("Add")` also matches the
    // Toolbar's Add-element trigger, whose label text is present (just CSS
    // `hidden`) even in its mobile icon-only state, and :has-text() matches DOM
    // text regardless of CSS visibility.
    const addAttrInput = page.locator('input[placeholder="Add attribute..."]');
    const addAttrButton = addAttrInput.locator('..').getByRole('button', { name: 'Add', exact: true });
    for (let i = 0; i < 15; i++) {
        await addAttrInput.fill(`custom_attr_${i}`);
        await addAttrButton.click();
        await page.waitForTimeout(30);
    }
    const attrCount = await page.locator('[data-attr]').count();
    if (attrCount < 15) throw new Error(`Expected at least 15 custom attributes, found ${attrCount} rows`);
    console.log(`PASS: created 15 custom attributes to force a long list (${attrCount} rows total)`);

    // "name" is near the top of the table (default attribute, listed before the customs).
    await page.evaluate(() => {
        const table = document.querySelector('[data-attr="name"]')?.closest('div.overflow-y-auto');
        if (table) table.scrollTop = 0;
    });
    await page.locator('[data-attr="name"]').click();

    // No manual scroll here — the panel must already be visible immediately.
    const assignmentHeader = page.getByText('Assignment', { exact: true });
    await assignmentHeader.waitFor({ state: 'visible', timeout: 3000 });
    console.log('PASS: Assignment panel visible immediately after selecting a row near the top');

    // Part of the attributes list (rows well past "name", the selected row) should
    // still be visible alongside the panel — not scrolled out of view by the panel
    // taking over the whole screen.
    const laterRowVisible = await page.locator('[data-attr="custom_attr_1"]').isVisible();
    if (!laterRowVisible) throw new Error('Attributes list rows should remain visible alongside the panel, not require scrolling past all of them first');
    console.log('PASS: attributes list stays visible alongside the pinned panel');

    await page.screenshot({ path: '/tmp/attributes-panel-pinned.png' });

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/attributes-panel-pinned-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
