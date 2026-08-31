// Regression test for textadventures/quest#2095: Core.aslx's InitInterface used to target
// "#status", an element id that was renamed to "#qv-status" in commit 42cdd9b8 (so the Save
// button's container stays visible regardless of what the location-bar settings do). All 8
// JS.setCss/display calls driving customlocationcolour/customlocationtextcolour/
// customlocationbordercolour and the showlocation display toggle were silent no-ops as a
// result. This verifies:
//   1. The custom colours actually apply to #qv-status.
//   2. Turning showlocation off hides #location but does NOT hide #qv-status while the Save
//      button is present - the exact behaviour the 42cdd9b8 rename was protecting.
//   3. The same holds in an editor-preview session (?source=editor), where Save is hidden
//      instead (WebPlayer.setCanSave(!isPreview)) and Debug is forced on - a pre-existing
//      bug in playercore.js's updateStatusVisibility(), found while investigating #2095, only
//      ever checked #location/#cmdSave, so #qv-status (and the Debug button it hosts, plus
//      the responsive hamburger menu once resized narrow) collapsed to display:none whenever
//      both of those happened to be off, regardless of what else was actually showing.
//   4. Resizing to a narrow window after that reveals the hamburger (#cmdShowPanes) and
//      #qv-status comes back up with it, rather than staying stuck hidden.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';
import { readFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
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

    // Editor-preview session: source=editor hands the game over via BroadcastChannel
    // instead of a URL, and flips isPreview - which hides Save and forces Debug on
    // (see wasm-player.js's initWasmPlayer/WebPlayer.setCanSave|setCanDebug).
    await page.goto(`${baseUrl}/?source=editor`);
    await page.waitForTimeout(500);
    const fixtureBytes = [...readFileSync(join(here, 'fixtures', 'location-bar-hidden-save-visible-test.aslx'))];
    await page.evaluate(({ bytes, filename }) => {
        const bc = new BroadcastChannel('quest-preview');
        bc.postMessage({ type: 'game', bytes: new Uint8Array(bytes), filename });
    }, { bytes: fixtureBytes, filename: 'location-bar-hidden-save-visible-test.aslx' });
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    // wasm-player.js's boot order is: WebPlayer.initUI() (which calls
    // updateStatusVisibility() once, while #cmdSave is still at its pre-setCanSave
    // default-visible state) -> setCanSave(false)/setCanDebug(true) (neither of which
    // re-triggers updateStatusVisibility()) -> Bridge.Begin(), which is what actually
    // runs Core.aslx's InitInterface and its JS.uiHide("#location") call - the first
    // one that recomputes #qv-status against the real post-setCanSave state. Waiting
    // on #cmdDebug alone (settled by setCanDebug, before Bridge.Begin()) would read
    // #qv-status before that recomputation ever happens, so wait on #location's own
    // inline style instead - jQuery's .hide() sets it directly, unlike the "none" it
    // already has by default from CSS alone, so this can only become true once
    // InitInterface has actually run.
    await page.waitForFunction(
        () => document.querySelector('#location').style.display === 'none',
        { timeout: 10000 },
    );

    const previewDisplays = await page.evaluate(() => ({
        qvStatus: getComputedStyle(document.querySelector('#qv-status')).display,
        cmdSave: getComputedStyle(document.querySelector('#cmdSave')).display,
        cmdDebug: getComputedStyle(document.querySelector('#cmdDebug')).display,
    }));
    if (previewDisplays.cmdSave !== 'none') {
        throw new Error(`Save should be hidden in an editor-preview session, got display:${previewDisplays.cmdSave}`);
    }
    if (previewDisplays.qvStatus !== 'block') {
        throw new Error(`#qv-status should stay visible in editor-preview with showlocation=false (Debug button present), got display:${previewDisplays.qvStatus}`);
    }
    console.log('PASS: #qv-status stays visible in editor-preview when showlocation=false because Debug button is present (Save is hidden there)');

    // Resize to a narrow window: the hamburger (#cmdShowPanes) should appear, and
    // #qv-status must come up with it - doLayout() sets cmdShowPanes' own display on
    // resize, but only calling updateStatusVisibility() from there makes #qv-status
    // itself react.
    await page.setViewportSize({ width: 500, height: 800 });
    await page.waitForFunction(
        () => getComputedStyle(document.querySelector('#cmdShowPanes')).display !== 'none',
        { timeout: 10000 },
    );
    const narrowStatusDisplay = await page.$eval('#qv-status', el => getComputedStyle(el).display);
    if (narrowStatusDisplay !== 'block') {
        throw new Error(`#qv-status should stay visible after resizing narrow (hamburger now showing), got display:${narrowStatusDisplay}`);
    }
    console.log('PASS: #qv-status visible after resize reveals the hamburger menu');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
