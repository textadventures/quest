// Regenerates the 5 editor screenshots embedded in
// site/src/content/docs/other_guides/timelimitedpuzzles.md (previously part of the deleted
// helpsheets/ track, kept because this page still references them). See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, setScriptCodeView, addScriptCommand, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images', 'helpsheets');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'cupboard');
    await selectTreeNode(page, 'cupboard');
    await addElement(page, 'Add Object in "cupboard"', 'alien');

    // --- Hsbaddy11.jpg: cupboard, Container feature on, type "Container", "Is open" unticked ---
    await selectTreeNode(page, 'cupboard');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.waitForSelector('text=Container type:', { timeout: 10000 });
    await page.locator('text=Container type:').locator('xpath=following::select[1]').selectOption({ label: 'Container' });
    const isOpenCheckbox = page.getByText('Is open', { exact: true }).locator('xpath=..').locator('input[type="checkbox"]');
    await isOpenCheckbox.uncheck();
    await capture(page, out('Hsbaddy11.jpg'), { untilLocator: isOpenCheckbox, padding: 40 });

    // --- Hsbaddy12.jpg: "After opening the object" -> Print a message ---
    const openScriptCodeView = page.getByText('After opening the object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, openScriptCodeView,
        `msg ("You have surprised the sleeping (and hungry) alien!")`);
    await page.waitForSelector('xpath=//span[text()="Print"]', { timeout: 5000 });
    const printInput = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await capture(page, out('Hsbaddy12.jpg'), { untilLocator: printInput, padding: 40 });

    // --- Hsbaddy13.jpg: same script, second sibling command "Run script after a number of
    // seconds" (Timers) added, freshly-added with an empty nested Run script - the doc fills
    // in "10" for the wait only after this screenshot, so add the command via the UI (not code
    // view) to capture its just-added default state before setting the value. ---
    const secondAddBtn = page.getByText('After opening the object:', { exact: true })
        .locator('xpath=following::button[contains(.,"+ Add script")][1]');
    await addScriptCommand(page, secondAddBtn, { category: 'Timers', item: 'Run script after a number of seconds' });
    await page.waitForSelector('xpath=//span[text()="After"]', { timeout: 5000 });
    const afterInput = page.getByText('After', { exact: true }).locator('xpath=following::input[1]');
    await capture(page, out('Hsbaddy13.jpg'), { untilLocator: afterInput, padding: 90 });

    // --- Hsbaddy14.jpg: fill "10" seconds, add an "If" inside the timer's own nested script
    // checking whether the alien is still visible, printing a message and finishing the game ---
    await afterInput.fill('10');
    const timerCodeView = page.getByText('After', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, timerCodeView,
        `if (GetBoolean(alien, "visible")) {
  msg ("The alien is still hungry, and finishes you off before you can react.")
  finish
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    const ifLastInput = page.locator('xpath=(//span[text()="if"]/following::input)[last()]');
    await capture(page, out('Hsbaddy14.jpg'), { untilLocator: ifLastInput, padding: 60 });

    // --- Hsbaddy15.jpg: new "flame thrower" object, Use/Give -> "Use this on (other object)"
    // handled individually for "alien" -> print message + Remove object ---
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'flame thrower');
    await selectTreeNode(page, 'flame thrower');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Use/Give:');
    await openTab(page, 'Use/Give');
    const useonSelect = page.locator('select').filter({ hasText: 'None' }).first();
    await useonSelect.selectOption('scriptdictionary');
    const useonSection = useonSelect.locator('xpath=../..');
    await page.waitForTimeout(200);
    const objectPicker = useonSection.locator('select').filter({ hasText: /alien/ }).first();
    await objectPicker.selectOption({ label: 'alien' });
    await useonSection.locator('button:has-text("Add")').click();
    await page.waitForTimeout(300);

    const flameCodeView = useonSection.locator('button:has-text("Code view")').first();
    await setScriptCodeView(page, flameCodeView,
        `msg ("You blast the alien with the flame thrower. It bursts into flames and is destroyed.")
RemoveObject (alien)`);
    await page.waitForSelector('xpath=//span[text()="Remove object"]', { timeout: 5000 });
    const removeObjectSelect = page.locator('xpath=//span[text()="Remove object"]/following-sibling::select[1]');
    await capture(page, out('Hsbaddy15.jpg'), { untilLocator: removeObjectSelect, padding: 40 });
});
