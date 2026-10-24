// Checks player UI v2's colour tokens and themes (#2613, PR 6) by playing
// fixtures/v2-theme-test.aslx (610) under ?ui=v2. The script serves the fixture with the game
// inheriting each theme_* type, or with each pane theme, in place of its markers:
//
// - A game that keeps the editor's default colours gets no colour tokens from 610 Core (decision
//   5), so it follows the light or dark scheme, command bar included.
// - A game with colours of its own (theme_retro, theme_hotdogstand) keeps them in either scheme.
// - The themes and pane themes apply as tokens.
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

    async function open({ theme = '', panesTheme = '', scheme = 'light', width = 1200 } = {}) {
        const page = await browser.newPage({ viewport: { width, height: 700 }, colorScheme: scheme });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.route('**/e2e-fixtures/v2-theme-test.aslx', (route) => route.fulfill({
            contentType: 'text/xml',
            body: fixture
                .replace('<!--THEME-->', theme ? `<inherit name="${theme}"/>` : '')
                .replace('<!--PANESTHEME-->', panesTheme ? `<panestheme>${panesTheme}</panestheme>` : ''),
        }));
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-theme-test.aslx&ui=v2`);
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

    const paneThemes = { Midnight: 'midnightblue', Nature: '#254117', Parchment: '#FAEBD7', Vanilla: 'white', Black: 'black', Blood: '#800000', Tranquil: 'skyblue' };
    for (const [panesTheme, paneBg] of Object.entries(paneThemes)) {
        const { page, errors } = await open({ panesTheme });
        check((await colours(page)).inlinePaneBg === paneBg, `the ${panesTheme} pane theme sets the pane tokens`);
        await audit(page, `with the ${panesTheme} pane theme`);
        await done(page, errors, panesTheme);
    }
    {
        const { page, errors } = await open({ panesTheme: 'Blood', scheme: 'dark' });
        await audit(page, 'with the Blood pane theme, dark');
        await done(page, errors, 'Blood dark');
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
