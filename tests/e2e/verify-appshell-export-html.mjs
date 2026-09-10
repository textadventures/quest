// Verifies File > Export as HTML…: modal radios, zip export embeds the game with
// relative player assets (no CDN <base href>), includes _framework + shell files,
// and boots when served locally with the CDN blocked (issue #2234).
// Requires AppShell + WasmPlayer: ./dev.sh (or AppShell on :5174 with player proxied).
import { chromium } from './lib/tracked-chromium.mjs';
import { unzipSync } from 'fflate';
import { readFileSync, mkdirSync, writeFileSync, rmSync } from 'node:fs';
import { join } from 'node:path';
import { createServer } from 'node:http';
import { tmpdir } from 'node:os';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
let outDir = null;
let server = null;

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 }, acceptDownloads: true });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Export HTML ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 60000 });
    console.log('PASS: local draft created');

    // File menu (see Toolbar.svelte) — prefer label over fragile button index.
    await page.click('button:has-text("File")');
    await page.waitForSelector('.absolute button', { timeout: 10000 });
    await page.locator('.absolute button', { hasText: 'Export as HTML' }).click();
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });

    const dialog = page.locator('div[role="dialog"]');
    const title = await dialog.locator('h2').textContent();
    if (!title?.includes('Export as HTML')) throw new Error(`unexpected modal title: ${title}`);
    const cdnRadio = dialog.locator('input[type="radio"][value="cdn"]');
    const zipRadio = dialog.locator('input[type="radio"][value="zip"]');
    if (!(await cdnRadio.isChecked())) throw new Error('expected CDN option checked by default');
    console.log('PASS: Export as HTML modal opens with small-HTML default');

    await zipRadio.check();
    if (!(await zipRadio.isChecked())) throw new Error('zip radio did not stay checked');

    const [download] = await Promise.all([
        page.waitForEvent('download', { timeout: 120000 }),
        dialog.locator('button', { hasText: 'Export' }).click(),
    ]);
    const suggested = download.suggestedFilename();
    if (!suggested.endsWith('-html.zip')) throw new Error(`expected *-html.zip, got ${suggested}`);
    const zipPath = await download.path();
    if (!zipPath) throw new Error('download path missing');
    console.log('PASS: zip download started:', suggested);

    const entries = unzipSync(readFileSync(zipPath));
    const names = Object.keys(entries);
    if (!names.includes('index.html')) throw new Error('zip missing index.html');
    if (!names.includes('wasm-player.js')) throw new Error('zip missing wasm-player.js');
    if (!names.includes('_framework/dotnet.js')) throw new Error('zip missing _framework/dotnet.js');
    if (!names.includes('_framework/dotnet.native.wasm')) throw new Error('zip missing dotnet.native.wasm');
    if (names.includes('quest-config.js')) throw new Error('zip should not include unused quest-config.js');
    // Debug /player/ builds list PDBs in the boot manifest with SRI hashes — those must be
    // present or the runtime refuses to start. Release builds omit them from the manifest.

    const html = new TextDecoder().decode(entries['index.html']);
    if (html.includes('cdn.jsdelivr.net')) throw new Error('zip index.html must not point at the CDN');
    if (html.includes('<base href=')) throw new Error('zip index.html must not set <base href>');
    if (!html.includes('QuestVivaEmbeddedGame')) throw new Error('zip index.html missing embedded game');
    if (!/\bqv-booting\b/.test(html)) throw new Error('zip index.html missing qv-booting');
    console.log(`PASS: zip has ${names.length} files, embedded game, no CDN base`);

    // Modal should close after a successful export.
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
    console.log('PASS: modal closed after export');

    outDir = join(tmpdir(), `export-html-offline-${Date.now()}`);
    mkdirSync(outDir);
    for (const [name, data] of Object.entries(entries)) {
        const dest = join(outDir, name);
        mkdirSync(join(dest, '..'), { recursive: true });
        writeFileSync(dest, data);
    }

    const mime = {
        '.html': 'text/html', '.htm': 'text/html', '.js': 'application/javascript',
        '.css': 'text/css', '.wasm': 'application/wasm', '.svg': 'image/svg+xml',
        '.png': 'image/png', '.dat': 'application/octet-stream', '.json': 'application/json',
    };
    server = createServer((req, res) => {
        const url = decodeURIComponent((req.url || '/').split('?')[0]);
        const file = join(outDir, url === '/' ? 'index.html' : url.slice(1));
        try {
            const body = readFileSync(file);
            const ext = file.slice(file.lastIndexOf('.'));
            res.writeHead(200, { 'Content-Type': mime[ext] || 'application/octet-stream' });
            res.end(body);
        } catch {
            res.writeHead(404);
            res.end('missing ' + url);
        }
    });
    await new Promise(r => server.listen(0, '127.0.0.1', r));
    const port = server.address().port;

    const page2 = await ctx.newPage();
    await page2.route('**/*', route => {
        const u = route.request().url();
        if (u.includes('jsdelivr') || u.includes('unpkg.com')) return route.abort();
        return route.continue();
    });
    await page2.goto(`http://127.0.0.1:${port}/`, { waitUntil: 'domcontentloaded' });
    const bootErrors = [];
    page2.on('console', msg => { if (msg.type() === 'error') bootErrors.push(msg.text()); });
    page2.on('pageerror', err => bootErrors.push(String(err)));
    page2.on('requestfailed', req => bootErrors.push(`fail ${req.url()} ${req.failure()?.errorText}`));
    try {
        await page2.waitForSelector('#txtCommand, #divOutput, .compassbutton', { timeout: 120000 });
    } catch (err) {
        const body = await page2.evaluate(() => document.body?.innerText?.slice(0, 2000) ?? '');
        console.log('boot body:', body);
        console.log('boot errors:', bootErrors.slice(0, 30));
        throw err;
    }
    console.log('PASS: zip export boots locally with CDN blocked');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
    if (server) await new Promise(r => server.close(r));
    if (outDir) rmSync(outDir, { recursive: true, force: true });
}
