// Checks player UI v2's colour tokens and themes (#2613, PR 6) by playing
// fixtures/v2-theme-test.aslx (610) under ?ui=v2. The script serves the fixture with the game
// inheriting each theme_* type, or with a pane theme, in place of its markers:
//
// - A game that keeps the editor's default colours gets no colour tokens from 610 Core (decision
//   5), so it follows the light or dark scheme, command bar included.
// - A game with colours of its own (theme_retro, theme_hotdogstand) keeps them in either scheme.
// - The themes apply as tokens.
// - Pane themes are the classic interface's (#2719): v2 ignores one, and a game with
//   classicinterface still gets it on legacy.
// - A game's border colour colours v2's border line, and its location bar colours apply without
//   turning off the classic location style, which v2 doesn't have (#2719).
// - A new game's verb menus use the interface font, and so do those of a game that stores Quest
//   5's defaults (Arial, 9pt, black on white), which also still follows the dark scheme. A margin
//   colour gives the game its own colours, and the classic interface keeps Arial at 9pt (#2725).
// - axe-core finds no WCAG A/AA violations in any of them, light and dark. The one exception is
//   theme_hotdogstand's own yellow on red, which is below AA by design: nothing but its yellow text
//   may fail.
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

    // `allowed` is a text colour that may fall short of AA (a theme's own, on its own background
    // and the panes mixed from it).
    async function audit(page, state, allowed) {
        const violations = await page.evaluate(async (allow) => {
            const result = await window.axe.run(document, {
                runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
            });
            return result.violations.flatMap((v) => v.nodes
                .filter((n) => !(allow && v.id === 'color-contrast' && n.any[0]?.data?.fgColor === allow))
                .map((n) => `${v.id}: ${n.target.join(' ')} ${n.any[0]?.message ?? ''}`));
        }, allowed ?? null);
        if (violations.length) throw new Error(`axe found WCAG violations ${state}:\n    ${violations.slice(0, 8).join('\n    ')}`);
        console.log(`PASS: no WCAG A/AA violations ${state}${allowed ? ' (beyond the theme\'s own colours)' : ''}`);
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

    for (const scheme of ['light', 'dark']) {
        const { page, errors } = await open({ theme: 'theme_hotdogstand', scheme });
        const c = await colours(page);
        check(c.bg === 'Red' && c.text === 'rgb(255, 255, 0)', `${scheme}: theme_hotdogstand keeps its own yellow on red`);
        await audit(page, `with theme_hotdogstand, ${scheme}`, '#ffff00');
        await done(page, errors, `theme_hotdogstand ${scheme}`);
    }

    for (const theme of ['theme_novella', 'theme_typewriter']) {
        const { page, errors } = await open({ theme });
        const applied = await page.evaluate(() => ({
            width: document.querySelector('qv-player').style.getPropertyValue('--qv-output-width'),
            panes: !document.querySelector('[data-qv-part="panes"]').hidden,
        }));
        check(applied.width === '650px' && !applied.panes, `${theme} applies as tokens and options (650px wide, no panes)`);
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
