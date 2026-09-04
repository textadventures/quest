// Regenerates the 8 editor screenshots embedded in site/src/content/docs/adding_sounds.md
// (audio_controls.jpg is an in-game HTML-audio-tag screenshot, not editor chrome — out of
// scope for this script, deferred separately). See .claude/skills/docs-screenshots/SKILL.md.
//
// "play sound (filename, wait, loop)" / "stop sound" syntax confirmed from
// src/Engine/Core/CoreEditorScriptsOutput.aslx's <editor> blocks for these two commands.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, addVerb, addScriptCommand, setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Scripts');

    // --- play_a_sound.jpg: the "Add Script Command" dialog on "Play a sound" ---
    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command', { timeout: 10000 });
    await page.getByRole('option', { name: /^●\s*Play a sound$/ }).click();
    await capture(page, out('play_a_sound.jpg'), {
        untilLocator: page.locator('[role="dialog"]'),
    });
    await page.getByRole('button', { name: 'OK', exact: true }).click();

    // --- play_a_sound_GUI.jpg: the resulting command, filename filled, wait+loop shown ---
    await page.waitForSelector('text=Play sound', { timeout: 5000 });
    const soundFilenameField = page.locator('xpath=//span[text()="Play sound"]/following::input[1]');
    await soundFilenameField.fill('ambient sound.mp3');
    const waitSelect = page.getByText('Wait for sound to finish before continuing:', { exact: true })
        .locator('xpath=following::select[1]');
    await waitSelect.selectOption({ label: 'no' });
    const loopSelect = page.getByText('Loop:', { exact: true }).first().locator('xpath=following::select[1]');
    await loopSelect.selectOption({ label: 'no' });
    await capture(page, out('play_a_sound_GUI.jpg'), { untilLocator: loopSelect, padding: 40 });

    // --- stop_sound.jpg: the "Stop sound" command (no fields) ---
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `stop sound`);
    await page.waitForSelector('text=Stop sound', { timeout: 5000 });
    await capture(page, out('stop_sound.jpg'), {
        untilLocator: page.getByText('Stop sound', { exact: true }),
        padding: 40,
    });
    // Clear the Start script back out before moving on to room/object scripts.
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), ``);

    // --- Set up the objects the rest of the page's examples reference ---
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Door');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'button');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Room in "room"', 'Hall of Silence');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Room in "room"', 'Sound Effects Room');

    // Rename the default room to "Hub" to match the doc's narrative.
    await selectTreeNode(page, 'room');
    await page.locator('span:has-text("Name:")').locator('..').locator('input').fill('Hub');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="Hub"]', { timeout: 10000 });
    // Renaming collapsed the room's tree node — re-expand so its children (Door, button) are
    // clickable again. Scope to Hub specifically: "game" also has its own Expand button (it
    // always has Verbs/Commands subgroups), so a page-wide .first() would grab the wrong one.
    await page.locator('[data-value="Hub"][data-part="branch-control"]')
        .getByRole('button', { name: 'Expand' }).click();

    // --- Door: Container feature, Openable/Closable, open/close scripts ---
    await selectTreeNode(page, 'Door');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.waitForSelector('text=Container type:', { timeout: 10000 });
    await page.locator('text=Container type:').locator('xpath=following::select[1]').selectOption({ label: 'Openable/Closable' });
    await page.waitForSelector('text=Script to run when opening object:', { timeout: 10000 });
    const doorOpenCodeView = page.getByText('Script to run when opening object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, doorOpenCodeView, `play sound ("door_creak.mp3", false, false)`);
    await page.waitForSelector('text=Script to run when closing object:', { timeout: 10000 });
    const doorCloseCodeView = page.getByText('Script to run when closing object:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, doorCloseCodeView, `stop sound`);
    await page.waitForSelector('text=Play sound', { timeout: 5000 });
    await capture(page, out('play_audio_example4_door.jpg'), {
        untilLocator: page.getByText('Script to run when closing object:', { exact: true }).locator('xpath=following::input[1]'),
        padding: 200,
    });

    // --- Hub room: after entering / after leaving scripts (ambient loop tied to Door) ---
    await selectTreeNode(page, 'Hub');
    await openTab(page, 'Scripts');
    await page.waitForSelector('text=After entering the room:', { timeout: 10000 });
    const hubEnterCodeView = page.getByText('After entering the room:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, hubEnterCodeView, `if (Door.isopen) {
play sound ("ambient sound.mp3", false, true)
}`);
    await page.waitForSelector('text=After leaving the room:', { timeout: 10000 });
    const hubLeaveCodeView = page.getByText('After leaving the room:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, hubLeaveCodeView, `stop sound`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    await capture(page, out('play_audio_example1_loop.jpg'), {
        untilLocator: page.getByText('After leaving the room:', { exact: true }).locator('xpath=following::input[1]'),
        padding: 200,
    });

    // --- Hall of Silence: before entering the room -> stop sound ---
    await selectTreeNode(page, 'Hall of Silence');
    await openTab(page, 'Scripts');
    await page.waitForSelector('text=Before entering the room:', { timeout: 10000 });
    const hallCodeView = page.getByText('Before entering the room:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, hallCodeView, `stop sound`);
    await page.waitForSelector('text=Stop sound', { timeout: 5000 });
    await capture(page, out('stop_audio_example1.jpg'), {
        untilLocator: page.getByText('Stop sound', { exact: true }),
        padding: 60,
    });

    // --- Sound Effects Room: before entering the room -> message + play sound with wait=yes ---
    await selectTreeNode(page, 'Sound Effects Room');
    await openTab(page, 'Scripts');
    await page.waitForSelector('text=Before entering the room:', { timeout: 10000 });
    const sfxCodeView = page.getByText('Before entering the room:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, sfxCodeView, `msg ("A strange sound echoes through the corridor.")
play sound ("effect.mp3", true, false)`);
    await page.waitForSelector('text=Play sound', { timeout: 5000 });
    await capture(page, out('play_audio_example2_sync.jpg'), {
        untilLocator: page.locator('button:has-text("+ Add script")').first(),
        padding: 200,
    });

    // --- button: "press" verb, run script -> play a click sound ---
    await selectTreeNode(page, 'button');
    await openTab(page, 'Verbs');
    await page.waitForSelector('text=No verbs added yet', { timeout: 10000 });
    const verbInput = page.locator('button:has-text("Add Verb")').locator('..').locator('input');
    await verbInput.fill('press');
    await page.click('button:has-text("Add Verb")');
    await page.waitForSelector('td:has-text("press")', { timeout: 10000 });
    await page.click('td:has-text("press")');
    const behaviourTypeSelect = page.locator('text=Type').first().locator('xpath=following::select[1]');
    await behaviourTypeSelect.selectOption({ label: 'Run a script' });
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `play sound ("click.mp3", false, false)`);
    await page.waitForSelector('text=Play sound', { timeout: 5000 });
    await capture(page, out('play_audio_example3_button.jpg'), {
        untilLocator: page.locator('button:has-text("+ Add script")').last(),
        padding: 40,
    });
});
