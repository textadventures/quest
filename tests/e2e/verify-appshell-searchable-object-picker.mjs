// The object picker on a script command parameter (e.g. "Make object visible") is a searchable
// Combobox in strict mode: names are listed alphabetically, typing filters them, and typed text
// only commits when it names an object, so a partial or misspelt name can't become a broken
// object reference.
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
    await page.waitForSelector(`[data-value="${name}"]`, { timeout: 10000 });
}

async function selectTreeNode(name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Searchable Picker Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // Created out of alphabetical order, so creation order and sorted order differ.
    await addRoom('zebra');
    await addRoom('apple');
    await addRoom('mango');

    await selectTreeNode('room');
    await page.click('button:has-text("Scripts")');
    await page.click('button:has-text("Add script")');
    const addScriptDialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await addScriptDialog.waitFor({ timeout: 10000 });
    await addScriptDialog.getByRole('button', { name: 'Show', exact: true }).click();
    const picker = page.locator('input[role="combobox"]').last();
    await picker.waitFor({ timeout: 10000 });
    const modeSelect = page.locator('select:has(option[value="expression"])').last();
    console.log('PASS: added a "Make object visible" script with a searchable picker');

    await picker.click();
    const listbox = page.locator('[role="listbox"]').last();
    await listbox.waitFor({ timeout: 5000 });
    const names = (await listbox.locator('[role="option"]').allTextContents())
        .map(s => s.trim())
        .filter(s => ['zebra', 'apple', 'mango'].includes(s));
    if (JSON.stringify(names) !== JSON.stringify(['apple', 'mango', 'zebra'])) {
        throw new Error(`Expected picker names in alphabetical order, got ${JSON.stringify(names)}`);
    }
    console.log('PASS: picker lists object names alphabetically');

    await picker.fill('ang');
    const filtered = (await listbox.locator('[role="option"]').allTextContents()).map(s => s.trim());
    if (JSON.stringify(filtered) !== JSON.stringify(['mango'])) {
        throw new Error(`Expected typing "ang" to filter the list to ["mango"], got ${JSON.stringify(filtered)}`);
    }
    await picker.press('Enter');
    await page.waitForTimeout(300);
    if (await picker.inputValue() !== 'mango' || await modeSelect.inputValue() === 'expression') {
        throw new Error(`Expected "mango" selected in object mode, got value=${JSON.stringify(await picker.inputValue())} mode=${JSON.stringify(await modeSelect.inputValue())}`);
    }
    console.log('PASS: typing part of a name filters the list and Enter picks the match');

    await picker.click();
    await picker.fill('nosuchthing');
    await picker.press('Tab');
    await page.waitForTimeout(300);
    if (await picker.inputValue() !== 'mango' || await modeSelect.inputValue() === 'expression') {
        throw new Error(`Expected an unknown name to revert to "mango" in object mode, got value=${JSON.stringify(await picker.inputValue())} mode=${JSON.stringify(await modeSelect.inputValue())}`);
    }
    console.log('PASS: typing a name that matches no object reverts instead of committing it');

    await picker.click();
    await picker.fill('APPLE');
    await picker.press('Tab');
    await page.waitForTimeout(300);
    if (await picker.inputValue() !== 'apple') {
        throw new Error(`Expected "APPLE" to resolve to "apple", got ${JSON.stringify(await picker.inputValue())}`);
    }
    console.log('PASS: a full name typed in a different case resolves to the object');

    await page.getByRole('button', { name: 'Code view', exact: true }).first().click();
    const code = page.locator('.cm-content').first();
    await code.waitFor({ timeout: 10000 });
    const codeText = await code.innerText();
    if (!codeText.includes('MakeObjectVisible (apple)')) {
        throw new Error(`Expected code view to contain "MakeObjectVisible (apple)", got ${JSON.stringify(codeText)}`);
    }
    console.log('PASS: the picked object is saved in the script');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-searchable-object-picker-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
