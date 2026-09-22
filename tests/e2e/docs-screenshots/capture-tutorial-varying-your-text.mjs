// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/tutorial/varying-your-text.md. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    setLabeledField, selectLabeledField, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

// capture()'s own saveIfDirty() clicks the "Unsaved" toolbar pill, which would dismiss an
// open dropdown menu before the screenshot is taken - so settle the save first, then open
// the menu.
async function settleSave(page) {
    const chip = page.locator('.save-chip-unsaved');
    if (await chip.count() === 0 || !(await chip.isVisible())) return;
    await chip.click();
    await page.waitForFunction(
        () => !document.querySelector('.save-chip-unsaved') && !document.querySelector('.save-chip-saving'),
        { timeout: 15000 },
    ).catch(() => {});
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'lounge');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="lounge"]', { timeout: 10000 });

    await selectTreeNode(page, 'lounge');
    await addElement(page, 'Add Object in "lounge"', 'Bob');

    // --- VaryingText1.png: Bob's "Look at" description, as text, with an {either} directive ---
    await openTab(page, 'Setup');
    await selectLabeledField(page, 'object description:', 'string');
    const bobDescription = page.locator('textarea');
    await bobDescription.fill(
        '{either Bob.alive:Bob is sitting up, appearing to feel somewhat under the weather.'
        + '|Bob is lying on the floor, a lot more still than usual.}'
    );
    await capture(page, out('VaryingText1.png'), { untilLocator: bobDescription });

    // --- VaryingText2.png: the text box toolbar's "Insert" menu, open ---
    await settleSave(page);
    const insertButton = page.locator('button[aria-haspopup="menu"]:has-text("Insert")').first();
    await insertButton.click();
    const menu = page.locator('[role="menu"]').first();
    await menu.waitFor({ timeout: 5000 });
    await capture(page, out('VaryingText2.png'), { untilLocator: menu, padding: 16 });
});
