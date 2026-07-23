// Ad-hoc manual verification for the Electron "Recent games" feature: creating
// a game folder tracks it, it shows up on the /open page's Recent section and
// in the native File > Open Recent submenu, clicking a recent entry loads it
// directly (no picker), and Remove/Clear both work. Uses Playwright's
// _electron launcher against the already-built src/ElectronApp/dist — run
// the build steps in electron.sh once first (dotnet build WasmEditor/WasmPlayer
// Debug, npm run build in AppShell and ElectronApp) so dist/ and
// resources/app-static exist.
//
// Games are created through the app's own "Create new game" flow (at the
// default location, stubbed to a throwaway temp dir) rather than hand-
// written .aslx fixtures — a hand-written minimal game is a lot of surface
// to get right (see examples/blank.aslx), and this exercises the real
// createElectronGame() trackRecent() path anyway.
//
// --user-data-dir isolates this run's recent-games.json from the real app's
// userData (~/Library/Application Support/Quest Viva on mac), and the
// app.getPath('documents') stub below isolates game files themselves, so
// this doesn't pollute or depend on whatever the user has actually opened
// before.
import { _electron as electron } from 'playwright';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
// tests/e2e has its own node_modules (no `electron` in it — that's
// ElectronApp's own devDependency) — resolve the binary from there instead
// of relying on Playwright's default lookup next to this script.
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
const gamesRoot = mkdtempSync(join(tmpdir(), 'quest-electron-games-'));

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });

    // Stub app.getPath('documents') before the renderer gets a chance to call
    // paths:defaultGamesDir (fired on /open's mount) — createGame() below
    // never opens the "Change location…" picker, so without this the game
    // would land in the real user's ~/Documents/Quest Games instead of this
    // throwaway temp dir.
    await app.evaluate(({ app: electronAppSingleton }, fakeDocs) => {
        const original = electronAppSingleton.getPath.bind(electronAppSingleton);
        electronAppSingleton.getPath = (name) => (name === 'documents' ? fakeDocs : original(name));
    }, gamesRoot);

    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[pageerror]', err.message));
    win.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    // Menu items don't expose a stable id, and Playwright has no native-menu
    // driver — reach into the main process and click the MenuItem object
    // directly (Electron's MenuItem.click() fires exactly as a real click would).
    async function clickMenuItem(...labelPath) {
        await app.evaluate(({ Menu }, path) => {
            let items = Menu.getApplicationMenu().items;
            let item;
            for (const label of path) {
                item = items.find(i => i.label === label);
                if (!item) throw new Error(`Menu item not found: ${path.join(' > ')} (missing "${label}")`);
                items = item.submenu ? item.submenu.items : [];
            }
            item.click();
        }, labelPath);
    }

    async function menuHasItem(...labelPath) {
        return app.evaluate(({ Menu }, path) => {
            let items = Menu.getApplicationMenu().items;
            let item;
            for (const label of path) {
                item = items.find(i => i.label === label);
                if (!item) return false;
                items = item.submenu ? item.submenu.items : [];
            }
            return true;
        }, labelPath);
    }

    // Drives the "Create new game" form at the default location (stubbed
    // above to gamesRoot) — exercises createElectronGame()'s trackRecent()
    // call.
    async function createGame(name) {
        await win.fill('input[placeholder="Game name"]', name);
        await win.waitForSelector('text=Text adventure', { timeout: 10000 });
        await win.click('button:has-text("Create")');
        await win.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    }

    // 1. Fresh app, no recent games yet. Root lands on the Play tab by
    // default — the /open form lives behind the "Create" tab.
    await win.waitForSelector('a:has-text("Create")', { timeout: 30000 });
    await win.click('a:has-text("Create")');
    await win.waitForSelector('button:has-text("Open game…")', { timeout: 30000 });
    const noneYet = await win.isVisible('text=Recent');
    console.log('PASS: no "Recent" section before anything is opened:', !noneYet);
    if (noneYet) throw new Error('Recent section shown with an empty list');
    const hasNoRecentPlaceholder = await menuHasItem('File', 'Open Recent', 'No Recent Games');
    console.log('PASS: native menu shows "No Recent Games" placeholder:', hasNoRecentPlaceholder);
    if (!hasNoRecentPlaceholder) throw new Error('Expected "No Recent Games" placeholder in the native menu');

    // 2. Create Game A.
    await createGame('Game A');
    console.log('PASS: Game A created via "Create new game"');

    // 3. Go back to /open via File > New Game… (the only in-app way back to
    // /open without a picker — see +layout.svelte's comment on why menu
    // actions round-trip through goto() rather than a toolbar button).
    // Game A should now be listed under Recent.
    await clickMenuItem('File', 'New Game…');
    await win.waitForSelector('text=Recent', { timeout: 10000 });
    const gameAListed = await win.isVisible('text=Game A.aslx');
    console.log('PASS: Game A listed under Recent on /open:', gameAListed);
    if (!gameAListed) throw new Error('Game A missing from Recent list');
    const gameAInMenu = await menuHasItem('File', 'Open Recent', 'Game A.aslx — Game A');
    console.log('PASS: Game A listed in native Open Recent submenu:', gameAInMenu);
    if (!gameAInMenu) throw new Error('Game A missing from native Open Recent submenu');

    // 4. Create Game B, then click Game A's Recent entry on /open — should
    // load it directly, no picker.
    await createGame('Game B');
    await clickMenuItem('File', 'New Game…');
    await win.waitForSelector('text=Game B.aslx', { timeout: 10000 });
    await win.click('button:has-text("Game A.aslx")');
    await win.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: clicking Game A in Recent reopened it directly');

    // 5. Native "Open Recent" submenu click loads a game too (the nonce/query-
    // string path from +layout.svelte's onOpenRecent, not the /open page button).
    await clickMenuItem('File', 'Open Recent', 'Game B.aslx — Game B');
    await win.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: native "Open Recent" menu entry reopened Game B directly');

    // 6. Remove one entry from the /open page, confirm it's gone from both
    // the page and the native menu.
    await clickMenuItem('File', 'New Game…');
    await win.waitForSelector('text=Game A.aslx', { timeout: 20000 });
    const gameARow = win.locator('div.flex.items-center.gap-2.w-full', { hasText: 'Game A.aslx' }).locator('button:has-text("Remove")');
    await gameARow.click();
    await win.waitForSelector('text=Game A.aslx', { state: 'detached', timeout: 5000 }).catch(() => {});
    const gameAStillListed = await win.isVisible('text=Game A.aslx');
    console.log('PASS: Game A removed from /open Recent list:', !gameAStillListed);
    if (gameAStillListed) throw new Error('Game A still listed after Remove');
    const gameAStillInMenu = await menuHasItem('File', 'Open Recent', 'Game A.aslx — Game A');
    console.log('PASS: Game A removed from native menu too:', !gameAStillInMenu);
    if (gameAStillInMenu) throw new Error('Game A still in native menu after Remove');

    // 7. Clear Recent via the native menu — its click handler is async
    // (clear the file, then rebuild the menu and notify the renderer), so
    // wait for the already-mounted /open page's Recent section to actually
    // disappear rather than checking immediately. Confirms the page reacts
    // to a change that originates outside itself (recent.onChanged), not
    // just to its own Remove button's direct state update (step 6).
    await clickMenuItem('File', 'Open Recent', 'Clear Recent');
    await win.waitForSelector('text=Recent', { state: 'detached', timeout: 10000 }).catch(() => {});
    const recentGoneAfterClear = !(await win.isVisible('text=Recent'));
    console.log('PASS: Recent section gone from /open after Clear Recent:', recentGoneAfterClear);
    if (!recentGoneAfterClear) throw new Error('Recent section still shown after Clear Recent');
    const placeholderBack = await menuHasItem('File', 'Open Recent', 'No Recent Games');
    console.log('PASS: native menu back to "No Recent Games" after Clear Recent:', placeholderBack);
    if (!placeholderBack) throw new Error('Expected placeholder back after Clear Recent');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await app?.close();
    rmSync(userDataDir, { recursive: true, force: true });
    rmSync(gamesRoot, { recursive: true, force: true });
}
