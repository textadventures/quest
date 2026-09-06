// Regression check for issue #2184: clicking a wait()'s "Continue..." link
// flashed the command input into view for ~100ms before the game's next page
// of text appeared and hid it again.
//
// Cause: sendEndWait() called waitEnded() synchronously at click time, so the
// input was restored immediately and only re-hidden once the engine had
// resumed and reached the *next* wait() - i.e. for the whole round trip.
// endPause() had the same shape (it showed the command bar itself, then told
// the engine 100ms later), so chained Pause() calls flashed the same way.
//
// Both now restore the command bar only after uiEndWait()/uiEndPause()
// resolves - by which point the resumed turn has reached its next stopping
// point and flushed its UI calls - and no-op if that turn has meanwhile
// claimed the command bar itself (another wait/pause, or game over).
//
// Also covers the follow-up ask: the command input takes focus again when it
// comes back, so the player can keep typing without clicking into it first.
//
// Fixture: tests/e2e/fixtures/wait-pause-input-flicker-test.aslx
//   "gowait"  - prints a line, wait()s, prints #waitpage2 and wait()s again,
//               then prints #waitpage3 and ends the turn.
//   "gopause" - prints a line, Pause(1)s, prints #pausepage2 and Pause(1)s
//               again, then prints #pausepage3. (Pause needs asl version 600;
//               it throws for 550-580.)
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

const isVisible = selector => page.evaluate(sel => {
    const el = document.querySelector(sel);
    return !!(el && el.offsetParent !== null);
}, selector);

const focusedId = () => page.evaluate(() => document.activeElement && document.activeElement.id);

// Samples visibility on every animation frame, so a flash too brief for a
// Playwright poll to catch still gets recorded.
async function startSampling(durationMs) {
    await page.evaluate(ms => {
        window.__samples = [];
        const start = performance.now();
        const vis = sel => {
            const el = document.querySelector(sel);
            return !!(el && el.offsetParent !== null);
        };
        const tick = () => {
            window.__samples.push({
                t: Math.round(performance.now() - start),
                cmd: vis('#txtCommand'),
                bar: vis('#txtCommandDiv'),
                link: vis('#endWaitLink')
            });
            if (performance.now() - start < ms) requestAnimationFrame(tick);
        };
        requestAnimationFrame(tick);
    }, durationMs);
}

const samples = () => page.evaluate(() => window.__samples);

async function checkWait() {
    await page.fill('#txtCommand', 'gowait');
    await page.press('#txtCommand', 'Enter');
    await page.waitForSelector('#endWaitLink', { state: 'visible', timeout: 10000 });

    if (await isVisible('#txtCommand')) {
        throw new Error('command input should be hidden while the game is waiting');
    }
    console.log('PASS: command input is hidden while the first wait() is pending');

    const samplingMs = 3000;
    await startSampling(samplingMs);
    await page.click('#endWaitLink');
    await page.waitForSelector('#waitpage2', { state: 'attached', timeout: 10000 });
    await page.waitForTimeout(samplingMs + 200);

    const frames = await samples();
    if (frames.length < 10) {
        throw new Error(`expected the frame sampler to collect samples, got ${frames.length}`);
    }
    const flashes = frames.filter(s => s.cmd);
    if (flashes.length > 0) {
        throw new Error(
            `command input became visible ${flashes.length} frame(s) between the two chained waits ` +
            `(first at t=${flashes[0].t}ms)`);
    }
    console.log(`PASS: command input never flashed across ${frames.length} frames while the second wait() took over`);

    if (!(await isVisible('#endWaitLink'))) {
        throw new Error('the second wait()\'s Continue link should be visible');
    }
    console.log('PASS: the second wait()\'s Continue link is showing');

    // The other half of the fix: ending the last wait must still bring the
    // command input back, since nothing else in the player ever re-shows it.
    await page.click('#endWaitLink');
    await page.waitForSelector('#waitpage3', { state: 'attached', timeout: 10000 });
    await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 10000 });
    if (await isVisible('#endWaitLink')) {
        throw new Error('the Continue link should be hidden once the last wait() has ended');
    }
    console.log('PASS: command input is restored (and the Continue link hidden) after the final wait()');

    const focused = await focusedId();
    if (focused !== 'txtCommand') {
        throw new Error(`command input should have focus after the final wait(), but focus is on "${focused}"`);
    }
    console.log('PASS: command input has focus again after the final wait()');
}

async function checkPause() {
    // Two chained 1s pauses, so 3s of sampling covers both plus the tail.
    const samplingMs = 3000;
    await startSampling(samplingMs);
    await page.fill('#txtCommand', 'gopause');
    await page.press('#txtCommand', 'Enter');
    await page.waitForSelector('#pausepage2', { state: 'attached', timeout: 10000 });

    const midway = (await samples()).filter(s => s.t > 300);
    const shown = midway.filter(s => s.bar);
    if (shown.length > 0) {
        throw new Error(
            `command bar became visible ${shown.length} frame(s) between the two chained pauses ` +
            `(first at t=${shown[0].t}ms)`);
    }
    console.log(`PASS: command bar never flashed across ${midway.length} frames between the two chained pauses`);

    await page.waitForSelector('#pausepage3', { state: 'attached', timeout: 10000 });
    await page.waitForSelector('#txtCommandDiv', { state: 'visible', timeout: 10000 });
    console.log('PASS: command bar is restored once the last pause has ended');

    const focused = await focusedId();
    if (focused !== 'txtCommand') {
        throw new Error(`command input should have focus after the final pause, but focus is on "${focused}"`);
    }
    console.log('PASS: command input has focus again after the final pause');
}

async function run() {
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/wait-pause-input-flicker-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'visible' });

    await checkWait();
    await checkPause();

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-wait-pause-input-flicker-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
