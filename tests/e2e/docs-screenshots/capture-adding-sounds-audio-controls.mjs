// Regenerates the 1 in-game-player screenshot embedded in
// site/src/content/docs/howto/multimedia/adding_sounds.md (audio_controls.jpg) - deferred out
// of capture-adding-sounds.mjs since it needs a WasmPlayer preview, not editor chrome. The
// native HTML5 <audio controls> widget renders regardless of whether the src actually resolves
// to a playable file, so no real audio asset is needed for this screenshot. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addScriptCommand, setScriptCodeView,
    openPreview, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    await page.waitForSelector('text=Start script:', { timeout: 10000 });
    const startAddBtn = page.getByText('Start script:', { exact: true }).locator('xpath=following::button[contains(.,"+ Add script")][1]');
    await addScriptCommand(page, startAddBtn);
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(),
        `msg ("<audio src='ambient sound.mp3' autoplay controls />")`);
    await page.waitForSelector('xpath=//span[text()="Print"]', { timeout: 5000 });

    const playerPage = await openPreview(page);
    await capture(playerPage, out('audio_controls.jpg'), { untilLocator: playerPage.locator('#txtCommand') });
});
