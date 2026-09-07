// Regenerates the 3 editor screenshots embedded in
// site/src/content/docs/score_health_money.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, addScriptCommand, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await page.getByText('Score', { exact: true }).locator('..').locator('input[type="checkbox"]').check();
    await page.getByText('Health', { exact: true }).locator('..').locator('input[type="checkbox"]').check();
    await page.getByText('Money', { exact: true }).locator('..').locator('input[type="checkbox"]').check();

    // --- increase_decrease.png: Add Script Command dialog, "Player" category, the 6
    // score/health/money commands ---
    await selectTreeNode(page, 'room');
    await openTab(page, 'Scripts');
    await page.locator('button:has-text("+ Add script")').first().click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('option', { name: 'Player', exact: true }).click();
    const dialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await capture(page, out('increase_decrease.png'), { untilLocator: dialog });

    // --- increase.png: "Increase score" command, set to add 5 ---
    await page.getByRole('option', { name: /^●\s*Increase score$/ }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.waitForTimeout(200);
    await page.screenshot({ path: '/tmp/spike-increase-added.png' });
    const amountInput = page.locator('xpath=(//span[contains(text(), "Increase")]/following::input)[1]');
    await amountInput.fill('5');
    await page.waitForTimeout(200);
    await capture(page, out('increase.png'), { untilLocator: amountInput, padding: 40 });

    // --- you_died.png: game's Player tab, "when health goes to zero" script ---
    await selectTreeNode(page, 'game');
    await page.getByRole('button', { name: 'Player', exact: true }).click();
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const diedMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await diedMsg.fill('You died!');
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first(), { category: 'Game State', item: 'Finish the game' });
    await page.waitForTimeout(200);
    await capture(page, out('you_died.png'), { untilLocator: page.locator('button:has-text("+ Add script")').first(), padding: 40 });
});
