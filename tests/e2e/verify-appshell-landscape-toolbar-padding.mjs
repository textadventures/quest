// Regression test for user-reported feedback: the previous AppBar padding
// fix used a width-only media query (max-width: 767px), which never applies
// on a phone in landscape — landscape width commonly exceeds 767px (e.g.
// ~932px on a modern iPhone) even though the height is just as short as
// portrait. The query now also triggers on short height regardless of width.
// Also sanity-checks the new safe-area-inset class is applied to the
// editor's root container (the actual env(safe-area-inset-*) values can't be
// emulated in Chromium/Playwright — that requires real iOS/Safari — but this
// confirms the CSS is wired up and doesn't break layout).
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Landscape Toolbar Padding Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

try {
    // Wide-but-short: a modern iPhone in landscape (e.g. iPhone 15/16 Pro:
    // ~932x430). Width alone (932 > 767) would keep the desktop toolbar
    // arrangement — correctly, there's plenty of horizontal room for it —
    // but the height is exactly as constrained as a portrait phone.
    const landscapeCtx = await browser.newContext({ viewport: { width: 932, height: 430 }, hasTouch: true, isMobile: true });
    const page = await createDraftAndGetPage(landscapeCtx);

    const paddingBlockStart = await page.locator('[data-scope="app-bar"][data-part="root"]').evaluate(el => getComputedStyle(el).paddingBlockStart);
    if (paddingBlockStart !== '8px') throw new Error(`Expected trimmed AppBar padding (8px) on a wide-but-short landscape viewport, got ${paddingBlockStart}`);
    console.log(`PASS: AppBar padding trimmed on a wide (932px) but short (430px) landscape viewport (${paddingBlockStart})`);

    // At this width, the toolbar should still be in its desktop (uncollapsed)
    // arrangement — confirms the fix didn't accidentally also change which
    // breakpoint drives the toolbar's own layout mode.
    const fileMenuVisible = await page.getByRole('button', { name: 'File' }).isVisible();
    if (!fileMenuVisible) throw new Error('Expected the desktop File ▾ menu button to still be visible at 932px width');
    console.log('PASS: desktop toolbar arrangement (File ▾ menu) still used at 932px width');

    const safeAreaEl = page.locator('.safe-area-inset');
    if (await safeAreaEl.count() === 0) throw new Error('Expected the editor root container to have the safe-area-inset class');
    const paddingTop = await safeAreaEl.first().evaluate(el => getComputedStyle(el).paddingTop);
    console.log(`PASS: safe-area-inset class applied to editor root (computed padding-top: ${paddingTop} — 0px is expected here, Chromium doesn't emulate a real device safe area)`);

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
