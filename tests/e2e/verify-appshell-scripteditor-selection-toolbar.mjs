// Regression test for user-reported feedback: ScriptEditor's Cut/Copy/
// Delete/Move selection toolbar (shown when a root-level script row's
// checkbox is checked) got clipped on a narrow mobile screen instead of
// scrolling — the row had no overflow handling and its buttons weren't
// flex-shrink-0, so they just got cut off past the container edge. It should
// scroll horizontally instead, and the background should read a bit lighter.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function selectOneRootScript(page) {
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });
    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.getByRole('button', { name: 'Print', exact: true }).click();
    await page.waitForSelector('input[type="checkbox"]', { timeout: 5000 });
    await page.click('input[type="checkbox"]');
    await page.waitForSelector('text=selected', { timeout: 5000 });
}

try {
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true, isMobile: true });
    const page = await mobileCtx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Selection Toolbar Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await selectOneRootScript(page);

    const toolbar = page.locator('text=selected').locator('..');
    const isScrollable = await toolbar.evaluate(el => el.scrollWidth > el.clientWidth);
    if (!isScrollable) throw new Error('Expected the selection toolbar to overflow (and be scrollable) at 375px width with 5 buttons + a count');
    console.log('PASS: selection toolbar overflows its container at mobile width (needs to scroll)');

    // Nothing outside the toolbar's own box should be clipped/hidden — i.e.
    // the overflow is contained (scrollable), not just visually cut off by a
    // parent with overflow:hidden.
    const overflowX = await toolbar.evaluate(el => getComputedStyle(el).overflowX);
    if (overflowX !== 'auto' && overflowX !== 'scroll') throw new Error(`Expected overflow-x auto/scroll on the toolbar, got ${overflowX}`);
    console.log(`PASS: toolbar has overflow-x: ${overflowX}`);

    // Scrolling reveals the buttons/count that didn't fit initially.
    await toolbar.evaluate(el => { el.scrollLeft = el.scrollWidth; });
    const moveDownVisible = await page.getByRole('button', { name: /Move down/ }).isVisible();
    const countVisible = await page.getByText('1 selected').isVisible();
    if (!moveDownVisible || !countVisible) throw new Error('Expected "Move down" and "1 selected" to become visible after scrolling the toolbar right');
    console.log('PASS: scrolling the toolbar reveals the buttons/count that didn\'t fit');

    // Background reads lighter than the old opaque bg-surface-100-900 (alpha < 1).
    const bg = await toolbar.evaluate(el => getComputedStyle(el).backgroundColor);
    const alphaMatch = bg.match(/\/\s*([\d.]+)\s*\)/) ?? bg.match(/,\s*([\d.]+)\s*\)$/);
    const alpha = alphaMatch ? parseFloat(alphaMatch[1]) : 1;
    if (alpha >= 1) throw new Error(`Expected a lighter, translucent toolbar background, got ${bg}`);
    console.log(`PASS: toolbar background is translucent/lighter (${bg})`);

    await page.screenshot({ path: '/tmp/scripteditor-selection-toolbar-mobile.png' });

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
