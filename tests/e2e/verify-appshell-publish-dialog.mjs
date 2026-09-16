// Verifies the Publish dialog's file summary, file list, size warning, progress labels and
// Cancel: everything the dialog shows about what a publish will include and how it's going.
// (Target choice, downloads and the HTML zip itself are covered by verify-appshell-export-html.mjs.)
// Requires AppShell + WasmPlayer: ./dev.sh (or AppShell on :5174 with player proxied).
import { chromium } from './lib/tracked-chromium.mjs';
import { unzipSync } from 'fflate';
import { readFileSync, writeFileSync, mkdtempSync, rmSync } from 'node:fs';
import { join } from 'node:path';
import { tmpdir } from 'node:os';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const tempDir = mkdtempSync(join(tmpdir(), 'publish-dialog-'));

async function createDraft(page, name) {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', name);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 60000 });
}

async function uploadAsset(page, file) {
    await page.click('button[title="Manage assets"]');
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    await page.setInputFiles('div[role="dialog"] input[type="file"]', file);
    const name = typeof file === 'string' ? file.split('/').pop() : file.name;
    await page.locator('div[role="dialog"]', { hasText: name }).waitFor({ timeout: 30000 });
    await page.click('div[role="dialog"] button:has-text("Close")');
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
}

async function openPublish(page) {
    await page.click('button:has-text("File")');
    await page.waitForSelector('.absolute button', { timeout: 10000 });
    await page.locator('.absolute button', { hasText: 'Publish' }).click();
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    return page.locator('div[role="dialog"]');
}

async function closeDialog(page, dialog) {
    await dialog.locator('button', { hasText: 'Close' }).click();
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
}

