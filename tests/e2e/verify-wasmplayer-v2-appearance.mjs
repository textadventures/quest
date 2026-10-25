// Checks player UI v2's command bar presets, backgrounds, web fonts and output sections (#2627,
// PR 6) with three 610 fixtures under ?ui=v2:
//
// - v2-borderless-test.aslx: the "borderless" preset draws the input with no box, after the
//   game's prompt, with an underline in the focus colour while it has focus. With the editor's
//   default colours its text follows the light or dark scheme (610 Core sends no
//   --qv-command-fg then).
// - v2-shadow-test.aslx: the "shadow" preset swaps the border for a glow mixed from the text
//   colour, so it differs between light and dark.
// - v2-appearance-test.aslx, loaded as if from textadventures.co.uk (?id=, with the API mocked) so
//   its background image comes from a resourceRoot: the image at half opacity over a colour
//   blend; the Special Elite web font, its Google Fonts stylesheet added; SetWebFontName adding
//   another; an output section hidden while the text after it stays; and all of it still there
//   after loading a save.
// - axe-core finds no WCAG A/AA violations, at 1200px and 390px.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { createRequire } from 'node:module';
import { chromium } from 'playwright';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');
const baseUrl = process.argv[2] || 'http://localhost:5175';
const resourceRoot = 'https://images.example.test/res/';
const input = 'qv-command-bar input';
// A 1×1 PNG, for the background image.
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==', 'base64');

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(fixture, { width = 1200, colorScheme = 'light', expect = 'A plain room.', viaId = false } = {}) {
        const page = await browser.newPage({ viewport: { width, height: 700 }, colorScheme });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.route('https://fonts.googleapis.com/**', (route) => route.fulfill({ contentType: 'text/css', body: '/* web font */' }));
        if (viaId) {
            await page.route('**/api/game/**', (route) => route.fulfill({
                contentType: 'application/json',
                body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/${fixture}.aslx`, resourceRoot }),
            }));
            await page.route(`${resourceRoot}**`, (route) => route.fulfill({ contentType: 'image/png', body: png }));
            await page.goto(`${baseUrl}/?id=${fixture}&ui=v2`);
        } else {
            await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}.aslx&ui=v2`);
        }
        await page.waitForFunction((t) => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes(t), expect, { timeout: 30000 })
            .catch(() => { throw new Error(`${fixture} never started on v2`); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    async function send(page, command, expect) {
        await page.waitForFunction(() => window.canSendCommand, null, { timeout: 15000 });
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await page.waitForFunction((t) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(t),
            expect, { timeout: 15000 })
            .catch(() => { throw new Error(`"${command}" never printed "${expect}"`); });
    }
    async function audit(page, state) {
        const violations = await page.evaluate(async () => {
            const result = await window.axe.run(document, {
                runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
            });
            return result.violations.map((v) => `${v.id}: ${v.help} (${v.nodes.map((n) => n.target.join(' ')).slice(0, 3).join(', ')})`);
        });
        if (violations.length) throw new Error(`axe found WCAG violations ${state}:\n    ${violations.join('\n    ')}`);
        console.log(`PASS: no WCAG A/AA violations ${state}`);
    }
    const commandBar = (page) => page.evaluate(() => {
        const bar = document.querySelector('qv-command-bar');
        const field = bar.querySelector('input');
        const style = getComputedStyle(field);
        return {
            preset: bar.dataset.qvPreset ?? null,
            prompt: bar.querySelector('.qv-command-prompt')?.textContent ?? '',
            borderTop: style.borderTopWidth, borderBottom: `${style.borderBottomWidth} ${style.borderBottomColor}`,
            background: style.backgroundColor, colour: style.color, shadow: style.boxShadow, outline: style.outlineStyle,
            focused: document.activeElement === field,
            ring: getComputedStyle(document.querySelector('qv-player')).getPropertyValue('--qv-focus-ring').trim(),
        };
    });
    const sideways = (page) => page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth);
    const rgb = (hex) => `rgb(${parseInt(hex.slice(1, 3), 16)}, ${parseInt(hex.slice(3, 5), 16)}, ${parseInt(hex.slice(5, 7), 16)})`;

    // ── Borderless, light and dark ──────────────────────────────────────────

    for (const colorScheme of ['light', 'dark']) {
        const { page, errors } = await open('v2-borderless-test', { colorScheme });
        await page.focus(input);
        const c = await commandBar(page);
        check(c.preset === 'borderless' && c.prompt === '>', `[${colorScheme}] the borderless preset, after the game's prompt ("${c.prompt}")`);
        check(c.borderTop === '0px' && c.background === 'rgba(0, 0, 0, 0)', `[${colorScheme}] it has no box (border ${c.borderTop}, background ${c.background})`);
        check(c.focused && c.outline === 'none' && c.borderBottom === `2px ${rgb(c.ring)}`,
            `[${colorScheme}] with focus it's underlined in the focus colour (${c.borderBottom})`);
        const expected = colorScheme === 'light' ? 'rgb(26, 26, 26)' : 'rgb(228, 228, 231)';
        check(c.colour === expected, `[${colorScheme}] its text follows the ${colorScheme} scheme (${c.colour})`);
        await audit(page, `with the borderless command bar, ${colorScheme}`);
        if (errors.length) throw new Error(`borderless ${colorScheme}: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    {
        const { page, errors } = await open('v2-borderless-test', { width: 390 });
        check(!(await sideways(page)), 'the borderless command bar fits a phone');
        await audit(page, 'with the borderless command bar, narrow');
        if (errors.length) throw new Error(`borderless narrow: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Shadow, light and dark ──────────────────────────────────────────────

    const shadows = {};
    for (const colorScheme of ['light', 'dark']) {
        const { page, errors } = await open('v2-shadow-test', { colorScheme });
        const c = await commandBar(page);
        shadows[colorScheme] = c.shadow;
        check(c.preset === 'shadow' && c.shadow !== 'none' && c.shadow.includes('15px 5px'),
            `[${colorScheme}] the shadow preset glows (${c.shadow})`);
        await audit(page, `with the shadow command bar, ${colorScheme}`);
        if (errors.length) throw new Error(`shadow ${colorScheme}: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }
    check(shadows.light !== shadows.dark, "the shadow's colour follows the scheme");

    // ── Backgrounds, web fonts and output sections ──────────────────────────

    const appearance = (page) => page.evaluate(() => {
        const root = document.querySelector('qv-player');
        const frame = document.querySelector('[data-qv-part="border"]');
        const lines = [...document.querySelectorAll('qv-output span[style]')];
        return {
            background: getComputedStyle(root).backgroundImage,
            frame: getComputedStyle(frame).backgroundColor,
            font: getComputedStyle(document.querySelector('qv-output')).fontFamily,
            stylesheets: [...document.querySelectorAll('link[rel="stylesheet"]')].map((l) => l.href).filter((h) => h.includes('fonts.googleapis.com')),
            lastStyledFont: lines.at(-1)?.style.fontFamily ?? '',
        };
    });
    const halfOpacity = (colour) => /,\s*0\.5\)$|\/\s*0\.5\)$/.test(colour);

    async function checkAppearance(page, label) {
        const a = await appearance(page);
        check(a.background.includes(`url("${resourceRoot}paper.png")`), `[${label}] the background image shows`);
        check(a.background.includes('linear-gradient(rgb(30, 58, 138), rgb(190, 24, 93))'), `[${label}] over the colour blend`);
        check(halfOpacity(a.frame), `[${label}] the game's background is at half opacity (${a.frame})`);
        check(a.stylesheets.some((h) => h.includes('family=Special+Elite')), `[${label}] the web font's stylesheet is added`);
        check(/^["']Special Elite["']/.test(a.font), `[${label}] and the output is in it (${a.font})`);
        return a;
    }

    {
        const { page, errors } = await open('v2-appearance-test', { viaId: true, expect: 'A decorated room.' });
        await checkAppearance(page, 'start');
        await audit(page, 'with a background image, colour blend and web font');

        await send(page, 'webfont', 'In a new font.');
        const a = await appearance(page);
        check(a.stylesheets.some((h) => h.includes('family=Press+Start+2P')), "SetWebFontName adds the new font's stylesheet");
        check(a.lastStyledFont.includes('Press Start 2P'), `and the text after it is in that font (${a.lastStyledFont})`);

        await send(page, 'section', 'After the section.');
        await send(page, 'hidesection', 'Section hidden.');
        await page.waitForFunction(() => !document.querySelector('qv-output').textContent.includes('Inside the section.'), null, { timeout: 5000 })
            .catch(() => { throw new Error("HideOutputSection didn't remove the section's text"); });
        check(await page.evaluate(() => document.querySelector('qv-output').textContent.includes('After the section.')),
            "HideOutputSection removes the section's text and keeps what came after it");

        await page.close();
        if (errors.length) throw new Error(`appearance: page errors:\n  ${errors.join('\n  ')}`);
    }

    {
        const { page, errors } = await open('v2-appearance-test', { viaId: true, expect: 'A decorated room.' });
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
            .catch(() => { throw new Error("Save / Load didn't open the host's dialog"); });
        await page.click('#qv-saves-save-new');
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 10000 });
        await page.evaluate(() => document.getElementById('qv-saves')?.close());
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
            .catch(() => { throw new Error('the save never appeared in the Save/Load list'); });
        await page.click('#qv-saves-list [data-slot]');
        await page.waitForFunction(() => window.canSendCommand && !document.querySelector('qv-output').textContent.includes('Game saved.'),
            null, { timeout: 30000 }).catch(() => { throw new Error("loading didn't restore the saved game"); });
        // The restored game's InitInterface sets its tokens once the UI is up, the text tokens last.
        await page.waitForFunction(() => document.querySelector('qv-player')?.style.getPropertyValue('--qv-font-body'), null, { timeout: 10000 })
            .catch(() => { throw new Error("the restored game's interface options never arrived"); });
        await checkAppearance(page, 'after loading a save');
        if (errors.length) throw new Error(`restore: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    {
        const { page, errors } = await open('v2-appearance-test', { viaId: true, expect: 'A decorated room.', width: 390 });
        await checkAppearance(page, 'narrow');
        check(!(await sideways(page)), 'a phone gets no sideways scroll');
        await audit(page, 'with a background image, colour blend and web font, narrow');
        if (errors.length) throw new Error(`appearance narrow: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
