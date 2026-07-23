// Ad-hoc manual verification for the new filter textbox at the top of
// TreePanel (element tree, left) on the /edit page — lets authors with lots
// of objects narrow the tree down to matching names instead of scrolling.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function addViaToolbar(menuLabel, name) {
    await page.click('button[title="Add element"]');
    await page.locator('.add-dropdown button', { hasText: menuLabel }).click();
    await page.fill('#element-name', name);
    await page.keyboard.press('Enter');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Tree Filter Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    const filterInput = page.getByPlaceholder('Filter...');
    await filterInput.waitFor({ timeout: 10000 });
    console.log('PASS: filter input is present');

    // Build up a small tree: two top-level rooms, each with an object inside.
    await addViaToolbar('Add Room', 'Kitchen');
    await addViaToolbar('Add Room', 'Garden');

    await page.click('span:text-is("Kitchen")');
    await addViaToolbar('Add Object in "Kitchen"', 'Table');

    await page.click('span:text-is("Garden")');
    await addViaToolbar('Add Object in "Garden"', 'Flower');
    console.log('PASS: built Kitchen(Table) / Garden(Flower) tree');

    const treePanel = page.locator('div.border-r.border-surface-200-800.bg-surface-50-950').first();

    // Filtering for "table" should show Kitchen (ancestor) + Table, but hide Garden/Flower.
    await filterInput.fill('table');
    await page.waitForTimeout(200);
    let text = await treePanel.innerText();
    if (!text.includes('Kitchen')) throw new Error('Expected ancestor "Kitchen" to stay visible while filtering for "table"');
    if (!text.includes('Table')) throw new Error('Expected "Table" to be visible while filtering for "table"');
    if (text.includes('Garden')) throw new Error('Expected "Garden" to be hidden while filtering for "table"');
    if (text.includes('Flower')) throw new Error('Expected "Flower" to be hidden while filtering for "table"');
    console.log('PASS: filtering for "table" shows only Kitchen > Table');

    // Filtering for a room name should show that room's full subtree (Table).
    await filterInput.fill('kitchen');
    await page.waitForTimeout(200);
    text = await treePanel.innerText();
    if (!text.includes('Kitchen') || !text.includes('Table')) throw new Error('Expected matched room "Kitchen" to show its full subtree ("Table")');
    if (text.includes('Garden')) throw new Error('Expected "Garden" to stay hidden while filtering for "kitchen"');
    console.log('PASS: filtering for "kitchen" shows the matched room\'s full subtree');

    // Filtering is case-insensitive.
    await filterInput.fill('KITCHEN');
    await page.waitForTimeout(200);
    text = await treePanel.innerText();
    if (!text.includes('Kitchen')) throw new Error('Expected filter to be case-insensitive');
    console.log('PASS: filtering is case-insensitive');

    // No matches.
    await filterInput.fill('zzz-nonexistent');
    await page.waitForTimeout(200);
    await page.waitForSelector('text=No matches', { timeout: 5000 });
    console.log('PASS: "No matches" message shown for a query with no results');

    // Clear button resets the filter and restores the full tree.
    await page.locator('button[aria-label="Clear filter"]').click();
    await page.waitForTimeout(200);
    text = await treePanel.innerText();
    if (await filterInput.inputValue() !== '') throw new Error('Expected clear button to empty the filter input');
    if (!text.includes('Garden') || !text.includes('Flower')) throw new Error('Expected full tree to be restored after clearing the filter');
    console.log('PASS: clear button restores the full tree');

    // Escape clears the filter too.
    await filterInput.fill('table');
    await page.waitForTimeout(200);
    await filterInput.press('Escape');
    await page.waitForTimeout(200);
    if (await filterInput.inputValue() !== '') throw new Error('Expected Escape to clear the filter input');
    text = await treePanel.innerText();
    if (!text.includes('Garden')) throw new Error('Expected full tree to be restored after pressing Escape');
    console.log('PASS: Escape clears the filter');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-tree-filter-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
