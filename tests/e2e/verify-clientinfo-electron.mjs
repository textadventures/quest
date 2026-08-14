// Ad-hoc manual verification: preload.ts now populates window.electronApp.platform
// from process.platform, and wasm-player.js's api/Game/{id} fetch now attaches
// source/version/platform query params. Requires a fresh build:
//   dotnet build src/WasmPlayer/WasmPlayer.csproj
//   (cd src/ElectronApp && npm run build)
import { _electron as electron } from 'playwright';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));

let app;
try {
    app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[editor] [pageerror]', err.message));

    await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });

    const platform = await win.evaluate(() => window.electronApp.platform);
    console.log('window.electronApp.platform in editor window:', platform);
    if (!['Mac', 'Windows', 'Linux'].includes(platform)) {
        throw new Error(`Expected a mapped platform, got ${JSON.stringify(platform)}`);
    }
    console.log('PASS: preload.ts populated window.electronApp.platform');

    // Intercept at the context level so the route is already active before
    // the new player window navigates and fires its api/game/{id} fetch.
    let capturedUrl = null;
    await app.context().route('**/api/game/**', async (route) => {
        capturedUrl = route.request().url();
        await route.fulfill({
            status: 404,
            contentType: 'application/json',
            body: JSON.stringify({ message: 'not found (expected — test id)' }),
        });
    });

    const [playerWindow] = await Promise.all([
        app.waitForEvent('window'),
        win.evaluate(() => window.electronApp.player.openWindow({ id: 'clientinfo-test-id' })),
    ]);
    playerWindow.on('pageerror', err => console.log('[player] [pageerror]', err.message));

    await playerWindow.waitForTimeout(1500);

    console.log('Captured request URL:', capturedUrl);
    if (!capturedUrl) throw new Error('no request to api/game/* was observed from the player window');

    const url = new URL(capturedUrl);
    const source = url.searchParams.get('source');
    const version = url.searchParams.get('version');
    const gamePlatform = url.searchParams.get('platform');
    console.log({ source, version, platform: gamePlatform });

    if (source !== 'electron') throw new Error(`expected source=electron, got ${source}`);
    if (!version) throw new Error('expected a version param, got none');
    if (!['Mac', 'Windows', 'Linux'].includes(gamePlatform)) {
        throw new Error(`expected a UA-sniffed platform, got ${JSON.stringify(gamePlatform)}`);
    }
    console.log('PASS: player window\'s api/Game/{id} fetch carries source/version/platform');

    await playerWindow.close();
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await app?.close();
    rmSync(userDataDir, { recursive: true, force: true });
}
