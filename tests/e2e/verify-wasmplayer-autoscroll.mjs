// Manual verification for auto-scroll behaviour in the game output panel
// (playercore.js's scrollToEnd()/markScrollPosition()). Bug report: on a
// turn producing a long stream of output, the page should scroll just far
// enough that the *start* of that turn's output stays on screen, not jump
// straight to the document's bottom (which scrolls the beginning of the new
// text off the top). scrollToEnd() used to always target
// document.body.scrollHeight unconditionally, and queued a fresh jQuery
// .animate() on every OutputText call without .stop()'ing the previous one
// first — on a walkthrough firing several such turns back-to-back with no
// delay, that produced exactly the reported "scrolls immediately, then
// stalls, then slowly catches up" symptom (a growing backlog of queued
// animations targeting stale, already-passed scroll positions).
//
// Fixture: tests/e2e/fixtures/scroll-walkthrough-test.aslx defines six
// commands (shout1..shout6), each printing ~30 lines of filler text (well
// over one screenful) preceded by a <span id="turnmarkN"> marker at the very
// start of that turn's output. The "rapid" walkthrough fires all six with no
// delay between them — the exact repro shape (WasmPlayer's debugger
// walkthrough runner never disables animation, unlike WebPlayer's).
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
previewPage.on('console', msg => console.log(`[console.${msg.type()}]`, msg.text()));

const senderPage = await context.newPage();
const fixtureUrl = `${baseUrl}/e2e-fixtures/scroll-walkthrough-test.aslx`;
await senderPage.goto(baseUrl);

const handoffDone = senderPage.evaluate(async (url) => {
    const bytes = new Uint8Array(await (await fetch(url)).arrayBuffer());
    await new Promise((resolve) => {
        const bc = new BroadcastChannel('quest-preview');
        bc.onmessage = ({ data }) => {
            if (data.type === 'ready') {
                bc.postMessage({ type: 'game', bytes, filename: 'scroll-walkthrough-test.aslx' });
                resolve();
            }
        };
    });
}, fixtureUrl);

await previewPage.goto(`${baseUrl}/?source=editor`);
await handoffDone;

await previewPage.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
await previewPage.waitForTimeout(300);

// Sample document.scrollingElement.scrollTop throughout the run so we can
// see the shape of the scroll over time, not just the end state.
await previewPage.evaluate(() => {
    window.__scrollSamples = [];
    window.__scrollSampleTimer = setInterval(() => {
        window.__scrollSamples.push(document.scrollingElement.scrollTop);
    }, 50);
});

console.log('step: click debug');
await previewPage.click('#cmdDebug', { timeout: 10000 });
console.log('step: wait dialog open');
await previewPage.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });
console.log('step: click walkthrough tab');
await previewPage.click('#qv-debugger-tabs button:text("Walkthrough")', { timeout: 10000 });
console.log('step: select rapid');
await previewPage.click('#qv-debugger-list [data-item="rapid"]', { timeout: 10000 });
console.log('step: wait run button');
await previewPage.waitForSelector('[data-run-walkthrough="rapid"]', { timeout: 10000 });
console.log('step: click run');
await previewPage.click('[data-run-walkthrough="rapid"]', { timeout: 10000 });

console.log('step: wait for Done');
await previewPage.waitForFunction(() => {
    const el = document.querySelector('[data-walkthrough-status]');
    return el && el.textContent && el.textContent !== 'Running…';
}, { timeout: 15000 });
console.log('step: got status');
const status = await previewPage.$eval('[data-walkthrough-status]', el => el.textContent);
check('Walkthrough runs to completion', status === 'Done');

await previewPage.click('#qv-debugger-close');

// Let any trailing scroll animation settle (scrollToEnd()'s animation duration
// is capped at 2000ms), then stop sampling.
await previewPage.waitForTimeout(2500);
const samples = await previewPage.evaluate(() => {
    clearInterval(window.__scrollSampleTimer);
    return window.__scrollSamples;
});
console.log('  scrollTop samples over time:', samples.join(', '));

// ── The core assertion: the last turn's marker should still be near the top
// of the viewport, not scrolled past (off the top) or left far below (i.e.
// the fix should have scrolled *to* it, not *past* it to the document end).
const turn6Rect = await previewPage.$eval('#turnmark6', el => el.getBoundingClientRect());
console.log('  #turnmark6 rect:', JSON.stringify(turn6Rect));
check(
    'Start of the final turn (#turnmark6) is visible, not scrolled off the top',
    turn6Rect.top >= 0 && turn6Rect.top < 768
);

const finalScrollTop = await previewPage.evaluate(() => document.scrollingElement.scrollTop);
const maxScrollTop = await previewPage.evaluate(() => document.scrollingElement.scrollHeight - window.innerHeight);
console.log(`  finalScrollTop=${finalScrollTop} maxScrollTop=${maxScrollTop}`);
check(
    'Final scroll position stops well short of the true document bottom (does not overshoot past the last turn\'s start)',
    finalScrollTop < maxScrollTop - 200
);

// ── No pathological backlog: once scrolling reaches its final position it
// should stay flat, not keep creeping upward/downward well after the
// walkthrough itself already reported Done (a queued-animation backlog
// draining after the fact).
const tailSamples = samples.slice(-6);
const tailSpread = Math.max(...tailSamples) - Math.min(...tailSamples);
check('Scroll position is settled (not still drifting) by the time sampling stops', tailSpread < 5);

await browser.close();

if (failed) {
    console.error('\nFAIL: one or more checks failed.');
    process.exit(1);
}
console.log('\nPASS');
