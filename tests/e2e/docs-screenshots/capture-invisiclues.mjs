// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/other_guides/invisiclues.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, setScriptCodeView, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images', 'other_guides');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    // --- invisiclues.png: a "help2" Command, pattern "help;?", the InvisiClues foreach script ---
    await selectTreeNode(page, 'room');
    await page.click('button[title="Add element"]');
    await page.getByRole('menuitem', { name: 'Add Command to "room"', exact: true }).click();
    await page.waitForSelector('text=Pattern:', { timeout: 10000 });
    const patternField = page.getByRole('button', { name: 'Command', exact: true })
        .locator('xpath=following::input[@type="text"]').first();
    await patternField.fill('help;?');
    const nameField = page.getByText('Name:', { exact: true }).locator('..').locator('input[type="text"]');
    await nameField.fill('help2');

    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `if (HasAttribute(game, "defaultbackground")) {
bg = LCase (game.defaultbackground)
}
else {
bg = "white"
}
msg ("Drag your mouse over the text to reveal only the clues you need.")
foreach (key, game.helpdict) {
msg ("<i>" + key + "</i> [<font color=\\"" + bg + "\\">" + StringDictionaryItem(game.helpdict, key) + "</font>]")
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    await capture(page, out('invisiclues.png'), {
        untilLocator: page.locator('button:has-text("+ Add script")').last(),
        padding: 40,
    });

    // --- setup_hints.png: game's Start script builds game.helpdict as a string dictionary ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Scripts');
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `game.helpdict = NewStringDictionary()
dictionary add (game.helpdict, "How do I go north?", "Open the door!")
dictionary add (game.helpdict, "How do I open the door?", "Type OPEN DOOR!")
dictionary add (game.helpdict, "How do I kill the bugbear?", "There are allergic to jam...")
dictionary add (game.helpdict, "How do I kill the bugbear with jam?", "Perhaps you could give him a sandwich?")`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });
    await capture(page, out('setup_hints.png'), {
        untilLocator: page.locator('button:has-text("+ Add script")').first(),
        padding: 40,
    });
});
