// Ad-hoc manual verification for Text Adventure "Pages" (dialogue trees) — see
// src/Engine/Core/CorePages.aslx and issue #2040. Checks the full player-side loop:
// ShowPage renders the description plus numbered CommandLink options, clicking a
// link (an ordinary command, unlike ShowMenu's callback interception) advances the
// dialogue, typed option numbers work, a non-option command is refused while
// allowCancel is off, a page with no options ends the dialogue, and — the design's
// whole point — the Save button stays enabled mid-dialogue, because the game is
// genuinely idle between choices. Also covers a {page:} link embedded in an
// ordinary object's description (signpost) - clicking it from outside any active
// dialogue must launch that page, not fall through to "I don't understand".
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function waitUntilCanSendCommand() {
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 10000 });
}

async function sendCommand(command) {
    await waitUntilCanSendCommand();
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
    await waitUntilCanSendCommand();
}

async function output() {
    return await page.$eval('#divOutput', el => el.textContent);
}

async function expectInOutput(text, label) {
    const out = await output();
    if (!out.includes(text)) {
        throw new Error(`${label}: expected output to contain ${JSON.stringify(text)}, tail: ${out.slice(-300)}`);
    }
    console.log(`PASS: ${label}`);
}

async function run() {
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/dialogue-pages-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000 });
    console.log('PASS: game booted');

    // {page:} link embedded in an ordinary object's description, clicked from
    // outside any active dialogue.
    await sendCommand('look at signpost');
    await expectInOutput('talk to the guard', 'signpost renders its embedded page link');
    const signpostLink = page.locator('a.cmdlink[data-command="guard_intro"]');
    if (await signpostLink.count() === 0) {
        throw new Error('Expected the signpost link to be a command link (a.cmdlink[data-command="guard_intro"])');
    }
    await signpostLink.first().click();
    await waitUntilCanSendCommand();
    await expectInOutput('The guard eyes you suspiciously.', 'clicking the embedded page link launches the dialogue');
    if ((await output()).includes("don't understand")) {
        throw new Error('Clicking the embedded page link must not fall through to "I don\'t understand your command."');
    }
    console.log('PASS: {page:} link in an ordinary description launches the dialogue when clicked');
    await sendCommand('2'); // end the dialogue (guard_castle has no options) before the rest of the script

    await sendCommand('talk');
    await expectInOutput('The guard eyes you suspiciously.', 'ShowPage prints the page description');
    await expectInOutput('1: Ask about the weather', 'options are numbered');

    const weatherLink = page.locator('a.cmdlink[data-command="guard_weather"]');
    if (await weatherLink.count() === 0) {
        throw new Error('Expected the weather option to be a command link (a.cmdlink[data-command="guard_weather"])');
    }
    console.log('PASS: options render as command links');

    // The whole reason Pages is not built on ShowMenu: mid-dialogue the game is
    // idle, so Save must be enabled.
    const saveDisabled = await page.$eval('#cmdSave', el => el.disabled || el.classList.contains('ui-state-disabled'));
    if (saveDisabled) {
        throw new Error('Save must stay enabled mid-dialogue (the game is idle between choices)');
    }
    console.log('PASS: Save stays enabled mid-dialogue');

    // Click an option link — an ordinary command turn.
    await weatherLink.first().click();
    await waitUntilCanSendCommand();
    await expectInOutput('Nice weather today.', 'clicking an option link advances to its page');

    // The old page's option links are hidden with their output section (the
    // HideOutputSection call lands just after canSendCommand flips, so poll).
    await page.waitForFunction(() => {
        const link = document.querySelector('a.cmdlink[data-command="guard_castle"]');
        return !link || link.offsetParent === null;
    }, { timeout: 5000 });
    console.log('PASS: previous page option links are hidden after a choice');

    // Typed option number.
    await sendCommand('1');
    await expectInOutput('2: Ask about the castle', 'typed option number returns to the intro page');

    // Non-option command with allowCancel off: refused, not executed.
    await sendCommand('marker');
    await expectInOutput('Please choose one of the options above.', 'non-option command prompts to choose an option');
    if ((await output()).includes('marker ran')) {
        throw new Error('The non-option command must not run while cancel is forbidden');
    }
    console.log('PASS: non-option command did not execute mid-dialogue');

    // A page with no options ends the dialogue; ordinary commands work again.
    await sendCommand('2');
    await expectInOutput('The castle is closed.', 'terminal page renders');
    await sendCommand('marker');
    await expectInOutput('marker ran', 'ordinary commands work again after the dialogue ends');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-dialogue-pages-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
