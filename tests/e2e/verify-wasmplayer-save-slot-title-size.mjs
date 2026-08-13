// Ad-hoc manual verification: WasmPlayer's save-slot titles must not inherit
// the loaded game's <body> font-size. Quest's Core library runs
// JS.setCss("body", "color:...;font-family:...;font-size:...pt") at startup,
// so the game's narrative font can be anything. The slot titles are Skeleton
// .anchor buttons, whose font-size:var(--anchor-font-size) defaults to
// `inherit` — unlike .btn/.input/.text-sm, which hard-code their own size —
// so they used to inherit the game's font-size and render out of proportion
// with the rest of the dialog. chrome.css pins #qv-saves-list to
// var(--text-base); this script asserts that stays true when <body> is big.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

try {
    await page.goto(`${baseUrl}/?url=/examples/simple.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 10000 });

    // Simulate a game that restyles <body> (Core.aslx's JS.setCss call), open
    // the save dialog in manage mode, and inject a couple of slots the way
    // renderSavesList would for a real save.
    const sizes = await page.evaluate(() => {
        document.body.style.fontSize = '20pt';
        const dlg = document.getElementById('qv-saves');
        dlg.dataset.mode = 'manage';
        dlg.showModal();
        renderSavesList([
            { slotIndex: 0, name: 'Saved game — 13/08/2026, 14:30' },
            { slotIndex: 1, name: 'My custom save' },
        ], 'manage');
        return {
            body: getComputedStyle(document.body).fontSize,
            slots: [...document.querySelectorAll('#qv-saves-list [data-slot]')].map(el => getComputedStyle(el).fontSize),
            btn: getComputedStyle(document.getElementById('qv-saves-save-new')).fontSize,
        };
    });

    console.log('sizes:', JSON.stringify(sizes));

    const bodyBig = parseFloat(sizes.body);
    if (bodyBig <= 20) throw new Error(`expected simulated game font to be large, got ${sizes.body}`);
    for (const [i, size] of sizes.slots.entries()) {
        if (parseFloat(size) !== 16) {
            throw new Error(`save slot ${i} title font-size ${size} — expected 16px (must not inherit the game's body font)`);
        }
    }
    if (parseFloat(sizes.btn) !== 16) throw new Error(`expected button to be 16px, got ${sizes.btn}`);
    console.log('PASS: save-slot titles stay at the dialog base size despite a large game body font');
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-save-slot-title-size-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
