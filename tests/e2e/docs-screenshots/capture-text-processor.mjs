// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/text_processor.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await openTab(page, 'Room');
    const descInput = page.locator('.cm-content, textarea, [contenteditable="true"]').first();
    await descInput.click();
    await descInput.fill('This is a small dungeon. {once:There is a bad smell in here.}').catch(async () => {
        await descInput.pressSequentially('This is a small dungeon. {once:There is a bad smell in here.}');
    });
    await page.waitForTimeout(200);
    await capture(page, out('text_processor_text.png'), { untilLocator: descInput, padding: 100 });
});
