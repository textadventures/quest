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
    await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
    await page.waitForTimeout(500);
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
