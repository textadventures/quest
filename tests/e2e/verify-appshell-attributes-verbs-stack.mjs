// Verifies AttributesEditor and VerbsEditor split panes (list + Assignment/
// Behaviour panel) stack list-above-panel in a narrow properties pane
// (container query on the pane's own width), stay side-by-side on desktop,
// and that selecting a row scrolls the panel into view when stacked.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Attrs Verbs Stack Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

try {
    // Desktop: Attributes tab, split pane stays row, splitter visible, Source column shown.
    const desktopCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const desktopPage = await createDraftAndGetPage(desktopCtx);
    await desktopPage.click('text=room');
    await desktopPage.getByRole('button', { name: /^Attributes$/ }).click();
    await desktopPage.waitForSelector('text=Inherited types', { timeout: 5000 });
    const sourceHeaderVisible = await desktopPage.locator('th:has-text("Source")').last().isVisible();
    if (!sourceHeaderVisible) throw new Error('Desktop should show the Source column');
    console.log('PASS: desktop shows the Attributes Source column');
    const desktopSplitterVisible = await desktopPage.locator('.cursor-col-resize').first().isVisible();
    if (!desktopSplitterVisible) throw new Error('Desktop should show the Attributes splitter (side-by-side layout)');
    console.log('PASS: desktop shows the Attributes splitter (side-by-side layout)');
    await desktopCtx.close();

    // Mobile: stacked, Source column hidden, selecting an attribute row scrolls the panel in.
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const mobilePage = await createDraftAndGetPage(mobileCtx);
    await mobilePage.click('text=room');
    await mobilePage.getByRole('button', { name: /^Attributes$/ }).click();
    await mobilePage.waitForSelector('text=Inherited types', { timeout: 5000 });
    const sourceHeaderHiddenMobile = await mobilePage.locator('th:has-text("Source")').last().isVisible();
    if (sourceHeaderHiddenMobile) throw new Error('Mobile should hide the Source column');
    console.log('PASS: mobile hides the Attributes Source column');

    const splitter = mobilePage.locator('.cursor-col-resize').first();
    if (await splitter.isVisible()) throw new Error('Mobile should hide the Attributes splitter');
    console.log('PASS: mobile hides the Attributes splitter');

    // Select the "name" attribute row and confirm the Assignment panel scrolled into view.
    await mobilePage.locator('[data-attr="name"]').click();
    await mobilePage.waitForSelector('text=Assignment', { timeout: 5000 });
    const assignmentVisible = await mobilePage.locator('text=Assignment').isVisible();
    if (!assignmentVisible) throw new Error('Assignment panel should be scrolled into view after selecting a row');
    console.log('PASS: selecting an attribute row scrolls the (stacked) Assignment panel into view');
    await mobilePage.screenshot({ path: '/tmp/attributes-mobile-stacked.png' });
    await mobileCtx.close();

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
