// Checks player UI v2's interface strings in another language (#2627, PR 8) by playing
// fixtures/v2-i18n-test.aslx (610, Deutsch.aslx) under ?ui=v2:
//
// - The page's lang is the game's LanguageId ("de"), so screen readers read it as German.
// - None of v2's own labels is left in English: the Save / Load button, the command bar, the
//   panes button, the exits strip, the map and its zoom buttons, a YouTube embed's title, and
//   the "Game saved." message.
// - Restarting the game, which swaps in a fresh copy of the UI, keeps all of that in German.
// - axe-core finds no WCAG A/AA violations at either width.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { createRequire } from 'node:module';
import { chromium } from 'playwright';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');
const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';

// The English defaults (ChromeStrings in PlayerCore, and Core's own CompassLabel) of every label
// v2 shows, none of which should be left on a German game.
const english = ['Save / Load', 'Type your command', 'Show panels', 'Map', 'Zoom in', 'Zoom out', 'YouTube video',
    'Compass', 'Game saved.'];

const german = {
    save: 'Speichern / Laden',
    command: 'Gib deinen Befehl ein',
    panes: 'Bereiche anzeigen',
    compass: 'Kompass',
    map: 'Karte',
    saved: 'Spiel gespeichert.',
};

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(width) {
        const page = await browser.newPage({ viewport: { width, height: 800 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.route(/youtube-nocookie\.com|youtube\.com/, (route) =>
            route.fulfill({ status: 200, contentType: 'text/html', body: '<!doctype html><title>video</title>' }));
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-i18n-test.aslx&ui=v2`);
        await started(page);
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    async function started(page) {
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('Eine Halle.')
            && document.querySelector('qv-player svg'), null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started on v2 with its map drawn'); });
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

    // Every label in v2's UI: text, aria-label, title and placeholder, of controls and labelled parts.
    const labels = (page) => page.evaluate(() => {
        const found = [];
        for (const el of document.querySelectorAll('qv-player button, qv-player input, qv-player [aria-label], qv-player [title], qv-player iframe')) {
            for (const value of [el.getAttribute('aria-label'), el.getAttribute('title'), el.getAttribute('placeholder'),
                el.matches('button') ? el.textContent.trim() : null]) {
                if (value) found.push({ value, el: el.outerHTML.slice(0, 120) });
            }
        }
        found.push(...document.querySelector('qv-output').textContent.split(/(?<=\.)/).map((value) => ({ value: value.trim(), el: 'qv-output' })));
        return found;
    });

    async function checkGerman(page, when) {
        const state = await page.evaluate(() => ({
            lang: document.documentElement.lang,
            save: document.querySelector('qv-player #cmdSave')?.textContent.trim(),
            command: document.querySelector('qv-command-bar input')?.getAttribute('aria-label'),
            panes: [...document.querySelectorAll('qv-player .qv-panes-button')].map((b) => b.getAttribute('aria-label')),
            compass: document.querySelector('qv-player .qv-exits')?.getAttribute('aria-label'),
            map: document.querySelector('qv-player svg')?.getAttribute('aria-label'),
            zoom: [...document.querySelectorAll('qv-player [data-qv-part="map"] button')].map((b) => b.getAttribute('aria-label') || b.textContent.trim()),
        }));
        check(state.lang === 'de', `the page's lang is the game's language ${when} (got "${state.lang}")`);
        check(state.save === german.save, `the Save / Load button is in German ${when} (got "${state.save}")`);
        check(state.command === german.command, `the command bar's label is in German ${when} (got "${state.command}")`);
        check(state.panes.length > 0 && state.panes.every((l) => l === german.panes),
            `the panes buttons are labelled in German ${when} (got ${JSON.stringify(state.panes)})`);
        check(state.compass === german.compass, `the exits strip is labelled in German ${when} (got "${state.compass}")`);
        check(state.map?.includes(german.map), `the map is labelled in German ${when} (got "${state.map}")`);
        check(state.zoom.includes('Vergrößern') && state.zoom.includes('Verkleinern'),
            `the map's zoom buttons are in German ${when} (got ${JSON.stringify(state.zoom)})`);

        const left = (await labels(page)).filter((l) => english.includes(l.value));
        check(left.length === 0, `no label is left in English ${when}${left.length ? `: ${left.map((l) => `"${l.value}" on ${l.el}`).join(', ')}` : ''}`);
    }

    // ── Wide ────────────────────────────────────────────────────────────────

    const wide = await open(1200);
    await checkGerman(wide.page, 'at boot');

    await send(wide.page, 'video', 'Video gezeigt.');
    const videoTitle = await wide.page.evaluate(() => document.querySelector('qv-output iframe')?.title);
    check(videoTitle === 'YouTube-Video', `a YouTube embed's title is in German (got "${videoTitle}")`);

    await wide.page.click('#cmdSave');
    await wide.page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
        .catch(() => { throw new Error("Save / Load didn't open the host's dialog"); });
    await wide.page.click('#qv-saves-save-new');
    await wide.page.waitForFunction((t) => document.querySelector('qv-output').textContent.includes(t), german.saved, { timeout: 10000 })
        .catch(() => { throw new Error(`saving never printed "${german.saved}"`); });
    console.log('PASS: saving prints its message in German');
    await wide.page.evaluate(() => document.getElementById('qv-saves')?.close());
    await audit(wide.page, 'on a German game, wide');

    await wide.page.evaluate(() => window.RestartGame());
    await wide.page.waitForFunction(() => !document.querySelector('qv-output')?.textContent.includes('Video gezeigt.'), null, { timeout: 30000 })
        .catch(() => { throw new Error('the game never restarted'); });
    await started(wide.page);
    await checkGerman(wide.page, 'after a restart');

    if (wide.errors.length) throw new Error(`page errors (wide):\n  ${wide.errors.join('\n  ')}`);

    // ── Narrow ──────────────────────────────────────────────────────────────

    const narrow = await open(390);
    await checkGerman(narrow.page, 'at boot, narrow');
    const mapButton = await narrow.page.evaluate(() => {
        const button = document.querySelector('qv-player .qv-map-button');
        return button && { visible: button.offsetWidth > 0, label: button.textContent.trim() };
    });
    check(mapButton?.visible && mapButton.label === german.map, `the top bar's map button is in German (got ${JSON.stringify(mapButton)})`);
    await audit(narrow.page, 'on a German game, narrow');

    if (narrow.errors.length) throw new Error(`page errors (narrow):\n  ${narrow.errors.join('\n  ')}`);
    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
