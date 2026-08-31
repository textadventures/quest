// Regression test for textadventures/quest#2095: Core.aslx's InitInterface used to target
// "#status", an element id that was renamed to "#qv-status" in commit 42cdd9b8 (so the Save
// button's container stays visible regardless of what the location-bar settings do). All 8
// JS.setCss/display calls driving customlocationcolour/customlocationtextcolour/
// customlocationbordercolour and the showlocation display toggle were silent no-ops as a
// result. This verifies both halves of the fix:
//   1. The custom colours actually apply to #qv-status.
//   2. Turning showlocation off hides #location but does NOT hide #qv-status while the Save
//      button is present - the exact behaviour the 42cdd9b8 rename was protecting.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function run() {
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/location-bar-custom-colour-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    // #txtCommand attaches to the DOM before InitInterface has finished issuing all its
    // JS.setCss calls (single-threaded WASM interop - see CLAUDE.md), so poll for the
    // colour to actually land rather than racing it.
    await page.waitForFunction(
        () => getComputedStyle(document.querySelector('#qv-status')).backgroundColor === 'rgb(255, 0, 255)',
        { timeout: 10000 },
    );

    const style = await page.$eval('#qv-status', el => {
        const cs = getComputedStyle(el);
        return { background: cs.backgroundColor, border: cs.borderColor, color: cs.color, display: cs.display };
    });

    if (style.background !== 'rgb(255, 0, 255)') {
        throw new Error(`customlocationcolour not applied: expected rgb(255, 0, 255), got ${style.background}`);
    }
    console.log('PASS: customlocationcolour applied to #qv-status background');

    if (style.border !== 'rgb(0, 0, 0)') {
        throw new Error(`customlocationbordercolour not applied: expected rgb(0, 0, 0), got ${style.border}`);
    }
    console.log('PASS: customlocationbordercolour applied to #qv-status border');

    if (style.color !== 'rgb(255, 255, 255)') {
        throw new Error(`customlocationtextcolour not applied: expected rgb(255, 255, 255), got ${style.color}`);
    }
    console.log('PASS: customlocationtextcolour applied to #qv-status text colour');

    if (style.display !== 'block') {
        throw new Error(`#qv-status should be visible when showlocation=true, got display:${style.display}`);
    }
    console.log('PASS: #qv-status visible when showlocation=true');

    const locationDisplay = await page.$eval('#location', el => getComputedStyle(el).display);
    if (locationDisplay !== 'block') {
        throw new Error(`#location should be visible when showlocation=true, got display:${locationDisplay}`);
    }
    console.log('PASS: #location visible when showlocation=true');

    await page.goto(`${baseUrl}/?url=/e2e-fixtures/location-bar-hidden-save-visible-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    // Same InitInterface-hasn't-finished-yet race as above: wait for #qv-status to settle
    // into "block" (driven by updateStatusVisibility() via the JS.uiHide("#location") call)
    // before asserting on it or its sibling #location.
    await page.waitForFunction(
        () => getComputedStyle(document.querySelector('#qv-status')).display === 'block',
        { timeout: 10000 },
    );

    const locationDisplay2 = await page.$eval('#location', el => getComputedStyle(el).display);
    if (locationDisplay2 !== 'none') {
        throw new Error(`#location should be hidden when showlocation=false, got display:${locationDisplay2}`);
    }
    console.log('PASS: #location hidden when showlocation=false');

    const saveVisible = await page.$eval('#cmdSave', el => getComputedStyle(el).display !== 'none');
    if (!saveVisible) {
        throw new Error('#cmdSave should remain visible when showlocation=false');
    }
    console.log('PASS: #cmdSave still visible when showlocation=false');

    const statusDisplay = await page.$eval('#qv-status', el => getComputedStyle(el).display);
    if (statusDisplay !== 'block') {
        throw new Error(`#qv-status should stay visible (Save button present) when showlocation=false, got display:${statusDisplay} - this is the regression 42cdd9b8 was protecting against`);
    }
    console.log('PASS: #qv-status stays visible when showlocation=false because Save button is present');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
