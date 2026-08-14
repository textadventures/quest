// Ad-hoc manual verification: mirrors verify-wasmplayer-restart-sidebar-dup.mjs
// for WebPlayer. WebPlayer's Game.razor renders the player chrome
// (#qv-player-chrome, wrapping playercore.htm) once via Blazor; Blazor never
// re-diffs it afterwards since the underlying string never changes, so a
// mid-session restart (Start from the beginning / Load) needs to reset that
// DOM itself. Fixed via WebPlayer.resetPlayerUi(html), called before
// resetOutput/initUI on every restart.
//
// Doesn't wire up a real game with a custom <javascript> resource (WebPlayer's
// /dev/open + FileGameDataProvider flow has no adjacent-resource support to
// serve one locally) — instead manually injects a marker element into
// #sidebar to stand in for "a game's custom JS added something to the
// chrome", then confirms restart wipes it, and that basic gameplay (typing a
// command) still works afterwards (proving initUI/Player wiring survived the
// DOM reset). Requires WebPlayer running locally in Development mode:
//   ASPNETCORE_ENVIRONMENT=Development dotnet run --configuration Release \
//     --no-launch-profile --urls http://localhost:5099 --project src/WebPlayer
import { chromium } from 'playwright';
import path from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5099';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console error]', msg.text()); });

await page.goto(`${baseUrl}/dev/open`);
await page.waitForTimeout(1000);
const fileInput = await page.$('input[type=file]');
await fileInput.setInputFiles(path.resolve('../../examples/blank.aslx'));

await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
console.log('Game booted via /dev/open.');

const injectMarker = () => page.evaluate(() => {
    const sidebar = document.getElementById('sidebar');
    const div = document.createElement('div');
    div.className = 'newwindow-test-pane';
    div.textContent = 'pane';
    sidebar.appendChild(div);
});
const countMarkers = () => page.$$eval('.newwindow-test-pane', els => els.length);

await injectMarker();
console.log('markers before restart (expect 1):', await countMarkers());

await page.click('#cmdSave');
await page.waitForSelector('#questVivaSlots', { state: 'visible', timeout: 10000 });
await page.click('button:has-text("Start from the beginning")');

await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
await page.waitForTimeout(500);

const markersAfterRestart = await countMarkers();
console.log('markers after restart (expect 0 — resetPlayerUi wiped the chrome):', markersAfterRestart);

// Confirm the fresh chrome is actually wired up (initUI/Player.Initialise ran
// correctly against it, not just visually present).
await page.fill('#txtCommand', 'look');
await page.press('#txtCommand', 'Enter');
await page.waitForTimeout(500);
const outputText = await page.$eval('#divOutput', el => el.textContent).catch(() => null);
console.log('output after restart + "look" command:', outputText?.slice(-200));

await browser.close();

if (markersAfterRestart !== 0) {
    console.error('FAIL: expected the manually-injected chrome marker to be gone after restart.');
    process.exit(1);
}
if (!outputText || !outputText.toLowerCase().includes('room')) {
    console.error('FAIL: game did not respond to a command after restart — chrome reset likely broke wiring.');
    process.exit(1);
}
console.log('PASS');
