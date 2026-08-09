// Manual verification that scrollToEnd()'s "don't scroll the start of new
// output off the top" target accounts for the static picture frame feature
// (#gamePanel — position:sticky, shown via SetFramePicture/JS.setPanelContents,
// e.g. used by "The Shack") in addition to the #qv-status toolbar. Before
// this fix, stickyOverlayHeight() only looked at #qv-status, so a visible
// picture frame would sit on top of (hide) the very turn-start line the
// scroll was supposed to reveal.
//
// Fixture: tests/e2e/fixtures/scroll-frame-test.aslx defines "showframe"
// (sets a 150px-tall #gamePanel via JS.setPanelContents) and shout1/shout2
// (long filler output with a <span id="turnmarkN"> at the very start),
// wired into a "withframe" walkthrough that shows the frame then runs both
// shouts.
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

const context = await browser.newContext({ viewport: { width: 1024, height: 768 } });
const previewPage = await context.newPage();
previewPage.on('pageerror', err => console.log('[pageerror]', err.message));

const senderPage = await context.newPage();
const fixtureUrl = `${baseUrl}/e2e-fixtures/scroll-frame-test.aslx`;
await senderPage.goto(baseUrl);

const handoffDone = senderPage.evaluate(async (url) => {
    const bytes = new Uint8Array(await (await fetch(url)).arrayBuffer());
    await new Promise((resolve) => {
        const bc = new BroadcastChannel('quest-preview');
        bc.onmessage = ({ data }) => {
            if (data.type === 'ready') {
                bc.postMessage({ type: 'game', bytes, filename: 'scroll-frame-test.aslx' });
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
await previewPage.click('#qv-debugger-list [data-item="withframe"]');
await previewPage.waitForSelector('[data-run-walkthrough="withframe"]');
await previewPage.click('[data-run-walkthrough="withframe"]');

await previewPage.waitForFunction(() => {
    const el = document.querySelector('[data-walkthrough-status]');
    return el && el.textContent && el.textContent !== 'Running…';
}, { timeout: 15000 });
const status = await previewPage.$eval('[data-walkthrough-status]', el => el.textContent);
check('Walkthrough runs to completion', status === 'Done');

await previewPage.click('#qv-debugger-close');

// Let scroll animations settle.
await previewPage.waitForTimeout(2500);

const frameVisible = await previewPage.$eval('#gamePanel', el => getComputedStyle(el).display !== 'none');
check('#gamePanel (picture frame) is showing', frameVisible);

const frameRect = await previewPage.$eval('#gamePanel', el => el.getBoundingClientRect());
const turn2Rect = await previewPage.$eval('#turnmark2', el => el.getBoundingClientRect());
console.log('  #gamePanel rect:', JSON.stringify(frameRect));
console.log('  #turnmark2 rect:', JSON.stringify(turn2Rect));

check(
    'Final turn start (#turnmark2) is visible in the viewport at all',
    turn2Rect.top >= 0 && turn2Rect.top < 768
);
check(
    'Final turn start is not hidden behind the sticky picture frame',
    turn2Rect.top >= frameRect.bottom
);

await browser.close();

if (failed) {
    console.error('\nFAIL: one or more checks failed.');
    process.exit(1);
}
console.log('\nPASS');
