// Ad-hoc manual verification: renaming an object to an invalid name (e.g. containing a dash)
// used to silently fail — PropertyEditor.svelte discarded the ValidationResult returned by
// setAttribute(), and WasmEditorBridge.SetAttribute returned the raw C# enum name instead of a
// friendly message. This checks that an inline error now appears (and a toast that survives
// switching to a different tree node, unlike the tab-scoped inline error) and the rename doesn't
// stick.
import { chromium } from 'playwright';

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

async function addObjectIn(roomName, name) {
    await selectTreeNode(roomName);
    await page.click('button[title="Add element"]');
    await page.click(`button:has-text("Add Object in")`, { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Rename Invalid Name Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    await addRoom('Room One');
    console.log('PASS: created Room One');
    await addObjectIn('Room One', 'thing');
    console.log('PASS: created object "thing"');

    // The name field is the first textbox control on the object's properties panel.
    const nameInput = page.locator('input[type="text"]').first();
    const before = await nameInput.inputValue();
    if (before !== 'thing') throw new Error(`Expected name field to show "thing", got "${before}"`);

    await nameInput.fill('new-thing');
    await nameInput.blur();

    // Should show an inline validation error instead of silently accepting/discarding it, and it
    // should call out the specific offending character rather than a generic "invalid name".
    const errorText = page.locator('p.text-error-500', { hasText: /invalid/i });
    await errorText.waitFor({ timeout: 5000 });
    const errorMessage = await errorText.textContent();
    if (!errorMessage?.includes("'-'")) throw new Error(`Expected the error to call out the '-' character specifically, got: "${errorMessage}"`);
    console.log(`PASS: inline error shown, naming the specific bad character: "${errorMessage}"`);

    const borderClass = await nameInput.getAttribute('class');
    if (!borderClass?.includes('border-error-500')) throw new Error('Expected name input to get an error border');
    console.log('PASS: name input gets an error border');

    // A toast should also have fired, rendered at the layout level.
    const toast = page.locator('[role="alert"]', { hasText: /invalid/i });
    await toast.waitFor({ timeout: 5000 });
    console.log(`PASS: toast notification shown: "${await toast.textContent()}"`);

    // The tree label must NOT have changed to the invalid name.
    const treeStillThing = await page.locator('[data-value="thing"]').count();
    if (treeStillThing === 0) throw new Error('Expected tree to still show "thing" (rename should have been rejected)');
    const treeGotBadName = await page.locator('[data-value="new-thing"]').count();
    if (treeGotBadName !== 0) throw new Error('Tree picked up the invalid name — rename incorrectly succeeded');
    console.log('PASS: tree label unchanged ("thing"), invalid rename was rejected');

    // Navigating away to a DIFFERENT node and back should show the field reverted to the real
    // name, not the stale typed text (this matches the original repro: reselecting the SAME
    // already-selected node is a no-op and doesn't exercise the bug). The inline error (tied to
    // the "thing" node's property panel) is expected to disappear once we leave that node — but
    // the toast must still be showing, since it's the whole point of not being tab/element-scoped.
    await selectTreeNode('Room One');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 5000 });
    if ((await toast.count()) === 0) throw new Error('Expected the toast to still be visible after switching to a different tree node');
    console.log('PASS: toast survives navigating to a different tree node');

    await selectTreeNode('thing');
    const afterRevisit = await page.locator('input[type="text"]').first().inputValue();
    if (afterRevisit !== 'thing') throw new Error(`Expected name field to revert to "thing" after reselecting, got "${afterRevisit}"`);
    console.log('PASS: name field correctly reverted to "thing" after navigating away and back');

    // Now do a VALID rename and confirm it still works (no regression).
    await nameInput.fill('renamed thing');
    await nameInput.blur();
    await page.waitForSelector('[data-value="renamed thing"]', { timeout: 5000 });
    console.log('PASS: valid rename to "renamed thing" still works');

    // The toast's dismiss button should close it manually rather than waiting out the timeout.
    await toast.locator('button[aria-label="Dismiss"]').click();
    await toast.waitFor({ state: 'detached', timeout: 2000 });
    console.log('PASS: toast dismiss button closes it');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/rename-invalid-name-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
