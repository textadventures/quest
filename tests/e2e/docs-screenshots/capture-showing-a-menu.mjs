// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/showing_a_menu.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addScriptCommand, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const addScriptButtons = page => page.locator('button:has-text("+ Add script")');

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Set a variable or attribute' });
    await page.waitForTimeout(200);
    const varNameInput = page.getByText('Set variable', { exact: true }).locator('xpath=following-sibling::input[1]');
    await varNameInput.fill('menulist');
    const varTypeSelect = page.getByText('Set variable', { exact: true }).locator('xpath=following-sibling::select[1]');
    await varTypeSelect.selectOption('new string list');
    await page.waitForTimeout(200);

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Add a value to a list' });
    const addToListLabel = page.getByText('Add to list', { exact: true });
    const addToListTextInputs = addToListLabel.locator('xpath=following::input[@type="text"]');
    await addToListTextInputs.nth(0).fill('menulist');
    await addToListTextInputs.nth(1).fill('female');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Add a value to a list' });
    const addToListLabel2 = page.getByText('Add to list', { exact: true }).nth(1);
    const addToListTextInputs2 = addToListLabel2.locator('xpath=following::input[@type="text"]');
    await addToListTextInputs2.nth(0).fill('menulist');
    await addToListTextInputs2.nth(1).fill('male');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Output', item: 'Show a menu' });
    await page.waitForTimeout(200);
    const captionInput = page.getByText('Show menu with caption', { exact: true }).locator('xpath=following::input[@type="text"][1]');
    await captionInput.fill('please choose now');
    const optionsInput = page.getByText('Options from list/dictionary:', { exact: true }).locator('xpath=following::input[@type="text"][1]');
    await optionsInput.fill('menulist');
    const ignoreSelect = page.getByText('Allow player to ignore the menu:', { exact: true }).locator('xpath=following::select[1]');
    await ignoreSelect.selectOption('no');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'If...' });
    await page.waitForTimeout(200);
    const ifExprInput = page.locator('xpath=(//span[text()="if"]/following::input[@type="text"])[1]');
    await ifExprInput.fill('result="male"');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Set a variable or attribute' });
    const thenLabel1 = page.getByText('Set variable', { exact: true }).nth(1);
    const thenInputs1 = thenLabel1.locator('xpath=following::input[@type="text"]');
    await thenInputs1.nth(0).fill('playername');
    await thenInputs1.nth(1).fill('"Ken"');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Set a variable or attribute' });
    const thenLabel2 = page.getByText('Set variable', { exact: true }).nth(2);
    const thenInputs2 = thenLabel2.locator('xpath=following::input[@type="text"]');
    await thenInputs2.nth(0).fill('gender');
    await thenInputs2.nth(1).fill('"male"');

    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await page.waitForTimeout(200);
    const elseLabelSpan = page.locator('xpath=(//span[text()="else"])[1]');
    const elseAddScriptBtn = elseLabelSpan.locator('xpath=following::button[contains(., "+ Add script")][1]');
    await addScriptCommand(page, elseAddScriptBtn, { category: 'Variables', item: 'Set a variable or attribute' });
    const elseInputs1 = elseLabelSpan.locator('xpath=following::input[@type="text"]');
    await elseInputs1.nth(0).fill('playername');
    await elseInputs1.nth(1).fill('"Barbie"');

    const elseAddScriptBtn2 = elseLabelSpan.locator('xpath=following::button[contains(., "+ Add script")][1]');
    await addScriptCommand(page, elseAddScriptBtn2, { category: 'Variables', item: 'Set a variable or attribute' });
    const elseInputs2 = elseLabelSpan.locator('xpath=following::input[@type="text"]');
    await elseInputs2.nth(2).fill('gender');
    await elseInputs2.nth(3).fill('"female"');

    // The After-choosing script's own outer "+ Add script" (a sibling of the if-block, not
    // nested in it) is the first one whose position in the DOM comes after the if-block's
    // own closing "+ else if" button - anchor from that button instead of the if-block itself.
    const elseIfBtn = page.locator('xpath=(//button[contains(., "+ else if")])[1]');
    const afterChoosingAddBtn = elseIfBtn.locator('xpath=following::button[contains(., "+ Add script")][1]');
    await addScriptCommand(page, afterChoosingAddBtn);
    const finalPrintType = page.locator('xpath=(//span[text()="Print"])[last()]/following-sibling::select[1]');
    await finalPrintType.selectOption('expression');
    const finalPrintInput = page.locator('xpath=(//span[text()="Print"])[last()]/following::input[1]');
    await finalPrintInput.fill('"You have chosen the " + result');
    await finalPrintInput.evaluate(el => { el.scrollLeft = 0; });
    await page.waitForTimeout(200);
    await capture(page, out('ShowMenu.png'), { untilLocator: finalPrintInput, padding: 40 });
});
