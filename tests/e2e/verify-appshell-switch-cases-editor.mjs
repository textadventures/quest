// Verifies the Switch script command's "Cases" control in ScriptEditor.svelte, which used to
// render nothing but the literal text "[scriptdictionary]" - see CoreEditorScriptsScripts.aslx's
// switch editor (controltype scriptdictionary, attribute "1"). Unlike Use/Give's scriptdictionary
// (an element attribute, see verify-appshell-multi-control-editors.mjs), switch's cases are a
// *script parameter* nested inside a script tree - SwitchScript.GetParameter(1) returns the same
// QuestDictionary<IScript> instance every call, so mutations must go through
// WasmEditorBridge.AddScriptDictCase/RemoveScriptDictCase/RenameScriptDictCase (which resolve the
// owning switch node via containerPath/scriptIndex, mirroring AddScriptListItem's precedent for
// FunctionCallScript's parameter list) rather than the element-attribute-only
// AddScriptDictionaryItem/RemoveScriptDictionaryItem/MakeScriptDictEditable trio. This script
// exercises add/edit/rename/remove of a case and the switch/case textual round trip into saved
// XML (switch script bodies are saved as the `switch (...) { case (...) { ... } }` mini-language
// inside a single <script> text node, not per-case XML elements - see SwitchScript.Save()).
//
// Requires the AppShell dev server running locally (WasmEditor Debug build first):
//   cd src/AppShell && npm run dev
// Run: node tests/e2e/verify-appshell-switch-cases-editor.mjs [baseUrl]
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

// Case-key inputs are plain <input value={key}> elements (Svelte sets the DOM property, not the
// HTML attribute), so a CSS `[value=...]` selector can't find them - look them up by their
// current value instead, scoped to the "Case key" aria-label so nested case-body inputs (e.g. a
// Print command's message box) and the "Add case" input are never candidates.
async function caseKeyInput(scope, key) {
    const inputs = scope.getByLabel('Case key');
    const count = await inputs.count();
    for (let i = 0; i < count; i++) {
        const candidate = inputs.nth(i);
        if (await candidate.inputValue() === key) {
            return candidate;
        }
    }
    return null;
}

