// Checks the transcript (#2627, PR 5) by playing fixtures/v2-transcript-test.aslx (610) on player
// UI v2 and on legacy. The transcript store now belongs to the host (wasm-player.js), and both
// UIs write to it:
//
// - "transcript on" offers the game's name in the command bar (QuestUI.transcript.promptForName),
//   then records under the name entered, in localStorage's "questtranscript-<name>" as before,
//   with the date and time stamped in the output.
// - What's printed while it records goes in: room descriptions, a picture as its alt text,
//   line breaks as @@@NEW_LINE@@@ (as legacy has always stored them), and a text effect's text
//   once. "transcript off" stops it.
// - "view transcript" links to the transcript viewer, which lists the transcript.
// - The toolbar's Your Transcripts button (#2882) appears once there's a transcript to view, stays
//   after "transcript off" and on the next visit, and opens the viewer. A game with
//   "notranscript", from the start or once it's turned on, doesn't show it.
// - Legacy records the same game to the same store.
// - axe-core finds no WCAG A/AA violations on v2 at 1200px and 390px.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { createRequire } from 'node:module';
import { chromium } from 'playwright';

test('v2-transcript', async ({ baseURL }) => {
    const require = createRequire(import.meta.url);
    const axePath = require.resolve('axe-core/axe.min.js');
    const baseUrl = baseURL;

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    const ui = {
        v2: { input: 'qv-command-bar input', output: 'qv-output' },
        legacy: { input: '#txtCommand', output: '#divOutput' },
    };

    let browser;
    try {
        browser = await chromium.launch();

        async function open(context, width, name, fixture = 'v2-transcript-test', expect = 'A recording room.') {
            const page = await context.newPage();
            await page.setViewportSize({ width, height: 700 });
            const errors = [];
            page.on('pageerror', (err) => errors.push(String(err)));
            page.on('console', (msg) => {
                if (msg.type() === 'error') errors.push(msg.text());
            });
            await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}.aslx&ui=${name}`);
            await page.waitForFunction(([sel, text]) => !document.getElementById('qv-start') && window.canSendCommand
                && document.querySelector(sel)?.textContent.includes(text), [ui[name].output, expect], { timeout: 30000 })
                .catch(() => { throw new Error(`the game never started on ${name}`); });
            await page.addScriptTag({ path: axePath });
            return { page, errors, sel: ui[name] };
        }

        async function send(page, sel, command, expect) {
            await page.waitForFunction(() => window.canSendCommand, null, { timeout: 15000 });
            await page.fill(sel.input, command);
            await page.press(sel.input, 'Enter');
            await page.waitForFunction(([s, t]) => window.canSendCommand && document.querySelector(s).textContent.includes(t),
                [sel.output, expect], { timeout: 15000 })
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
        const buttonShown = (page) => page.evaluate(() => {
            const button = document.getElementById('cmdTranscripts');
            return !!button && getComputedStyle(button).display !== 'none';
        });
        const stored = (page, name) => page.evaluate((n) => localStorage.getItem(`questtranscript-${n}`) ?? '', name);

        // "transcript on" with a name, then the same recording on either UI.
        async function record(page, sel, label, name) {
            await send(page, sel, 'transcript on', 'name');
            const offered = await page.inputValue(sel.input);
            check(offered === 'V2TranscriptTest', `[${label}] "transcript on" offers the game's name in the command bar ("${offered}")`);
            await page.fill(sel.input, name);
            await page.press(sel.input, 'Enter');
            await page.waitForFunction((s) => window.canSendCommand && document.querySelector(s).textContent.includes('DATE AND TIME'),
                sel.output, { timeout: 15000 }).catch(() => { throw new Error(`[${label}] the transcript never started`); });
            let text = await stored(page, name);
            check(text.includes('DATE AND TIME'), `[${label}] it records under the name entered, with the date and time`);

            await send(page, sel, 'look', 'A recording room.');
            await send(page, sel, 'picture', 'Shown here.');
            text = await stored(page, name);
            check(text.includes('A recording room.'), `[${label}] what's printed goes in`);
            check(text.includes('a brass lamp Shown here.') && !text.includes('<img'), `[${label}] a picture goes in as its alt text`);
            check(text.includes('@@@NEW_LINE@@@'), `[${label}] line breaks are stored as legacy always stored them`);
            return text;
        }

        // ── v2 ──────────────────────────────────────────────────────────────────

        {
            const context = await browser.newContext();
            const { page, errors, sel } = await open(context, 1200, 'v2');
            check(!await buttonShown(page), 'the Your Transcripts button is hidden before there are any transcripts');
            await record(page, sel, 'v2', 'My run');
            check(await buttonShown(page), 'the Your Transcripts button appears once a transcript is recording');
            const label = await page.locator('#cmdTranscripts .qv-button-label').textContent();
            check(label === 'Your Transcripts', `the button is named by Core's link text ("${label}")`);

            await send(page, sel, 'typewriter', 'Typed for the record.');
            let text = await stored(page, 'My run');
            check(text.split('Typed for the record.').length === 2, "a text effect's text goes in once");

            await send(page, sel, 'view transcript', 'Transcript');
            const link = await page.evaluate(() => {
                const a = [...document.querySelectorAll('qv-output a')].at(-1);
                return { href: a?.getAttribute('href'), target: a?.getAttribute('target'), text: a?.textContent };
            });
            check(link.href === 'TranscriptViewer/index.html' && link.target === '_blank' && link.text.length > 0,
                `"view transcript" links to the transcript viewer ("${link.text}")`);
            await audit(page, 'while recording a transcript');

            await send(page, sel, 'transcript off', 'off');
            const before = await stored(page, 'My run');
            await send(page, sel, 'secret', 'Not for the record.');
            check(await stored(page, 'My run') === before, '"transcript off" stops recording');
            check(await buttonShown(page), 'the button stays after "transcript off"');

            const [viewer] = await Promise.all([context.waitForEvent('page'), page.click('#cmdTranscripts')]);
            await viewer.waitForLoadState();
            check(new URL(viewer.url()).pathname.endsWith('/TranscriptViewer/index.html'),
                `the button opens the transcript viewer (${viewer.url()})`);
            await viewer.waitForFunction(() => document.body.textContent.includes('My run'), null, { timeout: 10000 })
                .catch(() => { throw new Error("the transcript viewer doesn't list the transcript"); });
            check(true, 'the transcript viewer lists it');
            await viewer.close();

            await send(page, sel, 'no transcripts', 'Transcripts are off.');
            check(!await buttonShown(page), 'the button goes when the game turns transcripts off');

            if (errors.length) throw new Error(`v2: page errors:\n  ${errors.join('\n  ')}`);

            // The next visit, with the transcript kept.
            const again = await open(context, 1200, 'v2');
            check(await buttonShown(again.page), 'the button is there on the next visit, with a transcript kept');
            const none = await open(context, 1200, 'v2', 'v2-notranscript-test', 'A quiet room.');
            check(!await buttonShown(none.page), 'a game with "notranscript" never shows the button');
            for (const opened of [again, none]) {
                if (opened.errors.length) throw new Error(`v2 next visit: page errors:\n  ${opened.errors.join('\n  ')}`);
            }
            await context.close();
        }

        // ── v2, narrow ──────────────────────────────────────────────────────────

        {
            const context = await browser.newContext();
            const { page, errors, sel } = await open(context, 390, 'v2');
            await record(page, sel, 'v2 narrow', 'Phone run');
            await audit(page, 'while recording a transcript, narrow');
            if (errors.length) throw new Error(`v2 narrow: page errors:\n  ${errors.join('\n  ')}`);
            await context.close();
        }

        // ── Legacy ──────────────────────────────────────────────────────────────

        {
            const context = await browser.newContext();
            const { page, errors, sel } = await open(context, 1200, 'legacy');
            await record(page, sel, 'legacy', 'Legacy run');
            if (errors.length) throw new Error(`legacy: page errors:\n  ${errors.join('\n  ')}`);
            await context.close();
        }

        console.log('\nAll checks passed.');
    } catch (err) {
        console.error(`FAIL: ${err.message}`);
        throw err;
    } finally {
        await browser?.close();
    }
});
