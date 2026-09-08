// Ad-hoc manual verification that dialogue Pages (Text Adventure "Pages" -
// see CorePages.aslx / issue #2040) don't leak into attribute-level object
// pickers meant for physical world objects. Pages opt out of world scoping
// ("data, not locations" - CorePages.aslx) so they should never be selectable
// as: an exit's "to" destination (checked both via the room's Exits tab AND
// via the exit's own generic property editor - two separate code paths that
// both used to leak Pages), the player object (game.pov), a lockable
// container's key, or a room's "objects dropped here go to" destination.
// The exit "to" pickers are further narrowed to rooms only (not just
// non-pages) - generic objects shouldn't be exit destinations either.
// Requires the AppShell dev server running locally:
//   cd src/AppShell && npm run dev
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

const tabBar = page.locator('.flex.border-b.border-surface-200-800.overflow-x-auto.flex-shrink-0');

// Same disambiguation issue as verify-appshell-exits-editor.mjs's selectTreeNode: a leaf's row is
// [data-part="item"], a branch's own row is [data-part="branch-control"].
async function selectTreeNode(name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

async function addViaMenu(buttonText, name) {
    await page.click('button[title="Add element"]');
    await page.click(`button:has-text("${buttonText}")`, { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

// Reads every option currently offered by a combobox-style object picker (native <select> or the
// custom Combobox used by the Exits tab), after opening it.
async function comboboxOptionTexts(locator) {
    const role = await locator.getAttribute('role');
    if (role === 'combobox') {
        await locator.click();
        const options = page.locator('[role="option"]');
        await options.first().waitFor({ timeout: 5000 });
        return options.allTextContents();
    }
    return locator.locator('option').allTextContents();
}

function assertExcludes(optionTexts, forbidden, context) {
    if (optionTexts.some(t => t.trim() === forbidden)) {
        throw new Error(`${context}: expected "${forbidden}" to be excluded, got [${optionTexts.join(', ')}]`);
    }
}

function assertIncludes(optionTexts, expected, context) {
    if (!optionTexts.some(t => t.trim() === expected)) {
        throw new Error(`${context}: expected "${expected}" to be present, got [${optionTexts.join(', ')}]`);
    }
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Object Picker Pages Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: text adventure local draft created');

    // Fixture: a second room (dropdown target), a generic object, and a page - all siblings under
    // "room" so every picker below sees the same candidate set.
    await selectTreeNode('room');
    await addViaMenu('Add Object in', 'Test Object');
    await selectTreeNode('room');
    await addViaMenu('Add Page in', 'Test Page');
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Room")', { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', 'Second Room');
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector('text=Second Room', { timeout: 10000 });
    console.log('PASS: fixture created (room, Test Object, Test Page, Second Room)');

    // 1) Exit "to" via the room's dedicated Exits tab - rooms only (no Test Object, no Test Page).
    await selectTreeNode('room');
    await tabBar.getByRole('button', { name: 'Exits', exact: true }).click();
    await page.getByRole('button', { name: 'north', exact: true }).click();
    const exitsTabCombobox = page.locator('[role="combobox"]');
    const exitsTabOptions = await comboboxOptionTexts(exitsTabCombobox);
    assertExcludes(exitsTabOptions, 'Test Object', 'Exits tab destination picker');
    assertExcludes(exitsTabOptions, 'Test Page', 'Exits tab destination picker');
    assertIncludes(exitsTabOptions, 'Second Room', 'Exits tab destination picker');
    console.log('PASS: Exits tab destination picker offers rooms only, no objects or pages');
    await page.keyboard.press('Escape');

    // 2) Exit "to" via the generic "+ Exit" property editor - the second, separate code path for
    // the exact same underlying picker.
    await selectTreeNode('room');
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Exit from")', { timeout: 5000 });
    await page.waitForSelector('text=To:', { timeout: 10000 });
    const genericToCombobox = page.locator('[role="combobox"]').first();
    const genericToOptions = await comboboxOptionTexts(genericToCombobox);
    assertExcludes(genericToOptions, 'Test Object', 'Generic exit editor "To" picker');
    assertExcludes(genericToOptions, 'Test Page', 'Generic exit editor "To" picker');
    assertIncludes(genericToOptions, 'Second Room', 'Generic exit editor "To" picker');
    console.log('PASS: generic exit editor "To" picker offers rooms only, no objects or pages');
    await page.keyboard.press('Escape');

    // 3) Player object (game.pov) - any object/room, but not a page.
    await selectTreeNode('game');
    await tabBar.getByRole('button', { name: 'Player', exact: true }).click();
    const playerObjectCombobox = page.locator('[role="combobox"]').first();
    const playerObjectOptions = await comboboxOptionTexts(playerObjectCombobox);
    assertExcludes(playerObjectOptions, 'Test Page', 'Player object picker');
    assertIncludes(playerObjectOptions, 'Test Object', 'Player object picker');
    assertIncludes(playerObjectOptions, 'room', 'Player object picker');
    console.log('PASS: player object picker offers objects and rooms, excludes pages');
    await page.keyboard.press('Escape');

    // 4) Room "Objects dropped here go to" - any object/room, but not a page.
    await selectTreeNode('room');
    await tabBar.getByRole('button', { name: 'Room', exact: true }).click();
    // The advanced-controls expander is a native <details>/<summary> pair in the properties
    // panel - scoping by tag disambiguates from the tree's own unrelated "Advanced" folder node.
    await page.locator('summary', { hasText: 'Advanced' }).click();
    await page.waitForSelector('text=Objects dropped here go to', { timeout: 10000 });
    const dropDestCombobox = page.locator('[role="combobox"]').first();
    const dropDestOptions = await comboboxOptionTexts(dropDestCombobox);
    assertExcludes(dropDestOptions, 'Test Page', 'Room drop-destination picker');
    assertIncludes(dropDestOptions, 'Test Object', 'Room drop-destination picker');
    console.log('PASS: room drop-destination picker offers objects and rooms, excludes pages');
    await page.keyboard.press('Escape');

    // 5) Lockable container "Key" - any object/room, but not a page.
    await selectTreeNode('Test Object');
    await tabBar.getByRole('button', { name: 'Features', exact: true }).click();
    await page.getByText('Container: object is a container').click();
    await tabBar.getByRole('button', { name: 'Container', exact: true }).click();
    await page.selectOption('select', { label: 'Closed container' });
    await page.waitForSelector('text=LOCKING', { timeout: 10000 });
    await page.selectOption('select >> nth=1', { label: 'Lockable' });
    await page.waitForSelector('text=Number of keys to unlock container', { timeout: 10000 });
    const keyCountInput = page.locator('input[type="number"]');
    await keyCountInput.fill('1');
    // The number control commits its value on blur, not on every keystroke - Tab away to trigger
    // the commit so <onlydisplayif>GetInt(this, "keycount") > 0</onlydisplayif> re-evaluates and
    // reveals the Key picker.
    await keyCountInput.press('Tab');
    await page.waitForSelector('text=Key:', { timeout: 10000 });
    const keyCombobox = page.locator('[role="combobox"]').first();
    const keyOptions = await comboboxOptionTexts(keyCombobox);
    assertExcludes(keyOptions, 'Test Page', 'Container key picker');
    assertIncludes(keyOptions, 'room', 'Container key picker');
    console.log('PASS: container key picker offers objects and rooms, excludes pages');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-object-pickers-exclude-pages-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
