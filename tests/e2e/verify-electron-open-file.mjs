// Ad-hoc manual verification for Electron's "Open Game…" flow: picks a
// specific .aslx file via the native file picker directly, instead of
// picking a folder and then disambiguating between multiple .aslx files
// found inside it (that in-app disambiguation step only exists for FSA,
// which has to grant permission at the folder level — Electron's real
// filesystem access has no such constraint).
//
// A hand-written minimal .aslx isn't enough for the engine to load (missing
// templates etc. — see examples/blank.aslx for how much a real one needs),
// so this creates a real game through the app's own "Create new game" form
// first, then duplicates the generated file under a second name in the same
// folder — giving a folder with two siblings without depending on the
// engine's exact minimum-viable-game shape.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run the build steps in electron.sh once first
// (dotnet build WasmEditor/WasmPlayer Debug, npm run build in AppShell and
// ElectronApp) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync, readFileSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
const gamesRoot = mkdtempSync(join(tmpdir(), 'quest-electron-games-'));

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
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

    async function stubPicker(filePathOrDir) {
        await app.evaluate(({ dialog }, fp) => {
            dialog.showOpenDialog = async () => ({ canceled: false, filePaths: [fp] });
        }, filePathOrDir);
    }

    // 1. Root now lands on the Play tab (PlayCatalog) by default — the /open
    // form lives behind the "Create" tab. Button label there should say
    // "Open game…", not "Open game folder".
    await win.waitForSelector('a:has-text("Create")', { timeout: 30000 });
    await win.click('a:has-text("Create")');
    await win.waitForSelector('button:has-text("Open game…")', { timeout: 30000 });
    console.log('PASS: create-game page shows "Open game…" (not "Open game folder") on Electron');

    // 2. Create a real game via the app's own form, into gamesRoot directly
    // (bypassing the default Documents/Quest Games location) so it's easy to
    // find on disk afterwards.
    await stubPicker(gamesRoot);
    await win.fill('input[placeholder="Game name"]', 'First');
    await win.waitForSelector('text=Text adventure', { timeout: 10000 });
    await win.click('button:has-text("Change location…")');
    await win.waitForFunction((dir) => document.body.innerText.includes(dir), gamesRoot, { timeout: 10000 });
    await win.click('button:has-text("Create")');
    await win.waitForSelector('button:has-text("Assets")', { timeout: 30000 });

    const gameDir = join(gamesRoot, 'First');
    const firstAslx = join(gameDir, 'First.aslx');
    const secondAslx = join(gameDir, 'Second.aslx');
    writeFileSync(secondAslx, readFileSync(firstAslx));
    console.log('PASS: "First" game created, duplicated as "Second.aslx" in the same folder');

    // 3. Pick "Second.aslx" directly out of a folder containing two .aslx
    // files — should load it immediately, no "choose one" screen (the old
    // folder-then-disambiguate behavior this replaces would have shown one).
    await clickMenuItem('File', 'New Game…');
    await win.waitForSelector('button:has-text("Open game…")', { timeout: 10000 });
    await stubPicker(secondAslx);
    await win.click('button:has-text("Open game…")');
    await win.waitForSelector('button:has-text("Assets")', { timeout: 30000 });
    const multiFileScreenShown = await win.isVisible('text=Multiple game files found');
    console.log('PASS: "Second.aslx" opened directly with no multi-file disambiguation screen:', !multiFileScreenShown);
    if (multiFileScreenShown) throw new Error('Unexpected multi-file disambiguation screen for Electron');

    // 4. Native "Open Game…" menu item (renamed from "Open Game Folder…")
    // works the same way, via the ?action=open&t=<nonce> query-string path.
    await clickMenuItem('File', 'New Game…');
    await win.waitForSelector('button:has-text("Open game…")', { timeout: 10000 });
    await stubPicker(firstAslx);
    await clickMenuItem('File', 'Open Game…');
    await win.waitForSelector('button:has-text("Assets")', { timeout: 30000 });
    console.log('PASS: native "Open Game…" menu item opens "First.aslx" directly');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await app?.close();
    rmSync(userDataDir, { recursive: true, force: true });
    rmSync(gamesRoot, { recursive: true, force: true });
}
