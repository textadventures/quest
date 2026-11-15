// #2663 follow-up. Add script's tint means "this is the button for the block you're
// working in", so it follows focus only: hovering Add script lights up just the frame
// of the block it adds to, and hovering Code view (which adds nothing) lights up
// nothing. Both used to tint the button, so hovering Code view lit up Add script, most
// visibly on an empty script. Also checks the row's hover delete (×) button has a
// visible border like its ↑/↓/⋯ neighbours.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('scripteditor-add-row-highlight', async ({ baseURL }) => {
    const browser = await chromium.launch();
    try {
        const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseURL}/open`);
        // Buttons fade their background in, so read colours with transitions off.
        await page.addStyleTag({ content: '*, *::before, *::after { transition: none !important; }' });
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Add Row Highlight Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 30000 });

        await page.click('text=room');
        await page.getByRole('button', { name: /^Scripts$/ }).click();
        await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });

        const addButton = page.locator('button.add-script').first();
        const codeViewButton = page.locator('button.code-view-toggle').first();
        const background = () => addButton.evaluate(el => getComputedStyle(el).backgroundColor);
        const blockShadow = () => addButton.evaluate(el => getComputedStyle(el.closest('.script-block')).boxShadow);

        await page.mouse.move(0, 0);
        const restBackground = await background();
        const restShadow = await blockShadow();

        await codeViewButton.hover();
        if (await background() !== restBackground) throw new Error(`Hovering Code view highlighted Add script on an empty script (${restBackground} → ${await background()})`);
        if (await blockShadow() !== restShadow) throw new Error('Hovering Code view highlighted the script block');

        await codeViewButton.focus();
        if (await background() !== restBackground) throw new Error('Focusing Code view highlighted Add script');

        await addButton.hover();
        if (await background() !== restBackground) throw new Error('Hovering Add script should not tint it, only its block');
        if (await blockShadow() === restShadow) throw new Error('Hovering Add script should highlight its block');

        // Focus inside the block is what tints Add script.
        await page.mouse.move(0, 0);
        await addButton.focus();
        if (await background() === restBackground) throw new Error('Focus inside the block should tint its Add script button');
        await page.locator('body').evaluate(() => document.activeElement?.blur());

        // Add a row so its hover cluster exists, then check the × button's border.
        await addButton.click();
        await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
        await page.getByRole('button', { name: 'Print', exact: true }).click();
        await page.waitForSelector('.script-row', { timeout: 5000 });

        // Focus in a script row tints its block's Add script button.
        await page.locator('.script-row').first().locator('input:not([type=checkbox]), textarea, [contenteditable=true]').first().focus();
        if (await background() === restBackground) throw new Error('Focus in a script row should tint its block\'s Add script button');

        await page.locator('.script-row').first().hover();
        const deleteButton = page.locator('div.absolute.top-1 button[title="Delete"]').first();
        const border = await deleteButton.evaluate(el => {
            const s = getComputedStyle(el);
            return { width: s.borderTopWidth, style: s.borderTopStyle, color: s.borderTopColor };
        });
        if (border.style === 'none' || border.width === '0px') throw new Error(`Row delete button has no border: ${JSON.stringify(border)}`);
    } finally {
        await browser.close();
    }
});
