// Verifies the "Timers" example documented in
// site/src/content/docs/howto/ux/ui-callback.md — a JavaScript setInterval
// that updates the custom status pane itself and calls back into the game
// exactly once, through ASLEvent, when the countdown reaches zero.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-aslevent-timer.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';
const GAME = '/e2e-fixtures/aslevent-timer-test.aslx';

const browser = await chromium.launch();
const page = await browser.newPage();
const consoleErrors = [];
page.on('console', (m) => { if (m.type() === 'error') consoleErrors.push(m.text()); });
page.on('pageerror', (e) => consoleErrors.push(`pageerror: ${e.message}`));

try {
    await page.goto(`${BASE}/?url=${encodeURIComponent(GAME)}`);

    // Wait for the game to boot.
    const output = page.locator('#divOutput');
    await output.getByText('ASLEventTimerTest').first().waitFor({ timeout: 60000 });

    // The start script ran JS.addScript, so the function it defined must exist.
    const hasFn = await page.evaluate(() => typeof window.startCountdown === 'function');
    if (!hasFn) {
        throw new Error('JS.addScript did not define startCountdown() on the page');
    }

    // Kick off a 2-second countdown.
    const input = page.locator('#txtCommand');
    await input.fill('start');
    await input.press('Enter');
    await output.getByText('Countdown started.').first().waitFor({ timeout: 15000 });

    // While it runs, the custom status pane must be updated from JavaScript,
    // in tenths of a second, without the game world being involved.
    const status = page.locator('#customStatusPane');
    await status.waitFor({ state: 'visible', timeout: 10000 });

    const samples = [];
    for (let i = 0; i < 6; i++) {
        samples.push((await status.textContent())?.trim() ?? '');
        await page.waitForTimeout(150);
    }
    const ticking = samples.filter((s) => /^Time remaining: \d+\.\d$/.test(s));
    if (ticking.length < 4) {
        throw new Error(`custom status pane not counting down in tenths; samples: ${JSON.stringify(samples)}`);
    }
    if (new Set(ticking).size < 3) {
        throw new Error(`custom status pane not changing between samples: ${JSON.stringify(samples)}`);
    }

    // At zero, ASLEvent must call the game's CountdownFinished function, and
    // its string parameter must arrive intact.
    await output.getByText('Out of time! (timeout)').first().waitFor({ timeout: 15000 });

    // ...exactly once: the interval is cleared, so no repeat calls.
    await page.waitForTimeout(1500);
    const finishedCount = await output.getByText('Out of time! (timeout)').count();
    if (finishedCount !== 1) {
        throw new Error(`expected exactly 1 ASLEvent callback, got ${finishedCount}`);
    }

    const finalStatus = (await status.textContent())?.trim();
    if (finalStatus !== 'Time remaining: 0.0') {
        throw new Error(`expected final status "Time remaining: 0.0", got "${finalStatus}"`);
    }

    if (consoleErrors.length > 0) {
        throw new Error(`console errors: ${JSON.stringify(consoleErrors)}`);
    }

    console.log('PASS: countdown ticked in the browser, ASLEvent fired once at zero');
    console.log(`  status samples: ${JSON.stringify(samples)}`);
} catch (err) {
    console.error('FAIL:', err.message);
    if (consoleErrors.length > 0) console.error('console errors:', consoleErrors);
    process.exitCode = 1;
} finally {
    await browser.close();
}
