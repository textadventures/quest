// Checks when WasmPlayer asks for a click (the Begin card) before starting a game, so the browser
// lets it play sound. Both gamebooks are loaded as if from textadventures.co.uk (?id=, with the
// API mocked), where the player has the game's text but no list of its files, and the page has
// not been clicked yet (navigator.userActivation is faked, since headless Chromium reports it
// as already clicked).
//
// - fixtures/begin-gate-no-sound.aslx: a gamebook with "play sound" in its text, as every
//   published gamebook has from its copy of GamebookCore, but no sound file. It starts straight away.
// - fixtures/v2-gamebook-test.aslx: a page with a sound file. It shows Begin, and starts once
//   that's clicked.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    // Returns 'begin' if the Begin card shows, or 'started' if the game starts without it.
    async function open(fixture) {
        const page = await browser.newPage();
        await page.addInitScript(() => {
            Object.defineProperty(navigator, 'userActivation', { get: () => ({ hasBeenActive: false, isActive: false }) });
        });
        await page.route('**/api/game/**', (route) => route.fulfill({
            contentType: 'application/json',
            body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/${fixture}.aslx` }),
        }));
        await page.goto(`${baseUrl}/?id=${fixture}`);
        const outcome = await page.waitForFunction(() => {
            if (document.getElementById('qv-clicktobegin')?.offsetParent) return 'begin';
            if (!document.getElementById('qv-start') && window.canSendCommand) return 'started';
            return null;
        }, null, { timeout: 30000 }).then((h) => h.jsonValue()).catch(() => { throw new Error(`${fixture} never loaded`); });
        return { page, outcome };
    }

    const noSound = await open('begin-gate-no-sound');
    check(noSound.outcome === 'started', 'a gamebook with no sound file starts without Begin');
    await noSound.page.close();

    const sound = await open('v2-gamebook-test');
    check(sound.outcome === 'begin', 'a gamebook with a sound file shows Begin');
    await sound.page.click('#qv-clicktobegin-btn');
    await sound.page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand, null, { timeout: 30000 })
        .catch(() => { throw new Error('the game never started after Begin'); });
    check(true, 'clicking Begin starts the game');
    await sound.page.close();
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
