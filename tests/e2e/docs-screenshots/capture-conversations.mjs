// Regenerates the 1 editor screenshot embedded in
// site/src/content/docs/conversations.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, addScriptCommand, fieldByLabel, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Ask/Tell:');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Philippa');
    await selectTreeNode(page, 'Philippa');
    await page.getByRole('button', { name: 'Ask/Tell', exact: true }).click();
    const askInput = fieldByLabel(page, 'Ask about:');
    await askInput.fill('key lock');
    await askInput.locator('xpath=../..').locator('button:has-text("Add")').click();
    await page.waitForTimeout(300);

    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const line1 = page.locator('xpath=//span[text()="Print"]/following-sibling::input[1]');
    await line1.fill("'Hi,' you say to Philippa, 'can you help me find the key to this door?'");
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const line2 = page.locator('xpath=(//span[text()="Print"])[2]/following-sibling::input[1]');
    await line2.fill("'Sure, you need to look in the bedroom.'");
    await page.waitForTimeout(200);
    await capture(page, out('Talk3.png'), { untilLocator: line2, padding: 40 });
});
