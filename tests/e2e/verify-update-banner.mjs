// Ad-hoc manual verification: the desktop app's update-check banner
// (UpdateBanner.svelte, PlayCatalog.svelte, home-catalog.ts's fetchCatalog).
// Requires a fresh build:
//   PUBLIC_SHOW_HOME=true npm run build --prefix src/AppShell
//   dotnet build src/WasmEditor/WasmEditor.csproj
//   dotnet build src/WasmPlayer/WasmPlayer.csproj
//   (cd src/ElectronApp && npm run build)
import { _electron as electron } from 'playwright';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

function catalogResponse(update) {
    return {
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ categories: [], update }),
    };
}

async function launch() {
    const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
    const app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
    return { app, userDataDir };
}

let failed = false;

// ── Run 1: no update available — banner must not render ────────────────────
{
    const { app, userDataDir } = await launch();
    try {
        await app.context().route('**/api/Catalog**', route => route.fulfill(catalogResponse(null)));

        const win = await app.firstWindow();
        win.on('pageerror', err => console.log('[pageerror]', err.message));
        await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });
        await win.waitForTimeout(1000);

        const bannerVisible = await win.locator('text=is available').count();
        if (bannerVisible !== 0) throw new Error('banner rendered despite update:null');
        console.log('PASS: no banner when update is null');
    } catch (err) {
        console.error('FAIL (run 1):', err.message);
        failed = true;
    } finally {
        await app.close();
        rmSync(userDataDir, { recursive: true, force: true });
    }
}

// ── Run 2: update available — banner renders, links out, dismiss persists ──
{
    const { app, userDataDir } = await launch();
    try {
        const update = { latestVersion: 'v6.0.0-beta.99', url: 'https://github.com/textadventures/quest/releases/latest' };
        await app.context().route('**/api/Catalog**', route => route.fulfill(catalogResponse(update)));

        const win = await app.firstWindow();
        win.on('pageerror', err => console.log('[pageerror]', err.message));
        await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });

        const banner = win.locator('text=Quest Viva v6.0.0-beta.99 is available.');
        await banner.waitFor({ timeout: 10000 });
        console.log('PASS: banner rendered with the latest version');

        const downloadLink = win.locator('a:has-text("Download")');
        const href = await downloadLink.getAttribute('href');
        if (href !== update.url) throw new Error(`expected Download link href ${update.url}, got ${href}`);
        console.log('PASS: Download link points at', href);

        await win.locator('button:has-text("Later")').click();
        await banner.waitFor({ state: 'detached', timeout: 5000 });
        console.log('PASS: banner dismissed on "Later" click');

        await win.reload();
        await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });
        await win.waitForTimeout(1000);
        const stillHidden = await win.locator('text=is available').count();
        if (stillHidden !== 0) throw new Error('banner reappeared after reload despite dismissal');
        console.log('PASS: dismissal persists across reload');
    } catch (err) {
        console.error('FAIL (run 2):', err.message);
        failed = true;
    } finally {
        await app.close();
        rmSync(userDataDir, { recursive: true, force: true });
    }
}

if (failed) {
    process.exitCode = 1;
} else {
    console.log('PASS: all checks passed');
}
