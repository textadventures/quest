// Ad-hoc manual verification for Electron's "new game" folder flow:
// - Defaults to Documents/Quest Games (matches Quest 5's desktop editor) with
//   no dialog at all — createElectronGame() creates a subfolder named after
//   the game inside it, IDE "new project" style.
// - The /open page shows a live "Will be created in: ..." preview as the
//   user types, before they've created anything.
// - "Change location…" lets the user override the parent folder via a real
//   picker.
// - Creating a second game with the same name at the same location errors
//   instead of silently overwriting.
//
// Stubs Electron's app.getPath('documents') to a throwaway temp dir for the
// whole run — otherwise the "default location" cases would actually write
// into the real user's ~/Documents/Quest Games on whatever machine runs this.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run the build steps in electron.sh once first
// (dotnet build WasmEditor/WasmPlayer Debug, npm run build in AppShell and
// ElectronApp) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync, existsSync, readdirSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
const fakeDocumentsDir = mkdtempSync(join(tmpdir(), 'quest-electron-documents-'));
const customLocationDir = mkdtempSync(join(tmpdir(), 'quest-electron-customloc-'));

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });

    // Stub app.getPath('documents') before the renderer gets a chance to call
    // paths:defaultGamesDir (fired on /open's mount) — real launch.getPath
    // for every other name (userData, etc.) is left untouched.
    await app.evaluate(({ app: electronAppSingleton }, fakeDocs) => {
        const original = electronAppSingleton.getPath.bind(electronAppSingleton);
        electronAppSingleton.getPath = (name) => (name === 'documents' ? fakeDocs : original(name));
    }, fakeDocumentsDir);

    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[pageerror]', err.message));
    win.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

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

    // 1. Root now lands on the Play tab by default — the /open form lives
    // behind the "Create" tab. Fresh /open, type a name — the preview should
    // show the *parent* folder only (Documents/Quest Games, the stubbed fake
    // one), not the game's own subfolder — "Change location…" only ever
    // changes the parent, so showing the child folder name next to it read
    // as if that were also choosable.
    await win.waitForSelector('a:has-text("Create")', { timeout: 30000 });
    await win.click('a:has-text("Create")');
    await win.waitForSelector('button:has-text("Open game…")', { timeout: 30000 });
    await win.fill('input[placeholder="Game name"]', 'Game A');
    const previewLocator = win.locator('text=Will be created as a new folder in:');
    await previewLocator.waitFor({ timeout: 10000 });
    const previewText = await previewLocator.innerText();
    console.log('Preview text:', previewText);
    const previewLooksRight = previewText.includes('Quest Games') && previewText.includes(fakeDocumentsDir) && !previewText.includes('Game A');
    console.log('PASS: preview shows only the parent Documents/Quest Games dir, not a "Game A" child:', previewLooksRight);
    if (!previewLooksRight) throw new Error(`Unexpected preview text: ${previewText}`);

    // 2. Create it — no dialog stub in place for this step, so if the code
    // wrongly popped a real native picker, this would hang and time out
    // rather than silently pass.
    await win.waitForSelector('text=Text adventure', { timeout: 10000 });
    await win.click('button:has-text("Create")');
    await win.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });

    const questGamesDir = join(fakeDocumentsDir, 'Quest Games');
    const gameAAslx = join(questGamesDir, 'Game A', 'Game A.aslx');
    const gameACreated = existsSync(gameAAslx);
    console.log('PASS: Game A created at Documents/Quest Games/Game A with no dialog shown:', gameACreated);
    if (!gameACreated) throw new Error(`Expected ${gameAAslx} to exist`);

    // 3. Creating "Game A" again at the same (default) location should
    // error, not overwrite.
    await clickMenuItem('File', 'New Game…');
    await win.waitForSelector('input[placeholder="Game name"]', { timeout: 10000 });
    await win.fill('input[placeholder="Game name"]', 'Game A');
    await win.waitForSelector('text=Text adventure', { timeout: 10000 });
    await win.click('button:has-text("Create")');
    const errorLocator = win.locator('text=already exists');
    await errorLocator.waitFor({ timeout: 10000 });
    console.log('PASS: creating "Game A" again at the default location shows an "already exists" error');
    const stillOnOpenPage = await win.isVisible('button:has-text("Open game…")');
    if (!stillOnOpenPage) throw new Error('Expected to remain on /open after the collision error');

    // 4. "Change location…" opens a real picker (stubbed here) and overrides
    // the target for this game — Game B should land in customLocationDir,
    // not under Documents/Quest Games.
    await app.evaluate(({ dialog }, dir) => {
        dialog.showOpenDialog = async () => ({ canceled: false, filePaths: [dir] });
    }, customLocationDir);
    await win.click('button:has-text("Change location…")');
    await win.waitForFunction(
        (expected) => document.body.innerText.includes(expected),
        customLocationDir,
        { timeout: 10000 },
    );
    await win.fill('input[placeholder="Game name"]', 'Game B');
    await win.click('button:has-text("Create")');
    await win.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    const gameBAslx = join(customLocationDir, 'Game B', 'Game B.aslx');
    const gameBCreated = existsSync(gameBAslx);
    console.log('PASS: Game B created at the chosen custom location instead of Quest Games:', gameBCreated);
    if (!gameBCreated) throw new Error(`Expected ${gameBAslx} to exist`);
    const questGamesEntries = readdirSync(questGamesDir);
    const gameBNotInDefault = !questGamesEntries.includes('Game B');
    console.log('PASS: Game B is NOT under Documents/Quest Games:', gameBNotInDefault, questGamesEntries);
    if (!gameBNotInDefault) throw new Error('Game B should not have landed in the default Quest Games folder');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await app?.close();
    rmSync(userDataDir, { recursive: true, force: true });
    rmSync(fakeDocumentsDir, { recursive: true, force: true });
    rmSync(customLocationDir, { recursive: true, force: true });
}
