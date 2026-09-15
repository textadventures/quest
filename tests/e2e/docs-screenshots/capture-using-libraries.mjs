// Regenerates the editor screenshot embedded in both
// site/src/content/docs/advanced-topics/using-libraries.md and
// site/src/content/docs/howto/world/changing-templates.md (Showlibraryelements.png):
// the tree view options menu open, pointing at "Show Library Elements".
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await page.click('[title="Tree view options"]');
    const menuItem = page.getByRole('menuitem', { name: 'Show Library Elements' });
    await menuItem.waitFor({ timeout: 5000 });
    await capture(page, out('Showlibraryelements.png'), {
        untilLocator: menuItem,
        padding: 320,
        cursorAt: { locator: menuItem, at: 'center' },
    });
});
