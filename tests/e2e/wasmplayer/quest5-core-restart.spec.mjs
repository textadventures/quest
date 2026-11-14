// Quest 5's Core `restart` command (inlined into every game saved with Quest
// 5 — 4,941 games on textadventures.co.uk) runs, after the player answers yes:
//   if(webPlayer){window.location.reload();}
//   else if (typeof(RestartGame) != 'undefined'){RestartGame();}
//   else{addTextAndScroll('Try pressing CTRL+R.')};
// WasmPlayer's webPlayer is false, and until it defined a global RestartGame()
// those games just printed "Try pressing CTRL+R." instead of restarting.
// RestartGame() must go through the in-app restart (same as "Start from the
// beginning"), not a page reload, which would lose a game handed over from a
// file picker, the editor preview or the Electron app — so this also checks
// the page itself survived the restart. Current Core's restart (a page
// reload until RestartGame() existed) now prefers RestartGame() too, so the
// same checks run against a game using it.
// Requires the WasmPlayer dev server running locally:
//   node ../../src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('quest5-core-restart', async ({ baseURL }) => {
    const baseUrl = baseURL;

    const browser = await chromium.launch();
    const page = await browser.newPage();
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));

    const outputText = () => page.$eval('#divOutput', el => el.innerText);

    // A command's turn finishes asynchronously (single-threaded WASM), so a fixed
    // wait races it under load. canSendCommand alone can't tell a finished turn
    // from one not sent yet: a quick turn can clear and set it again before the
    // next page.evaluate() gets to look. So count the turns WebPlayer.sendCommand
    // (which a typed command goes through, and which sets canSendCommand again at
    // its end) has finished, and wait for this one to be among them.
    async function countTurns() {
        await page.evaluate(() => {
            window.__turnsDone = 0;
            const sendCommand = window.WebPlayer.sendCommand;
            window.WebPlayer.sendCommand = async function (...args) {
                try {
                    return await sendCommand.apply(this, args);
                } finally {
                    window.__turnsDone++;
                }
            };
        });
    }

    async function send(command) {
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 30000 });
        const before = await page.evaluate(() => window.__turnsDone);
        await page.fill('#txtCommand', command);
        await page.press('#txtCommand', 'Enter');
        await page.waitForFunction(
            n => window.__turnsDone > n && window.canSendCommand === true, before, { timeout: 30000 });
    }

    async function expectCount(n, label) {
        await send('showcount');
        const text = await outputText();
        const matches = [...text.matchAll(/Count: (\d+)/g)];
        const last = matches.at(-1)?.[1];
        if (last !== String(n)) throw new Error(`${label}: expected "Count: ${n}", last count shown was ${last ?? 'none'}`);
    }

    // Ask() renders its Yes/No inline as numbered options; answer by typing one.
    async function answerRestart(answer) {
        await send('restart');
        const text = await outputText();
        if (!text.includes('1: Yes') || !text.includes('2: No')) throw new Error('restart did not ask for confirmation');
        await send(answer === 'Yes' ? '1' : '2');
    }

    async function checkRestart(label, fixture) {
        console.log(`── ${label}`);
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}`);
        await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
        await countTurns();

        await send('bump');
        await send('bump');
        await expectCount(2, 'before restart');

        // Answering no leaves the game as it was.
        await answerRestart('No');
        await expectCount(2, 'after declining restart');
        console.log('PASS: answering no kept the game going');

        // Survives an in-app restart, but not a page reload.
        await page.evaluate(() => {
            window.__notReloaded = true;
            document.getElementById('divOutput').dataset.beforeRestart = 'true';
        });
        await answerRestart('Yes');
        // The in-app restart swaps in a fresh copy of the UI (so a new #divOutput)
        // and hides #qv-restarting once the game has really begun again;
        // #txtCommand is back long before that. Waiting for #qv-restarting to
        // appear first could miss it under load, so wait for the end state.
        await page.waitForFunction(() => {
            const output = document.getElementById('divOutput');
            return output && output.dataset.beforeRestart === undefined
                && document.getElementById('qv-restarting')?.classList.contains('hidden')
                && window.canSendCommand === true;
        }, null, { timeout: 30000 });
        await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 10000 });

        const text = await outputText();
        if (text.includes('Try pressing CTRL+R.')) throw new Error('restart printed "Try pressing CTRL+R." instead of restarting');
        if (!(await page.evaluate(() => window.__notReloaded === true))) {
            throw new Error('restart reloaded the page instead of restarting in place');
        }
        await expectCount(0, 'after restart');
        console.log('PASS: restart + yes restarted the game in place');
    }

    async function run() {
        await checkRestart("Quest 5's inlined Core", 'restart-quest5-core-test.aslx');
        await checkRestart('current Core', 'restart-current-core-test.aslx');

        if (pageErrors.length > 0) throw new Error(`Page errors: ${pageErrors.join('; ')}`);
        console.log('PASS: all checks passed');
    }

    try {
        await run();
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
