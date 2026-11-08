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
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
const pageErrors = [];
page.on('pageerror', err => pageErrors.push(err.message));

const outputText = () => page.$eval('#divOutput', el => el.innerText);

async function send(command) {
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
    await page.waitForTimeout(300);
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
    await page.waitForTimeout(500);

    await send('bump');
    await send('bump');
    await expectCount(2, 'before restart');

    // Answering no leaves the game as it was.
    await answerRestart('No');
    await page.waitForTimeout(500);
    await expectCount(2, 'after declining restart');
    console.log('PASS: answering no kept the game going');

    // Survives an in-app restart, but not a page reload.
    await page.evaluate(() => { window.__notReloaded = true; });

    await answerRestart('Yes');
    // #qv-restarting brackets restartGameCore's actual work (see
    // verify-wasmplayer-restart-sidebar-dup.mjs) — #txtCommand is back long
    // before the game has really restarted.
    await page.waitForSelector('#qv-restarting', { state: 'visible', timeout: 10000 });
    await page.waitForSelector('#qv-restarting', { state: 'hidden', timeout: 30000 });
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
    process.exitCode = 1;
} finally {
    await browser.close();
}
