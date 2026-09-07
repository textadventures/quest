// Regenerates the 1 editor screenshot embedded in site/src/content/docs/blocks_and_scripts.md
// (nested_switch.png) — was blocked on the Switch command's case-list editor
// (task_082ae91c); now fixed upstream (PR #2090). See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, setScriptCodeView, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Scripts');

    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `options = Split("Red;Green;Blue;Yellow", ";")
ShowMenu ("What is your favourite colour?", options, false) {
switch (result) {
case ("Red") {
msg ("You must be very passionate. Or like a team that play in red.")
}
case ("Yellow") {
msg ("What a bright, cheerful colour!.")
}
case ("Green", "Blue") {
msg (result + "? Seriously?")
}
}
options = Split("Dog;Turtle;Duck;Newt;Trout", ";")
ShowMenu ("Okay, and what is your favourite animal?", options, false) {
msg ("Really? Big fan of " + result + "s, are you?")
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    const caseToggles = page.getByRole('button', { name: '▶' });
    while (await caseToggles.count() > 0) {
        await caseToggles.first().click();
    }
    await capture(page, out('nested_switch.png'), {
        untilLocator: page.locator('button:has-text("+ Add script")').last(),
        padding: 40,
    });
});
