// Verifies File > Publish…: modal radios, the .quest download, and the zip HTML
// target — zip export embeds the game with relative player assets (no CDN
// <base href>), includes _framework + shell files, and boots when served locally
// with the CDN blocked (issue #2234).
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

    const gameName = `Export HTML ${Date.now()}`;
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', gameName);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 60000 });
    console.log('PASS: local draft created');

    // File menu (see Toolbar.svelte) — prefer label over fragile button index.
    await page.click('button:has-text("File")');
    await page.waitForSelector('.absolute button', { timeout: 10000 });
    await page.locator('.absolute button', { hasText: 'Publish' }).click();
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });

    const dialog = page.locator('div[role="dialog"]');
    const title = await dialog.locator('h2').textContent();
    if (!title?.includes('Publish')) throw new Error(`unexpected modal title: ${title}`);
    const radios = dialog.locator('input[type="radio"]');
    const values = await radios.evaluateAll(els => els.map(el => el.value));
    if (values.join(',') !== 'quest,cdn,zip') throw new Error(`unexpected publish targets: ${values.join(',')}`);
    const questRadio = dialog.locator('input[type="radio"][value="quest"]');
    const zipRadio = dialog.locator('input[type="radio"][value="zip"]');
    if (!(await questRadio.isChecked())) throw new Error('expected .quest option checked by default');
    if (await dialog.locator('input[type="checkbox"]').count() !== 0) throw new Error('unexpected checkbox (walkthrough option should be gone)');
    if (await dialog.locator('.publish-footnote').count() !== 0) throw new Error('HTML footnote should be hidden for the .quest target');
    console.log('PASS: Publish modal offers quest/cdn/zip with .quest default');

    // Each radio should sit on the visual centre of its caption's first line: midway between
    // the centres of a capital and a lowercase letter, measured from the real baseline (a
    // zero-size inline-block probe) — the line box's own centre sits noticeably higher.
    const offsets = await dialog.locator('label').evaluateAll(labels => labels.map(label => {
        const radio = label.querySelector('input[type="radio"]').getBoundingClientRect();
        const title = label.querySelector('.font-medium');
        const probe = document.createElement('span');
        probe.style.cssText = 'display:inline-block;width:0;height:0';
        title.prepend(probe);
        const baseline = probe.getBoundingClientRect().bottom;
        probe.remove();
        const style = getComputedStyle(title);
        const ctx = document.createElement('canvas').getContext('2d');
        ctx.font = `${style.fontWeight} ${style.fontSize} ${style.fontFamily}`;
        const capHeight = ctx.measureText('H').actualBoundingBoxAscent;
        const xHeight = ctx.measureText('x').actualBoundingBoxAscent;
        return (radio.top + radio.height / 2) - (baseline - (capHeight + xHeight) / 4);
    }));
    if (offsets.some(o => Math.abs(o) > 0.75)) throw new Error(`radio not centred on caption's first line (offsets ${offsets.map(o => o.toFixed(2)).join(', ')}px; negative = too high)`);
    console.log('PASS: radios centred on caption first lines');

    const [questDownload] = await Promise.all([
        page.waitForEvent('download', { timeout: 120000 }),
        dialog.locator('button', { hasText: 'Publish' }).click(),
    ]);
    if (!questDownload.suggestedFilename().endsWith('.quest')) throw new Error(`expected *.quest, got ${questDownload.suggestedFilename()}`);
    const questEntries = unzipSync(readFileSync(await questDownload.path()));
    if (!questEntries['game.aslx']) throw new Error('.quest package missing game.aslx');
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
    console.log('PASS: .quest target downloads a package:', questDownload.suggestedFilename());

    await page.click('button:has-text("File")');
    await page.waitForSelector('.absolute button', { timeout: 10000 });
    await page.locator('.absolute button', { hasText: 'Publish' }).click();
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });

    await zipRadio.check();
    if (!(await zipRadio.isChecked())) throw new Error('zip radio did not stay checked');
    if (await dialog.locator('.publish-footnote').count() !== 1) throw new Error('HTML footnote should show for the zip target');

    const [download] = await Promise.all([
        page.waitForEvent('download', { timeout: 120000 }),
        dialog.locator('button', { hasText: 'Publish' }).click(),
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
    if (html.includes('<!--')) throw new Error('zip index.html should not carry the shell\'s dev comments');
    console.log(`PASS: zip has ${names.length} files, embedded game, no CDN base`);

    // <title> should be the game's own name, not the shared shell's "Quest Viva".
    const titleMatch = /<title>([^<]*)<\/title>/.exec(html);
    if (!titleMatch) throw new Error('zip index.html missing <title>');
    if (titleMatch[1] !== gameName) throw new Error(`expected <title>${gameName}</title>, got <title>${titleMatch[1]}</title>`);
    console.log('PASS: exported <title> matches game name');

    // Modal should close after a successful export.
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
    console.log('PASS: modal closed after export');

    // The dialog should reopen on the target this game was last published to — also after
    // reopening the game in a fresh page load. A bare reload redirects to /open (isLoaded is
    // in-memory SPA state), so reopen the draft by name, as verify-appshell-autosave.mjs does.
    for (const when of ['reopen', 'reload']) {
        if (when === 'reload') {
            await page.goto(`${baseUrl}/open`);
            await page.waitForSelector('text=Your local drafts', { timeout: 10000 });
            await page.click(`button:has-text("${gameName}.aslx")`);
            await page.waitForSelector('button[title="Preview game"]', { timeout: 60000 });
        }
        await page.click('button:has-text("File")');
        await page.waitForSelector('.absolute button', { timeout: 10000 });
        await page.locator('.absolute button', { hasText: 'Publish' }).click();
        await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
        await page.waitForFunction(() => document.querySelector('div[role="dialog"] input[value="zip"]')?.checked, null, { timeout: 5000 })
            .catch(() => { throw new Error(`expected zip target remembered after ${when}`); });
        await dialog.locator('button', { hasText: 'Close' }).click();
        await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
    }
    console.log('PASS: last-used target remembered on reopen and after reload');

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
