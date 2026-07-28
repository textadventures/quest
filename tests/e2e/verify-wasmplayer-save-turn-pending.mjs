// Ad-hoc manual verification: WasmPlayer's Save button now defaults to
// disabled and only enables once the engine confirms a turn has genuinely
// finished with nothing outstanding (WorldModel._pendingCallbackCount == 0,
// pushed via IPlayer.SetTurnPending) - fixing a flicker where Save briefly
// showed enabled between chained wait()s (e.g. a game with several waits at
// startup). Also verifies Save gets disabled again once the game finishes.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function isSaveDisabled() {
    return await page.$eval('#cmdSave', el => el.disabled || el.classList.contains('ui-state-disabled'));
}

async function assertSaveDisabled(expected, label) {
    const actual = await isSaveDisabled();
    if (actual !== expected) {
        throw new Error(`${label}: expected Save disabled=${expected}, got ${actual}`);
    }
    console.log(`PASS: ${label} (disabled=${actual})`);
}

// sendCommand() in player.js silently drops a command while canSendCommand is
// still false from the previous one's round-trip - waiting on a fixed sleep
// between commands is flaky, so poll the same flag the client uses instead.
async function waitUntilCanSendCommand() {
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 10000 });
}

async function sendCommand(command) {
    await waitUntilCanSendCommand();
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
}

async function run() {
    // save-turn-pending-test.aslx's StartGame chains two nested wait{} blocks
    // before the game is genuinely idle - this is the "several waits at the
    // start of the game" scenario from the bug report.
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/save-turn-pending-test.aslx`);
    // StartGame hits a wait{} immediately, so #txtCommand is hidden (not just
    // present) right from boot - wait for it attached rather than visible.
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    await page.waitForSelector('#endWaitLink', { timeout: 30000 });
    console.log('PASS: game booted; StartGame is chaining two wait{} blocks');

    await assertSaveDisabled(true, 'Save disabled while first wait is pending');

    await page.click('#endWaitLink');
    await page.waitForTimeout(300);
    await assertSaveDisabled(true, 'Save disabled between the two chained waits (the flicker bug)');

    await page.click('#endWaitLink');
    await page.waitForTimeout(300);
    await assertSaveDisabled(false, 'Save enabled once both waits are done and StartGame is idle');

    // Sanity: a normal command that doesn't hit wait/get-input/pause/menu/ask still enables Save
    await sendCommand('noop');
    await page.waitForTimeout(300);
    await assertSaveDisabled(false, 'Save enabled after an ordinary non-suspending command');

    // "get input { }" statement form (GetInputScript) - already covered by
    // verify-wasmplayer-save-disabled-during-wait-input.mjs, repeated here so
    // this fixture alone demonstrates every TCS-suspension path.
    await sendCommand('getinputscript');
    await page.waitForTimeout(300);
    await assertSaveDisabled(true, 'Save disabled during get input {} script');
    await sendCommand('answer1');
    await page.waitForTimeout(300);
    await assertSaveDisabled(false, 'Save enabled after get input {} answered');

    // GetInput() expression function form (ExpressionOwner.GetInput) - this one
    // was genuinely missing BeginPendingCallback/EndPendingCallbackAsync (found
    // via code audit prompted by this test-coverage question, not previously
    // caught) - awaited inline exactly like the legacy DoWaitAsync/DoPauseAsync
    // gap, so _pendingCallbackCount never incremented and Save would have stayed
    // enabled throughout.
    await sendCommand('getinputfn');
    await page.waitForTimeout(300);
    await assertSaveDisabled(true, 'Save disabled during GetInput() function');
    await sendCommand('answer2');
    await page.waitForTimeout(300);
    await assertSaveDisabled(false, 'Save enabled after GetInput() answered');

    // Ask() expression function form (ExpressionOwner.Ask) - same class of gap
    // as GetInput(), found and fixed at the same time.
    await sendCommand('askfn');
    await page.waitForTimeout(300);
    await assertSaveDisabled(true, 'Save disabled while Ask() dialog is showing');
    await page.click('button:has-text("Yes")');
    await page.waitForTimeout(300);
    await assertSaveDisabled(false, 'Save enabled after Ask() answered');

    // ShowMenu() expression function form (ExpressionOwner.ShowMenu) - same class
    // of gap, found and fixed at the same time. (The "show menu { }" statement
    // form, ShowMenuScript, already had BeginPendingCallback - only this
    // function-call form was missing it.)
    await sendCommand('menufn');
    await page.waitForTimeout(300);
    await assertSaveDisabled(true, 'Save disabled while ShowMenu() dialog is showing');
    // dialogSelect() in player.js reads $("#dialogOptions").val(), which jQuery
    // returns as null (not the visually-shown first option) unless an <option>
    // was explicitly selected - a separate, pre-existing bug independent of this
    // change (fixed in #1951) - select explicitly to exercise this path the way
    // a real player would (clicking an option before "Select").
    await page.selectOption('#dialogOptions', { index: 0 });
    await page.click('button:has-text("Select")');
    await page.waitForTimeout(300);
    await assertSaveDisabled(false, 'Save enabled after ShowMenu() answered');

    // Game completion should disable Save even though the turn itself finished cleanly
    await sendCommand('finishit');
    await page.waitForTimeout(600);
    await assertSaveDisabled(true, 'Save disabled after game completion');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-save-turn-pending-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
