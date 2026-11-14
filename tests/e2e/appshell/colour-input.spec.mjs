// Checks the editor's colour control (ColourInput.svelte), used by every "colour" control and by
// script commands' "colour" parameters: a text field taking any CSS colour, which keeps its text
// when clicked so it can be edited, with a swatch that follows what's typed and the colour names
// as swatched suggestions (as you type, or from its ▾ button); a "Pick a colour" button beside it
// that opens the native colour picker (committing #rrggbb); and a distinct empty state ("Player's
// own" for the game's text colour, "Not set" otherwise), reached with the clear button or by
// emptying the field; and the toolbar's Undo and Redo reaching it. Runs in the light and the dark editor theme, with SCREENSHOT_DIR set saving
// a picture of the Appearance tab in each.
//
// Requires the AppShell dev server (5174).
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';
import { join } from 'node:path';

test('colour-input', async ({ baseURL }) => {
    const baseUrl = baseURL;
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

        // Text colour sits in the Light column of the Light/Dark colour table (data-colour-grid).
        const caption = page.locator('[data-colour-grid] [data-colour-cell="defaultforeground"]');
        const control = caption.locator('[data-colour-input]');
        await control.waitFor({ timeout: 5000 });
        const text = control.locator('input[role="combobox"]');
        const picker = caption.locator('input[type="color"]');
        const swatch = control.locator('.colour-swatch').first();
        const swatchColour = () => swatch.evaluate(el => getComputedStyle(el).backgroundColor);
        const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
            .then(() => page.waitForTimeout(300));

        // The text field carries the caption as its name, not the colour input layered over the swatch.
        const named = await page.locator('[data-colour-grid="defaultbackground"]').getByRole('combobox', { name: 'Text colour', exact: true }).count();
        if (named !== 1) throw new Error(`${named} text fields in the colour table are named "Text colour"`);

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

        // The picker is a visible button beside the field, and it (and the swatch) really open it: a
        // real click reaches them and calls showPicker. (The colour input used to be laid invisibly
        // over the swatch, and never got the click.)
        const pickButton = caption.getByRole('button', { name: 'Pick a colour' });
        const pickBox = await pickButton.boundingBox();
        const fieldBox = await control.boundingBox();
        if (!pickBox || pickBox.width < 16 || pickBox.x < fieldBox.x + fieldBox.width) {
            throw new Error(`the Pick a colour button isn't a visible button after the field: ${JSON.stringify({ pickBox, fieldBox })}`);
        }
        await page.evaluate(() => {
            window.__pickerOpened = [];
            const showPicker = HTMLInputElement.prototype.showPicker;
            HTMLInputElement.prototype.showPicker = function () {
                showPicker.call(this);
                window.__pickerOpened.push(this.type);
            };
        });
        await pickButton.click();
        await control.locator('button[title="Pick a colour"]').click();
        const opened = await page.evaluate(() => window.__pickerOpened);
        if (JSON.stringify(opened) !== '["color","color"]') throw new Error(`clicking the button and the swatch opened the picker: ${JSON.stringify(opened)}`);
        await page.keyboard.press('Escape');
        await picker.fill('#336699');
        await settle();
        if (await text.inputValue() !== '#336699') throw new Error(`picking #336699 left the field at "${await text.inputValue()}"`);
        if (await swatchColour() !== 'rgb(51, 102, 153)') throw new Error(`the swatch for #336699 is ${await swatchColour()}`);
        console.log(`PASS [${colorScheme}]: clicking the Pick a colour button beside the field, or the swatch, opens the native picker, which writes #rrggbb`);

        // The toolbar's Undo and Redo reach it. The stylesheet editor further down the tab used to
        // take them over for its own (empty) text history, so nothing on the tab could be undone.
        await page.getByRole('button', { name: 'Undo', exact: true }).click();
        await page.waitForTimeout(300);
        if (await text.inputValue() !== 'red' || await swatchColour() !== 'rgb(255, 0, 0)') {
            throw new Error(`Undo left "${await text.inputValue()}" with a ${await swatchColour()} swatch, not red`);
        }
        await page.getByRole('button', { name: 'Redo', exact: true }).click();
        await page.waitForTimeout(300);
        if (await text.inputValue() !== '#336699' || await swatchColour() !== 'rgb(51, 102, 153)') {
            throw new Error(`Redo left "${await text.inputValue()}" with a ${await swatchColour()} swatch, not #336699`);
        }
        console.log(`PASS [${colorScheme}]: the toolbar's Undo and Redo change the colour back and forth`);

        // Clicking keeps the text, so it can be edited in place, and the swatch follows the typing
        // before it's committed.
        await text.click();
        if (await text.inputValue() !== '#336699') throw new Error(`clicking the field changed it to "${await text.inputValue()}"`);
        await page.keyboard.press('End');
        await page.keyboard.press('Backspace');
        await page.keyboard.press('Backspace');
        await page.keyboard.type('00');
        if (await text.inputValue() !== '#336600') throw new Error(`editing gave "${await text.inputValue()}"`);
        if (await swatchColour() !== 'rgb(51, 102, 0)') throw new Error(`while typing #336600 the swatch is ${await swatchColour()}`);
        await page.keyboard.press('Escape');
        if (await text.inputValue() !== '#336699' || await swatchColour() !== 'rgb(51, 102, 153)') {
            throw new Error(`Escape left "${await text.inputValue()}" with a ${await swatchColour()} swatch`);
        }
        await page.keyboard.press('End');
        await page.keyboard.press('Backspace');
        await page.keyboard.press('Backspace');
        await page.keyboard.type('00');
        await page.keyboard.press('Enter');
        await settle();
        if (await text.inputValue() !== '#336600') throw new Error(`Enter committed "${await text.inputValue()}"`);
        console.log(`PASS [${colorScheme}]: the field keeps its text on click, can be edited, previews as you type, and Escape undoes it`);

        // Typing suggests the colour names that match.
        await text.click();
        await page.keyboard.press(process.platform === 'darwin' ? 'Meta+A' : 'Control+A');
        await page.keyboard.type('nav');
        const suggestions = await page.locator('[role="listbox"]').last().locator('[role="option"]').allInnerTexts();
        if (!suggestions.map(o => o.trim()).includes('Navy') || suggestions.length > 3) {
            throw new Error(`typing "nav" suggests ${JSON.stringify(suggestions)}`);
        }
        await page.keyboard.press('Escape');
        await settle();
        console.log(`PASS [${colorScheme}]: typing suggests the matching colour names`);

        await control.getByRole('button', { name: 'Show colour names' }).click();
        const listbox = page.locator('[role="listbox"]').last();
        await listbox.waitFor({ timeout: 3000 });
        const first = listbox.locator('[role="option"]').first();
        if ((await first.innerText()).trim() !== "Default") throw new Error(`the first suggestion is "${await first.innerText()}"`);
        const navy = listbox.locator('[role="option"]', { hasText: /^\s*Navy\s*$/ });
        const navySwatch = await navy.locator('.colour-swatch').evaluate(el => getComputedStyle(el).backgroundColor);
        if (navySwatch !== 'rgb(0, 0, 128)') throw new Error(`Navy's suggestion swatch is ${navySwatch}`);
        await navy.click();
        await settle();
        if (await text.inputValue() !== 'Navy') throw new Error(`choosing Navy left the field at "${await text.inputValue()}"`);
        console.log(`PASS [${colorScheme}]: the suggestions start with the empty state and carry swatches`);

        if (screenshotDir) {
            await control.getByRole('button', { name: 'Show colour names' }).click();
            await listbox.waitFor({ timeout: 3000 });
            await page.screenshot({ path: join(screenshotDir, `colour-input-open-${colorScheme}.png`), clip: { x: 240, y: 380, width: 600, height: 300 } });
            await page.keyboard.press('Escape');
            await settle();
            await page.mouse.move(0, 0);
            await page.locator('[role="tabpanel"], main').first().screenshot({ path: join(screenshotDir, `colour-input-${colorScheme}.png`) });
        }

        // Text colour is <nullable/>: clearing removes the attribute, so it shows the game type's
        // default - empty, shown as "Default", or a named colour on engines from before the
        // default text colour was emptied (#2725). Clearing is the clear button, or emptying the
        // field.
        const checkCleared = async (how) => {
            const cleared = await text.inputValue();
            if (cleared === '') {
                if (await text.getAttribute('placeholder') !== "Default") throw new Error(`${how}, the empty field reads "${await text.getAttribute('placeholder')}"`);
                if (!await swatch.evaluate(el => el.classList.contains('colour-swatch-player'))) throw new Error(`${how}, the swatch is not the player's own one`);
                if (await control.getByRole('button', { name: 'Clear colour' }).count() !== 0) throw new Error(`${how}, the clear button still shows`);
            } else if (cleared === 'Navy') {
                throw new Error(`${how} left the field showing "${cleared}"`);
            }
            console.log(`PASS [${colorScheme}]: ${how} returns Text colour to its default (${cleared || "Default"})`);
        };
        await control.getByRole('button', { name: 'Clear colour' }).click();
        await settle();
        await checkCleared('the clear button');

        await text.fill('Navy');
        await settle();
        await text.click();
        await page.keyboard.press(process.platform === 'darwin' ? 'Meta+A' : 'Control+A');
        await page.keyboard.press('Backspace');
        await settle();
        await checkCleared('emptying the field');

        // Margin colour isn't <playertheme/>: its empty state reads "Not set".
        const margin = page.locator('[data-colour-cell="marginscolour"] [data-colour-input]');
        if (await margin.locator('input[role="combobox"]').getAttribute('placeholder') !== 'Not set') {
            throw new Error(`an empty Margin colour reads "${await margin.locator('input[role="combobox"]').getAttribute('placeholder')}"`);
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
        await scriptColour.locator('xpath=..').locator('input[type="color"]').fill('#ff8800');
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
        throw err;
    } finally {
        await browser.close();
    }
});
