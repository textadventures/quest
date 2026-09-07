// Regenerates the 4 editor screenshots embedded in
// site/src/content/docs/tutorial/using_scripts.md, showing the current AppShell's
// if/then/else script editor. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addVerb,
    ifExpressionSelect, ifObjectSelect, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

// Print's default "message" mode renders its value as a <textarea> (see lib.mjs's
// following-sibling::textarea convention), not an <input> — anchor on the "then"/"else"
// label span instead of a positional input[type="text"] index, which shifts whenever an
// unrelated field elsewhere on the page gains or loses a text input.
const thenMessageInput = page => page.locator('xpath=//span[text()="then"]/following::*[self::input[@type="text"] or self::textarea][1]');
const elseMessageInput = page => page.locator('xpath=//span[text()="else"]/following::*[self::input[@type="text"] or self::textarea][1]');

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'TV');
    await openTab(page, 'Verbs');
    await addVerb(page, 'watch');
    // Only one <select> exists in the BEHAVIOUR panel before any script command is added.
    await page.locator('select').first().selectOption('script');

    // --- Addif.png: "Add Script Command" dialog, Scripts category, "If…" highlighted ---
    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('option', { name: 'Scripts', exact: true }).click();
    // Every command row's accessible name is prefixed with a "●" marker, so match by
    // substring rather than exact — "If..." alone would never match.
    const ifOption = page.getByRole('option', { name: 'If...' });
    await ifOption.click();
    const dialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await capture(page, out('Addif.png'), { untilLocator: dialog, cursorAt: { locator: ifOption, at: 'left' } });
    await page.getByRole('button', { name: 'OK', exact: true }).click();

    // --- Addif2.png: freshly-added if/then/else editor, condition not yet set ---
    await page.waitForSelector('text=then');
    const elseIfButton = page.locator('button:has-text("+ else if")');
    await capture(page, out('Addif2.png'), { untilLocator: elseIfButton });

    // --- Addif3.png: condition set to "object is switched on", object "TV" ---
    await ifExpressionSelect(page).selectOption('object is switched on');
    await ifObjectSelect(page, { selectIndex: 3 }).selectOption('TV');
    await capture(page, out('Addif3.png'), { untilLocator: elseIfButton, cursorAt: ifObjectSelect(page, { selectIndex: 3 }) });

    // --- Addif4.png: Then/Else print-message scripts filled in ---
    await page.locator('button:has-text("+ Add script")').first().click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await thenMessageInput(page).fill(
        "You watch for a few minutes. As your will to live slowly ebbs away, you remember that you've always hated watching westerns."
    );

    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await page.locator('button:has-text("+ Add script")').nth(1).click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await elseMessageInput(page).fill(
        "You watch for a few minutes, thinking that the latest episode of ‘Big Brother’ is even more boring than usual. You then realise that the TV is in fact switched off."
    );
    await capture(page, out('Addif4.png'), { untilLocator: elseIfButton, padding: 2 });
});
