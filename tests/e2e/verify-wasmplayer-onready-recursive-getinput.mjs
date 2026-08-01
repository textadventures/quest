// Regression check for a bug reported against Giantkiller Too's Indoor Market
// partition puzzle: the map (and other on-ready-driven UI, like the location
// header) stopped updating partway through a "get_partition_sequence"-style
// puzzle loop, where each response opens the next get input from inside the
// current callback, before that callback's own finally block runs. This kept
// WorldModel._pendingCallbackCount above zero indefinitely while the loop ran,
// so anything queued via AddOnReady (changedparent -> OnEnterRoom -> on ready,
// which drives JS.updateLocation and the automatic map redraw) never got
// flushed - EndPendingCallbackAsync only drained the queue once the count
// returned to exactly 0, which the loop's structure never allowed.
//
// That part is fixed in WorldModel.EndPendingCallbackAsync (drains whenever
// its own Begin/End pair resolves, not only at global count zero) - confirmed
// by tracing that the drain now runs and completes without error every turn.
//
// BUT: this script demonstrates the fix is still not enough for WasmPlayer
// specifically. WasmPlayerBridge buffers JS.*() calls (_uiBuffer) and only
// flushes them once WorldModel's _turnSuspendedTcs resolves (see
// FlushBufferAndYieldAsync, called right after `await _game.SendCommand()`).
// GetInputScript.ExecuteAsync calls SignalTurnSuspended() unconditionally
// every time a *new* get input is opened - including the one the puzzle loop
// opens for its *next* prompt, from inside the current response's own
// callback, before EndPendingCallbackAsync's drain has run. Since
// _turnSuspendedTcs only resolves once (TrySetResult, first call wins), that
// nested get-input's signal fires and resolves it *before* the drain's output
// has been added to _uiBuffer - so WasmPlayerBridge flushes too early and
// misses it. The drained output isn't lost, just buffered - it only becomes
// visible once the *next* command's own flush runs, so the map (and location
// header) is permanently one input behind wherever the player actually is.
// WebPlayer doesn't have this bug because its IPlayer.RunScriptAsync isn't
// buffered this way - each JS call goes out as it happens.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function waitUntilCanSendCommand() {
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 10000 });
}

async function sendCommand(command) {
    await waitUntilCanSendCommand();
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
}

async function locationText() {
    return await page.$eval('#location', el => el.textContent.trim());
}

async function assertLocation(expected, label) {
    const actual = await locationText();
    if (actual !== expected) {
        throw new Error(`${label}: expected location "${expected}", got "${actual}"`);
    }
    console.log(`PASS: ${label} (location="${actual}")`);
}

async function run() {
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/onready-recursive-getinput-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });

    await sendCommand('startloop');
    await page.waitForTimeout(300);
    await assertLocation('A RoomA', 'location updates on the first move (top-level command, count starts at 0)');

    // This is the buggy transition: resolving the puzzle loop's get input opens
    // the *next* get input before this callback's own finally decrements the
    // count, so it never returns to 0 while the loop keeps running.
    await sendCommand('b');
    await page.waitForTimeout(300);
    await assertLocation('A RoomB', 'location updates on a move made from inside the recursive get-input loop');

    await sendCommand('a');
    await page.waitForTimeout(300);
    await assertLocation('A RoomA', 'location updates on a second nested-loop move');

    await sendCommand('stop');
    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-onready-recursive-getinput-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
