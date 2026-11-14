// Checks that player UI v2 warns, once per game, when a game prints a <script> element (#2608),
// on fixtures/v2-script-warning-test.aslx (610):
//
// - Ordinary HTML in output gives no warning.
// - A <script> in output doesn't run, and the console says so, naming JS.eval and the classic
//   interface.
// - Printing another <script> doesn't warn again.
// - Restarting the game is a new game, so it warns again there.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('v2-script-warning', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const output = 'qv-output';
    const input = 'qv-command-bar input';

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    const browser = await chromium.launch();
    try {
        const page = await browser.newPage({ viewport: { width: 1200, height: 800 } });
        const warnings = [];
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'warning' && msg.text().includes('<script>')) warnings.push(msg.text());
            if (msg.type() === 'error') errors.push(msg.text());
        });

        const ready = () => window.canSendCommand && !document.querySelector('qv-command-bar form').hidden;
        async function waitForText(text) {
            await page.waitForFunction(([sel, t]) => document.querySelector(sel)?.textContent.includes(t), [output, text], { timeout: 10000 })
                .catch(() => { throw new Error(`"${text}" never appeared`); });
        }
        async function type(command, expect) {
            await page.waitForFunction(ready, null, { timeout: 10000 });
            const before = await page.$eval(output, (el, t) => el.textContent.split(t).length, expect);
            await page.fill(input, command);
            await page.press(input, 'Enter');
            // Another copy of the expected text, since an earlier turn may have printed it already
            await page.waitForFunction(([sel, t, n]) => document.querySelector(sel).textContent.split(t).length > n,
                [output, expect, before], { timeout: 10000 })
                .catch(() => { throw new Error(`"${expect}" never appeared after "${command}"`); });
            await page.waitForFunction(ready, null, { timeout: 10000 });
        }
        async function start() {
            await page.waitForFunction((sel) => !document.getElementById('qv-start') && document.querySelector(sel)?.textContent.includes('A plain hall.'),
                output, { timeout: 30000 })
                .catch(() => { throw new Error('the game never started'); });
        }

        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-script-warning-test.aslx`);
        await start();
        check(await page.evaluate(() => !!document.querySelector('qv-player')), 'a 610 game plays on v2');

        await type('plain', 'plain text.');
        check(warnings.length === 0, 'ordinary HTML in output gives no warning');

        await type('snippet', 'After.');
        check(await page.evaluate(() => window.qvScriptRan === undefined), 'a <script> in output does not run');
        check(warnings.length === 1, `printing a <script> warns once (got ${warnings.length})`);
        check(warnings[0].includes('JS.eval') && warnings[0].includes('classic interface'),
            'the warning names JS.eval and the classic interface');

        await type('snippet', 'After.');
        check(warnings.length === 1, `a second <script> doesn't warn again (got ${warnings.length})`);

        // Start a new game from the Save / Load dialog, as v2-saving-restart-undo.spec.mjs does
        await page.click('#cmdSave');
        await page.waitForSelector('#qv-saves[open]', { timeout: 10000 });
        await page.click('#qv-saves-restart');
        // The dialog asks first when anything has happened since the last save.
        await page.locator('#qv-saves-confirm-yes, #qv-restarting:not(.hidden)').first().waitFor({ timeout: 5000 });
        if (await page.locator('#qv-saves-confirm-yes').count()) await page.click('#qv-saves-confirm-yes');
        await page.waitForSelector('#qv-restarting', { state: 'visible', timeout: 5000 });
        await page.waitForSelector('#qv-restarting', { state: 'hidden', timeout: 30000 });
        await start();
        await type('snippet', 'After.');
        check(warnings.length === 2, `after a restart, it warns again (got ${warnings.length})`);

        check(errors.length === 0, `no page errors (${errors.join('; ')})`);
        console.log('PASS');
    } catch (e) {
        console.error(`FAIL: ${e.message}`);
        throw e;
    } finally {
        await browser.close();
    }
});
