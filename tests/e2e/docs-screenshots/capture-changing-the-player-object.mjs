// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/changing_the_player_object.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, toggleFeature, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Bob');
    await selectTreeNode(page, 'Bob');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Player:');
    await openTab(page, 'Player');
    await page.locator('select').first().selectOption({ label: 'Can be a player' });
    await page.waitForTimeout(200);
    await capture(page, out('Pov1.png'), { untilLocator: page.locator('select').first(), padding: 400 });
});
