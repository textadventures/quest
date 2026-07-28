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

// save-turn-pending-test.aslx's StartGame chains two nested wait{} blocks
// before the game is genuinely idle - this is the "several waits at the
// start of the game" scenario from the bug report.
await page.goto(`${baseUrl}/?url=/e2e-fixtures/save-turn-pending-test.aslx`);
// StartGame hits a wait{} immediately, so #txtCommand is hidden (not just
// present) right from boot - wait for it attached rather than visible.
await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
await page.waitForSelector('#endWaitLink', { timeout: 30000 });
console.log('Game booted; StartGame is chaining two wait{} blocks.');

console.log('Save disabled while first wait is pending (expect true):', await isSaveDisabled());

await page.click('#endWaitLink');
await page.waitForTimeout(300);
console.log('Save disabled between the two chained waits (expect true, this is the flicker bug):', await isSaveDisabled());

await page.click('#endWaitLink');
await page.waitForTimeout(300);
console.log('Save disabled once both waits are done and StartGame is idle (expect false):', await isSaveDisabled());

// Sanity: a normal command that doesn't hit wait/get-input/pause/menu/ask still enables Save
await sendCommand('noop');
await page.waitForTimeout(300);
console.log('Save disabled after an ordinary non-suspending command (expect false):', await isSaveDisabled());

// "get input { }" statement form (GetInputScript) - already covered by
// verify-wasmplayer-save-disabled-during-wait-input.mjs, repeated here so
// this fixture alone demonstrates every TCS-suspension path.
await sendCommand('getinputscript');
await page.waitForTimeout(300);
console.log('Save disabled during get input {} script (expect true):', await isSaveDisabled());
await sendCommand('answer1');
await page.waitForTimeout(300);
console.log('Save disabled after get input {} answered (expect false):', await isSaveDisabled());

// GetInput() expression function form (ExpressionOwner.GetInput) - this one
// was genuinely missing BeginPendingCallback/EndPendingCallbackAsync (found
// via code audit prompted by this test-coverage question, not previously
// caught) - awaited inline exactly like the legacy DoWaitAsync/DoPauseAsync
// gap, so _pendingCallbackCount never incremented and Save would have stayed
// enabled throughout.
await sendCommand('getinputfn');
await page.waitForTimeout(300);
console.log('Save disabled during GetInput() function (expect true):', await isSaveDisabled());
await sendCommand('answer2');
await page.waitForTimeout(300);
console.log('Save disabled after GetInput() answered (expect false):', await isSaveDisabled());

// Ask() expression function form (ExpressionOwner.Ask) - same class of gap
// as GetInput(), found and fixed at the same time.
await sendCommand('askfn');
await page.waitForTimeout(300);
console.log('Save disabled while Ask() dialog is showing (expect true):', await isSaveDisabled());
await page.click('button:has-text("Yes")');
await page.waitForTimeout(300);
console.log('Save disabled after Ask() answered (expect false):', await isSaveDisabled());

// ShowMenu() expression function form (ExpressionOwner.ShowMenu) - same class
// of gap, found and fixed at the same time. (The "show menu { }" statement
// form, ShowMenuScript, already had BeginPendingCallback - only this
// function-call form was missing it.)
await sendCommand('menufn');
await page.waitForTimeout(300);
console.log('Save disabled while ShowMenu() dialog is showing (expect true):', await isSaveDisabled());
// dialogSelect() in player.js reads $("#dialogOptions").val(), which jQuery
// returns as null (not the visually-shown first option) unless an <option>
// was explicitly selected - a separate, pre-existing bug independent of this
// change (see conversation) - select explicitly to exercise this path the
// way a real player would (clicking an option before "Select").
await page.selectOption('#dialogOptions', { index: 0 });
await page.click('button:has-text("Select")');
await page.waitForTimeout(300);
console.log('Save disabled after ShowMenu() answered (expect false):', await isSaveDisabled());

// Game completion should disable Save even though the turn itself finished cleanly
await sendCommand('finishit');
await page.waitForTimeout(600);
console.log('Save disabled after game completion (expect true):', await isSaveDisabled());

await browser.close();
