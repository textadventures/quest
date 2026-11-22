// The desktop editor must never autosave over a change made to the game file outside it (by an
// AI agent or a text editor, say) — issue #2922. The main-process watcher (ipc/file-watch.ts)
// shows the banner as soon as the file changes, but nothing used to stop a later autosave writing
// the editor's stale copy over it, even with the banner showing.
//
// 1. Change the file on disk: the watcher shows the banner. Dismiss it and edit anyway: autosave
//    still writes nothing, and the banner comes back asking which copy to keep. "Use the changed
//    file" loads the outside change.
// 2. Change it again and edit: "Keep my changes" saves over the file.
//
// Uses Playwright's _electron launcher against the already-built src/ElectronApp/dist — run the
// build steps in electron.sh once first so dist/ and resources/app-static exist.
import { test } from '@playwright/test';
import { _electron as electron } from 'playwright';
import { mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';
import { killElectronApp, removeTempDirs } from '../lib/electron-cleanup.mjs';

test('disk-change-save-guard', async () => {
    const electronAppDir = join(import.meta.dirname, '..', '..', '..', 'src', 'ElectronApp');
    const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

    const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
    const gamesRoot = mkdtempSync(join(tmpdir(), 'quest-electron-games-'));
    const gameFile = join(gamesRoot, 'Guard', 'Guard.aslx');

    let app;
    let win;
    try {
        app = await electron.launch({
            executablePath: electronExecutablePath,
            args: [electronAppDir, `--user-data-dir=${userDataDir}`],
        });
        win = await app.firstWindow();
        win.on('pageerror', err => console.log('[pageerror]', err.message));
        win.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

        // Adds a room, as an agent might, so the outside change is easy to see in the tree.
        function changeOutside(roomName) {
            const xml = readFileSync(gameFile, 'utf8');
            if (!xml.includes('</asl>')) throw new Error('Unexpected game XML: no </asl>');
            const changed = xml.replace('</asl>', `  <object name="${roomName}">\n    <inherit name="editor_room" />\n  </object>\n</asl>`);
            writeFileSync(gameFile, changed);
            return changed;
        }

        // See appshell/autosave.spec.mjs for why the Filter box is excluded and the events
        // dispatched by hand.
        async function editDescription(text) {
            const box = win.locator('textarea, input[type="text"]:not([placeholder="Filter..."])').first();
            await box.waitFor({ timeout: 10000 });
            await box.fill(text);
            await box.dispatchEvent('input');
            await box.dispatchEvent('change');
            await win.click('.toolbar-divider');
            await win.locator('.save-chip-unsaved').waitFor({ state: 'visible', timeout: 5000 });
        }

        const banner = win.getByTestId('file-changed-banner');

        // Waits longer than the 800 ms autosave debounce, then checks the file is still the
        // outside version.
        async function expectHeld(expected) {
            await banner.getByRole('button', { name: 'Keep my changes' }).waitFor({ timeout: 10000 });
            await win.waitForTimeout(2000);
            if (readFileSync(gameFile, 'utf8') !== expected) throw new Error('The editor saved over the change made outside it');
            if (!(await win.locator('.save-chip-unsaved').isVisible())) throw new Error('Expected the edit to still show as unsaved');
        }

        // Create a game in gamesRoot through the app's own form.
        await app.evaluate(({ dialog }, dir) => {
            dialog.showOpenDialog = async () => ({ canceled: false, filePaths: [dir] });
        }, gamesRoot);
        await win.waitForSelector('a:has-text("Create")', { timeout: 30000 });
        await win.click('a:has-text("Create")');
        await win.fill('input[placeholder="Game name"]', 'Guard');
        await win.waitForSelector('text=Text adventure', { timeout: 10000 });
        await win.click('button:has-text("Change location…")');
        await win.waitForFunction((dir) => document.body.innerText.includes(dir), gamesRoot, { timeout: 10000 });
        await win.click('button:has-text("Create")');
        await win.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });

        // --- 1. The watcher's banner, dismissed, then Use the changed file ---
        const outside1 = changeOutside('agentroomone');
        await banner.getByRole('button', { name: 'Reload from disk' }).waitFor({ timeout: 10000 });
        await banner.getByRole('button', { name: 'Dismiss' }).click();
        await editDescription(`Dropped ${Date.now()}`);
        await expectHeld(outside1);
        await banner.getByRole('button', { name: 'Use the changed file' }).click();
        await banner.waitFor({ state: 'hidden', timeout: 30000 });
        if (await win.locator('.save-chip-unsaved').isVisible()) throw new Error('Expected nothing unsaved after "Use the changed file"');
        const filter = win.locator('input[placeholder="Filter..."]');
        await filter.fill('agentroomone');
        await win.locator('[role="treeitem"]:has-text("agentroomone")').first().waitFor({ timeout: 5000 });
        await filter.fill('');
        if (readFileSync(gameFile, 'utf8') !== outside1) throw new Error('"Use the changed file" changed the file');

        // --- 2. Keep my changes ---
        const outside2 = changeOutside('agentroomtwo');
        const kept = `Kept ${Date.now()}`;
        await editDescription(kept);
        await expectHeld(outside2);
        await banner.getByRole('button', { name: 'Keep my changes' }).click();
        await win.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
        const afterKeep = readFileSync(gameFile, 'utf8');
        if (!afterKeep.includes(kept)) throw new Error('"Keep my changes" did not save the edit');
        if (afterKeep.includes('agentroomtwo')) throw new Error('"Keep my changes" kept the outside change too');
        if (await banner.isVisible()) throw new Error('Banner still showing after "Keep my changes"');
    } catch (err) {
        await win?.screenshot({ path: '/tmp/electron-disk-change-save-guard-failure.png' });
        throw err;
    } finally {
        await killElectronApp(app);
        removeTempDirs(userDataDir, gamesRoot);
    }
});
