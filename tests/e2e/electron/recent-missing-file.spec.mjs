// A Recent entry whose file has since been moved or deleted: clicking it on the
// Play tab (Recently Played) or the Create tab (Recent) should say plainly that
// the file can't be found and offer to remove it from the list, not show
// Node's ENOENT text inside Electron's "Error invoking remote method" wrapper.
// On the Play tab that notice replaces the card itself.
//
// Seeds both recent lists through the same recent:add IPC the app itself uses,
// pointing at a file that was never created.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist — run electron.sh once first (or the build steps
// inside it) so dist/ and resources/app-static exist.
import { test } from '@playwright/test';
import { _electron as electron } from 'playwright';
import { mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';
import { removeTempDirs } from '../lib/electron-cleanup.mjs';

test('recent-missing-file', async () => {
    const electronAppDir = join(import.meta.dirname, '..', '..', '..', 'src', 'ElectronApp');
    const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

    const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
    const gameDir = mkdtempSync(join(tmpdir(), 'quest-electron-missing-'));
    const filename = 'gone.aslx';

    let app;
    try {
        app = await electron.launch({
            executablePath: electronExecutablePath,
            args: [electronAppDir, `--user-data-dir=${userDataDir}`],
        });
        const win = await app.firstWindow();
        win.on('pageerror', err => console.log('[pageerror]', err.message));

        await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });
        await win.evaluate(async ({ dir, file }) => {
            await window.electronApp.recent.add('play', dir, file);
            await window.electronApp.recent.add('edit', dir, file);
        }, { dir: gameDir, file: filename });

        // Play tab: the list was read on mount, so reload to pick up the seeded entry.
        await win.reload();
        const playCard = win.locator('button', { hasText: filename });
        await playCard.waitFor({ timeout: 30000 });
        await playCard.click();

        // The card itself turns into a "File not found" notice with a Remove button.
        const missingNotice = win.locator('[role="alert"]', { hasText: 'File not found' });
        await missingNotice.waitFor({ timeout: 10000 });
        const playText = await missingNotice.innerText();
        console.log('[play] card shows:', playText);
        for (const raw of ['ENOENT', 'Error invoking remote method']) {
            if (playText.includes(raw)) throw new Error(`Raw error text "${raw}" leaked onto the card: ${playText}`);
        }
        if (await playCard.count() !== 0) throw new Error('Missing file still shown as a playable card');

        await win.getByRole('button', { name: 'Remove', exact: true }).click();
        await missingNotice.waitFor({ state: 'detached', timeout: 5000 });
        if (await win.locator('text=' + filename).count() !== 0) throw new Error('Missing file still listed after Remove');
        const playRecents = await win.evaluate(() => window.electronApp.recent.list('play'));
        if (playRecents.length !== 0) throw new Error(`Play recent list not emptied: ${JSON.stringify(playRecents)}`);
        console.log('PASS: Play tab explained the missing file and removed it from Recently Played');

        // Create tab: same file in the editor's Recent list.
        await win.getByRole('link', { name: 'Create' }).click();
        const editEntry = win.locator('button', { hasText: filename }).first();
        await editEntry.waitFor({ timeout: 30000 });
        await editEntry.click();

        const editError = win.locator('p.text-error-600-400', { hasText: filename });
        await editError.waitFor({ timeout: 10000 });
        const editText = await editError.innerText();
        console.log('[create] error shown:', editText);
        assertFriendly(editText);

        // Two "Remove from Recent" controls now: the row's own × (aria-label)
        // and the one under the error. Use the one next to the error.
        await editError.locator('xpath=following-sibling::button[1]').click();
        await win.waitForFunction(
            () => window.electronApp.recent.list('edit').then(l => l.length === 0),
            null, { timeout: 5000 });
        if (await editError.count() !== 0) throw new Error('Create tab error still showing after removing the entry');
        console.log('PASS: Create tab explained the missing file and removed it from Recent');
    } finally {
        await app?.close();
        removeTempDirs(userDataDir, gameDir);
    }

    function assertFriendly(text) {
        if (!text.includes(filename) || !text.includes(gameDir)) {
            throw new Error(`Expected the file and folder in the message, got: ${text}`);
        }
        for (const raw of ['ENOENT', 'Error invoking remote method', 'Error:']) {
            if (text.includes(raw)) throw new Error(`Raw error text "${raw}" leaked into the message: ${text}`);
        }
    }
});
