// Regenerates all 4 editor screenshots embedded in
// site/src/content/docs/tutorial/custom_commands.md (Commandsay.png, Commandweigh.png,
// Checkforattribute.png, Say_to_troll.png). Say_to_troll.png (the "Additional Example
// (Advanced)" using a Switch script command with per-object cases) was blocked on the Switch
// case-list editor (task_082ae91c); now fixed upstream (PR #2090). The doc gives no exact code
// for this example beyond the pattern and "switch command ... different response for different
// characters, and a default too" — the case/message content here is a reasonable invented
// match for that description, not transcribed from the doc.
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, addScriptCommand, setScriptCodeView, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

// Unlike "Add Object", "Add Command" creates the element immediately with no name modal
// (see tests/e2e/verify-appshell-multi-control-editors.mjs, which this script's Pattern-field
// selectors are copied from).
async function addCommand(page) {
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Command to")', { timeout: 5000 });
    await page.waitForSelector('text=Command:', { timeout: 10000 });
}

// Pattern field has no id/aria-label - scoped to its own row two levels up from the
// "Pattern:" label (span -> label/select row -> flex-col wrapper) so a plain
// input[type=text].first() doesn't instead match the sidebar's "Filter..." box.
function patternInput(page) {
    const patternRow = page.getByText('Pattern:', { exact: true }).locator('xpath=../..');
    return patternRow.locator(':scope > input[type=text]');
}

const addScriptButtons = page => page.locator('button:has-text("+ Add script")');

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');

    // --- Commandsay.png: "say #text#" command, print (expression) with escaped quotes ---
    await addCommand(page);
    await patternInput(page).fill('say #text#');
    await addScriptCommand(page, addScriptButtons(page).first());
    const sayPrintType = page.locator('xpath=//span[text()="Print"]/following-sibling::select[1]');
    await sayPrintType.selectOption('expression');
    const sayExprInput = page.locator('xpath=//span[text()="Print"]/following::input[1]');
    await sayExprInput.fill('"You say \\"" + text + "\\", but nobody replies."');
    await sayExprInput.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('Commandsay.png'), { untilLocator: sayExprInput });

    // --- Commandweigh.png: "weigh #object#" command, print (expression) reading object.weight ---
    await selectTreeNode(page, 'room');
    await addCommand(page);
    await patternInput(page).fill('weigh #object#');
    await addScriptCommand(page, addScriptButtons(page).first());
    const weighPrintType = page.locator('xpath=//span[text()="Print"]/following-sibling::select[1]');
    await weighPrintType.selectOption('expression');
    const weighExprInput = page.locator('xpath=//span[text()="Print"]/following::input[1]');
    await weighExprInput.fill('"It weighs " + object.weight + " grams."');
    await weighExprInput.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('Commandweigh.png'), { untilLocator: weighExprInput });

    // --- Checkforattribute.png: same "weigh" command, flat print replaced by an
    // if(object has attribute)/then/else wrapping it ---
    await page.locator('button[title="Delete"]').first().click();
    await page.waitForTimeout(300);
    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'If...' });
    const ifSelect = page.locator('xpath=(//span[text()="if"]/following-sibling::select[1])[1]');
    await ifSelect.selectOption('object has attribute');
    // Two inputs follow: the object expression (defaults to "object", left as-is) and the
    // attribute name (defaults to an empty quoted string) — take the second by position.
    const attrNameInput = page.locator('xpath=//span[text()="if"]/following-sibling::input[2]');
    await attrNameInput.fill('weight');

    // The "Then" block's own "+ Add script" is the first on screen at this point (the if's
    // else-branch doesn't exist yet, so the only other one is the outer command-level button).
    await addScriptCommand(page, addScriptButtons(page).first());
    const thenPrintType = page.locator('xpath=//span[text()="Print"]/following-sibling::select[1]');
    await thenPrintType.selectOption('expression');
    const thenExprInput = page.locator('xpath=//span[text()="Print"]/following::input[1]');
    await thenExprInput.fill('"It weighs " + object.weight + " grams."');
    await thenExprInput.evaluate(el => { el.scrollLeft = 0; });

    await page.getByRole('button', { name: '+ else', exact: true }).click();
    // Now 3 "+ Add script" buttons exist: Then's, the newly-created Else's, and the outer
    // command-level one - the Else block's is the middle one.
    await addScriptCommand(page, addScriptButtons(page).nth(1));
    const elseMsgInput = page.locator('xpath=//span[text()="else"]/following::input[@type="text"][1]');
    await elseMsgInput.fill("You can't weigh that.");
    await capture(page, out('Checkforattribute.png'), { untilLocator: elseMsgInput });

    // --- Say_to_troll.png: "say #text_talk# to #object_one#" command, switch(object_one)
    // with a case for the troll and a default for anyone/anything else ---
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'troll');
    await selectTreeNode(page, 'room');
    await addCommand(page);
    await patternInput(page).fill('say #text_talk# to #object_one#');
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `switch (object_one) {
case (troll) {
msg ("'Ugh,' grunts the troll, only half listening.")
}
default {
msg (GetDisplayName(object_one) + " doesn't seem interested in talking about that.")
}
}`);
    await page.waitForSelector('text=Switch:', { timeout: 5000 });
    const trollCaseToggles = page.getByRole('button', { name: '▶' });
    while (await trollCaseToggles.count() > 0) {
        await trollCaseToggles.first().click();
    }
    await capture(page, out('Say_to_troll.png'), { untilLocator: addScriptButtons(page).last(), padding: 40 });
});
