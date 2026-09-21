// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/commands.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addScriptCommand, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const addScriptButtons = page => page.locator('button:has-text("+ Add script")');

async function addCommand(page) {
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Command to")', { timeout: 5000 });
    await page.waitForSelector('text=Command:', { timeout: 10000 });
}

function patternInput(page) {
    const patternRow = page.getByText('Pattern:', { exact: true }).locator('xpath=../..');
    return patternRow.locator(':scope > input[type=text]');
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');

    // --- CommandHelp.png: "help" command, print message ---
    await addCommand(page);
    await patternInput(page).fill('help');
    await addScriptCommand(page, addScriptButtons(page).first());
    const helpMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await helpMsg.fill("You're on your own with this one.");
    await helpMsg.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('CommandHelp.png'), { untilLocator: helpMsg, padding: 40 });

});
