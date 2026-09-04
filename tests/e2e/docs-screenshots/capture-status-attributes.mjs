// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/status_attributes.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'player');
    await openTab(page, 'Attributes');
    const attrNameInput = page.getByPlaceholder('Attribute', { exact: true });
    await attrNameInput.fill('score');
    const formatInput = page.getByPlaceholder('Format string (optional)');
    await formatInput.click();
    await page.waitForTimeout(200);
    await capture(page, out('status2.png'), { untilLocator: formatInput, padding: 200 });
});
