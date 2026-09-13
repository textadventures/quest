// Ad-hoc manual verification: Ask()/ShowMenu() and the 'ask'/'show menu' script
// commands used to open a jQuery UI modal for every game. For v600+ games the
// engine now draws them as numbered links in the transcript instead, so the
// synchronous forms look the same as the callback forms in CoreFunctions.aslx
// (https://github.com/alexwarren/quest/issues/2287). Clicking a link and typing
// its number are the same path - the link sends its number as a command.
// Games below v600 (and Quest 4 games, which have no WorldModel version at all)
// keep the dialog, which examples/test.aslx - version 530 - still covers.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
const pageErrors = [];
page.on('pageerror', err => { pageErrors.push(err.message); console.log('[pageerror]', err.message); });

const output = () => page.$eval('#divOutput', el => el.textContent);

// Note there is no "> command" echo to wait on for a menu response: the prompt
// intercepts the command before HandleCommand echoes it, exactly as CoreParser.aslx
// does for the callback forms. So this just sends, and the assertions below do the
// waiting - generously, since the engine is on the one WASM thread and the first
// turn after boot is much slower than the rest.
async function send(command) {
    // runCommand() silently drops the keystroke if a turn is still in flight
    // (canSendCommand), so wait for the engine to come back before typing.
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
    await page.waitForTimeout(300);
}

async function waitForText(label, text) {
    try {
        await page.waitForFunction(
            expected => document.getElementById('divOutput').textContent.includes(expected),
            text, { timeout: 15000 });
    } catch {
        throw new Error(`${label}: timed out waiting for "${text}", got tail: ${(await output()).slice(-200)}`);
    }
}

// The menu links are ordinary transcript links, so "no dialog" means jQuery UI
// never created its wrapper around #dialog/#msgbox.
async function assertNoDialog(label) {
    const visible = await page.evaluate(() => {
        const dialogs = [...document.querySelectorAll('.ui-dialog')];
        return dialogs.filter(d => d.offsetParent !== null).length;
    });
    if (visible !== 0) throw new Error(`${label}: expected no jQuery UI dialog, found ${visible}`);
}

// The command echo lands before the rest of the turn's output, so every positive
// assertion waits rather than reading straight away.
const assertContains = waitForText;

async function assertNotContains(label, text) {
    // Only meaningful once the turn has actually had a chance to produce output.
    await page.waitForTimeout(500);
    const actual = await output();
    if (actual.includes(text)) {
        throw new Error(`${label}: expected output NOT to contain "${text}", got tail: ${actual.slice(-200)}`);
    }
}

async function run() {
    await page.goto(`${baseUrl}/?url=/examples/sync-and-async/v600.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000 });
    console.log('PASS: v600 game booted');

    // ── ShowMenu() expression form ───────────────────────────────────────────
    await send('menufn');
    await assertNoDialog('ShowMenu()');
    await assertContains('ShowMenu()', 'What is your favourite colour?');
    for (const [n, colour] of [[1, 'Red'], [2, 'Green'], [3, 'Blue'], [4, 'Yellow']]) {
        await assertContains('ShowMenu()', `${n}: ${colour}`);
    }
    console.log('PASS: ShowMenu() rendered four numbered options inline, no dialog');

    // Each option is a link that sends its own number as a command.
    const linkCount = await page.locator('#divOutput a.commandlink').count();
    if (linkCount !== 4) throw new Error(`Expected 4 option links, found ${linkCount}`);
    await page.locator('#divOutput a.commandlink').nth(2).click();
    await assertContains('ShowMenu() click', 'You chose Blue');
    console.log('PASS: clicking an option link resolved ShowMenu()');

    // The links are taken away once the choice is made (HideOutputSection fades the
    // section out before removing it).
    await page.waitForTimeout(800);
    const remaining = await page.locator('#divOutput a.commandlink:visible').count();
    if (remaining !== 0) throw new Error(`Expected the option links to be removed, ${remaining} still visible`);
    console.log('PASS: option links removed after choosing');

    // ── Typing the option number ─────────────────────────────────────────────
    await send('menufn');
    await send('2');
    await assertContains('ShowMenu() typed', 'You chose Green');
    console.log('PASS: typing an option number resolved ShowMenu()');

    // ── Not cancellable: other input is ignored ──────────────────────────────
    await send('menufn');
    await send('xyzzy');
    await assertNotContains('ShowMenu() uncancellable', "I don't understand your command");
    await send('1');
    await assertContains('ShowMenu() uncancellable', 'You chose Red');
    console.log('PASS: a non-option command was ignored while an uncancellable menu was open');

    // ── Cancellable: other input dismisses the menu ──────────────────────────
    await send('menufncancel');
    await send('xyzzy');
    await assertContains('ShowMenu() cancellable', "You didn't choose.");
    console.log('PASS: a non-option command cancelled a cancellable menu');

    // ── Ask() expression form ────────────────────────────────────────────────
    await send('askfn');
    await assertNoDialog('Ask()');
    await assertContains('Ask()', 'Buy dodgy watch?');
    await assertContains('Ask()', '1: Yes');
    await assertContains('Ask()', '2: No');
    await send('1');
    await assertContains('Ask()', 'You hand over $50.');
    console.log('PASS: Ask() rendered Yes/No inline and resolved from a typed number');

    // ── 'show menu' script command ───────────────────────────────────────────
    await send('menucmd');
    await assertNoDialog('show menu');
    await assertContains('show menu', '1: Red');
    await send('4');
    await assertContains('show menu', 'You chose Yellow');
    console.log("PASS: the 'show menu' script command rendered inline too");

    // ── A pre-v600 game still gets the dialog ────────────────────────────────
    await page.goto(`${baseUrl}/?url=/examples/test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000 });
    await send('menufn');
    await page.waitForSelector('#dialogOptions option', { timeout: 10000 });
    const dialogVisible = await page.evaluate(
        () => [...document.querySelectorAll('.ui-dialog')].filter(d => d.offsetParent !== null).length);
    if (dialogVisible !== 1) {
        throw new Error(`v530 game: expected the ShowMenu dialog, found ${dialogVisible} visible dialogs`);
    }
    console.log('PASS: a v530 game still opens the ShowMenu dialog');

    if (pageErrors.length > 0) throw new Error(`Page errors: ${pageErrors.join('; ')}`);
    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
