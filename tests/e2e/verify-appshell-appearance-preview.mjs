// Checks the preview on the game's Appearance tab (AppearancePreview.svelte): sample text in the
// game's font and colours, as the v2 player shows it, in light mode and dark mode side by side.
// A new game shows the player's own light and dark colours; an accent of its own is lightened in
// dark mode; a page colour of its own makes the game look the same in both, said underneath,
// until a dark page colour gives it dark ones; a margin colour, which has no dark version, keeps
// it light; a dark-only theme is drawn dark, in its web font; font size and link underlining
// follow; and the classic interface, which v2's preview doesn't describe, has none.
// With SCREENSHOT_DIR set, it saves pictures of the preview along the way.
//
// Requires the AppShell dev server (5174).
import { chromium } from './lib/tracked-chromium.mjs';
import { join } from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const screenshotDir = process.env.SCREENSHOT_DIR;

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    const errors = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Appearance Preview Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();

    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));
    const preview = page.locator('[data-appearance-preview]');
    const colour = (field) => page.locator('[data-colour-grid]').getByRole('combobox', { name: field, exact: true });
    // (Text and Border colour are in the location bar's table too, so these are in the game's own.)
    const gameColour = (field) => page.locator('[data-colour-grid="defaultbackground"]').getByRole('combobox', { name: field, exact: true });
    const shot = async (name) => {
        if (screenshotDir) await preview.screenshot({ path: join(screenshotDir, `appearance-preview-${name}.png`) });
    };
    // What each scheme's sample shows: its page, text, link and accent colours, and its font.
    const schemes = () => preview.locator('[data-appearance-scheme]').evaluateAll(figs => figs.map(fig => {
        const pageEl = fig.querySelector('.appearance-page');
        const style = getComputedStyle(pageEl);
        const link = fig.querySelector('[data-appearance-link]');
        const accent = fig.querySelector('[data-appearance-accent]');
        return {
            scheme: fig.dataset.appearanceScheme,
            label: fig.querySelector('figcaption').textContent.trim(),
            background: style.backgroundColor,
            text: style.color,
            fontFamily: style.fontFamily,
            fontSize: style.fontSize,
            link: link ? getComputedStyle(link).color : null,
            underline: link ? getComputedStyle(link).textDecorationLine : null,
            accent: accent ? getComputedStyle(accent).color : null,
            location: !!fig.querySelector('[data-appearance-location]'),
            locationBackground: fig.querySelector('[data-appearance-location]') ? getComputedStyle(fig.querySelector('[data-appearance-location]')).backgroundColor : null,
            command: !!fig.querySelector('[data-appearance-command]'),
        };
    }));
    const fixedNote = async () => (await preview.locator('[data-appearance-fixed]').count())
        ? (await preview.locator('[data-appearance-fixed]').innerText()).trim() : null;
    const expectEqual = (what, actual, expected) => {
        if (actual !== expected) throw new Error(`${what} is ${JSON.stringify(actual)}, not ${JSON.stringify(expected)}`);
    };

    await preview.waitFor({ timeout: 5000 }).catch(() => { throw new Error('the Appearance tab has no preview'); });
    let s = await schemes();
    expectEqual('a new game\'s schemes', s.map(x => `${x.scheme}:${x.label}`).join(','), 'light:Light mode,dark:Dark mode');
    expectEqual('the light page', s[0].background, 'rgb(255, 255, 255)');
    expectEqual('the light text', s[0].text, 'rgb(26, 26, 26)');
    expectEqual('the light link (the player\'s accent)', s[0].link, 'rgb(31, 111, 120)');
    expectEqual('the dark page', s[1].background, 'rgb(24, 24, 27)');
    expectEqual('the dark text', s[1].text, 'rgb(228, 228, 231)');
    expectEqual('the dark link (the player\'s dark accent)', s[1].link, 'rgb(124, 196, 204)');
    if (!s.every(x => x.location && x.command && x.underline === 'underline')) {
        throw new Error(`a new game's preview lacks the location bar, command bar or underlined links: ${JSON.stringify(s)}`);
    }
    expectEqual('the text size', s[0].fontSize, '16px');
    if (!s[0].fontFamily.includes('Georgia')) throw new Error(`the sample text is in ${s[0].fontFamily}, not the base font Georgia`);
    expectEqual('the note under a game that follows light and dark mode', await fixedNote(), null);
    expectEqual('an empty Text colour in a new game', await gameColour('Text colour').getAttribute('placeholder'), "Player's own");
    console.log('PASS: a new game shows the player\'s own light and dark colours side by side, with the location bar, links and command bar');
    await shot('new-game');

    // An accent of its own: as given in light mode, lightened in dark mode, and still both.
    await colour('Accent colour').fill('#a31515');
    await settle();
    s = await schemes();
    expectEqual('the light link with an accent', s[0].link, 'rgb(163, 21, 21)');
    expectEqual('the light prompt with an accent', s[0].accent, 'rgb(163, 21, 21)');
    if (s.length !== 2 || s[1].accent === 'rgb(163, 21, 21)' || s[1].accent === 'rgb(124, 196, 204)') {
        throw new Error(`in dark mode, an accent of the game's own isn't lightened: ${JSON.stringify(s[1])}`);
    }
    console.log('PASS: an accent of the game\'s own is used in light mode, and lightened in dark mode');

    // A page colour of its own: the same in both modes, so one sample, and a note saying so.
    await colour('Page colour').fill('#f2f1ee');
    await settle();
    s = await schemes();
    expectEqual('the schemes with a page colour', s.map(x => `${x.scheme}:${x.label}`).join(','), 'light:Light and dark mode');
    expectEqual('the page colour', s[0].background, 'rgb(242, 241, 238)');
    expectEqual('the text with only a page colour (Quest\'s classic Black)', s[0].text, 'rgb(0, 0, 0)');
    expectEqual('the link with only a page colour (Quest\'s classic Blue)', s[0].link, 'rgb(0, 0, 255)');
    if (!(await fixedNote())?.includes('Dark column')) throw new Error(`the note under one sample is ${JSON.stringify(await fixedNote())}`);
    // The empty text and link colours say what they now are, as the sample shows.
    expectEqual('an empty Text colour with a page colour', await gameColour('Text colour').getAttribute('placeholder'), 'Black (default)');
    expectEqual('an empty Link colour with a page colour', await colour('Link colour').getAttribute('placeholder'), 'Blue (default)');
    const textSwatch = await page.locator('[data-colour-cell="defaultforeground"] .colour-swatch').first()
        .evaluate(el => getComputedStyle(el).backgroundColor);
    expectEqual('the empty Text colour\'s swatch', textSwatch, 'rgb(0, 0, 0)');
    console.log('PASS: with a page colour of its own, an empty text or link colour reads as Quest\'s Black or Blue, not the player\'s own');
    console.log('PASS: a page colour of the game\'s own shows one sample, with a note to set dark colours');
    await shot('own-colours');

    // A dark page colour: both again, the dark one with it and the player's dark text.
    await colour('Dark mode page colour').fill('#121212');
    await settle();
    await colour('Dark mode link colour').fill('#f07070');
    await settle();
    s = await schemes();
    expectEqual('the schemes with dark colours', s.map(x => x.scheme).join(','), 'light,dark');
    expectEqual('the dark page colour', s[1].background, 'rgb(18, 18, 18)');
    expectEqual('the dark text, left to the player', s[1].text, 'rgb(228, 228, 231)');
    expectEqual('the dark link colour', s[1].link, 'rgb(240, 112, 112)');
    expectEqual('the dark prompt (the dark link colour)', s[1].accent, 'rgb(240, 112, 112)');
    expectEqual('the note with dark colours', await fixedNote(), null);
    console.log('PASS: a dark page colour gives the dark sample the game\'s dark colours');
    await shot('light-and-dark');

    // Font size and link underlining.
    await page.locator('label', { hasText: /^\s*Font size:/ }).first().locator('input').fill('16');
    await settle();
    await page.getByLabel('Underline links', { exact: true }).uncheck();
    await settle();
    s = await schemes();
    if (Math.abs(parseFloat(s[0].fontSize) - (16 * 4) / 3) > 0.01) throw new Error(`the 16pt text size is ${s[0].fontSize}`);
    expectEqual('the links without underlining', s[0].underline, 'none');
    console.log('PASS: the font size and link underlining follow');

    // A margin colour without a dark version keeps the game light, and the note says which.
    await colour('Margin colour').fill('#d8d4c8');
    await settle();
    s = await schemes();
    expectEqual('the schemes with a margin colour', s.map(x => x.scheme).join(','), 'light');
    expectEqual('the note with a margin colour', await fixedNote(),
        'The game looks the same in light and dark mode. Choose a dark mode margin colour in the Dark column too, and it will follow the player’s mode.');
    const margins = () => preview.locator('.appearance-margin').evaluateAll(els => els.map(el => getComputedStyle(el).backgroundColor));
    expectEqual('the margin', (await margins())[0], 'rgb(216, 212, 200)');
    console.log('PASS: a margin colour without a dark version keeps the game light, and the note names it');

    // Two of them are both named.
    await page.locator('[data-colour-grid="customlocationcolour"]').getByRole('combobox', { name: 'Background colour', exact: true }).fill('Tan');
    await settle();
    expectEqual('the note with a margin and location bar colour', await fixedNote(),
        'The game looks the same in light and dark mode. Choose a dark mode margin colour and location bar colour in the Dark column too, and it will follow the player’s mode.');
    await colour('Dark mode location bar colour').fill('#3a2e22');
    await settle();

    // With its dark version too, the game follows light and dark mode again, with each margin.
    await colour('Dark mode margin colour').fill('#2b2b2b');
    await settle();
    s = await schemes();
    expectEqual('the schemes with a dark margin colour', s.map(x => x.scheme).join(','), 'light,dark');
    expectEqual('the margins in each mode', JSON.stringify(await margins()), JSON.stringify(['rgb(216, 212, 200)', 'rgb(43, 43, 43)']));
    expectEqual('the location bar in dark mode', s[1].locationBackground, 'rgb(58, 46, 34)');
    expectEqual('the note with dark versions', await fixedNote(), null);
    console.log('PASS: with dark versions of its margin and location bar colours, the game follows light and dark mode, each with its own');

    // A dark-only theme with a web font: drawn once, as dark, in its web font.
    await page.getByRole('group', { name: 'Theme', exact: true }).getByRole('button', { name: 'Terminal', exact: true }).click();
    await settle();
    s = await schemes();
    expectEqual('Terminal\'s schemes', s.map(x => `${x.scheme}:${x.label}`).join(','), 'dark:Light and dark mode');
    expectEqual('Terminal\'s page', s[0].background, 'rgb(11, 15, 11)');
    if (!s[0].fontFamily.includes('VT323')) throw new Error(`Terminal's sample is in ${s[0].fontFamily}, not VT323`);
    const fontLink = await page.locator('link[data-appearance-font*="VT323"]').count();
    if (fontLink !== 1) throw new Error(`Terminal's web font is linked ${fontLink} times`);
    expectEqual('the note under a dark-only theme', await fixedNote(), 'The game looks the same in light and dark mode.');
    console.log('PASS: a dark-only theme is drawn dark, in its web font, which is loaded once');
    await shot('terminal');

    // The location bar and command bar follow the Interface tab.
    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    await page.getByLabel('Show location bar', { exact: false }).first().uncheck();
    await settle();
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();
    await preview.waitFor({ timeout: 5000 });
    s = await schemes();
    if (s.some(x => x.location)) throw new Error('the preview still shows the location bar after it was turned off');
    console.log('PASS: the preview leaves out the location bar when it\'s turned off');

    // The Layout section's padding boxes share one row (<samerow/>), each caption as wide as its text
    const advanced = page.locator('summary', { hasText: 'Advanced' });
    if (!(await advanced.evaluate(el => el.closest('details')?.open))) await advanced.click();
    await page.getByLabel('Set custom padding', { exact: true }).first().check();
    await settle();
    const boxes = await Promise.all(['Top', 'Bottom', 'Left', 'Right'].map(caption =>
        page.locator('label', { hasText: new RegExp(`^\\s*${caption}:`) }).first().locator('input').boundingBox()));
    if (boxes.some(b => !b) || boxes.some(b => Math.abs(b.y - boxes[0].y) > 1) || boxes.some((b, i) => i > 0 && b.x <= boxes[i - 1].x)) {
        throw new Error(`the padding boxes aren't on one row: ${JSON.stringify(boxes)}`);
    }
    console.log('PASS: the padding boxes share one row');

    // The classic interface: no preview.
    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    const summary = page.locator('summary', { hasText: 'Advanced' });
    if (!(await summary.evaluate(el => el.closest('details')?.open))) await summary.click();
    await page.getByLabel('Use the classic interface', { exact: true }).check();
    await settle();
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();
    await page.locator('label', { hasText: /^\s*Font size:/ }).first().waitFor({ timeout: 5000 });
    if (await preview.count() !== 0) throw new Error('the classic interface still has the preview');
    console.log('PASS: the classic interface has no preview');

    if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
