// Ad-hoc manual verification for Electron's IDE-style "new game creates its
// own folder" flow: createElectronGame() now picks a *parent* location and
// creates a subfolder named after the game inside it, rather than requiring
// the user to pre-create and pick the exact target folder. Also checks the
// "a folder with that name already exists there" error path.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run the build steps in electron.sh once first
// (dotnet build WasmEditor/WasmPlayer Debug, npm run build in WebEditor and
// ElectronApp) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync, mkdirSync, existsSync, readdirSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
const parentDir = mkdtempSync(join(tmpdir(), 'quest-electron-parent-'));

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[pageerror]', err.message));
    win.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    async function stubParentDirPicker() {
        await app.evaluate(({ dialog }, dir) => {
            dialog.showOpenDialog = async () => ({ canceled: false, filePaths: [dir] });
        }, parentDir);
    }

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

    // 1. Create "Game A" — parentDir itself should NOT get the .aslx directly;
    // a "Game A" subfolder should appear inside it instead.
    await win.waitForSelector('button:has-text("Open game folder")', { timeout: 30000 });
    await stubParentDirPicker();
    await win.fill('input[placeholder="Game name"]', 'Game A');
    await win.waitForSelector('text=Text adventure', { timeout: 10000 });
    await win.click('button:has-text("Save to my computer")');
    await win.waitForSelector('button:has-text("Assets")', { timeout: 30000 });

    const parentEntries = readdirSync(parentDir);
    console.log('parentDir entries after create:', parentEntries);
    const gotSubfolder = parentEntries.includes('Game A') && !parentEntries.some(e => e.endsWith('.aslx'));
    console.log('PASS: "Game A" subfolder created, no .aslx directly in parentDir:', gotSubfolder);
    if (!gotSubfolder) throw new Error(`Expected only a "Game A" subfolder in ${parentDir}, got: ${parentEntries.join(', ')}`);

    const subfolderEntries = readdirSync(join(parentDir, 'Game A'));
    const gotAslx = subfolderEntries.includes('Game A.aslx');
    console.log('PASS: "Game A.aslx" written inside the new subfolder:', gotAslx, subfolderEntries);
    if (!gotAslx) throw new Error(`Expected Game A.aslx inside the subfolder, got: ${subfolderEntries.join(', ')}`);

    // 2. Create a second game with the SAME name at the SAME parent location
    // — should error, not silently overwrite or crash.
    await clickMenuItem('File', 'New Game…');
    await win.waitForSelector('input[placeholder="Game name"]', { timeout: 10000 });
    await stubParentDirPicker();
    await win.fill('input[placeholder="Game name"]', 'Game A');
    await win.waitForSelector('text=Text adventure', { timeout: 10000 });
    await win.click('button:has-text("Save to my computer")');
    const errorLocator = win.locator('text=already exists');
    await errorLocator.waitFor({ timeout: 10000 });
    const errorShown = await errorLocator.isVisible();
    console.log('PASS: creating "Game A" again at the same location shows an "already exists" error:', errorShown);
    if (!errorShown) throw new Error('Expected an "already exists" error, none shown');
    const stillOnOpenPage = await win.isVisible('button:has-text("Open game folder")');
    console.log('PASS: still on /open after the collision error (no crash, no false navigation):', stillOnOpenPage);
    if (!stillOnOpenPage) throw new Error('Expected to remain on /open after the collision error');

    // 3. A different game name at the same parent location should succeed
    // (proves the collision check is scoped to the folder name, not the
    // whole parent directory).
    await stubParentDirPicker();
    await win.fill('input[placeholder="Game name"]', 'Game B');
    await win.click('button:has-text("Save to my computer")');
    await win.waitForSelector('button:has-text("Assets")', { timeout: 30000 });
    const gameBExists = existsSync(join(parentDir, 'Game B', 'Game B.aslx'));
    console.log('PASS: "Game B" created successfully alongside "Game A":', gameBExists);
    if (!gameBExists) throw new Error('Expected Game B.aslx inside its own subfolder');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await app?.close();
    rmSync(userDataDir, { recursive: true, force: true });
    rmSync(parentDir, { recursive: true, force: true });
}
