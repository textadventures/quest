// Checks that choosing a theme on the game's Appearance tab clears the author's own values for
// anything a theme sets and the rest of the look (the Theme control's <resetoverrides/> and
// <resetattributes>), so the new theme shows: a page colour and margin colour set under Quest
// Standard give way to Retro's, a toast names what was replaced, and one Undo brings back both the
// old theme and the author's colours. Then the Reset button beside the Theme: disabled while
// nothing differs from the theme, it clears a colour changed under Retro and keeps Retro.
//
// The Theme is a ThemePicker: a group of buttons, each a preview in the theme's colours, with the
// chosen one pressed. The arrow keys move between them without choosing, Enter chooses, and the
// retired Hot Dog Stand isn't offered to a new game.
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
    const picker = page.getByRole('group', { name: 'Theme', exact: true });
    const themeButton = (name) => picker.getByRole('button', { name, exact: true });
    const chosenTheme = () => picker.locator('button[aria-pressed="true"] .theme-name').innerText().then((t) => t.trim());
    const chooseTheme = (name) => themeButton(name).click();

    const offered = await picker.locator('.theme-card .theme-name').allInnerTexts();
    for (const name of ['Quest Standard', 'Parchment', 'Noir', 'Storybook', 'Novella', 'Typewriter', 'Midnight', 'Terminal', 'Retro']) {
        if (!offered.some((t) => t.trim() === name)) throw new Error(`the Theme picker doesn't offer ${name}: ${JSON.stringify(offered)}`);
    }
    if (offered.some((t) => /hot dog/i.test(t))) throw new Error('the retired Hot Dog Stand is offered to a new game');
    if (await chosenTheme() !== 'Quest Standard') throw new Error(`a new game's theme is "${await chosenTheme()}"`);
    const swatches = await themeButton('Parchment').locator('.theme-half').evaluateAll((els) => els.map((el) => getComputedStyle(el).backgroundColor));
    if (JSON.stringify(swatches) !== JSON.stringify(['rgb(250, 243, 227)', 'rgb(42, 33, 25)'])) {
        throw new Error(`Parchment's preview isn't its light and dark pages: ${JSON.stringify(swatches)}`);
    }
    const midnight = await themeButton('Midnight').locator('.theme-half').count();
    if (midnight !== 1) throw new Error(`Midnight, dark only, is previewed in ${midnight} halves`);
    console.log('PASS: the picker offers every theme but Hot Dog Stand, previewed in its colours (light and dark, or one)');

    // The keyboard: one tab stop, arrows move without choosing, Enter chooses
    const tabStops = await picker.locator('button[tabindex="0"]').count();
    if (tabStops !== 1) throw new Error(`the picker has ${tabStops} tab stops`);
    await themeButton('Quest Standard').focus();
    await page.keyboard.press('ArrowRight');
    const focused = await page.evaluate(() => document.activeElement?.getAttribute('data-theme-value'));
    if (focused !== 'theme_parchment') throw new Error(`ArrowRight moved to ${focused}, not Parchment`);
    if (await chosenTheme() !== 'Quest Standard') throw new Error('ArrowRight chose a theme rather than just moving');
    await page.keyboard.press('Enter');
    await page.waitForTimeout(300);
    if (await chosenTheme() !== 'Parchment') throw new Error(`Enter chose "${await chosenTheme()}", not Parchment`);
    await page.getByRole('button', { name: 'Undo', exact: true }).click();
    await page.waitForTimeout(300);
    if (await chosenTheme() !== 'Quest Standard') throw new Error('Undo didn\'t bring back Quest Standard');
    console.log('PASS: the arrow keys move between themes, Enter chooses one');
    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));

    const reset = page.getByRole('button', { name: 'Reset', exact: true });
    const resetDisabled = () => reset.isDisabled();
    if (!await resetDisabled()) throw new Error('Reset is enabled on a new game, with nothing to reset');

    await field('Page colour').fill('pink');
    await settle();
    await field('Margin colour').fill('green');
    await settle();
    if (await resetDisabled()) throw new Error('Reset is still disabled with a page colour set');

    await chooseTheme('Retro');
    await page.waitForTimeout(300);

    const toast = page.getByRole('alert').filter({ hasText: 'The new theme replaced' });
    await toast.waitFor({ timeout: 5000 });
    const toastText = (await toast.locator('span').innerText()).trim();
    if (!toastText.includes('Page colour') || !toastText.includes('Margin colour')) {
        throw new Error(`the toast doesn't name the page and margin colours: "${toastText}"`);
    }
    console.log(`PASS: choosing Retro says what it replaced ("${toastText}")`);

    const page1 = await field('Page colour').inputValue();
    if (page1.toLowerCase() !== 'black') throw new Error(`after choosing Retro the page colour is "${page1}", not Retro's Black`);
    const margin1 = await field('Margin colour').inputValue();
    if (margin1 !== '') throw new Error(`after choosing Retro the margin colour is still "${margin1}"`);
    console.log('PASS: Retro\'s page colour shows, and the margin colour is cleared');

    await page.getByRole('button', { name: 'Undo', exact: true }).click();
    await page.waitForTimeout(300);
    const themeAfterUndo = await chosenTheme();
    if (themeAfterUndo !== 'Quest Standard') throw new Error(`after Undo the theme is "${themeAfterUndo}", not Quest Standard`);
    const page2 = await field('Page colour').inputValue();
    if (page2 !== 'pink') throw new Error(`after Undo the page colour is "${page2}", not pink`);
    console.log('PASS: one Undo brings back the old theme and the page colour');

    await chooseTheme('Retro');
    await page.waitForTimeout(300);
    if (!await resetDisabled()) throw new Error('Reset is enabled straight after choosing Retro');
    await field('Text colour').fill('lime');
    await settle();
    if (await resetDisabled()) throw new Error('Reset is disabled after changing Retro\'s text colour');
    const hint = await reset.getAttribute('title');
    if (!hint?.includes('Text colour')) throw new Error(`Reset's hint doesn't name the text colour: "${hint}"`);
    await reset.click();
    await page.waitForTimeout(300);
    const text3 = await field('Text colour').inputValue();
    if (text3.toLowerCase() !== 'white') throw new Error(`after Reset the text colour is "${text3}", not Retro's White`);
    const theme3 = await chosenTheme();
    if (theme3 !== 'Retro') throw new Error(`Reset changed the theme to "${theme3}"`);
    if (!await resetDisabled()) throw new Error('Reset is still enabled with nothing left to reset');
    await page.getByRole('alert').filter({ hasText: 'Cleared your own setting for Text colour' }).waitFor({ timeout: 5000 });
    console.log('PASS: Reset clears the text colour back to Retro\'s, keeps Retro, and then disables');

    if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser.close();
}
