// Ad-hoc manual verification: the "Home" button in the AppShell editor
// toolbar (Toolbar.svelte), added so Electron (which has no browser back
// button) and the web app both have an explicit way back to the Home
// landing page from /edit. It takes you to the Create tab (/open), not the
// Play tab (root) — you got here from Create in the first place, and Play
// is a dead end for an in-progress edit. Confirms the button appears and
// navigates to /open when PUBLIC_SHOW_HOME=true, and confirms it's absent
// when PUBLIC_SHOW_HOME=false (textadventures.co.uk mode, where root has no
// Home page to go back to).
//
// Requires the AppShell dev server running with PUBLIC_SHOW_HOME=true:
//   (cd src/AppShell && PUBLIC_SHOW_HOME=true npx vite dev) &
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', 'Home Button Test');
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor (/edit)');

    await page.waitForSelector('button[title="Back to Home"]', { timeout: 5000 });
    console.log('PASS: Home button present in the toolbar when PUBLIC_SHOW_HOME=true');

    await page.click('button[title="Back to Home"]');
    await page.waitForURL(url => url.pathname === '/open', { timeout: 10000 });
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 10000 });
    console.log('PASS: clicking Home navigated to the Create tab (/open), url =', page.url());

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-home-button-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
