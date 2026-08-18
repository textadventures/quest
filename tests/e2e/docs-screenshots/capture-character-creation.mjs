// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/howto/rpg/character_creation.md. See .claude/skills/docs-screenshots/SKILL.md.
//
// "get input" is intentionally no longer offered by the Add Script Command picker (superseded
// by the synchronous GetInput() expression form, rendered in the Visual editor as the
// "player's typed input" value template - see CoreEditorScriptsOutput.aslx's "Removed from
// adder" comments) but the old callback form remains fully editable once present. Both scripts
// here are built by typing raw quest-script into the Start script's Code view and switching
// back to Visual editor, rather than via addScriptCommand.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, setScriptCodeView, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Scripts');

    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `msg ("Let's generate a character...")
msg ("First, what is your name?")
player.alias = GetInput()
msg ("Hi, " + player.alias)`);
    await page.waitForSelector('text=Set variable');
    const lastRow1 = page.locator('button:has-text("+ Add script")').last();
    await capture(page, out('Creation1.png'), { untilLocator: lastRow1, padding: 40 });

    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `msg ("Let's generate a character...")
msg ("First, what is your name?")
player.alias = GetInput()
msg ("Hi, " + player.alias)
show menu ("Your gender?", Split ("Male;Female", ";"), false) {
  player.gender = result
  show menu ("Your character class?", Split ("Warrior;Wizard;Priest;Thief", ";"), false) {
    player.class = result
    msg (" ")
    msg (player.alias + " was a " + LCase (player.gender) + " " + LCase (player.class) + ".")
    msg (" ")
    msg ("Now press a key to begin...")
    wait {
      ClearScreen
    }
  }
}`);
    await page.waitForSelector('text=Set variable');
    const lastRow2 = page.locator('button:has-text("+ Add script")').last();
    await capture(page, out('Creation2.png'), { untilLocator: lastRow2, padding: 40 });
});