async function summaryText(dialog) {
    const summary = dialog.locator('.publish-files-summary');
    await summary.waitFor({ timeout: 10000 });
    return (await summary.textContent()).trim();
}

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 }, acceptDownloads: true });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    // --- A small game: summary, list, progress, cancel ---
    const smallName = `Publish dialog ${Date.now()}`;
    await createDraft(page, smallName);

    let dialog = await openPublish(page);
    // The file list loads asynchronously; give it time to (not) appear.
    await page.waitForTimeout(1500);
    if (await dialog.locator('.publish-files').count() !== 0) throw new Error('files box should be hidden when there are no other files');
    console.log('PASS: no files box for a game with no other files');
    await closeDialog(page, dialog);

    await uploadAsset(page, { name: 'tiny.wav', mimeType: 'audio/wav', buffer: Buffer.alloc(2500, 1) });

    dialog = await openPublish(page);
    let summary = await summaryText(dialog);
    if (!/Also includes 1 file \(2\.5 kB\)/.test(summary)) throw new Error(`unexpected summary: "${summary}"`);
    if (await dialog.locator('.publish-files li').count() !== 0) throw new Error('file list should start collapsed');
    await dialog.locator('button', { hasText: 'Show files' }).click();
    const rows = await dialog.locator('.publish-files li').allTextContents();
    if (rows.length !== 1 || !rows[0].includes('tiny.wav')) throw new Error(`unexpected file list: ${JSON.stringify(rows)}`);
    if (await dialog.locator('.publish-size-warning').count() !== 0) throw new Error('small game should not show a size warning');
    console.log('PASS: summary counts and sizes the uploaded file; list expands');

    // Record every progress label the dialog shows while publishing the .quest file.
    await page.evaluate(() => {
        window.__publishProgress = [];
        new MutationObserver(() => {
            const text = document.querySelector('.publish-progress')?.textContent?.trim();
            if (text && window.__publishProgress.at(-1) !== text) window.__publishProgress.push(text);
        }).observe(document.body, { subtree: true, childList: true, characterData: true });
    });
    const [questDownload] = await Promise.all([
        page.waitForEvent('download', { timeout: 120000 }),
        dialog.locator('button', { hasText: 'Publish' }).click(),
    ]);
    const questEntries = unzipSync(readFileSync(await questDownload.path()));
    if (!questEntries['tiny.wav']) throw new Error(`.quest missing tiny.wav: ${Object.keys(questEntries).join(', ')}`);
    const labels = await page.evaluate(() => window.__publishProgress);
    for (const expected of ['Reading files (1 of 1)…', 'Building the game file…']) {
        if (!labels.includes(expected)) throw new Error(`progress never showed "${expected}" (saw ${JSON.stringify(labels)})`);
    }
    console.log('PASS: progress labels shown while publishing:', labels.join(' → '));

    // Cancel while the zip export is still fetching the player: dialog closes, nothing downloads.
    dialog = await openPublish(page);
    await dialog.locator('input[type="radio"][value="zip"]').check();
    const downloads = [];
    page.on('download', d => downloads.push(d));
    await dialog.locator('button', { hasText: 'Publish' }).click();
    await dialog.locator('button', { hasText: 'Cancel' }).click();
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
    await page.waitForTimeout(20000);
    if (downloads.length !== 0) throw new Error(`cancelled publish still downloaded ${downloads[0].suggestedFilename()}`);
    console.log('PASS: Cancel closes the dialog and stops the download');

    // --- A game folder that's too big: warning for both kinds of target, big file highlighted ---
    await createDraft(page, `Publish dialog large ${Date.now()}`);
    const bigFile = join(tempDir, 'huge.wav');
    writeFileSync(bigFile, Buffer.alloc(51_000_000));
    await uploadAsset(page, bigFile);
    await uploadAsset(page, { name: 'small.png', mimeType: 'image/png', buffer: Buffer.alloc(100, 1) });

    dialog = await openPublish(page);
    summary = await summaryText(dialog);
    if (!/Also includes 2 files \(51 MB\)/.test(summary)) throw new Error(`unexpected summary: "${summary}"`);
    const questWarning = await dialog.locator('.publish-size-warning').textContent();
    if (!questWarning?.includes('50 MB upload limit')) throw new Error(`expected upload-limit warning, got "${questWarning}"`);
    await dialog.locator('input[type="radio"][value="zip"]').check();
    const htmlWarning = await dialog.locator('.publish-size-warning').textContent();
    if (!htmlWarning?.includes('before the game starts')) throw new Error(`expected HTML download warning, got "${htmlWarning}"`);
    // A local draft's files are its own copies, so the fix is the Assets dialog (a real folder
    // would get "move files out of <folder>" advice instead — deleting there is permanent).
    if (!htmlWarning.includes('Open Assets to remove')) throw new Error(`expected Assets advice, got "${htmlWarning}"`);
    console.log('PASS: size warning shown, worded for the selected target');

    // The HTML footnote belongs with the options, above the files box.
    const footnoteFirst = await dialog.evaluate(el => {
        const footnote = el.querySelector('.publish-footnote');
        const files = el.querySelector('.publish-files');
        return !!footnote && !!files && !!(footnote.compareDocumentPosition(files) & Node.DOCUMENT_POSITION_FOLLOWING);
    });
    if (!footnoteFirst) throw new Error('HTML footnote should come before the files box');
    console.log('PASS: HTML footnote sits above the files box');

    await dialog.locator('button', { hasText: 'Show files' }).click();
    const items = dialog.locator('.publish-files li');
    const first = await items.nth(0).textContent();
    if (!first?.includes('huge.wav')) throw new Error(`largest file should be listed first, got "${first}"`);
    if (!(await items.nth(0).evaluate(el => el.classList.contains('font-medium')))) throw new Error('large file should be highlighted');
    if (await items.nth(1).evaluate(el => el.classList.contains('font-medium'))) throw new Error('small file should not be highlighted');
    console.log('PASS: largest file listed first and highlighted');

    // Both the warning's "Assets" link and the box's Manage assets… swap the Publish dialog
    // for the Assets dialog.
    for (const link of ['.publish-warning-assets-link', '.publish-manage-assets']) {
        if (link === '.publish-manage-assets') dialog = await openPublish(page);
        await dialog.locator(link).click();
        await page.locator('div[role="dialog"] h2', { hasText: 'Assets' }).waitFor({ timeout: 10000 });
        if (await page.locator('div[role="dialog"]').count() !== 1) throw new Error(`Publish dialog should close when ${link} opens Assets`);
        if (!(await page.locator('div[role="dialog"]').textContent())?.includes('huge.wav')) throw new Error('Assets dialog should list the game\'s files');
        await page.click('div[role="dialog"] button:has-text("Close")');
        await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
    }
    console.log('PASS: warning link and Manage assets… both open the Assets dialog');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
    rmSync(tempDir, { recursive: true, force: true });
}
