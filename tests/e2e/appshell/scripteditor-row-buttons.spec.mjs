// ScriptEditor's per-row action buttons on touch. The row's move-up/move-down/
// delete buttons used to be forced always-visible on a coarse pointer for every
// row, which crowded every row with three buttons. Now, on touch, each row (root
// AND nested if/for/while blocks) shows just its ⋯ menu, always, which holds every
// row action (add/paste above or below, move, help, delete); ↑/↓/× stay hover-only
// on a desktop pointer and are hidden on touch. Every row also keeps its checkbox +
// selection toolbar for multi-row actions.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('scripteditor-row-buttons', async ({ baseURL }) => {
    const baseUrl = baseURL;

    const browser = await chromium.launch();

    try {
        // isMobile (not just hasTouch) is needed for Chromium to actually report
        // (pointer: coarse) via matchMedia — see touch-affordances.spec.mjs.
        const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true, isMobile: true });
        const page = await mobileCtx.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));
        page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

        await page.goto(`${baseUrl}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `ScriptEditor Row Buttons Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 30000 });

        await page.click('text=room');
        await page.getByRole('button', { name: /^Scripts$/ }).click();
        await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });

        // Add a top-level "Print a message" command (root row) via the quick-add shortcut.
        await page.click('button:has-text("Add script")');
        await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
        await page.getByRole('button', { name: 'Print', exact: true }).click();
        await page.waitForSelector('input[type="checkbox"]', { timeout: 5000 });
        console.log('PASS: added a root-level script command');

        // Scoped to ScriptEditor's own row-actions wrapper (div.absolute.top-1) —
        // ElementsList (the child-object list further down the same tab) also has
        // hover-reveal reorder buttons titled "Move up", positioned via top-1/2
        // instead, which a plain `button[title="Move up"]` would also match.
        // Playwright's isVisible() doesn't consider opacity:0 hidden (the element
        // is still in-flow, just transparent), so check computed opacity directly.
        const rootMoveButton = page.locator('div.absolute.top-1 button[title="Move up"]').first();
        const rootMenuButton = page.locator('div.absolute.top-1 button[aria-label="More actions"]').first();
        const rootState = await rootMoveButton.evaluate(el => ({
            clusterOpacity: getComputedStyle(el.parentElement).opacity,
            moveDisplay: getComputedStyle(el).display,
        }));
        if (rootState.clusterOpacity !== '1') throw new Error(`On touch a row's ⋯ should always be visible — cluster opacity was ${rootState.clusterOpacity}`);
        if (rootState.moveDisplay !== 'none') throw new Error(`On touch a row's ↑/↓/× should be hidden (the ⋯ menu holds them) — ↑ display was ${rootState.moveDisplay}`);
        if (!(await rootMenuButton.isVisible())) throw new Error('On touch a row\'s ⋯ button should be visible');
        console.log('PASS: on touch, a root row shows only its ⋯ menu button');

        // The ⋯ menu holds move and delete, the actions the hidden buttons did.
        await rootMenuButton.click();
        for (const name of ['Move up', 'Move down', 'Delete', 'Add script above', 'Add script below']) {
            if (await page.getByRole('menuitem', { name, exact: true }).count() !== 1) throw new Error(`The ⋯ menu should have a "${name}" item`);
        }
        await page.keyboard.press('Escape');
        console.log('PASS: the ⋯ menu holds add, move and delete');

        // Selecting the row's checkbox reveals the equivalent selection toolbar.
        await page.click('input[type="checkbox"]');
        await page.waitForSelector('text=selected', { timeout: 5000 });
        console.log('PASS: checking a root row reveals the Cut/Copy/Delete/Move selection toolbar');
        // Deselect again so the toolbar state doesn't leak into the nested-row check below.
        await page.click('input[type="checkbox"]');

        // Add an "If" block. Nested (non-root) rows have their own checkbox + selection
        // toolbar too (they're not forced always-visible on touch).
        await page.click('button:has-text("Add script")');
        await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
        await page.getByRole('button', { name: 'If', exact: true }).click();
        await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });
        // The if-block's nested "then" branch renders its own "+ Add script" first
        // in DOM order, ahead of that same root slot's *own* continuation add
        // button (root script lists can hold more commands after an if-block) and
        // every other slot's — so .first() is the nested one, not .last().
        const nestedAddButtons = page.locator('button:has-text("Add script")');
        await nestedAddButtons.first().click();
        await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
        await page.getByRole('button', { name: 'Print', exact: true }).click();
        await page.waitForTimeout(300);

        // Rather than guess DOM ordinal position among a mix of root and nested
        // rows, classify every ScriptEditor row: a row is nested if it lives
        // inside a nested ScriptEditor wrapper (the depth>0 indentClass —
        // "border-l border-surface-300-700 pl-2"), root otherwise. Every row — root or nested — must
        // have a checkbox and an always-visible ⋯ with its ↑/↓/× hidden.
        const rows = await page.evaluate(() => {
            const results = [];
            for (const actions of document.querySelectorAll('div.absolute.top-1')) {
                const row = actions.closest('div.group.relative');
                if (!row) continue;
                const isNested = !!row.closest('div.border-l.border-surface-300-700.pl-2');
                const move = actions.querySelector('button[title="Move up"]');
                results.push({
                    isNested,
                    opacity: getComputedStyle(actions).opacity,
                    moveHidden: !!move && getComputedStyle(move).display === 'none',
                    hasMenu: !!actions.querySelector('button[aria-label="More actions"]'),
                    hasCheckbox: !!row.querySelector('input[type="checkbox"]'),
                });
            }
            return results;
        });
        const rootRows = rows.filter(r => !r.isNested);
        const nestedRows = rows.filter(r => r.isNested);
        if (nestedRows.length === 0) throw new Error('Expected at least one nested (non-root) script row after adding an If block with a nested Print command');
        if (rows.some(r => !r.hasCheckbox)) throw new Error(`Every row — root and nested — should have a selection checkbox: ${JSON.stringify(rows)}`);
        if (rows.some(r => r.opacity !== '1' || !r.moveHidden || !r.hasMenu)) throw new Error(`On touch every row should show just its ⋯ (opacity 1, ↑/↓/× hidden): ${JSON.stringify(rows)}`);
        console.log(`PASS: ${rootRows.length} root row(s) and ${nestedRows.length} nested row(s) all have checkboxes and just a ⋯ on touch`);

        // The nested row's checkbox must also reveal the selection toolbar (the fix
        // that gave nested blocks their own checkbox + Cut/Copy/Delete/Move).
        await page.locator('div.border-l.border-surface-300-700.pl-2 input[type="checkbox"]').first().click();
        await page.waitForSelector('text=selected', { timeout: 5000 });
        console.log('PASS: checking a nested row reveals the Cut/Copy/Delete/Move selection toolbar');

        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
