// Regenerates 1 of the 2 editor screenshots embedded in
// site/src/content/docs/changing_templates.md (Templates.png). Showlibraryelements.png
// already shows the current AppShell UI correctly and doesn't need regenerating.
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await page.click('[title="Tree view options"]');
    await page.getByText('Show Library Elements').click();
    await page.waitForTimeout(200);
    await page.locator('[data-value="_advanced"][data-part="branch-control"]').locator('..')
        .locator('[data-part="branch-indicator"], svg').first().click();
    await page.waitForTimeout(200);
    await page.getByText('Templates', { exact: true }).click();
    await page.waitForTimeout(200);
    await capture(page, out('Templates.png'), { untilLocator: page.locator('button:has-text("+ Add Template")'), padding: 600 });
});
