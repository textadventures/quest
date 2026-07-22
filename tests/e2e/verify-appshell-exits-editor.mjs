// Ad-hoc manual verification for the new Room Exits editor tab: previously the
// "exits" controltype rendered nothing (only the "Exits list prefix" textbox
// showed). This checks the compass-grid quick-create (with reciprocal exit),
// the full exit list, delete, and navigating into an exit's own editor page.
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

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Exits Editor Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // Enable "look" exits at the game level so the compass cell's "Create a look exit instead"
    // shortcut is offered (it's gated on game.allowlookdirections, off by default). The "game"
    // node is already selected by default right after load, so no tree click needed here — its
    // bounding box spans its (already-expanded) Verbs/Commands children too, which made an
    // explicit click on it land on the wrong row.
    await page.click('button:has-text("Features")');
    await page.waitForSelector('text=Directional exits can be look only', { timeout: 10000 });
    await page.click('label:has-text("Directional exits can be look only") input[type="checkbox"]');
    console.log('PASS: enabled "look" exits at the game level');

    await addRoom('Room One');
    await addRoom('Room Two');
    console.log('PASS: created Room One and Room Two');

    // Select Room One in the tree and open its Exits tab.
    await page.click('[data-value="Room One"]');
    await page.waitForSelector('button:has-text("Exits")', { timeout: 10000 });
    await page.click('button:has-text("Exits")');
    await page.waitForSelector('text=+ north', { timeout: 10000 });
    console.log('PASS: Exits tab renders the compass grid (previously rendered nothing)');

    // Quick-create a North exit from Room One to Room Two, with the reciprocal exit.
    // Directions are lowercase captions ("north", "northwest", ...) — use exact-match role
    // queries throughout since plain text= substring matching would match "northwest" too.
    await page.getByRole('button', { name: '+ north', exact: true }).click();
    const combobox = page.locator('[role="combobox"]');
    await combobox.click();
    await combobox.fill('Room Two');
    await page.waitForSelector('[role="option"]:has-text("Room Two")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("Room Two")');
    const inverseCheckbox = page.locator('text=Also create the return exit').locator('..').locator('input[type="checkbox"]');
    if (!(await inverseCheckbox.isChecked())) throw new Error('Expected "create return exit" to default to checked');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=→ Room Two', { timeout: 10000 });
    console.log('PASS: North compass cell now shows the created exit to Room Two');

    // The full exit list below should also show the new exit.
    await page.waitForSelector('text=north → Room Two', { timeout: 5000 });
    console.log('PASS: full exit list shows the new exit');

    // Room stays selected/in-view after the quick-create (no navigation away) — the "Exits list
    // prefix" field only renders on the room's own Exits tab, so its continued presence proves
    // selection didn't jump elsewhere.
    await page.waitForSelector('text=Exits list prefix', { timeout: 3000 });
    console.log('PASS: Room One stays selected after quick-create (no unwanted navigation)');

    // Switch to Room Two and confirm the reciprocal South exit was created.
    await page.click('[data-value="Room Two"]');
    await page.waitForSelector('button:has-text("Exits")', { timeout: 10000 });
    await page.click('button:has-text("Exits")');
    await page.waitForSelector('text=→ Room One', { timeout: 10000 });
    console.log('PASS: Room Two automatically got the reciprocal South exit to Room One');

    // Click into the exit from the compass grid — should navigate to the exit's own editor.
    // Exact match: the full list row below also contains "→ Room One" as a substring ("south →
    // Room One"), which a plain text= substring locator would ambiguously match too.
    await page.getByRole('button', { name: '→ Room One', exact: true }).click();
    await page.waitForSelector('text=Exit', { timeout: 10000 });
    const toValue = await page.locator('[role="combobox"]').first().inputValue();
    if (toValue !== 'Room One') throw new Error(`Expected exit's own editor "To" field to show Room One, got "${toValue}"`);
    console.log('PASS: clicking an existing exit navigates to its own working editor page');

    // Back on Room One, create a "look" exit (east) — no destination room, one-way.
    await page.click('[data-value="Room One"]');
    await page.waitForSelector('button:has-text("Exits")', { timeout: 10000 });
    await page.click('button:has-text("Exits")');
    await page.getByRole('button', { name: '+ east', exact: true }).click();
    await page.click('text=Create a look exit instead');
    await page.waitForSelector('text=(look)', { timeout: 10000 });
    console.log('PASS: "Create a look exit instead" creates a one-way look exit with no destination');

    // Go back to Room One and delete the north exit via the full list, room should stay selected.
    await page.click('button:has-text("Exits")');
    const rowButton = page.getByRole('button', { name: 'north → Room Two', exact: true });
    await rowButton.waitFor({ timeout: 10000 });
    const listRow = rowButton.locator('..');
    await listRow.hover();
    await listRow.locator('button[title="Delete"]').click();

    // Deleting resets the active tab to the first one (Setup) — same pre-existing behavior as
    // ElementsList's delete-then-reselect-parent elsewhere in the app, not specific to this
    // feature. Room One itself must still be the selected tree node throughout, though.
    await page.waitForSelector('[data-value="Room One"][data-selected]', { timeout: 10000 });
    await page.click('button:has-text("Exits")');
    await page.getByRole('button', { name: '+ north', exact: true }).waitFor({ timeout: 10000 });
    await page.waitForSelector('text=Exits list prefix', { timeout: 3000 });
    console.log('PASS: deleting an exit removes it and keeps Room One selected');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-exits-editor-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
