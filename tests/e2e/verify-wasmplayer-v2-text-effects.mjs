// Checks player UI v2's text effects (#2627, PR 4) by playing fixtures/v2-text-effects-test.aslx
// (610) under ?ui=v2:
//
// - TextFX_Typewriter types its text out a letter at a time, with markup in it dropped as legacy
//   did; TextFX_Unscramble shows scrambled letters that turn into the text, keeping its spaces.
// - The full text is in the output from the start for screen readers (visually hidden), and the
//   animated copy is hidden from them.
// - With the game's starting style, the effect has no inline style of its own (610 Core passes
//   it as ""); after SetForegroundColour it's drawn in that colour.
// - A key press or click finishes a running effect at once; under reduced motion it's shown
//   finished straight away; a save taken while one runs restores with it finished.
// - axe-core finds no WCAG A/AA violations at either width, and a phone gets no sideways scroll.
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

    async function open(width, { reducedMotion } = {}) {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        if (reducedMotion) await page.emulateMedia({ reducedMotion: 'reduce' });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-text-effects-test.aslx&ui=v2`);
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('An effects room.'), null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started on v2'); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    // Sends a command and waits for its turn to finish (an effect's text is in the output at once).
    async function send(page, command, expect) {
        await page.waitForFunction(() => window.canSendCommand, null, { timeout: 15000 });
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await page.waitForFunction((t) => window.canSendCommand
            && [...document.querySelectorAll('.qv-text-effect-full')].some((e) => e.textContent.includes(t)),
        expect, { timeout: 15000 })
            .catch(() => { throw new Error(`"${command}" never added an effect with "${expect}"`); });
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

    const effect = (page) => page.evaluate(() => {
        const all = document.querySelectorAll('qv-output .qv-text-effect');
        const e = all[all.length - 1];
        const full = e.querySelector('.qv-text-effect-full');
        const shown = e.querySelector('.qv-text-effect-shown');
        const box = full.getBoundingClientRect();
        return {
            kind: e.dataset.qvEffect, full: full.textContent, shown: shown.textContent,
            shownHidden: shown.getAttribute('aria-hidden') === 'true', fullHidden: full.closest('[aria-hidden="true"]') !== null,
            fullVisible: box.width > 1 || box.height > 1, style: e.getAttribute('style') ?? '', colour: getComputedStyle(shown).color,
            bold: !!e.querySelector('b'), running: e.classList.contains('qv-text-effect-running'),
        };
    });
    const finished = (page) => page.waitForFunction(() => {
        const all = document.querySelectorAll('qv-output .qv-text-effect');
        const e = all[all.length - 1];
        return e.querySelector('.qv-text-effect-shown').textContent === e.querySelector('.qv-text-effect-full').textContent;
    }, null, { timeout: 10000 }).catch(() => { throw new Error('the effect never finished'); });

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        await send(page, 'typewriter', 'typed one letter');
        let e = await effect(page);
        const text = 'Hello world, typed one letter at a time.';
        check(e.kind === 'typewriter' && e.full === text && !e.bold, `the full text is there at once, with its markup dropped ("${e.full}")`);
        check(e.shownHidden && !e.fullHidden && !e.fullVisible, 'the full text is for screen readers only, and the animated copy is hidden from them');
        check(e.running && e.shown.length < text.length && text.startsWith(e.shown.replace(/_$/, '')),
            `it types out from the start ("${e.shown}")`);
        check(e.style === '', `with the game's starting style it has no inline style (${e.style || 'none'})`);
        await audit(page, 'with a typewriter running');
        await finished(page);
        check(!(await effect(page)).running, 'the typewriter finishes with the whole text');

        await send(page, 'unscramble', 'A secret message');
        e = await effect(page);
        const secret = 'A secret message, unscrambled.';
        check(e.kind === 'unscramble' && e.shown.length === secret.length && e.shown !== secret
            && [...secret].every((c, i) => (c === ' ') === (e.shown[i] === ' ')),
            `unscramble starts scrambled, keeping its spaces ("${e.shown}")`);
        await finished(page);
        check(true, 'and turns into the text');

        await send(page, 'slow', 'very long time');
        check((await effect(page)).running, 'a slow typewriter is running');
        await page.keyboard.press('Shift');
        e = await effect(page);
        check(!e.running && e.shown === e.full, 'a key press finishes it at once');
        await send(page, 'slow', 'very long time');
        await page.mouse.click(600, 300);
        e = await effect(page);
        check(!e.running && e.shown === e.full, 'and so does a click');

        await send(page, 'red', 'Red text.');
        e = await effect(page);
        check(e.colour === 'rgb(255, 0, 0)', `after SetForegroundColour the effect is in that colour (${e.colour})`);

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Restore ─────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        await send(page, 'slow', 'very long time');
        // Saving opens a dialog with a click, which would finish the effect, so save from script.
        await page.evaluate(() => GameSaver.save());
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 10000 })
            .catch(() => { throw new Error('the game never saved'); });
        const saved = await page.evaluate(() => {
            const shown = [...document.querySelectorAll('.qv-text-effect-shown')].at(-1).textContent;
            return shown;
        });
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
            .catch(() => { throw new Error('the save never appeared in the Save/Load list'); });
        await page.click('#qv-saves-list [data-slot]');
        await page.waitForFunction(() => window.canSendCommand && document.querySelector('.qv-text-effect')
            && !document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 30000 })
            .catch(() => { throw new Error("loading didn't restore the saved game"); });
        const e = await effect(page);
        check(!e.running && e.shown === e.full, `a save taken mid-effect restores it finished (saved at "${saved}")`);
        if (errors.length) throw new Error(`restore: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow, with reduced motion ─────────────────────────────────────────

    {
        const { page, errors } = await open(390, { reducedMotion: true });
        await send(page, 'slow', 'very long time');
        const e = await effect(page);
        check(!e.running && e.shown === e.full, 'under reduced motion the text is shown at once');
        check(!(await page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth)),
            'a phone gets no sideways scroll');
        await audit(page, 'with an effect, narrow');
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
