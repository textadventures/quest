// Ad-hoc manual verification: AppShell's Play tab (PlayCatalog.svelte)
// "Open a game file…" -> pick a file -> "Start" flow. Two distinct clicks
// (pick, then start), each with its own fresh browser activation, rather than
// one click that tries to both show a native file dialog and open a new tab
// (that fails — the two compete for a single click's popup-blocker
// exemption; verified experimentally while building this). Confirms: picking
// a file reveals the Start button, clicking Start opens a new tab, and the
// game's bytes make it across the BroadcastChannel handoff to actually boot
// (wasm-player.js's `source=local` boot branch).
//
// Requires AppShell (5174) and WasmPlayer (5175) dev servers running:
//   node src/WasmPlayer/dev-server.mjs &
//   (cd src/AppShell && PUBLIC_SHOW_HOME=true npx vite dev) &
import { chromium } from 'playwright';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const context = await browser.newContext();
const page = await context.newPage();
page.on('pageerror', err => console.log('[appshell] [pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[appshell] [console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/`);
    await page.waitForSelector('button:has-text("Open a game file…")', { timeout: 15000 });
    console.log('[appshell] Play tab loaded, "Open a game file…" button present');

    const fixturePath = join(dirname(fileURLToPath(import.meta.url)), 'fixtures', 'restart-test.aslx');
    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        page.click('button:has-text("Open a game file…")'),
    ]);
    await fileChooser.setFiles(fixturePath);
    console.log('[appshell] file picked');

    await page.waitForSelector('button:has-text("Start")', { timeout: 5000 });
    console.log('[appshell] Start button appeared with picked file name:', await page.textContent('text=restart-test.aslx'));

    const [popup] = await Promise.all([
        context.waitForEvent('page'),
        page.click('button:has-text("Start")'),
    ]);
    console.log('[appshell] popup tab opened on Start click:', popup.url());

    popup.on('pageerror', err => console.log('[player] [pageerror]', err.message));
    popup.on('console', msg => { if (msg.type() === 'error') console.log('[player] [console.error]', msg.text()); });

    await popup.waitForFunction(() => document.title === 'Simple', null, { timeout: 15000 });
    console.log('[player] game booted, document.title:', await popup.title());

    // Back on the AppShell tab, the picked-file UI should have reset to the plain button.
    await page.waitForSelector('button:has-text("Open a game file…")', { timeout: 5000 });
    console.log('[appshell] picked-file UI reset after handoff');

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-play-open-file-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
