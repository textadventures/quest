// Checks player UI v2's accessibility (#2595) with axe-core's WCAG 2.0-2.2 A and AA rules, in
// each state v2 can be in under ?ui=v2, plus the keyboard focus it manages:
//
// - fixtures/v2-links-test.aslx (610): playing, with the verb menu open and with a {popup:} open.
// - fixtures/v2-prompts-test.aslx (610): during a wait.
// - fixtures/v2-dialogs-test.aslx (580): with a ShowMenu dialog open.
//
// It also checks the parts that make v2 usable without a mouse: the output is a live region
// inside the page's main landmark, the command input has a label, focus lands on the command
// input after a turn, on the Continue button during a wait and on the first choice in a menu, and
// the controls are 44px touch targets.
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

    async function open(fixture, startText) {
        const page = await browser.newPage();
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

    // Fails on any WCAG A/AA violation, listing each rule and the elements it found.
    async function audit(page, state) {
        const violations = await page.evaluate(async () => {
            const result = await window.axe.run(document, {
                runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
            });
            return result.violations.map((v) => `${v.id} (${v.impact}): ${v.help}\n      ${v.nodes.slice(0, 5).map((n) => n.target.join(' ')).join('\n      ')}`);
        });
        if (violations.length) throw new Error(`axe found WCAG violations ${state}:\n    ${violations.join('\n    ')}`);
        console.log(`PASS: no WCAG A/AA violations ${state}`);
    }

    async function type(page, command, expect) {
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await page.waitForFunction((t) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(t),
            expect, { timeout: 10000 })
            .catch(() => { throw new Error(`"${command}" never printed "${expect}"`); });
    }

    const focused = (page) => page.evaluate(() => {
        const el = document.activeElement;
        return el ? `${el.tagName.toLowerCase()}${el.className ? '.' + el.className.split(' ')[0] : ''}:${el.textContent?.trim() ?? ''}` : '';
    });

    // ── Playing, links, verb menu and popup ─────────────────────────────────

    {
        const { page, errors } = await open('v2-links-test.aslx', 'A hall with a bell');
        await page.waitForFunction(() => document.querySelector('qv-output a.elementmenu')?.dataset.verbs, null, { timeout: 10000 });

        const structure = await page.evaluate(() => {
            const output = document.querySelector('qv-output');
            const inputEl = document.querySelector('qv-command-bar input');
            return {
                inMain: !!output.closest('main'),
                live: output.getAttribute('role') === 'log' && output.getAttribute('aria-live') === 'polite',
                label: inputEl.getAttribute('aria-label'),
                inputHeight: inputEl.offsetHeight,
                saveHeight: document.getElementById('cmdSave').offsetHeight,
                inputFocused: document.activeElement === inputEl,
            };
        });
        check(structure.inMain && structure.live, 'the output is a live region in the main landmark');
        check(!!structure.label, `the command input has a label ("${structure.label}")`);
        check(structure.inputHeight >= 44 && structure.saveHeight >= 44,
            `the command input and Save are 44px touch targets (${structure.inputHeight}px, ${structure.saveHeight}px)`);
        check(structure.inputFocused, 'the command input has focus when the game starts');
        await audit(page, 'while playing');

        await type(page, 'look', '> look');
        check((await focused(page)).startsWith('input'), 'focus is back on the command input after a turn');

        await page.click('qv-output a.elementmenu >> nth=-1');
        check((await focused(page)) === 'button:Look at', "opening a verb menu puts focus on its first verb");
        await audit(page, 'with the verb menu open');
        await page.keyboard.press('Escape');

        await type(page, 'note', 'There is a');
        await page.click('qv-output [data-qv-popup] >> nth=-1');
        await audit(page, 'with a popup open');
        await page.keyboard.press('Escape');

        if (errors.length) throw new Error(`v2-links-test.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── A wait ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-prompts-test.aslx', 'You are in');
        await page.fill(input, 'waitonce');
        await page.press(input, 'Enter');
        await page.waitForFunction(() => !document.querySelector('qv-command-bar .qv-continue').hidden, null, { timeout: 10000 });
        const button = await page.evaluate(() => {
            const b = document.querySelector('qv-command-bar .qv-continue');
            return { focused: document.activeElement === b, height: b.offsetHeight };
        });
        check(button.focused && button.height >= 44, 'during a wait, focus is on the Continue button, a 44px target');
        await audit(page, 'during a wait');
        if (errors.length) throw new Error(`v2-prompts-test.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── A dialog, before 600 ────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-dialogs-test.aslx', 'You are in');
        await page.fill(input, 'menu');
        await page.press(input, 'Enter');
        await page.waitForFunction(() => document.querySelector('dialog.qv-dialog')?.open, null, { timeout: 10000 });
        check((await focused(page)) === 'button:Apple', "a menu dialog puts focus on its first option");
        await audit(page, 'with a menu dialog open');
        if (errors.length) throw new Error(`v2-dialogs-test.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
