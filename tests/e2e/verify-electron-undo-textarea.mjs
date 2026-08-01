// Ad-hoc manual verification for the Cmd+Z-in-a-textarea fix (main.ts's
// editorUndoRedo): pressing the app-wide Undo accelerator while a text field
// inside the editor window is focused should invoke the native
// webContents.undo() edit command (so in-progress typing can be undone
// character-by-character, like any browser text field), not blindly route to
// EditorCore's app-level undo stack — which previously happened whenever the
// editor window had OS focus, textarea or not.
//
// Verifies both sides of the routing so a future regression that always
// takes one path would be caught:
//  - text field focused -> native undo fires (value reverts), no "undo"
//    menu-action reaches the renderer's onAction listener.
//  - no text field focused (tree/canvas) -> app-level "undo" menu-action
//    still fires, preserving the original app-level undo behavior.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run electron.sh once first (or the build steps
// inside it) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir, homedir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
// Electron creates games as a real folder on disk (unlike the browser build's
// in-browser draft) — use a unique name so reruns never collide with a
// leftover folder from a previous (e.g. failed) run.
const gameName = `UndoTest-${Date.now()}`;
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

    // Root is the Play/Create Home (PUBLIC_SHOW_HOME=true, see electron.sh) — switch to Create, make a local draft.
    await win.waitForSelector('a:has-text("Create")', { timeout: 30000 });
    await win.click('a:has-text("Create")');
    await win.waitForSelector('input[placeholder="Game name"]', { timeout: 15000 });
    await win.fill('input[placeholder="Game name"]', gameName);
    // "Game type" radios + a template list only render once a name is entered;
    // pick "Text adventure" and its first available template before creating.
    await win.locator('label:has-text("Text adventure")').click();
    await win.waitForSelector('text=Please select a template.', { state: 'detached', timeout: 15000 }).catch(() => {});
    const templateRadio = win.locator('input[type="radio"]').first();
    await templateRadio.waitFor({ timeout: 15000 });
    await templateRadio.click();
    // Electron (unlike the browser build) creates games as a real folder on disk
    // rather than an in-browser draft — the button is a plain "Create" near the
    // "Will be created as a new folder in: ..." text, not "Create local draft".
    await win.click('button:has-text("Create"):near(:text("Will be created as a new folder"))');

    // Editor loaded once the tree's default "room" object is selectable.
    await win.waitForSelector('text=GAME OBJECTS', { timeout: 30000 });
    console.log('[editor] editor loaded');

    // Record every app-level menu-action the renderer receives, alongside the +layout.svelte subscriber already listening.
    await win.evaluate(() => {
        window.__actionsSeen = [];
        window.electronApp.menu.onAction((a) => window.__actionsSeen.push(a));
    });

    // Select the "room" tree node (second top-level item under game), whose Setup tab has a plain <input> Name field.
    const roomNode = win.locator('text=room').first();
    await roomNode.click();
    await win.waitForSelector('text=Alias:', { timeout: 10000 }); // Room Setup tab loaded

    // Svelte binds .value as a JS property, not a static "value" attribute, so
    // match by DOM position instead: "Name:" is the first text input on the
    // properties panel (excluding the sidebar's "Filter game objects" box).
    const nameInput = win.locator('input[type="text"]:not([placeholder="Filter..."])').first();
    if ((await nameInput.inputValue()) !== 'room') {
        throw new Error(`Expected the Name field to read "room", got "${await nameInput.inputValue()}"`);
    }
    await nameInput.click();
    await win.keyboard.press('End');
    await win.keyboard.type('XYZ'); // real keystrokes, so Chromium builds a native undo entry
    const valueBeforeUndo = await nameInput.inputValue();
    console.log('[editor] typed into Name field, value now:', valueBeforeUndo);
    if (valueBeforeUndo !== 'roomXYZ') throw new Error(`Expected "roomXYZ", got "${valueBeforeUndo}"`);

    // --- Case A: text field focused -> Undo accelerator should take the native path ---
    await app.evaluate(({ Menu, BrowserWindow }) => {
        const menu = Menu.getApplicationMenu();
        const editMenu = menu.items.find((i) => i.label === 'Edit');
        const undoItem = editMenu.submenu.items.find((i) => i.label === 'Undo');
        const editorWin = BrowserWindow.getAllWindows()[0];
        undoItem.click(undefined, editorWin, undefined);
    });
    await win.waitForTimeout(500); // let the async executeJavaScript focus-check + native undo settle

    const valueAfterUndo = await nameInput.inputValue();
    const actionsAfterTextFieldUndo = await win.evaluate(() => window.__actionsSeen.slice());
    console.log('[editor] value after Undo (text field focused):', valueAfterUndo);
    console.log('[editor] app-level actions seen so far:', actionsAfterTextFieldUndo);

    if (valueAfterUndo === valueBeforeUndo) {
        throw new Error('FAIL: native undo did not revert the Name field — Cmd+Z-in-textfield bug is back');
    }
    if (actionsAfterTextFieldUndo.includes('undo')) {
        throw new Error('FAIL: app-level "undo" menu-action fired while a text field was focused');
    }
    console.log('PASS: text-field-focused Undo took the native webContents.undo() path, not app-level undo');

    // --- Case B: no text field focused (blur to the tree) -> app-level Undo should still fire ---
    await win.locator('text=GAME OBJECTS').click();
    await win.waitForTimeout(100);
    await app.evaluate(({ Menu, BrowserWindow }) => {
        const menu = Menu.getApplicationMenu();
        const editMenu = menu.items.find((i) => i.label === 'Edit');
        const undoItem = editMenu.submenu.items.find((i) => i.label === 'Undo');
        const editorWin = BrowserWindow.getAllWindows()[0];
        undoItem.click(undefined, editorWin, undefined);
    });
    await win.waitForTimeout(500);

    const actionsAfterTreeUndo = await win.evaluate(() => window.__actionsSeen.slice());
    console.log('[editor] app-level actions seen after tree-focused Undo:', actionsAfterTreeUndo);
    if (!actionsAfterTreeUndo.includes('undo')) {
        throw new Error('FAIL: app-level "undo" menu-action did not fire when no text field was focused');
    }
    console.log('PASS: tree-focused Undo still routes to the app-level EditorCore undo stack');

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
    app?.process().kill('SIGKILL');
    // SIGKILL doesn't wait for Chromium to finish its disk-cache writeback, so an immediate
    // rmSync can hit ENOTEMPTY on Cache_Data — maxRetries/retryDelay ride out that race.
    rmSync(userDataDir, { recursive: true, force: true, maxRetries: 5, retryDelay: 200 });
    rmSync(gameDirPath, { recursive: true, force: true });
}
