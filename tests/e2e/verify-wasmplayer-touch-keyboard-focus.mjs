// Verifies that on a touch device the player doesn't move focus to the command input by itself,
// which would open the on-screen keyboard (#2674): not on load, and not at the end of a turn
// started by tapping a link. Once the player has tapped the input to type, it keeps focus between
// turns, so their keyboard stays up. With a mouse, the input is still focused on load and after
// every turn. Covers both the legacy and the v2 UI.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-touch-keyboard-focus.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';
const GAME = '/e2e-fixtures/touch-keyboard-focus-test.aslx';

// Long enough for legacy's turn-end scroll (up to 2s) to finish and focus the input, as it would.
const SETTLE_MS = 2500;

const UIS = {
    legacy: { output: '#divOutput', input: '#txtCommand' },
    v2: { output: '[data-qv-part="output"]', input: '.qv-command-input' },
};

const browser = await chromium.launch();

async function open(ui, touch) {
    const context = await browser.newContext(touch
        ? { viewport: { width: 390, height: 844 }, hasTouch: true, isMobile: true }
        : { viewport: { width: 1280, height: 800 } });
    const page = await context.newPage();
    await page.goto(`${BASE}/?ui=${ui}&url=${encodeURIComponent(GAME)}`);
    await page.locator(UIS[ui].output).getByText('You are in a room.').first().waitFor({ timeout: 60000 });
    await page.waitForTimeout(SETTLE_MS);
    return { context, page };
}

async function inputFocused(page, ui) {
    return page.evaluate((sel) => document.activeElement === document.querySelector(sel), UIS[ui].input);
}

async function expectFocus(page, ui, expected, label) {
    const focused = await inputFocused(page, ui);
    if (focused !== expected) {
        throw new Error(`${ui}: ${label}: expected the command input ${expected ? '' : 'not '}to be focused`);
    }
    console.log(`PASS: ${ui}: ${label}: command input ${expected ? 'focused' : 'not focused'}`);
}

async function waves(page, ui) {
    return page.evaluate((sel) => document.querySelector(sel).innerText.split('You wave.').length - 1, UIS[ui].output);
}

// Taps (or clicks) the newest wave link and waits for its turn to end.
async function useLink(page, ui, touch) {
    const before = await waves(page, ui);
    const link = page.locator(UIS[ui].output).getByText(/^Wave( again)?$/).last();
    if (touch) await link.tap(); else await link.click();
    await page.waitForFunction(
        ([sel, n]) => document.querySelector(sel).innerText.split('You wave.').length - 1 > n,
        [UIS[ui].output, before], { timeout: 15000 });
    await page.waitForTimeout(SETTLE_MS);
}

try {
    for (const ui of Object.keys(UIS)) {
        const touch = await open(ui, true);
        try {
            await expectFocus(touch.page, ui, false, 'touch, after load');
            await useLink(touch.page, ui, true);
            await expectFocus(touch.page, ui, false, 'touch, after tapping a link');

            const input = touch.page.locator(UIS[ui].input);
            await input.tap();
            await input.fill('wave');
            const before = await waves(touch.page, ui);
            await input.press('Enter');
            await touch.page.waitForFunction(
                ([sel, n]) => document.querySelector(sel).innerText.split('You wave.').length - 1 > n,
                [UIS[ui].output, before], { timeout: 15000 });
            await touch.page.waitForTimeout(SETTLE_MS);
            await expectFocus(touch.page, ui, true, 'touch, after typing a command');

            await useLink(touch.page, ui, true);
            await expectFocus(touch.page, ui, false, 'touch, after tapping a link again');
        } finally {
            await touch.context.close();
        }

        const mouse = await open(ui, false);
        try {
            await expectFocus(mouse.page, ui, true, 'mouse, after load');
            await mouse.page.evaluate((sel) => document.querySelector(sel).blur(), UIS[ui].input);
            await useLink(mouse.page, ui, false);
            await expectFocus(mouse.page, ui, true, 'mouse, after clicking a link');
        } finally {
            await mouse.context.close();
        }
    }
    console.log('PASS: all checks passed');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
