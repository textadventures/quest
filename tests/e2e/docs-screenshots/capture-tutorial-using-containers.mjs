// Regenerates the 3 editor screenshots embedded in
// site/src/content/docs/tutorial/using_containers.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, setLabeledField, capture,
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
    await openTab(page, 'Container');
    const contentsPrefixInput = page.locator('text=Contents prefix').locator('..').locator('input');
    await contentsPrefixInput.fill('It contains');
    await capture(page, out('Containerfridge.png'), { untilLocator: contentsPrefixInput });

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
