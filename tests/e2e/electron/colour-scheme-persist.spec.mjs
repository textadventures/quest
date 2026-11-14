// Checks that the player's light or dark choice (v2's toolbar menu, scheme.ts) survives an
// Electron relaunch. In a browser it's kept in localStorage, but the desktop app's static server
// listens on a new port each launch, so its origin (and localStorage) is never the same twice.
// There, the choice goes through player-preload.ts's electronPlayerSettings to a userData file
// (player-settings-store.ts) instead.
//
// Launches Electron twice with the same --user-data-dir, playing fixtures/v2-theme-test.aslx
// (610, so v2) from disk each time: the first chooses Dark, the second checks the game opens
// dark from the stored choice, and that the bridge stores nothing but the three choices.
import { test } from '@playwright/test';
import { _electron as electron } from 'playwright';
import { mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createRequire } from 'node:module';
import { removeTempDirs } from '../lib/electron-cleanup.mjs';

test('colour-scheme-persist', async () => {
    const electronAppDir = join(import.meta.dirname, '..', '..', '..', 'src', 'ElectronApp');
    const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');

    const userDataDir = mkdtempSync(join(tmpdir(), 'quest-electron-userdata-'));
    const gameDir = mkdtempSync(join(tmpdir(), 'quest-electron-scheme-'));
    const aslxPath = join(gameDir, 'v2-theme-test.aslx');
    writeFileSync(aslxPath, readFileSync(join(import.meta.dirname, '..', 'fixtures', 'v2-theme-test.aslx'), 'utf8'));

    async function playGame() {
        const app = await electron.launch({
            executablePath: electronExecutablePath,
            args: [electronAppDir, `--user-data-dir=${userDataDir}`],
        });
        const win = await app.firstWindow();
        await app.evaluate(({ dialog }, fp) => {
            dialog.showOpenDialog = async () => ({ canceled: false, filePaths: [fp] });
        }, aslxPath);
        await win.waitForSelector('button:has-text("Open a game file…")', { timeout: 30000 });
        const [player] = await Promise.all([
            app.waitForEvent('window'),
            win.click('button:has-text("Open a game file…")'),
        ]);
        const errors = [];
        player.on('pageerror', (err) => errors.push(err.message));
        await player.waitForFunction(() => window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('A hall'), null, { timeout: 30000 });
        return { app, player, errors, port: new URL(player.url()).port };
    }

    const scheme = (player) => player.evaluate(() => document.querySelector('qv-player').getAttribute('data-qv-scheme'));

    try {
        let firstPort;
        {
            const { app, player, errors, port } = await playGame();
            firstPort = port;
            if (await scheme(player) !== 'light') throw new Error(`a first launch on a light system isn't light (${await scheme(player)})`);
            await player.locator('.qv-scheme-button').click();
            await player.getByRole('menuitemradio', { name: 'Dark' }).click();
            if (await scheme(player) !== 'dark') throw new Error('choosing Dark didn\'t make the game dark');
            if (await player.evaluate(() => localStorage.getItem('questviva-colour-scheme')) !== null) {
                throw new Error('the choice went to localStorage rather than the app\'s settings');
            }
            if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
            console.log('PASS: choosing Dark in the desktop app keeps it out of localStorage');
            await app.close();
        }
        {
            const { app, player, port } = await playGame();
            if (port === firstPort) throw new Error(`the second launch reused port ${port}, so this checks nothing`);
            if (await scheme(player) !== 'dark') throw new Error(`after a relaunch (port ${firstPort} → ${port}) the game isn't dark`);
            const stored = await player.evaluate(() => window.electronPlayerSettings.getColourScheme());
            if (stored !== 'dark') throw new Error(`the stored choice is ${JSON.stringify(stored)}`);
            console.log(`PASS: after a relaunch (port ${firstPort} → ${port}) the game opens dark`);

            // A game's own script can reach the bridge, so it stores only the three choices
            await player.evaluate(() => window.electronPlayerSettings.setColourScheme('<script>'));
            const after = await player.evaluate(() => window.electronPlayerSettings.getColourScheme());
            if (after !== 'dark') throw new Error(`the bridge stored something other than a choice: ${JSON.stringify(after)}`);
            console.log('PASS: the bridge ignores anything but light, dark and system');
            await app.close();
        }
        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        removeTempDirs(userDataDir, gameDir);
    }
});
