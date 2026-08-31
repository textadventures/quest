// Regenerates all 7 editor screenshots embedded in
// site/src/content/docs/tutorial/more_things_to_do_with_objects.md. Add.png is a tiny inline
// icon crop (the Ask/Tell "Add" button referenced mid-sentence, not a full screenshot) - it
// uses locator.screenshot() directly instead of this file's other capture() calls.
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, addAdvancedElement, openTab,
    toggleFeature, addScriptCommand, selectLabeledField, fieldByLabel,
    ifExpressionSelect, ifObjectSelect, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const addScriptButtons = page => page.locator('button:has-text("+ Add script")');

// Fills the flag-name text input on a just-added "if" row (the field immediately following
// the object picker <select>, once "object has flag" is selected as the expression type) —
// it's the first plain input after the "if" label in document order.
function ifFlagNameInput(page) {
    return page.locator('xpath=(//span[text()="if"]/following::input)[1]');
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Bob');
    await selectTreeNode(page, 'Bob');

    // --- Use1.png: Bob's "look at" description -> if(object has flag, Bob, alive)/then/else ---
    await openTab(page, 'Setup');
    await selectLabeledField(page, 'object description:', 'script');
    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'If...' });
    await ifExpressionSelect(page).selectOption('object has flag');
    await ifObjectSelect(page, { selectIndex: 3 }).selectOption({ label: 'Bob' });
    await ifFlagNameInput(page).fill('alive');

    await addScriptCommand(page, addScriptButtons(page).first());
    const use1ThenMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await use1ThenMsg.fill('Bob is sitting up, appearing to feel somewhat under the weather.');
    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await addScriptCommand(page, addScriptButtons(page).nth(1));
    const use1ElseMsg = page.locator('xpath=(//span[text()="Print"])[2]/following-sibling::textarea[1]');
    await use1ElseMsg.fill('Bob is lying on the floor, a lot more still than usual.');
    await capture(page, out('Use1.png'), { untilLocator: use1ElseMsg });

    // --- Use2.png: Bob's Use/Give tab, "Use (other object) on this" -> Handle objects individually ---
    await openTab(page, 'Features');
    await toggleFeature(page, 'Use/Give:');
    await openTab(page, 'Use/Give');
    const useonSelect = page.locator('select').filter({ hasText: 'None' }).first();
    await useonSelect.selectOption('scriptdictionary');
    await capture(page, out('Use2.png'), { untilLocator: useonSelect });

    // --- Use3.png: add "defibrillator" to the per-object list, fill its script ---
    const useonSection = useonSelect.locator('xpath=../..');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'defibrillator');
    await selectTreeNode(page, 'Bob');
    await openTab(page, 'Use/Give');
    const objectPicker = useonSection.locator('select').filter({ hasText: /defibrillator/ }).first();
    await objectPicker.selectOption({ label: 'defibrillator' });
    await useonSection.locator('button:has-text("Add")').click();
    await page.waitForTimeout(300);

    await addScriptCommand(page, useonSection.locator('button:has-text("+ Add script")').first());
    const use3Msg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await use3Msg.fill('Miraculously, the defibrillator lived up to its promise, and Bob is now alive again. He says his head feels kind of fuzzy.');
    await addScriptCommand(page, useonSection.locator('button:has-text("+ Add script")').last(), { category: 'Variables', item: 'Set object flag' });
    const use3FlagObject = page.locator('xpath=//span[text()="Set flag"]/following-sibling::select[2]');
    await use3FlagObject.selectOption({ label: 'Bob' });
    const use3FlagName = page.locator('xpath=//span[text()="Set flag"]/following-sibling::input[1]');
    await use3FlagName.fill('alive');
    await capture(page, out('Use3.png'), { untilLocator: use3FlagName });

    // --- Functionrevive.png: new Function "revive bob" with the same print+set-flag script ---
    await addAdvancedElement(page, 'Function', 'revive bob');
    await addScriptCommand(page, addScriptButtons(page).first());
    const funcMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await funcMsg.fill('Miraculously, the defibrillator lived up to its promise, and Bob is now alive again. He says his head feels kind of fuzzy.');
    await addScriptCommand(page, addScriptButtons(page).last(), { category: 'Variables', item: 'Set object flag' });
    const funcFlagObject = page.locator('xpath=//span[text()="Set flag"]/following-sibling::select[2]');
    await funcFlagObject.selectOption({ label: 'Bob' });
    const funcFlagName = page.locator('xpath=//span[text()="Set flag"]/following-sibling::input[1]');
    await funcFlagName.fill('alive');
    await capture(page, out('Functionrevive.png'), { untilLocator: funcFlagName });

    // --- Functionrevive2.png: defibrillator's own "Use (on its own)" -> Run script -> Call function ---
    await selectTreeNode(page, 'defibrillator');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Use/Give:');
    await openTab(page, 'Use/Give');
    const useOwnActionSelect = page.locator('select').first();
    await useOwnActionSelect.selectOption({ label: 'Run script' });
    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'Call function' });
    const callFnInput = page.locator('xpath=//*[contains(text(), "Call function")]/following::input[@type="text"][1]');
    await callFnInput.fill('revive bob');
    // Dismiss the function-name autocomplete popover (which would otherwise overlap the crop)
    // by clicking the section heading instead of pressing Escape - Escape clears the field.
    await page.getByText('Use (on its own)', { exact: true }).click();
    await capture(page, out('Functionrevive2.png'), { untilLocator: callFnInput });

    // --- Asktell.png: global Ask/Tell feature, Bob's Ask/Tell tab, one topic with a guarded script ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Ask/Tell:');
    await selectTreeNode(page, 'Bob');
    await page.getByRole('button', { name: 'Ask/Tell', exact: true }).click();
    const askInput = fieldByLabel(page, 'Ask about:');
    await askInput.fill('heart attack cardiac arrest');
    const askAddButton = askInput.locator('xpath=../..').locator('button:has-text("Add")');
    await askAddButton.screenshot({ path: out('Add.png') });
    await askAddButton.click();
    await page.waitForTimeout(300);

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'If...' });
    await ifExpressionSelect(page).selectOption('object has flag');
    await ifObjectSelect(page, { selectIndex: 3 }).selectOption({ label: 'Bob' });
    await ifFlagNameInput(page).fill('alive');
    await addScriptCommand(page, addScriptButtons(page).first());
    const askMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await askMsg.fill("Well, one moment I was sitting there, feeling pretty happy with myself after eating my afternoon snack - a cheeseburger, pizza and ice cream pie, smothered in bacon, which I'd washed down with a bucket of coffee and six cans of Red Bull - when all of a sudden, I was in terrible pain, and then everything was peaceful. Then you came along.");
    await askMsg.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('Asktell.png'), { untilLocator: askMsg });
});
