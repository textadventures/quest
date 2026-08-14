// Verifies the AppShell theme setting (Light/Dark/Match system, added in the
// theme-toggle feature) persists across Electron relaunches, mirroring
// verify-electron-locale-persist.mjs.
//
// Root cause this guards against: AppShell's theme store persists to
// localStorage, which is scoped per origin, but ElectronApp's static-server.ts
// binds to an OS-assigned ephemeral port (listen(0, ...)) that changes every
// launch — so the origin was never the same twice. Fix moves Electron's
// persistence to a userData JSON file over IPC (theme-store.ts / ipc/theme.ts),
// same pattern as locale-store.ts / ipc/locale.ts.
//
// This launches Electron twice against the *same* --user-data-dir: first
// launch switches the theme to Dark via the real Settings UI and checks the
// .dark class + color-scheme apply live; second launch (a fresh process,
// fresh random port) checks the stored theme survived via BOTH the async IPC
// getter AND the synchronous pre-paint getSync (the one app.html's inline
// script uses, which must agree or there'd be a flash of the wrong theme).
import { _electron as electron } from 'playwright';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));

// The Settings button's title/aria-label are translated (t("common.settings")),
// so they can't be used as a locale-independent selector. It's
// HomeHeader.svelte's only <button>.
const SETTINGS_BUTTON = 'header button.home-header-link';

async function launch() {
    const app = await electron.launch({
        executablePath: electronExecutablePath,
        args: [electronAppDir, `--user-data-dir=${userDataDir}`],
    });
    const win = await app.firstWindow();
    win.on('pageerror', err => console.log('[editor] [pageerror]', err.message));
    return { app, win };
}

async function htmlThemeState(win, label) {
    const s = await win.evaluate(() => ({
        dark: document.documentElement.classList.contains('dark'),
        colorScheme: document.documentElement.style.colorScheme,
    }));
    console.log(`[${label}] html.dark=${s.dark} colorScheme=${s.colorScheme}`);
    return s;
}

try {
    // --- Launch 1: switch theme to Dark ---
    {
        const { app, win } = await launch();
        await win.waitForSelector(SETTINGS_BUTTON, { timeout: 30000 });
        console.log('[launch 1] loaded on port', new URL(win.url()).port);

        // Default is "system"; headless Electron follows the OS (light here).
        let s = await htmlThemeState(win, 'launch 1 initial');
        if (s.dark !== false) throw new Error(`Expected default system theme to render light, got html.dark=${s.dark}`);

        await win.click(SETTINGS_BUTTON);
        await win.waitForSelector('#settings-theme', { timeout: 10000 });
        await win.selectOption('#settings-theme', 'dark');

        s = await htmlThemeState(win, 'launch 1 after Dark');
        if (!s.dark) throw new Error('Expected .dark class after selecting Dark');
        if (s.colorScheme !== 'dark') throw new Error(`Expected colorScheme dark after selecting Dark, got ${s.colorScheme}`);

        const stored = await win.evaluate(() => window.electronApp.theme.get());
        console.log('[launch 1] theme.get() after set:', stored);
        if (stored !== 'dark') throw new Error(`Expected theme store to read 'dark' after selecting it, got ${JSON.stringify(stored)}`);
        console.log('PASS: theme persisted to userData store within the same launch');

        await app.close();
    }

    // --- Launch 2: fresh process, same --user-data-dir, different random port ---
    {
        const { app, win } = await launch();
        await win.waitForSelector(SETTINGS_BUTTON, { timeout: 30000 });
        const portAfter = new URL(win.url()).port;
        console.log('[launch 2] loaded on port', portAfter);
        if (portAfter === undefined) throw new Error('Expected a fresh random port on relaunch');

        // The async IPC getter (theme-store.ts's readTheme).
        const stored = await win.evaluate(() => window.electronApp.theme.get());
        console.log('[launch 2] theme.get():', stored);
        if (stored !== 'dark') throw new Error(`Expected theme to still be 'dark' after relaunch, got ${JSON.stringify(stored)}`);

        // The synchronous pre-paint getter (app.html's inline script) must
        // agree, or a relaunch would flash the wrong theme.
        const storedSync = await win.evaluate(() => window.electronApp.theme.getSync());
        console.log('[launch 2] theme.getSync():', storedSync);
        if (storedSync !== 'dark') throw new Error(`Expected theme.getSync() to agree with get(), got ${JSON.stringify(storedSync)}`);

        // And the UI itself must have applied it (the .dark class comes from
        // app.html's inline script at parse time, before the Svelte app boots).
        const s = await htmlThemeState(win, 'launch 2 after relaunch');
        if (!s.dark) throw new Error('Expected .dark class on relaunch (pre-paint script applied stored Dark)');
        if (s.colorScheme !== 'dark') throw new Error(`Expected colorScheme dark on relaunch, got ${s.colorScheme}`);

        console.log('PASS: Dark survived a full app relaunch (fresh static-server port each time)');
        await app.close();
    }

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    rmSync(userDataDir, { recursive: true, force: true });
}
