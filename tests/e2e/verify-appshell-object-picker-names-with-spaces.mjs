// An "objects" simple-editor script parameter (MakeExitInvisible, MakeObjectVisible, etc.) must
// stay in object/exit mode when the chosen element's name contains a space. ScriptEditor.svelte
// used to classify the stored value with an identifier-only regex, so `MakeExitInvisible (trap
// door)` was forced into expression mode, and switching back to "exit" cleared the name.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function addRoom(name) {
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Room")', { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

async function selectTreeNode(name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Picker Spaces Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    await addRoom('Room One');
    await addRoom('Room Two');

    await selectTreeNode('Room One');
    await page.click('button:has-text("Exits")');
    await page.getByRole('button', { name: 'north', exact: true }).click();
    const combobox = page.locator('[role="combobox"]');
    await combobox.click();
    await combobox.fill('Room Two');
    await page.waitForSelector('[role="option"]:has-text("Room Two")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("Room Two")');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=→ Room Two', { timeout: 10000 });
    await page.click('button[title="Edit exit"]');
    await page.waitForSelector('text=Name:', { timeout: 10000 });
    const nameField = page.locator('span:has-text("Name:")').locator('..').locator('input[type="text"]');
    await nameField.fill('trap door');
    await nameField.blur();
    await page.waitForSelector('[data-value="trap door"]', { timeout: 10000 });
    console.log('PASS: named the north exit "trap door"');

    await selectTreeNode('Room One');
    await page.waitForSelector('button:has-text("Script")', { timeout: 10000 });
    await page.click('button:has-text("Script")');
    await page.waitForSelector('button:has-text("Add script")', { timeout: 10000 });
    await page.click('button:has-text("Add script")');
    const addScriptDialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await addScriptDialog.waitFor({ timeout: 10000 });
    await addScriptDialog.getByRole('option', { name: 'Objects', exact: true }).click();
    await addScriptDialog.getByRole('option', { name: 'Make exit invisible' }).click();
    await addScriptDialog.getByRole('button', { name: 'OK', exact: true }).click();
    console.log('PASS: added a "Make exit invisible" script command');

    // The mode toggle is the only <select> offering "expression"; the exit picker is the
    // searchable Combobox (it disappears entirely once the row is in expression mode).
    const modeSelect = page.locator('select:has(option[value="expression"])').last();
    const exitPicker = page.locator('input[role="combobox"]').last();
    await exitPicker.click();
    await exitPicker.fill('trap');
    await exitPicker.press('Enter');

    // Selecting commits the value and re-renders the row from the stored script, which is where
    // a name with a space used to be reclassified as an expression.
    await page.waitForTimeout(500);
    const mode = await modeSelect.inputValue();
    if (mode === 'expression') {
        throw new Error('Expected the parameter to stay in exit mode after picking "trap door", but it switched to expression mode');
    }
    const picked = await exitPicker.inputValue();
    if (picked !== 'trap door') {
        throw new Error(`Expected the exit picker to show "trap door", got ${JSON.stringify(picked)}`);
    }
    console.log('PASS: a name containing a space stays selected in exit mode');

    // Round-trip through code view, the path the original report used to enter the command.
    await page.getByRole('button', { name: 'Code view', exact: true }).first().click();
    const code = page.locator('.cm-content').first();
    await code.waitFor({ timeout: 10000 });
    const codeText = await code.innerText();
    if (!codeText.includes('MakeExitInvisible (trap door)')) {
        throw new Error(`Expected code view to contain "MakeExitInvisible (trap door)", got ${JSON.stringify(codeText)}`);
    }
    await page.getByRole('button', { name: /^Visual (view|editor)$/ }).first().click();
    await page.waitForTimeout(500);
    const afterMode = await modeSelect.inputValue();
    const afterPick = await exitPicker.inputValue();
    if (afterMode === 'expression' || afterPick !== 'trap door') {
        throw new Error(`Expected exit mode with "trap door" after a code view round trip, got mode=${JSON.stringify(afterMode)} value=${JSON.stringify(afterPick)}`);
    }
    console.log('PASS: still in exit mode with "trap door" selected after a code view round trip');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-object-picker-names-with-spaces-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
