// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/tutorial/cloak-of-darkness.md. See
// .claude/skills/docs-screenshots/SKILL.md.
//
// The chapter's prose at this point is about the shape of the game - three rooms, and the
// cloak, hook and message - so the shot is of the tree with all of them in it, not of the
// game object's own Setup tab.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, setLabeledField, addElement, openTab,
    capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

async function expandBranch(page, name) {
    const branch = page.locator(`[data-part="branch"][data-value="${name}"]`);
    await branch.waitFor({ timeout: 10000 });
    if (await branch.getAttribute('aria-expanded') !== 'true') {
        await branch.locator('[data-part="branch-control"] button').first().click();
    }
    await page.locator(`[data-value="${name}"]`).first().waitFor({ timeout: 5000 });
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Cloak of Darkness');

    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'foyer');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="foyer"]', { timeout: 10000 });

    // "Add Room" with a room selected offers to nest the new room inside it, so add the
    // other two from the game node.
    await selectTreeNode(page, 'game');
    await addElement(page, 'Add Room', 'cloakroom');
    await selectTreeNode(page, 'game');
    await addElement(page, 'Add Room', 'bar');

    // The player sits inside the foyer, whose branch starts collapsed - its rows are in the
    // DOM but hidden, so expand it first via the chevron button on its branch control.
    await expandBranch(page, 'foyer');
    await selectTreeNode(page, 'player');
    await addElement(page, 'Add Object in "player"', 'cloak');
    await selectTreeNode(page, 'cloakroom');
    await addElement(page, 'Add Object in "cloakroom"', 'hook');
    await selectTreeNode(page, 'bar');
    await addElement(page, 'Add Object in "bar"', 'message');

    await selectTreeNode(page, 'foyer');
    await openTab(page, 'Setup');
    await capture(page, out('cod01.png'), {
        untilLocator: page.locator('[data-value="message"]').first(),
        padding: 40,
    });
});
