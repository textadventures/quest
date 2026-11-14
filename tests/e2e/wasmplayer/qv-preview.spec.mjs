// `qv preview` (src/Cli/PreviewServer.cs + wasm-player.js's ?source=qv boot branch): serves an
// author's game folder in WasmPlayer. Checks, in a real browser:
//   1. the game boots from the folder, including a library of the author's own beside it
//      (<include ref="mylib.aslx"/>), which a plain ?url= load can't stage;
//   2. its picture and its <javascript src> load from the folder (resourceRoot);
//   3. Debug is available, as the game is an .aslx being worked on;
//   4. editing the game file reloads the page with the change.
//
// Runs the qv built in this checkout (dotnet build src/Cli; override with QV="..."), serving the
// WasmPlayer bundle in QV_PLAYER_DIR (default: the Release AppBundle, as the e2e job publishes it).
import { test } from '@playwright/test';
import { chromium } from 'playwright';
import { spawn } from 'node:child_process';
import { mkdtempSync, writeFileSync, rmSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';

test('qv-preview', async () => {
    const repo = resolve(import.meta.dirname, '..', '..', '..');
    const playerDir = resolve(process.env.QV_PLAYER_DIR
        || join(repo, 'src/WasmPlayer/bin/Release/net11.0/browser-wasm/AppBundle'));
    const qv = process.env.QV?.split(' ')
        ?? ['dotnet', join(repo, 'src/Cli/bin/Debug/net11.0/qv.dll')];

    const game = (message) => `<asl version="580">
      <include ref="English.aslx" />
      <include ref="Core.aslx" />
      <include ref="mylib.aslx" />
      <game name="qv preview test">
        <gameid>4d1b0d5e-7a7e-4a3c-9a55-6f0e2a1c0001</gameid>
        <start type="script">
          msg ("${message}")
          Greet
          picture ("dot.png")
        </start>
      </game>
      <javascript src="flag.js" />
      <object name="room">
        <object name="player" />
      </object>
    </asl>
    `;

    // A 1x1 PNG.
    const dot = Buffer.from(
        'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==',
        'base64');

    const folder = mkdtempSync(join(tmpdir(), 'qv-preview-'));
    let server = null;
    let browser = null;

    try {
        if (!existsSync(join(playerDir, 'index.html'))) {
            throw new Error(`No WasmPlayer bundle at ${playerDir}: publish src/WasmPlayer first`);
        }

        writeFileSync(join(folder, 'game.aslx'), game('Version one.'));
        writeFileSync(join(folder, 'mylib.aslx'),
            '<library>\n  <function name="Greet">\n    msg ("Hello from my library.")\n  </function>\n</library>\n');
        writeFileSync(join(folder, 'dot.png'), dot);
        writeFileSync(join(folder, 'flag.js'), 'window.qvPreviewScriptLoaded = true;\n');

        server = spawn(qv[0], [...qv.slice(1), 'preview', join(folder, 'game.aslx'), '--player', playerDir],
            { stdio: ['ignore', 'pipe', 'pipe'] });
        server.stderr.on('data', d => process.stderr.write(`[qv] ${d}`));
        const url = await new Promise((resolveUrl, reject) => {
            let output = '';
            const timeout = setTimeout(() => reject(new Error(`qv preview printed no URL: ${output}`)), 60000);
            server.stdout.on('data', d => {
                output += d;
                const match = output.match(/Open (http:\/\/127\.0\.0\.1:\d+\/\?source=qv\S*)/);
                if (match) {
                    clearTimeout(timeout);
                    resolveUrl(match[1]);
                }
            });
            server.on('exit', code => reject(new Error(`qv preview exited (${code}): ${output}`)));
        });
        console.log(`qv preview at ${url}`);

        browser = await chromium.launch();
        const page = await browser.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        const waitForText = (text) => page.waitForFunction(
            t => document.body.innerText.includes(t), text, { timeout: 60000 });

        await page.goto(url);
        await waitForText('Version one.');
        await waitForText('Hello from my library.');
        console.log('PASS: the game boots from its folder, with its own library');

        await page.waitForFunction(() => [...document.images].some(i => i.src.includes('game/dot.png') && i.naturalWidth === 1),
            null, { timeout: 15000 });
        console.log('PASS: its picture loads from the folder');

        await page.waitForFunction(() => window.qvPreviewScriptLoaded === true, null, { timeout: 15000 });
        console.log('PASS: its <javascript src> loads from the folder');

        const debugVisible = await page.$eval('#cmdDebug', el => getComputedStyle(el).display !== 'none');
        if (!debugVisible) throw new Error('The Debug button is hidden for an .aslx preview');
        console.log('PASS: Debug is available');

        const reloaded = page.waitForEvent('load', { timeout: 30000 });
        writeFileSync(join(folder, 'game.aslx'), game('Version two.'));
        await reloaded;
        await waitForText('Version two.');
        if (await page.evaluate(() => document.body.innerText.includes('Version one.'))) {
            throw new Error('The old version is still showing after the reload');
        }
        console.log('PASS: editing the game reloads it');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser?.close();
        server?.kill();
        rmSync(folder, { recursive: true, force: true });
    }
});
