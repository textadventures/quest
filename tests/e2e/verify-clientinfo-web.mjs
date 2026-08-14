import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

let capturedUrl = null;
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
    process.exit(1);
}
const url = new URL(capturedUrl);
const source = url.searchParams.get('source');
const version = url.searchParams.get('version');
const platform = url.searchParams.get('platform');
console.log({ source, version, platform });

if (source !== 'web') { console.error(`FAIL: expected source=web, got ${source}`); process.exit(1); }
if (!version) { console.error('FAIL: expected a version param, got none'); process.exit(1); }
if (platform !== null) { console.error(`FAIL: expected no platform param on plain web build, got ${platform}`); process.exit(1); }

console.log('PASS');
