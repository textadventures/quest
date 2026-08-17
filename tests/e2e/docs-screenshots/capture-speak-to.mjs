// Regenerates 1 of the 2 editor screenshots embedded in
// site/src/content/docs/speak_to.md (Talk1.png). Talk2.png uses a Switch script
// command whose case-list editor isn't implemented in AppShell yet (same gap as
// custom_commands.md's Say_to_troll.png, see task_082ae91c) - left for later.
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addVerb, addScriptCommand, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Boris');
    await selectTreeNode(page, 'Boris');
    await openTab(page, 'Verbs');
    await addVerb(page, 'speak');
    await page.locator('select').first().selectOption('script');
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const msg1 = page.locator('xpath=//span[text()="Print"]/following-sibling::input[1]');
    await msg1.fill("'Hi,' you say to Boris, 'can you help me find the key to this door?'");
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const msg2 = page.locator('xpath=(//span[text()="Print"])[2]/following-sibling::input[1]');
    await msg2.fill("'Sure, you need to look in the bedroom.'");
    await page.waitForTimeout(200);
    await capture(page, out('Talk1.png'), { untilLocator: msg2, padding: 40 });
});
