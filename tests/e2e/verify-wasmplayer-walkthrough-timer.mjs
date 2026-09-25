// A debugger walkthrough sends its steps straight to the game rather than through the player's
// own sendCommand, which is what normally stops the real-time timer until a turn ends. So the
// timer kept ticking during a walkthrough, and a tick that landed while a step was still running
// (the player yields to the browser part-way through a long step) ran the timer's script in the
// middle of that step's output.
//
// Fixture: tests/e2e/fixtures/walkthrough-timer-test.aslx's "bursts" walkthrough runs 150 steps
// that each print a "burst start N" ... "burst end N" block, while a 1-second timer prints
// "TICK N". Checks that no TICK lands inside a block, that the blocks come out whole and in order,
// and that the timer still fires during the walkthrough (between steps). Its "blockingtimer"
// walkthrough enables a timer that stops at a blocking wait, which the walkthrough has to click
// through without deadlocking.
//
// Requires the WasmPlayer dev server running locally:
//   node ../../src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const steps = 150;

const browser = await chromium.launch();

// Runs a walkthrough in a fresh session and returns its output as trimmed, non-empty lines.
async function runWalkthrough(name) {
    const page = await browser.newPage();
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));

    await page.goto(`${baseUrl}/?url=/e2e-fixtures/walkthrough-timer-test.aslx`);
    await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });

    await page.evaluate(() => WebPlayer.setCanDebug(true));
    await page.click('#cmdDebug');
    await page.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });
    await page.click('#qv-debugger-tabs button:text("Walkthrough")');
    await page.click(`#qv-debugger-list [data-item="${name}"]`);
    await page.click(`[data-run-walkthrough="${name}"]`);

    await page.waitForFunction(() => {
        const el = document.querySelector('[data-walkthrough-status]');
        return el && el.textContent && el.textContent !== 'Running…';
    }, undefined, { timeout: 120000 });
    const status = await page.$eval('[data-walkthrough-status]', el => el.textContent);
    if (status !== 'Done') throw new Error(`${name}: walkthrough status was "${status}", expected "Done"`);
    if (pageErrors.length > 0) throw new Error(`${name}: page errors: ${pageErrors.join('; ')}`);

    const lines = (await page.$eval('#divOutput', el => el.innerText))
        .split('\n').map(l => l.trim()).filter(l => l.length > 0);
    await page.close();
    return lines;
}

// Checks every burst block is whole and in order with no timer output inside one, and returns
// the timer lines printed after the first burst ended and before the last one started.
function checkBursts(name, lines, expectedBursts) {
    let open = null;
    let lastEnded = 0;
    const timerLinesDuring = [];
    for (const line of lines) {
        let m;
        if ((m = /^burst start (\d+)$/.exec(line))) {
            const n = Number(m[1]);
            if (open !== null) throw new Error(`${name}: burst ${n} started before burst ${open} ended`);
            if (n !== lastEnded + 1) throw new Error(`${name}: burst ${n} started after burst ${lastEnded}`);
            open = n;
        } else if ((m = /^burst end (\d+)$/.exec(line))) {
            const n = Number(m[1]);
            if (open !== n) throw new Error(`${name}: burst ${n} ended while burst ${open} was open`);
            open = null;
            lastEnded = n;
        } else if (/^(TICK \d+|timer (before|after) wait)$/.test(line)) {
            if (open !== null) throw new Error(`${name}: "${line}" was printed inside burst ${open}`);
            if (lastEnded > 0 && lastEnded < expectedBursts) timerLinesDuring.push(line);
        }
    }
    if (lastEnded !== expectedBursts) throw new Error(`${name}: only ${lastEnded} of ${expectedBursts} bursts completed`);
    return timerLinesDuring;
}

try {
    const ticks = checkBursts('bursts', await runWalkthrough('bursts'), steps);
    console.log(`PASS: all ${steps} bursts printed whole and in order, no TICK inside one`);
    if (ticks.length === 0) throw new Error('bursts: the timer never fired during the walkthrough');
    console.log(`PASS: timer still fired ${ticks.length} time(s) between walkthrough steps`);

    // A blocking wait in a timer script keeps that tick from finishing until the walkthrough
    // clicks the wait through, so the walkthrough mustn't wait for the tick to finish first.
    const timerLines = checkBursts('blockingtimer', await runWalkthrough('blockingtimer'), 30);
    if (!timerLines.includes('timer before wait') || !timerLines.includes('timer after wait')) {
        throw new Error(`blockingtimer: timer's wait wasn't clicked through (timer output: ${JSON.stringify(timerLines)})`);
    }
    console.log('PASS: a blocking wait in a timer script is clicked through, between steps');

    console.log('\nPASS');
} catch (err) {
    console.error(`\nFAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser.close();
}
