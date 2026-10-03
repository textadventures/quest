// Regression test for issue #2343: the "player's choice from a menu" expression template
// (ShowMenu(#caption#, #options#, #allowcancel#)) split its parameters on every comma, including
// the one inside a nested Split("...", ";"), so the Visual editor showed three broken fields each
// flagged with a mismatched-brackets error. Template parameters now only match bracket-balanced
// text (see SimplePatternLoader.LoadEditorExpression), so each field gets the right argument.
//
// Requires the AppShell dev server running locally (WasmEditor Debug build first):
//   cd src/AppShell && npm run dev
// Run: node tests/e2e/verify-appshell-expression-template-nested-commas.mjs [baseUrl]
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Nested Commas Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Command to")', { timeout: 5000 });
    await page.waitForSelector('text=Command:', { timeout: 10000 });

    // Enter the script through the per-script Code view, which commits on blur when we switch
    // back to the Visual editor.
    await page.click('button:has-text("Code view")');
    const code = page.locator('.cm-content').first();
    await code.click();
    await page.keyboard.insertText('player.class = ShowMenu("Your character class?", Split("Warrior;Wizard;Priest;Thief", ";"), false)');
    await page.click('button:has-text("Visual editor")');
    await page.waitForSelector('button:has-text("Code view")', { timeout: 5000 });
    console.log('PASS: ShowMenu set-variable script entered via code view');

    const templateSelect = page.locator('select').filter({ has: page.locator('option:checked', { hasText: /choice from a menu/i }) });
    await templateSelect.first().waitFor({ timeout: 5000 });
    console.log('PASS: the "player\'s choice from a menu" template is selected');

    const values = await page.locator('input[type=text]').evaluateAll(els => els.map(e => e.value));
    // The caption is a plain string literal, so it's shown in its simple (unquoted) text mode.
    for (const expected of ['Your character class?', 'Split("Warrior;Wizard;Priest;Thief", ";")']) {
        if (!values.some(v => v.trim() === expected)) {
            throw new Error(`Expected a template field containing ${expected}, got fields: ${JSON.stringify(values)}`);
        }
    }
    console.log('PASS: caption and options fields hold the full arguments');

    const brokenFragment = values.find(v => v.trim() === '";")' || v.includes('?", Split('));
    if (brokenFragment !== undefined) throw new Error(`Found a mis-split field: ${brokenFragment}`);

    const errors = await page.locator('p.text-error-500').count();
    if (errors !== 0) throw new Error(`Expected no field errors, found ${errors}: ${await page.locator('p.text-error-500').allTextContents()}`);
    console.log('PASS: no bracket-mismatch errors shown');

    // Each ShowMenu parameter has <breakbefore/>, so caption, options and allow cancel each start
    // a new line below the template picker rather than running on in one long row.
    const top = async locator => (await locator.boundingBox()).y;
    const inputTop = async predicate => {
        const index = await page.locator('input[type=text]').evaluateAll(
            (els, src) => els.findIndex(new Function('e', `return ${src}`)), predicate);
        return top(page.locator('input[type=text]').nth(index));
    };
    const tops = {
        picker: await top(templateSelect.first()),
        caption: await inputTop(`e.value.trim() === 'Your character class?'`),
        options: await inputTop(`e.value.trim().startsWith('Split(')`),
        allowCancel: await top(page.getByRole('combobox', { name: 'allow cancel', exact: true })),
    };
    if (!(tops.picker < tops.caption && tops.caption < tops.options && tops.options < tops.allowCancel)) {
        throw new Error(`Expected picker, caption, options and allow cancel on successive lines, got tops ${JSON.stringify(tops)}`);
    }
    console.log('PASS: template parameters each start on a new line');

    // options has no simple (plain text) mode, since ShowMenu needs a list - so it's labelled
    // "options" and has no simple/expression toggle, which would otherwise read "expression".
    const optionsLabel = page.getByText('options', { exact: true });
    if (!await optionsLabel.isVisible()) throw new Error('Expected a visible "options" label');
    if (Math.abs(await top(optionsLabel) - tops.options) > 8) {
        throw new Error('Expected the "options" label on the same line as the options field');
    }
    const expressionToggles = await page.locator('select').evaluateAll(els => els.filter(e => e.value === 'expression').length);
    if (expressionToggles !== 0) throw new Error(`Expected no field showing an "expression" toggle, found ${expressionToggles}`);
    console.log('PASS: options is labelled and has no simple/expression toggle');

    // allowcancel is matched without the space after its comma, so the yes/no dropdown
    // recognises it rather than falling back to an expression box holding " false".
    // It's also labelled with the parameter's name, since its options are only yes/no/expression.
    const allowCancel = page.getByRole('combobox', { name: 'allow cancel', exact: true });
    if (await allowCancel.count() !== 1) throw new Error(`Expected one dropdown labelled "allow cancel", found ${await allowCancel.count()}`);
    if (!await page.getByText('allow cancel', { exact: true }).isVisible()) throw new Error('Expected a visible "allow cancel" label');
    const allowCancelValue = await allowCancel.inputValue();
    if (allowCancelValue !== 'false') throw new Error(`Expected the allow-cancel dropdown to show "false", got "${allowCancelValue}"`);
    console.log('PASS: allow-cancel shown as the yes/no dropdown, set to No');

    // Changing a field rebuilds the expression from the template pattern, keeping its spacing
    // and the other fields' values intact.
    await allowCancel.selectOption('true');
    await page.click('button:has-text("Code view")');
    const rebuilt = (await page.locator('.cm-content').first().innerText()).trim();
    const expectedCode = 'player.class = ShowMenu("Your character class?", Split("Warrior;Wizard;Priest;Thief", ";"), true)';
    if (rebuilt !== expectedCode) throw new Error(`Expected rebuilt code:\n  ${expectedCode}\ngot:\n  ${rebuilt}`);
    console.log('PASS: changing allow-cancel rebuilds the expression correctly');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/appshell-expression-template-nested-commas-failure.png' });
    } catch { /* best-effort */ }
    process.exitCode = 1;
} finally {
    await browser.close();
}
