// Regenerates the 3 in-game-player screenshots embedded in
// site/src/content/docs/howto/ux/ui-style.md — game.aslx Display/Interface tab settings,
// captured against the resulting WasmPlayer preview rather than the editor itself. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, openTab, openPreview, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const checkboxFor = (page, label) => page.getByText(label, { exact: true }).locator('xpath=..').locator('input[type="checkbox"]');

// Like lib.mjs's openPreview, but for a game with the command bar turned off (ui-no-cursor.png)
// - #txtCommand never becomes visible in that state, so wait on window.canSendCommand alone.
async function openPreviewNoCommandBar(page) {
    const context = page.context();
    const [playerPage] = await Promise.all([
        context.waitForEvent('page', { timeout: 15000 }),
        page.click('button:has-text("Preview")'),
    ]);
    await playerPage.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
    // #txtCommand never becomes visible with the command bar off, so wait for the location bar
    // to actually show the starting room name instead - confirmed live that the boot sequence
    // fully *removes* #gameTitle (its "Loading..." placeholder <h1>) once real content replaces
    // it, rather than just changing its text, so waiting for its text to change never resolves.
    await playerPage.waitForFunction(() => (document.getElementById('location')?.textContent ?? '').trim() !== '', { timeout: 20000 });
    return playerPage;
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');

    // --- ui-classic.png: default, unmodified interface ---
    const playerClassic = await openPreview(page);
    await capture(playerClassic, out('ui-classic.png'), { untilLocator: playerClassic.locator('#txtCommand') });
    // Close each player tab once captured before opening the next - each openPreview() boots a
    // fresh WASM runtime from scratch (no caching across tabs in dev mode), and leaving prior
    // tabs open while a new one boots was observed to starve it enough to stall indefinitely.
    await playerClassic.close();

    // --- ui-no-cursor.png: subtle colour-blend background, command line turned off ---
    await openTab(page, 'Display');
    await checkboxFor(page, 'Colour blend for background?').check();
    const topInput = page.getByText('Colour at top:', { exact: true }).locator('xpath=following::input[1]');
    await topInput.fill('AliceBlue');
    const bottomInput = page.getByText('Colour at bottom:', { exact: true }).locator('xpath=following::input[1]');
    await bottomInput.fill('LightSteelBlue');
    await openTab(page, 'Interface');
    await checkboxFor(page, 'Show command bar').uncheck();
    const playerNoCursor = await openPreviewNoCommandBar(page);
    await capture(playerNoCursor, out('ui-no-cursor.png'), { untilLocator: playerNoCursor.locator('body') });
    await playerNoCursor.close();

    // --- ui-cursor.png: panes off, command bar replaced with a plain cursor, minimalist look ---
    await checkboxFor(page, 'Show command bar').check();
    await checkboxFor(page, 'Use a cursor instead of a box for commands?').check();
    await checkboxFor(page, 'Show panes (Inventory, Places and Objects, Compass)').uncheck();
    const playerCursor = await openPreview(page);
    await capture(playerCursor, out('ui-cursor.png'), { untilLocator: playerCursor.locator('#txtCommand') });
});
