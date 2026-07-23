// Ad-hoc manual verification: WasmPlayer's "Start from the beginning" used to
// duplicate DOM elements a game's custom JS injects into the chrome (e.g. a
// custom sidebar pane), because restartGame() only cleared #divOutput while
// leaving the rest of the live DOM (and the game's <javascript> resources,
// which re-run their setup on every restart) untouched. Fixed by having
// restartGame() rebuild the whole player chrome via swapInPlayerUi(), same as
// the initial boot. Requires the WasmPlayer dev server running locally:
//   node ../../src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

await page.goto(`${baseUrl}/?url=/e2e-fixtures/restart-test.aslx`);
await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
await page.waitForTimeout(500);

const countPanes = () => page.$$eval('.newwindow-test-pane', els => els.length);

const restart = async () => {
    await page.click('#cmdSave');
    await page.waitForSelector('#qv-saves', { state: 'visible', timeout: 10000 });
    await page.click('#qv-saves-start-new');
    // #txtCommand is part of the static chrome swapInPlayerUi() puts back
    // immediately, so it's visible well before Initialise()/Begin() (which
    // re-parses the whole Core library XML from scratch) actually finish —
    // not a safe readiness signal, and on a loaded CI runner that work can
    // easily outlast a flat waitForTimeout, reading the pane count before
    // the game's StartGame (which adds it) has even run. #qv-restarting is
    // an opaque, z-index:9999 full-viewport overlay shown for exactly the
    // duration of that work (wasm-player.js's restartGameCore) — wait for it
    // to appear and then disappear, which brackets the restart precisely.
    await page.waitForSelector('#qv-restarting', { state: 'visible', timeout: 5000 });
    await page.waitForSelector('#qv-restarting', { state: 'hidden', timeout: 30000 });
};

const initial = await countPanes();
console.log('panes after initial start:', initial);

await restart();
const after1 = await countPanes();
console.log('panes after 1st restart:', after1);

await restart();
const after2 = await countPanes();
console.log('panes after 2nd restart:', after2);

await browser.close();

if (initial !== 1 || after1 !== 1 || after2 !== 1) {
    console.error('FAIL: expected exactly 1 pane at every step (no duplication, no loss).');
    process.exit(1);
}
console.log('PASS');
