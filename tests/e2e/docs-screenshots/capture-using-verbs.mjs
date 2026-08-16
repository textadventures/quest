// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/using_verbs.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addVerb, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'dial');
    await openTab(page, 'Verbs');
    await addVerb(page, 'rotate');
    await selectTreeNode(page, 'game');
    await page.locator('[data-value="game"][data-part="branch-control"]').locator('..').locator('[data-part="branch-indicator"], svg').first().click();
    await page.waitForTimeout(200);
    await page.getByText('Verbs', { exact: true }).click();
    await page.waitForTimeout(200);
    await page.getByRole('button', { name: 'Verb: rotate' }).click();
    await page.getByRole('button', { name: 'Dismiss' }).click().catch(() => {});
    await page.waitForTimeout(200);
    const lastField = page.getByText('If no objects available, show this message:', { exact: true })
        .locator('xpath=following::input[1]');
    await capture(page, out('verb_element.png'), { untilLocator: lastField, padding: 40 });
});
