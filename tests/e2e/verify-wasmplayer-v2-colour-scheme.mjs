// Checks player UI v2's light or dark choice (scheme.ts) by playing fixtures/v2-theme-test.aslx
// under ?ui=v2, with the game's colours put in its <!--PANESTHEME--> marker:
//
// - A game with the editor's default colours follows the device's scheme, and offers the toolbar's
//   light or dark menu: choosing Dark or Light overrides the device, is kept for the next game
//   (localStorage), and "Match my device" goes back to following it.
// - A game with light and dark colours of its own (a darkbackground, HasDarkColours) shows
//   each scheme's, and offers the menu too.
// - A game whose colours are for light only (Retro, or light and dark colours with a margin
//   colour, which has no dark version) is always light, whatever the choice, and doesn't offer it.
// - The menu works from the keyboard: arrows open it on its current choice and move through it,
//   Escape closes it back to its button.
// - axe-core finds no WCAG A/AA violations with the menu open, on the dark scheme.
// - The host's Save/Load dialog and debugger (chrome.css) follow the game's scheme, the player's
//   choice included, and stay light over a legacy game, which has no dark scheme. axe-core finds
//   no violations in them, dark.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { chromium } from 'playwright';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');
const baseUrl = process.argv[2] || 'http://localhost:5175';
const fixture = readFileSync(new URL('./fixtures/v2-theme-test.aslx', import.meta.url), 'utf8');

const dual = '<defaultbackground>#faf3e3</defaultbackground><defaultforeground>#3b2f22</defaultforeground>'
    + '<defaultlinkforeground>#8a3b12</defaultlinkforeground><darkbackground>#2a2119</darkbackground>'
    + '<darkforeground>#efe4cf</darkforeground><darklinkforeground>#f0b080</darklinkforeground>';

