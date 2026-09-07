// Regenerates the 1 in-game-player screenshot embedded in
// site/src/content/docs/howto/ux/custom_panes.md (indicator-bar.png) - a custom status pane
// showing a graphical hit-points bar, built via the game object's "User interface
// initialisation script" (Advanced Scripts tab) and "Start script" (Scripts tab), captured
// against the resulting WasmPlayer preview. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, openTab, toggleFeature,
    setScriptCodeView, openPreview, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const checkboxFor = (page, label) => page.getByText(label, { exact: true }).locator('xpath=..').locator('input[type="checkbox"]');

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');

    await openTab(page, 'Features');
    await toggleFeature(page, 'Show advanced scripts for the game object');

    await openTab(page, 'Interface');
    await checkboxFor(page, 'Show a custom status pane (use JS.setCustomStatus to set)').check();

    await openTab(page, 'Advanced Scripts');
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `s = "<table width=\\"100%\\"><tr>"
s = s + "   <td style=\\"text-align:right;\\" width=\\"50%\\">Hit points:</td>"
s = s + "   <td style=\\"text-align:left;\\" width=\\"50%\\"><span id=\\"hits-span\\">---</span></td>"
s = s + " </tr>"
s = s + " <tr>"
s = s + "   <td colspan=\\"2\\" style=\\"border: thin solid;background:white;text-align:left;\\">"
s = s + "   <span id=\\"hits-indicator\\" style=\\"background-color:black;padding-right:200px;\\"></span>"
s = s + "   </td>"
s = s + " </tr>"
s = s + "</table>"

JS.setCustomStatus (s)
if (HasScript(player, "changedhitpoints")) {
  do (player, "changedhitpoints")
}`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });

    await openTab(page, 'Scripts');
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `player.changedhitpoints => {
  JS.eval ("$('#hits-span').html('" + game.pov.hitpoints + "/" + game.pov.maxhitpoints + "');")
  JS.eval ("$('#hits-indicator').css('padding-right', '" + (200 * game.pov.hitpoints / game.pov.maxhitpoints) + "px');")
}

player.maxhitpoints = 70
player.hitpoints = 70`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });

    const playerPage = await openPreview(page);
    await capture(playerPage, out('indicator-bar.png'), { untilLocator: playerPage.locator('#txtCommand') });
});
