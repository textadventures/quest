// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/cloak_of_darkness.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, fieldByLabel, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Cloak of Darkness');
    await fieldByLabel(page, 'Author:').fill('The Pixie');
    const descInput = fieldByLabel(page, 'Description:');
    await descInput.fill('From the specification here:\nhttp://www.firthworks.com/roger/cloak/');
    await page.waitForTimeout(200);
    await capture(page, out('cod01.png'), { untilLocator: descInput, padding: 40 });
});
