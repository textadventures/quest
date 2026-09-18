// Regenerates the 2 editor screenshots embedded in site/src/content/docs/howto/world/exits.md. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    setLabeledField, setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    // Rename the default room to "kitchen" (matches the doc's narrative continuing from the
    // main tutorial's kitchen), add "garden" (the locked-door destination) and "room2" (the
    // portal destination used later in the Exit script examples).
    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'kitchen');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="kitchen"]', { timeout: 10000 });
    await addElement(page, 'Add Room', 'garden');
    await addElement(page, 'Add Room', 'room2');

    // "door" (the object whose "unlock" verb the doc describes) and "talisman" (the object
    // checked by Got() in the conditional-move example) both live in the kitchen.
    await selectTreeNode(page, 'kitchen');
    await addElement(page, 'Add Object in "kitchen"', 'door');
    await selectTreeNode(page, 'kitchen');
    await addElement(page, 'Add Object in "kitchen"', 'talisman');

    // --- Create the south exit to garden ---
    await selectTreeNode(page, 'kitchen');
    await openTab(page, 'Exits');
    await page.getByRole('button', { name: 'south', exact: true }).click();
    const destCombobox = page.locator('[role="combobox"]');
    await destCombobox.click();
    await destCombobox.fill('garden');
    await page.waitForSelector('[role="option"]:has-text("garden")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("garden")');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=south → garden', { timeout: 10000 });

    // --- Name the exit (the doc's Name section); no screenshot here any more - the locked-door
    // example moved to howto/world/doors.md ---
    // Select the newly created (still auto-named "Exit: garden") exit via its tree row, not
    // the "south → garden" summary link on the Exits tab — that link selects the destination
    // room ("garden"), not the exit itself.
    await page.getByText('Exit: garden', { exact: true }).click();
    await page.waitForSelector('button:has-text("Exit")', { timeout: 10000 });
    await setLabeledField(page, 'Name:', 'garden exit');

    // --- Exit script examples: enable
    // "Run a script" and type each example directly into the script's Code view ---
    await page.getByText('Run a script (instead of moving the player automatically)', { exact: true })
        .locator('..').locator('input[type="checkbox"]').check();
    await page.waitForSelector('text=Script to run:', { timeout: 5000 });

    // --- exitscript1.png: conditional move based on Got(talisman) ---
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `if (Got(talisman)) {
msg ("The talisman hums as you pass through the portal.")
MoveObject (player, room2)
}
else {
msg ("For some reason you cannot get through the portal.")
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    const lastScript1Row = page.locator('button:has-text("+ Add script")').last();
    await capture(page, out('exitscript1.png'), { untilLocator: lastScript1Row, padding: 40 });

    // --- exitscript2.png: firsttime + MoveObject ---
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `firsttime {
msg ("As you walk down the path, the sky darkens alarmingly ")
SetObjectFlagOn (player, "apocolyse started")
}
MoveObject (player, room2)`);
    await page.waitForSelector('text=The first time,', { timeout: 5000 });
    const lastScript2Row = page.locator('button:has-text("+ Add script")').last();
    await capture(page, out('exitscript2.png'), { untilLocator: lastScript2Row, padding: 40 });
});
