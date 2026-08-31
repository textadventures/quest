// Ad-hoc manual verification for main.ts's async "Leave without saving?"
// dialog (the will-prevent-unload handler). The previous implementation used
// dialog.showMessageBoxSync, which blocks the entire main process — no IPC,
// no other window's menu actions — for as long as it's on screen. The fix
// defers: the first close attempt just stays blocked (event.preventDefault()
// is NOT called synchronously) while an async dialog.showMessageBox() is
// pending, and only a confirmed "Leave" answer calls editorWindow.close()
// again, which re-fires this same event — this time forced through
// immediately via a forceCloseConfirmed flag, without asking again.
//
// Driving this through a *real* window close (clicking the OS close button,
// or BrowserWindow.close()) turns out to also trigger Chromium's own native
// beforeunload confirmation as a CDP-visible "javascript dialog", which
// races with Playwright's own default auto-dismiss handling for such dialogs
// and throws unrelated to anything this change touches — that plumbing
// (beforeunload -> will-prevent-unload) is unchanged, pre-existing Electron
// behavior. So instead this emits 'will-prevent-unload' on the editor
// window's webContents directly (it's a plain EventEmitter event) to
// exercise exactly the handler logic that changed, in isolation.
//
// forceCloseConfirmed is real per-window state that's meant to persist once
// set (so a confirmed close's retry doesn't ask again) — so "Leave" and
// "Cancel" each get their own fresh app launch/window rather than sharing
// one, otherwise the second scenario would inherit the first's already-
// confirmed flag and skip its own dialog (which is correct production
// behavior, just not what a fresh scenario means to exercise).
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run electron.sh once first (or the build steps
// inside it) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';
import { killElectronApp, removeTempDirs } from './lib/electron-cleanup.mjs';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

async function withFreshApp(fn) {
    const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
    let app;
    try {
        app = await electron.launch({ executablePath: electronExecutablePath, args: [electronAppDir, `--user-data-dir=${userDataDir}`] });
        const win = await app.firstWindow();
        win.on('pageerror', err => console.log('[editor] [pageerror]', err.message));
        await win.waitForSelector('a:has-text("Create"), a:has-text("Play")', { timeout: 30000 });
        return await fn(app, win);
    } finally {
        await killElectronApp(app);
        removeTempDirs(userDataDir);
    }
}

try {
    // --- Scenario: "Leave" ---
    const leaveResult = await withFreshApp((app) => app.evaluate(({ BrowserWindow, dialog }) => new Promise((resolve, reject) => {
        try {
            const editorWin = BrowserWindow.getAllWindows()[0];
            let closeCalled = false;
            editorWin.close = () => { closeCalled = true; }; // spy, avoid the real native close/CDP-dialog path

            let resolveDialog;
            let showMessageBoxCalls = 0;
            dialog.showMessageBox = () => {
                showMessageBoxCalls++;
                return new Promise((res) => { resolveDialog = () => res({ response: 1 }); }); // 1 = "Leave"
            };

            const event1 = { defaultPrevented: false, preventDefault() { this.defaultPrevented = true; } };
            editorWin.webContents.emit('will-prevent-unload', event1);
            const blockedSynchronously = event1.defaultPrevented === false; // must stay false: no sync preventDefault

            setTimeout(() => {
                try {
                    if (typeof resolveDialog !== 'function') throw new Error('dialog.showMessageBox was never called on the first attempt');
                    resolveDialog();
                    setTimeout(() => {
                        const event2 = { defaultPrevented: false, preventDefault() { this.defaultPrevented = true; } };
                        editorWin.webContents.emit('will-prevent-unload', event2); // stands in for close()'s real retry
                        resolve({ blockedSynchronously, closeCalledAfterLeave: closeCalled, forcedThroughOnRetry: event2.defaultPrevented, showMessageBoxCalls });
                    }, 50);
                } catch (err) { reject(err); }
            }, 50);
        } catch (err) { reject(err); }
    })));
    console.log('[editor] Leave scenario:', JSON.stringify(leaveResult));
    if (!leaveResult.blockedSynchronously) throw new Error('FAIL: first will-prevent-unload call synchronously prevented default — no room for an async dialog');
    if (!leaveResult.closeCalledAfterLeave) throw new Error('FAIL: confirming "Leave" did not call editorWindow.close()');
    if (!leaveResult.forcedThroughOnRetry) throw new Error('FAIL: retried will-prevent-unload after "Leave" was not forced through synchronously');
    if (leaveResult.showMessageBoxCalls !== 1) throw new Error(`FAIL: expected exactly one dialog for the retry (forceCloseConfirmed should skip it), got ${leaveResult.showMessageBoxCalls}`);
    console.log('PASS: "Leave" flow — first attempt stays open async, close() called, retry forced through without a second dialog');

    // --- Scenario: "Cancel" (fresh app/window — forceCloseConfirmed must start false) ---
    const cancelResult = await withFreshApp((app) => app.evaluate(({ BrowserWindow, dialog }) => new Promise((resolve, reject) => {
        try {
            const editorWin = BrowserWindow.getAllWindows()[0];
            let closeCalled = false;
            editorWin.close = () => { closeCalled = true; };

            let resolveDialog;
            dialog.showMessageBox = () => new Promise((res) => { resolveDialog = () => res({ response: 0 }); }); // 0 = "Cancel"

            const event1 = { defaultPrevented: false, preventDefault() { this.defaultPrevented = true; } };
            editorWin.webContents.emit('will-prevent-unload', event1);

            setTimeout(() => {
                try {
                    if (typeof resolveDialog !== 'function') throw new Error('dialog.showMessageBox was never called on the first attempt');
                    resolveDialog();
                    setTimeout(() => {
                        const event2 = { defaultPrevented: false, preventDefault() { this.defaultPrevented = true; } };
                        editorWin.webContents.emit('will-prevent-unload', event2); // a later close attempt should ask again, not force through
                        resolve({ closeCalledAfterCancel: closeCalled, forcedOnSecondAttempt: event2.defaultPrevented });
                    }, 50);
                } catch (err) { reject(err); }
            }, 50);
        } catch (err) { reject(err); }
    })));
    console.log('[editor] Cancel scenario:', JSON.stringify(cancelResult));
    if (cancelResult.closeCalledAfterCancel) throw new Error('FAIL: "Cancel" incorrectly called editorWindow.close()');
    if (cancelResult.forcedOnSecondAttempt) throw new Error('FAIL: a later attempt after "Cancel" was forced through instead of asking again');
    console.log('PASS: "Cancel" flow — window never closed, a later attempt still asks again rather than forcing through');

    // --- Responsiveness proof: main process answers unrelated work while a dialog is deliberately left pending ---
    const responsive = await withFreshApp((app) => app.evaluate(({ BrowserWindow, dialog }) => new Promise((resolve) => {
        const editorWin = BrowserWindow.getAllWindows()[0];
        editorWin.close = () => {};
        dialog.showMessageBox = () => new Promise(() => {}); // never resolves
        editorWin.webContents.emit('will-prevent-unload', { defaultPrevented: false, preventDefault() {} });
        // If the handler were still synchronous (the old showMessageBoxSync), this
        // very evaluate() call would never have gotten a chance to run at all.
        setTimeout(() => resolve(true), 100);
    })));
    if (!responsive) throw new Error('FAIL: main process did not respond promptly with a dialog left pending');
    console.log('PASS: main process stayed responsive with an unresolved dialog pending (would have hung under the old *Sync call)');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
}
