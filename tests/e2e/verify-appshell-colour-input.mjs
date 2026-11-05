// Checks the editor's colour control (ColourInput.svelte), used by every "colour" control and by
// script commands' "colour" parameters: a swatch of the current colour that opens the native
// colour picker (committing #rrggbb), a text field taking any CSS colour with the colour names as
// swatched suggestions, and a distinct empty state ("Player's own" for the game's text colour,
// "Not set" otherwise), with a button to clear the colour. Runs in the light and the dark editor theme, with
// SCREENSHOT_DIR set saving a picture of the Appearance tab in each.
//
// Requires the AppShell dev server (5174).
import { chromium } from './lib/tracked-chromium.mjs';
import { join } from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const screenshotDir = process.env.SCREENSHOT_DIR;

const browser = await chromium.launch();

async function run(colorScheme) {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 }, colorScheme });
    const page = await ctx.newPage();
    const errors = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Colour Input Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();

    const control = page.locator('label', { hasText: /^\s*Text colour:/ }).first().locator('[data-colour-input]');
    // The location bar has a "Text colour" of its own, further down the tab.
    await control.waitFor({ timeout: 5000 });
    const text = control.locator('input[role="combobox"]');
    const picker = control.locator('input[type="color"]');
    const swatch = control.locator('.colour-swatch').first();
    const swatchColour = () => swatch.evaluate(el => getComputedStyle(el).backgroundColor);
    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));

    // The caption labels the text field, not the colour input layered over the swatch.
    const labelled = await page.locator('label', { hasText: /^\s*Text colour:/ }).first()
        .evaluate(el => el.control?.getAttribute('role') ?? el.control?.getAttribute('type'));
    if (labelled !== 'combobox') throw new Error(`the Text colour caption labels a ${labelled}, not the text field`);

    // As wide as the plain text fields around it (Font size, Interface font), set or not.
    const checkWidth = async (when) => {
        const fontSize = await page.locator('label', { hasText: /^\s*Font size:/ }).first().locator('input').evaluate(el => el.getBoundingClientRect().width);
        const colour = await control.evaluate(el => el.getBoundingClientRect().width);
        const field = await text.evaluate(el => el.getBoundingClientRect().width);
        if (Math.abs(colour - fontSize) > 0.5 || Math.abs(field - fontSize) > 0.5) {
            throw new Error(`${when}, the colour control is ${colour}px (its field ${field}px), Font size ${fontSize}px`);
        }
    };
    await checkWidth('before setting it');

    await text.fill('red');
    await settle();
    if (await text.inputValue() !== 'red') throw new Error(`typed "red", the field shows "${await text.inputValue()}"`);
    if (await swatchColour() !== 'rgb(255, 0, 0)') throw new Error(`the swatch for "red" is ${await swatchColour()}`);
    if (await picker.inputValue() !== '#ff0000') throw new Error(`the picker is seeded with ${await picker.inputValue()}, not #ff0000`);
    await checkWidth('with a colour set');
    console.log(`PASS [${colorScheme}]: a typed CSS colour shows on the swatch and seeds the picker`);
    console.log(`PASS [${colorScheme}]: the control is as wide as the text fields beside it`);

    await picker.fill('#336699');
    await settle();
    if (await text.inputValue() !== '#336699') throw new Error(`picking #336699 left the field at "${await text.inputValue()}"`);
    if (await swatchColour() !== 'rgb(51, 102, 153)') throw new Error(`the swatch for #336699 is ${await swatchColour()}`);
    console.log(`PASS [${colorScheme}]: the native picker writes #rrggbb`);

    await text.click();
    const listbox = page.locator('[role="listbox"]').last();
    await listbox.waitFor({ timeout: 3000 });
    const first = listbox.locator('[role="option"]').first();
    if ((await first.innerText()).trim() !== "Player's own") throw new Error(`the first suggestion is "${await first.innerText()}"`);
    const navy = listbox.locator('[role="option"]', { hasText: /^\s*Navy\s*$/ });
    const navySwatch = await navy.locator('.colour-swatch').evaluate(el => getComputedStyle(el).backgroundColor);
    if (navySwatch !== 'rgb(0, 0, 128)') throw new Error(`Navy's suggestion swatch is ${navySwatch}`);
    await navy.click();
    await settle();
    if (await text.inputValue() !== 'Navy') throw new Error(`choosing Navy left the field at "${await text.inputValue()}"`);
    console.log(`PASS [${colorScheme}]: the suggestions start with the empty state and carry swatches`);

    if (screenshotDir) {
        await text.click();
        await listbox.waitFor({ timeout: 3000 });
        await page.screenshot({ path: join(screenshotDir, `colour-input-open-${colorScheme}.png`), clip: { x: 240, y: 380, width: 600, height: 300 } });
        await page.keyboard.press('Escape');
        await settle();
        await page.mouse.move(0, 0);
        await page.locator('[role="tabpanel"], main').first().screenshot({ path: join(screenshotDir, `colour-input-${colorScheme}.png`) });
    }

    // Text colour is <nullable/>: clearing removes the attribute, so it shows the game type's
    // default - empty, shown as "Player's own", or a named colour on engines from before the
    // default text colour was emptied (#2725).
    await control.getByRole('button', { name: 'Clear colour' }).click();
    await settle();
    const cleared = await text.inputValue();
    if (cleared === "Player's own") {
        if (!await swatch.evaluate(el => el.classList.contains('colour-swatch-player'))) throw new Error('the cleared swatch is not the player\'s own one');
        if (await control.getByRole('button', { name: 'Clear colour' }).count() !== 0) throw new Error('the clear button still shows when empty');
    } else if (cleared === 'Navy' || cleared === '') {
        throw new Error(`clearing left the field showing "${cleared}"`);
    }
    console.log(`PASS [${colorScheme}]: clearing returns Text colour to its default (${cleared})`);

    // Margin colour isn't <playertheme/>: its empty state reads "Not set".
    const margin = page.locator('label', { hasText: /^\s*Margin colour:/ }).first().locator('[data-colour-input]');
    if (await margin.locator('input[role="combobox"]').inputValue() !== 'Not set') {
        throw new Error(`an empty Margin colour shows "${await margin.locator('input[role="combobox"]').inputValue()}"`);
    }
    if (!await margin.locator('.colour-swatch-unset').count()) throw new Error('an empty Margin colour has no "not set" swatch');
    console.log(`PASS [${colorScheme}]: a plain colour's empty state reads "Not set"`);

    // A script command's colour parameter uses the same control, quoting the string it writes.
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    await page.getByText('+ Add script', { exact: false }).first().click();
    await page.locator('[role="listbox"][aria-label="Script command categories"]').getByText('Output', { exact: true }).click();
    await page.locator('[role="listbox"][aria-label^="Script commands in Output"]')
        .locator('button', { hasText: 'Change foreground colour' }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.getByRole('dialog').waitFor({ state: 'hidden', timeout: 3000 });
    const scriptColour = page.locator('[data-colour-input]').first();
    await scriptColour.waitFor({ timeout: 5000 });
    const scriptText = scriptColour.locator('input[role="combobox"]');
    if (await scriptText.inputValue() !== 'Black') throw new Error(`the new command's colour shows "${await scriptText.inputValue()}"`);
    await scriptColour.locator('input[type="color"]').fill('#ff8800');
    await settle();
    await page.waitForFunction(() => document.querySelector('[data-colour-input] input[role="combobox"]')?.value === '#ff8800',
        null, { timeout: 5000 }).catch(async () => {
        throw new Error(`picking #ff8800 for the script command left "${await scriptText.inputValue()}"`);
    });
    if (await scriptColour.locator('.colour-swatch').first().evaluate(el => getComputedStyle(el).backgroundColor) !== 'rgb(255, 136, 0)') {
        throw new Error('the script command\'s swatch did not follow the picked colour');
    }
    console.log(`PASS [${colorScheme}]: a script command's colour parameter uses the colour control`);

    if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
    await ctx.close();
}

try {
    await run('light');
    await run('dark');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
