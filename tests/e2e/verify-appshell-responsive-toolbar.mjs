import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Toolbar Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

async function checkNoHorizontalOverflow(page, label) {
    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
    if (overflow > 1) throw new Error(`${label}: horizontal overflow of ${overflow}px`);
    console.log(`PASS: ${label} — no horizontal overflow`);
}

try {
    // Desktop
    const desktopCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const desktopPage = await createDraftAndGetPage(desktopCtx);
    await checkNoHorizontalOverflow(desktopPage, 'desktop 1280x800');
    await desktopPage.waitForSelector('button:has-text("File")', { timeout: 5000 });
    console.log('PASS: desktop shows File ▾ menu button');
    await desktopPage.waitForSelector('button:has-text("Delete")', { timeout: 5000 });
    console.log('PASS: desktop shows Delete button directly');
    await desktopPage.click('button:has-text("File")');
    await desktopPage.waitForSelector('text=Backup…', { timeout: 5000 });
    console.log('PASS: File menu opens with Backup… item');
    await desktopPage.keyboard.press('Escape');
    await desktopPage.screenshot({ path: '/tmp/toolbar-desktop.png' });
    await desktopCtx.close();

    // Mobile
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const mobilePage = await createDraftAndGetPage(mobileCtx);
    await checkNoHorizontalOverflow(mobilePage, 'mobile 375x667');
    const deleteVisible = await mobilePage.locator('button:has-text("Delete")').isVisible().catch(() => false);
    if (deleteVisible) throw new Error('Delete button should be hidden on mobile (folded into ⋯)');
    console.log('PASS: mobile hides standalone Delete button');
    const fileVisible = await mobilePage.locator('button:has-text("File")').isVisible().catch(() => false);
    if (fileVisible) throw new Error('File ▾ button should be hidden on mobile');
    console.log('PASS: mobile hides File ▾ button');
    await mobilePage.click('button[title="More"]');
    await mobilePage.locator('text=Delete').last().waitFor({ state: 'visible', timeout: 5000 });
    await mobilePage.waitForSelector('text=Backup…', { timeout: 5000 });
    console.log('PASS: mobile ⋯ menu contains Delete and Backup…');
    await mobilePage.screenshot({ path: '/tmp/toolbar-mobile-menu-open.png' });
    await mobilePage.keyboard.press('Escape');
    await mobilePage.screenshot({ path: '/tmp/toolbar-mobile.png' });
    await mobileCtx.close();

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
