// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/editor_user_interface_elements.md. See
// .claude/skills/docs-screenshots/SKILL.md.
//
// The doc's example <editor> XML block (adding "Enable timer" to a "Timers"
// category) is not hypothetical library-author sample code that needs to be
// injected — it's a verbatim copy of the real, already-shipped registration in
// src/Engine/Core/CoreEditorScriptsTimers.aslx (confirmed: pasting the doc's
// snippet as a duplicate top-level <editor> via the raw XML view throws
// Argument_AddingDuplicateWithKey, (function)EnableTimer). So this just opens
// the real Add Script Command dialog and navigates to the real Timers category.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Scripts');

    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('option', { name: 'Timers', exact: true }).click();
    await page.waitForSelector('text=ADVANCED');
    await capture(page, out('Editorui1.png'), { untilLocator: page.locator('[role="dialog"]') });

    await page.getByRole('option', { name: /^●\s*Enable timer$/ }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.waitForSelector('text=Enable timer');
    const timerRow = page.locator('text=Enable timer').locator('..');
    await capture(page, out('Editorui2.png'), { untilLocator: timerRow, padding: 60 });
});
