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
    const helpMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::input[1]');
    await helpMsg.fill("You're on your own with this one.");
    await helpMsg.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('CommandHelp.png'), { untilLocator: helpMsg, padding: 40 });

    // --- CommandAttack.png: "attack #object#;strike #object#;hit #object#" command,
    // nested if/else if/else checklist ---
    await selectTreeNode(page, 'room');
    await addCommand(page);
    await patternInput(page).fill('attack #object#;strike #object#;hit #object#');

    await addScriptCommand(page, addScriptButtons(page).first(), { category: 'Scripts', item: 'If...' });
    const ifExpr = page.locator('xpath=(//span[text()="if"]/following::input[@type="text"])[1]');
    await ifExpr.fill('not HasAttribute(object, "enemy")');
    await addScriptCommand(page, addScriptButtons(page).first());
    const msg1 = page.locator('xpath=//span[text()="Print"]/following-sibling::input[1]');
    await msg1.fill('You should not attack that.');
    await msg1.evaluate(el => { el.scrollLeft = 0; });

    await page.getByRole('button', { name: '+ else if', exact: true }).click();
    // Anchor from the specific "else if" span occurrence (parens around the span selector
    // itself), not from the union of all "else if" spans' following inputs - the latter
    // silently resolves to a stale index into an earlier branch once more than one
    // "else if" exists (see project memory: same class of bug as ShowMenu.png).
    const elseIf1Expr = page.locator('xpath=(//span[text()="else if"])[1]/following::input[@type="text"][1]');
    await elseIf1Expr.fill('not object.alive');
    await addScriptCommand(page, addScriptButtons(page).nth(1));
    const msg2 = page.locator('xpath=(//span[text()="Print"])[2]/following-sibling::input[1]');
    await msg2.fill('It is already dead.');
    await msg2.evaluate(el => { el.scrollLeft = 0; });

    await page.getByRole('button', { name: '+ else if', exact: true }).click();
    const elseIf2Expr = page.locator('xpath=(//span[text()="else if"])[2]/following::input[@type="text"][1]');
    await elseIf2Expr.fill('not HasObject(player, "weapon")');
    await addScriptCommand(page, addScriptButtons(page).nth(2));
    const msg3 = page.locator('xpath=(//span[text()="Print"])[3]/following-sibling::input[1]');
    await msg3.fill('Not advisable without a weapon.');
    await msg3.evaluate(el => { el.scrollLeft = 0; });

    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await addScriptCommand(page, addScriptButtons(page).nth(3));
    const msg4 = page.locator('xpath=(//span[text()="Print"])[4]/following-sibling::input[1]');
    await msg4.fill('You attack it with all your might.');
    await msg4.evaluate(el => { el.scrollLeft = 0; });
    await page.waitForTimeout(200);
    await capture(page, out('CommandAttack.png'), { untilLocator: msg4, padding: 60 });
});
