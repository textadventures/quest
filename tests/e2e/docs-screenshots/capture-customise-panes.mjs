// Captures the player screenshot for site/src/content/docs/customise/panes.md's "Example: a
// phone that shows the inventory and exits": the recipe's game, after taking the torch.
//
// Unlike the other capture scripts this doesn't build the game in the editor: the recipe is all
// script and stylesheet, which fixtures/v2-phone-pane-test.aslx already holds (and
// wasmplayer/v2-phone-pane.spec.mjs checks). It plays that file through AppShell's /player proxy
// to WasmPlayer.
//
// Needs the AppShell dev server (port 5174) and the WasmPlayer dev server (port 5175) running:
//   cd tests/e2e && node docs-screenshots/capture-customise-panes.mjs http://localhost:5174
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { capturePlayer, runCapture, sendCommand, waitForPlayer } from './lib.mjs';

const IMAGES = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../../site/public/images');

await runCapture(async ({ page, baseUrl }) => {
    await page.goto(`${baseUrl}/player/?url=/player/e2e-fixtures/v2-phone-pane-test.aslx`);
    await waitForPlayer(page);
    await sendCommand(page, 'take torch');
    await page.waitForFunction(() => document.querySelector('[data-qv-pane="phone"] .phone-screen')?.textContent.includes('torch'));
    await capturePlayer(page, path.join(IMAGES, 'phone_pane.png'));
});
