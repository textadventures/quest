// Regenerates the 2 distinct editor screenshots embedded in site/src/content/docs/convert.md
// (make1.png is reused 3 times in the doc for closely related states — this captures the
// primary, most-referenced one: the CmdMakeBow command's own script). See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Convert Test');

    await addElement(page, 'Add Room', 'nowhere');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'string');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'branch');
    await selectTreeNode(page, 'nowhere');
    await addElement(page, 'Add Object in "nowhere"', 'bow');

    // --- make1.png: the CmdMakeBow command's pattern + script ---
    await selectTreeNode(page, 'room');
    await page.click('button[title="Add element"]');
    await page.getByRole('button', { name: 'Add Command to "room"', exact: true }).click();
    await page.waitForSelector('text=Pattern:', { timeout: 10000 });
    // Scope away from the tree's own "Filter..." textbox (input[type="text"] index 0 on the
    // whole page) by anchoring off the "Command" properties-panel tab, not a bare .first().
    const patternField = page.getByRole('button', { name: 'Command', exact: true })
        .locator('xpath=following::input[@type="text"]').first();
    await patternField.fill('make bow;construct bow');
    const nameField = page.getByText('Name:', { exact: true }).locator('..').locator('input[type="text"]');
    await nameField.fill('CmdMakeBow');

    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `if (not Got(branch)) {
msg ("You need some wood to make a bow.")
}
else if (not Got(string)) {
msg ("You need some string to make a bow.")
}
else {
MoveObject(bow, player)
MoveObject(string, nowhere)
MoveObject(branch, nowhere)
msg("You fasten the string to each end of the branch. Now you have a bow! Of sorts...")
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    const lastRow = page.locator('button:has-text("+ Add script")').last();
    await capture(page, out('make1.png'), { untilLocator: lastRow, padding: 40 });

    // --- make2.png: string object's Use/Give "Use (other object) on this" section, branch
    // added, script "do (CmdMakeBow, \"script\")" ---
    // Note: the "Run object" picker this renders as (the generic named-parameter form for the
    // Do() builtin) only lists true Object elements, not Commands — so it can't show
    // "CmdMakeBow" selected even though the script attribute ("script") is correctly parsed.
    // This is consistent with every other object picker in the editor (Commands, like
    // Functions/Timers, aren't Objects), not a capture bug — captured faithfully as-is.
    await selectTreeNode(page, 'string');
    await openTab(page, 'Features');
    // "Use/Give:" is a checkbox row whose full label is a long sentence starting with
    // "Use/Give:" — match by prefix, not exact, since the checkbox's accessible container
    // wraps that entire sentence as one text node.
    await page.locator('text=/^Use\\/Give:/').locator('..').locator('input[type="checkbox"]').check();
    await openTab(page, 'Use/Give');
    await page.waitForSelector('text=USE (OTHER OBJECT) ON THIS', { timeout: 10000 });
    const useOtherActionSelect = page.locator('text=USE (OTHER OBJECT) ON THIS')
        .locator('xpath=following::select[1]');
    await useOtherActionSelect.selectOption({ label: 'Handle objects individually' });
    const addObjectSelect = page.locator('text=USE (OTHER OBJECT) ON THIS')
        .locator('xpath=following::select').nth(1);
    await addObjectSelect.selectOption({ label: 'branch' });
    await page.locator('text=USE (OTHER OBJECT) ON THIS').locator('xpath=following::button[contains(., "Add")]').first().click();
    await page.waitForSelector('button:has-text("branch")', { timeout: 10000 });

    const branchCodeView = page.locator('button:has-text("branch")')
        .locator('xpath=following::button[contains(., "Code view")]').first();
    await setScriptCodeView(page, branchCodeView, `do (CmdMakeBow, "script")`);
    await page.waitForSelector('text=Run object', { timeout: 5000 });
    const lastRow2 = page.locator('button:has-text("+ Add script")').last();
    await capture(page, out('make2.png'), { untilLocator: lastRow2, padding: 40 });
});
