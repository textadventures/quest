// Verifies two "multi" attribute-editor controls in PropertyEditor.svelte that used to
// render nothing at all because their subEditorType had no matching branch in the
// controlRow/controlOnly if/else chains - see CoreEditorCommand.aslx's Command "Pattern"
// control (subEditorType "simplepattern") and CoreEditorObjectUseGive.aslx's "Handle objects
// individually" controls (subEditorType "scriptdictionary"). Both are handled by matching the
// raw type-key directly (no aslx <editors> override needed) - the old WPF/ASP.NET editors had
// a hardcoded default type->widget map covering these standard types (see
// EditorControls/MultiControl.xaml.cs / WebEditor's ControlHelpers.cs on the v5 tags) that
// PropertyEditor.svelte has no equivalent of; it only recognises a fixed set of literal
// subEditorType strings. Fixing the missing render branches alone wasn't enough: the actual
// mode-switch handler, WasmEditorBridge.SetMultiType, also had no case for either
// "scriptdictionary" or "simplepattern" (silently no-opped via its `default` branch), and
// AddDropdownTypeValues' typeof-resolution switch had no case mapping
// IEditableDictionary<IEditableScripts> back to the "scriptdictionary" label - so even
// after SetMultiType created the underlying value, the mode dropdown kept reporting "None"
// and the newly-rendered branch never engaged. This script exercises the full round trip
// for both controls so a regression in any one of those three layers fails here.
//
// Requires the AppShell dev server running locally (WasmEditor Debug build first):
//   cd src/AppShell && npm run dev
// Run: node tests/e2e/verify-appshell-multi-control-editors.mjs [baseUrl]
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

// A leaf's row is [data-part="item"], a branch's own row is [data-part="branch-control"]
// (same disambiguation as verify-appshell-exits-editor.mjs's selectTreeNode).
async function selectTreeNode(page, name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

async function addViaMenu(page, buttonText, name) {
    await page.click('button[title="Add element"]');
    await page.click(`button:has-text("${buttonText}")`, { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
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
    await page.fill('input[placeholder="Game name"]', `Multi Control Editors Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // ── Command "Pattern" control (simplepattern / string) ──────────────────────────
    // Unlike "Add Object", "Add Command" creates the element immediately with no name modal.
    await selectTreeNode(page, 'room');
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Command to")', { timeout: 5000 });
    await page.waitForSelector('text=Command:', { timeout: 10000 });

    // Scoped tightly to the Pattern row's own wrapper (two levels up from the "Pattern:" label:
    // span -> label/select row -> flex-col wrapper) and to its *direct* children only - a plain
    // input[type=text].first() would instead match the sidebar's "Filter game objects" textbox,
    // and an unscoped descendant search would also catch the "Unresolved object text" control
    // that shares the same outer flex-col ancestor.
    const patternRow = page.getByText('Pattern:', { exact: true }).locator('xpath=../..');
    const patternModeSelect = patternRow.locator(':scope > select, :scope > div > select').first();
    await patternModeSelect.waitFor({ state: 'visible', timeout: 5000 });
    const patternInput = patternRow.locator(':scope > input[type=text]');
    await patternInput.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: Pattern textbox renders in "Command pattern" (simplepattern) mode');

    await patternInput.fill('say #text#');
    await patternInput.blur();
    await page.waitForTimeout(300);

    const modeAfterTyping = await patternModeSelect.inputValue();
    if (modeAfterTyping !== 'simplepattern') {
        throw new Error(`Typing into the Pattern field must not change its mode - got "${modeAfterTyping}", expected "simplepattern"`);
    }
    console.log('PASS: typing a simple pattern does not reclassify it as a regex');

    let xml = await readRawXml(page);
    if (!xml.includes('<pattern>say #text#</pattern>')) {
        throw new Error(`Expected a plain <pattern>say #text#</pattern> element in saved XML, got: ${xml.slice(0, 800)}`);
    }
    console.log('PASS: simple pattern round-trips as a plain <pattern> element (not type="string")');

    // Switch to "Regular expression" mode and back, exercising SetMultiType's
    // "string" and "simplepattern" cases both ways.
    await patternModeSelect.selectOption('string');
    await page.waitForTimeout(200);
    await patternInput.fill('^say (.*)$');
    await patternInput.blur();
    await page.waitForTimeout(300);
    const modeAfterRegex = await patternModeSelect.inputValue();
    if (modeAfterRegex !== 'string') throw new Error('Switching to "Regular expression" did not stick');
    console.log('PASS: "Regular expression" mode works and persists');

    await patternModeSelect.selectOption('simplepattern');
    await page.waitForTimeout(200);
    const modeAfterSwitchBack = await patternModeSelect.inputValue();
    if (modeAfterSwitchBack !== 'simplepattern') {
        throw new Error('Switching back to "Command pattern" from "Regular expression" did not stick (SetMultiType missing "simplepattern" case)');
    }
    console.log('PASS: switching back to "Command pattern" mode works');

    // ── Use/Give "Handle objects individually" control (scriptdictionary) ───────────
    await selectTreeNode(page, 'room');
    await addViaMenu(page, 'Add Object in', 'defibrillator');
    await selectTreeNode(page, 'room');
    await addViaMenu(page, 'Add Object in', 'patient');

    await selectTreeNode(page, 'patient');
    await page.getByRole('button', { name: 'Features', exact: true }).click();
    await page.getByText(/Use\/Give:/).click();
    await page.getByRole('button', { name: 'Use/Give', exact: true }).click();

    const useonSelect = page.locator('select').filter({ hasText: 'None' }).first();
    await useonSelect.waitFor({ state: 'visible', timeout: 5000 });
    await useonSelect.selectOption('scriptdictionary');
    await page.waitForTimeout(300);

    const modeAfterScriptDict = await useonSelect.inputValue();
    if (modeAfterScriptDict !== 'scriptdictionary') {
        throw new Error('Selecting "Handle objects individually" did not stick (SetMultiType missing "scriptdictionary" case, or AddDropdownTypeValues missing its typeof mapping)');
    }
    console.log('PASS: "Handle objects individually" mode is selected and persists');

    // Scoped to the useon control's own wrapper - an unscoped 'button:has-text("Add")' would
    // instead hit the toolbar's "+ Add" (element) button, which precedes this in the DOM, and an
    // unscoped 'defibrillator' text match would hit the tree sidebar's own "defibrillator" node.
    const useonSection = useonSelect.locator('xpath=../..');
    const objectPicker = useonSection.locator('select').filter({ hasText: /Select object/ }).first();
    await objectPicker.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: the per-object list (ScriptDictionaryEditor) renders once scriptdictionary mode is active');

    await objectPicker.selectOption({ label: 'defibrillator' });
    await useonSection.locator('button:has-text("Add")').click();
    await page.waitForTimeout(300);

    // The key renders as a plain text node inside the expand/collapse <button>, not its own span.
    const entry = useonSection.locator('button', { hasText: 'defibrillator' });
    await entry.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "defibrillator" entry added to the per-object list');

    xml = await readRawXml(page);
    if (!xml.includes('<useon type="scriptdictionary">') || !xml.includes('key="defibrillator"')) {
        throw new Error(`Expected <useon type="scriptdictionary"> with a defibrillator item in saved XML, got: ${xml.slice(0, 1000)}`);
    }
    console.log('PASS: scriptdictionary entry round-trips into saved XML');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/multi-control-editors-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
