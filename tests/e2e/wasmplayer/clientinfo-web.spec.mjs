import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('clientinfo-web', async ({ baseURL }) => {
    const baseUrl = baseURL;

    const browser = await chromium.launch();
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    let capturedUrl = null;
    // The player reports how far the game got (GameProgress); nothing to check here.
    await page.route('**/api/gameprogress/**', (route) => route.fulfill({ status: 204 }));
    await page.route('**/api/game/**', async (route) => {
        capturedUrl = route.request().url();
        await route.fulfill({
            status: 404,
            contentType: 'application/json',
            body: JSON.stringify({ message: 'not found (expected — test id)' }),
        });
    });

    await page.goto(`${baseUrl}/?id=test-game-id`);
    await page.waitForTimeout(1500);

    await browser.close();

    console.log('Captured request URL:', capturedUrl);
    if (!capturedUrl) {
        console.error('FAIL: no request to api/game/* was observed');
        throw new Error('One or more checks failed (see the output above)');
    }
    const url = new URL(capturedUrl);
    const source = url.searchParams.get('source');
    const version = url.searchParams.get('version');
    const platform = url.searchParams.get('platform');
    console.log({ source, version, platform });

    if (source !== 'web') { console.error(`FAIL: expected source=web, got ${source}`); throw new Error('One or more checks failed (see the output above)'); }
    if (!version) { console.error('FAIL: expected a version param, got none'); throw new Error('One or more checks failed (see the output above)'); }
    if (platform !== null) { console.error(`FAIL: expected no platform param on plain web build, got ${platform}`); throw new Error('One or more checks failed (see the output above)'); }

    console.log('PASS');
});
