// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/tutorial/score-and-winning.md. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    // --- ScoreFeature.png: the game's Features tab, with Score ticked ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await page.waitForSelector('text=Health', { timeout: 10000 });
    await toggleFeature(page, 'Score');
    const asktellRow = page.locator('text=Ask/Tell: players can ask or tell characters about selected topics');
    await capture(page, out('ScoreFeature.png'), { untilLocator: asktellRow, padding: 24 });

    // --- ScoreFinish.png: the garden's "after entering" script, scoring and finishing ---
    await selectTreeNode(page, 'game');
    await addElement(page, 'Add Room', 'garden');
    await selectTreeNode(page, 'garden');
    await openTab(page, 'Scripts');
    await page.waitForSelector('text=After entering the room:', { timeout: 10000 });
    const afterEnterCodeView = page.getByText('After entering the room:', { exact: true })
        .locator('xpath=following::button[contains(.,"Code view")][1]');
    await setScriptCodeView(page, afterEnterCodeView, `IncreaseScore (3)
msg ("You step out into the garden and close the door behind you.")
msg ("")
msg ("<b>You have escaped the house, with " + game.score + " points out of 10.</b>")
finish`);
    await page.waitForSelector('text=Increase score', { timeout: 10000 });
    await capture(page, out('ScoreFinish.png'), {
        untilLocator: page.getByText('After leaving the room:', { exact: true }).first(),
        padding: 16,
    });
});
