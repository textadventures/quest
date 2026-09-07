// Regenerates the shared Debugger.png embedded in both
// site/src/content/docs/debugging_your_game.md and site/src/content/docs/about_types.md.
// The Debugger is a WasmPlayer feature (opened via #cmdDebug), not an editor feature -
// only reachable through an editor-preview session (openPreview), same as a player
// screenshot. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, openPreview, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    const playerPage = await openPreview(page);

    await playerPage.click('#cmdDebug');
    await playerPage.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });
    await playerPage.click('#qv-debugger-tabs button:text("Objects")');
    await playerPage.waitForSelector('#qv-debugger-list [data-item]');
    await playerPage.click('#qv-debugger-list [data-item="player"]');
    await playerPage.waitForSelector('[data-attr-row]');
    await capture(playerPage, out('Debugger.png'), { untilLocator: playerPage.locator('#questVivaDebugger') });
});