// A computed colour as rgb(r, g, b), whether it came back as that or as color(srgb r g b) (a
// colour-mix result)
function rgb(colour) {
    const srgb = /^color\(srgb ([\d.]+) ([\d.]+) ([\d.]+)/.exec(colour);
    if (!srgb) return colour;
    return `rgb(${srgb.slice(1, 4).map((c) => Math.round(Number(c) * 255)).join(', ')})`;
}

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(context, { theme = '', extra = '' } = {}) {
        const page = await context.newPage();
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        await page.route((url) => url.pathname === '/e2e-fixtures/v2-theme-test.aslx', (route) => route.fulfill({
            contentType: 'text/xml',
            body: fixture
                .replace('<!--THEME-->', theme ? `<inherit name="${theme}"/>` : '')
                .replace('<!--PANESTHEME-->', extra),
        }));
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-theme-test.aslx&ui=v2`);
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('A hall'), null, { timeout: 30000 })
            .catch(() => { throw new Error(`${theme || extra || 'default'}: the game never started on v2`); });
        return { page, errors };
    }

    const state = (page) => page.evaluate(() => {
        const player = document.querySelector('qv-player');
        const wrapper = player.querySelector('.qv-scheme');
        return {
            scheme: player.getAttribute('data-qv-scheme'),
            offered: !!wrapper && !wrapper.hidden && wrapper.getBoundingClientRect().width > 0,
            bg: getComputedStyle(player.querySelector('[data-qv-part="border"]')).backgroundColor,
            // The scheme's own page colour, unmixed
            pageBg: getComputedStyle(player).getPropertyValue('--qv-bg').trim(),
            text: getComputedStyle(player).color,
            icon: [...player.querySelectorAll('[data-qv-scheme-icon]')].find((i) => !i.hidden)?.dataset.qvSchemeIcon,
        };
    }).then((s) => ({ ...s, bg: rgb(s.bg) }));

    // The host's dialogs' scheme, with axe-core run over the open dialog
    async function dialogs(page, what) {
        const result = {};
        if (!await page.evaluate(() => !!window.axe)) await page.addScriptTag({ path: axePath });
        for (const [name, open, selector] of [
            ['saves', () => page.click('#cmdSave'), '#qv-saves'],
            ['debugger', async () => {
                await page.evaluate(() => WebPlayer.setCanDebug(true));
                await page.click('#cmdDebug');
            }, '#questVivaDebugger'],
        ]) {
            await open();
            await page.waitForSelector(`${selector}[open]`, { timeout: 5000 });
            result[name] = await page.evaluate((sel) => getComputedStyle(document.querySelector(sel)).colorScheme, selector);
            const violations = await page.evaluate(async (sel) => (await window.axe.run(sel, {
                runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
            })).violations.flatMap((v) => v.nodes.map((n) => `${v.id}: ${n.target.join(' ')}`)), selector);
            if (violations.length) throw new Error(`${what}: WCAG violations in ${name}: ${violations.slice(0, 5).join('; ')}`);
            await page.evaluate((sel) => document.querySelector(sel).close(), selector);
        }
        return result;
    }

    async function choose(page, label) {
        await page.locator('.qv-scheme-button').click();
        await page.getByRole('menuitemradio', { name: label }).click();
    }

    // ── A game with the editor's colours, on a light device ──
    const light = await browser.newContext({ viewport: { width: 1200, height: 700 }, colorScheme: 'light' });
    {
        const { page, errors } = await open(light);
        let s = await state(page);
        check(s.scheme === 'light' && s.offered && s.icon === 'system', `default colours on a light device: light, the menu offered on "system" (${JSON.stringify(s)})`);

        await choose(page, 'Dark');
        s = await state(page);
        check(s.scheme === 'dark' && s.bg === 'rgb(24, 24, 27)' && s.icon === 'dark', `choosing Dark shows v2's dark colours (${s.bg}, icon ${s.icon})`);
        const checked = await page.locator('[role="menuitemradio"][aria-checked="true"]').getAttribute('data-qv-scheme-choice');
        check(checked === 'dark', 'the menu marks Dark as the choice');
        const stored = await page.evaluate(() => localStorage.getItem('questviva-colour-scheme'));
        check(stored === 'dark', 'the choice is kept in localStorage');
        check(await page.locator('.qv-scheme-menu').isHidden() && await page.evaluate(() => document.activeElement?.classList.contains('qv-scheme-button')),
            'choosing closes the menu, back on its button');

        await page.addScriptTag({ path: axePath });
        await page.locator('.qv-scheme-button').click();
        const violations = await page.evaluate(async () => (await window.axe.run(document, {
            runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
        })).violations.flatMap((v) => v.nodes.map((n) => `${v.id}: ${n.target.join(' ')}`)));
        check(violations.length === 0, `no WCAG violations with the menu open, dark (${violations.slice(0, 5).join('; ')})`);
        await page.keyboard.press('Escape');
        const dark = await dialogs(page, 'chosen Dark');
        check(dark.saves === 'dark' && dark.debugger === 'dark', `the Save/Load dialog and debugger are dark too, on a light device (${JSON.stringify(dark)})`);
        check(errors.length === 0, `no page errors (${errors.join('; ')})`);
        await page.close();
    }
    {
        // The choice is the player's, for the next game too
        const { page } = await open(light);
        let s = await state(page);
        check(s.scheme === 'dark', 'the next game opens dark, from the kept choice');

        // From the keyboard
        await page.locator('.qv-scheme-button').focus();
        await page.keyboard.press('ArrowDown');
        let focused = await page.evaluate(() => document.activeElement?.getAttribute('data-qv-scheme-choice'));
        check(focused === 'dark', 'ArrowDown opens the menu on the current choice');
        await page.keyboard.press('ArrowDown');
        focused = await page.evaluate(() => document.activeElement?.getAttribute('data-qv-scheme-choice'));
        check(focused === 'system', 'ArrowDown moves on, round to the first');
        await page.keyboard.press('Escape');
        check(await page.locator('.qv-scheme-menu').isHidden()
            && await page.evaluate(() => document.activeElement?.classList.contains('qv-scheme-button')), 'Escape closes the menu, back on its button');

        await choose(page, 'Match my device');
        s = await state(page);
        check(s.scheme === 'light' && s.icon === 'system', 'Match my device follows the light device again');
        check(await page.evaluate(() => localStorage.getItem('questviva-colour-scheme')) === null, 'and forgets the choice');
        await page.close();
    }
    await light.close();

    // ── On a dark device ──
    const dark = await browser.newContext({ viewport: { width: 1200, height: 700 }, colorScheme: 'dark' });
    {
        const { page } = await open(dark);
        let s = await state(page);
        check(s.scheme === 'dark', 'default colours on a dark device: dark');
        await choose(page, 'Light');
        s = await state(page);
        check(s.scheme === 'light' && s.bg === 'rgb(255, 255, 255)', `choosing Light overrides the dark device (${s.bg})`);
        const light = await dialogs(page, 'chosen Light');
        check(light.saves === 'light' && light.debugger === 'light', `and the dialogs are light (${JSON.stringify(light)})`);
        await choose(page, 'Match my device');
        await page.close();
    }

    // ── A game with light and dark colours of its own ──
    {
        const { page } = await open(dark, { extra: dual });
        let s = await state(page);
        check(s.scheme === 'dark' && s.offered && s.bg === 'rgb(42, 33, 25)' && s.text === 'rgb(239, 228, 207)',
            `light and dark colours on a dark device: the game's dark ones, the menu offered (${JSON.stringify(s)})`);
        const link = await page.locator('qv-output a').first().evaluate((a) => getComputedStyle(a).color);
        check(link === 'rgb(240, 176, 128)', `its dark link colour (${link})`);
        await choose(page, 'Light');
        s = await state(page);
        check(s.scheme === 'light' && s.bg === 'rgb(250, 243, 227)' && s.text === 'rgb(59, 47, 34)', `choosing Light shows its light ones (${JSON.stringify(s)})`);
        await choose(page, 'Match my device');
        await page.close();
    }

    // ── Colours for light only ──
    for (const [what, options] of [
        ['Retro', { theme: 'theme_retro' }],
        ['light and dark colours with a margin colour', { extra: dual + '<marginscolour>Tan</marginscolour>' }],
    ]) {
        const { page } = await open(dark, options);
        await page.evaluate(() => localStorage.setItem('questviva-colour-scheme', 'dark'));
        const s = await state(page);
        check(s.scheme === 'light' && !s.offered, `${what}: always light, the menu not offered (${JSON.stringify(s)})`);
        await page.evaluate(() => localStorage.removeItem('questviva-colour-scheme'));
        await page.close();
    }
    {
        // A legacy game is light, so its dialogs are too
        const page = await dark.newPage();
        await page.goto(`${baseUrl}/?url=/examples/simple.aslx`);
        await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
        await page.click('#cmdSave');
        await page.waitForSelector('#qv-saves[open]', { timeout: 5000 });
        const saves = await page.evaluate(() => getComputedStyle(document.getElementById('qv-saves')).colorScheme);
        check(saves === 'light', `over a legacy game on a dark device, the Save/Load dialog is light (${saves})`);
        await page.close();
    }
    await dark.close();
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
