// Regenerates the 4 editor screenshots embedded in the gamebook tutorial track
// (site/src/content/docs/tutorial/gamebook/). See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, addScriptCommand, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

// The options list's own "add" row: a page picker, a link-text box and an Add button.
async function addOption(page, destination, linkText) {
    const label = page.getByText('Options:', { exact: true });
    await label.locator('xpath=following::select[1]').selectOption({ label: destination });
    // The add-row's own box, by placeholder - a plain "last input after the label" match
    // picks up a hidden responsive duplicate instead (see lib.mjs's addVerb comment).
    await page.locator('input[placeholder*="link text"]').fill(linkText);
    await label.locator('xpath=following::button[text()="Add"][1]').click();
    await page.waitForTimeout(300);
}

await runCapture(async ({ page, baseUrl }) => {
    // --- gb01.png: the "Create new game" form, with Gamebook chosen ---
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    const existing = page.locator('text=The Lighthouse.aslx');
    if (await existing.count() > 0) {
        await existing.locator('..').locator('button[title="Delete draft"]').click();
        await page.getByRole('button', { name: 'Delete', exact: true }).click();
        await existing.waitFor({ state: 'detached', timeout: 10000 });
    }
    await page.fill('input[placeholder="Game name"]', 'The Lighthouse');
    await page.waitForSelector('text=Gamebook', { timeout: 10000 });
    await page.getByText('Gamebook', { exact: true }).click();
    const createButton = page.locator('button:has-text("Create local draft")');
    await capture(page, out('gb01.png'), { untilLocator: createButton, padding: 40 });

    await createLocalDraft(page, baseUrl, 'The Lighthouse', { gameType: 'Gamebook' });

    // --- gb02.png: Page1's Page tab - page type, description, options ---
    await selectTreeNode(page, 'Page1');
    await page.waitForSelector('text=Page type:', { timeout: 10000 });
    const description = page.locator('textarea').first();
    await description.fill(
        'The boat that brought you is already a smudge on the horizon, and the rain has '
        + 'started in earnest. The lighthouse stands at the end of a short stone causeway, '
        + 'dark from top to bottom.'
    );
    await capture(page, out('gb02.png'), {
        untilLocator: page.getByText('Options:', { exact: true }).locator('xpath=following::button[text()="Add"][1]'),
        padding: 24,
    });

    // --- gb03.png: the Options list, with the two options this chapter builds ---
    await addElement(page, 'Add Page', 'TakeLantern');
    await addElement(page, 'Add Page', 'Causeway');
    await selectTreeNode(page, 'Page1');
    await page.waitForSelector('text=Options:', { timeout: 10000 });
    // Clear the two options the new-game template came with.
    for (const dest of ['Page2', 'Page3']) {
        const row = page.locator(`text=${dest}`).locator('xpath=following::button[text()="✕"][1]');
        if (await row.count() > 0) { await row.click(); await page.waitForTimeout(250); }
    }
    await addOption(page, 'TakeLantern', 'Take the lantern');
    await addOption(page, 'Causeway', 'Leave it and walk up to the lighthouse');
    await capture(page, out('gb03.png'), {
        untilLocator: page.getByText('Options:', { exact: true }).locator('xpath=following::button[text()="+ New Page"][1]'),
        padding: 24,
    });

    // --- gb04.png: TakeLantern as a "Script + Text" page, setting a flag ---
    await selectTreeNode(page, 'TakeLantern');
    await page.waitForSelector('text=Page type:', { timeout: 10000 });
    await page.locator('select').first().selectOption({ label: 'Script + Text' });
    await page.waitForSelector('button:has-text("+ Add script")', { timeout: 10000 });
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first(),
        { category: 'Variables', item: 'Set flag on' });
    const flagInput = page.locator('xpath=//span[contains(text(), "Set flag")]/following::input[1]');
    await flagInput.fill('lantern');
    await capture(page, out('gb04.png'), { untilLocator: flagInput, padding: 40 });
});
