// Checks boot-time player UI selection (#2593): the host loads only the UI bundle a game gets,
// once the engine has loaded the game, and ?ui= forces either one.
//
// - A pre-610 game and a 610 game both get the legacy UI (v2 only becomes the 610 default at
//   Phase 5 of #2584), and nothing under v2/ is requested.
// - ?ui=v2 loads only v2, with no legacy files and no jQuery, and
//   the game still plays: its text shows and a command gets a response.
// - ?ui=legacy forces legacy, and an unknown ?ui= value is ignored.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

let browser;
try {
    browser = await chromium.launch();

    async function play(query, { input, response }) {
        const page = await browser.newPage();
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        const bundles = new Set();
        page.on('request', (request) => {
            const match = new URL(request.url()).pathname.match(/^\/(legacy|v2)\//);
            if (match) bundles.add(match[1]);
        });

        await page.goto(`${baseUrl}/${query}`);
        await page.waitForFunction(
            () => !document.getElementById('qv-start') && document.body.innerText.includes('You are in'),
            null, { timeout: 30000 })
            .catch(() => { throw new Error(`${query}: the game never started`); });

        await page.fill(input, 'look');
        await page.press(input, 'Enter');
        await page.waitForFunction(
            (text) => document.body.innerText.split('You are in').length > 2 && document.body.innerText.includes(text),
            response, { timeout: 10000 })
            .catch(() => { throw new Error(`${query}: "look" got no response`); });

        const state = await page.evaluate(() => ({
            uis: Object.keys(window.QuestPlayerUis ?? {}).sort().join(','),
            jquery: typeof window.jQuery,
            questUi: typeof window.QuestUI,
        }));
        await page.close();
        if (errors.length) throw new Error(`${query}: page errors:\n  ${errors.join('\n  ')}`);
        return { bundles: [...bundles].sort().join(','), ...state };
    }

    function expect(actual, expected, what) {
        if (actual !== expected) throw new Error(`${what}: expected ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
        console.log(`PASS: ${what}`);
    }

    const legacyInput = { input: '#txtCommand', response: 'You can see' };

    const oldGame = await play('?url=/examples/simple.aslx', legacyInput);
    expect(oldGame.bundles, 'legacy', 'a pre-610 game requests only the legacy bundle');
    expect(oldGame.uis, 'legacy', 'a pre-610 game gets the legacy UI');

    // This fixture's room is empty, so "look" only repeats "You are in".
    const game610 = await play('?url=/e2e-fixtures/legacy-questui-test.aslx', { input: '#txtCommand', response: 'You are in' });
    expect(game610.bundles, 'legacy', 'a 610 game requests only the legacy bundle until v2 is the 610 default');
    expect(game610.uis, 'legacy', 'a 610 game gets the legacy UI');

    const forcedLegacy = await play('?url=/examples/simple.aslx&ui=legacy', legacyInput);
    expect(forcedLegacy.uis, 'legacy', '?ui=legacy forces the legacy UI');

    const unknown = await play('?url=/examples/simple.aslx&ui=nonsense', legacyInput);
    expect(unknown.uis, 'legacy', 'an unknown ?ui= value is ignored');

    const v2 = await play('?url=/examples/simple.aslx&ui=v2', { input: 'qv-command-bar input', response: 'You can see' });
    expect(v2.bundles, 'v2', '?ui=v2 requests only the v2 bundle');
    expect(v2.uis, 'v2', '?ui=v2 gets the v2 UI');
    expect(v2.jquery, 'undefined', '?ui=v2 loads no jQuery');
    expect(v2.questUi, 'object', 'v2 defines QuestUI');

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
