// Regression test for user-reported feedback: ScriptEditor's Cut/Copy/
// Delete/Move selection toolbar (shown when a root-level script row's
// checkbox is checked) got clipped on a narrow mobile screen instead of
// scrolling — the row had no overflow handling and its buttons weren't
// flex-shrink-0, so they just got cut off past the container edge. It should
// scroll horizontally if it ever overflows, and the background should read a bit
// lighter. Since the row ⋯ menu became the touch path to single-row actions, the
// toolbar only holds multi-row ones (Cut/Copy/Delete/↑/↓ + count + deselect), so it
// should now fit a 375px screen without scrolling at all.
import { chromium } from './lib/tracked-chromium.mjs';

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
    const widths = await toolbar.evaluate(el => ({ scroll: el.scrollWidth, client: el.clientWidth }));
    if (widths.scroll > widths.client) throw new Error(`Expected the selection toolbar to fit at 375px width without scrolling, needed ${widths.scroll}px of ${widths.client}px`);
    console.log(`PASS: selection toolbar fits at mobile width (${widths.scroll}px of ${widths.client}px)`);

    // If it ever does overflow (a longer translation, say), the overflow must be
    // contained (scrollable), not just visually cut off by a parent with
    // overflow:hidden.
    const overflowX = await toolbar.evaluate(el => getComputedStyle(el).overflowX);
    if (overflowX !== 'auto' && overflowX !== 'scroll') throw new Error(`Expected overflow-x auto/scroll on the toolbar, got ${overflowX}`);
    console.log(`PASS: toolbar has overflow-x: ${overflowX}`);

    const moveDownVisible = await toolbar.getByRole('button', { name: 'Move down' }).isVisible();
    const countVisible = await page.getByText('1 selected').isVisible();
    const deselectVisible = await toolbar.getByRole('button', { name: 'Deselect all' }).isVisible();
    if (!moveDownVisible || !countVisible || !deselectVisible) throw new Error('Expected "Move down", "1 selected" and "Deselect all" all to be visible without scrolling');
    console.log('PASS: Move down, the count and Deselect all are all visible');

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
