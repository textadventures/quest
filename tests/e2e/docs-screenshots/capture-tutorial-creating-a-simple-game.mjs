// Regenerates the 7 editor screenshots embedded in
// site/src/content/docs/tutorial/creating_a_simple_game.md, which previously showed
// the old Quest 5 desktop/web editor. Walks through the same steps the tutorial
// prose itself describes, using the current AppShell editor, and saves each shot
// over the existing filename in site/public/images/ so no markdown changes are
// needed. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    setLabeledField, selectLabeledField, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    // --- Renameroom.png: rename the default "room" to "lounge" ---
    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'lounge');
    const defaultPrefixCheckbox = page.locator('text=Use default prefix and suffix');
    await capture(page, out('Renameroom.png'), { untilLocator: defaultPrefixCheckbox });

    // Commit the rename by switching tabs before continuing.
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="lounge"]', { timeout: 10000 });

    // --- Editroomdescription.png: enter the room description ---
    const roomDescription = page.locator('textarea');
    await roomDescription.fill(
        'This is quite a plain lounge with an old beige carpet and peeling wallpaper.'
    );
    await capture(page, out('Editroomdescription.png'), { untilLocator: roomDescription });

    // --- Add a second room, "kitchen" ---
    await addElement(page, 'Add Room', 'kitchen');

    // --- Addexit1.png: lounge's Exits tab, South cell clicked ---
    await selectTreeNode(page, 'lounge');
    await openTab(page, 'Exits');
    await page.getByRole('button', { name: 'south', exact: true }).click();
    const lookExitLink = page.locator('text=Create a look exit instead');
    await capture(page, out('Addexit1.png'), { untilLocator: lookExitLink });

    // --- Addexit2.png: kitchen chosen as destination, ready to create ---
    const combobox = page.locator('[role="combobox"]');
    await combobox.click();
    await combobox.fill('kitchen');
    await page.waitForSelector('[role="option"]:has-text("kitchen")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("kitchen")');
    await capture(page, out('Addexit2.png'), { untilLocator: lookExitLink });
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=south → kitchen', { timeout: 10000 });

    // --- Add the TV object to the lounge ---
    await addElement(page, 'Add Object in "lounge"', 'TV');

    // --- Objectdescription.png: Setup tab, "Look at" description set to Text ---
    await openTab(page, 'Setup');
    await selectLabeledField(page, 'object description:', 'string');
    const objectDescription = page.locator('textarea');
    await objectDescription.fill(
        'The TV is an old model, possibly 20 years old. It is currently showing an old western.'
    );
    await capture(page, out('Objectdescription.png'), { untilLocator: objectDescription });

    // --- Othernames.png: Object tab, "television" added to Other names ---
    await openTab(page, 'Object');
    await page.waitForSelector('text=Other names:', { timeout: 15000 });
    // Placeholder uses a real ellipsis character (…), not three periods — match by prefix instead.
    const otherNameInput = page.locator('input[placeholder^="Add additional name"]');
    await otherNameInput.fill('television');
    await otherNameInput.locator('..').locator('button:has-text("Add")').click();
    await page.waitForSelector('text=television', { timeout: 10000 });
    // Crop below the input row — the "Hyperlink options"/"Display verbs" sections
    // further down aren't relevant to what this screenshot is illustrating.
    await capture(page, out('Othernames.png'), { untilLocator: otherNameInput });

    // --- Addverb.png: Verbs tab, "watch" verb with a print-message response ---
    await openTab(page, 'Verbs');
    await page.waitForSelector('text=No verbs added yet', { timeout: 15000 });
    const addVerbButton = page.locator('button:has-text("Add Verb")');
    await addVerbButton.locator('..').locator('input').fill('watch');
    await addVerbButton.click();
    await page.waitForSelector('text=watch', { timeout: 10000 });
    await page.click('td:has-text("watch")');
    const verbValue = page.locator('textarea');
    await verbValue.fill(
        "You watch for a few minutes. As your will to live slowly ebbs away, you remember that you've always hated watching westerns."
    );
    await capture(page, out('Addverb.png'), { untilLocator: verbValue });
});
