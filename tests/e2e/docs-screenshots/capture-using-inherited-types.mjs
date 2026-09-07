// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/using_inherited_types.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, toggleFeature, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'chest');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Container:');
    await openTab(page, 'Container');
    await page.locator('select').first().selectOption({ label: 'Closed container' });
    await openTab(page, 'Attributes');
    await page.waitForTimeout(200);
    await capture(page, out('type_attributes.png'), { untilLocator: page.locator('text=Inherited Types').first(), padding: 300 });
});
