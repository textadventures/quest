// Ad-hoc manual verification that the "Save to server" create button (only
// shown when PUBLIC_HAS_SERVER=true, i.e. the textadventures.co.uk build)
// gets the same storage-location caption as the OPFS/FSA buttons added in
// the same change — "Saved to your textadventures.co.uk account".
//
// Requires a dev server started with PUBLIC_HAS_SERVER=true, e.g.:
//   cd src/AppShell && PUBLIC_HAS_SERVER=true npx vite dev --port 5199
// (dev.sh's own --api-proxy flag also sets this, but needs a real
// textadventures.co.uk backend to proxy to — not needed just to check this
// caption renders, since it doesn't require the templates fetch to succeed.)
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5199';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Save to server")', { timeout: 15000 });
    console.log('PASS: "Save to server" button present (PUBLIC_HAS_SERVER=true)');

    const captionVisible = await page.isVisible('text=Saved to your textadventures.co.uk account');
    console.log('PASS: storage-location caption shown under "Save to server":', captionVisible);
    if (!captionVisible) throw new Error('Expected the server-mode caption to be visible');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-server-mode-caption-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
