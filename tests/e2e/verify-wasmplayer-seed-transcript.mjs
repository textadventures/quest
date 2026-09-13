// Checks WasmPlayer's replay test hooks, which let a browser run of a walkthrough be diffed
// against the headless quest-e2e-tests harness:
//   ?seed=N        seeds game-script randomness (GetRandomInt etc.) before the game starts
//   ?transcript=1  records the raw text/errors the engine emits, read back with
//                  window.QuestVivaTest.takeTranscript()
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const fixture = '/e2e-fixtures/seed-transcript-test.aslx';

let browser;
try {
    browser = await chromium.launch();

    async function sendCommand(page, command, expectedText) {
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 10000 })
            .catch(() => { throw new Error(`can't send "${command}": player never became ready for a command`); });
        await page.fill('#txtCommand', command);
        await page.press('#txtCommand', 'Enter');
        await page.waitForFunction(
            (text) => document.querySelector('#divOutput').textContent.includes(text),
            expectedText, { timeout: 10000 })
            .catch(() => { throw new Error(`"${command}": output never showed "${expectedText}"`); });
    }

    async function playSession(query) {
        const page = await browser.newPage();
        try {
            await page.goto(`${baseUrl}/?url=${fixture}${query}`);
            await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
            await page.waitForFunction(
                () => document.querySelector('#divOutput')?.textContent.includes('Start roll:'),
                null, { timeout: 30000 });
            await sendCommand(page, 'roll', 'Roll:');
            await sendCommand(page, 'broken', 'Sorry, an error occurred');
            const output = await page.$eval('#divOutput', (el) => el.textContent);
            const rolls = [...output.matchAll(/(?:Start roll|Roll): ([\d ]+)/g)].map((m) => m[1].trim()).join(' | ');
            const transcript = await page.evaluate(() => window.QuestVivaTest.takeTranscript());
            const afterTake = await page.evaluate(() => window.QuestVivaTest.takeTranscript());
            return { rolls, transcript, afterTake };
        } finally {
            await page.close();
        }
    }

    const first = await playSession('&seed=42&transcript=1');
    const second = await playSession('&seed=42&transcript=1');
    const other = await playSession('&seed=43&transcript=1');
    const unseeded = await playSession('');

    if (!/^\d+ \| \d+ \d+$/.test(first.rolls)) {
        throw new Error(`couldn't read the three rolls from the output: "${first.rolls}"`);
    }
    if (first.rolls !== second.rolls) {
        throw new Error(`same seed gave different rolls: "${first.rolls}" vs "${second.rolls}"`);
    }
    console.log(`PASS: seed=42 is reproducible (${first.rolls})`);

    if (first.rolls === other.rolls) {
        throw new Error(`seeds 42 and 43 gave identical rolls: "${first.rolls}"`);
    }
    console.log(`PASS: seed=43 differs (${other.rolls})`);

    const chunks = first.transcript;
    if (JSON.stringify(chunks) !== JSON.stringify(second.transcript)) {
        throw new Error('same seed gave different transcripts');
    }
    const textIndex = (needle) => chunks.findIndex((c) => c.type === 'text' && c.text.includes(needle));
    const startIndex = textIndex(`Start roll: ${first.rolls.split(' | ')[0]}`);
    const rollIndex = textIndex(`Roll: ${first.rolls.split(' | ')[1]}`);
    const errorIndex = chunks.findIndex((c) => c.type === 'error' && c.text.includes('nosuchobject'));
    if (startIndex < 0) throw new Error(`start script output not captured: ${JSON.stringify(chunks)}`);
    if (rollIndex < 0) throw new Error(`"roll" output not captured: ${JSON.stringify(chunks)}`);
    if (errorIndex < 0) throw new Error(`script error not captured as an error chunk: ${JSON.stringify(chunks)}`);
    if (!(startIndex < rollIndex && rollIndex < errorIndex)) {
        throw new Error(`transcript out of order: start=${startIndex} roll=${rollIndex} error=${errorIndex}`);
    }
    if (chunks.some((c) => c.text.includes('Sorry, an error occurred'))) {
        throw new Error('player-side error text leaked into the transcript');
    }
    console.log(`PASS: transcript captured start output, command output and the script error in order (${chunks.length} chunks)`);

    if (first.afterTake.length !== 0) {
        throw new Error(`takeTranscript didn't clear the buffer: ${JSON.stringify(first.afterTake)}`);
    }
    console.log('PASS: takeTranscript clears what it returned');

    if (unseeded.transcript.length !== 0) {
        throw new Error(`transcript captured without ?transcript=: ${JSON.stringify(unseeded.transcript)}`);
    }
    console.log('PASS: nothing captured without ?transcript=');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
