// When the browser blocks IndexedDB (a Firefox private window, all site data blocked, a privacy
// add-on), the Save dialog still opens and says saving here isn't available, instead of failing to
// open at all (#2908). Save to file and Load from file don't use the browser's storage, so they
// stay; the Save form, which would write to IndexedDB, goes. indexedDB.open is stubbed to throw the
// way Firefox's does when storage is blocked.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('save-storage-blocked', async ({ baseURL }) => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));
    await page.addInitScript(() => {
        IDBFactory.prototype.open = () => { throw new DOMException('The operation is insecure.', 'SecurityError'); };
    });

    function check(condition, message) {
        if (!condition) throw new Error(message);
        console.log(`PASS: ${message}`);
    }

    try {
        await page.goto(`${baseURL}/?url=/examples/simple.aslx`);
        // The boot-time "Continue your game?" check lists saves too; it should carry on into the game.
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 30000 });

        await page.evaluate(() => WebPlayer.onSaveClick());
        await page.locator('#qv-saves[open]').waitFor({ timeout: 5000 });

        const error = page.locator('#qv-saves-error');
        check(await error.isVisible(), 'the dialog shows a message');
        check((await error.innerText()).includes("Saving here isn't available"), "the message says saving here isn't available");
        check(!await page.locator('#qv-saves-save-form').isVisible(), 'the Save form is hidden');
        check(await page.locator('#qv-saves-list .qv-saves-empty').count() === 0, 'the list doesn\'t also say "No saved games yet."');
        check(await page.locator('#qv-saves-download').isVisible(), 'Save to file is still offered');
        check(await page.locator('#qv-saves-upload-btn').isVisible(), 'Load from file is still offered');
        check(pageErrors.length === 0, `no uncaught page errors (${pageErrors.join('; ')})`);
    } finally {
        await browser.close();
    }
});
