// Manual verification that growing the static picture frame (#gamePanel —
// SetFramePicture/JS.setPanelContents → setPanelHeight(), e.g. "The Shack")
// after a turn has already ended doesn't leave that turn's opening line
// hidden behind the frame.
//
// Root cause (found via this test): scrollToTurnStart() unconditionally
// called $("#txtCommand").focus() after positioning the scroll. Calling
// .focus() on an element that ISN'T already the focused one makes the
// browser scroll it into view natively — and since #txtCommand sits right
// after all of #divOutput, that silently jumped straight to the document's
// bottom, discarding every position calculated above it. #txtCommand is
// often not already focused in exactly this scenario: right after page
// load, on mobile before the on-screen keyboard has been tapped, or any
// time focus was lost to a click/selection elsewhere — which is why this
// was "more noticeable on a small [mobile] browser window". Fixed by
// focusing with {preventScroll: true} (focusCommandInput()).
//
// This test simulates a slow-loading picture directly (real network image
// timing isn't controllable/deterministic here): run a normal turn with the
// frame still at its natural (empty, ~0-height) size, let the turn boundary
// finish and settle as it normally would — including losing input focus,
// simulating a player who tapped/clicked elsewhere — then simulate the
// image's delayed onload firing afterwards by growing #gamePanel and calling
// window.setPanelHeight() directly, exactly what the real onload handler does.
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

const page = await browser.newPage({ viewport: { width: 500, height: 500 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

await page.goto(`${baseUrl}/?url=/e2e-fixtures/scroll-walkthrough-test.aslx`);
await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
await page.waitForTimeout(300);

console.log('typing shout1 (frame still empty/0-height, as it would be before a real image loads)');
await page.fill('#txtCommand', 'shout1');
await page.press('#txtCommand', 'Enter');
await page.waitForFunction(() => document.querySelector('#divOutput')?.textContent.includes('Turn 1, filler line 60'), { timeout: 15000 });
// Let the turn-boundary scroll settle, exactly as it would in real play.
await page.waitForTimeout(1000);

const turnmark1BeforeGrow = await page.$eval('#turnmark1', el => el.getBoundingClientRect());
console.log('  #turnmark1 rect before frame grows:', JSON.stringify(turnmark1BeforeGrow));

// Blur the input first — simulating a player who tapped/clicked elsewhere
// (or a mobile session where it was never focused in the first place), the
// exact condition that exposed the .focus()-triggered scroll hijack.
await page.evaluate(() => document.getElementById('txtCommand')?.blur());

console.log('simulating the picture finishing loading well after the turn ended (growing #gamePanel + calling window.setPanelHeight())');
await page.evaluate(() => {
    const panel = document.getElementById('gamePanel');
    panel.innerHTML = 'FRAME';
    panel.style.cssText += ';display:block;height:150px;background:red;';
    window.setPanelHeight();
});

// setPanelHeight() itself waits 100ms before re-scrolling; give the
// (possibly up-to-2s) resulting animation time to settle too.
await page.waitForTimeout(2500);

const frameRect = await page.$eval('#gamePanel', el => el.getBoundingClientRect());
const turnmark1Rect = await page.$eval('#turnmark1', el => el.getBoundingClientRect());
console.log('  #gamePanel rect after growing:', JSON.stringify(frameRect));
console.log('  #turnmark1 rect after re-scroll:', JSON.stringify(turnmark1Rect));

check(
    'Turn 1 start is still on screen after the frame grows',
    turnmark1Rect.top >= 0 && turnmark1Rect.top < 500
);
check(
    'Turn 1 start is not left hidden behind the now-fully-sized picture frame',
    turnmark1Rect.top >= frameRect.bottom
);

await browser.close();

if (failed) {
    console.error('\nFAIL: one or more checks failed.');
    process.exit(1);
}
console.log('\nPASS');
