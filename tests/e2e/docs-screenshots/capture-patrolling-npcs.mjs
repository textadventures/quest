// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/patrolling_npcs.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addScriptCommand, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const addScriptButtons = page => page.locator('button:has-text("+ Add script")');

async function addAttribute(page, name, type) {
    const addAttrInput = page.locator('input[placeholder="Add attribute..."]');
    await addAttrInput.fill(name);
    await addAttrInput.locator('..').locator('button:has-text("Add")').click();
    await page.waitForTimeout(300);
    if (type) {
        const typeSelect = page.locator('select').filter({ hasText: 'String' }).first();
        await typeSelect.selectOption(type);
        await page.waitForTimeout(200);
    }
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Mary');
    await selectTreeNode(page, 'Mary');
    await openTab(page, 'Attributes');
    await addAttribute(page, 'route', 'String List');
    await addAttribute(page, 'patrolstate', 'Integer');
    await addAttribute(page, 'takeaturn', 'Script');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Set a variable or attribute' });
    const label1 = page.getByText('Set variable', { exact: true }).nth(0);
    const inputs1 = label1.locator('xpath=following::input[@type="text"]');
    await inputs1.nth(0).fill('oldroom');
    await inputs1.nth(1).fill('this.parent');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Set a variable or attribute' });
    const label2 = page.getByText('Set variable', { exact: true }).nth(1);
    const inputs2 = label2.locator('xpath=following::input[@type="text"]');
    await inputs2.nth(0).fill('this.patrolstate');
    await inputs2.nth(1).fill('(this.patrolstate + 1) % ListCount(this.route)');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Variables', item: 'Set a variable or attribute' });
    const label3 = page.getByText('Set variable', { exact: true }).nth(2);
    const inputs3 = label3.locator('xpath=following::input[@type="text"]');
    await inputs3.nth(0).fill('this.parent');
    await inputs3.nth(1).fill('GetObject(StringListItem(this.route, this.patrolstate))');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'If...' });
    const ifExprInput = page.locator('xpath=(//span[text()="if"]/following::input[@type="text"])[1]');
    await ifExprInput.fill('not oldroom = this.parent');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'Call function' });
    await page.waitForTimeout(200);
    const callFnLabel1 = page.getByText('Call function', { exact: true }).nth(0);
    const callFnInput1 = callFnLabel1.locator('xpath=following::input[@type="text"][1]');
    await callFnInput1.fill('PrintIfHere');
    const paramBtn1 = callFnLabel1.locator('xpath=following::button[contains(., "+ param")][1]');
    await paramBtn1.click();
    await page.waitForTimeout(200);
    await paramBtn1.click();
    await page.waitForTimeout(200);
    const allInputs1 = callFnLabel1.locator('xpath=following::input[@type="text"]');
    await allInputs1.nth(1).fill('oldroom');
    await allInputs1.nth(2).fill('"Mary leaves the room."');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'Call function' });
    await page.waitForTimeout(200);
    const callFnLabel2 = page.getByText('Call function', { exact: true }).nth(1);
    const callFnInput2 = callFnLabel2.locator('xpath=following::input[@type="text"][1]');
    await callFnInput2.fill('PrintIfHere');
    const paramBtn2 = callFnLabel2.locator('xpath=following::button[contains(., "+ param")][1]');
    await paramBtn2.click();
    await paramBtn2.click();
    await page.waitForTimeout(200);
    const allInputs2 = callFnLabel2.locator('xpath=following::input[@type="text"]');
    await allInputs2.nth(1).fill('this.parent');
    await allInputs2.nth(2).fill('"Mary enters the room."');
    await page.waitForTimeout(200);
    const lastParamInput = allInputs2.nth(2);
    await capture(page, out('patrol1.png'), { untilLocator: lastParamInput, padding: 60 });
});
