// Regenerates the 3 editor/player screenshots embedded in
// site/src/content/docs/tutorial/interacting_with_objects.md
// (Takedrop.png, Switchonoff.png, Switchonoffplay.png).
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, selectLabeledField, openPreview, sendCommand, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');

    // --- Takedrop.png: newspaper object, Inventory tab, "Object can be taken" ticked ---
    await addElement(page, 'Add Object in "room"', 'newspaper');
    await openTab(page, 'Inventory');
    const takeCheckbox = page.getByText('Object can be taken', { exact: true }).locator('..').locator('input[type="checkbox"]');
    await takeCheckbox.check();
    const takeMessageInput = page.getByText('Take message (leave blank for default):', { exact: true })
        .locator('xpath=following::input[1]');
    await takeMessageInput.fill('You fold the newspaper and place it neatly under your arm.');
    await capture(page, out('Takedrop.png'), { untilLocator: takeMessageInput });

    // --- Switchonoff.png: TV object, Switchable feature, "Can be switched on/off" ---
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'TV');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Switchable:');
    await openTab(page, 'Switchable');
    await page.locator('select').first().selectOption({ label: 'Can be switched on/off' });
    const onMessageInput = page.getByText('Message to print when switching on (leave blank for default):', { exact: true })
        .locator('xpath=following::input[1]');
    await onMessageInput.fill('You switch it on.');
    const offMessageInput = page.getByText('Message to print when switching off (leave blank for default):', { exact: true })
        .locator('xpath=following::input[1]');
    await offMessageInput.fill('You switch it off.');
    const extraOnInput = page.getByText('Extra object description when switched on:', { exact: true })
        .locator('xpath=following::input[1]');
    await extraOnInput.fill('It is currently showing an old western.');
    const extraOffInput = page.getByText('Extra object description when switched off:', { exact: true })
        .locator('xpath=following::input[1]');
    await extraOffInput.fill('It is currently switched off.');
    await capture(page, out('Switchonoff.png'), { untilLocator: extraOffInput });

    // --- Switchonoffplay.png: playing the game, switching the TV on ---
    // Doc text: "Go back to the Setup tab and change the 'Look at' description
    // so it just reads 'The TV is an old model, possibly 20 years old.'"
    await openTab(page, 'Setup');
    await selectLabeledField(page, 'object description:', 'string');
    await page.locator('textarea').fill('The TV is an old model, possibly 20 years old.');
    const playerPage = await openPreview(page);
    await sendCommand(playerPage, 'look at tv');
    await sendCommand(playerPage, 'switch on tv');
    await sendCommand(playerPage, 'look at tv');
    await capture(playerPage, out('Switchonoffplay.png'), { untilLocator: playerPage.locator('#txtCommand') });
});
