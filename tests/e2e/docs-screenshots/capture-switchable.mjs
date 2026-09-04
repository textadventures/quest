// Regenerates the 7 editor screenshots embedded in site/src/content/docs/switchable.md. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, selectLabeledField, setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const codeViewBtn = page => page.locator('button:has-text("Code view")').first();
const lastAddScript = page => page.locator('button:has-text("+ Add script")').last();

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'machine');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'crystal ball');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'generator');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'light');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'rabbit');

    // --- switchbasic.png: machine, Switchable feature turned on, "Can be switched on/off" ---
    await selectTreeNode(page, 'machine');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Switchable:');
    await openTab(page, 'Switchable');
    await selectLabeledField(page, 'Switchable:', 'switchable');
    await page.waitForSelector('text=Switched on at the start of the game', { timeout: 10000 });
    const extraOnField = page.getByText('Extra object description when switched on:', { exact: true })
        .locator('xpath=following::input[1]');
    await extraOnField.fill(' It is chugging away to itself.');
    await capture(page, out('switchbasic.png'), {
        untilLocator: page.getByText('After switching off the object:', { exact: true })
            .locator('xpath=following::button[contains(.,"Code view")][1]'),
        padding: 40,
    });

    // --- switchlookat.png: machine's "Look at" description set to Run script ---
    await openTab(page, 'Setup');
    await page.getByText('"Look at" object description:', { exact: true })
        .locator('xpath=following::select[1]').selectOption({ label: 'Run script' });
    await setScriptCodeView(page, codeViewBtn(page), `if (this.switchedon) {
msg ("A funny looking machine chugging away.")
}
else {
msg ("A funny looking machine.")
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    await capture(page, out('switchlookat.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- switchpower.png: a "power" Command setting machine.cannotswitchon = null ---
    await selectTreeNode(page, 'room');
    await page.click('button[title="Add element"]');
    await page.getByRole('menuitem', { name: 'Add Command to "room"', exact: true }).click();
    await page.waitForSelector('text=Pattern:', { timeout: 10000 });
    const patternField = page.getByRole('button', { name: 'Command', exact: true })
        .locator('xpath=following::input[@type="text"]').first();
    await patternField.fill('power');
    await setScriptCodeView(page, codeViewBtn(page), `machine.cannotswitchon = null`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });
    await capture(page, out('switchpower.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- switchstate.png: crystal ball's "Use (on its own)" script checking machine.switchedon ---
    await selectTreeNode(page, 'crystal ball');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Use/Give:');
    await openTab(page, 'Use/Give');
    await page.waitForSelector('text=USE (ON ITS OWN)', { timeout: 10000 });
    const useOwnActionSelect = page.locator('text=USE (ON ITS OWN)').locator('xpath=following::select[1]');
    await useOwnActionSelect.selectOption({ label: 'Run script' });
    await setScriptCodeView(page, codeViewBtn(page), `if (machine.switchedon) {
msg ("You consult the crystal ball, and learn all sorts of stuff.")
}
else {
msg ("The crystal ball is dark for some reason.")
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    await capture(page, out('switchstate.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- switchgenerator.png: generator, Switchable on, both turn-on/turn-off scripts filled ---
    await selectTreeNode(page, 'generator');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Switchable:');
    await openTab(page, 'Switchable');
    await selectLabeledField(page, 'Switchable:', 'switchable');
    await page.waitForSelector('text=After switching on the object:', { timeout: 10000 });
    const onCodeView = page.getByText('After switching on the object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, onCodeView, `light.lightsource = true
light.look = "A light, shining brightly."
machine.cannotswitchon = null`);
    await page.waitForSelector('text=After switching off the object:', { timeout: 10000 });
    const offCodeView = page.getByText('After switching off the object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, offCodeView, `light.lightsource = false
light.look = "A light."
machine.cannotswitchon = "No power!"
if (machine.switchedon) {
msg("The machine stops when the power fails.")
}
machine.switchedon = false`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    await capture(page, out('switchgenerator.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- switchmoment.png: machine's "After switching on" script cloning a rabbit + SwitchOff ---
    await selectTreeNode(page, 'machine');
    await openTab(page, 'Switchable');
    await page.waitForSelector('text=After switching on the object:', { timeout: 10000 });
    const machineOnCodeView = page.getByText('After switching on the object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, machineOnCodeView, `CloneObjectAndMove (rabbit, player.parent)
SwitchOff (machine)`);
    await page.waitForSelector('text=Clone object', { timeout: 5000 });
    await capture(page, out('switchmoment.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- switchdisplayverbs.png: generator's "After switching on" script, extended with
    // this.displayverbs = Split(...) ---
    await selectTreeNode(page, 'generator');
    await openTab(page, 'Switchable');
    await page.waitForSelector('text=After switching on the object:', { timeout: 10000 });
    const generatorOnCodeView2 = page.getByText('After switching on the object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, generatorOnCodeView2, `light.lightsource = true
light.look = "A light, shining brightly."
machine.cannotswitchon = null
this.displayverbs = Split("Look at;Switch off", ";")`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });
    await capture(page, out('switchdisplayverbs.png'), { untilLocator: lastAddScript(page), padding: 40 });
});
