// Regenerates the 3 editor screenshots embedded in
// site/src/content/docs/howto/tasks/multiple_choices_using_a_switch_script.md — was blocked
// on the Switch case-list editor (task_082ae91c); now fixed upstream (PR #2090). See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addVerb,
    setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const expandAllCases = async page => {
    const toggles = page.getByRole('button', { name: '▶' });
    while (await toggles.count() > 0) {
        await toggles.first().click();
    }
};

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Mary');
    await selectTreeNode(page, 'Mary');
    await openTab(page, 'Setup');
    await page.getByText('Type:', { exact: true }).first().locator('xpath=following::select[1]').selectOption({ label: 'Female character (named)' });
    await openTab(page, 'Verbs');
    await addVerb(page, 'Speak to');
    await page.locator('select').first().selectOption('script');
    const codeViewBtn = page.locator('button:has-text("Code view")').first();

    // --- switch01.png: ShowMenu with a freshly-added, empty "Switch (result)" ---
    await setScriptCodeView(page, codeViewBtn, `options = Split ("The weather;Her hair;The Lost Key of Arenbos", ";")
ShowMenu ("Talk about?", options, true) {
switch (result) {
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await capture(page, out('switch01.png'), { untilLocator: page.locator('button:has-text("+ Add script")').last(), padding: 40 });

    // --- switch02.png: one case, "The weather", printing Mary's reply ---
    await setScriptCodeView(page, codeViewBtn, `options = Split ("The weather;Her hair;The Lost Key of Arenbos", ";")
ShowMenu ("Talk about?", options, true) {
switch (result) {
case ("The weather") {
msg ("'Hasn't it been awful,' says Mary.")
}
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await expandAllCases(page);
    await capture(page, out('switch02.png'), { untilLocator: page.locator('button:has-text("+ Add script")').last(), padding: 40 });

    // --- switch04.png: all three cases plus a default, to show the "now there are three
    // Add new script buttons" state described just before this image ---
    await setScriptCodeView(page, codeViewBtn, `options = Split ("The weather;Her hair;The Lost Key of Arenbos", ";")
ShowMenu ("Talk about?", options, true) {
switch (result) {
case ("The weather") {
msg ("'Hasn't it been awful,' says Mary.")
}
case ("Her hair") {
msg ("'Do you like it this colour?' she asks.")
}
case ("The Lost Key of Arenbos") {
msg ("'Oh, I suppose you want it back.'")
MoveObject (The Lost Key of Arenbos, player)
}
default {
msg ("That was not even an option!")
}
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await expandAllCases(page);
    await page.waitForSelector('text=Default:', { timeout: 5000 });
    await capture(page, out('switch04.png'), { untilLocator: page.locator('button:has-text("+ Add script")').last(), padding: 40 });
});
