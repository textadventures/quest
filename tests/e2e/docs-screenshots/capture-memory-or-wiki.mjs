// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/memory_or_wiki.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, addScriptCommand, fieldByLabel, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const addScriptButtons = page => page.locator('button:has-text("+ Add script")');

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Ask/Tell:');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'memory');
    await selectTreeNode(page, 'memory');
    await page.getByRole('button', { name: 'Ask/Tell', exact: true }).click();
    const askInput = fieldByLabel(page, 'Ask about:');
    await askInput.fill('weddle hoots');
    await askInput.locator('xpath=../..').locator('button:has-text("Add")').click();
    await page.waitForTimeout(300);
    await addScriptCommand(page, addScriptButtons(page).first());
    const topicMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await topicMsg.fill('The Weddle-Hoots are an old aristocratic family.');
    await topicMsg.evaluate(el => { el.scrollLeft = 0; });

    // "Script to run when asked about an unknown topic:" lives behind the bottom-level
    // "ADVANCED" expander on the Ask/Tell tab (a plain <details>/<summary>, distinct from
    // the tree's own "Advanced" node).
    await page.locator('summary', { hasText: 'Advanced' }).click();
    const unknownLabel = page.getByText('Script to run when asked about an unknown topic:', { exact: true });
    const unknownAddBtn = unknownLabel.locator('xpath=following::button[contains(., "+ Add script")][1]');
    await addScriptCommand(page, unknownAddBtn);
    const unknownType = page.locator('xpath=(//span[text()="Print"])[last()]/following-sibling::select[1]');
    await unknownType.selectOption('expression');
    const unknownExpr = page.locator('xpath=(//span[text()="Print"])[last()]/following::*[self::input or self::textarea][1]');
    await unknownExpr.fill('"You remember nothing about " + text + "."');
    await unknownExpr.evaluate(el => { el.scrollLeft = 0; });
    await page.waitForTimeout(200);
    await capture(page, out('memory1.png'), { untilLocator: unknownExpr, padding: 40 });

    // --- memory2.png: "remember #text#" command, Call function DoAskTell with 5 params ---
    await selectTreeNode(page, 'room');
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Command to")', { timeout: 5000 });
    await page.waitForSelector('text=Command:', { timeout: 10000 });
    const patternRow = page.getByText('Pattern:', { exact: true }).locator('xpath=../..');
    await patternRow.locator(':scope > input[type=text]').fill('remember #text#');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'Call function' });
    const callFnLabel = page.getByText('Call function', { exact: true });
    const callFnInput = callFnLabel.locator('xpath=following::input[@type="text"][1]');
    await callFnInput.fill('DoAskTell');
    // Selecting the "Library functions" autocomplete suggestion (rather than just filling
    // the text) is what makes AppShell recognise the function and swap the generic
    // "+ param" list for named fields matching its actual signature (object/text/property/
    // defaultscript/defaulttemplate) - fill those directly instead of adding params.
    await page.getByText('DoAskTell', { exact: true }).last().click();
    await page.waitForTimeout(300);
    const defaultTemplateInput = page.locator('xpath=(//*[contains(text(),"defaulttemplate")]/following::input)[1]');
    await defaultTemplateInput.waitFor({ state: 'visible', timeout: 5000 });
    const propertyInput = page.locator('xpath=(//span[text()="property:"]/following::input)[1]');
    const defaultScriptInput = page.locator('xpath=(//span[text()="defaultscript:"]/following::input)[1]');
    await page.locator('xpath=(//span[text()="object:"]/following::input)[1]').fill('memory');
    await page.locator('xpath=(//span[text()="text:"]/following::input)[1]').fill('text');
    await propertyInput.fill('"ask"');
    await defaultScriptInput.fill('"askdefault"');
    await defaultTemplateInput.fill('"DefaultAsk"');
    for (const el of [propertyInput, defaultScriptInput, defaultTemplateInput]) {
        await el.evaluate(node => { node.scrollLeft = 0; });
    }
    await page.waitForTimeout(200);
    await capture(page, out('memory2.png'), { untilLocator: defaultTemplateInput, padding: 60 });
});
