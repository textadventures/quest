// Ad-hoc manual verification: a game whose contents the engine rejects (e.g. an
// unknown function in a script) must show the load error on the start screen
// instead of hanging on the loading spinner forever. Previously initWasmPlayer
// wrote the errors to the game output pane (hidden under the #qv-start overlay)
// then threw before the overlay was removed, so every boot path without a catch
// (embedded export, editor preview, ?id=, ?url=) sat on the spinner indefinitely.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function expectStartScreenError(label) {
    // #qv-error keeps its .hidden class; showGameLoadError shows it via an
    // inline style.display override (same as showError), so wait on computed
    // visibility, not the class.
    await page.waitForSelector('#qv-error', { state: 'visible', timeout: 30000 });
    const errorText = await page.$eval('#qv-error', el => el.textContent);
    if (!errorText.includes("Function not found: 'missing_function'")) {
        throw new Error(`${label}: expected the engine error text on the start screen, got: ${errorText}`);
    }
    console.log(`PASS: ${label}`);

    const loadingVisible = await page.$eval('#qv-loading-msg', el => el.style.display !== 'none');
    if (loadingVisible) {
        throw new Error(`${label}: loading spinner must be hidden after a load failure`);
    }
    console.log('PASS: loading spinner is hidden after the failure');

    const pickersVisible = await page.$eval('#qv-pickers', el => el.style.display !== 'none');
    if (!pickersVisible) {
        throw new Error(`${label}: file pickers must be shown again after a load failure`);
    }
    console.log('PASS: file pickers are shown again');
}

// A game launched by the Quest Viva app itself (Play tab, editor preview) was
// handed over a BroadcastChannel — the app owns the file-picking UI, so the
// player must show only the error, in the same format the editor uses for a
// load-time error ("Failed to load game due to the following errors:" + "* "
// bullets, with a blank line before the message), and must NOT show the
// pickers or "You can try a different file below".
async function expectAppLaunchedError(label, channelName) {
    await page.goto(`${baseUrl}/?source=${channelName === 'quest-preview' ? 'editor' : 'local'}`);
    await page.evaluate(async ({ fixtureUrl, channelName, filename }) => {
        // Simulates the app side of the handoff: fetch the fixture and post it
        // over the same channel the player window is listening on. The player
        // page's own bc.onmessage is set synchronously during boot, so this
        // lands after that handler is live.
        const resp = await fetch(fixtureUrl);
        const bytes = new Uint8Array(await resp.arrayBuffer());
        const bc = new BroadcastChannel(channelName);
        bc.postMessage({ type: 'game', bytes, filename, adjacentFiles: null });
    }, { fixtureUrl: `${baseUrl}/e2e-fixtures/load-error-test.aslx`, channelName, filename: 'load-error-test.aslx' });

    await page.waitForSelector('#qv-error', { state: 'visible', timeout: 30000 });
    const errorText = await page.$eval('#qv-error', el => el.textContent);
    if (!errorText.includes('Failed to load game due to the following errors:')) {
        throw new Error(`${label}: expected the editor-style error header, got: ${JSON.stringify(errorText)}`);
    }
    if (!errorText.includes("* Error: Error adding script attribute 'speak' to element 'person': Function not found: 'missing_function'")) {
        throw new Error(`${label}: expected the editor-style bulleted error, got: ${JSON.stringify(errorText)}`);
    }
    if (!errorText.includes('errors:\n\n')) {
        throw new Error(`${label}: expected a blank line before the error message, got: ${JSON.stringify(errorText)}`);
    }
    console.log(`PASS: ${label} shows the editor-style error`);

    const loadingVisible = await page.$eval('#qv-loading-msg', el => el.style.display !== 'none');
    if (loadingVisible) {
        throw new Error(`${label}: loading spinner must be hidden after a load failure`);
    }
    console.log('PASS: loading spinner is hidden after the failure');

    const pickersVisible = await page.$eval('#qv-pickers', el => el.style.display !== 'none');
    if (pickersVisible) {
        throw new Error(`${label}: the file pickers must not be shown for an app-launched game`);
    }
    console.log('PASS: file pickers are hidden for an app-launched game');
}

async function run() {
    // 1. Load the broken game by URL (?url= boot path — one of the ones that
    //    previously hung forever). The engine error must appear on the start
    //    screen and the spinner must be gone.
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/load-error-test.aslx`);
    await expectStartScreenError('?url= boot shows the engine load error on the start screen');

    // 2. Start-screen "Load from URL" input path: the error must surface and the
    //    failed game's URL must not be persisted to the address bar (a refresh
    //    would otherwise re-run the same broken load).
    await page.goto(baseUrl);
    await page.waitForSelector('#qv-url-input', { timeout: 30000 });
    await page.fill('#qv-url-input', `${baseUrl}/e2e-fixtures/load-error-test.aslx`);
    await page.click('#qv-url-btn');
    await expectStartScreenError('start-screen URL input shows the engine load error');
    if (page.url().includes('url=')) {
        throw new Error(`failed game URL must not be persisted, got: ${page.url()}`);
    }
    console.log('PASS: failed game URL is not persisted');

    // 3. File-picker boot path: the error must surface there too (previously it
    //    called showError(null, true) → "It doesn't look like a Quest game file",
    //    hiding the real reason).
    await page.goto(baseUrl);
    const response = await page.request.get(`${baseUrl}/e2e-fixtures/load-error-test.aslx`);
    await page.locator('#qv-file-input').setInputFiles({
        name: 'load-error-test.aslx',
        mimeType: 'application/xml',
        buffer: await response.body(),
    });
    await expectStartScreenError('file-picker boot shows the engine load error on the start screen');

    // 4. Play-tab launch (source=local) and editor preview (source=editor) both
    //    hand the game over over a BroadcastChannel — the Quest Viva app owns the
    //    file-picking UI in those, so the error must show WITHOUT the pickers /
    //    "try a different file" message, in the editor's load-error format.
    await expectAppLaunchedError('source=local', 'quest-play-local');
    await expectAppLaunchedError('source=editor', 'quest-preview');

    // 5. Sanity check: a healthy game still boots to the player (no regression —
    //    initWasmPlayer now returns true instead of throwing/nothing).
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/dialogue-pages-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000 });
    console.log('PASS: a healthy game still boots normally');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/verify-game-load-error-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
