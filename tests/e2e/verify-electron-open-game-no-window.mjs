// Ad-hoc manual verification for File > New Game…/Open Game… doing nothing
// when the editor window has been closed entirely (macOS: the app keeps
// running with the menu bar up, but zero windows open) — sendMenuAction
// required editorWindow to already exist, so with it null, both
// focusEditorWindow() and the plain IPC send silently no-op. The comment on
// focusEditorWindow already claimed "every File-menu editor action is
// reachable even ... on mac, no window at all", but that was never actually
// true until this fix: sendMenuAction now recreates the editor window
// (landing straight on /open, with ?action=open for "Open Game…" so the
// file picker opens immediately, matching the existing file-association
// cold-start pattern in initialUrlPath) when there isn't one already.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run electron.sh once first (or the build steps
// inside it) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

async function clickMenuItem(app, itemLabel) {
    await app.evaluate(({ Menu, BrowserWindow }, label) => {
        const menu = Menu.getApplicationMenu();
        const fileMenu = menu.items.find((i) => i.label === 'File');
        const item = fileMenu.submenu.items.find((i) => i.label === label);
        const win = BrowserWindow.getAllWindows()[0]; // may be undefined — that's the whole point being tested
        item.click(undefined, win, undefined);
    }, itemLabel);
}

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));

let app;
try {
    app = await electron.launch({ executablePath: electronExecutablePath, args: [electronAppDir, `--user-data-dir=${userDataDir}`] });
    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[editor] [pageerror]', err.message));
    await win.waitForSelector('a:has-text("Create"), a:has-text("Play")', { timeout: 30000 });
    console.log('[editor] app ready, closing the only window');

    // Fresh Home page, nothing loaded/dirtied — closes immediately, no beforeunload dialog involved.
    await app.evaluate(({ BrowserWindow }) => BrowserWindow.getAllWindows()[0].close());
    for (let i = 0; i < 20; i++) {
        const count = await app.evaluate(({ BrowserWindow }) => BrowserWindow.getAllWindows().length);
        if (count === 0) break;
        await new Promise((r) => setTimeout(r, 100));
    }
    const windowCount = await app.evaluate(({ BrowserWindow }) => BrowserWindow.getAllWindows().length);
    console.log('[editor] window count after close:', windowCount);
    if (windowCount !== 0) throw new Error('FAIL: window did not actually close (test setup problem, not the fix)');

    // --- "Open Game…" with no window open ---
    const [newWinForOpen] = await Promise.all([
        app.waitForEvent('window', { timeout: 10000 }),
        clickMenuItem(app, 'Open Game…'),
    ]);
    await newWinForOpen.waitForLoadState();
    console.log('[editor] "Open Game…" with no window recreated one at:', newWinForOpen.url());
    if (!newWinForOpen.url().includes('/open') || !newWinForOpen.url().includes('action=open')) {
        throw new Error(`FAIL: expected the new window to land on /open?action=open..., got ${newWinForOpen.url()}`);
    }
    console.log('PASS: "Open Game…" with no window open recreates a window and lands on the file-open action');

    await app.evaluate(({ BrowserWindow }) => BrowserWindow.getAllWindows()[0].close());
    for (let i = 0; i < 20; i++) {
        const count = await app.evaluate(({ BrowserWindow }) => BrowserWindow.getAllWindows().length);
        if (count === 0) break;
        await new Promise((r) => setTimeout(r, 100));
    }

    // --- "New Game…" with no window open ---
    const [newWinForNew] = await Promise.all([
        app.waitForEvent('window', { timeout: 10000 }),
        clickMenuItem(app, 'New Game…'),
    ]);
    await newWinForNew.waitForLoadState();
    console.log('[editor] "New Game…" with no window recreated one at:', newWinForNew.url());
    if (!newWinForNew.url().includes('/open') || newWinForNew.url().includes('action=open')) {
        throw new Error(`FAIL: expected the new window to land on plain /open (no action=open), got ${newWinForNew.url()}`);
    }
    console.log('PASS: "New Game…" with no window open recreates a window and lands on /open');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    app?.process().kill('SIGKILL');
    // SIGKILL doesn't wait for Chromium to finish its disk-cache writeback, so an immediate
    // rmSync can hit ENOTEMPTY on Cache_Data — maxRetries/retryDelay ride out that race.
    rmSync(userDataDir, { recursive: true, force: true, maxRetries: 5, retryDelay: 200 });
}
