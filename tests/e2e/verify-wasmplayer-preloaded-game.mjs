// A ?id= link must boot straight from window.QuestVivaPreloadedGame when it's
// present for that id, without its own textadventures API round trip
// (textadventures/quest#2194). On play.questviva.com that global is inlined by
// a Pages Function (src/PlayFunctions/lib/game-location.ts); here it's set by
// an init script instead, since the WasmPlayer dev server has no Functions.
// A preloaded game for a different id must be ignored, falling back to the API.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
let page = null;

// Boots /?id=<urlId> with a preloaded game for <preloadedId>, returning how
// many api/game requests the player made.
async function boot(urlId, preloaded) {
    page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    let apiRequests = 0;
    await page.route('**/api/game/**', async (route) => {
        apiRequests++;
        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ sourceGameUrl: '/examples/simple.aslx', resourceRoot: null }),
        });
    });
    await page.addInitScript((game) => { window.QuestVivaPreloadedGame = game; }, preloaded);
    await page.goto(`${baseUrl}/?id=${urlId}`);
    await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
    return apiRequests;
}

async function run() {
    const matching = await boot('preloaded-id', {
        id: 'preloaded-id', sourceGameUrl: '/examples/simple.aslx', resourceRoot: null,
    });
    if (matching !== 0) {
        throw new Error(`expected no api/game request when the game was preloaded, got ${matching}`);
    }
    console.log('PASS: preloaded game boots with no api/game request');
    await page.close();

    // Points at a nonexistent file, so booting at all proves it was ignored.
    const mismatched = await boot('requested-id', {
        id: 'some-other-id', sourceGameUrl: '/does-not-exist.aslx', resourceRoot: null,
    });
    if (mismatched !== 1) {
        throw new Error(`expected exactly 1 api/game request for a mismatched preloaded id, got ${mismatched}`);
    }
    console.log('PASS: preloaded game for a different id is ignored, falling back to the API');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page?.screenshot({ path: '/tmp/verify-wasmplayer-preloaded-game-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
