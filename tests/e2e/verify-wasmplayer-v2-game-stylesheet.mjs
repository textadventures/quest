// Checks a game's own stylesheet (game.stylesheet, #2613 PR 7, decision 6) by playing
// fixtures/v2-game-stylesheet-test.aslx (610). The host injects it as one unlayered <style> on
// every Initialise:
//
// - On v2 its rules win over v2's own without !important, though they're no more specific: a
//   token (the link colour), a property v2 sets (the location's font-weight) and the link's
//   underline style.
// - Loading a save brings it back, still only once.
// - On legacy (?ui=legacy) it applies too, after legacy's own stylesheet.
// - axe-core finds no WCAG A/AA violations with it applied.
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

    async function open(ui) {
        const page = await browser.newPage({ viewport: { width: 1200, height: 700 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-game-stylesheet-test.aslx&ui=${ui}`);
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.body.textContent.includes('A hall, with'), null, { timeout: 30000 })
            .catch(() => { throw new Error(`${ui}: the game never started`); });
        return { page, errors };
    }

    const applied = (page) => page.evaluate(() => {
        const styles = document.querySelectorAll('#qv-game-stylesheet');
        const link = document.querySelector('a.cmdlink.elementmenu');
        const location = document.querySelector('[data-qv-part="location"], #location');
        return {
            count: styles.length,
            last: styles[0] === [...document.head.querySelectorAll('style, link[rel="stylesheet"]')].at(-1),
            layered: /@layer/.test(styles[0]?.textContent ?? ''),
            linkColour: link ? getComputedStyle(link).color : null,
            linkDecoration: link ? getComputedStyle(link).textDecorationStyle : null,
            linkToken: getComputedStyle(document.querySelector('qv-player') ?? document.body).getPropertyValue('--qv-link').trim(),
            locationWeight: getComputedStyle(location).fontWeight,
        };
    });

    // ── v2 ──────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2');
        let a = await applied(page);
        check(a.count === 1 && a.last && !a.layered, 'the game stylesheet is one unlayered <style>, after the player UI\'s styles');
        check(a.linkColour === 'rgb(160, 82, 45)', `it sets a token (--qv-link, ${a.linkColour})`);
        check(a.locationWeight === '400' && a.linkDecoration === 'dotted',
            `its rules win over v2's own, no more specific, without !important (weight ${a.locationWeight}, ${a.linkDecoration})`);
        await page.addScriptTag({ path: axePath });
        const violations = await page.evaluate(async () => (await window.axe.run(document, {
            runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
        })).violations.map((v) => `${v.id}: ${v.nodes.map((n) => n.target.join(' ')).slice(0, 3).join(', ')}`));
        if (violations.length) throw new Error(`axe found WCAG violations with the game stylesheet:\n    ${violations.join('\n    ')}`);
        console.log('PASS: no WCAG A/AA violations with the game stylesheet');

        // Save, then load it back.
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 });
        await page.click('#qv-saves-save-new');
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 10000 });
        await page.evaluate(() => document.getElementById('qv-saves')?.close());
        await page.fill('qv-command-bar input', 'fine');
        await page.press('qv-command-bar input', 'Enter');
        await page.waitForFunction(() => window.canSendCommand && document.querySelector('qv-output').textContent.includes('All fine.'),
            null, { timeout: 10000 });
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 });
        await page.click('#qv-saves-list [data-slot]');
        await page.waitForFunction(() => window.canSendCommand && document.querySelector('qv-output')
            && !document.querySelector('qv-output').textContent.includes('All fine.'), null, { timeout: 30000 })
            .catch(() => { throw new Error("loading didn't restore the saved game"); });
        a = await applied(page);
        check(a.count === 1 && a.linkToken === 'rgb(160, 82, 45)' && a.locationWeight === '400',
            'loading a save brings the game stylesheet back, still only once');

        if (errors.length) throw new Error(`v2: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Legacy ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('legacy');
        const a = await applied(page);
        check(a.count === 1 && a.last && a.linkDecoration === 'dotted' && a.locationWeight === '400',
            'on legacy the game stylesheet applies too, after legacy\'s own');
        if (errors.length) throw new Error(`legacy: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
