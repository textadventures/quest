// Verifies that an open verb menu (the legacy UI's jjmenu pop-up on an object
// hyperlink) keeps keyboard focus when the player refocuses the command box
// by itself.
//
// Its arrow keys, Enter and Escape are handled on the menu items, so focus
// has to stay on one. Just after a game loads, the first turn's delayed
// re-scroll (setPanelHeight's timeout) and the jQuery "inview" handler both
// call for #txtCommand's focus, and used to take it out of a menu opened in
// that window, leaving it open with Escape going to the command box.
// mobile-touch-targets hung on exactly that on a slow CI runner.
//
// The race is timing-dependent, so as well as opening a menu straight after
// load, this calls each of the refocusing paths directly with the menu open.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: npx playwright test --project=wasmplayer verb-menu-focus

import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('verb-menu-focus', async ({ baseURL }) => {
    const GAME = '/e2e-fixtures/mobile-touch-targets-large-menu-test.aslx';

    const browser = await chromium.launch();
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    const focusInMenu = () => page.evaluate(() => !!document.activeElement?.closest('#jjmenu_main'));
    const activeDescription = () => page.evaluate(() => {
        const el = document.activeElement;
        return el ? `${el.tagName.toLowerCase()}${el.id ? '#' + el.id : ''}.${el.className}` : 'nothing';
    });

    async function openMenu() {
        await page.click('#divOutput .elementmenu');
        await page.waitForSelector('#jjmenu_main .jj_menu_item', { timeout: 5000 });
        if (!await focusInMenu()) {
            throw new Error(`opening the verb menu should focus its first item, but focus is on ${await activeDescription()}`);
        }
    }

    async function expectFocusKept(label) {
        if (!await focusInMenu()) {
            throw new Error(`${label}: focus left the open verb menu for ${await activeDescription()}`);
        }
        console.log(`PASS: ${label}: focus stays in the open verb menu`);
    }

    async function closeWithEscape(label) {
        await page.keyboard.press('Escape');
        await page.waitForFunction(() => !document.querySelector('#jjmenu_main'), undefined, { timeout: 5000 });
        const backOnLink = await page.evaluate(() => !!document.activeElement?.closest('#divOutput .elementmenu'));
        if (!backOnLink) {
            throw new Error(`${label}: Escape closed the menu but left focus on ${await activeDescription()}, not the link that opened it`);
        }
        console.log(`PASS: ${label}: Escape closes the menu and returns focus to its link`);
    }

    try {
        await page.goto(`${baseURL}/?url=${encodeURIComponent(GAME)}`);
        await page.waitForFunction(
            () => document.querySelectorAll('#divOutput .elementmenu').length > 0,
            undefined,
            { timeout: 60000 },
        );

        // Straight after load, while the first turn's delayed re-scroll and
        // the inview handler are still to come
        await openMenu();
        await page.waitForTimeout(1000);
        await expectFocusKept('just after load');
        await closeWithEscape('just after load');

        // Each refocusing path, called while the menu is open
        await openMenu();
        await page.evaluate(() => window.scrollToEnd());
        await expectFocusKept('end-of-turn scroll');
        await page.evaluate(() => window.setPanelHeight());
        await page.waitForTimeout(300);
        await expectFocusKept('picture frame re-scroll');
        await page.evaluate(() => window.jQuery('#txtCommand').trigger('inview', [true]));
        await expectFocusKept('command bar scrolling into view');
        await closeWithEscape('after the refocusing calls');

        // With no menu open, the command box still takes focus as before
        await page.evaluate(() => document.activeElement?.blur());
        await page.evaluate(() => window.scrollToEnd());
        const commandFocused = await page.evaluate(() => document.activeElement?.id === 'txtCommand');
        if (!commandFocused) {
            throw new Error(`with no menu open, the end-of-turn scroll should focus the command box, but focus is on ${await activeDescription()}`);
        }
        console.log('PASS: with no menu open, the end-of-turn scroll still focuses the command box');

        console.log('\nAll checks passed.');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
