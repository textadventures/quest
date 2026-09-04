// Regenerates the 4 in-game-player screenshots embedded in
// site/src/content/docs/howto/ux/ui-custom.md via the game object's "User interface
// initialisation script" (Advanced Scripts tab), captured against the resulting WasmPlayer
// preview. See .claude/skills/docs-screenshots/SKILL.md.
//
// Note: the doc's first JS.setCss call targets "#qv-status" - src/PlayerCore/Resources/
// playercore.htm's top bar was renamed "status" -> "qv-status" in commit 42cdd9b8, and
// Core.aslx's own InitInterface was retargeted to match in textadventures/quest#2169 (not
// yet merged into this branch as of writing). Regenerating these 4 screenshots before that
// fix has landed here will still show the top bar's default styling rather than the doc's
// border/background - re-run this capture once #2169 is merged into main and merged/rebased
// into this branch.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, openTab, toggleFeature,
    setScriptCodeView, openPreview, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const checkboxFor = (page, label) => page.getByText(label, { exact: true }).locator('xpath=..').locator('input[type="checkbox"]');

const scriptFor = (backandborder, button) => `backandborder = "${backandborder}"
button = "${button}"
text = "color:black;font-family:georgia, serif"
JS.setCss ("#qv-status", backandborder)
JS.setCss (".ui-accordion-header", "border-radius: 0px;" + backandborder)
JS.setCss (".ui-accordion-content", "border-radius: 0px;" + backandborder + ";border-top:none")
JS.setCss (".accordion-header-text", text)
JS.setCss (".ui-icon", "display:none")
JS.setCommands ("Look;Wait", "black")
JS.setCss ("#commandPane", text + ";" + backandborder)
JS.setCss ("#verblinkwait", button)
JS.setCss ("#verblinklook", button)
JS.setCss ("#gamePanes", "margin-top: 16px")
JS.eval ("$('#gamePanes').width(227);")`;

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');

    await openTab(page, 'Features');
    await toggleFeature(page, 'Show advanced scripts for the game object');

    await openTab(page, 'Interface');
    await checkboxFor(page, 'Show a command pane (use JS.setCommands to set)').check();

    await openTab(page, 'Advanced Scripts');
    const codeViewBtn = page.locator('button:has-text("Code view")').first();

    const variants = [
        { file: 'interface1.png', backandborder: 'border: chocolate ridge 6px;background:sandybrown', button: 'padding:5px;background:BurlyWood;border:ridge chocolate 1px;' },
        { file: 'interface2.png', backandborder: 'border: darkblue double 6px;background:dodgerblue', button: 'padding:5px;background:skyblue;border:double darkblue 1px;' },
        { file: 'interface3.png', backandborder: 'border: darkgrey outset 6px;background:grey', button: 'padding:5px;background:silver;border:outset darkgrey 1px;' },
        { file: 'interface4.png', backandborder: 'border: Indigo dotted 6px;background:MediumPurple', button: 'padding:5px;background:Violet;border:dotted Indigo 1px;' },
    ];

    for (const { file, backandborder, button } of variants) {
        await setScriptCodeView(page, codeViewBtn, scriptFor(backandborder, button));
        await page.waitForSelector('text=Set variable', { timeout: 5000 });
        const playerPage = await openPreview(page);
        await capture(playerPage, out(file), { untilLocator: playerPage.locator('#txtCommand') });
        await playerPage.close();
    }
});
