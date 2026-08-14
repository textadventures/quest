// Regression test for user-reported feedback: ScriptEditor's per-row
// move-up/move-down/delete buttons used to be forced always-visible on a
// coarse pointer for every row. Every row (root AND nested if/for/while
// blocks) now has a checkbox + selection toolbar (Cut/Copy/Delete/Move) as
// the touch-friendly reorder/delete path, so the row's own buttons stay
// hover-only at every nesting level — even on touch, where there's no hover
// and the checkbox/toolbar cover it.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    // isMobile (not just hasTouch) is needed for Chromium to actually report
    // (pointer: coarse) via matchMedia — see verify-appshell-touch-affordances.mjs.
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
    const rootOpacity = await rootMoveButton.evaluate(el => getComputedStyle(el.parentElement).opacity);
    if (rootOpacity !== '0') throw new Error(`Root-level row buttons should not be always-visible on touch (checkbox + toolbar already cover this) — computed opacity was ${rootOpacity}`);
    console.log('PASS: root-level row buttons are not forced visible (checkbox/toolbar cover it instead)');

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
    // "ml-4 border-l ..."), root otherwise. Every row — root or nested — must
    // have a checkbox and hover-only (opacity 0) row actions.
    const rows = await page.evaluate(() => {
        const results = [];
        for (const actions of document.querySelectorAll('div.absolute.top-1')) {
            const row = actions.closest('div.group.relative');
            if (!row) continue;
            const isNested = !!row.closest('div.ml-4.border-l.border-surface-300-700.pl-2');
            results.push({ isNested, opacity: getComputedStyle(actions).opacity, hasCheckbox: !!row.querySelector('input[type="checkbox"]') });
        }
        return results;
    });
    const rootRows = rows.filter(r => !r.isNested);
    const nestedRows = rows.filter(r => r.isNested);
    if (nestedRows.length === 0) throw new Error('Expected at least one nested (non-root) script row after adding an If block with a nested Print command');
    if (rows.some(r => !r.hasCheckbox)) throw new Error(`Every row — root and nested — should have a selection checkbox: ${JSON.stringify(rows)}`);
    if (rows.some(r => r.opacity !== '0')) throw new Error(`All rows should have hover-only row buttons (opacity 0, checkbox + toolbar cover touch): ${JSON.stringify(rows)}`);
    console.log(`PASS: ${rootRows.length} root row(s) and ${nestedRows.length} nested row(s) all have checkboxes and hover-only row buttons`);

    // The nested row's checkbox must also reveal the selection toolbar (the fix
    // that gave nested blocks their own checkbox + Cut/Copy/Delete/Move).
    await page.locator('div.ml-4.border-l.border-surface-300-700.pl-2 input[type="checkbox"]').first().click();
    await page.waitForSelector('text=selected', { timeout: 5000 });
    console.log('PASS: checking a nested row reveals the Cut/Copy/Delete/Move selection toolbar');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
