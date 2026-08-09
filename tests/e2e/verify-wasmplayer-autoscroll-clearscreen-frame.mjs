// Manual verification that ClearScreen() (JS.clearScreen(), used by gamebook
// mode's DoPage on every page transition — including game start, via
// defaultplayer's changedparent — whenever game.clearlastpage is set, e.g.
// "The Shack"'s Continue links) doesn't leave the new page's opening line
// hidden behind a tall static picture frame (#gamePanel).
//
// clearScreen() used to hard-jump to page scrollTop 0 regardless of the
// frame. That's only correct if the frame is shorter than the viewport; on
// a small/mobile screen a full-width picture very often isn't, so the new
// page's first line rendered behind it. Fixed by routing through
// scrollToTurnStart(0) — the same "reveal what's below the sticky chrome"
// logic normal turns already use — instead of a bare scrollTop(0).
//
// Requires the WasmPlayer dev server running locally:
//   node ../../src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
let failed = false;

function check(label, condition) {
    console.log(`${condition ? 'PASS' : 'FAIL'}: ${label}`);
    if (!condition) failed = true;
}

// Small/mobile-sized viewport, matching the user's report that this is
// specifically where it's noticeable. The fixture's frame is 200px tall —
// well under the real feature's own cap (div#gamePanel img's max-height is
// dynamically set to 50% of window height, see updatePanelImageMaxHeight()
// in playercore.js), so it fits under this 500px viewport with room to
// spare, same as a real picture would.
const context = await browser.newContext({ viewport: { width: 375, height: 500 } });
const previewPage = await context.newPage();
previewPage.on('pageerror', err => console.log('[pageerror]', err.message));

const senderPage = await context.newPage();
const fixtureUrl = `${baseUrl}/e2e-fixtures/scroll-clearscreen-frame-test.aslx`;
await senderPage.goto(baseUrl);

const handoffDone = senderPage.evaluate(async (url) => {
    const bytes = new Uint8Array(await (await fetch(url)).arrayBuffer());
    await new Promise((resolve) => {
        const bc = new BroadcastChannel('quest-preview');
        bc.onmessage = ({ data }) => {
            if (data.type === 'ready') {
                bc.postMessage({ type: 'game', bytes, filename: 'scroll-clearscreen-frame-test.aslx' });
                resolve();
            }
        };
    });
}, fixtureUrl);

await previewPage.goto(`${baseUrl}/?source=editor`);
await handoffDone;

await previewPage.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
await previewPage.waitForTimeout(300);

await previewPage.click('#cmdDebug');
await previewPage.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });
await previewPage.click('#qv-debugger-tabs button:text("Walkthrough")');
await previewPage.click('#qv-debugger-list [data-item="clearwithframe"]');
await previewPage.waitForSelector('[data-run-walkthrough="clearwithframe"]');
await previewPage.click('[data-run-walkthrough="clearwithframe"]');

await previewPage.waitForFunction(() => {
    const el = document.querySelector('[data-walkthrough-status]');
    return el && el.textContent && el.textContent !== 'Running…';
}, { timeout: 15000 });
const status = await previewPage.$eval('[data-walkthrough-status]', el => el.textContent);
check('Walkthrough runs to completion', status === 'Done');

await previewPage.click('#qv-debugger-close');

// clearScreen()'s own scroll is on a 100ms setTimeout; give the (possibly
// up-to-2s) resulting animation time to settle too.
await previewPage.waitForTimeout(2500);

const frameRect = await previewPage.$eval('#gamePanel', el => el.getBoundingClientRect());
const pageMarkRect = await previewPage.$eval('#page2mark', el => el.getBoundingClientRect());
console.log('  #gamePanel rect:', JSON.stringify(frameRect));
console.log('  #page2mark rect:', JSON.stringify(pageMarkRect));

check(
    'Page 2 start is visible in the viewport at all',
    pageMarkRect.top >= 0 && pageMarkRect.top < 500
);
check(
    'Page 2 start is not hidden behind the sticky picture frame after ClearScreen',
    pageMarkRect.top >= frameRect.bottom
);

await browser.close();

if (failed) {
    console.error('\nFAIL: one or more checks failed.');
    process.exit(1);
}
console.log('\nPASS');
