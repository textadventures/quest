// Ad-hoc manual verification: the update-check banner never renders on a
// non-Electron (web) build, even if the server hypothetically returned a
// non-null update field — belt-and-braces alongside the server-side gate
// (ApiController.Catalog only computes Update for source=electron).
// Requires the dev server running: ./dev.sh
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

const update = { latestVersion: 'v6.0.0-beta.99', url: 'https://github.com/textadventures/quest/releases/latest' };
await page.route('**/api/Catalog**', route => route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ categories: [], update }),
}));

await page.goto(`${baseUrl}/`);
await page.waitForSelector('button:has-text("Open a game file")', { timeout: 30000 });
await page.waitForTimeout(1000);

const bannerCount = await page.locator('text=is available').count();
await browser.close();

if (bannerCount !== 0) {
    console.error('FAIL: banner rendered on a non-Electron build even though the server (hypothetically) returned update info');
    process.exit(1);
}
console.log('PASS: no banner on web build, even with a non-null update in the response (client-side isElectronApp guard)');
