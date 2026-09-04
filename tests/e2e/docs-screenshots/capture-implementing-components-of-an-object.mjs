// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/other_guides/implementing_components_of_an_object.md.
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, toggleFeature, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images', 'other_guides');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'machine');
    await selectTreeNode(page, 'machine');
    await addElement(page, 'Add Object in "machine"', 'button');
    await selectTreeNode(page, 'machine');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.locator('select').first().selectOption({ label: 'Surface' });
    await page.waitForTimeout(200);
    await capture(page, out('Component.png'), { untilLocator: page.locator('select').first(), padding: 300 });
});
