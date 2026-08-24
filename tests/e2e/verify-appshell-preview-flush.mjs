// Regression test: clicking Toolbar's "Preview game" button used to open the WasmPlayer preview
// without first blurring the currently-focused field or flushing pending edits to storage —
// unlike handleHome(), which explicitly calls saveGame() before navigating away. A field the
// author was still mid-typing in (not yet blurred) never committed into the bridge at all (text
// inputs commit on blur/change, not on every keystroke), so Preview could open showing stale
// content. Fixed by calling saveGame() at the top of handlePreview(), matching handleHome()'s
// pattern. This only checks the editor-side flush (blur + persist) — it doesn't drive the actual
// WasmPlayer preview tab, since that requires its own dev server running separately.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Preview Flush Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 30000 });
    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });
    console.log('PASS: local draft created and starts in Saved state');

    // Edit the Author field (Setup tab, third text input) and deliberately do NOT blur it —
    // clicking straight into Preview from here is the exact scenario that used to lose the edit.
    const authorInput = page.locator('input').nth(2);
    await authorInput.click();
    await authorInput.type('UNBLURRED AUTHOR EDIT', { delay: 20 });
    const activeBefore = await page.evaluate(() => document.activeElement?.tagName);
    if (activeBefore !== 'INPUT') throw new Error(`Expected the Author input to be focused before clicking Preview, got activeElement=${activeBefore}`);

    // window.open() targets a real new tab; ignore rejection if the headless context blocks it —
    // we only care about what handlePreview() did to the *editor* page before that point.
    await page.click('button[title="Preview game"]').catch(() => {});
    await page.waitForTimeout(1500);

    const activeAfter = await page.evaluate(() => document.activeElement?.tagName);
    if (activeAfter === 'INPUT') throw new Error('Expected clicking Preview to blur the focused field (so its edit commits into the bridge), but it is still focused');
    console.log('PASS: clicking Preview blurs the focused field, committing its pending edit');

    await page.waitForSelector('.save-chip-saved', { timeout: 5000 });
    console.log('PASS: clicking Preview flushes the committed edit to storage (chip settles to Saved, not left Unsaved)');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
