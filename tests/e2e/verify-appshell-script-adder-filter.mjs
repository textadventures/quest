// Ad-hoc manual verification for the new filter textbox in the "Add Script Command" modal
// (AddScriptModal.svelte). Unlike the elements tree filter (TreePanel.svelte), the script adder
// is a two-pane category-sidebar + command-list, so filtering can't just narrow a single tree —
// while the filter box has text, it replaces that view with one flat list of matches across
// every category (each tagged with its category name), then reverts to the normal per-category
// browsing view when cleared. Also covers Up/Down arrow-key selection, added afterwards since
// the command list is a custom listbox (plain buttons) with no keyboard nav for free.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Script Adder Filter Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // Open the game's own "before entering"-style top-level script editor via the tree root,
    // which is simplest: use the "Script" tab on the root game object.
    await page.locator('[data-value]').first().click();
    await page.waitForSelector('button:has-text("Script")', { timeout: 10000 });
    await page.click('button:has-text("Script")');
    await page.waitForSelector('button:has-text("Add script")', { timeout: 10000 });
    await page.click('button:has-text("Add script")');
    const dialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await dialog.waitFor({ timeout: 10000 });
    console.log('PASS: opened Add Script Command modal');

    // Baseline: two-pane category view, sidebar visible with multiple categories.
    const categoryButtons = dialog.locator('button:has-text("Objects")');
    await categoryButtons.first().waitFor({ timeout: 5000 });
    console.log('PASS: category sidebar visible before filtering');

    const filterInput = dialog.getByPlaceholder('Filter commands...');
    await filterInput.waitFor({ timeout: 5000 });

    const isOptionSelected = async (row) => (await row.getAttribute('aria-selected')) === 'true';

    // Arrow-key nav should also work in the plain (non-filtered) per-category command list, not
    // just the flattened filtered one — same underlying custom listbox, same lack of free
    // keyboard nav. Scope to the commands listbox specifically, since the category sidebar is
    // now *also* a listbox of options (see the category-nav check further down) and a bare
    // `[role="listbox"] [role="option"]` would match both.
    await categoryButtons.first().click();
    const commandRowsInCategory = dialog.locator('[role="listbox"][aria-label^="Script commands in"] [role="option"]');
    const commandRowCount = await commandRowsInCategory.count();
    if (commandRowCount < 2) {
        throw new Error(`Expected the "Objects" category to have at least 2 commands to test navigation, got ${commandRowCount}`);
    }
    if (!(await isOptionSelected(commandRowsInCategory.nth(0)))) {
        throw new Error('Expected the first command in the category to be selected by default');
    }
    await filterInput.press('ArrowDown');
    if (await isOptionSelected(commandRowsInCategory.nth(0)) || !(await isOptionSelected(commandRowsInCategory.nth(1)))) {
        throw new Error('Expected ArrowDown to move selection within the non-filtered category list');
    }
    console.log('PASS: ArrowDown moves the selection within the non-filtered category list too');
    await filterInput.press('ArrowUp');

    // Up/Down while focus is on the category sidebar itself should switch categories, not move
    // through commands — this is the thing that was missing (both are plain buttons, so without
    // explicit wiring the general Up/Down handler can't tell them apart).
    const categoryOptions = dialog.locator('[role="listbox"][aria-label="Script command categories"] [role="option"]');
    const categoryCount = await categoryOptions.count();
    if (categoryCount < 2) {
        throw new Error(`Expected at least 2 categories to test navigation, got ${categoryCount}`);
    }
    // Click (not just focus) the first category button so it becomes both the selected category
    // and the DOM focus target — a plain .focus() wouldn't update selectedCategoryIndex, since
    // that's driven by the click handler, not focus.
    await categoryOptions.first().click();
    if (!(await isOptionSelected(categoryOptions.nth(0)))) {
        throw new Error('Expected the first category to be selected once clicked');
    }
    const commandsLabelBefore = await dialog.locator('[role="listbox"][aria-label^="Script commands in"]').getAttribute('aria-label');
    await page.keyboard.press('ArrowDown');
    if (await isOptionSelected(categoryOptions.nth(0)) || !(await isOptionSelected(categoryOptions.nth(1)))) {
        throw new Error('Expected ArrowDown, with focus on the category sidebar, to move to the next category');
    }
    const commandsLabelAfter = await dialog.locator('[role="listbox"][aria-label^="Script commands in"]').getAttribute('aria-label');
    if (commandsLabelAfter === commandsLabelBefore) {
        throw new Error('Expected the command list to switch to the newly-selected category');
    }
    const focusedIsSecondCategory = await categoryOptions.nth(1).evaluate((el) => el === document.activeElement);
    if (!focusedIsSecondCategory) {
        throw new Error('Expected DOM focus to move to the newly-selected category button');
    }
    console.log(`PASS: ArrowDown on the category sidebar moves category selection and focus (now "${commandsLabelAfter}")`);

    await page.keyboard.press('ArrowUp');
    if (!(await isOptionSelected(categoryOptions.nth(0)))) {
        throw new Error('Expected ArrowUp, with focus on the category sidebar, to move back to the first category');
    }
    console.log('PASS: ArrowUp on the category sidebar moves back to the previous category');

    // Roving tabindex: only the selected option in each listbox should be a tab stop, not every
    // row — otherwise Tab has to step through every category (and every command) individually.
    const tabIndicesOf = (locator) => locator.evaluateAll((els) => els.map((el) => el.tabIndex));
    const categoryTabIndices = await tabIndicesOf(categoryOptions);
    if (categoryTabIndices.filter((t) => t === 0).length !== 1) {
        throw new Error(`Expected exactly one category with tabIndex 0, got ${JSON.stringify(categoryTabIndices)}`);
    }
    console.log('PASS: only the selected category is a tab stop (roving tabindex)');

    const commandTabIndices = await tabIndicesOf(commandRowsInCategory);
    if (commandTabIndices.filter((t) => t === 0).length !== 1) {
        throw new Error(`Expected exactly one command row with tabIndex 0, got ${JSON.stringify(commandTabIndices)}`);
    }
    console.log('PASS: only the selected command row is a tab stop (roving tabindex)');

    // ArrowRight, with focus on the category sidebar, should jump to the (already-selected) row
    // in the command list; ArrowLeft from there should jump back.
    await page.keyboard.press('ArrowRight');
    const focusedIsCommandRow = await commandRowsInCategory.nth(0).evaluate((el) => el === document.activeElement);
    if (!focusedIsCommandRow) {
        throw new Error('Expected ArrowRight from the category sidebar to move focus into the command list');
    }
    console.log('PASS: ArrowRight moves focus from the category sidebar into the command list');

    await page.keyboard.press('ArrowLeft');
    const focusedIsCategoryAgain = await categoryOptions.nth(0).evaluate((el) => el === document.activeElement);
    if (!focusedIsCategoryAgain) {
        throw new Error('Expected ArrowLeft from the command list to move focus back to the category sidebar');
    }
    console.log('PASS: ArrowLeft moves focus from the command list back to the category sidebar');

    // Filtering should surface a match ("move" appears in commands from multiple categories,
    // e.g. Objects' "Move object to..." and other move-related commands) and stop showing the
    // category sidebar.
    await filterInput.fill('move');
    await page.waitForTimeout(200);

    // The category sidebar is the only "w-40" column in the modal; check its absence rather than
    // any specific category name, since matched rows are now tagged with their category name too
    // (e.g. "Variables" legitimately appears as a result's category tag, not just a sidebar button).
    const sidebarStillThere = await dialog.locator('div.w-40').count();
    if (sidebarStillThere > 0) {
        throw new Error('Expected the category sidebar to be replaced by a flat list while filtering');
    }
    console.log('PASS: category sidebar hidden while filtering');

    // Scope past the "Move" quick-add shortcut pill, which also matches /move/i, to just the
    // flat results list (identified by its category-tag rows using text-surface-400).
    const matchRows = dialog.locator('.flex-1.overflow-y-auto button', { hasText: /move/i });
    const matchCount = await matchRows.count();
    if (matchCount === 0) {
        throw new Error('Expected at least one match for "move"');
    }
    console.log(`PASS: found ${matchCount} matching command row(s) for "move"`);

    // Each result row should be tagged with its source category name so authors know where it
    // lives even though the sidebar is gone.
    const firstRowText = await matchRows.first().textContent();
    if (!firstRowText || firstRowText.trim().length === 0) {
        throw new Error('Expected matching row to have visible text (command + category tag)');
    }
    console.log(`PASS: first match row reads "${firstRowText.trim()}"`);

    // Arrow keys should move the highlighted (aria-selected) row without needing a mouse click —
    // these are custom listboxes (plain buttons), so this doesn't come for free.
    const isRowSelected = async (row) => (await row.getAttribute('aria-selected')) === 'true';
    if (!(await isRowSelected(matchRows.nth(0)))) {
        throw new Error('Expected the first match to be selected by default');
    }
    await filterInput.press('ArrowDown');
    if (await isRowSelected(matchRows.nth(0)) || !(await isRowSelected(matchRows.nth(1)))) {
        throw new Error('Expected ArrowDown to move selection from the first match to the second');
    }
    console.log('PASS: ArrowDown moves the selection to the next match');

    await filterInput.press('ArrowUp');
    if (!(await isRowSelected(matchRows.nth(0)))) {
        throw new Error('Expected ArrowUp to move selection back to the first match');
    }
    console.log('PASS: ArrowUp moves the selection back to the previous match');

    // A nonsense query should show the empty state, not stale results.
    await filterInput.fill('zzz_no_such_command_zzz');
    await page.waitForTimeout(200);
    await dialog.locator('text=No matching commands').waitFor({ timeout: 5000 });
    console.log('PASS: "No matching commands" empty state shown for a non-matching query');

    // Clearing the filter (the × button) should restore the category sidebar.
    await dialog.getByLabel('Clear filter').click();
    await categoryButtons.first().waitFor({ timeout: 5000 });
    const filterValueAfterClear = await filterInput.inputValue();
    if (filterValueAfterClear !== '') {
        throw new Error('Expected filter box to be empty after clicking clear');
    }
    console.log('PASS: clearing the filter restores the category sidebar');

    // Escape while the filter has text should clear it rather than closing the modal.
    await filterInput.fill('move');
    await page.waitForTimeout(200);
    await filterInput.press('Escape');
    await dialog.waitFor({ timeout: 2000 });
    const filterValueAfterEscape = await filterInput.inputValue();
    if (filterValueAfterEscape !== '') {
        throw new Error('Expected Escape to clear a non-empty filter rather than closing the modal');
    }
    console.log('PASS: Escape clears a non-empty filter instead of closing the modal');

    // Escape again (now empty) should close the modal.
    await filterInput.press('Escape');
    await dialog.waitFor({ state: 'hidden', timeout: 5000 });
    console.log('PASS: Escape with an empty filter closes the modal');

    // Round-trip proof that Enter adds whichever row is *currently selected*, not always the
    // first: add the plain default match, then add again after ArrowDown, and confirm the two
    // resulting script rows are different commands.
    async function addedRowTexts() {
        return page.locator('[role="region"] > div.group').allTextContents();
    }

    await page.click('button:has-text("Add script")');
    await dialog.waitFor({ timeout: 10000 });
    await dialog.getByPlaceholder('Filter commands...').fill('move');
    await page.waitForTimeout(200);
    await dialog.getByPlaceholder('Filter commands...').press('Enter');
    await dialog.waitFor({ state: 'hidden', timeout: 5000 });
    const rowsAfterFirstAdd = await addedRowTexts();
    if (rowsAfterFirstAdd.length !== 1) {
        throw new Error(`Expected exactly one script row after the first add, got ${rowsAfterFirstAdd.length}`);
    }
    console.log(`PASS: plain Enter added the default (first) match: "${rowsAfterFirstAdd[0].trim()}"`);

    await page.click('button:has-text("Add script")');
    await dialog.waitFor({ timeout: 10000 });
    await dialog.getByPlaceholder('Filter commands...').fill('move');
    await page.waitForTimeout(200);
    await dialog.getByPlaceholder('Filter commands...').press('ArrowDown');
    await dialog.getByPlaceholder('Filter commands...').press('Enter');
    await dialog.waitFor({ state: 'hidden', timeout: 5000 });
    const rowsAfterSecondAdd = await addedRowTexts();
    if (rowsAfterSecondAdd.length !== 2) {
        throw new Error(`Expected two script rows after the second add, got ${rowsAfterSecondAdd.length}`);
    }
    const secondAddedText = rowsAfterSecondAdd[1].trim();
    if (secondAddedText === rowsAfterFirstAdd[0].trim()) {
        throw new Error('Expected ArrowDown + Enter to add a different command than plain Enter did');
    }
    console.log(`PASS: ArrowDown + Enter added a different match: "${secondAddedText}" (confirms keyboard-moved selection, not just the default, drives Enter)`);

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-script-adder-filter-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
