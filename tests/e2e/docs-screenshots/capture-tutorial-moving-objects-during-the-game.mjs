// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/tutorial/moving-objects-during-the-game.md. See
// .claude/skills/docs-screenshots/SKILL.md.
//
// The window is an Openable/Closable object, with an opening message and an "After opening
// the object" (onopen) script, which runs once Quest Viva has opened it.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, setLabeledField, addElement, openTab,
    toggleFeature, addScriptCommand, ifExpressionSelect, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

async function pickObject(page, combobox, name) {
    await combobox.click();
    await combobox.fill(name);
    await page.locator('[role="option"]', { hasText: name }).first().click();
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'kitchen');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="kitchen"]', { timeout: 10000 });

    await selectTreeNode(page, 'kitchen');
    await addElement(page, 'Add Object in "kitchen"', 'bee');
    await selectTreeNode(page, 'kitchen');
    await addElement(page, 'Add Object in "kitchen"', 'window');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.waitForSelector('text=Container type:', { timeout: 10000 });
    await page.locator('text=Container type:').locator('xpath=following::select[1]')
        .selectOption({ label: 'Openable/Closable' });
    await page.waitForSelector('text=After opening the object:', { timeout: 10000 });
    await setLabeledField(page, 'Message to print when opening', 'You open the window.');

    // Every "+ Add script" button below the "After opening the object:" label, in
    // document order. The script's own root button is *last*, after the rows it holds, so the
    // indices shift as nested blocks appear: [1] is the root until the "if" is added, then the
    // "then" block, and [2] becomes the "else" block once "+ else" has been clicked.
    const openLabel = page.getByText('After opening the object:', { exact: true });
    const addScriptButton = n => openLabel.locator(`xpath=following::button[text()="+ Add script"][${n}]`);

    // --- Bee.png: move the bee in only if it isn't already here ---
    await addScriptCommand(page, addScriptButton(1), { category: 'Scripts', item: 'If...' });
    await page.waitForSelector('text=then');
    await ifExpressionSelect(page).selectOption('object contains');
    // Both object arguments ("Parent:"/"Contains child:") are object comboboxes.
    const ifObjects = page.locator('xpath=//span[text()="if"]/following::input[@role="combobox"]');
    await pickObject(page, ifObjects.nth(0), 'kitchen');
    await pickObject(page, ifObjects.nth(1), 'bee');

    await addScriptCommand(page, addScriptButton(1));
    const thenInput = page.locator('xpath=//span[text()="then"]/following::*[self::input[@type="text"] or self::textarea][1]');
    await thenInput.fill('The bee is still buzzing around.');

    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await addScriptCommand(page, addScriptButton(2));
    const elseInput = page.locator('xpath=//span[text()="else"]/following::*[self::input[@type="text"] or self::textarea][1]');
    await elseInput.fill('A bee flies in and starts buzzing around the kitchen.');
    await addScriptCommand(page, addScriptButton(2), { category: 'Objects', item: 'Move object' });
    const moveObjects = page.locator('xpath=//span[text()="Move object"]/following::input[@role="combobox"]');
    await pickObject(page, moveObjects.nth(0), 'bee');
    await pickObject(page, moveObjects.nth(1), 'kitchen');

    // Start the shot at the opening message rather than part-way through the note above it.
    await page.getByText('Message to print when opening', { exact: false }).first()
        .evaluate(el => el.scrollIntoView({ block: 'start' }));
    await capture(page, out('Bee.png'), {
        untilLocator: page.locator('button:has-text("+ else if")').first(),
        padding: 8,
    });
});
