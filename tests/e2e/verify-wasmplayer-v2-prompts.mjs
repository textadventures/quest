// Checks player UI v2's prompts (#2595) under ?ui=v2:
//
// - fixtures/v2-prompts-test.aslx (610): wait shows a Continue button in place of the input,
//   continued by the button, any key (not Tab) or, with setContinueOnClick, a click anywhere;
//   chained waits never show the input in between; pause hides the command bar until it ends;
//   get input takes the next command; and show menu/ask are answered through their inline
//   numbered links, by clicking or typing the number.
// - fixtures/v2-dialogs-test.aslx (580): before 600, show menu and ask open a modal <dialog>
//   with the "menu" part, which can be cancelled only when the menu allows it.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';
const continueButton = 'qv-command-bar .qv-continue';

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(fixture, startText) {
        const page = await browser.newPage();
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}&ui=v2`);
        await page.waitForFunction(
            (text) => !document.getElementById('qv-start') && document.querySelector('qv-output')?.textContent.includes(text),
            startText, { timeout: 30000 })
            .catch(() => { throw new Error(`${fixture}: the game never started on v2`); });
        return { page, errors };
    }

    function helpers(page) {
        const outputText = () => page.evaluate(() => document.querySelector('qv-output').textContent);
        async function waitForText(text, what, timeout = 10000) {
            await page.waitForFunction((t) => document.querySelector('qv-output').textContent.includes(t), text, { timeout })
                .catch(() => { throw new Error(`${what}: "${text}" never appeared`); });
        }
        async function type(command) {
            await page.fill(input, command);
            await page.press(input, 'Enter');
        }
        const bar = () => page.evaluate(() => {
            const commandBar = document.querySelector('qv-command-bar');
            const form = commandBar.querySelector('form');
            const button = commandBar.querySelector('.qv-continue');
            return {
                input: !form.hidden && getComputedStyle(commandBar).visibility === 'visible',
                continue: !button.hidden && getComputedStyle(commandBar).visibility === 'visible',
                continueFocused: document.activeElement === button,
                inputFocused: document.activeElement === form.querySelector('input'),
                label: button.textContent,
                held: document.querySelector('qv-player').hasAttribute('data-qv-input-held'),
            };
        });
        // Resolves once the input is back after a prompt.
        const inputBack = () => page.waitForFunction(() => {
            const form = document.querySelector('qv-command-bar form');
            return window.canSendCommand && !form.hidden && !document.querySelector('qv-player').hasAttribute('data-qv-input-held');
        }, null, { timeout: 10000 });
        return { outputText, waitForText, type, bar, inputBack };
    }

    // ── A 610 game ──────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-prompts-test.aslx', 'You are in');
        const { outputText, waitForText, type, bar, inputBack } = helpers(page);

        await type('waitonce');
        await waitForText('Before the wait.', 'wait');
        await page.waitForFunction(() => !document.querySelector('qv-command-bar .qv-continue').hidden, null, { timeout: 10000 });
        let state = await bar();
        check(state.continue && !state.input, 'a wait shows the Continue button in place of the input');
        check(state.continueFocused, 'the Continue button has focus');
        check(state.label.trim() !== '', `the button uses the game's ContinueLabel ("${state.label}")`);
        await page.keyboard.press('Tab');
        await page.waitForTimeout(300);
        check(!(await outputText()).includes('After the wait.'), "Tab doesn't continue a wait");
        await page.focus(continueButton);
        await page.keyboard.press('x');
        await waitForText('After the wait.', 'a key press during the wait');
        await inputBack();
        state = await bar();
        check(state.input && !state.continue && state.inputFocused, 'any key continues, and the input comes back with focus');

        // Chained waits: the input must not appear between them.
        await type('waittwice');
        await waitForText('First page.', 'the first of two waits');
        await page.waitForFunction(() => !document.querySelector('qv-command-bar .qv-continue').hidden, null, { timeout: 10000 });
        await page.evaluate(() => {
            window.inputShownDuringWaits = false;
            const form = document.querySelector('qv-command-bar form');
            window.waitObserver = new MutationObserver(() => {
                if (!form.hidden) window.inputShownDuringWaits = true;
            });
            window.waitObserver.observe(form, { attributes: true, attributeFilter: ['hidden'] });
        });
        await page.click(continueButton);
        await waitForText('Second page.', 'the second wait');
        await page.waitForFunction(() => !document.querySelector('qv-command-bar .qv-continue').hidden, null, { timeout: 10000 });
        const flashed = await page.evaluate(() => {
            window.waitObserver.disconnect();
            return window.inputShownDuringWaits;
        });
        check(!flashed, "the input doesn't appear between chained waits");
        await page.click(continueButton);
        await waitForText('Third page.', 'after both waits');
        await inputBack();
        check((await bar()).input, 'the input comes back after the last of them');

        // Pause
        await type('pausecmd');
        await waitForText('Before the pause.', 'pause');
        state = await bar();
        const during = await outputText();
        check(state.held && !during.includes('After the pause.'), 'a pause hides the command bar and holds the turn');
        await waitForText('After the pause.', 'the end of the pause', 5000);
        await inputBack();
        check((await bar()).inputFocused, 'the command bar comes back with focus once the pause is over');

        // Continue on click
        await type('clickwait');
        await waitForText('Click anywhere.', 'continue on click');
        await page.waitForFunction(() => !document.querySelector('qv-command-bar .qv-continue').hidden, null, { timeout: 10000 });
        await page.click('qv-output', { position: { x: 5, y: 5 } });
        await waitForText('Clicked through.', 'a click in the output with setContinueOnClick(true)');
        await inputBack();
        console.log('PASS: setContinueOnClick lets a click anywhere continue a wait');

        // get input
        await type('name');
        await waitForText('What is your name?', 'get input');
        await inputBack();
        await type('Ada');
        await waitForText('Hello, Ada.', 'answering get input');
        console.log('PASS: get input takes the next thing typed');

        // An inline menu, answered by clicking and by typing its number.
        await type('menu');
        await waitForText('Pick a fruit', 'show menu');
        await inputBack();
        await page.click('qv-output a.commandlink:text-is("Banana")');
        await waitForText('You picked Banana.', 'clicking a menu option');
        await page.waitForFunction(() => !document.querySelector('qv-output').textContent.includes('Cherry'), null, { timeout: 5000 })
            .catch(() => { throw new Error("the menu's options were left on screen after it was answered"); });
        console.log("PASS: clicking an inline menu option answers it, and the options go");
        await inputBack();
        await type('menu');
        await waitForText('Pick a fruit', 'show menu again');
        await inputBack();
        await type('3');
        await waitForText('You picked Cherry.', 'typing a menu option number');
        console.log('PASS: typing an option number answers an inline menu');

        await inputBack();
        await type('question');
        await waitForText('Are you sure?', 'ask');
        await inputBack();
        await page.click('qv-output a.commandlink:text-is("Yes")');
        await waitForText('You said yes.', 'answering ask');
        console.log('PASS: an inline question is answered through its links');

        if (errors.length) throw new Error(`v2-prompts-test.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Dialogs, before 600 ─────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-dialogs-test.aslx', 'You are in');
        const { waitForText, type, inputBack } = helpers(page);
        const dialog = () => page.evaluate(() => {
            const d = document.querySelector('dialog.qv-dialog');
            return d && {
                open: d.open,
                modal: d.matches(':modal'),
                part: d.dataset.qvPart,
                caption: d.querySelector('.qv-dialog-caption').textContent,
                choices: [...d.querySelectorAll('.qv-dialog-choices button')].map((b) => b.textContent),
                cancel: !!d.querySelector('.qv-dialog-cancel'),
                focused: document.activeElement?.textContent ?? '',
                label: d.getAttribute('aria-labelledby') && document.getElementById(d.getAttribute('aria-labelledby'))?.textContent,
            };
        });
        const dialogOpen = () => page.waitForFunction(() => document.querySelector('dialog.qv-dialog')?.open, null, { timeout: 10000 })
            .catch(() => { throw new Error('no dialog opened'); });
        const dialogGone = () => page.waitForFunction(() => !document.querySelector('dialog.qv-dialog'), null, { timeout: 5000 })
            .catch(() => { throw new Error('the dialog stayed open'); });

        await type('menu');
        await dialogOpen();
        let d = await dialog();
        check(d.modal && d.part === 'menu', 'show menu before 600 opens a modal <dialog> with the "menu" part');
        check(d.caption === 'Pick a fruit' && d.label === 'Pick a fruit', 'the dialog is labelled by its caption');
        check(d.choices.join('/') === 'Apple/Banana/Cherry' && d.cancel, 'it lists the options, with Cancel when the menu allows it');
        check(d.focused === 'Apple', 'focus starts on the first option');
        await page.click('dialog.qv-dialog button:text-is("Banana")');
        await dialogGone();
        await waitForText('You picked Banana.', 'choosing from the dialog');
        console.log('PASS: choosing an option closes the dialog and answers the menu');

        await inputBack();
        await type('menu');
        await dialogOpen();
        await page.keyboard.press('Escape');
        await dialogGone();
        // The engine runs the callback with no result for a cancelled menu.
        await waitForText('You picked .', 'cancelling the menu');
        await inputBack();
        console.log('PASS: Escape cancels a menu that allows it');
        await type('look');
        await waitForText('> look', 'a command after cancelling');
        console.log('PASS: the game carries on after a cancelled menu');

        await inputBack();
        await type('mustpick');
        await dialogOpen();
        check(!(await dialog()).cancel, 'a menu that must be answered has no Cancel');
        await page.keyboard.press('Escape');
        await page.waitForTimeout(300);
        check((await dialog())?.open === true, "Escape doesn't close a menu that must be answered");
        await page.click('dialog.qv-dialog button:text-is("Red")');
        await dialogGone();
        await waitForText('You picked Red.', 'answering a menu that must be answered');

        await inputBack();
        await type('question');
        await dialogOpen();
        d = await dialog();
        check(d.caption === 'Are you sure?' && d.choices.join('/') === 'Yes/No' && !d.cancel, 'ask before 600 opens a Yes/No dialog');
        await page.click('dialog.qv-dialog button:text-is("No")');
        await dialogGone();
        await waitForText('You said no.', 'answering the question');
        console.log('PASS: answering the question dialog sends the answer');

        if (errors.length) throw new Error(`v2-dialogs-test.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
