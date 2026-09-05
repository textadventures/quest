// Verifies that the ~10.6 MB .NET runtime download starts up front rather than
// behind the game fetch, and that it still doesn't start at all when no game is
// on its way.
//
// The runtime has nothing to do with which game is being played, but it used to
// be imported inside initWasmPlayer() — i.e. only once the game bytes had
// arrived, which for a ?id= link means after a textadventures API round trip
// and then the game download. wasm-player.js's boot IIFE now kicks off
// dotnet.download() before either (issue #2186), and index.html modulepreloads
// dotnet.js so the preload scanner starts it before the parser even reaches
// wasm-player.js. Both are invisible when they regress — the player still
// works, just slower — hence this guard.
//
// Run against the WasmPlayer dev server:
//   node src/WasmPlayer/dev-server.mjs
//   node tests/e2e/verify-wasmplayer-runtime-preload.mjs http://localhost:5175
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const GAME_DELAY_MS = 3000;

const browser = await chromium.launch();

try {
    // ── The runtime download must overlap the game fetch, not follow it ──────
    {
        const page = await browser.newPage();
        const framework = [];
        let gameDelivered = false;
        let frameworkRequestsBeforeGame = 0;

        page.on('request', r => { if (r.url().includes('/_framework/')) framework.push(r.url()); });

        // Hold the game file back long enough that a runtime download gated on
        // it could not possibly have got going by the time we check.
        await page.route('**/examples/simple.aslx', async route => {
            await new Promise(resolve => setTimeout(resolve, GAME_DELAY_MS));
            // Sampled at the end of the delay: whatever is in flight by now got
            // there without the game bytes, which are still being withheld.
            frameworkRequestsBeforeGame = framework.length;
            gameDelivered = true;
            await route.continue();
        });

        await page.goto(`${baseUrl}/?url=/examples/simple.aslx`, { waitUntil: 'load' });
        await page.waitForFunction(
            () => document.querySelector('#divOutput')?.textContent?.includes('You are in a room'),
            { timeout: 120000 });
        if (!gameDelivered) throw new Error('the game route was never hit — the fixture URL must have changed');

        // Measured mid-delay: several dozen assets are already in flight by
        // then. Anything in single figures means the download is waiting on
        // something it shouldn't be.
        if (frameworkRequestsBeforeGame < 20) {
            throw new Error(`only ${frameworkRequestsBeforeGame} _framework requests had started ${GAME_DELAY_MS}ms into the game fetch `
                + '— the runtime download looks gated on the game bytes again (see preloadRuntime in wasm-player.js)');
        }

        // A modulepreload that doesn't match the import()'s URL exactly costs a
        // second full fetch of every module rather than saving anything.
        const counts = framework.reduce((acc, u) => (acc[u] = (acc[u] || 0) + 1, acc), {});
        const repeated = Object.entries(counts).filter(([, n]) => n > 1);
        if (repeated.length) {
            throw new Error(`${repeated.length} _framework URLs were fetched more than once (the modulepreload and the import() disagree on a URL):\n  `
                + repeated.slice(0, 5).map(([u, n]) => `${n}x ${u}`).join('\n  '));
        }

        console.log(`PASS: ${frameworkRequestsBeforeGame} runtime assets already downloading while the game fetch was still outstanding, no duplicated fetches.`);
        await page.close();
    }

    // ── ...but not when there's no game to play ─────────────────────────────
    {
        const page = await browser.newPage();
        const framework = [];
        page.on('request', r => { if (r.url().includes('/_framework/')) framework.push(r.url()); });

        await page.goto(baseUrl, { waitUntil: 'load' });
        await page.waitForSelector('#qv-pickers', { state: 'visible', timeout: 30000 });
        await page.waitForTimeout(4000);

        // dotnet.js itself is modulepreloaded unconditionally — it's 37 KB, and
        // having it ready is what makes the download start promptly once the
        // user picks a file. The other ~190 files are the ones that must wait.
        const beyondLoader = framework.filter(u => !/\/_framework\/dotnet\.js(\?|$)/.test(u));
        if (beyondLoader.length) {
            throw new Error(`the start screen pulled ${beyondLoader.length} runtime assets with no game loading — `
                + `preloadRuntime() must stay off the wireStartScreen path. e.g.\n  ${beyondLoader.slice(0, 5).join('\n  ')}`);
        }

        console.log(`PASS: start screen with no game fetched only the ${framework.length} modulepreloaded loader file, not the runtime.`);
        await page.close();
    }
} catch (e) {
    process.exitCode = 1;
    console.error(`FAIL: ${e.message}`);
} finally {
    await browser.close();
}