async function readRawXml(page) {
    await page.click('button[title="Raw XML code view"]');
    const text = await page.locator('.cm-content').first().innerText();
    await page.click('button:has-text("Cancel")');
    await page.waitForSelector('button[title="Raw XML code view"]', { timeout: 5000 });
    return text;
}

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Switch Cases Editor Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // ── Add a Switch command to "room"'s "After entering the room" script ───────────
    await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();

    const afterEnteringSection = page.getByText('After entering the room:', { exact: true }).locator('xpath=..');
    await afterEnteringSection.getByRole('button', { name: 'Add script' }).click();
    await page.fill('input[placeholder="Filter commands..."]', 'Switch');
    await page.click('text=Switch...', { timeout: 5000 });
    await page.click('[role="dialog"] button:has-text("OK")');
    await page.waitForSelector('text=Switch:', { timeout: 10000 });
    console.log('PASS: Switch command added');

    // Scoped to the Switch row's own wrapper, matching verify-appshell-multi-control-editors.mjs's
    // approach for the analogous Use/Give scriptdictionary control.
    const switchRow = page.getByText('Switch:', { exact: true }).locator('xpath=../..');

    const switchExprInput = switchRow.locator('input[type=text]').first();
    await switchExprInput.fill('"gold"');
    await switchExprInput.blur();
    await page.waitForTimeout(300);
    console.log('PASS: switch expression set');

    // A bare "[scriptdictionary]" placeholder span would mean the render branch regressed - the
    // "Cases:" label and the literal control-type text sit in the same row.
    const placeholderRegression = await switchRow.getByText('[scriptdictionary]').count();
    if (placeholderRegression > 0) {
        throw new Error('Switch "Cases" control still renders the raw "[scriptdictionary]" placeholder - ScriptEditor.svelte is missing its render branch');
    }
    console.log('PASS: no raw "[scriptdictionary]" placeholder text rendered');

    const addCaseInput = switchRow.getByPlaceholder('Add case…');
    await addCaseInput.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: the Cases editor (switchCasesEditor) renders an "Add case" control');

    // ── Add a case and a nested script inside it ─────────────────────────────────────
    await addCaseInput.fill('"sword"');
    await switchRow.getByRole('button', { name: 'Add', exact: true }).click();
    await page.waitForTimeout(300);

    // The case row's own key input holds the literal value '"sword"' - locate it precisely by value.
    const swordKeyInput = await caseKeyInput(switchRow, '"sword"');
    if (!swordKeyInput) throw new Error('"sword" case key input not found after adding it');
    console.log('PASS: "sword" case added and its key input renders');

    // Newly-added cases auto-expand, exposing the nested ScriptEditor's own "+ Add script".
    const caseBody = swordKeyInput.locator('xpath=ancestor::div[contains(@class,"border")][1]');
    await caseBody.getByRole('button', { name: 'Add script' }).click();
    await page.fill('input[placeholder="Filter commands..."]', 'Print a message');
    await page.click('text=Print a message', { timeout: 5000 });
    await page.click('[role="dialog"] button:has-text("OK")');
    await page.waitForTimeout(300);

    // Print's message field renders as a <textarea> (multiline, see ScriptEditor.svelte's
    // `ctrl.multiline` branch), not an `input[type=text]` - match both so `.last()` finds it
    // rather than falling back to the case's own key input.
    const messageInput = caseBody.locator('input[type=text], textarea').last();
    await messageInput.fill('You found a sword!');
    await messageInput.blur();
    await page.waitForTimeout(300);
    console.log('PASS: nested script inside the case body was added and edited');

    // ── Add a second case, then rename it ────────────────────────────────────────────
    await addCaseInput.fill('"shield"');
    await switchRow.getByRole('button', { name: 'Add', exact: true }).click();
    await page.waitForTimeout(300);

    const shieldKeyInput = await caseKeyInput(switchRow, '"shield"');
    if (!shieldKeyInput) throw new Error('"shield" case key input not found after adding it');
    await shieldKeyInput.fill('"buckler"');
    await shieldKeyInput.blur();
    await page.waitForTimeout(300);

    const bucklerKeyInput = await caseKeyInput(switchRow, '"buckler"');
    if (!bucklerKeyInput) throw new Error('Renaming "shield" to "buckler" did not stick in the UI');
    console.log('PASS: second case added and renamed from "shield" to "buckler"');

    let xml = await readRawXml(page);
    if (!xml.includes('switch ("gold")')) {
        throw new Error(`Expected the switch expression to round-trip, got: ${xml.slice(0, 1200)}`);
    }
    if (!xml.includes('case ("sword")') || !xml.includes('msg ("You found a sword!")')) {
        throw new Error(`Expected the "sword" case with its nested msg script, got: ${xml.slice(0, 1200)}`);
    }
    if (!xml.includes('case ("buckler")')) {
        throw new Error(`Expected the renamed "buckler" case, got: ${xml.slice(0, 1200)}`);
    }
    if (xml.includes('case ("shield")')) {
        throw new Error(`Renaming "shield" to "buckler" should not have left the old key behind, got: ${xml.slice(0, 1200)}`);
    }
    console.log('PASS: switch/case script round-trips correctly into saved XML');

    // readRawXml's "Cancel" navigates back to the Setup tab, so re-open Scripts before
    // continuing to interact with the switch row.
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    await page.waitForSelector('text=Switch:', { timeout: 10000 });

    // ── Remove the "buckler" case ────────────────────────────────────────────────────
    const bucklerCaseRowAfterReload = await caseKeyInput(switchRow, '"buckler"');
    if (!bucklerCaseRowAfterReload) throw new Error('"buckler" case not found after returning to the Scripts tab');
    const bucklerCaseRow = bucklerCaseRowAfterReload.locator('xpath=..');
    await bucklerCaseRow.locator('button', { hasText: '×' }).click();
    await page.waitForTimeout(300);

    const bucklerStillThere = await caseKeyInput(switchRow, '"buckler"');
    if (bucklerStillThere) {
        throw new Error('Removing the "buckler" case did not remove its row from the UI');
    }
    console.log('PASS: "buckler" case removed from the UI');

    xml = await readRawXml(page);
    if (xml.includes('case ("buckler")')) {
        throw new Error(`Expected "buckler" case to be gone from saved XML after removal, got: ${xml.slice(0, 1200)}`);
    }
    if (!xml.includes('case ("sword")')) {
        throw new Error(`Expected the "sword" case to survive removing the other case, got: ${xml.slice(0, 1200)}`);
    }
    console.log('PASS: case removal round-trips into saved XML');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/switch-cases-editor-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
