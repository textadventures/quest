// Regression test for user-reported feedback: the Add Script Command modal
// looked cluttered on mobile — text-sm throughout didn't match the rest of
// the app's compact text-xs, and the "Quick add" shortcut pills wrapped onto
// several rows on a narrow modal instead of staying on one scrollable row.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true, isMobile: true });
    const page = await mobileCtx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `AddScript Modal Mobile Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });
    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });

    // "Quick add" pills should all sit on one row (same top position), not wrap.
    const quickAddRow = page.locator('.overflow-x-auto').first();
    const pillTops = await quickAddRow.locator('button').evaluateAll(
        els => els.map(el => Math.round(el.getBoundingClientRect().top))
    );
    const uniqueTops = new Set(pillTops);
    if (uniqueTops.size !== 1) throw new Error(`Expected all "Quick add" pills on one row, got tops: ${JSON.stringify(pillTops)}`);
    console.log(`PASS: all ${pillTops.length} "Quick add" pills sit on a single row`);

    // The row itself should be horizontally scrollable (content wider than the
    // visible strip) rather than clipped or forced to wrap.
    const isScrollable = await quickAddRow.evaluate(el => el.scrollWidth > el.clientWidth);
    if (!isScrollable) console.log('NOTE: quick-add row fit without needing to scroll at this width (not a failure)');
    else console.log('PASS: "Quick add" row is horizontally scrollable at this width');

    // Command rows now use text-xs (12px) instead of text-sm (14px), matching
    // the rest of the app (e.g. the tree panel).
    const rowFontSize = await page.locator('[role="option"]').first().evaluate(el => parseFloat(getComputedStyle(el).fontSize));
    if (rowFontSize >= 13.5) throw new Error(`Expected command rows to use the smaller text-xs size (~12px), got ${rowFontSize}px — looks like text-sm (14px) is still in effect`);
    console.log(`PASS: command rows use the app's compact text-xs size (${rowFontSize}px)`);

    await page.screenshot({ path: '/tmp/addscript-modal-mobile.png' });

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
