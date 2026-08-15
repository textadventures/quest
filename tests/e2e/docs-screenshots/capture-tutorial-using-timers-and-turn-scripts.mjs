// Regenerates the 4 editor screenshots embedded in
// site/src/content/docs/tutorial/using_timers_and_turn_scripts.md. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addAdvancedElement, openTab,
    ifExpressionSelect, ifObjectSelect, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    // --- TimerBee1.png: "bee timer", interval 20, print-message script ---
    await addAdvancedElement(page, 'Timer', 'bee timer');
    const intervalInput = page.getByText('Interval (sec):', { exact: true }).locator('xpath=following::input[1]');
    await intervalInput.fill('20');
    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    const messageInput = page.locator('input[type="text"]').last();
    await messageInput.fill('The bee buzzes past you. Pesky bee.');
    await messageInput.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('TimerBee1.png'), { untilLocator: messageInput });

    // --- TimerBee2.png: a second timer whose script is "if player is in room, print" —
    // a fresh element rather than editing the first, so there's no existing row to remove ---
    await addAdvancedElement(page, 'Timer', 'bee timer checked');
    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('option', { name: 'Scripts', exact: true }).click();
    await page.getByRole('option', { name: 'If...' }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.waitForSelector('text=then');
    await ifExpressionSelect(page).selectOption('player is in room');
    await ifObjectSelect(page).selectOption('room');
    await page.locator('button:has-text("+ Add script")').first().click();
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    const thenMessageInput = page.locator('xpath=//span[text()="then"]/following::input[@type="text"][1]');
    await thenMessageInput.fill('The bee buzzes past you. Pesky bee.');
    const elseIfButton = page.locator('button:has-text("+ else if")').first();
    await capture(page, out('TimerBee2.png'), { untilLocator: elseIfButton, padding: 4 });

    // --- Turncounter1.png: player's Attributes tab, "turns" attribute added as Integer,
    // added to the Status attributes list ---
    await selectTreeNode(page, 'player');
    await openTab(page, 'Attributes');
    const addAttrInput = page.locator('input[placeholder="Add attribute..."]');
    await addAttrInput.fill('turns');
    await addAttrInput.locator('..').locator('button:has-text("Add")').click();
    await page.waitForSelector('text=ASSIGNMENT');
    const typeSelect = page.locator('select').filter({ hasText: 'String dictionary' });
    await typeSelect.selectOption('Integer');
    const statusAttrInput = page.locator('input[placeholder="Attribute"]');
    await statusAttrInput.fill('turns');
    await statusAttrInput.locator('..').locator('button:has-text("Add")').click();
    await page.waitForSelector('table >> text=turns', { timeout: 10000 });
    // Rendered "INHERITED TYPES" — uppercase is CSS-only, actual DOM text is title case.
    const inheritedTypesHeading = page.getByText('Inherited types', { exact: true });
    await capture(page, out('Turncounter1.png'), { untilLocator: inheritedTypesHeading, padding: 250 });

    // --- Turnscript.png: a turn script setting player.turns to player.turns + 1 ---
    await selectTreeNode(page, 'room');
    await page.click('button[title="Add element"]');
    await page.getByRole('button', { name: 'Add Turn Script to "room"', exact: true }).click();
    await page.waitForSelector('button:has-text("+ Add script")');
    await page.click('button:has-text("+ Add script")');
    await page.waitForSelector('text=Add Script Command');
    await page.getByRole('option', { name: 'Variables', exact: true }).click();
    await page.getByRole('option', { name: /^●\s*Set a variable or attribute$/ }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    // input[type="text"] index 0 is always the tree's own "Filter..." box; index 1 is the
    // turn script's own optional "Name:" field (left blank, per the tutorial).
    const exprInputs = page.locator('input[type="text"]');
    const nameInput = exprInputs.nth(2);
    const valueInput = exprInputs.nth(3);
    await nameInput.fill('player.turns');
    await valueInput.fill('player.turns + 1');
    await capture(page, out('Turnscript.png'), { untilLocator: valueInput });
});
