// When the browser blocks storage, the Create tab says so instead of failing with the browser's own
// error (#2908). Chromium can't be set to block OPFS the way Firefox can (see
// appshell-firefox/storage-blocked.spec.mjs), so navigator.storage.getDirectory is stubbed to
// reject the way it does there. Chromium has the File System Access API, so here the notice should
// say a game folder still works, and Open game folder and Save to folder… should stay while the
// local-draft controls go.
//
// Requires the AppShell dev server running locally:
//   npx vite dev --port 5174   (in src/AppShell)
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('storage-blocked', async ({ baseURL }) => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));
    await page.addInitScript(() => {
        StorageManager.prototype.getDirectory = () => Promise.reject(new DOMException('The operation is insecure.', 'SecurityError'));
    });

    function check(condition, message) {
        if (!condition) throw new Error(message);
        console.log(`PASS: ${message}`);
    }

    try {
        await page.goto(`${baseURL}/open`);

        const notice = page.getByTestId('storage-blocked');
        await notice.waitFor({ timeout: 30000 });
        const text = await notice.innerText();
        check(text.includes('This browser is blocking storage'), 'the notice says storage is blocked');
        check(text.includes('SecurityError: The operation is insecure.'), "the notice gives the browser's own message as a detail");
        check(text.includes('open a game folder'), 'the notice says a game folder still works');

        check(await page.locator('button:has-text("Import game file")').count() === 0, 'no Import game file button');
        check(await page.locator('text=Your local drafts').count() === 0, 'no drafts list');
        check(await page.locator('button:has-text("Open game folder")').isVisible(), 'Open game folder is still offered');

        await page.fill('input[placeholder="Game name"]', 'Blocked Storage Test');
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        check(await page.locator('button:has-text("Create local draft")').count() === 0, 'no Create local draft button');
        check(await page.locator('button:has-text("Save to folder…")').isVisible(), 'Save to folder… is still offered');

        // Enter in the name box used to go straight to a local draft, and fail with the browser's error.
        // It now goes to the folder picker instead; nothing should land in the form's error.
        await page.evaluate(() => {
            window.showDirectoryPicker = () => {
                window.__pickerCalled = true;
                return Promise.reject(new DOMException('The user aborted a request.', 'AbortError'));
            };
        });
        await page.press('input[placeholder="Game name"]', 'Enter');
        await page.waitForFunction(() => window.__pickerCalled === true, null, { timeout: 10000 });
        console.log('PASS: Enter in the name box opens the folder picker');
        await page.locator('button:has-text("Save to folder…")').waitFor({ timeout: 10000 });
        check(await page.locator('text=SecurityError').count() === 1, "the browser's message appears only in the notice, not as a form error");
        check(pageErrors.length === 0, `no uncaught page errors (${pageErrors.join('; ')})`);
    } finally {
        await browser.close();
    }
});
