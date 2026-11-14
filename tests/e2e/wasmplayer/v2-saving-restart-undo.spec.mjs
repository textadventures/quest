// Checks saving, restart and undo on player UI v2, with the checks of legacy's
// save-disabled-during-wait-input.spec.mjs, save-turn-pending, restart-sidebar-dup and
// undo-after-command, on fixtures/v2-restart-test.aslx (610) and, under ?ui=v2, those scripts'
// fixtures, which have live Core:
//
// - The Save / Load button is disabled whenever the engine is suspended mid-turn (wait, get input,
//   GetInput, Ask, ShowMenu), including between chained waits at the start of the game, and enabled
//   between turns and after the game has finished, when it's the way to load or restart.
// - Starting again rebuilds the UI, so a pane the game adds isn't duplicated, and the game's own
//   script file runs again.
// - Undo undoes each turn once, including after "undo, then another command".
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('v2-saving-restart-undo', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const input = 'qv-command-bar input';
    const continueButton = 'qv-command-bar .qv-continue';

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    let browser;

    async function open(url, ready = input) {
        const page = await browser.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));
        await page.goto(`${baseUrl}/?url=${url}`);
        await page.waitForSelector(ready, { state: 'visible', timeout: 60000 });
        return page;
    }

    async function send(page, command) {
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
        await page.waitForSelector(input, { state: 'visible', timeout: 15000 });
        await page.fill(input, command);
        await page.press(input, 'Enter');
    }

    const hasOutput = (page, text) => page.waitForFunction(
        t => document.querySelector('qv-output').textContent.includes(t), text, { timeout: 15000 });

    async function saveShouldBe(page, disabled, what) {
        await page.waitForFunction(d => document.getElementById('cmdSave').disabled === d, disabled, { timeout: 5000 })
            .catch(() => { throw new Error(`Save / Load should be ${disabled ? 'disabled' : 'enabled'}: ${what}`); });
        // Long enough to see it flick back.
        await page.waitForTimeout(300);
        check(await page.$eval('#cmdSave', el => el.disabled) === disabled, `Save / Load is ${disabled ? 'disabled' : 'enabled'}: ${what}`);
    }

    const clickOption = (page, text) => page.locator('qv-output a.commandlink:visible', { hasText: text }).first().click();

    async function waitAndInput() {
        const page = await open('/examples/test.aslx&ui=v2');
        await saveShouldBe(page, false, 'before any command');
        await send(page, 'wait2');
        await saveShouldBe(page, true, 'during a wait');
        await page.click(continueButton);
        await saveShouldBe(page, false, 'after it');
        await send(page, 'input2');
        await saveShouldBe(page, true, 'during get input');
        await send(page, 'some answer');
        await saveShouldBe(page, false, 'after it');
        await page.click('#cmdSave');
        await page.waitForSelector('#qv-saves[open]', { timeout: 5000 });
        console.log('PASS: the Save / Load dialog opens between turns');
        await page.close();
    }

    async function everySuspension() {
        const page = await open('/e2e-fixtures/save-turn-pending-test.aslx&ui=v2', continueButton);
        await saveShouldBe(page, true, 'during the first of two waits at the start of the game');
        await page.evaluate(() => {
            window.saveFrames = [];
            const start = performance.now();
            const sample = () => {
                window.saveFrames.push(document.getElementById('cmdSave').disabled);
                if (performance.now() - start < 700) requestAnimationFrame(sample);
            };
            requestAnimationFrame(sample);
        });
        await page.click(continueButton);
        await page.waitForTimeout(800);
        const frames = await page.evaluate(() => window.saveFrames);
        check(frames.length > 10 && !frames.includes(false), `it stays disabled between the two (${frames.length} frames)`);
        await page.click(continueButton);
        await saveShouldBe(page, false, 'once both are done');
        await send(page, 'noop');
        await saveShouldBe(page, false, 'after an ordinary command');
        await send(page, 'getinputscript');
        await saveShouldBe(page, true, 'during get input');
        await send(page, 'answer1');
        await saveShouldBe(page, false, 'after it');
        await send(page, 'getinputfn');
        await saveShouldBe(page, true, 'during GetInput()');
        await send(page, 'answer2');
        await saveShouldBe(page, false, 'after it');
        await send(page, 'askfn');
        await hasOutput(page, '1: Yes');
        await saveShouldBe(page, true, 'during Ask()');
        await clickOption(page, 'Yes');
        await hasOutput(page, 'Ask() got: yes');
        await saveShouldBe(page, false, 'after it');
        await send(page, 'menufn');
        await hasOutput(page, '1: optiona');
        await saveShouldBe(page, true, 'during ShowMenu()');
        await clickOption(page, 'optiona');
        await hasOutput(page, 'ShowMenu() got: optiona');
        await saveShouldBe(page, false, 'after it');
        await send(page, 'finishit');
        await page.waitForTimeout(600);
        await saveShouldBe(page, false, 'after the game has finished');
        await page.close();
    }

    async function restart() {
        const page = await open('/e2e-fixtures/v2-restart-test.aslx');
        const pane = '[data-qv-pane="clues"]';
        await page.waitForSelector(pane, { timeout: 10000 });
        const state = () => page.evaluate(sel => ({
            panes: document.querySelectorAll(sel).length,
            runs: window.__restartTestScriptRuns,
        }), pane);
        const first = await state();
        check(first.panes === 1 && first.runs === 1, 'the game starts with its pane, and its script file has run');
        for (const time of [1, 2]) {
            await page.click('#cmdSave');
            await page.waitForSelector('#qv-saves[open]', { timeout: 10000 });
            await page.click('#qv-saves-restart');
            // The dialog asks first when anything has happened since the last save.
            await page.locator('#qv-saves-confirm-yes, #qv-restarting:not(.hidden)').first().waitFor({ timeout: 5000 });
            if (await page.locator('#qv-saves-confirm-yes').count()) await page.click('#qv-saves-confirm-yes');
            await page.waitForSelector('#qv-restarting', { state: 'visible', timeout: 5000 });
            await page.waitForSelector('#qv-restarting', { state: 'hidden', timeout: 30000 });
            const after = await state();
            check(after.panes === 1, `restart ${time}: the pane is there once`);
            check(after.runs === time + 1, `restart ${time}: the script file ran again`);
        }
        await page.close();
    }

    async function undo() {
        const page = await open('/e2e-fixtures/undo-after-command-test.aslx&ui=v2');
        await hasOutput(page, 'You are in a room.');
        const outputLength = () => page.$eval('qv-output', el => el.innerText.length);
        for (const command of ['first', 'second', 'undo', 'third', 'undo', 'undo', 'undo', 'showcount']) {
            const before = await outputLength();
            await send(page, command);
            await page.waitForFunction(n => document.querySelector('qv-output').innerText.length > n, before, { timeout: 15000 });
            await page.waitForTimeout(300);
        }
        const lines = (await page.$eval('qv-output', el => el.innerText)).split('\n').map(l => l.trim());
        const undoLines = lines.filter(l => l.startsWith('Undo:') || l === 'Nothing to undo!');
        const expected = ['Undo: second', 'Undo: third', 'Undo: first', 'Nothing to undo!'];
        check(JSON.stringify(undoLines) === JSON.stringify(expected), `each turn is undone once (${undoLines.join(', ')})`);
        check(lines.includes('Final count: 0'), 'back to the start');
        await page.close();
    }

    try {
        browser = await chromium.launch();
        await waitAndInput();
        await everySuspension();
        await restart();
        await undo();
        console.log('PASS');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser?.close();
    }
});
