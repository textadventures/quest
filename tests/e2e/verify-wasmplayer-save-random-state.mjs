// A saved game stores where its random numbers have got to (#2673), so restoring it carries on
// with the same numbers the game would have drawn next, instead of a new sequence. Without that, a
// bug a player hits after loading a save can't be reproduced, even knowing the game's seed.
//
// Plays without ?seed=, so the game has a random seed of its own: draws some numbers, saves, draws
// the next few, restores the save through the player's own restart path, and expects the same
// numbers again.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
const pageErrors = [];
page.on('pageerror', err => pageErrors.push(err.message));

const roll = () => page.evaluate(async () => {
    const values = [];
    for (let i = 0; i < 3; i++) values.push(String(await window.QuestVivaTest.evaluate('GetRandomInt(1, 1000000)')));
    return values;
});

try {
    await page.goto(`${baseUrl}/?url=/examples/simple.aslx`);
    await page.waitForSelector('#txtCommand, qv-command-bar input', { timeout: 30000, state: 'attached' });
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });

    await roll();
    await page.evaluate(async () => {
        await Bridge.PrepareSaveGame('');
        window.__savedBytes = Bridge.GetSaveGameBytes();
    });
    const expected = await roll();

    await page.evaluate(async () => { await restartGameCore(window.__savedBytes); });
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
    const actual = await roll();

    if (JSON.stringify(actual) !== JSON.stringify(expected)) {
        throw new Error(`Restored save drew ${actual.join(', ')}, expected ${expected.join(', ')}`);
    }
    if (pageErrors.length) throw new Error(`Page errors: ${JSON.stringify(pageErrors)}`);
    console.log(`PASS: a restored save carries on with the same random numbers (${actual.join(', ')})`);
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
