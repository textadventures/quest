// WasmPlayer's Save/Load dialog must not inherit the loaded game's <body> font-size. Quest's Core
// library runs JS.setCss("body", "color:...;font-family:...;font-size:...pt") at startup, so the
// game's narrative font can be anything. Plain text in the dialog, like the save titles, and
// Skeleton's .anchor (the footer links, whose font-size:var(--anchor-font-size) defaults to
// `inherit`) would follow it and render out of proportion with the rest of the dialog. chrome.css
// pins the dialog's card to var(--text-base); this script asserts that stays true when <body> is
// big.
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
            { slotIndex: 0, name: 'Saved game — 13/08/2026, 14:30', timestamp: new Date() },
            { slotIndex: 1, name: 'My custom save', location: 'Kitchen', timestamp: new Date() },
        ], 'manage');
        return {
            body: getComputedStyle(document.body).fontSize,
            slots: [...document.querySelectorAll('#qv-saves-list .qv-save-title')].map(el => getComputedStyle(el).fontSize),
            links: [...document.querySelectorAll('#qv-saves .qv-saves-footer .anchor')].map(el => getComputedStyle(el).fontSize),
            btn: getComputedStyle(document.getElementById('qv-saves-save-new')).fontSize,
            // Each row's options button's sprite icon must point at a <symbol> that
            // build-icons.mjs actually spliced into the page.
            optionIcons: [...document.querySelectorAll('#qv-saves-list [data-slot-options] use')].map(use => {
                const id = use.getAttribute('href').slice(1);
                return { id, resolves: document.getElementById(id)?.tagName.toLowerCase() === 'symbol' };
            }),
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
    if (sizes.slots.length !== 2) throw new Error(`expected 2 save titles, got ${sizes.slots.length}`);
    if (sizes.links.length !== 4) throw new Error(`expected 4 footer links, got ${sizes.links.length}`);
    for (const size of sizes.links) {
        if (parseFloat(size) !== 14) throw new Error(`footer link font-size ${size} — expected 14px`);
    }
    if (parseFloat(sizes.btn) !== 16) throw new Error(`expected button to be 16px, got ${sizes.btn}`);
    if (sizes.optionIcons.length !== 2) throw new Error(`expected 2 options-button icons, got ${sizes.optionIcons.length}`);
    for (const icon of sizes.optionIcons) {
        if (!icon.resolves) throw new Error(`options-button icon #${icon.id} has no matching <symbol> in the page`);
    }
    console.log('PASS: the save rows\' options icons resolve to a sprite symbol');
    console.log('PASS: save titles and footer links stay at the dialog\'s own sizes despite a large game body font');
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-save-slot-title-size-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
