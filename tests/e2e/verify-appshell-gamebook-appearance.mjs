// Checks a gamebook's Appearance tab (GamebookCoreEditor.aslx, #2656): the Theme picker, without
// the retired themes; the preview, drawn as a page with its choices rather than the text
// adventure's location bar and command bar; the Light/Dark colour table, including the border
// colour that outlines the choices; and the layout options. In a running Preview, a theme's
// colours and the border colour reach the player straight away.
// With SCREENSHOT_DIR set, it saves a picture of the tab.
//
// Requires both dev servers running (./dev.sh): AppShell on 5174, proxying /player to WasmPlayer.
import { chromium } from './lib/tracked-chromium.mjs';
import { join } from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const screenshotDir = process.env.SCREENSHOT_DIR;

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 }, colorScheme: 'light' });
    const page = await ctx.newPage();
    const errors = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Gamebook Appearance Test ${Date.now()}`);
    await page.waitForSelector('text=Game type', { timeout: 10000 });
    await page.locator('label:has-text("Gamebook") input[type="radio"]').check();
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 30000 });
    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });

    const expectEqual = (what, actual, expected) => {
        if (JSON.stringify(actual) !== JSON.stringify(expected)) {
            throw new Error(`${what} is ${JSON.stringify(actual)}, not ${JSON.stringify(expected)}`);
        }
    };
    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));

    // A Preview of the gamebook, running while the tab is changed
    const [preview] = await Promise.all([
        ctx.waitForEvent('page'),
        page.click('button[title="Preview game"]'),
    ]);
    const previewErrors = [];
    preview.on('pageerror', err => previewErrors.push(err.message));
    await preview.locator('qv-player[data-qv-gamebook] a.commandlink').first().waitFor({ timeout: 60000 });
    console.log('PASS: the gamebook is running in Preview');

    await page.locator('[data-value="game"]:is([data-part="branch-control"], [data-part="item"])').first().click();
    if (await page.getByRole('button', { name: 'Display', exact: true }).count()) {
        throw new Error('the gamebook still has a Display tab');
    }
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();

    // The Theme picker: every current theme, none of the retired ones
    const themes = page.getByRole('group', { name: 'Theme', exact: true });
    await themes.waitFor({ timeout: 5000 });
    const names = (await themes.locator('[data-theme-value]').evaluateAll(els => els.map(el => el.dataset.themeValue)));
    expectEqual('the themes offered to a gamebook', names,
        ['*', 'theme_parchment', 'theme_noir', 'theme_storybook', 'theme_novella', 'theme_typewriter', 'theme_midnight', 'theme_terminal']);
    console.log('PASS: the gamebook is offered every theme but the retired ones');

    // The preview: a page with its choices, in light and dark mode
    const appearance = page.locator('[data-appearance-preview]');
    const samples = () => appearance.locator('[data-appearance-scheme]').evaluateAll(figs => figs.map(fig => {
        const pageEl = fig.querySelector('.appearance-page');
        const choice = fig.querySelector('[data-appearance-choice]');
        return {
            scheme: fig.dataset.appearanceScheme,
            background: getComputedStyle(pageEl).backgroundColor,
            fontFamily: getComputedStyle(pageEl).fontFamily,
            choices: fig.querySelectorAll('[data-appearance-choice]').length,
            choiceBorder: choice ? getComputedStyle(choice).borderTopColor : null,
            choiceText: choice ? getComputedStyle(choice).color : null,
            location: !!fig.querySelector('[data-appearance-location]'),
            command: !!fig.querySelector('[data-appearance-command]'),
        };
    }));
    let s = await samples();
    expectEqual('a new gamebook\'s schemes', s.map(x => x.scheme), ['light', 'dark']);
    if (!s.every(x => x.choices === 2 && !x.location && !x.command)) {
        throw new Error(`the gamebook preview isn't a page with two choices: ${JSON.stringify(s)}`);
    }
    if (!s[0].fontFamily.includes('Georgia')) throw new Error(`the sample is in ${s[0].fontFamily}, not Georgia`);
    console.log('PASS: the preview shows a page with its choices, in light and dark mode, with no location or command bar');

    // The colour table: no colour is hidden by a text adventure option the gamebook doesn't have
    const colour = (field) => page.locator('[data-colour-grid]').getByRole('combobox', { name: field, exact: true });
    for (const field of ['Page colour', 'Text colour', 'Link colour', 'Accent colour', 'Margin colour', 'Border colour']) {
        if (!(await colour(field).count())) throw new Error(`the colour table has no ${field}`);
    }
    console.log('PASS: the Light/Dark table has the page, text, link, accent, margin and border colours');

    // A theme: its colours in the preview, and in the running Preview
    await themes.getByRole('button', { name: 'Parchment', exact: true }).click();
    await settle();
    s = await samples();
    expectEqual('Parchment\'s light page', s[0].background, 'rgb(250, 243, 227)');
    expectEqual('Parchment\'s dark page', s[1].background, 'rgb(42, 33, 25)');
    expectEqual('Parchment\'s light choice', s[0].choiceText, 'rgb(138, 59, 18)');
    await preview.waitForFunction(() => document.querySelector('qv-player')?.style.getPropertyValue('--qv-bg-light').trim() === '#faf3e3',
        null, { timeout: 10000 }).catch(async () => {
        throw new Error(`Preview didn't get Parchment's page colour: ${await preview.evaluate(() => document.querySelector('qv-player')?.getAttribute('style'))}`);
    });
    console.log('PASS: choosing Parchment shows in the preview and the running Preview');

    // The border colour outlines the choices, in the preview and the running Preview
    await colour('Border colour').fill('#336699');
    await settle();
    s = await samples();
    expectEqual('the light choice border', s[0].choiceBorder, 'rgb(51, 102, 153)');
    expectEqual('the dark choice border (the light one, with no dark one)', s[1].choiceBorder, 'rgb(51, 102, 153)');
    await preview.waitForFunction(() => {
        const player = document.querySelector('qv-player');
        const choice = document.querySelector('[data-qv-sections] a.commandlink:not(.disabled)');
        return player && choice && getComputedStyle(choice).borderTopColor === 'rgb(51, 102, 153)';
    }, null, { timeout: 10000 }).catch(async () => {
        throw new Error(`Preview's choices aren't outlined in the border colour: ${await preview.evaluate(() => document.querySelector('qv-player')?.getAttribute('style'))}`);
    });
    console.log('PASS: the border colour outlines the choices, in the preview and the running Preview');

    // The layout: clear screen, border, width and padding
    for (const label of ['Clear screen between each page', 'Show border', 'Set a custom display width', 'Set custom padding']) {
        if (!(await page.getByLabel(label, { exact: false }).count())) throw new Error(`the tab has no "${label}" option`);
    }
    console.log('PASS: the layout options are on the tab');

    if (screenshotDir) await page.screenshot({ path: join(screenshotDir, 'gamebook-appearance.png'), fullPage: true });

    if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
    if (previewErrors.length) throw new Error(`Preview errors: ${previewErrors.join('; ')}`);
    console.log('\nAll gamebook Appearance tab checks passed.');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
