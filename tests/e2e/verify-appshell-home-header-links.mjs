// Verifies the Documentation/Discord/GitHub links appear in both places
// they're wired up:
//   - HomeHeader.svelte, top-right, on the Play (/) and Create (/open) tabs,
//     as plain <a> elements — only rendered when PUBLIC_SHOW_HOME=true, so
//     this script needs it set
//   - Toolbar.svelte, in the editor (/edit), as items inside the overflow
//     ("...") DropdownMenu — shown unconditionally, regardless of
//     PUBLIC_SHOW_HOME. These are <button role="menuitem"> elements that call
//     window.open(), NOT anchors, so they're checked by intercepting the popup
//     rather than by reading an href.
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5180';
const expectedDocsHref = 'https://questviva.com/intro/';
const expectedDiscordHref = 'https://textadventures.co.uk/community/discord';
const expectedGithubHref = 'https://github.com/textadventures/quest';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1200, height: 500 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function checkHeaderLinks(label) {
    await page.waitForSelector('a[title="Join us on Discord"]', { timeout: 30000 });
    const expected = {
        Documentation: [expectedDocsHref, 'a[title="Documentation"]'],
        Discord: [expectedDiscordHref, 'a[title="Join us on Discord"]'],
        GitHub: [expectedGithubHref, 'a[title="View on GitHub"]'],
    };
    for (const [name, [href, selector]] of Object.entries(expected)) {
        if (await page.locator(selector).count() !== 1) {
            throw new Error(`[${label}] expected exactly one ${name} link matching ${selector}`);
        }
        const actual = await page.getAttribute(selector, 'href');
        if (actual !== href) throw new Error(`[${label}] unexpected ${name} href: ${actual} (expected ${href})`);
    }
    console.log(`PASS: [${label}] Documentation, Discord and GitHub links present with correct hrefs`);
}

// The toolbar items call openLink() -> window.open(), so the URL only shows up
// as a popup request. Asserting on it is the only way to check the target;
// there is no href to read.
async function checkToolbarMenuItem(label, itemLabel, expectedUrl) {
    await page.click('button[aria-label="More"]');
    const menuItem = page.locator(`[role="menu"] button[role="menuitem"]:has-text("${itemLabel}")`);
    if (await menuItem.count() !== 1) {
        throw new Error(`[${label}] expected exactly one "${itemLabel}" item in the overflow menu`);
    }
    const [popup] = await Promise.all([
        page.waitForEvent('popup', { timeout: 10000 }),
        menuItem.click(),
    ]);
    const actual = popup.url();
    await popup.close();
    if (actual !== expectedUrl) {
        throw new Error(`[${label}] "${itemLabel}" opened ${actual} (expected ${expectedUrl})`);
    }
    console.log(`PASS: [${label}] "${itemLabel}" opens ${expectedUrl}`);
}

async function run() {
    // HomeHeader — Play tab (root) and Create tab (/open)
    await page.goto(`${baseUrl}/`);
    await checkHeaderLinks('Play tab (HomeHeader)');

    await page.goto(`${baseUrl}/open`);
    await checkHeaderLinks('Create tab (HomeHeader)');

    // Toolbar — inside the editor (/edit), reached by creating a local draft.
    // waitForURL matters: without it the assertions below race the SPA
    // navigation and can run against /open's HomeHeader instead.
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', 'Toolbar Icons Test');
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForURL('**/edit', { timeout: 30000 });
    await page.waitForSelector('button[aria-label="More"]', { timeout: 30000 });

    if (await page.locator('a[title="Join us on Discord"]').count() !== 0) {
        throw new Error('[Editor toolbar (/edit)] HomeHeader is unexpectedly rendered here — the checks below would be testing the wrong surface');
    }

    await checkToolbarMenuItem('Editor toolbar (/edit)', 'Documentation', expectedDocsHref);
    await checkToolbarMenuItem('Editor toolbar (/edit)', 'Discord', expectedDiscordHref);
    await checkToolbarMenuItem('Editor toolbar (/edit)', 'GitHub', expectedGithubHref);

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
