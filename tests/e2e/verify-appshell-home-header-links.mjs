// Verifies the Discord/GitHub links added to HomeHeader.svelte: present with
// correct hrefs on both the Play and Create tabs (PUBLIC_SHOW_HOME=true).
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from 'playwright';

const baseUrl = 'http://localhost:5180';
const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1000, height: 500 } });

for (const path of ['/', '/open']) {
    await page.goto(`${baseUrl}${path}`);
    await page.waitForSelector('a[title="Join us on Discord"]');

    const discordHref = await page.getAttribute('a[title="Join us on Discord"]', 'href');
    const githubHref = await page.getAttribute('a[title="View on GitHub"]', 'href');

    if (discordHref !== 'https://textadventures.co.uk/community/discord') {
        throw new Error(`[${path}] unexpected Discord href: ${discordHref}`);
    }
    if (githubHref !== 'https://github.com/textadventures/quest') {
        throw new Error(`[${path}] unexpected GitHub href: ${githubHref}`);
    }
    console.log(`[${path}] OK — Discord and GitHub links present with correct hrefs`);
}

await browser.close();
