// The editor's tree works from the keyboard (#2962): its rows are inside a role="tree", the
// arrow keys move between rows and open and close them, and the Menu key or Shift+F10 opens a
// row's menu (the mouse's ⋯ or right-click), which the arrow keys move through and Escape
// closes, giving the row the focus back.
//
// Requires the AppShell dev server running:
//   ./dev.sh
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('tree-keyboard', async ({ baseURL }) => {
    const browser = await chromium.launch();
    try {
        const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseURL}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Tree Keyboard ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 60000 });

        const tree = page.getByRole('tree');
        if (await tree.count() !== 1) throw new Error(`Expected one tree, found ${await tree.count()}`);
        const gameRow = page.locator('[data-value="game"][data-part="branch-control"]');
        if (await tree.locator('[data-value="game"][data-part="branch-control"]').count() !== 1) {
            throw new Error('Expected the game row inside the tree');
        }

        const focusedValue = () => page.evaluate(() =>
            document.activeElement?.closest('[data-part="branch-control"], [data-part="item"]')?.getAttribute('data-value') ?? null);
        const expanded = value => page.locator(`[data-value="${value}"][data-part="branch"]`).getAttribute('aria-expanded');

        // Arrow keys: Right opens the game row, Down moves to its first child, Left goes back
        await gameRow.click();
        if (await focusedValue() !== 'game') throw new Error(`Expected the game row focused, got ${await focusedValue()}`);
        if (await expanded('game') !== 'false') {
            await page.keyboard.press('ArrowLeft');
            if (await expanded('game') !== 'false') throw new Error('Expected ArrowLeft to close the game row');
        }
        await page.keyboard.press('ArrowRight');
        if (await expanded('game') !== 'true') throw new Error('Expected ArrowRight to open the game row');
        await page.keyboard.press('ArrowDown');
        const child = await focusedValue();
        if (!child || child === 'game') throw new Error(`Expected ArrowDown to move into the game row's children, got ${child}`);
        await page.keyboard.press('ArrowLeft');
        if (await focusedValue() !== 'game') throw new Error(`Expected ArrowLeft on a child to go back to the game row, got ${await focusedValue()}`);

        // The row's menu from the keyboard
        await page.keyboard.press('Shift+F10');
        const menu = page.locator('.tree-dropdown[role="menu"]');
        await menu.waitFor({ state: 'visible', timeout: 5000 });
        const items = menu.getByRole('menuitem');
        if (await items.count() < 2) throw new Error(`Expected the game row's menu to have items, found ${await items.count()}`);
        const focusedItem = () => page.evaluate(() => document.activeElement?.getAttribute('role') === 'menuitem' ? document.activeElement.textContent : null);
        const first = await items.nth(0).textContent();
        const second = await items.nth(1).textContent();
        if (await focusedItem() !== first) throw new Error(`Expected the first item "${first}" focused, got ${await focusedItem()}`);
        await page.keyboard.press('ArrowDown');
        if (await focusedItem() !== second) throw new Error(`Expected ArrowDown to focus "${second}", got ${await focusedItem()}`);
        await page.keyboard.press('ArrowUp');
        await page.keyboard.press('ArrowUp');
        const last = await items.last().textContent();
        if (await focusedItem() !== last) throw new Error(`Expected ArrowUp from the first item to wrap to "${last}", got ${await focusedItem()}`);

        await page.keyboard.press('Escape');
        await menu.waitFor({ state: 'detached', timeout: 5000 });
        if (await focusedValue() !== 'game') throw new Error(`Expected Escape to give the game row the focus back, got ${await focusedValue()}`);

        // The ⋯ and the chevron are for the mouse only, out of the tab order and hidden
        // from assistive technology, since a tree row can't hold controls of its own
        const options = gameRow.locator('[data-node-options]');
        if (await options.getAttribute('aria-hidden') !== 'true') throw new Error('Expected the ⋯ hidden from assistive technology');
        if (await gameRow.locator('button, [tabindex="0"]').count() !== 0) throw new Error('Expected no focusable controls inside the game row');
        await options.click({ force: true });
        await menu.waitFor({ state: 'visible', timeout: 5000 });
        await page.keyboard.press('Escape');
        await menu.waitFor({ state: 'detached', timeout: 5000 });
    } finally {
        await browser.close();
    }
});
