// Regression/feature test for issue #2257: the editor never checked expressions for mismatched
// brackets or quotes before saving them, even though Quest 5's desktop editor and WebEditor both
// refused to save a broken expression and showed why (see EditorController.ValidateExpression,
// covered by EditorCoreTests but never wired up to WasmEditor/AppShell). ExpressionInput.svelte
// now calls the new WasmEditorBridge.ValidateExpression export live as the user types and refuses
// to commit an invalid expression, showing an inline error the same way AddElementModal's name
// field already does (see verify-rename-invalid-name.mjs for that precedent). Since every
// expression-shaped control (if conditions, expression-type script parameters, expression-type
// properties) renders through the single shared ExpressionInput component, this exercises the "if"
// condition case as the representative path.
//
// Requires the AppShell dev server running locally (WasmEditor Debug build first):
//   cd src/AppShell && npm run dev
// Run: node tests/e2e/verify-appshell-expression-validation.mjs [baseUrl]
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

// This command's Script panel has its own visual/code toggle (distinct from the toolbar's
// whole-game "Raw XML code view") - "Code view" switches the script in place to a CodeMirror
// editor, and "Visual editor" switches it back. No modal involved.
async function readRawCode(page) {
    await page.click('button:has-text("Code view")');
    const text = await page.locator('.cm-content').first().innerText();
    await page.click('button:has-text("Visual editor")');
    await page.waitForSelector('button:has-text("Code view")', { timeout: 5000 });
    return text;
}

try {
    const ctx = await browser.newContext({ viewport: { width: 1400, height: 1000 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Expression Validation Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // ── Build a Command with an If block, exercising the "if" condition's ExpressionInput ──
    await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Command to")', { timeout: 5000 });
    await page.waitForSelector('text=Command:', { timeout: 10000 });

    const patternRow = page.getByText('Pattern:', { exact: true }).locator('xpath=../..');
    const patternInput = patternRow.locator(':scope > input[type=text]');
    await patternInput.waitFor({ state: 'visible', timeout: 5000 });
    await patternInput.fill('test command');
    await patternInput.blur();

    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.getByRole('button', { name: 'If', exact: true }).click();
    await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });
    console.log('PASS: command with an If block created');

    const ifRow = page.locator('div.flex.items-center.gap-1.flex-wrap').filter({ hasText: 'then' }).first();
    // Freshly added, the "if" condition starts empty, which doesn't match any condition template
    // (see expressionField in ScriptEditor.svelte), so it renders the raw ExpressionInput field
    // directly rather than a template's own sub-controls.
    const exprInput = ifRow.locator('input[type=text]').first();
    await exprInput.waitFor({ state: 'visible', timeout: 5000 });

    // ── Mismatched brackets: must be rejected, not silently saved ──────────────────────────
    const badExpression = 'GetBoolean(game, "flag"';
    await exprInput.fill(badExpression);
    await exprInput.blur();

    const bracketError = page.locator('p.text-error-500', { hasText: /brackets/i });
    await bracketError.waitFor({ timeout: 5000 });
    console.log(`PASS: mismatched-brackets error shown inline: "${await bracketError.textContent()}"`);

    const borderClass = await exprInput.getAttribute('class');
    if (!borderClass?.includes('border-error-500')) throw new Error('Expected the expression input to get an error border');
    console.log('PASS: expression input gets an error border');

    const valueAfterBlur = await exprInput.inputValue();
    if (valueAfterBlur !== badExpression) throw new Error(`Expected the invalid text to remain in the field, got "${valueAfterBlur}"`);
    console.log('PASS: invalid expression text stays in the field (not reverted) so it can be fixed');

    const codeAfterBadExpr = await readRawCode(page);
    if (codeAfterBadExpr.includes(badExpression)) throw new Error('The invalid expression must not have been committed to the saved script');
    console.log('PASS: invalid expression was not committed to the underlying script');

    // ── Mismatched quotes: same rejection ───────────────────────────────────────────────────
    await exprInput.fill('"no end quote');
    await exprInput.blur();
    const quoteError = page.locator('p.text-error-500', { hasText: /quote/i });
    await quoteError.waitFor({ timeout: 5000 });
    console.log(`PASS: missing-quote error shown inline: "${await quoteError.textContent()}"`);

    // ── A valid expression is accepted once fixed, clearing the error (no regression) ──────
    const goodExpression = 'GetBoolean(game, "flag")';
    await exprInput.fill(goodExpression);
    await exprInput.blur();
    await page.waitForSelector('p.text-error-500', { state: 'detached', timeout: 5000 });
    console.log('PASS: error clears once the expression is fixed');

    const borderClassAfterFix = await exprInput.getAttribute('class');
    if (borderClassAfterFix?.includes('border-error-500')) throw new Error('Expected the error border to be removed once the expression is valid');
    console.log('PASS: error border removed once valid');

    const codeAfterGoodExpr = await readRawCode(page);
    if (!codeAfterGoodExpr.includes(goodExpression)) throw new Error(`Expected the saved script to contain the valid expression, got:\n${codeAfterGoodExpr}`);
    console.log('PASS: valid expression was committed to the underlying script');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/appshell-expression-validation-failure.png' });
    } catch { /* best-effort */ }
    process.exitCode = 1;
} finally {
    await browser.close();
}
