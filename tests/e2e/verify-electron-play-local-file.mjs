// Ad-hoc manual verification for Electron's Play tab "Open a game file…"
// flow (PlayCatalog.svelte's handleElectronPlay): a single click on the
// button should pick a file via the native dialog *and* launch it — no
// separate "Start" click (unlike the browser build, which needs one to get a
// fresh activation for its own window.open()). The launched player window is
// created by the main process (ipc/player.ts's player:openWindow), so it
// isn't subject to that activation requirement at all.
//
// Also verifies the two things that only work because of it:
//  - sibling resources: restart-test.aslx references an external
//    restart-test.js in the same folder, which only loads if the
//    'resource-request' BroadcastChannel handoff (backed by a real
//    ElectronFileAdapter reading from disk, not just raw bytes) is wired up.
//  - no "click to begin" sound-activation gate: wasm-player.js's
//    activationLikelyGranted() should short-circuit on the Electron user
//    agent, so the game boots straight through even though this window was
//    never focus-clicked before Bridge.Begin() ran.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run electron.sh once first (or the build steps
// inside it) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync, copyFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
const gameDir = mkdtempSync(join(tmpdir(), 'quest-electron-play-'));

const fixturesDir = join(dirname(fileURLToPath(import.meta.url)), 'fixtures');
const aslxPath = join(gameDir, 'restart-test.aslx');
copyFileSync(join(fixturesDir, 'restart-test.aslx'), aslxPath);
copyFileSync(join(fixturesDir, 'restart-test.js'), join(gameDir, 'restart-test.js'));

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[editor] [pageerror]', err.message));
    win.on('console', msg => { if (msg.type() === 'error') console.log('[editor] [console.error]', msg.text()); });

    await app.evaluate(({ dialog }, fp) => {
        dialog.showOpenDialog = async () => ({ canceled: false, filePaths: [fp] });
    }, aslxPath);

    // Root is the Play tab (PUBLIC_SHOW_HOME=true, see electron.sh).
    await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });
    console.log('[editor] Play tab loaded, "Open a game file…" button present');

    const [playerWindow] = await Promise.all([
        app.waitForEvent('window'),
        win.click('button:has-text("Open a game file…")'),
    ]);
    console.log('[editor] single click opened a new player window — no separate Start click needed');

    playerWindow.on('pageerror', err => console.log('[player] [pageerror]', err.message));
    playerWindow.on('console', msg => { if (msg.type() === 'error') console.log('[player] [console.error]', msg.text()); });

    await playerWindow.waitForFunction(() => document.title === 'Simple', null, { timeout: 15000 });
    console.log('[player] game booted, document.title:', await playerWindow.title());

    // No "click to begin" gate: the start screen overlay should already be
    // gone (game UI visible) without this test ever having interacted with
    // the player window at all.
    const gateVisible = await playerWindow.isVisible('#qv-clicktobegin');
    console.log('PASS: no click-to-begin sound gate shown:', !gateVisible);
    if (gateVisible) throw new Error('Unexpected click-to-begin gate in Electron player window');

    // Sibling resource (restart-test.js) loaded via the resource-request
    // handoff and ran, injecting its sidebar pane.
    await playerWindow.waitForSelector('.newwindow-test-pane', { timeout: 10000 });
    const scriptRuns = await playerWindow.evaluate(() => window.__restartTestScriptRuns);
    console.log('PASS: sibling restart-test.js loaded from disk over resource-request, ran', scriptRuns, 'time(s)');
    if (scriptRuns !== 1) throw new Error(`Expected restart-test.js to run exactly once, got ${scriptRuns}`);

    // Back on the editor window, the button should be its plain idle state
    // again (not stuck showing "Opening…").
    await win.waitForSelector('button:has-text("Open a game file…"):not([disabled])', { timeout: 5000 });
    console.log('PASS: Play tab button reset to idle after handoff');

    // Catalog play (play/[id]/+page.svelte's handleElectronPlay) drives the
    // same player:openWindow IPC with an {id} — exercised directly here
    // (rather than via a real textadventures.co.uk catalog fetch) to confirm
    // it opens a *separate* window instead of navigating this editor window
    // itself, which is what closes the filesystem-access hole this change
    // was meant to fix: a downloaded game's <javascript> can eval, and this
    // editor window's own preload exposes window.electronApp's fs bridge.
    const editorUrlBefore = win.url();
    const [catalogPlayerWindow] = await Promise.all([
        app.waitForEvent('window'),
        win.evaluate(() => window.electronApp.player.openWindow({ id: 'catalog-test-id' })),
    ]);
    console.log('PASS: catalog play opened a separate window:', catalogPlayerWindow.url());
    if (!catalogPlayerWindow.url().includes('/player/?id=catalog-test-id')) {
        throw new Error(`Unexpected catalog player window URL: ${catalogPlayerWindow.url()}`);
    }
    if (win.url() !== editorUrlBefore) {
        throw new Error(`Editor window navigated in-place (${editorUrlBefore} -> ${win.url()}) instead of staying put`);
    }
    console.log('PASS: editor window itself did not navigate (still fs-bridge-capable, but never sees game content)');
    await catalogPlayerWindow.close();

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await app?.close();
    rmSync(userDataDir, { recursive: true, force: true });
    rmSync(gameDir, { recursive: true, force: true });
}
