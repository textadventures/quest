// The toolbar's File, Add and More menus open from their button's left edge, as a menu
// bar's do, and flip to its right edge only where that would go off the screen. Also
// checks File comes first in the toolbar's right-hand group, and that the buttons don't run
// over the game's name and save state.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('toolbar-menu-alignment', async ({ baseURL }) => {
    const browser = await chromium.launch();

    async function openEditor(viewport) {
        const context = await browser.newContext({ viewport });
        const page = await context.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));
        await page.goto(`${baseURL}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Toolbar Menu Alignment ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 30000 });
        return page;
    }

    // Opens the menu behind `trigger`, and returns which of the trigger's edges it lines up with
    async function menuSide(page, trigger) {
        await trigger.click();
        const menu = page.getByRole('menu');
        await menu.waitFor();
        const t = await trigger.boundingBox();
        const m = await menu.boundingBox();
        const width = page.viewportSize().width;
        await page.keyboard.press('Escape');
        await menu.waitFor({ state: 'detached' });
        if (m.x < -0.5 || m.x + m.width > width + 0.5) throw new Error(`Menu off screen: x=${m.x}, width=${m.width}, viewport ${width}`);
        if (Math.abs(m.x - t.x) < 1) return 'left';
        if (Math.abs(m.x + m.width - (t.x + t.width)) < 1) return 'right';
        throw new Error(`Menu (x=${m.x}) lines up with neither edge of its trigger (x=${t.x}, width=${t.width})`);
    }

    async function expectSide(page, name, trigger, expected, label) {
        const side = await menuSide(page, trigger);
        if (side !== expected) throw new Error(`${label}: expected the ${name} menu to open from its button's ${expected} edge, got ${side}`);
        console.log(`PASS: ${label}: ${name} menu opens from the ${side} edge`);
    }

    // Which edge a menu (w-56, 224px) should line up with: the left, unless it would go off the right
    async function expectedSide(page, trigger) {
        const t = await trigger.boundingBox();
        return t.x + 224 <= page.viewportSize().width ? 'left' : 'right';
    }

    try {
        for (const [width, height] of [[1440, 900], [1024, 900], [800, 900], [768, 900], [375, 812]]) {
            const label = `${width}px`;
            const page = await openEditor({ width, height });
            const chip = await page.locator('.save-chip').boundingBox();
            const trail = await page.locator('[data-scope="app-bar"][data-part="trail"]').boundingBox();
            if (chip.x + chip.width > trail.x) throw new Error(`${label}: the toolbar's buttons (from x=${trail.x}) overlap the save chip (to x=${chip.x + chip.width})`);
            console.log(`PASS: ${label}: the toolbar's buttons don't overlap the save chip`);
            const menus = [['File', page.getByRole('button', { name: 'File' })],
                ['Add', page.getByRole('button', { name: /^Add/ })],
                ['More', page.getByRole('button', { name: 'More' })]];
            if (width >= 768) {
                const [fileBox, addBox] = [await menus[0][1].boundingBox(), await menus[1][1].boundingBox()];
                if (!(fileBox.x < addBox.x)) throw new Error(`${label}: expected File to come before Add in the toolbar`);
                console.log(`PASS: ${label}: File comes before Add`);
            }
            const sides = {};
            for (const [name, trigger] of menus) {
                if (!(await trigger.isVisible())) continue;
                const expected = await expectedSide(page, trigger);
                await expectSide(page, name, trigger, expected, label);
                sides[name] = expected;
            }
            // The cases this is about: from the left at a wide width, flipping near the right edge
            if (width === 1440 && Object.values(sides).some(s => s !== 'left')) throw new Error(`${label}: expected every menu to open from the left edge`);
            if (width <= 800 && sides.More !== 'right') throw new Error(`${label}: expected the More menu to flip to the right edge`);
            await page.context().close();
        }
    } finally {
        await browser.close();
    }
});
