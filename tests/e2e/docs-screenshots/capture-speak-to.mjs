// Regenerates both editor screenshots embedded in site/src/content/docs/speak_to.md.
// Talk2.png was blocked on the Switch command's case-list editor (task_082ae91c); now fixed
// upstream (PR #2090). See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addVerb, addScriptCommand, setScriptCodeView, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Boris');
    await selectTreeNode(page, 'Boris');
    await openTab(page, 'Verbs');
    await addVerb(page, 'speak');
    await page.locator('select').first().selectOption('script');
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const msg1 = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await msg1.fill("'Hi,' you say to Boris, 'can you help me find the key to this door?'");
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const msg2 = page.locator('xpath=(//span[text()="Print"])[2]/following-sibling::textarea[1]');
    await msg2.fill("'Sure, you need to look in the bedroom.'");
    await page.waitForTimeout(200);
    await capture(page, out('Talk1.png'), { untilLocator: msg2, padding: 40 });

    // --- Talk2.png: same "speak" verb, replaced with a topic menu + switch ---
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `topics = Split ("Where is key;Who is the Queen;How do I defeat the troll", ";")
ShowMenu ("Talk to Boris about...", topics, true) {
switch (result) {
case ("Where is key") {
msg ("'You need to look in the bedroom.'")
}
case ("Who is the Queen") {
msg ("'Just some girl.'")
}
case ("How do I defeat the troll") {
msg ("'Use fire to stop it regenerating.'")
}
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    const caseToggles = page.getByRole('button', { name: '▶' });
    while (await caseToggles.count() > 0) {
        await caseToggles.first().click();
    }
    await capture(page, out('Talk2.png'), { untilLocator: page.locator('button:has-text("+ Add script")').last(), padding: 40 });
});
