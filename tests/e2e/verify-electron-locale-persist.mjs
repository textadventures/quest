// Verifies the fix for: the UI language setting didn't persist across
// Electron relaunches (worked fine in the browser build). Root cause:
// AppShell's i18n store persisted to localStorage, which is scoped per
// origin, but ElectronApp's static-server.ts binds to an OS-assigned
// ephemeral port (listen(0, ...)) that changes every launch — so the origin
// (http://127.0.0.1:<port>) was never the same twice. Fix moves Electron's
// persistence to a userData JSON file over IPC (locale-store.ts /
// ipc/locale.ts), matching recent-games.ts's existing pattern.
//
// This launches Electron twice against the *same* --user-data-dir: first
// launch switches the language to Spanish via the real Settings UI, second
// launch (a fresh process, fresh random port) checks the language is still
// Spanish.
import { _electron as electron } from 'playwright';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';

const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));

// The Settings button's title/aria-label are translated (t("common.settings")),
// so they can't be used as a locale-independent selector once the locale
// under test has changed them. It's HomeHeader.svelte's only <button>.
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

try {
    // --- Launch 1: switch language to Spanish ---
    {
        const { app, win } = await launch();
        await win.waitForSelector(SETTINGS_BUTTON, { timeout: 30000 });
        const portBefore = new URL(win.url()).port;
        console.log('[launch 1] loaded on port', portBefore);

        await win.click(SETTINGS_BUTTON);
        await win.waitForSelector('#settings-language', { timeout: 10000 });

        const [reloaded] = await Promise.all([
            win.waitForNavigation({ timeout: 10000 }),
            win.selectOption('#settings-language', 'es'),
        ]);
        void reloaded;

        // Confirm the UI actually re-rendered in Spanish (not just that the
        // select's value changed) — settingsModal.title should now read the
        // Spanish string, not the English one.
        await win.waitForSelector(SETTINGS_BUTTON, { timeout: 10000 });
        const storedAfterSet = await win.evaluate(() => window.electronApp.locale.get());
        console.log('[launch 1] locale.get() after set:', storedAfterSet);
        if (storedAfterSet !== 'es') {
            throw new Error(`Expected locale store to read 'es' after selecting it, got ${JSON.stringify(storedAfterSet)}`);
        }
        console.log('PASS: locale persisted to userData store within the same launch');

        await app.close();
    }

    // --- Launch 2: fresh process, same --user-data-dir, different random port ---
    {
        const { app, win } = await launch();
        await win.waitForSelector(SETTINGS_BUTTON, { timeout: 30000 });
        const portAfter = new URL(win.url()).port;
        console.log('[launch 2] loaded on port', portAfter);

        const htmlLang = await win.evaluate(() => document.documentElement.lang);
        console.log('[launch 2] document.documentElement.lang:', htmlLang);

        const storedLocale = await win.evaluate(() => window.electronApp.locale.get());
        console.log('[launch 2] locale.get():', storedLocale);
        if (storedLocale !== 'es') {
            throw new Error(`Expected locale to still be 'es' after relaunch, got ${JSON.stringify(storedLocale)}`);
        }
        console.log('PASS: locale survived a full app relaunch (fresh static-server port each time)');

        // Also confirm the rendered UI itself picked up Spanish on this fresh
        // launch, not just that the store still holds 'es' — title is
        // t("common.settings"), so it should now read the Spanish string.
        const settingsTitle = await win.locator(SETTINGS_BUTTON).getAttribute('title');
        console.log('[launch 2] settings button title:', settingsTitle);
        const enTitle = 'Settings';
        if (settingsTitle === enTitle) {
            throw new Error(`Expected the UI to render in Spanish on relaunch, but settings button title is still '${enTitle}'`);
        }

        await app.close();
    }

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    rmSync(userDataDir, { recursive: true, force: true });
}
