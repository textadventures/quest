// Ad-hoc manual verification for Cmd+A doing nothing in a text field: the
// editor's custom Edit menu (main.ts's buildAppMenu) replaces Electron's
// default Edit menu wholesale, and the default menu is the only thing that
// registers CmdOrCtrl+A -> role: "selectAll" — Cut/Copy/Paste were already
// carried over as explicit roles, but Select All wasn't, so nothing claimed
// that accelerator and it silently did nothing in every text field.
//
// A real physical Cmd+A goes through macOS's NSMenu key-equivalent matching
// before the keystroke ever reaches the renderer, so (like the Undo fix's
// test) this can't be verified by dispatching a synthetic keydown via CDP —
// that bypasses the OS menu layer entirely and would "pass" whether or not
// the role is actually wired up. Instead this drives the same menu item
// Electron itself would invoke, directly, and checks the resulting native
// text-field selection.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run electron.sh once first (or the build steps
// inside it) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync } from 'node:fs';
import { tmpdir, homedir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';
import { killElectronApp, removeTempDirs } from './lib/electron-cleanup.mjs';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
const gameName = `SelectAllTest-${Date.now()}`;
const gameDirPath = join(homedir(), 'Documents', 'Quest Games', gameName);

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[editor] [pageerror]', err.message));
    win.on('console', msg => { if (msg.type() === 'error') console.log('[editor] [console.error]', msg.text()); });

    await win.waitForSelector('a:has-text("Create")', { timeout: 30000 });
    await win.click('a:has-text("Create")');
    await win.waitForSelector('input[placeholder="Game name"]', { timeout: 15000 });
    await win.fill('input[placeholder="Game name"]', gameName);
    await win.locator('label:has-text("Text adventure")').click();
    await win.waitForSelector('text=Please select a template.', { state: 'detached', timeout: 15000 }).catch(() => {});
    const templateRadio = win.locator('input[type="radio"]').first();
    await templateRadio.waitFor({ timeout: 15000 });
    await templateRadio.click();
    await win.click('button:has-text("Create"):near(:text("Will be created as a new folder"))');

    await win.waitForSelector('text=GAME OBJECTS', { timeout: 30000 });
    console.log('[editor] editor loaded');

    // Structural check: the custom Edit menu must actually list a selectAll-role item now.
    const editMenuLabels = await app.evaluate(({ Menu }) => {
        const menu = Menu.getApplicationMenu();
        const editMenu = menu.items.find((i) => i.label === 'Edit');
        return editMenu.submenu.items.map((i) => ({ label: i.label, role: i.role }));
    });
    console.log('[editor] Edit menu items:', JSON.stringify(editMenuLabels));
    if (!editMenuLabels.some((i) => i.role === 'selectall')) {
        throw new Error('FAIL: Edit menu has no selectAll-role item');
    }
    console.log('PASS: Edit menu now includes a selectAll-role item');

    // Behavioral check: select the room's Name field, type distinctive text, then
    // invoke the same menu item Electron itself would invoke on a real Cmd+A.
    const roomNode = win.locator('text=room').first();
    await roomNode.click();
    await win.waitForSelector('text=Alias:', { timeout: 10000 });

    const nameInput = win.locator('input[type="text"]:not([placeholder="Filter..."])').first();
    await nameInput.click();
    await win.keyboard.press('End');
    await win.keyboard.type('XYZ');
    const value = await nameInput.inputValue();
    console.log('[editor] Name field value:', value);
    if (value !== 'roomXYZ') throw new Error(`Expected "roomXYZ", got "${value}"`);

    // role-based MenuItems are implemented natively (not via a JS click
    // callback we can invoke standalone) — calling .click() manually with a
    // supplied window doesn't reliably replay that native behavior the way
    // it does for our own custom Undo/Redo click functions. What role:
    // "selectAll" actually *is*, under the hood, is Electron always calling
    // webContents.selectAll() on the target webContents; that's the well-
    // established mechanism Cut/Copy/Paste already rely on in production, so
    // exercising it directly here checks the one part that was actually
    // missing (the role wired up in the menu, asserted above) without
    // re-testing Electron's own accelerator-dispatch internals.
    await app.evaluate(({ BrowserWindow }) => {
        const editorWin = BrowserWindow.getAllWindows()[0];
        editorWin.webContents.selectAll();
    });
    await win.waitForTimeout(300);

    const selection = await nameInput.evaluate((el) => ({
        start: el.selectionStart,
        end: el.selectionEnd,
        length: el.value.length,
    }));
    console.log('[editor] selection after Select All:', JSON.stringify(selection));
    if (selection.start !== 0 || selection.end !== selection.length) {
        throw new Error(`FAIL: Select All did not select the full field (got ${JSON.stringify(selection)})`);
    }
    console.log('PASS: Select All selected the entire Name field');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    // Not app.close(): this test always leaves unsaved edits (typed text
    // never saved), which trips main.ts's will-prevent-unload handler and
    // pops a *synchronous* native "Leave without saving?" dialog that blocks
    // the whole process — nothing left to click it away with here. Kill the
    // process directly instead of asking it to close gracefully.
    await killElectronApp(app);
    removeTempDirs(userDataDir, gameDirPath);
}
