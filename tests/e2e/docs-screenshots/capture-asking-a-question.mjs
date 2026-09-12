// Regenerates the 4 editor screenshots embedded in
// site/src/content/docs/howto/tasks/asking-a-question.md. See .claude/skills/docs-screenshots/SKILL.md.
//
// The page used to teach "get input { }", which is intentionally no longer offered by the Add
// Script Command picker (superseded by the synchronous GetInput() expression form - see
// CoreEditorScriptsOutput.aslx's "Removed from adder" comments), so it now teaches GetInput()
// throughout and these captures follow. A bare GetInput() renders in the Visual editor as the
// "player's typed input" value template on a Set variable row (wrapping it in anything else
// falls back to a raw expression field, which is why the page's examples assign it directly).
// Each state is built by typing raw quest-script into the Start script's Code view and
// switching back to Visual editor, rather than via addScriptCommand.
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
player.alias = GetInput()`);
    await page.waitForSelector('text=Set variable');
    await capture(page, out('Question1.png'), { untilLocator: lastAddScript(), padding: 40 });

    // --- Question2.png ---
    await setScriptCodeView(page, codeViewBtn(), `msg ("What is your name?")
player.alias = GetInput()
msg ("How old are you?")
player.age = GetInput()`);
    await page.waitForSelector('text=Set variable');
    await capture(page, out('Question2.png'), { untilLocator: lastAddScript(), padding: 40 });

    // --- Question3.png ---
    await setScriptCodeView(page, codeViewBtn(), `msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
answer = GetInput()
if (answer = "man") {
msg ("'Is it a man?' you ask.")
msg ("'How come everyone knows the answer?'")
msg ("'We have this thing called the internet nowadays...'")
}
else {
msg ("'Is it \\"" + answer + "\\"?' you ask.")
msg ("'No!'")
}`);
    await page.waitForSelector('text=Set variable');
    await capture(page, out('Question3.png'), { untilLocator: lastAddScript(), padding: 40 });

    // --- Question4.png ---
    await setScriptCodeView(page, codeViewBtn(), `msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
JS.eval("$('#txtCommand').attr('placeholder', 'Your answer');")
JS.panesVisible(false)
answer = GetInput()
if (IsRegexMatch  ("^(a )?(man|lady|woman|human|person)$", LCase (answer))) {
msg ("'Is it a man?' you ask.")
msg ("'How come everyone knows the answer?'")
msg ("'We have this thing called the internet nowadays...'")
}
else {
msg ("'Is it \\"" + answer + "\\"?' you ask.")
msg ("'No!'")
}
JS.eval("$('#txtCommand').attr('placeholder', 'Type here...');")
JS.panesVisible(true)`);
    await page.waitForSelector('text=Set variable');
    await capture(page, out('Question4.png'), { untilLocator: lastAddScript(), padding: 40 });
});
