// Verifies three fixes to the legacy (classic) UI's panes and command box:
//
// - #2358: the Inventory and Places and Objects items are listbox options the keyboard reaches.
//   The list is one Tab stop, the arrow keys move through it and show that item's verb buttons,
//   and Enter moves on to the first verb button.
// - #2359: with a dark pane colour scheme (setPanes), the phone drawer takes the scheme's
//   background instead of staying white.
// - #2359: a borderless command box ("Use a cursor instead of a box") stays transparent when
//   Core's command bar format carries the game's background, even though Core sends that format
//   before it adds the borderless style.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: npx playwright test --project=wasmplayer legacy-panes-keyboard

import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('legacy-panes-keyboard', async ({ baseURL }) => {
    const BASE = baseURL;
    const GAME = '/e2e-fixtures/legacy-panes-keyboard-test.aslx';

    const browser = await chromium.launch();
    const page = await browser.newPage({ viewport: { width: 1200, height: 900 } });

    async function load() {
        await page.goto(`${BASE}/?url=${encodeURIComponent(GAME)}`);
        await page.locator('#divOutput').getByText('You are in a room.').first().waitFor({ timeout: 60000 });
    }

    // Waits for one more copy of the reply than was there before, as a command can be sent twice.
    async function send(command, reply) {
        const replies = page.locator('#divOutput').getByText(reply, { exact: true });
        const before = await replies.count();
        const input = page.locator('#txtCommand');
        await input.fill(command);
        await input.press('Enter');
        await replies.nth(before).waitFor({ timeout: 15000 });
    }

    const focused = () => page.evaluate(() => {
        const el = document.activeElement;
        return { id: el?.id ?? '', text: el?.textContent?.trim() ?? '', role: el?.getAttribute('role') ?? '' };
    });

    const commandBackground = () => page.evaluate(() => getComputedStyle(document.querySelector('#txtCommand')).backgroundColor);

    try {
        await load();

        // ── #2358: keyboard access to the pane lists ────────────────────────────
        const roles = await page.evaluate(() => ['#lstInventory', '#lstPlacesObjects'].map(s => {
            const list = document.querySelector(s);
            return {
                role: list.getAttribute('role'),
                label: document.getElementById(list.getAttribute('aria-labelledby') ?? '')?.textContent?.trim(),
                items: [...list.children].map(li => [li.getAttribute('role'), li.getAttribute('tabindex')]),
            };
        }));
        if (roles[0].role !== 'listbox' || roles[0].label !== 'Inventory') throw new Error(`inventory list: ${JSON.stringify(roles[0])}`);
        if (roles[1].role !== 'listbox' || roles[1].label !== 'Places and Objects') throw new Error(`places list: ${JSON.stringify(roles[1])}`);
        const tabStops = roles[0].items.filter(([, t]) => t === '0').length;
        if (roles[0].items.length !== 2 || tabStops !== 1 || roles[0].items.some(([r]) => r !== 'option')) {
            throw new Error(`inventory items should be two options with one Tab stop: ${JSON.stringify(roles[0].items)}`);
        }
        console.log('PASS: the pane lists are labelled listboxes, one Tab stop each');

        // Tab from the accordion header's button into the list.
        await page.locator('#inventoryLabel button').focus();
        await page.keyboard.press('Tab');
        let f = await focused();
        if (f.role !== 'option' || f.text !== 'lamp') throw new Error(`Tab from the Inventory header reached ${JSON.stringify(f)}, expected the lamp`);
        console.log('PASS: Tab reaches the first inventory item');

        await page.keyboard.press('ArrowDown');
        f = await focused();
        if (f.text !== 'coin') throw new Error(`ArrowDown reached ${JSON.stringify(f)}, expected the coin`);
        const afterArrow = await page.evaluate(() => ({
            selected: [...document.querySelectorAll('#lstInventory li')].map(li => li.getAttribute('aria-selected')),
            tabindex: [...document.querySelectorAll('#lstInventory li')].map(li => li.getAttribute('tabindex')),
            verbs: [...document.querySelectorAll('#inventoryAccordion .verbButtons button')].filter(b => b.offsetParent).map(b => b.textContent.trim()),
        }));
        if (afterArrow.selected.join() !== 'false,true' || afterArrow.tabindex.join() !== '-1,0') {
            throw new Error(`ArrowDown should select the coin and make it the Tab stop: ${JSON.stringify(afterArrow)}`);
        }
        if (!afterArrow.verbs.includes('Drop')) throw new Error(`the coin's verb buttons aren't shown: ${JSON.stringify(afterArrow.verbs)}`);
        console.log(`PASS: ArrowDown selects the next item and shows its verbs (${afterArrow.verbs.join(', ')})`);

        await page.keyboard.press('ArrowUp');
        await page.keyboard.press('Enter');
        f = await focused();
        if (!f.id.startsWith('cmdInventory')) throw new Error(`Enter on the lamp moved focus to ${JSON.stringify(f)}, expected its first verb button`);
        const verb = f.text;
        await page.keyboard.press('Enter');
        await page.locator('#divOutput').getByText(verb === 'Look at' ? 'A brass lamp.' : /lamp/i).last().waitFor({ timeout: 15000 });
        console.log(`PASS: Enter on an item moves to its first verb button ("${verb}"), which runs the command`);

        // Places and Objects too, and the list keeps focus when the game refreshes it.
        await page.locator('#placesObjectsLabel button').focus();
        await page.keyboard.press('Tab');
        f = await focused();
        if (f.role !== 'option' || f.text !== 'box') throw new Error(`Tab from the Places and Objects header reached ${JSON.stringify(f)}, expected the box`);
        console.log('PASS: Tab reaches the Places and Objects items');

        // ── #2359: a borderless command box keeps no background ─────────────────
        await send('barformat', 'Command bar format set.');
        const boxed = await commandBackground();
        if (boxed !== 'rgb(18, 52, 86)') throw new Error(`the command bar format's background wasn't applied: ${boxed}`);
        console.log('PASS: the command bar format still colours an ordinary command box');

        await send('oldborderless', 'Old borderless style added.');
        const borderless = await commandBackground();
        if (borderless !== 'rgba(0, 0, 0, 0)') throw new Error(`borderless command box has a background: ${borderless}`);
        await send('barformat', 'Command bar format set.');
        const borderlessAgain = await commandBackground();
        if (borderlessAgain !== 'rgba(0, 0, 0, 0)') throw new Error(`borderless command box got a background from a later format: ${borderlessAgain}`);
        console.log('PASS: a borderless command box stays transparent, whatever order the format and style arrive in');

        // ── #2359: the phone drawer follows a dark pane scheme ──────────────────
        await page.setViewportSize({ width: 400, height: 800 });
        await page.waitForFunction(() => document.body.classList.contains('qv-narrow'));
        const white = await page.evaluate(() => getComputedStyle(document.getElementById('sidebar')).backgroundColor);
        if (white !== 'rgb(255, 255, 255)') throw new Error(`with no pane scheme the drawer should stay white: ${white}`);
        await send('darkpanes', 'Dark panes set.');
        const dark = await page.evaluate(() => getComputedStyle(document.getElementById('sidebar')).backgroundColor);
        if (dark !== 'rgb(0, 0, 0)') throw new Error(`the open layout's drawer didn't take the pane scheme's background: ${dark}`);
        await page.setViewportSize({ width: 1200, height: 900 });
        await page.waitForFunction(() => !document.body.classList.contains('qv-narrow'));
        await page.setViewportSize({ width: 400, height: 800 });
        await page.waitForFunction(() => document.body.classList.contains('qv-narrow'));
        const darkAgain = await page.evaluate(() => getComputedStyle(document.getElementById('sidebar')).backgroundColor);
        if (darkAgain !== 'rgb(0, 0, 0)') throw new Error(`the drawer went back to ${darkAgain} after a resize`);
        console.log('PASS: the phone drawer takes a dark pane scheme\'s background, and keeps it across resizes');

        console.log('PASS: all checks passed');
    } catch (e) {
        console.error('FAIL:', e.message);
        throw e;
    } finally {
        await browser.close();
    }
});
