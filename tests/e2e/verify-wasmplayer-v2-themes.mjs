// Checks player UI v2's colour tokens and themes (#2613, PR 6) by playing
// fixtures/v2-theme-test.aslx (610) under ?ui=v2. The script serves the fixture with the game
// inheriting each theme_* type, or with a pane theme, in place of its markers:
//
// - A game that keeps the editor's default colours gets no colour tokens from 610 Core (decision
//   5), so it follows the light or dark scheme, command bar included.
// - A game with colours of its own for one scheme (theme_retro, theme_midnight, theme_terminal)
//   keeps them in either scheme, and one with light and dark colours (theme_parchment, theme_noir,
//   theme_storybook) shows each scheme's (#2656).
// - The themes apply as tokens. The retired theme_hotdogstand still loads.
// - Pane themes are the classic interface's (#2719): v2 ignores one, and a game with
//   classicinterface still gets it on legacy.
// - A game's border colour colours v2's border line, and its location bar colours apply without
//   turning off the classic location style, which v2 doesn't have (#2719).
// - A new game's verb menus use the interface font, and so do those of a game that stores Quest
//   5's defaults (Arial, 9pt, black on white), which also still follows the dark scheme. A margin
//   colour gives the game its own colours, and the classic interface keeps Arial at 9pt (#2725).
// - axe-core finds no WCAG A/AA violations in any of them, light and dark.
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

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open({ theme = '', panesTheme = '', classic = false, extra = '', scheme = 'light', width = 1200 } = {}) {
        const page = await browser.newPage({ viewport: { width, height: 700 }, colorScheme: scheme });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        // Matched on the path, since a classic game's page URL also ends in the fixture's name.
        await page.route((url) => url.pathname === '/e2e-fixtures/v2-theme-test.aslx', (route) => route.fulfill({
            contentType: 'text/xml',
            body: fixture
                .replace('<!--THEME-->', theme ? `<inherit name="${theme}"/>` : '')
                .replace('<!--PANESTHEME-->', (panesTheme ? `<panestheme>${panesTheme}</panestheme>` : '')
                    + (classic ? '<classicinterface type="boolean">true</classicinterface>' : '') + extra),
        }));
        // A classic game is left to choose its own UI, which is legacy.
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-theme-test.aslx${classic ? '' : '&ui=v2'}`);
        if (classic) {
            await page.waitForFunction(() => !document.getElementById('qv-start') && document.body.innerText.includes('A hall'), null, { timeout: 30000 })
                .catch(() => { throw new Error('classic: the game never started'); });
            return { page, errors };
        }
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('A hall'), null, { timeout: 30000 })
            .catch(() => { throw new Error(`${theme || panesTheme || 'default'}: the game never started on v2`); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    async function audit(page, state) {
        const violations = await page.evaluate(async () => {
            const result = await window.axe.run(document, {
                runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
            });
            return result.violations.flatMap((v) => v.nodes
                .map((n) => `${v.id}: ${n.target.join(' ')} ${n.any[0]?.message ?? ''}`));
        });
        if (violations.length) throw new Error(`axe found WCAG violations ${state}:\n    ${violations.slice(0, 8).join('\n    ')}`);
        console.log(`PASS: no WCAG A/AA violations ${state}`);
    }

    const colours = (page) => page.evaluate(() => {
        const root = document.querySelector('qv-player');
        const output = document.querySelector('[data-qv-part="output"] .qv-block span');
        const input = document.querySelector('qv-command-bar input');
        return {
            inlineText: root.style.getPropertyValue('--qv-text'),
            inlineBg: root.style.getPropertyValue('--qv-bg'),
            inlinePaneBg: root.style.getPropertyValue('--qv-pane-bg'),
            text: getComputedStyle(output).color,
            bg: getComputedStyle(root).getPropertyValue('--qv-bg').trim(),
            inputColour: input.style.color,
            inputBackground: input.style.background,
        };
    });
    async function done(page, errors, what) {
        if (errors.length) throw new Error(`${what}: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── The editor's default colours follow the scheme ──────────────────────

    for (const scheme of ['light', 'dark']) {
        const { page, errors } = await open({ scheme });
        const c = await colours(page);
        check(c.inlineText === '' && c.inlineBg === '' && c.inputColour === '' && c.inputBackground === '',
            `${scheme}: a game with the default colours gets no colour tokens, and no colours on its command bar`);
        const expected = scheme === 'light'
            ? { text: 'rgb(26, 26, 26)', bg: '#ffffff' }
            : { text: 'rgb(228, 228, 231)', bg: '#18181b' };
        check(c.text === expected.text && c.bg === expected.bg,
            `${scheme}: its text and background are v2's ${scheme} defaults (${c.text} on ${c.bg})`);
        await audit(page, `with the default colours, ${scheme}`);
        await done(page, errors, `default ${scheme}`);
    }

    {
        const { page, errors } = await open({ scheme: 'dark', width: 390 });
        await page.click('.qv-topbar .qv-panes-button');
        await page.waitForTimeout(350);
        await audit(page, 'with the default colours, dark, narrow with the drawer open');
        await done(page, errors, 'default dark narrow');
    }

    // ── Themes ──────────────────────────────────────────────────────────────

    for (const scheme of ['light', 'dark']) {
        const { page, errors } = await open({ theme: 'theme_retro', scheme });
        const c = await colours(page);
        check(c.inlineBg === 'Black' && c.inlineText === 'White' && c.bg === 'Black' && c.text === 'rgb(255, 255, 255)',
            `${scheme}: theme_retro keeps its own white on black`);
        await audit(page, `with theme_retro, ${scheme}`);
        await done(page, errors, `theme_retro ${scheme}`);
    }

    {
        // Retired: no longer offered, but a game that has it still plays
        const { page, errors } = await open({ theme: 'theme_hotdogstand' });
        const c = await colours(page);
        check(c.bg === 'Red' && c.text === 'rgb(255, 255, 0)', 'the retired theme_hotdogstand still loads, yellow on red');
        await done(page, errors, 'theme_hotdogstand');
    }

    // One scheme's colours, whatever the device's
    for (const [theme, bg, text] of [
        ['theme_midnight', '#101a30', 'rgb(230, 233, 242)'],
        ['theme_terminal', '#0b0f0b', 'rgb(79, 240, 122)'],
    ]) {
        for (const scheme of ['light', 'dark']) {
            const { page, errors } = await open({ theme, scheme });
            const c = await colours(page);
            check(c.inlineBg === bg && c.text === text, `${scheme}: ${theme} keeps its own colours (${c.text} on ${c.bg})`);
            await audit(page, `with ${theme}, ${scheme}`);
            await done(page, errors, `${theme} ${scheme}`);
        }
    }

    // Light and dark colours, following the device
    for (const [theme, light, dark] of [
        ['theme_parchment', ['#faf3e3', 'rgb(59, 47, 34)'], ['#2a2119', 'rgb(239, 228, 207)']],
        ['theme_noir', ['#f2f1ee', 'rgb(18, 18, 18)'], ['#121212', 'rgb(233, 231, 227)']],
        ['theme_storybook', ['#fff6ee', 'rgb(67, 48, 90)'], ['#2d2338', 'rgb(248, 234, 244)']],
    ]) {
        for (const [scheme, [bg, text]] of [['light', light], ['dark', dark]]) {
            const { page, errors } = await open({ theme, scheme });
            const c = await colours(page);
            check(c.inlineBg === '' && c.bg === bg && c.text === text, `${scheme}: ${theme} shows its ${scheme} colours (${c.text} on ${c.bg})`);
            await audit(page, `with ${theme}, ${scheme}`);
            await done(page, errors, `${theme} ${scheme}`);
        }
    }

    for (const theme of ['theme_novella', 'theme_typewriter']) {
        const { page, errors } = await open({ theme });
        const applied = await page.evaluate(() => ({
            width: document.querySelector('qv-player').style.getPropertyValue('--qv-output-width'),
            panes: !document.querySelector('[data-qv-part="panes"]').hidden,
        }));
        // A theme is only the look: the panes are the Interface tab's, and stay on
        check(applied.width === '650px' && applied.panes, `${theme} applies as tokens (650px wide), leaving the panes on`);
        await audit(page, `with ${theme}`);
        await done(page, errors, theme);
    }

    // ── Pane themes ─────────────────────────────────────────────────────────

    {
        const { page, errors } = await open({ panesTheme: 'Midnight' });
        check((await colours(page)).inlinePaneBg === '', 'v2 ignores a pane theme: the panes follow the game\'s colours');
        await done(page, errors, 'Midnight on v2');
    }
    {
        const { page, errors } = await open({ panesTheme: 'Midnight', classic: true });
        const applied = await page.evaluate(() => ({
            v2: !!document.querySelector('qv-player'),
            paneBg: document.documentElement.style.getPropertyValue('--qv-pane-bg'),
        }));
        check(!applied.v2 && applied.paneBg === 'midnightblue', 'with classicinterface, the Midnight pane theme colours the classic panes');
        await done(page, errors, 'Midnight on legacy');
    }

    // ── Border and location bar colours ─────────────────────────────────────

    {
        const borderColour = (page) => page.evaluate(() =>
            getComputedStyle(document.querySelector('[data-qv-part="border"]')).borderLeftColor);
        const plain = await open();
        const hairline = await borderColour(plain.page);
        await done(plain.page, plain.errors, 'default border');

        const { page, errors } = await open({
            extra: '<bordercolour>red</bordercolour><customlocationcolour>navy</customlocationcolour>'
                + '<customlocationtextcolour>white</customlocationtextcolour>',
        });
        check(hairline !== 'rgb(255, 0, 0)' && await borderColour(page) === 'rgb(255, 0, 0)',
            `a game's border colour colours the border line (${hairline} by default)`);
        const bar = await page.evaluate(() => {
            const style = getComputedStyle(document.querySelector('.qv-topbar'));
            return { bg: style.backgroundColor, fg: style.color };
        });
        check(bar.bg === 'rgb(0, 0, 128)' && bar.fg === 'rgb(255, 255, 255)',
            `the location bar takes the game's colours with the classic location style still on (${bar.fg} on ${bar.bg})`);
        await done(page, errors, 'border and location bar colours');
    }

    // ── The player's own defaults (#2725) ───────────────────────────────────

    const menuFont = (page) => page.evaluate(() => {
        const menu = getComputedStyle(document.querySelector('qv-verb-menu'));
        return {
            font: menu.fontFamily,
            size: menu.fontSize,
            uiFont: getComputedStyle(document.querySelector('.qv-topbar')).fontFamily,
        };
    });

    {
        const { page, errors } = await open();
        const m = await menuFont(page);
        check(m.font === m.uiFont && !/arial/i.test(m.font) && m.size === '16px',
            `a new game's verb menus use the interface font at 1rem (${m.font}, ${m.size})`);
        await done(page, errors, 'default menu font');
    }

    {
        // As games saved by Quest 5 or an earlier Viva store them
        const quest5Defaults = '<defaultforeground>Black</defaultforeground><defaultlinkforeground>Blue</defaultlinkforeground>'
            + '<defaultbackground>White</defaultbackground><menufont>Arial</menufont><menufontsize type="int">9</menufontsize>';
        const { page, errors } = await open({ extra: quest5Defaults, scheme: 'dark' });
        const c = await colours(page);
        check(c.inlineText === '' && c.inlineBg === '' && c.bg === '#18181b',
            `dark: a game that stores Quest 5's default colours still follows the dark scheme (${c.text} on ${c.bg})`);
        const m = await menuFont(page);
        check(m.font === m.uiFont && m.size === '16px',
            `and Quest 5's Arial at 9pt counts as unset, so its verb menus use the interface font (${m.font}, ${m.size})`);
        await done(page, errors, 'Quest 5 defaults');
    }

    {
        const { page, errors } = await open({ extra: '<menufont>Courier New</menufont><menufontsize type="int">12</menufontsize>' });
        const m = await menuFont(page);
        check(/courier new/i.test(m.font) && m.size === '16px',
            `a verb menu font of the game's own is used (${m.font}, 12pt = ${m.size})`);
        await done(page, errors, 'own menu font');
    }

    {
        const { page, errors } = await open({ extra: '<marginscolour>LightGrey</marginscolour>', scheme: 'dark' });
        const c = await colours(page);
        check(c.inlineBg === 'White' && c.inlineText === 'Black' && c.text === 'rgb(0, 0, 0)',
            `dark: a margin colour gives the game its own colours, black on white (${c.text} on ${c.bg})`);
        await audit(page, 'with only a margin colour, dark');
        await done(page, errors, 'margins only');
    }

    {
        const { page, errors } = await open({ classic: true });
        const font = await page.evaluate(() => {
            const probe = document.createElement('div');
            probe.className = 'jjmenu';
            document.body.append(probe);
            const style = getComputedStyle(probe);
            const result = { font: style.fontFamily, size: style.fontSize };
            probe.remove();
            return result;
        });
        check(/arial/i.test(font.font) && font.size === '12px',
            `with classicinterface, the verb menus keep Arial at 9pt (${font.font}, ${font.size})`);
        await done(page, errors, 'classic menu font');
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
