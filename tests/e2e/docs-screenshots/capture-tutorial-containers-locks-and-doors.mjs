// Regenerates the 3 editor screenshots embedded in
// site/src/content/docs/tutorial/containers-locks-and-doors.md. Walks the same steps
// the chapter tells the reader to take, in the kitchen of the running tutorial game.
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    setLabeledField, toggleFeature, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    // The chapter continues from the tutorial's kitchen, so rename the starting room
    // rather than adding a third one the reader wouldn't recognise.
    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'kitchen');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="kitchen"]', { timeout: 10000 });
    // Add the garden from the "game" node, not from the kitchen - "Add Room" with a room
    // selected offers to nest the new room inside it, which isn't what the chapter describes.
    await selectTreeNode(page, 'game');
    await addElement(page, 'Add Room', 'garden');

    await selectTreeNode(page, 'kitchen');
    await addElement(page, 'Add Object in "kitchen"', 'cupboard');

    // --- TutorialCupboard.png: Container feature on, "Closed container" chosen ---
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.waitForSelector('text=Container type:', { timeout: 10000 });
    await page.locator('text=Container type:').locator('xpath=following::select[1]')
        .selectOption({ label: 'Closed container' });
    await page.waitForSelector('text=Transparent', { timeout: 10000 });
    await capture(page, out('TutorialCupboard.png'), {
        untilLocator: page.getByText('Transparent', { exact: false }).first(),
        padding: 24,
    });

    // --- TutorialListChildren.png: the Advanced section, with "List children..." ticked ---
    const advanced = page.locator('summary:has-text("Advanced")').first();
    await advanced.click();
    const listChildren = page.locator('text=List children when object is looked at or opened');
    await listChildren.waitFor({ timeout: 10000 });
    await listChildren.locator('..').locator('input[type="checkbox"]').check();
    // The Advanced section sits at the very bottom of a long tab - scroll it up to the top
    // of the pane so the crop (which always starts at the top of the viewport) frames it.
    await advanced.evaluate(el => el.scrollIntoView({ block: 'start' }));
    await page.waitForTimeout(300);
    await capture(page, out('TutorialListChildren.png'), { untilLocator: listChildren, padding: 24 });

    // --- Create the south exit from the kitchen to the garden ---
    await selectTreeNode(page, 'kitchen');
    await openTab(page, 'Exits');
    await page.getByRole('button', { name: 'south', exact: true }).click();
    const destCombobox = page.locator('[role="combobox"]');
    await destCombobox.click();
    await destCombobox.fill('garden');
    await page.waitForSelector('[role="option"]:has-text("garden")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("garden")');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=south → garden', { timeout: 10000 });

    // --- TutorialLockedExit.png: the exit's own tab, locked, with a message and a name ---
    // Select the exit itself via its tree row - the "south → garden" summary link on the
    // Exits tab selects the destination room instead (see capture-exits.mjs).
    await page.getByText('Exit: garden', { exact: true }).click();
    await page.waitForSelector('button:has-text("Exit")', { timeout: 10000 });
    await setLabeledField(page, 'Name:', 'garden exit');
    const lockedCheckbox = page.locator('text=Locked').first().locator('..').locator('input[type="checkbox"]');
    await lockedCheckbox.check();
    const lockedMessage = page.locator('text=Print message when locked').first()
        .locator('xpath=following::input[1]');
    await lockedMessage.fill('The back door is locked.');
    await capture(page, out('TutorialLockedExit.png'), { untilLocator: lockedMessage, padding: 24 });
});
