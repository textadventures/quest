// Regenerates the 4 editor screenshots embedded in
// site/src/content/docs/asking_a_question.md. See .claude/skills/docs-screenshots/SKILL.md.
// Like character_creation.md, these all use "get input" — no longer offered by the Add Script
// Command picker (superseded by the GetInput() expression form) but still fully editable once
// present — so each is built by typing raw quest-script into the Start script's Code view.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, setScriptCodeView, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Scripts');

    const codeViewBtn = () => page.locator('button:has-text("Code view")').first();
    const lastAddScript = () => page.locator('button:has-text("+ Add script")').last();

    // --- Question1.png ---
    await setScriptCodeView(page, codeViewBtn(), `msg ("What is your name?")
get input {
player.alias = CapFirst(result)
}`);
    await page.waitForSelector('text=Get input, then');
    await capture(page, out('Question1.png'), { untilLocator: lastAddScript(), padding: 40 });

    // --- Question2.png ---
    await setScriptCodeView(page, codeViewBtn(), `msg ("What is your name?")
get input {
player.alias = CapFirst(result)
msg ("How old are you?")
get input {
player.age = result
}
}`);
    await page.waitForSelector('text=Get input, then');
    await capture(page, out('Question2.png'), { untilLocator: lastAddScript(), padding: 40 });

    // --- Question3.png ---
    await setScriptCodeView(page, codeViewBtn(), `msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
get input {
if (result = "man") {
msg ("'Is it a man?' you ask.")
msg ("'How come everyone knows the answer?'")
msg ("'We have this thing called the internet nowadays...'")
}
else {
msg ("'Is it \\"" + result + "\\"?' you ask.")
msg ("'No!'")
}
}`);
    await page.waitForSelector('text=Get input, then');
    await capture(page, out('Question3.png'), { untilLocator: lastAddScript(), padding: 40 });

    // --- Question4.png ---
    await setScriptCodeView(page, codeViewBtn(), `msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
JS.eval("$('#txtCommand').attr('placeholder', 'Your answer');")
JS.panesVisible(false)
get input {
if (IsRegexMatch  ("^(a )?(man|lady|woman|human|person)$", LCase (result))) {
msg ("'Is it a man?' you ask.")
msg ("'How come everyone knows the answer?'")
msg ("'We have this thing called the internet nowadays...'")
}
else {
msg ("'Is it \\"" + result + "\\"?' you ask.")
msg ("'No!'")
}
JS.eval("$('#txtCommand').attr('placeholder', 'Type here...');")
JS.panesVisible(true)
}`);
    await page.waitForSelector('text=Get input, then');
    await capture(page, out('Question4.png'), { untilLocator: lastAddScript(), padding: 40 });
});
