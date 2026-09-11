// Regression check for a user report against Xanadu - In the Compound - Revenge: after
// `focus on light`, WasmPlayer printed the response but never scrolled, and wouldn't accept
// another command - with nothing in the console.
//
// The game's inlined (Quest 5.6-era) SetBackgroundColour calls `request (Background,
// "LightGray")`. playercore.js's colourNameToHex table only had "lightgrey", so hexToRgb
// returned null and setBackground threw dereferencing it. Quest 5 threw too, but ran each JS
// call on its own so nothing else was affected; WebPlayer's runJs wraps each call in
// try/catch. WasmPlayerBridge.FlushBuffer ran the whole buffered turn in one unguarded loop,
// so the throw skipped every UI call queued after it (scroll, timer request) and escaped into
// the fire-and-forget turn task, where it was silently dropped.
//
// Covers both fixes: the missing CSS grey/gray spellings (plus a non-hex colour falling back
// to the raw CSS value), and FlushBuffer isolating a UI function that genuinely throws.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
let browser;
try {
    browser = await chromium.launch();
    const page = await browser.newPage();
    const consoleErrors = [];
    page.on('console', (msg) => {
        if (msg.type() === 'error') consoleErrors.push(msg.text());
    });
    page.on('pageerror', (err) => consoleErrors.push(String(err)));

    async function sendCommand(command, expectedText) {
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 10000 })
            .catch(() => { throw new Error(`can't send "${command}": player never became ready for a command`); });
        await page.fill('#txtCommand', command);
        await page.press('#txtCommand', 'Enter');
        await page.waitForFunction(
            (text) => document.querySelector('#divOutput').textContent.includes(text),
            expectedText, { timeout: 10000 })
            .catch(() => { throw new Error(`"${command}": output never showed "${expectedText}"`); });
        console.log(`PASS: ${command} -> "${expectedText}"`);
    }

    const gameBorderBackground = () =>
        page.$eval('#gameBorder', (el) => getComputedStyle(el).backgroundColor);

    await page.goto(`${baseUrl}/?url=/e2e-fixtures/ui-call-throws-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });

    await sendCommand('lightgray', 'Background is now light gray.');
    await sendCommand('ping', 'pong');
    const lightGray = await gameBorderBackground();
    if (lightGray !== 'rgb(211, 211, 211)') {
        throw new Error(`LightGray not applied to #gameBorder: got ${lightGray}`);
    }
    console.log(`PASS: LightGray applied (${lightGray})`);

    await sendCommand('rgbbackground', 'Background is now rgb.');
    const rgb = await gameBorderBackground();
    if (rgb !== 'rgb(10, 20, 30)') {
        throw new Error(`rgb() background not applied to #gameBorder: got ${rgb}`);
    }
    console.log(`PASS: rgb() background applied (${rgb})`);

    const errorsBeforeThrow = consoleErrors.length;
    await sendCommand('throwui', 'Still accepting commands after a throwing UI call.');
    await sendCommand('ping', 'pong');
    if (!consoleErrors.slice(errorsBeforeThrow).some((e) => e.includes('ui-call-throws-test'))) {
        throw new Error(`throwing UI call wasn't reported to the console: ${JSON.stringify(consoleErrors)}`);
    }
    console.log('PASS: throwing UI call reported via console.error');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
