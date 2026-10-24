// Checks player UI v2's compass pane and exits strip (#2613, PR 2) by playing
// fixtures/v2-compass-test.aslx (610) under ?ui=v2:
//
// - Wide: the compass pane has all twelve directions, labelled with the game's direction names,
//   with only the available exits enabled. A button moves the player, and the compass follows.
//   The exits strip isn't shown, since the compass is on screen.
// - Narrow: the exits strip under the transcript repeats just the available exits, as 44px
//   buttons that move the player. It goes during a wait and comes back after it, and goes for
//   good when the game hides the compass.
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

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(width) {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-compass-test.aslx&ui=v2`);
        await page.waitForFunction(
            () => !document.getElementById('qv-start') && window.canSendCommand
                && document.querySelector('qv-output')?.textContent.includes('A hall.')
                && document.querySelector('.qv-compass-button:not(:disabled)'),
            null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started on v2 with its compass filled'); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    async function waitForTurn(page, expect, what) {
        await page.waitForFunction((t) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(t),
            expect, { timeout: 10000 })
            .catch(() => { throw new Error(`${what}: "${expect}" never appeared`); });
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
    const compass = (page) => page.evaluate(() => {
        const buttons = [...document.querySelectorAll('.qv-compass-button')];
        return {
            count: buttons.length,
            labels: buttons.map((b) => b.getAttribute('aria-label')).join(','),
            enabled: buttons.filter((b) => !b.disabled).map((b) => b.dataset.qvDirection).join(','),
        };
    });
    const strip = (page) => page.evaluate(() => {
        const nav = document.querySelector('[data-qv-part="exits"]');
        const buttons = [...nav.querySelectorAll('.qv-exit')];
        return {
            shown: nav.offsetHeight > 0,
            exits: buttons.map((b) => b.dataset.qvDirection).join(','),
            minHeight: Math.min(...buttons.map((b) => b.offsetHeight)),
            label: nav.getAttribute('aria-label'),
        };
    });

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        let c = await compass(page);
        check(c.count === 12, 'the compass pane has all twelve directions');
        check(c.labels === 'northwest,north,northeast,west,east,southwest,south,southeast,up,down,in,out',
            `its buttons are labelled with the game's direction names (${c.labels})`);
        check(c.enabled === 'north,east,up,in', `only the available exits are enabled (${c.enabled})`);
        check(!(await strip(page)).shown, "the exits strip isn't shown on a wide layout");
        await audit(page, 'with the compass pane');

        await page.click('.qv-compass-button[data-qv-direction="east"]');
        await waitForTurn(page, '> east', 'the compass');
        await page.waitForFunction(() => [...document.querySelectorAll('.qv-compass-button:not(:disabled)')].map((b) => b.dataset.qvDirection).join(',') === 'west',
            null, { timeout: 5000 }).catch(() => { throw new Error("the compass didn't follow the player"); });
        console.log('PASS: a compass button moves the player, and the compass follows');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(390);
        let s = await strip(page);
        check(s.shown && s.exits === 'north,east,up,in', `on a narrow layout the exits strip shows the available exits (${s.exits})`);
        check(s.minHeight >= 44, `its exits are 44px targets (${s.minHeight}px)`);
        check(s.label === 'Compass', `the strip is labelled by CompassLabel ("${s.label}")`);
        await audit(page, 'with the exits strip');

        await page.click('.qv-exit[data-qv-direction="north"]');
        await waitForTurn(page, 'A kitchen.', 'the exits strip');
        await page.waitForFunction(() => [...document.querySelectorAll('.qv-exit')].map((b) => b.dataset.qvDirection).join(',') === 'south',
            null, { timeout: 5000 }).catch(() => { throw new Error("the exits strip didn't follow the player"); });
        console.log('PASS: an exit in the strip moves the player, and the strip follows');

        await page.fill(input, 'waitcmd');
        await page.press(input, 'Enter');
        await page.waitForFunction(() => !document.querySelector('qv-command-bar .qv-continue').hidden, null, { timeout: 10000 });
        check(!(await strip(page)).shown, 'the strip goes during a wait');
        await page.click('qv-command-bar .qv-continue');
        await waitForTurn(page, 'Done waiting.', 'continuing');
        await page.waitForFunction(() => document.querySelector('[data-qv-part="exits"]').offsetHeight > 0, null, { timeout: 5000 })
            .catch(() => { throw new Error("the strip didn't come back after the wait"); });
        console.log('PASS: the strip comes back once the wait is over');

        await page.fill(input, 'hidecompass');
        await page.press(input, 'Enter');
        await waitForTurn(page, 'Compass hidden.', 'hiding the compass');
        check(!(await strip(page)).shown, 'hiding the compass hides the strip too');

        if (errors.length) throw new Error(`narrow: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
