// Checks the Light/Dark colour table on the game's Appearance tab: colour controls that name a
// dark mode counterpart (<darkattribute>) are drawn as one table, with each light colour and its
// dark one side by side, instead of a long list of separate rows. The dark text, link and accent
// colours only appear once the game has a dark page colour; each cell is named by its own
// caption and saves its own attribute; a narrow pane puts each caption above its two colours;
// and with the classic interface (no dark colours) the light colours are ordinary rows again.
// With SCREENSHOT_DIR set, it saves a picture of the table, wide and narrow.
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
    await page.fill('input[placeholder="Game name"]', `Colour Grid Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();

    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));
    const rawXml = async () => {
        await page.click('button[title="Raw XML code view"]');
        await page.waitForSelector('.cm-editor', { timeout: 5000 });
        const text = await page.locator('.cm-content').first().innerText();
        await page.click('button[title="Raw XML code view"]');
        return text;
    };
    const grid = page.locator('[data-colour-grid]');
    const cells = () => grid.locator('[data-colour-cell]').evaluateAll(els => els.map(el => el.dataset.colourCell));
    const field = (name) => grid.getByRole('combobox', { name, exact: true });

    await grid.waitFor({ timeout: 5000 }).catch(() => { throw new Error('the Appearance tab has no colour table'); });
    if (await page.locator('[data-colour-grid]').count() !== 1) throw new Error('the colours are split over more than one table');
    const headers = await grid.locator(':scope > span[aria-hidden="true"]').allInnerTexts();
    if (JSON.stringify(headers.map(h => h.trim()).filter(Boolean)) !== '["Light","Dark"]') {
        throw new Error(`the table's headings are ${JSON.stringify(headers)}`);
    }
    const before = await cells();
    if (JSON.stringify(before) !== JSON.stringify(['defaultbackground', 'darkbackground', 'defaultforeground', 'defaultlinkforeground', 'accentcolour'])) {
        throw new Error(`with no dark page colour, the table's cells are ${JSON.stringify(before)}`);
    }
    // The dark page colour isn't also drawn as a row of its own.
    if (await page.locator('label', { hasText: /^\s*Dark mode page colour:/ }).count() !== 0) {
        throw new Error('"Dark mode page colour" is still a row of its own');
    }
    if (await page.getByText('Dark mode', { exact: true }).count() !== 0) throw new Error('the old "Dark mode" section heading is still shown');
    console.log('PASS: page, text, link and accent colours share one Light/Dark table, with only the dark page colour until it is set');

    if (screenshotDir) await grid.screenshot({ path: join(screenshotDir, 'colour-grid-unset.png') });

    await field('Dark mode page colour').fill('#121212');
    await settle();
    const after = await cells();
    const expected = ['defaultbackground', 'darkbackground', 'defaultforeground', 'darkforeground',
        'defaultlinkforeground', 'darklinkforeground', 'accentcolour', 'darkaccentcolour'];
    if (JSON.stringify(after) !== JSON.stringify(expected)) throw new Error(`with a dark page colour, the table's cells are ${JSON.stringify(after)}`);
    console.log('PASS: setting a dark page colour fills in the dark text, link and accent colours beside their light ones');

    // Each cell writes its own attribute, the dark ones beside the light ones in the same row.
    await field('Dark mode text colour').fill('#e9e7e3');
    await settle();
    await field('Link colour').fill('#a31515');
    await settle();
    for (const [light, dark] of [['Page colour', 'Dark mode page colour'], ['Text colour', 'Dark mode text colour'],
        ['Link colour', 'Dark mode link colour'], ['Accent colour', 'Dark mode accent colour']]) {
        const l = await field(light).boundingBox();
        const d = await field(dark).boundingBox();
        if (!l || !d || Math.abs(l.y - d.y) > 1 || d.x <= l.x + l.width) {
            throw new Error(`${dark} isn't beside ${light}: ${JSON.stringify({ l, d })}`);
        }
    }
    const xml = await rawXml();
    for (const want of ['<darkbackground>#121212</darkbackground>', '<darkforeground>#e9e7e3</darkforeground>',
        '<defaultlinkforeground>#a31515</defaultlinkforeground>']) {
        if (!xml.includes(want)) throw new Error(`the game XML has no ${want}`);
    }
    console.log('PASS: each light colour has its dark one beside it, and each cell saves its own attribute');
    // The code view leaves the editor on its first tab.
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();
    await grid.waitFor({ timeout: 5000 });

    // Undo reaches a dark cell.
    await page.getByRole('button', { name: 'Undo', exact: true }).click();
    await page.waitForTimeout(300);
    await page.getByRole('button', { name: 'Undo', exact: true }).click();
    await page.waitForTimeout(300);
    if (await field('Dark mode text colour').inputValue() !== '') {
        throw new Error(`two Undos left the dark text colour at "${await field('Dark mode text colour').inputValue()}"`);
    }
    await page.getByRole('button', { name: 'Redo', exact: true }).click();
    await page.waitForTimeout(300);
    if (await field('Dark mode text colour').inputValue() !== '#e9e7e3') throw new Error('Redo didn\'t bring back the dark text colour');
    console.log('PASS: Undo and Redo reach the dark colours');

    if (screenshotDir) await grid.screenshot({ path: join(screenshotDir, 'colour-grid-wide.png') });

    // A narrow pane: each caption goes above its two colours, and nothing overflows.
    await page.setViewportSize({ width: 480, height: 900 });
    await page.waitForTimeout(300);
    const pageCaption = await grid.getByText('Page colour:', { exact: true }).boundingBox();
    const pageLight = await field('Page colour').boundingBox();
    const pageDark = await field('Dark mode page colour').boundingBox();
    if (!pageCaption || !pageLight || !pageDark || pageLight.y <= pageCaption.y + pageCaption.height - 1 || Math.abs(pageLight.y - pageDark.y) > 1) {
        throw new Error(`in a narrow pane the Page colour caption isn't above its two colours: ${JSON.stringify({ pageCaption, pageLight, pageDark })}`);
    }
    const overflow = await grid.evaluate(el => el.scrollWidth - el.clientWidth);
    if (overflow > 1) throw new Error(`in a narrow pane the colour table overflows by ${overflow}px`);
    console.log('PASS: in a narrow pane each caption goes above its light and dark colours, without overflowing');
    if (screenshotDir) await grid.screenshot({ path: join(screenshotDir, 'colour-grid-narrow.png') });
    await page.setViewportSize({ width: 1280, height: 900 });

    // The classic interface has no dark colours, so there's no table: the light colours are rows.
    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    const summary = page.locator('summary', { hasText: 'Advanced' });
    if (!(await summary.evaluate(el => el.closest('details')?.open))) await summary.click();
    await page.getByLabel('Use the classic interface', { exact: true }).check();
    await settle();
    await page.getByRole('button', { name: 'Appearance', exact: true }).click();
    await page.locator('label', { hasText: /^\s*Page colour:/ }).first().waitFor({ timeout: 5000 })
        .catch(() => { throw new Error('with the classic interface, Page colour is not a row of its own'); });
    if (await grid.count() !== 0) throw new Error('with the classic interface, the colour table is still shown');
    console.log('PASS: with the classic interface the light colours are ordinary rows, with no table');

    if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
