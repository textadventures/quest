// Issue #2683: a library element (here a Core function, shown with "Show Library Elements" on) is
// view-only, but its editor panel must still scroll with the mouse wheel, so a long library script
// can be read to the end. It used to set pointer-events: none on the scrolling panel itself, which
// stopped the wheel as well as clicks. Also checks that the controls still don't take a click.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('library-element-scroll', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const FUNCTION_NAME = 'HandleSingleCommandPattern';

    const browser = await chromium.launch();

    try {
        const page = await browser.newPage({ viewport: { width: 1280, height: 600 } });
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseUrl}/open`);
        await page.waitForSelector('input[type="text"]', { timeout: 30000 });
        await page.fill('input[type="text"]', 'Library Scroll Test');
        // The template list (the language dropdown) fills in once WasmEditor has loaded.
        await page.waitForFunction(() => (document.querySelector('select')?.options.length ?? 0) > 0, null, { timeout: 60000 });
        await page.locator('button.preset-filled-primary-500').last().click();
        await page.waitForSelector('header .toolbar-icon-btn', { timeout: 60000 });

        // Turn on "Show Library Elements" (the tree's view-options menu).
        await page.locator('#workspace-content button[aria-label]').first().click();
        await page.click('[role="menu"] button');

        // Functions live under the collapsed Advanced header; filtering the tree reveals the match.
        await page.fill('input[placeholder="Filter..."]', FUNCTION_NAME);
        const treeRow = page.locator('[data-scope="tree-view"] [data-part="item"]').filter({ hasText: FUNCTION_NAME }).last();
        await treeRow.waitFor({ timeout: 10000 });
        await treeRow.click();
        await page.waitForSelector('.bg-warning-100-900', { timeout: 10000 }); // LibraryElementBanner

        // The greyed-out panel (the second selector is how it looked before the fix, so this script
        // fails on the wheel check, not on finding the panel, if the bug comes back that way).
        const PANEL = '.read-only-controls, .flex-1.pointer-events-none.opacity-60';
        const panel = page.locator(PANEL).first();
        await panel.waitFor({ timeout: 10000 });

        // Wheel over the middle of the panel, then find whichever element scrolled.
        const box = await panel.boundingBox();
        if (!box) throw new Error('read-only panel has no bounding box');
        const scrolledBefore = await page.evaluate(sel => {
                const root = document.querySelector(sel);
                return [root, ...root.querySelectorAll('*')].reduce((n, el) => n + el.scrollTop, 0);
            }, PANEL);
        if (scrolledBefore !== 0) throw new Error(`expected the panel to start unscrolled, got ${scrolledBefore}`);
        await page.mouse.move(box.x + box.width / 2, box.y + Math.min(box.height / 2, 250));
        await page.mouse.wheel(0, 600);
        await page.waitForTimeout(500);
        const scrolledAfter = await page.evaluate(sel => {
                const root = document.querySelector(sel);
                return [root, ...root.querySelectorAll('*')].reduce((n, el) => n + el.scrollTop, 0);
            }, PANEL);
        if (scrolledAfter <= 0) throw new Error(`mouse wheel didn't scroll the ${FUNCTION_NAME} library function panel`);
        console.log(`PASS: wheel scrolled the library function panel by ${scrolledAfter}px`);

        // Controls stay view-only: whatever is under the pointer in the panel is a scroller or the
        // panel itself, never a control inside it.
        const hit = await page.evaluate(({ x, y }) => {
            const el = document.elementFromPoint(x, y);
            return { tag: el?.tagName, pointerEvents: el ? getComputedStyle(el).pointerEvents : null };
        }, { x: box.x + box.width / 2, y: box.y + Math.min(box.height / 2, 250) });
        if (['INPUT', 'TEXTAREA', 'SELECT', 'BUTTON'].includes(hit.tag ?? '') || hit.pointerEvents !== 'auto') {
            throw new Error(`a control in the read-only panel takes the pointer: ${JSON.stringify(hit)}`);
        }
        const anyClickable = await page.evaluate(sel =>
            Array.from(document.querySelector(sel).querySelectorAll(':is(input, textarea, select, button, [contenteditable="true"])'))
                .filter(el => getComputedStyle(el).pointerEvents !== 'none').length, PANEL);
        if (anyClickable !== 0) throw new Error(`${anyClickable} controls in the read-only panel still take clicks`);
        console.log('PASS: controls in the library element panel still take no clicks');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
