// Checks WasmPlayer's walkthrough-replay hooks for expressions and game time:
//   window.QuestVivaTest.evaluate(expr) / assert(expr)   evaluate against the running game
//   ?clock=manual                                         stops the real 1-second timer interval
//   window.QuestVivaTest.tick(n) / fireNextTimeout()      advance game time explicitly instead
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const fixture = '/e2e-fixtures/manual-clock-eval-test.aslx';

let browser;
try {
    browser = await chromium.launch();

    const outputText = (page) => page.$eval('#divOutput', (el) => el.textContent);

    async function openGame(query) {
        const page = await browser.newPage();
        await page.goto(`${baseUrl}/?url=${fixture}${query}`);
        await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
        await page.waitForFunction(() => window.QuestVivaTest && window.canSendCommand === true, null, { timeout: 30000 });
        return page;
    }

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

    const page = await openGame('&clock=manual');

    const evaluations = [
        ['game.counter + 1', '8'],
        ['"a" + "b"', 'ab'],
        ['player.parent.name', 'room'],
    ];
    for (const [expression, expected] of evaluations) {
        const actual = await page.evaluate((e) => window.QuestVivaTest.evaluate(e), expression);
        if (actual !== expected) throw new Error(`evaluate(${expression}) returned "${actual}", expected "${expected}"`);
    }
    console.log('PASS: evaluate returns expression values as strings');

    if (await page.evaluate(() => window.QuestVivaTest.assert('game.counter = 7')) !== true) {
        throw new Error('assert(game.counter = 7) was not true');
    }
    if (await page.evaluate(() => window.QuestVivaTest.assert('game.counter = 8')) !== false) {
        throw new Error('assert(game.counter = 8) was not false');
    }
    console.log('PASS: assert returns true/false');

    const badExpression = await page.evaluate(
        () => window.QuestVivaTest.evaluate('nosuchobject.nosuchattribute').then(() => 'resolved', (e) => `rejected: ${e.message}`));
    if (!badExpression.startsWith('rejected')) {
        throw new Error(`evaluating an invalid expression should reject, got ${badExpression}`);
    }
    console.log('PASS: an invalid expression rejects');

    await sendCommand(page, 'delayed', 'Timeout set.');
    await page.waitForTimeout(3500);
    if ((await outputText(page)).includes('Timeout fired.')) {
        throw new Error('SetTimeout(2) fired in real time under ?clock=manual');
    }
    console.log('PASS: SetTimeout does not fire in real time under ?clock=manual');

    const fired = [];
    for (let i = 0; i < 5; i++) {
        const didFire = await page.evaluate(() => window.QuestVivaTest.fireNextTimeout());
        if (!didFire) break;
        fired.push((await outputText(page)).match(/Second timeout fired\.|Timeout fired\./g)?.length ?? 0);
    }
    if (JSON.stringify(fired) !== '[1,2]') {
        throw new Error(`fireNextTimeout should fire the timeout, then the one it scheduled, then report none left; got ${JSON.stringify(fired)}`);
    }
    const afterTimeouts = await outputText(page);
    if (afterTimeouts.indexOf('Timeout fired.') > afterTimeouts.indexOf('Second timeout fired.')) {
        throw new Error('timeouts fired out of order');
    }
    console.log('PASS: fireNextTimeout fires chained SetTimeouts in order, then returns false');

    await sendCommand(page, 'start beeper', 'Beeper started.');
    await page.waitForTimeout(3000);
    if ((await outputText(page)).includes('Beep 1')) {
        throw new Error('a 2-second timer fired in real time under ?clock=manual');
    }
    if (await page.evaluate(() => window.QuestVivaTest.fireNextTimeout()) !== false) {
        throw new Error('fireNextTimeout fired an authored (non-SetTimeout) timer');
    }
    await page.evaluate(() => window.QuestVivaTest.tick(1));
    if ((await outputText(page)).includes('Beep 1')) {
        throw new Error('tick(1) fired a 2-second timer');
    }
    await page.evaluate(() => window.QuestVivaTest.tick(1));
    await page.waitForFunction(() => document.querySelector('#divOutput').textContent.includes('Beep 1'), null, { timeout: 5000 })
        .catch(() => { throw new Error('two tick(1) calls never fired the 2-second timer'); });
    const beeps = await page.evaluate(() => window.QuestVivaTest.evaluate('game.beeps'));
    if (beeps !== '1') throw new Error(`expected game.beeps = 1 after 2 seconds of ticks, got ${beeps}`);
    console.log('PASS: authored timers only advance through tick');
    await page.close();

    const realTime = await openGame('');
    await sendCommand(realTime, 'delayed', 'Timeout set.');
    await realTime.waitForFunction(() => document.querySelector('#divOutput').textContent.includes('Timeout fired.'), null, { timeout: 8000 })
        .catch(() => { throw new Error('without ?clock=manual, SetTimeout(2) never fired in real time'); });
    console.log('PASS: without ?clock=manual, timers still run in real time');
    await realTime.close();
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
