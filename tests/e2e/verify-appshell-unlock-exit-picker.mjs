// Ad-hoc manual verification for the "unlock exit" (and lock/make-visible/make-invisible exit)
// script command: its object picker used <objecttype>exit</objecttype> in the .aslx editor
// definition, but ScriptEditor.svelte's "objects" simpleeditor always listed plain object names
// (rooms/objects), never named exits, because the objecttype tag was silently dropped when
// building ScriptControlData for script parameters (it was only wired up for the unrelated
// PropertyEditor "objects"/"elementslist" controls). Fixed by threading objecttype through to
// the frontend and loading a separate exit-names list for objecttype="exit" controls.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function addRoom(name) {
    await page.click('button[title="Add element"]');
    await page.click('.add-dropdown button:has-text("Add Room")', { timeout: 5000 });
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
    await page.fill('input[placeholder="Game name"]', `Unlock Exit Picker Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    await addRoom('Room One');
    await addRoom('Room Two');
    console.log('PASS: created Room One and Room Two');

    // Create a north exit from Room One to Room Two via the compass grid, then open its own
    // editor page and give it a name — only named, non-anonymous exits are eligible to be
    // referenced from a script (matches the "you must name this exit to unlock it" hint on the
    // exit's own editor page).
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
    // The property editor renders "Name:" as a plain <span> label sibling of the input, not a
    // <label for=...> — locate the input via their shared container.
    const nameField = page.locator('span:has-text("Name:")').locator('..').locator('input[type="text"]');
    await nameField.fill('gate');
    await nameField.blur();
    await page.waitForSelector('[data-value="gate"]', { timeout: 10000 });
    console.log('PASS: named the north exit "gate"');

    // Add an "Unlock exit" script to Room One's "before entering" script and open its parameter.
    await selectTreeNode('Room One');
    await page.waitForSelector('button:has-text("Script")', { timeout: 10000 });
    await page.click('button:has-text("Script")');
    await page.waitForSelector('button:has-text("Add script")', { timeout: 10000 });
    await page.click('button:has-text("Add script")');
    const addScriptDialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await addScriptDialog.waitFor({ timeout: 10000 });
    await addScriptDialog.getByRole('button', { name: 'Objects', exact: true }).click();
    await addScriptDialog.getByRole('button', { name: 'Unlock exit' }).click();
    await addScriptDialog.getByRole('button', { name: 'OK', exact: true }).click();
    console.log('PASS: added an "Unlock exit" script command');

    // The exit picker is the first <select> in the newly-added script row.
    const exitSelect = page.locator('select').last();
    const options = await exitSelect.locator('option').allTextContents();
    if (!options.includes('gate')) {
        throw new Error(`Expected the Unlock exit picker to list the named exit "gate", got: ${JSON.stringify(options)}`);
    }
    if (options.includes('Room One') || options.includes('Room Two')) {
        throw new Error(`Expected the Unlock exit picker to exclude plain object names, got: ${JSON.stringify(options)}`);
    }
    console.log('PASS: Unlock exit picker lists the named exit ("gate") and excludes room/object names');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-unlock-exit-picker-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
