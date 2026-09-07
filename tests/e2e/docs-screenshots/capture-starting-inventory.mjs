// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/other_guides/starting_inventory.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images', 'other_guides');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'player');
    await addElement(page, 'Add Object in "player"', 'torch');
    await selectTreeNode(page, 'player');
    await page.waitForTimeout(200);
    await capture(page, out('Howto_startinventory.jpg'), { untilLocator: page.locator('[data-value="torch"]').first(), padding: 300 });
});
