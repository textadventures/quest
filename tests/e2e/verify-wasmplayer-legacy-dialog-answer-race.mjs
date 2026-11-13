// Legacy UI (games before ASL 600): a menu or question dialog's answer reaches the engine 100ms
// after the dialog closes. A command sent in that gap used to reach the engine first, and if it
// asked something of its own, the delayed answer resolved that prompt instead (#2612). Commands
// are now refused from when a dialog opens until the answer's turn has finished.
//
// Each check answers a dialog and submits a command in the same tick, as a script or a very fast
// typist could.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
const pageErrors = [];
page.on('pageerror', err => { pageErrors.push(err.message); console.log('[pageerror]', err.message); });

const output = () => page.$eval('#divOutput', el => el.textContent);
const commandText = () => page.$eval('#txtCommand', el => el.value);
// jQuery UI throws if a dialog that has never been opened is asked whether it's open.
const isOpen = (selector) => page.evaluate(s => !!$(s).dialog('instance') && $(s).dialog('isOpen'), selector);
const waitForOpen = (selector) => page.waitForFunction(
    s => !!$(s).dialog('instance') && $(s).dialog('isOpen'), selector, { timeout: 10000 });

async function waitForOutput(text, what) {
    try {
        await page.waitForFunction(
            t => document.querySelector('#divOutput').textContent.includes(t), text, { timeout: 10000 });
    } catch {
        throw new Error(`${what}: expected the output to contain "${text}", got tail: ${(await output()).slice(-200)}`);
    }
}

async function enter(command) {
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
}

// Clicks the dialog button, then types a command and submits it, all in one tick.
async function answerAndSubmit(buttonText, command) {
    await page.evaluate(([buttonText, command]) => {
        const button = [...document.querySelectorAll('.ui-dialog:not([style*="display: none"]) .ui-dialog-buttonpane button')]
            .find(b => b.textContent === buttonText);
        if (!button) throw new Error(`No visible dialog button "${buttonText}"`);
        button.click();
        $('#txtCommand').val(command);
        runCommand();
    }, [buttonText, command]);
}

async function run() {
    await page.goto(`${baseUrl}/?url=/examples/test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000 });
    console.log('PASS: game booted');

    // 1. Cancel a menu, and in the same tick ask for another one. Before the fix, the command
    // opened the second menu and the delayed cancel then cancelled that one.
    await enter('menufn');
    await waitForOpen('#dialog');
    const before = (await output()).split('You chose:').length;
    await answerAndSubmit('Cancel', 'menufn');
    await page.waitForFunction(
        n => document.querySelector('#divOutput').textContent.split('You chose:').length > n, before, { timeout: 10000 });
    await page.waitForTimeout(500);
    if (await isOpen('#dialog')) {
        throw new Error('A second menu opened: the command sent while the cancel was pending reached the engine');
    }
    if ((await output()).split('You chose:').length !== before + 1) {
        throw new Error(`Expected exactly one "You chose:" for the cancelled menu, got tail: ${(await output()).slice(-200)}`);
    }
    if (await commandText() !== 'menufn') {
        throw new Error(`Expected the refused command to stay in the command bar, got ${JSON.stringify(await commandText())}`);
    }
    console.log('PASS: a command submitted with a menu cancel is refused, and stays in the command bar');

    // The command bar works again once the answer's turn has finished.
    await page.press('#txtCommand', 'Enter');
    await waitForOpen('#dialog');
    await answerAndSubmit('Select', 'look');
    await waitForOutput('You chose: One', 'selecting from the menu');
    await page.waitForTimeout(500);
    if (await commandText() !== 'look') {
        throw new Error(`Expected the command submitted with Select to be refused, got ${JSON.stringify(await commandText())}`);
    }
    console.log('PASS: the command bar works again after the turn, and a command submitted with Select is refused');

    // 2. Two questions in a row: answering the first opens the second during the first answer's
    // turn. That turn finishing mustn't unblock commands while the second is still waiting.
    await enter('ask');
    await waitForOpen('#msgbox');
    await answerAndSubmit('Yes', 'look');
    await waitForOutput('Response 1: yes', 'answering the first question');
    await waitForOpen('#msgbox');
    await page.waitForTimeout(300);
    if (!(await page.evaluate(() => _dialogPending))) {
        throw new Error('Commands were unblocked while the second question was still open');
    }
    console.log('PASS: commands stay blocked while the next question is open');

    await answerAndSubmit('No', 'look');
    await waitForOutput('Response 2: no', 'answering the second question');
    await page.waitForTimeout(500);
    if (await commandText() !== 'look') {
        throw new Error(`Expected the command submitted with No to be refused, got ${JSON.stringify(await commandText())}`);
    }
    console.log('PASS: a command submitted with a question answer is refused');

    const length = (await output()).length;
    await page.press('#txtCommand', 'Enter');
    await page.waitForFunction(
        n => document.querySelector('#txtCommand').value === ''
            && document.querySelector('#divOutput').textContent.length > n, length, { timeout: 10000 });
    console.log('PASS: the command bar works again after both questions');

    if (pageErrors.length > 0) {
        throw new Error(`Page errors: ${pageErrors.join('; ')}`);
    }
    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-legacy-dialog-answer-race-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
