// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/tutorial/moving_objects_during_the_game.md. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab, toggleFeature,
    ifExpressionSelect, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'bee');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'window');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.locator('select').first().selectOption({ label: 'Openable/Closable' });

    // --- Bee.png: "if object contains kitchen/bee" with then/else print+move scripts, as the
    // "script to run when opening object" ---
    const openLabel = page.getByText('Script to run when opening object:', { exact: true });
    const addScriptButton = n => openLabel.locator(`xpath=following::button[text()="+ Add script"][${n}]`);
    await addScriptButton(1).click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('option', { name: 'Scripts', exact: true }).click();
    await page.getByRole('option', { name: 'If...' }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.waitForSelector('text=then');
    await ifExpressionSelect(page).selectOption('object contains');
    const objectSelects = page.locator('xpath=//span[text()="if"]/following-sibling::select');
    await objectSelects.nth(1).selectOption('room');
    await objectSelects.nth(2).selectOption('bee');

    // then: print a message
    await addScriptButton(1).click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    const thenInput = page.locator('xpath=//span[text()="then"]/following::input[@type="text"][1]');
    await thenInput.fill('You open the window. Not much happens.');

    // else: print a message + move the bee
    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await addScriptButton(2).click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    const elseInput = page.locator('xpath=//span[text()="else"]/following::input[@type="text"][1]');
    await elseInput.fill('You open the window and a bee flies into the kitchen.');
    await addScriptButton(2).click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('option', { name: 'Objects', exact: true }).click();
    await page.getByRole('option', { name: /^●\s*Move object$/ }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();

    const elseIfButton = page.locator('button:has-text("+ else if")');
    await capture(page, out('Bee.png'), { untilLocator: elseIfButton, padding: 4 });
});
