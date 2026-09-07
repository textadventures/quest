// Regenerates the 5 editor screenshots embedded in
// site/src/content/docs/other_guides/unlockdoor.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    toggleFeature, setScriptCodeView, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images', 'other_guides');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Room', 'vault');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'keypad');

    // --- Unlockdoor1.jpg: south exit, Locked + a message, named "vault door" ---
    await selectTreeNode(page, 'room');
    await openTab(page, 'Exits');
    await page.getByRole('button', { name: 'south', exact: true }).click();
    const destCombobox = page.locator('[role="combobox"]');
    await destCombobox.click();
    await destCombobox.fill('vault');
    await page.waitForSelector('[role="option"]:has-text("vault")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("vault")');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=south → vault', { timeout: 10000 });
    await page.getByText('Exit: vault', { exact: true }).click();
    await page.waitForSelector('button:has-text("Exit")', { timeout: 10000 });
    await page.locator('span:has-text("Name:")').locator('..').locator('input').fill('vault door');
    await page.getByText('Locked', { exact: true }).locator('..').locator('input[type="checkbox"]').check();
    const lockedMsgField = page.getByText('Print message when locked:', { exact: true })
        .locator('xpath=following::input[1]');
    await lockedMsgField.fill("The vault door won't budge. It needs a code.");
    await capture(page, out('Unlockdoor1.jpg'), { untilLocator: lockedMsgField, padding: 60 });

    // --- Unlockdoor2.jpg: keypad, Use/Give "Use (on its own)" -> get input + if fixed code ---
    await selectTreeNode(page, 'keypad');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Use/Give:');
    await openTab(page, 'Use/Give');
    await page.waitForSelector('text=USE (ON ITS OWN)', { timeout: 10000 });
    const useOwnActionSelect = page.locator('text=USE (ON ITS OWN)').locator('xpath=following::select[1]');
    await useOwnActionSelect.selectOption({ label: 'Run script' });
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `msg ("Enter the code:")
get input {
if (result = "1234") {
msg ("The door unlocks.")
vault door.locked = false
}
else {
msg ("Wrong code.")
}
}`);
    await page.waitForSelector('text=Get input, then', { timeout: 5000 });
    await capture(page, out('Unlockdoor2.jpg'), {
        untilLocator: page.locator('button:has-text("+ Add script")').last(),
        padding: 40,
    });

    // --- Unlockdoor4.png: game's Start script, game.code = random 4-digit string ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Scripts');
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `game.code = "" + GetRandomInt(1000, 9999)`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });
    await capture(page, out('Unlockdoor4.png'), {
        untilLocator: page.locator('button:has-text("+ Add script")').first(),
        padding: 40,
    });

    // --- Unlockdoor3.png: keypad script, fixed "1234" swapped for game.code ---
    await selectTreeNode(page, 'keypad');
    await openTab(page, 'Use/Give');
    await page.waitForSelector('text=USE (ON ITS OWN)', { timeout: 10000 });
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `msg ("Enter the code:")
get input {
if (result = game.code) {
msg ("The door unlocks.")
vault door.locked = false
}
else {
msg ("Wrong code.")
}
}`);
    await page.waitForSelector('text=Get input, then', { timeout: 5000 });
    await capture(page, out('Unlockdoor3.png'), {
        untilLocator: page.locator('button:has-text("+ Add script")').last(),
        padding: 40,
    });

    // --- Randomcode3.png: a "note" object's "Look at" description tells the player the code ---
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'note');
    await openTab(page, 'Setup');
    await page.getByText('"Look at" object description:', { exact: true })
        .locator('xpath=following::select[1]').selectOption({ label: 'Run script' });
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `msg ("A note on the wall says the code is " + game.code + ".")`);
    await page.waitForSelector('text=Print', { timeout: 5000 });
    await capture(page, out('Randomcode3.png'), {
        untilLocator: page.locator('button:has-text("+ Add script")').first(),
        padding: 40,
    });
});
