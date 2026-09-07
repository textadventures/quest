// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/howto/scripting/copy_and_paste_code.md — the TV's "look" script shown
// in Code View, reusing its "Look at" description text from
// tutorial/interacting_with_objects.md ("The TV is an old model, possibly 20 years old.") as a
// script rather than plain text, matching the doc's own framing. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    selectLabeledField, addScriptCommand, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'TV');
    await selectTreeNode(page, 'TV');
    await openTab(page, 'Setup');
    await selectLabeledField(page, 'object description:', 'script');
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const msgInput = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await msgInput.fill('The TV is an old model, possibly 20 years old.');

    await page.click('button:has-text("Code view")');
    const cm = page.locator('.cm-editor .cm-content').first();
    await cm.waitFor({ timeout: 5000 });
    await capture(page, out('codeview_web.png'), { untilLocator: cm, padding: 40 });
});
