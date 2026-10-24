// Checks player UI v2's status pane and pinned status bar (#2613, PR 3) by playing
// fixtures/v2-status-test.aslx (610) under ?ui=v2. The fixture pins score and health
// (game.pinnedstatusattributes) and gives health and mana maximums
// (game.statusattributemaximums), which 610 Core sends as "pinned" and "max":
//
// - Wide: the status pane lists every attribute's text, with a bar (role="meter") for each that
//   has a maximum. A bar follows its value from turn to turn. The pinned bar isn't shown.
// - Narrow: the pinned status bar under the top bar shows just the pinned attributes, and goes
//   when the game hides the status pane.
// - A game with no status attributes has no status pane.
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

    async function open(width, fixture = 'v2-status-test.aslx', startText = 'A room.') {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}&ui=v2`);
        await page.waitForFunction(
            (text) => !document.getElementById('qv-start') && window.canSendCommand
                && document.querySelector('qv-output')?.textContent.includes(text),
            startText, { timeout: 30000 })
            .catch(() => { throw new Error(`${fixture}: the game never started on v2`); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    async function send(page, command, expect) {
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await page.waitForFunction((t) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(t),
            expect, { timeout: 10000 })
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
    const describe = (page, selector) => page.evaluate((sel) => {
        const root = document.querySelector(sel);
        return {
            shown: !!root && root.offsetHeight > 0,
            items: root ? [...root.querySelectorAll('.qv-status-item')].map((item) => {
                const meter = item.querySelector('[role="meter"]');
                return {
                    text: item.querySelector('.qv-status-text').textContent,
                    now: meter?.getAttribute('aria-valuenow') ?? null,
                    max: meter?.getAttribute('aria-valuemax') ?? null,
                    labelled: !!meter && document.getElementById(meter.getAttribute('aria-labelledby'))?.textContent === item.querySelector('.qv-status-text').textContent,
                    fill: meter ? meter.querySelector('.qv-status-meter-fill').style.width : null,
                };
            }) : [],
        };
    }, selector);

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        let pane = await describe(page, 'qv-pane[data-qv-pane="status"]');
        check(pane.shown && pane.items.map((i) => i.text).join(' / ') === 'Score: 0 / Health: 70% / Mana: 3 / Gold: 5',
            `the status pane lists every attribute (${pane.items.map((i) => i.text).join(' / ')})`);
        const [score, health, mana, gold] = pane.items;
        check(health.now === '70' && health.max === '100' && health.fill === '70%' && health.labelled,
            'an attribute with a numeric maximum gets a bar, labelled by its text');
        check(mana.now === '3' && mana.max === '8', "a maximum can come from another attribute (mana's from maxmana)");
        check(score.now === null && gold.now === null, 'attributes without a maximum are text only');
        check(!(await describe(page, '[data-qv-part="status-bar"]')).shown, "the pinned status bar isn't shown on a wide layout");
        await audit(page, 'with the status pane');

        await send(page, 'hurt', 'Ouch.');
        await page.waitForFunction(() => document.querySelector('qv-pane[data-qv-pane="status"] [data-qv-status="health"] [role="meter"]')
            ?.getAttribute('aria-valuenow') === '50', null, { timeout: 5000 })
            .catch(() => { throw new Error("the health bar didn't follow the value"); });
        console.log('PASS: a bar follows its value from turn to turn');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(390);
        const bar = await describe(page, '[data-qv-part="status-bar"]');
        check(bar.shown && bar.items.map((i) => i.text).join(' / ') === 'Score: 0 / Health: 70%',
            `on a narrow layout the pinned status bar shows just the pinned attributes (${bar.items.map((i) => i.text).join(' / ')})`);
        check(bar.items[1].now === '70', "a pinned attribute with a maximum keeps its bar");
        check(await page.evaluate(() => !!document.querySelector('[data-qv-part="status-bar"]').closest('.qv-topbar')),
            'the pinned bar is part of the sticky top bar');
        await audit(page, 'with the pinned status bar');

        await send(page, 'hidestatus', 'Status hidden.');
        check(!(await describe(page, '[data-qv-part="status-bar"]')).shown, 'hiding the status pane hides the pinned bar too');

        if (errors.length) throw new Error(`narrow: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── No status attributes ────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200, 'v2-core-test.aslx', 'A plain hall');
        check(!(await describe(page, 'qv-pane[data-qv-pane="status"]')).shown, 'a game with no status attributes has no status pane');
        if (errors.length) throw new Error(`v2-core-test.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
