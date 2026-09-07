// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/tutorial/custom_attributes.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    setLabeledField, selectLabeledField, fieldByLabel, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');

    // --- Weightflour.png: flour object, Attributes tab, "weight" = Integer 500 ---
    await addElement(page, 'Add Object in "room"', 'flour');
    await openTab(page, 'Attributes');
    const addAttrInput = page.locator('input[placeholder="Add attribute..."]');
    await addAttrInput.fill('weight');
    await addAttrInput.locator('..').locator('button:has-text("Add")').click();
    await page.waitForSelector('text=ASSIGNMENT');
    await addAttrInput.fill('');
    const typeSelect = page.locator('select').filter({ hasText: 'String dictionary' });
    await typeSelect.selectOption('Integer');
    // Rendered as "Value" — uppercase is CSS-only (text-transform), not the actual DOM text.
    const valueInput = page.getByText('Value', { exact: true }).locator('xpath=following::*[self::input or self::textarea][1]');
    await valueInput.fill('500');
    await capture(page, out('Weightflour.png'), { untilLocator: valueInput });

    // --- Printexpression.png: eggs object, Setup tab, "Look at" -> Run script -> Print (expression) ---
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'eggs');
    await openTab(page, 'Setup');
    await selectLabeledField(page, 'object description:', 'script');
    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    const printTypeSelect = page.locator('xpath=//span[text()="Print"]/following-sibling::select[1]');
    await printTypeSelect.selectOption('expression');
    const exprInput = page.locator('xpath=//span[text()="Print"]/following::*[self::input or self::textarea][1]');
    await exprInput.fill('"A box of eggs, weighing " + eggs.weight + " grams."');
    await exprInput.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('Printexpression.png'), { untilLocator: exprInput, cursorAt: printTypeSelect });
});
