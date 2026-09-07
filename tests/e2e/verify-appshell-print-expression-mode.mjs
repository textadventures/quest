// Regression test for a bug found while regenerating docs screenshots: ScriptEditor.svelte's
// isSimpleValue() classified a Print command's message field as "simple" (plain quoted text)
// whenever the raw value merely started and ended with a `"`, even when the middle contained an
// unescaped expression like `"a" + b + "c"`. Any mutate()-triggering edit anywhere in the script
// tree (typing into the field itself, or a structural edit like adding an "else" branch) calls
// refresh(), which unconditionally clears expressionOverrides — so the mode dropdown then
// re-derives "simple" from isSimpleValue() and silently flips back from "expression" to
// "message", and toSimpleDisplay()'s slice(1, -1) mangles the display into garbled literal text
// (the outer quotes stripped, the `+ object.weight +` left as literal characters). The
// underlying saved script text was never actually corrupted by this — GetScriptCode/raw XML
// round-tripped the real expression correctly the whole time — but the display bug made it look
// like the expression had been lost, which is what this test guards against. See
// isSimpleValue's default case in ScriptEditor.svelte.
//
// Requires the AppShell dev server running locally (WasmEditor Debug build first):
//   cd src/AppShell && npm run dev
// Run: node tests/e2e/verify-appshell-print-expression-mode.mjs [baseUrl]
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1400, height: 1000 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Print Expression Mode Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // ── Build a Command with a nested If > Print, matching the reported repro ──────
    await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Command to")', { timeout: 5000 });
    await page.waitForSelector('text=Command:', { timeout: 10000 });

    const patternRow = page.getByText('Pattern:', { exact: true }).locator('xpath=../..');
    const patternInput = patternRow.locator(':scope > input[type=text]');
    await patternInput.waitFor({ state: 'visible', timeout: 5000 });
    await patternInput.fill('weigh #object#');
    await patternInput.blur();

    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.getByRole('button', { name: 'If', exact: true }).click();
    await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });
    console.log('PASS: command with an If block created');

    // Condition doesn't matter for this bug, but "object has attribute" matches the report and
    // exercises the same nested-block depth.
    const ifRow = page.locator('div.flex.items-center.gap-1.flex-wrap').filter({ hasText: 'then' }).first();
    const condSelect = ifRow.locator('select').first();
    await condSelect.selectOption({ label: 'object has attribute' });
    await page.waitForTimeout(200);
    const objectInput = ifRow.locator('input[type=text]').first();
    await objectInput.fill('object');
    await objectInput.blur();
    const attrNameInput = ifRow.locator('input[placeholder="name"]');
    await attrNameInput.fill('weight');
    await attrNameInput.blur();
    await page.waitForTimeout(200);
    console.log('PASS: "object has attribute" condition filled in');

    // Add "Print a message" inside the "then" branch — the nested "+ Add script" is first in DOM
    // order (see verify-appshell-scripteditor-row-buttons.mjs).
    await page.locator('button:has-text("Add script")').first().click();
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.getByRole('button', { name: 'Print', exact: true }).click();
    await page.waitForTimeout(300);
    console.log('PASS: Print added inside the then-branch');

    const printRow = page.getByText('Print', { exact: true }).locator('xpath=..');
    const printModeSelect = printRow.locator('select').last();

    // Switch to expression mode and type a concatenation expression whose *value* happens to
    // both start and end with `"` — exactly the shape that fooled the old isSimpleValue() check.
    await printModeSelect.selectOption('expression');
    await page.waitForTimeout(200);
    const exprText = '"It weighs " + object.weight + " grams."';
    const exprInput = printRow.locator('input[type=text]').last();
    await exprInput.fill(exprText);
    await exprInput.blur();
    await page.waitForTimeout(300);

    const modeAfterTyping = await printRow.locator('select').last().inputValue();
    if (modeAfterTyping !== 'expression') {
        throw new Error(`Typing a "a" + b + "c"-shaped expression into Print's message field must not flip its mode back to "message" — got "${modeAfterTyping}"`);
    }
    const valueAfterTyping = await printRow.locator('input[type=text]').last().inputValue();
    if (valueAfterTyping !== exprText) {
        throw new Error(`Print's expression field must keep showing the typed expression verbatim — got "${valueAfterTyping}"`);
    }
    console.log('PASS: typing a quote-bracketed expression keeps the field in expression mode with the exact text');

    // A structural mutation elsewhere in the same script tree (refresh()'s trigger in the
    // original bug report) must not disturb it either.
    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await page.waitForTimeout(500);

    const modeAfterElse = await printRow.locator('select').last().inputValue();
    if (modeAfterElse !== 'expression') {
        throw new Error(`Adding an "else" branch elsewhere in the script must not revert Print's field to "message" mode — got "${modeAfterElse}"`);
    }
    const valueAfterElse = await printRow.locator('input[type=text]').last().inputValue();
    if (valueAfterElse !== exprText) {
        throw new Error(`Adding an "else" branch must not mangle Print's expression text — got "${valueAfterElse}"`);
    }
    console.log('PASS: adding an else branch elsewhere does not revert or mangle the Print expression');

    // The autosave chip must still reach "Saved".
    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });
    console.log('PASS: autosave completes ("Saved" chip shown) after the edits');

    // And the underlying saved script text must be the real expression, not a mangled literal.
    await page.click('button:has-text("Code view")');
    const rawCode = await page.locator('.cm-content').first().innerText();
    if (!rawCode.includes('msg ("It weighs " + object.weight + " grams.")')) {
        throw new Error(`Expected the saved script to contain the real Print expression, got:\n${rawCode}`);
    }
    console.log('PASS: saved script code contains the real (unmangled) Print expression');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/print-expression-mode-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
