// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/tutorial/moving-objects-during-the-game.md. See
// .claude/skills/docs-screenshots/SKILL.md.
//
// The window is an Openable/Closable object, which inherits "openable" but not
// "container_base" — so the Container tab's "After opening the object" (onopen) field
// isn't offered for it at all, and the chapter uses "Script to run when opening object"
// (openscript) instead. That script replaces the normal opening, hence the leading
// "Open object" command (HelperOpenObject, which only sets the state and prints nothing).
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, setLabeledField, addElement, openTab,
    toggleFeature, addScriptCommand, ifExpressionSelect, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

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
    await page.waitForSelector('text=Script to run when opening object:', { timeout: 10000 });

    // Every "+ Add script" button below the "Script to run when opening object:" label, in
    // document order. The script's own root button is *last*, after the rows it holds, so the
    // indices shift as nested blocks appear: [1] is the root until the "if" is added, then the
    // "then" block, and [2] becomes the "else" block once "+ else" has been clicked.
    const openLabel = page.getByText('Script to run when opening object:', { exact: true });
    const addScriptButton = n => openLabel.locator(`xpath=following::button[text()="+ Add script"][${n}]`);

    // --- Bee.png: open the window, then move the bee in only if it isn't already here ---
    await addScriptCommand(page, addScriptButton(1), { category: 'Objects', item: 'Open object' });
    await page.locator('xpath=//span[text()="Open object"]/following-sibling::select[2]')
        .selectOption({ label: 'window' });

    await addScriptCommand(page, addScriptButton(1), { category: 'Scripts', item: 'If...' });
    await page.waitForSelector('text=then');
    await ifExpressionSelect(page).selectOption('object contains');
    // Both object arguments ("Parent:"/"Contains child:") carry their own object/expression
    // toggle select, so the value selects are the 3rd and 5th following <select>.
    const objectSelects = page.locator('xpath=//span[text()="if"]/following-sibling::select');
    await objectSelects.nth(2).selectOption('kitchen');
    await objectSelects.nth(4).selectOption('bee');

    await addScriptCommand(page, addScriptButton(1));
    const thenInput = page.locator('xpath=//span[text()="then"]/following::*[self::input[@type="text"] or self::textarea][1]');
    await thenInput.fill('You open the window. Nothing much happens this time.');

    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await addScriptCommand(page, addScriptButton(2));
    const elseInput = page.locator('xpath=//span[text()="else"]/following::*[self::input[@type="text"] or self::textarea][1]');
    await elseInput.fill('You open the window, and a bee flies into the kitchen.');
    await addScriptCommand(page, addScriptButton(2), { category: 'Objects', item: 'Move object' });
    const moveSelects = page.locator('xpath=//span[text()="Move object"]/following-sibling::select');
    await moveSelects.nth(1).selectOption({ label: 'bee' });
    await moveSelects.nth(3).selectOption({ label: 'kitchen' });

    await capture(page, out('Bee.png'), {
        untilLocator: page.locator('button:has-text("+ else if")').first(),
        padding: 8,
    });
});
