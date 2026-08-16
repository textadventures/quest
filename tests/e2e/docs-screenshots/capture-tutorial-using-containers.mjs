// Regenerates the 3 editor screenshots plus 1 player screenshot embedded in
// site/src/content/docs/tutorial/using_containers.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, setLabeledField, openPreview, sendCommand, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');

    // --- Container.png: fridge object, Features -> Container -> Closed container ---
    await addElement(page, 'Add Object in "room"', 'fridge');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.locator('select').first().selectOption({ label: 'Closed container' });
    const closeMessageInput = page.locator('text=Message to print when closing').locator('..').locator('input');
    await capture(page, out('Container.png'), { untilLocator: closeMessageInput });

    // --- Containerfridge.png: contents prefix customised (shown before "It contains ..." text) ---
    await selectTreeNode(page, 'fridge');
    await addElement(page, 'Add Object in "fridge"', 'milk');
    await selectTreeNode(page, 'fridge');
    await addElement(page, 'Add Object in "fridge"', 'cheese');
    await selectTreeNode(page, 'fridge');
    await addElement(page, 'Add Object in "fridge"', 'beer');
    await selectTreeNode(page, 'fridge');
    await openTab(page, 'Container');
    // "List children when object is looked at or opened" is <advanced/> in CoreEditorObjectContainer.aslx,
    // so it's folded into the collapsed "Advanced" expander at the bottom of the tab — without checking
    // it, "Contents prefix" has no effect on the actual game output (see capture-tutorial-using-containers
    // spike notes / project memory: the prefix field itself still renders and accepts input even while
    // this checkbox is off, which silently produces a screenshot that doesn't match the player transcript).
    await page.locator('summary', { hasText: 'Advanced' }).click();
    await page.locator('text=List children when object is looked at or opened').locator('..').locator('input[type="checkbox"]').check();
    const contentsPrefixInput = page.locator('text=Contents prefix').locator('..').locator('input');
    await contentsPrefixInput.fill('It contains');
    await capture(page, out('Containerfridge.png'), { untilLocator: contentsPrefixInput });

    // --- Containerfridgeplayer.png: playing the game, opening the fridge to see the custom prefix ---
    const playerPage = await openPreview(page);
    await sendCommand(playerPage, 'open fridge');
    await capture(playerPage, out('Containerfridgeplayer.png'), { untilLocator: playerPage.locator('#txtCommand') });

    // --- Lockedcontainer.png: box object, Locking section, Lockable ---
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'key');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'box');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.locator('select').first().selectOption({ label: 'Closed container' });
    const lockTypeSelect = page.getByText('Lock type:', { exact: true }).locator('xpath=following::select[1]');
    await lockTypeSelect.selectOption({ label: 'Lockable' });
    const keyCountInput = page.getByText('Number of keys to unlock container:', { exact: true })
        .locator('xpath=following::input[1]');
    await keyCountInput.fill('1');
    await capture(page, out('Lockedcontainer.png'), { untilLocator: keyCountInput });
});
