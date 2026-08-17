// Regenerates the 1 editor screenshot embedded in site/src/content/docs/ask_about.md
// (Asktell3.png) — was blocked on ScriptDictionaryEditor having no rename/edit-key
// affordance; now fixed upstream (PR #2091). See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, toggleFeature, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Ask/Tell:');

    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Mary');
    await openTab(page, 'Ask/Tell');
    await page.waitForSelector('text=ASK', { timeout: 10000 });

    const askEntryInput = page.getByPlaceholder('Add entry key…').first();
    await askEntryInput.fill('dr black');
    await askEntryInput.locator('xpath=following-sibling::button[1]').click();
    await page.waitForSelector('text=dr black', { timeout: 10000 });

    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command', { timeout: 5000 });
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.waitForSelector('xpath=//span[text()="Print"]', { timeout: 5000 });
    const msgInput = page.locator('xpath=//span[text()="Print"]/following::input[1]');
    await msgInput.fill("'Terrible business,' says Mary. 'The poor doctor never stood a chance.'");

    await page.click('button:has-text("Edit Key")');
    const keyInput = page.locator('text=Ask about:').locator('xpath=following::input[1]');
    await keyInput.click({ clickCount: 3 });
    await keyInput.fill('dr doctor black');
    await page.keyboard.press('Enter');
    await page.waitForSelector('text=dr doctor black', { timeout: 5000 });

    await page.waitForSelector('button:has-text("Edit Key")', { timeout: 5000 });
    await capture(page, out('Asktell3.png'), { untilLocator: msgInput, padding: 60 });
});
