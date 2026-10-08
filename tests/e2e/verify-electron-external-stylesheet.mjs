// Regression coverage for issue #2452: a game that loads a .css file from its
// own folder with AddExternalStylesheet(GetFileURL(...)) worked once published
// to a .quest, but not when played from the desktop app. ElectronFileAdapter
// types the Blobs it reads from disk from a fixed extension list, which had no
// .css - so the stylesheet reached the player as
// data:application/octet-stream, and the browser refused to apply it.
//
// Plays the game through the Play tab's "Open a game file…" button, which
// serves sibling files through the same ElectronFileAdapter.
//
// Uses Playwright's _electron launcher against the already-built
// src/ElectronApp/dist - run electron.sh once first (or the build steps
// inside it) so dist/ and resources/app-static exist.
import { _electron as electron } from 'playwright';
import { mkdtempSync, copyFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
import { removeTempDirs } from './lib/electron-cleanup.mjs';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
const gameDir = mkdtempSync(join(tmpdir(), 'quest-electron-css-'));

const fixturesDir = join(dirname(fileURLToPath(import.meta.url)), 'fixtures');
const aslxPath = join(gameDir, 'external-stylesheet-test.aslx');
copyFileSync(join(fixturesDir, 'external-stylesheet-test.aslx'), aslxPath);
copyFileSync(join(fixturesDir, 'external-stylesheet-test.css'), join(gameDir, 'external-stylesheet-test.css'));

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[editor] [pageerror]', err.message));

    await app.evaluate(({ dialog }, fp) => {
        dialog.showOpenDialog = async () => ({ canceled: false, filePaths: [fp] });
    }, aslxPath);

    await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });
    const [playerWindow] = await Promise.all([
        app.waitForEvent('window'),
        win.click('button:has-text("Open a game file…")'),
    ]);
    playerWindow.on('pageerror', err => console.log('[player] [pageerror]', err.message));
    playerWindow.on('console', msg => { if (msg.type() === 'error') console.log('[player] [console.error]', msg.text()); });

    await playerWindow.waitForFunction(() => document.title === 'External stylesheet', null, { timeout: 15000 });
    console.log('[player] game booted');

    const link = await playerWindow.waitForSelector('link[rel="stylesheet"][href^="data:"]', { state: 'attached', timeout: 10000 });
    const href = await link.getAttribute('href');
    const mimeType = href.slice('data:'.length, href.indexOf(';'));
    if (mimeType !== 'text/css') {
        throw new Error(`Stylesheet data: URL has type '${mimeType}', expected 'text/css'`);
    }
    console.log('PASS: stylesheet handed over as a text/css data: URL');

    await playerWindow.waitForFunction(
        () => getComputedStyle(document.getElementById('divOutput')).borderTopColor === 'rgb(1, 2, 3)',
        null, { timeout: 10000 });
    console.log('PASS: the game\'s own stylesheet was applied');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await app?.close();
    removeTempDirs(userDataDir, gameDir);
}
