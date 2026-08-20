// Regenerates the 8 editor screenshots embedded in site/src/content/docs/containers.md. See
// .claude/skills/docs-screenshots/SKILL.md.
//
// Note: the doc's lockandkey.png section describes a "Require all keys" checkbox
// ("As of Quest 5.8, you can untick..."), but the current engine's container_lockable type
// (src/Engine/Core/CoreTypes.aslx) always calls AllKeysAvailable() unconditionally — there is
// no "require any key" code path and no such checkbox in the current Container tab. Captured
// faithfully without it (not a capture bug — the feature isn't present to capture).
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const codeViewBtn = page => page.locator('button:has-text("Code view")').first();
const lastAddScript = page => page.locator('button:has-text("+ Add script")').last();

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'chest');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'key');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'pixie');

    // --- container2.png: chest, Container feature on, "Container" type, default options ---
    await selectTreeNode(page, 'chest');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.waitForSelector('text=Container type:', { timeout: 10000 });
    await page.locator('text=Container type:').locator('xpath=following::select[1]').selectOption({ label: 'Container' });
    await page.waitForSelector('text=Script to run when trying to add an object:', { timeout: 10000 });
    await capture(page, out('container2.png'), {
        untilLocator: page.getByText('Script to run when trying to add an object:', { exact: true })
            .locator('xpath=following::button[contains(.,"Code view")][1]'),
        padding: 40,
    });

    // --- lockandkey.png: Closed container, Lockable, 1 key = "key" ---
    await page.locator('text=Container type:').locator('xpath=following::select[1]').selectOption({ label: 'Closed container' });
    await page.waitForSelector('text=LOCKING', { timeout: 10000 });
    await page.locator('text=Lock type:').locator('xpath=following::select[1]').selectOption({ label: 'Lockable' });
    await page.waitForSelector('text=Number of keys to unlock container:', { timeout: 10000 });
    const keyCountField = page.locator('text=Number of keys to unlock container:')
        .locator('xpath=following::input[1]');
    await keyCountField.click();
    await keyCountField.fill('1');
    await keyCountField.press('Tab');
    await page.waitForSelector('text=Key:', { timeout: 10000 });
    const keyCombobox = page.locator('text=Key:').first().locator('xpath=following::input[1]');
    await keyCombobox.click();
    await keyCombobox.fill('key');
    await page.waitForSelector('[role="option"]:has-text("key")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("key")');
    await page.waitForSelector('text=Automatically unlock if player has the key(s)', { timeout: 10000 });
    await capture(page, out('lockandkey.png'), {
        untilLocator: page.getByText('Automatically open when unlocked', { exact: true }),
        padding: 60,
    });

    // --- unlock.png: pixie's "talk" verb, run script, chest.locked = false ---
    await selectTreeNode(page, 'pixie');
    await openTab(page, 'Verbs');
    await page.waitForSelector('text=No verbs added yet', { timeout: 10000 });
    const verbInput = page.locator('button:has-text("Add Verb")').locator('..').locator('input');
    await verbInput.fill('talk');
    await page.click('button:has-text("Add Verb")');
    await page.waitForSelector('td:has-text("talk")', { timeout: 10000 });
    await page.click('td:has-text("talk")');
    const behaviourTypeSelect = page.locator('text=Type').first().locator('xpath=following::select[1]');
    await behaviourTypeSelect.selectOption({ label: 'Run a script' });
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `msg ("The pixie waves her wand, and you hear a click from the chest.")
chest.locked = false`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });
    await capture(page, out('unlock.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- limitbycount.png / limitbyvolume.png: a "backpack" Limited container ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Inventory limits:');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'backpack');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.waitForSelector('text=Container type:', { timeout: 10000 });
    await page.locator('text=Container type:').locator('xpath=following::select[1]').selectOption({ label: 'Limited container' });
    await page.waitForSelector('text=Maximum number of objects:', { timeout: 10000 });
    const maxObjectsField = page.getByText('Maximum number of objects:', { exact: true })
        .locator('xpath=following::input[1]');
    await maxObjectsField.fill('5');
    const countMessageField = page.getByText('Full container message (leave blank for default):', { exact: true }).first()
        .locator('xpath=following::input[1]');
    await countMessageField.fill("You can't fit anything else in the backpack.");
    await capture(page, out('limitbycount.png'), { untilLocator: countMessageField, padding: 60 });

    await maxObjectsField.fill('1000000');
    const volumeField = page.getByText('Maximum volume of objects:', { exact: true })
        .locator('xpath=following::input[1]');
    await volumeField.fill('50');
    await capture(page, out('limitbyvolume.png'), { untilLocator: volumeField, padding: 60 });

    // --- containeropenscript.png: chest, "After opening the object" trap script ---
    await selectTreeNode(page, 'chest');
    await openTab(page, 'Container');
    await page.waitForSelector('text=Container type:', { timeout: 10000 });
    await page.locator('text=Container type:').locator('xpath=following::select[1]').selectOption({ label: 'Container' });
    await page.waitForSelector('text=After opening the object:', { timeout: 10000 });
    const openScriptCodeView = page.getByText('After opening the object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, openScriptCodeView, `firsttime {
if (not GetBoolean(this, "disarmed")) {
msg ("As you open the chest, there is a sudden explosion! It was trapped.")
DecreaseHealth (20)
}
}`);
    await page.waitForSelector('text=The first time,', { timeout: 5000 });
    // Anchor to the "After closing the object:" label, not a global .last() "+ Add script" —
    // the page also has Locking-section "+ Add script" buttons further down, so .last() would
    // pull in the whole Locking section instead of cropping right after this script.
    await capture(page, out('containeropenscript.png'), {
        untilLocator: page.getByText('After closing the object:', { exact: true }),
        padding: 40,
    });

    // --- containerfussy.png: chest, "Script to run when trying to add an object" — clothing only ---
    await page.waitForSelector('text=Script to run when trying to add an object:', { timeout: 10000 });
    const addScriptCodeView = page.getByText('Script to run when trying to add an object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, addScriptCodeView, `if (DoesInherit(object, "wearable")) {
MoveObject (object, this)
msg ("You put " + object.article + " in the chest.")
}
else {
msg ("You can't put " + object.article + " in the chest; it only likes clothing!")
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    await capture(page, out('containerfussy.png'), {
        untilLocator: page.getByText('Locking', { exact: true }),
        padding: 40,
    });

    // --- containercounter.png: chest, same script slot — count items, finish at 3+ ---
    const addScriptCodeView2 = page.getByText('Script to run when trying to add an object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, addScriptCodeView2, `MoveObject (object, this)
msg ("You put " + object.article + " in the chest.")
if (ListCount(GetAllChildObjects(this)) > 2) {
msg ("Congratulations, you filled the chest, and completed your quest.")
finish
}`);
    await page.waitForSelector('text=Move object', { timeout: 5000 });
    await capture(page, out('containercounter.png'), {
        untilLocator: page.getByText('Locking', { exact: true }),
        padding: 40,
    });
});
