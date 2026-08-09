// Manual verification for a stale-turnStart bug found by testing against
// the real "The Shack" (id ihxy6CUAKkarClWOh2pWag): a script that prints a
// long page, wait()s, then resumes and calls ClearScreen followed by a much
// shorter page used to leave the short page's opening line scrolled off the
// top — landing at the *old* (pre-clear) page's scroll target instead.
//
// Sequence: SignalTurnSuspended's own ordinary end-of-turn scrollToEnd()
// call still fires when the turn suspends for wait() the *second* time
// (after the resumed script's ClearScreen), but ClearScreen ran mid-script,
// so beginningOfCurrentTurnScrollPosition read by that point is whatever a
// concurrent flush left it at — in practice this reproduced as scrolling to
// the (large, now-irrelevant) old page's clamped-to-document-bottom
// position, which the "never scroll backward" guard then refused to correct
// once clearScreen()'s own delayed 100ms scrollToTurnStart(0) call (with the
// *right* answer) ran afterwards, since it was asking to scroll backward
// from where the stale call had already landed.
//
// Fixed by letting clearScreen()'s own correction move the scroll position
// backward (scrollToTurnStart(0, true)) — a screen clear makes "wherever the
// user happened to be scrolled" irrelevant, so it should always win.
//
// Fixture: tests/e2e/fixtures/scroll-clearscreen-after-wait-test.aslx's
// "gopage1" command prints ~40 lines (#page1mark at the start), wait()s,
// then on resume calls ClearScreen and prints two short lines (#page2mark).
// The debugger's walkthrough runner auto-resolves wait() calls, matching how
// The Shack's WalkthroughRunner-driven "Continue" links behave.
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

const context = await browser.newContext({ viewport: { width: 375, height: 500 } });
const previewPage = await context.newPage();
previewPage.on('pageerror', err => console.log('[pageerror]', err.message));

const senderPage = await context.newPage();
const fixtureUrl = `${baseUrl}/e2e-fixtures/scroll-clearscreen-after-wait-test.aslx`;
await senderPage.goto(baseUrl);

const handoffDone = senderPage.evaluate(async (url) => {
    const bytes = new Uint8Array(await (await fetch(url)).arrayBuffer());
    await new Promise((resolve) => {
        const bc = new BroadcastChannel('quest-preview');
        bc.onmessage = ({ data }) => {
            if (data.type === 'ready') {
                bc.postMessage({ type: 'game', bytes, filename: 'scroll-clearscreen-after-wait-test.aslx' });
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
await previewPage.click('#qv-debugger-list [data-item="clearafterwait"]');
await previewPage.waitForSelector('[data-run-walkthrough="clearafterwait"]');
await previewPage.click('[data-run-walkthrough="clearafterwait"]');

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

const page1Exists = await previewPage.$('#page1mark');
check('Page 1 content was actually cleared (not just scrolled past)', page1Exists === null);

const page2Rect = await previewPage.$eval('#page2mark', el => el.getBoundingClientRect());
console.log('  #page2mark rect:', JSON.stringify(page2Rect));
check(
    'Page 2 start is visible in the viewport, not scrolled off the top by a stale pre-clear target',
    page2Rect.top >= 0 && page2Rect.top < 500
);

const scrollTop = await previewPage.evaluate(() => document.scrollingElement.scrollTop);
console.log('  final scrollTop:', scrollTop);
check('Final scroll position is near the top of the (now very short) cleared page', scrollTop < 100);

await browser.close();

if (failed) {
    console.error('\nFAIL: one or more checks failed.');
    process.exit(1);
}
console.log('\nPASS');
