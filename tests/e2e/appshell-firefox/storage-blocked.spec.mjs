// When the browser blocks storage, the Create tab says so instead of failing with the browser's own
// error (#2908). Firefox set to block all cookies (network.cookie.cookieBehavior = 2) is a real way
// to get there: navigator.storage.getDirectory() rejects with a SecurityError, so nothing that keeps
// a local draft can work. Firefox has no File System Access API either, so the page should show the
// notice and nothing else: no drafts list, no Import button, no create form. Reading
// window.localStorage throws too, which used to stop the app starting at all.
//
// Requires the AppShell dev server running locally:
//   npx vite dev --port 5174   (in src/AppShell)
import { test } from '@playwright/test';
import { firefox } from 'playwright';

test('storage-blocked', async ({ baseURL }) => {
    const browser = await firefox.launch({ firefoxUserPrefs: { 'network.cookie.cookieBehavior': 2 } });
    const page = await browser.newPage();
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));

    function check(condition, message) {
        if (!condition) throw new Error(message);
        console.log(`PASS: ${message}`);
    }

    try {
        // The pref really does block OPFS here, or the rest of this test proves nothing.
        await page.goto(`${baseURL}/open`);
        const probe = await page.evaluate(async () => {
            try { await navigator.storage.getDirectory(); return 'ok'; } catch (e) { return e.name; }
        });
        check(probe !== 'ok', `blocking all cookies blocks OPFS in Firefox (${probe})`);

        const notice = page.getByTestId('storage-blocked');
        await notice.waitFor({ timeout: 30000 });
        const text = await notice.innerText();
        check(text.includes('This browser is blocking storage'), 'the notice says storage is blocked');
        check(text.includes(probe), "the notice gives the browser's own message as a detail");
        check(await notice.getByRole('link', { name: 'Get the desktop app' }).isVisible(), 'the notice links to the desktop app');
        check(!text.includes('open a game folder'), "the notice doesn't offer a game folder where there's no File System Access API");

        check(await page.locator('button:has-text("Import game file")').count() === 0, 'no Import game file button');
        check(await page.locator('button:has-text("Create local draft")').count() === 0, 'no Create local draft button');
        check(await page.locator('input[placeholder="Game name"]').count() === 0, 'no create form');
        check(await page.locator('text=Your local drafts').count() === 0, 'no drafts list');
        check(await page.locator('text=Open existing game').count() === 0, 'no empty "Open existing game" heading');
        check(pageErrors.length === 0, `no uncaught page errors on /open (${pageErrors.join('; ')})`);

        // Reading window.localStorage itself throws here, and the locale and theme stores used to do
        // that unguarded at startup, which left every page blank. The Play tab should still work.
        await page.goto(`${baseURL}/`);
        await page.getByRole('link', { name: 'Create' }).waitFor({ timeout: 30000 });
        await page.waitForTimeout(1000);
        check(pageErrors.length === 0, `no uncaught page errors on the Play tab (${pageErrors.join('; ')})`);
    } finally {
        await browser.close();
    }
});
