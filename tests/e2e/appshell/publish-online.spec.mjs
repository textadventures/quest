// Verifies File > Publish… to textadventures.co.uk: the dialog starts on it, uploads the .quest
// package, opens the page where the author finishes and links to it, says when the game is
// published, remembers the choice, and explains a refused upload.
// textadventures.co.uk is stubbed here (its upload and status API, and the page the author is
// sent to), so nothing is uploaded and nothing is published.
// Requires AppShell + WasmPlayer: ./dev.sh (or AppShell on :5174 with player proxied).
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('publish-online', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const browser = await chromium.launch();

    const CLAIM_URL = 'https://textadventures.co.uk/submit/from-editor/TESTUPLOADID0000000000';
    const GAME_URL = 'https://textadventures.co.uk/games/view/abc123/publish-online';

    try {
        const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });

        // What the stubbed site does next, and what it was sent
        const site = { uploadStatus: 201, published: false, uploads: [], statusChecks: 0 };
        await ctx.route('https://textadventures.co.uk/**', async (route) => {
            const request = route.request();
            const url = new URL(request.url());
            const cors = { 'Access-Control-Allow-Origin': '*', 'Access-Control-Allow-Headers': 'Content-Type', 'Access-Control-Allow-Methods': 'GET,POST' };
            if (url.pathname === '/api/publish/uploads') {
                if (request.method() === 'OPTIONS') return route.fulfill({ status: 204, headers: cors });
                site.uploads.push({ filename: url.searchParams.get('filename'), source: url.searchParams.get('source'), body: request.postDataBuffer() });
                if (site.uploadStatus !== 201) return route.fulfill({ status: site.uploadStatus, headers: cors });
                return route.fulfill({
                    status: 201,
                    headers: cors,
                    contentType: 'application/json',
                    body: JSON.stringify({ id: 'TESTUPLOADID0000000000', claimUrl: CLAIM_URL, expires: new Date(Date.now() + 3600e3).toISOString() }),
                });
            }
            if (url.pathname === '/api/publish/uploads/TESTUPLOADID0000000000') {
                site.statusChecks++;
                const body = site.published ? { status: 'published', gameId: 'abc123', gameUrl: GAME_URL } : { status: 'pending' };
                return route.fulfill({ status: 200, headers: cors, contentType: 'application/json', body: JSON.stringify(body) });
            }
            if (url.pathname.startsWith('/submit/from-editor/')) {
                return route.fulfill({ status: 200, contentType: 'text/html', body: '<title>Stub claim page</title><p>Stub claim page</p>' });
            }
            // The home page's catalog, and anything else: not this spec's business, and never the real site
            return route.fulfill({ status: 404, headers: cors });
        });

        const page = await ctx.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        async function openPublish() {
            await page.click('button:has-text("File")');
            await page.waitForSelector('.absolute button', { timeout: 10000 });
            await page.locator('.absolute button', { hasText: 'Publish' }).click();
            await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
            return page.locator('div[role="dialog"]');
        }

        await page.goto(`${baseUrl}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Publish online ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="Preview game"]', { timeout: 60000 });

        // --- The dialog starts on textadventures.co.uk, and uploads the package ---
        let dialog = await openPublish();
        const onlineRadio = dialog.locator('input[type="radio"][value="online"]');
        if (!(await onlineRadio.isChecked())) throw new Error('expected textadventures.co.uk checked by default');
        if (await dialog.locator('.publish-footnote').count() !== 0) throw new Error('HTML footnote should be hidden for textadventures.co.uk');

        const [claimPage] = await Promise.all([
            ctx.waitForEvent('page', { timeout: 120000 }),
            dialog.locator('button', { hasText: 'Publish' }).click(),
        ]);
        await claimPage.waitForLoadState();
        if (claimPage.url() !== CLAIM_URL) throw new Error(`expected the claim page to open, got ${claimPage.url()}`);
        await claimPage.close();

        if (site.uploads.length !== 1) throw new Error(`expected one upload, got ${site.uploads.length}`);
        const upload = site.uploads[0];
        if (!upload.filename?.endsWith('.quest')) throw new Error(`expected a .quest filename, got ${upload.filename}`);
        if (upload.source !== 'web') throw new Error(`expected source=web, got ${upload.source}`);
        if (upload.body?.subarray(0, 2).toString() !== 'PK') throw new Error('expected the upload to be a .quest package (a zip)');
        console.log(`PASS: uploads ${upload.filename} (${upload.body.length} bytes) and opens the claim page`);

        // --- It links to the claim page, and waits ---
        const continueLink = dialog.locator('.publish-continue');
        await continueLink.waitFor({ timeout: 10000 });
        if (await continueLink.getAttribute('href') !== CLAIM_URL) throw new Error(`unexpected Continue link: ${await continueLink.getAttribute('href')}`);
        const title = await dialog.locator('h2').textContent();
        if (!title?.includes('textadventures.co.uk')) throw new Error(`unexpected title while waiting: ${title}`);
        if (await dialog.locator('input[type="radio"]').count() !== 0) throw new Error('the options should be gone once the game is uploaded');
        await page.waitForFunction(() => true, null, { timeout: 1000 });
        if (await dialog.locator('.publish-published').count() !== 0) throw new Error('should not say published before the site does');
        console.log('PASS: links to the claim page and waits');

        // --- It says when the game is published ---
        site.published = true;
        await dialog.locator('.publish-published').waitFor({ timeout: 15000 });
        const viewGame = dialog.locator('.publish-view-game');
        if (await viewGame.getAttribute('href') !== GAME_URL) throw new Error(`unexpected game link: ${await viewGame.getAttribute('href')}`);
        if (site.statusChecks < 1) throw new Error('expected the status to have been asked for');
        await dialog.locator('button', { hasText: 'Close' }).click();
        await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
        console.log('PASS: says the game is published, with its link');

        // --- The choice is remembered ---
        dialog = await openPublish();
        await dialog.locator('input[type="radio"][value="zip"]').check();
        await dialog.locator('button', { hasText: 'Close' }).click();
        await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
        dialog = await openPublish();
        await page.waitForFunction(() => document.querySelector('div[role="dialog"] input[value="online"]')?.checked, null, { timeout: 5000 })
            .catch(() => { throw new Error('expected textadventures.co.uk remembered after publishing to it'); });
        console.log('PASS: textadventures.co.uk remembered as the last target');

        // --- A refused upload says why, and Back returns to the options ---
        site.uploadStatus = 413;
        await dialog.locator('button', { hasText: 'Publish' }).click();
        const error = dialog.locator('.publish-upload-error');
        await error.waitFor({ timeout: 120000 });
        if (!(await error.textContent())?.includes('50 MB')) throw new Error(`expected the size limit explained, got "${await error.textContent()}"`);
        await dialog.locator('button', { hasText: 'Back' }).click();
        if (!(await dialog.locator('input[type="radio"][value="online"]').isChecked())) throw new Error('Back should return to the options');
        console.log('PASS: a refused upload is explained, and Back returns to the options');

        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
