// Checks that choosing a theme on the game's Appearance tab clears the author's own values for
// anything a theme sets (the Theme control's <resetoverrides/>), so the new theme shows: a page
// colour set under Quest Standard gives way to Retro's black, a toast names what was replaced,
// and one Undo brings back both the old theme and the author's colour. An option no theme sets
// (the accent colour) is left alone.
//
// Requires the AppShell dev server (5174).
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    const errors = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Theme Reset Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();

    const field = (caption) => page.locator('label', { hasText: new RegExp(`^\\s*${caption}:`) }).first()
        .locator('[data-colour-input] input[role="combobox"]');
    const theme = page.locator('label', { hasText: /^\s*Theme:/ }).first().locator('select');
    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));

    await field('Page colour').fill('pink');
    await settle();
    await field('Accent colour').fill('purple');
    await settle();

    await theme.selectOption({ label: 'Retro' });
    await page.waitForTimeout(300);

    const toast = page.getByRole('alert').filter({ hasText: 'The new theme replaced' });
    await toast.waitFor({ timeout: 5000 });
    const toastText = (await toast.locator('span').innerText()).trim();
    if (!toastText.includes('Page colour')) throw new Error(`the toast doesn't name the page colour: "${toastText}"`);
    if (toastText.includes('Accent colour')) throw new Error(`the toast names the accent colour, which no theme sets: "${toastText}"`);
    console.log(`PASS: choosing Retro says what it replaced ("${toastText}")`);

    const page1 = await field('Page colour').inputValue();
    if (page1.toLowerCase() !== 'black') throw new Error(`after choosing Retro the page colour is "${page1}", not Retro's Black`);
    const accent1 = await field('Accent colour').inputValue();
    if (accent1 !== 'purple') throw new Error(`choosing Retro changed the accent colour to "${accent1}"`);
    console.log('PASS: Retro\'s page colour shows, and the accent colour is kept');

    await page.getByRole('button', { name: 'Undo', exact: true }).click();
    await page.waitForTimeout(300);
    const themeAfterUndo = await theme.evaluate(el => el.selectedOptions[0]?.textContent?.trim());
    if (themeAfterUndo !== 'Quest Standard') throw new Error(`after Undo the theme is "${themeAfterUndo}", not Quest Standard`);
    const page2 = await field('Page colour').inputValue();
    if (page2 !== 'pink') throw new Error(`after Undo the page colour is "${page2}", not pink`);
    console.log('PASS: one Undo brings back the old theme and the page colour');

    if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser.close();
}
