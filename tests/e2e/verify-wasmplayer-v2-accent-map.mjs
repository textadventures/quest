// Checks player UI v2's accent colour, interface font and map position by playing
// fixtures/v2-accent-map-test.aslx (610) under ?ui=v2, with fixtures/v2-compass-test.aslx for a
// game left on the defaults:
//
// - A game on the defaults draws its links and open compass directions in v2's own accent, and
//   its location bar on the game's background rather than a dark bar.
// - A game's accentcolour is drawn as given on the light scheme, and lightened on the dark one,
//   unless the game has sent its own background too. Its interfacefont is the panes' font.
// - mapposition "panes" puts the map in the first pane, drawn and with no collapse button at
//   either width; a layout with no panes puts it back above the game, as does setMapPosition
//   ("header").
// - The top bar's buttons are icons, named by a hidden label that is also their tooltip.
// - axe-core finds no WCAG A/AA violations with the map in the panes, light or dark.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { createRequire } from 'node:module';
import { chromium } from 'playwright';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');
const baseUrl = process.argv[2] || 'http://localhost:5175';

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(fixture, expect, { width = 1280, colorScheme = 'light' } = {}) {
        const page = await browser.newPage({ viewport: { width, height: 800 }, colorScheme });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}.aslx&ui=v2`);
        await page.waitForFunction((t) => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes(t), expect, { timeout: 30000 })
            .catch(() => { throw new Error(`${fixture} never started on v2`); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
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

    // Computed colours, as "r,g,b" whatever syntax the browser serialises a color-mix() in.
    const look = (page) => page.evaluate(() => {
        const canvas = document.createElement('canvas');
        canvas.width = canvas.height = 1;
        const ctx = canvas.getContext('2d', { willReadFrequently: true });
        const rgb = (colour) => {
            ctx.clearRect(0, 0, 1, 1);
            ctx.fillStyle = colour;
            ctx.fillRect(0, 0, 1, 1);
            return [...ctx.getImageData(0, 0, 1, 1).data.slice(0, 3)].join(',');
        };
        const css = (selector, prop) => {
            const el = document.querySelector(selector);
            return el ? getComputedStyle(el)[prop] : null;
        };
        const region = document.querySelector('[data-qv-part="map"]');
        const svg = region?.querySelector('svg')?.getBoundingClientRect();
        return {
            link: rgb(css('[data-qv-part="output"] a.cmdlink', 'color')),
            exit: rgb(css('.qv-compass-button:not(:disabled)', 'color')),
            topbar: rgb(css('.qv-topbar', 'backgroundColor')),
            frame: rgb(css('[data-qv-part="border"]', 'backgroundColor')),
            panesFont: css('[data-qv-part="panes"]', 'fontFamily'),
            mapInPane: !!region?.closest('qv-pane[data-qv-pane="map"]'),
            mapInHeader: !!region?.closest('.qv-header'),
            mapPaneFirst: document.querySelector('[data-qv-part="panes"] qv-pane')?.dataset.qvPane === 'map',
            mapPaneShown: !!document.querySelector('qv-pane[data-qv-pane="map"]:not([hidden])'),
            mapDrawn: !!svg && svg.width > 50 && svg.height > 50
                && !!region.querySelector('.qv-map-player'),
            mapButtonShown: !!document.querySelector('.qv-map-button:not([hidden])'),
        };
    });

    const brightness = (rgb) => rgb.split(',').map(Number).reduce((a, b) => a + b, 0);

    // ── A game left on the defaults ──
    {
        const { page, errors } = await open('v2-compass-test', 'A hall.');
        const a = await look(page);
        check(a.link === '31,111,120', `links are v2's own accent, #1f6f78 (${a.link})`);
        check(a.exit === a.link, `an open compass direction is drawn in the accent too (${a.exit})`);
        check(a.topbar === a.frame, `the location bar is on the game's background (${a.topbar}, ${a.frame})`);
        await page.hover('#cmdSave');
        const save = await page.evaluate(() => {
            const button = document.getElementById('cmdSave');
            const icon = button.querySelector('.qv-icon').getBoundingClientRect();
            const box = button.getBoundingClientRect();
            return {
                name: button.textContent.trim(),
                title: button.title,
                labelWidth: button.querySelector('.qv-button-label').getBoundingClientRect().width,
                centred: Math.abs((icon.top + icon.bottom) / 2 - (box.top + box.bottom) / 2) < 1,
            };
        });
        check(save.name === 'Save / Load' && save.title === 'Save / Load' && save.labelWidth <= 1,
            `Save / Load is an icon, named by a hidden label and tooltip (${JSON.stringify(save)})`);
        check(save.centred, 'the icon is centred in its button');
        check(errors.length === 0, `no page errors on the defaults (${errors.join('; ')})`);
        await page.close();
    }

    // ── A game's own accent and interface font, with its map in the panes ──
    {
        const { page, errors } = await open('v2-accent-map-test', 'A hall.');
        let a = await look(page);
        check(a.link === '122,62,110', `links are the game's accent, #7a3e6e, on the light scheme (${a.link})`);
        check(a.exit === a.link, `so is an open compass direction (${a.exit})`);
        check(a.panesFont.startsWith('Verdana'), `the panes are in the game's interface font (${a.panesFont})`);
        check(a.mapInPane && a.mapPaneFirst && a.mapPaneShown, 'the map is the first pane');
        check(a.mapDrawn, 'the map is drawn in its pane, with the player on it');
        await audit(page, 'with the map in the panes (light)');

        await page.evaluate(() => window.QuestUI.setLayoutPreset('nopanes'));
        a = await look(page);
        check(a.mapInHeader && !a.mapPaneShown, 'with no panes, the map goes back above the game');
        check(a.mapDrawn, 'and is still drawn there');

        await page.evaluate(() => window.QuestUI.setLayoutPreset('sidepanes'));
        a = await look(page);
        check(a.mapInPane && a.mapPaneShown, 'with the panes back, so is the map\'s pane');

        await page.evaluate(() => window.QuestUI.setMapPosition('header'));
        a = await look(page);
        check(a.mapInHeader && !a.mapPaneShown && a.mapDrawn, 'setMapPosition("header") puts the map above the game');
        check(errors.length === 0, `no page errors (${errors.join('; ')})`);
        await page.close();
    }

    // ── Narrow: the map is in the drawer, with no collapse button ──
    {
        const { page, errors } = await open('v2-accent-map-test', 'A hall.', { width: 600 });
        let a = await look(page);
        check(a.mapInPane && !a.mapButtonShown, 'on a narrow layout the map is in the drawer, with no collapse button');
        await page.click('.qv-topbar .qv-panes-button');
        await page.waitForFunction(() => document.querySelector('[data-qv-part="panes"]').classList.contains('qv-drawer-open'));
        await page.waitForTimeout(300);
        a = await look(page);
        check(a.mapDrawn, 'the map is drawn once the drawer opens');
        check(errors.length === 0, `no page errors when narrow (${errors.join('; ')})`);
        await page.close();
    }

    // ── Dark: the accent is lightened, unless the game has its own background ──
    {
        const { page, errors } = await open('v2-accent-map-test', 'A hall.', { colorScheme: 'dark' });
        let a = await look(page);
        check(a.link !== '122,62,110' && brightness(a.link) > brightness('122,62,110') + 150,
            `on the dark scheme the accent is lightened (${a.link})`);
        await audit(page, 'with the map in the panes (dark)');

        await page.evaluate(() => window.QuestUI.setTokens({ '--qv-bg': '#202040', '--qv-text': '#ffffff' }));
        a = await look(page);
        check(a.link === '122,62,110', `with the game's own background, its accent is as given (${a.link})`);
        check(errors.length === 0, `no page errors on the dark scheme (${errors.join('; ')})`);
        await page.close();
    }
} catch (e) {
    console.error(`FAIL: ${e.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
