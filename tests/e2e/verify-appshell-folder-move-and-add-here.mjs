// Manual verification for the "move a function folder up/down", "Add Function here" (from a
// folder's own "..." menu, pre-filling the create modal's folder picker), and "Add Object here"
// (now offered on objects too, not just rooms, with a create-modal parent picker scoped to the
// clicked object's own ancestor chain) features.
// Requires the AppShell dev server running locally:
//   cd src/AppShell && npm run dev
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

function selectTreeNode(name) {
    return page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

// Locates a folder/library group's own branch-control row by its visible label (these synthetic
// nodes have no stable data-value - see TreePanel's groupLibraryChildren).
function groupRow(label) {
    return page.locator('[data-part="branch-control"]', { hasText: label }).first();
}

async function openNodeMenu(rowLocator) {
    await rowLocator.locator('[title="Options"]').click();
    const dropdown = page.locator('.tree-dropdown');
    await dropdown.waitFor({ state: 'visible', timeout: 5000 });
    return dropdown;
}

// The tree's "..." dropdown only closes on an outside mousedown (see TreePanel's onOutside) - it
// has no Escape-key handling, so closing it means clicking somewhere else in the page instead.
async function closeNodeMenu() {
    await page.locator('input[placeholder="Filter..."]').click();
}

// Expands a branch via its own chevron button (idempotent - a no-op if already expanded).
// Double-clicking the row also works (see TreePanel's ondblclick) but doesn't select it - the
// chevron button's own onclick stops propagation, so selecting a node is always a separate click.
async function expandNode(rowLocator) {
    const expandBtn = rowLocator.getByRole('button', { name: 'Expand' });
    if (await expandBtn.count() > 0) await expandBtn.click();
}

async function fillAndConfirmAddModal(name) {
    await page.waitForSelector('#element-name', { timeout: 10000 });
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    // 'attached' rather than the default 'visible' - a newly-created function inside an
    // already-collapsed folder (e.g. via "Add Function here") is in the DOM but not shown until
    // the folder is expanded, which is fine; this only confirms creation actually happened.
    await page.locator(`text=${name}`).first().waitFor({ state: 'attached', timeout: 10000 });
}

// Picks an existing option by exact label if present, otherwise types the name as free text and
// commits it by blurring (creating a new folder) - mirrors how a real user would use this
// combobox. Deliberately blurs rather than pressing Enter: the modal wrapper's own onkeydown also
// treats Enter as "confirm", and since the combobox's Enter handler commits the value into the
// same synchronous state read by that outer handler, a real Enter keypress bubbles up and
// immediately submits the whole dialog too - a double-submit this helper must avoid so the
// caller's own explicit submit-button click has a dialog left to click on.
async function chooseOrCreateOption(combo, label) {
    await combo.click();
    await combo.fill(label);
    try {
        await page.locator('[role="option"]', { hasText: label }).first().click({ timeout: 2000 });
    } catch {
        await page.locator('[role="dialog"] h2').click();
    }
}

async function comboboxOptionTexts(locator) {
    await locator.click();
    const options = page.locator('[role="option"]');
    await options.first().waitFor({ timeout: 5000 });
    return options.allTextContents();
}

function assertIncludes(actual, expected, context) {
    if (!actual.some(t => t.trim() === expected)) {
        throw new Error(`${context}: expected "${expected}" to be present, got [${actual.join(', ')}]`);
    }
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Folder Move Add Here Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: text adventure local draft created');

    // ── Fixture: three top-level functions, two of them grouped into a "Math" folder ──────────
    // "Functions" has no dedicated tree branch until it's non-empty (empty advanced categories
    // collapse into the "Advanced" node's own generic "+ Add X" buttons - see TreePanel's empty-
    // category handling), so the very first function is created from there.
    const advancedRow = page.locator('[data-part="branch-control"]', { hasText: 'Advanced' }).first();
    await advancedRow.click(); // selects it, showing its "+ Add X" panel
    await expandNode(advancedRow); // the chevron button stops propagation, so expand needs its own click
    await page.getByRole('button', { name: '+ Add Function', exact: true }).click();
    await fillAndConfirmAddModal('Alpha');
    for (const name of ['Beta', 'Gamma']) {
        // Creating a function selects it (see afterCreate/selectNode), which swaps the properties
        // panel away from the Functions header's own "+ Add Function" button - reselect it first.
        await selectTreeNode('_functions');
        await page.getByRole('button', { name: '+ Add Function', exact: true }).click();
        await fillAndConfirmAddModal(name);
    }
    for (const name of ['Alpha', 'Beta']) {
        const row = page.locator('[data-part="item"]', { hasText: name }).first();
        const dropdown = await openNodeMenu(row);
        await dropdown.getByRole('button', { name: 'Move to folder…' }).click();
        await page.waitForSelector('text=Move "' + name + '" to folder', { timeout: 10000 });
        await chooseOrCreateOption(page.locator('[role="dialog"] [role="combobox"]'), 'Math');
        await page.click('[role="dialog"] button:has-text("Move")');
        await page.waitForTimeout(300);
    }
    console.log('PASS: Alpha and Beta grouped into "Math" folder (final order: Gamma, Math[Alpha, Beta])');

    // ── Folder "..." menu: Move up present (Gamma precedes), Move down absent (nothing follows) ─
    const mathRow = groupRow('Math');
    await mathRow.waitFor({ timeout: 10000 });
    let dropdown = await openNodeMenu(mathRow);
    const menuTexts = await dropdown.locator('button').allTextContents();
    if (!menuTexts.includes('Add Function here')) throw new Error(`Folder menu missing "Add Function here": [${menuTexts.join(', ')}]`);
    if (!menuTexts.includes('Move up')) throw new Error(`Folder menu missing "Move up": [${menuTexts.join(', ')}]`);
    if (menuTexts.includes('Move down')) throw new Error(`Folder menu should not offer "Move down" (nothing follows it): [${menuTexts.join(', ')}]`);
    console.log('PASS: "Math" folder\'s "..." menu offers "Add Function here" and "Move up" only');
    await closeNodeMenu();

    // ── "Add Function here" defaults the create modal's folder picker to "Math" ────────────────
    dropdown = await openNodeMenu(mathRow);
    await dropdown.getByRole('button', { name: 'Add Function here' }).click();
    await page.waitForSelector('#element-name', { timeout: 10000 });
    const folderPicker = page.locator('[role="dialog"] [role="combobox"]');
    const folderPickerValue = await folderPicker.inputValue();
    if (folderPickerValue !== 'Math') throw new Error(`Expected folder picker to default to "Math", got "${folderPickerValue}"`);
    console.log('PASS: "Add Function here" pre-fills the folder picker with "Math"');
    await fillAndConfirmAddModal('Delta');
    // createFunction() and setFunctionFolder() both fire their own tree-rebuild event
    // (see handleAddConfirm) - give the second (folder-assignment) rebuild's reactive grouping
    // recompute a moment to settle before inspecting the tree, so this doesn't catch the
    // intermediate state where Delta briefly exists as a bare top-level function.
    await page.waitForTimeout(500);

    // Confirm Delta landed inside Math, not at the top level: expand the folder and check for it.
    await expandNode(mathRow);
    await page.locator('[data-part="item"]', { hasText: 'Delta' }).waitFor({ timeout: 10000 });
    console.log('PASS: "Delta" was created inside the "Math" folder');

    // ── Move folder up: "Math" should now precede "Gamma" ──────────────────────────────────────
    async function topLevelFunctionOrder() {
        // Every row's own label text, in document order - only the relative order of "Gamma" vs
        // "Math" is checked below, so rows for unrelated nodes elsewhere in the tree don't matter.
        return page.locator('[data-part="item"], [data-part="branch-control"]').allTextContents();
    }
    const before = (await topLevelFunctionOrder()).map(t => t.trim());
    const gammaIdx = before.findIndex(t => t.includes('Gamma'));
    const mathIdx = before.findIndex(t => t.includes('Math'));
    if (!(gammaIdx >= 0 && mathIdx >= 0 && gammaIdx < mathIdx)) {
        throw new Error(`Expected Gamma before Math folder before the move, got: [${before.join(', ')}]`);
    }

    dropdown = await openNodeMenu(mathRow);
    await dropdown.getByRole('button', { name: 'Move up', exact: true }).click();
    await page.waitForTimeout(300);

    const after = (await topLevelFunctionOrder()).map(t => t.trim());
    const gammaIdx2 = after.findIndex(t => t.includes('Gamma'));
    const mathIdx2 = after.findIndex(t => t.includes('Math'));
    if (!(mathIdx2 >= 0 && gammaIdx2 >= 0 && mathIdx2 < gammaIdx2)) {
        throw new Error(`Expected Math folder before Gamma after "Move up", got: [${after.join(', ')}]`);
    }
    console.log('PASS: "Move up" moved the whole "Math" folder block ahead of "Gamma"');

    // ── "Add Object here" on an object (not just rooms), with a scoped parent picker ───────────
    // Uses the room's own "..." menu (pre-existing action) rather than the Objects-tab toolbar
    // button, consistent with how the folder/function actions above are driven.
    const roomRow = page.locator('[data-part="item"], [data-part="branch-control"]', { hasText: 'room' }).first();
    dropdown = await openNodeMenu(roomRow);
    await dropdown.getByRole('button', { name: 'Add Object here' }).click();
    await fillAndConfirmAddModal('Test Object');

    const objectRow = page.locator('[data-part="item"], [data-part="branch-control"]', { hasText: 'Test Object' }).first();
    dropdown = await openNodeMenu(objectRow);
    const objectMenuTexts = await dropdown.locator('button').allTextContents();
    if (!objectMenuTexts.includes('Add Object here')) {
        throw new Error(`Expected "Test Object"'s "..." menu to offer "Add Object here", got: [${objectMenuTexts.join(', ')}]`);
    }
    console.log('PASS: an object\'s own "..." menu now offers "Add Object here"');

    await dropdown.getByRole('button', { name: 'Add Object here' }).click();
    await page.waitForSelector('#element-name', { timeout: 10000 });
    const parentPicker = page.locator('[role="dialog"] [role="combobox"]');
    const parentDefault = await parentPicker.inputValue();
    if (parentDefault !== 'Test Object') throw new Error(`Expected parent picker to default to "Test Object", got "${parentDefault}"`);
    const parentOptions = await comboboxOptionTexts(parentPicker);
    // The template's default "room" has no Parent element of its own (true top level), so the
    // ancestor chain for an object nested two levels under it is just [room, Test Object] - no
    // synthetic "game"/"_objects" entry (this mirrors GetPossibleNewObjectParentsForCurrentSelection
    // walking Element.Parent directly, not the tree's own "_objects" sentinel).
    assertIncludes(parentOptions, 'room', 'Add Object parent picker');
    assertIncludes(parentOptions, 'Test Object', 'Add Object parent picker');
    if (parentOptions.length !== 2) throw new Error(`Expected only [room, Test Object], got [${parentOptions.join(', ')}]`);
    console.log('PASS: parent picker defaults to "Test Object" and is scoped to its ancestor chain (room, Test Object) only');
    // Close the combobox's own popup (not the whole modal - Escape would also cancel the dialog).
    await page.locator('[role="dialog"] h2').click();
    await fillAndConfirmAddModal('Inner Thing');

    await expandNode(objectRow);
    await page.locator('[data-part="item"]', { hasText: 'Inner Thing' }).waitFor({ timeout: 10000 });
    console.log('PASS: "Inner Thing" was created nested inside "Test Object" (the default parent)');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-folder-move-and-add-here-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
