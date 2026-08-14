// Ad-hoc manual verification for a bug found in manual testing: refreshing
// the WasmPlayer tab opened by AppShell's Play tab "Start" flow used to spin
// forever with no error — the BroadcastChannel handoff (see PlayCatalog.svelte
// handleStart / wasm-player.js's `source=local` boot branch) closed itself
// after the first message, so a refresh's fresh 'ready' broadcast had no one
// listening. Fixed by (a) keeping the AppShell-side channel open for the
// life of that Play tab, matching the never-closed editor-preview channel,
// and (b) an 8s timeout in WasmPlayer that surfaces a real error (with a
// working fallback picker) if nothing ever answers.
//
// Covers both paths:
//   1. Refresh while the AppShell tab is still open -> should recover and
//      re-boot the same game (no lost session).
//   2. Refresh after the AppShell tab has been closed -> should show the
//      "Couldn't reconnect" error within ~8s, not spin forever.
//
// Requires AppShell (5174) and WasmPlayer (5175) dev servers running.
import { chromium } from 'playwright';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const fixturePath = join(dirname(fileURLToPath(import.meta.url)), 'fixtures', 'restart-test.aslx');

async function startGameFromPlayTab(context, page) {
    await page.goto(`${baseUrl}/`);
    await page.waitForSelector('button:has-text("Open a game file…")', { timeout: 15000 });
    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        page.click('button:has-text("Open a game file…")'),
    ]);
    await fileChooser.setFiles(fixturePath);
    await page.waitForSelector('button:has-text("Start")', { timeout: 5000 });

    const [popup] = await Promise.all([
        context.waitForEvent('page'),
        page.click('button:has-text("Start")'),
    ]);
    await popup.waitForFunction(() => document.title === 'Simple', null, { timeout: 15000 });
    return popup;
}

async function testRefreshWithOpenerStillOpen() {
    const browser = await chromium.launch();
    const context = await browser.newContext();
    const page = await context.newPage();
    try {
        const popup = await startGameFromPlayTab(context, page);
        console.log('[refresh-recovers] game booted, refreshing player tab...');

        await popup.reload();
        await popup.waitForFunction(() => document.title === 'Simple', null, { timeout: 15000 });
        console.log('[refresh-recovers] PASS — game re-booted after refresh with AppShell tab still open');
    } catch (err) {
        console.error('[refresh-recovers] FAIL:', err.message);
        await page.screenshot({ path: '/tmp/appshell-play-refresh-recovers-failure.png' });
        process.exitCode = 1;
    } finally {
        await browser.close();
    }
}

async function testRefreshAfterOpenerClosed() {
    const browser = await chromium.launch();
    const context = await browser.newContext();
    const page = await context.newPage();
    try {
        const popup = await startGameFromPlayTab(context, page);
        console.log('[refresh-errors] game booted, closing AppShell tab, then refreshing player tab...');
        await page.close();

        await popup.reload();
        await popup.waitForSelector('text=Couldn\'t reconnect to Quest Viva', { timeout: 12000 });
        console.log('[refresh-errors] error message shown');

        // The fallback picker in that same error state should still work.
        const [fileChooser] = await Promise.all([
            popup.waitForEvent('filechooser'),
            popup.click('#qv-file-btn'),
        ]);
        await fileChooser.setFiles(fixturePath);
        await popup.waitForFunction(() => document.title === 'Simple', null, { timeout: 15000 });
        console.log('[refresh-errors] PASS — error shown within timeout, and its fallback picker still works');
    } catch (err) {
        console.error('[refresh-errors] FAIL:', err.message);
        await popup?.screenshot({ path: '/tmp/appshell-play-refresh-errors-failure.png' }).catch(() => {});
        process.exitCode = 1;
    } finally {
        await browser.close();
    }
}

await testRefreshWithOpenerStillOpen();
await testRefreshAfterOpenerClosed();
