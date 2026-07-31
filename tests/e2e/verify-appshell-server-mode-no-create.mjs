// Ad-hoc manual verification that textadventures.co.uk's /open (PUBLIC_HAS_SERVER=true)
// no longer offers game creation at all — it should only ever open existing
// games (via a ?game= link from the /quest dashboard), with a link out to
// play.questviva.com/open for anyone wanting to create a new one.
//
// Requires a dev server started with PUBLIC_HAS_SERVER=true, e.g.:
//   cd src/AppShell && PUBLIC_HAS_SERVER=true npx vite dev --port 5199
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5199';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('text=This editor only opens games you already have here.', { timeout: 15000 });
    console.log('PASS: "existing games only" message shown (PUBLIC_HAS_SERVER=true)');

    const createButtonCount = await page.locator('button:has-text("Save to server"), button:has-text("Create local draft")').count();
    if (createButtonCount !== 0) throw new Error(`Expected no create-game buttons, found ${createButtonCount}`);
    console.log('PASS: no create-game button present');

    const link = page.locator('a[href="https://play.questviva.com/open"]');
    const linkCount = await link.count();
    if (linkCount !== 1) throw new Error(`Expected exactly one link to play.questviva.com/open, found ${linkCount}`);
    console.log('PASS: link to play.questviva.com/open present');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-server-mode-no-create-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
