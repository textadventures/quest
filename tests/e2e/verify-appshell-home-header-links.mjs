// Verifies the Discord/GitHub links (DiscordIcon.svelte / GithubIcon.svelte)
// appear in both places they're wired up:
//   - HomeHeader.svelte, top-right, on the Play (/) and Create (/open) tabs
//     — only rendered when PUBLIC_SHOW_HOME=true, so this script needs it set
//   - Toolbar.svelte, end of the trailing button cluster, in the editor
//     (/edit) — shown unconditionally, regardless of PUBLIC_SHOW_HOME
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from 'playwright';

const baseUrl = 'http://localhost:5180';
const expectedDiscordHref = 'https://textadventures.co.uk/community/discord';
const expectedGithubHref = 'https://github.com/textadventures/quest';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1200, height: 500 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function checkLinks(label) {
    await page.waitForSelector('a[title="Join us on Discord"]', { timeout: 30000 });
    const discordHref = await page.getAttribute('a[title="Join us on Discord"]', 'href');
    const githubHref = await page.getAttribute('a[title="View on GitHub"]', 'href');
    if (discordHref !== expectedDiscordHref) throw new Error(`[${label}] unexpected Discord href: ${discordHref}`);
    if (githubHref !== expectedGithubHref) throw new Error(`[${label}] unexpected GitHub href: ${githubHref}`);
    console.log(`PASS: [${label}] Discord and GitHub links present with correct hrefs`);
}

async function run() {
    // HomeHeader — Play tab (root) and Create tab (/open)
    await page.goto(`${baseUrl}/`);
    await checkLinks('Play tab (HomeHeader)');

    await page.goto(`${baseUrl}/open`);
    await checkLinks('Create tab (HomeHeader)');

    // Toolbar — inside the editor (/edit), reached by creating a local draft
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', 'Toolbar Icons Test');
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await checkLinks('Editor toolbar (/edit)');

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-home-header-links-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
