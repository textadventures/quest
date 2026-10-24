// Checks saving and restoring a game on player UI v2 (#2595), playing
// fixtures/v2-prompts-test.aslx (610) under ?ui=v2:
//
// - The top bar has the host's Save / Load button, which is disabled while the engine is
//   suspended mid-turn (a wait) and enabled again between turns.
// - Saving through the host's Save/Load dialog captures v2's transcript, and loading it back
//   restores that transcript on a fresh v2 UI: what was played before the save is there, what
//   came after isn't, the view is at the end of it, and the game carries on.
// - Leaving the page with unsaved progress asks first; straight after a save it doesn't.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();
    const page = await browser.newPage({ viewport: { width: 800, height: 400 } });
    const errors = [];
    page.on('pageerror', (err) => errors.push(String(err)));
    page.on('console', (msg) => {
        if (msg.type() === 'error') errors.push(msg.text());
    });

    // A fresh profile, so the only save is the one made here.
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-prompts-test.aslx&ui=v2`);
    await page.waitForFunction(
        () => !document.getElementById('qv-start') && document.querySelector('qv-output')?.textContent.includes('You are in'),
        null, { timeout: 30000 })
        .catch(() => { throw new Error('the game never started on v2'); });

    const outputText = () => page.evaluate(() => document.querySelector('qv-output').textContent);
    async function waitForText(text, what) {
        await page.waitForFunction((t) => document.querySelector('qv-output').textContent.includes(t), text, { timeout: 10000 })
            .catch(() => { throw new Error(`${what}: "${text}" never appeared`); });
    }
    const inputBack = () => page.waitForFunction(() => window.canSendCommand
        && !document.querySelector('qv-command-bar form').hidden
        && !document.querySelector('qv-player').hasAttribute('data-qv-input-held'), null, { timeout: 10000 });
    async function type(command) {
        await inputBack();
        await page.fill(input, command);
        await page.press(input, 'Enter');
    }
    const saveButton = () => page.evaluate(() => {
        const button = document.querySelector('qv-player #cmdSave');
        return button && { visible: button.offsetWidth > 0, disabled: button.disabled, label: button.textContent };
    });

    // ── The Save button ─────────────────────────────────────────────────────

    await page.waitForFunction(() => !document.querySelector('#cmdSave').disabled, null, { timeout: 10000 })
        .catch(() => { throw new Error('the Save button never became enabled after the game started'); });
    let save = await saveButton();
    check(save.visible && save.label === 'Save / Load', 'the top bar has the Save / Load button');

    await type('waitonce');
    await waitForText('Before the wait.', 'wait');
    await page.waitForFunction(() => !document.querySelector('qv-command-bar .qv-continue').hidden, null, { timeout: 10000 });
    check((await saveButton()).disabled, 'Save is disabled while the engine waits mid-turn');
    await page.click('qv-command-bar .qv-continue');
    await waitForText('After the wait.', 'continuing');
    await page.waitForFunction(() => !document.querySelector('#cmdSave').disabled, null, { timeout: 10000 })
        .catch(() => { throw new Error('Save stayed disabled after the wait'); });
    console.log('PASS: Save is enabled again once the turn is over');

    await type('menu');
    await waitForText('Pick a fruit', 'menu');
    await inputBack();
    await page.click('qv-output a.commandlink:text-is("Banana")');
    await waitForText('You picked Banana.', 'choosing from the menu');

    // ── Save, play on, then load ────────────────────────────────────────────

    await inputBack();
    await page.click('#cmdSave');
    await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
        .catch(() => { throw new Error("Save / Load didn't open the host's dialog"); });
    await page.click('#qv-saves-save-new');
    await waitForText('Game saved.', 'saving');
    console.log("PASS: saving through the host's dialog works on v2");
    await page.evaluate(() => document.getElementById('qv-saves')?.close());

    // Leaving straight after a save doesn't ask.
    const askedAfterSave = await page.evaluate(() => {
        const e = new Event('beforeunload', { cancelable: true });
        window.dispatchEvent(e);
        return e.defaultPrevented;
    });
    check(!askedAfterSave, "leaving straight after a save doesn't ask first");

    await type('question');
    await waitForText('Are you sure?', 'a question after saving');
    await inputBack();
    await page.click('qv-output a.commandlink:text-is("No")');
    await waitForText('You said no.', 'answering after saving');
    const askedWithProgress = await page.evaluate(() => {
        const e = new Event('beforeunload', { cancelable: true });
        window.dispatchEvent(e);
        return e.defaultPrevented;
    });
    check(askedWithProgress, 'leaving with unsaved progress asks first');

    await inputBack();
    await page.click('#cmdSave');
    await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
        .catch(() => { throw new Error('the save never appeared in the Save/Load list'); });
    await page.click('#qv-saves-list [data-slot]');
    await page.waitForFunction(() => {
        const text = document.querySelector('qv-output')?.textContent ?? '';
        return text.includes('You picked Banana.') && !text.includes('You said no.') && window.canSendCommand;
    }, null, { timeout: 30000 })
        .catch(() => { throw new Error("loading didn't bring back the transcript as it was saved"); });
    console.log('PASS: loading restores the transcript as it was when saved');

    const restored = await page.evaluate(() => ({
        ui: Object.keys(window.QuestPlayerUis).join(','),
        jquery: typeof window.jQuery,
        atEnd: Math.abs(window.scrollY + window.innerHeight - document.documentElement.scrollHeight) < 4,
        links: [...document.querySelectorAll('qv-output a.cmdlink')].every((a) => a.getAttribute('role') === 'button'),
    }));
    check(restored.ui === 'v2' && restored.jquery === 'undefined', 'the restored game is still on v2');
    check(restored.atEnd, 'the view is at the end of the restored transcript');
    check(restored.links, 'links in the restored transcript are set up as buttons');

    await type('waitonce');
    await waitForText('Before the wait.', 'playing on after loading');
    await page.click('qv-command-bar .qv-continue');
    const after = await page.waitForFunction(() => document.querySelector('qv-output').textContent.split('After the wait.').length === 3,
        null, { timeout: 10000 }).then(() => true, () => false);
    check(after, 'the restored game carries on, with new output after the restored transcript');
    check(!(await outputText()).includes('You said no.'), 'nothing from after the save came back');

    if (errors.length) throw new Error(`page errors:\n  ${errors.join('\n  ')}`);
    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
