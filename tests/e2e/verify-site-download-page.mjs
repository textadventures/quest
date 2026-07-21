// Ad-hoc manual verification: questviva.com's /download page
// (site/src/components/DownloadButton.astro) — OS detection, primary
// button, other-platform links, and the static "all downloads" fallback
// when the GitHub releases API call fails (or before JS runs).
// Requires the site built and served: cd site && npm run build && npx serve dist -l 4321
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:4321';

const sampleRelease = {
    tag_name: 'v6.0.0-beta.42',
    assets: [
        { name: 'Quest.Viva.Setup.6.0.0-beta.42.exe', browser_download_url: 'https://example.com/win.exe' },
        { name: 'Quest.Viva-6.0.0-beta.42-arm64.dmg', browser_download_url: 'https://example.com/mac.dmg' },
        { name: 'Quest.Viva-6.0.0-beta.42.deb', browser_download_url: 'https://example.com/linux.deb' },
        { name: 'Quest.Viva-6.0.0-beta.42.AppImage', browser_download_url: 'https://example.com/linux.AppImage' },
    ],
};

let failed = false;

// ── Run 1: Mac UA — primary is Mac, Windows/Linux listed as others ─────────
{
    const browser = await chromium.launch();
    const page = await browser.newPage({ userAgent: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15' });
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.route('https://api.github.com/repos/textadventures/quest/releases/latest', route => route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(sampleRelease),
    }));

    try {
        await page.goto(`${baseUrl}/download/`);
        const primary = page.locator('a:has-text("Download for Mac")');
        await primary.waitFor({ timeout: 10000 });
        const href = await primary.getAttribute('href');
        if (href !== 'https://example.com/mac.dmg') throw new Error(`expected mac.dmg link, got ${href}`);
        console.log('PASS: primary link matches Mac UA');

        const winLink = page.locator('a:has-text("Windows")').first();
        const winHref = await winLink.getAttribute('href');
        if (winHref !== 'https://example.com/win.exe') throw new Error(`expected win.exe link, got ${winHref}`);
        console.log('PASS: Windows listed as another platform');

        const linuxLink = page.locator('a:has-text("Linux (.deb)")');
        const linuxHref = await linuxLink.getAttribute('href');
        if (linuxHref !== 'https://example.com/linux.deb') throw new Error(`expected linux.deb link, got ${linuxHref}`);
        console.log('PASS: Linux listed as another platform, x64 .deb picked over AppImage');

        const allDownloads = page.locator('a:has-text("All downloads")');
        const allHref = await allDownloads.getAttribute('href');
        if (allHref !== 'https://github.com/textadventures/quest/releases/latest') {
            throw new Error(`expected releases-page fallback link, got ${allHref}`);
        }
        console.log('PASS: "All downloads" always links to the releases page');
    } catch (err) {
        console.error('FAIL (run 1):', err.message);
        failed = true;
    } finally {
        await browser.close();
    }
}

// ── Run 2: GitHub API failure — static fallback link survives ──────────────
{
    const browser = await chromium.launch();
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.route('https://api.github.com/repos/textadventures/quest/releases/latest', route => route.fulfill({ status: 500 }));

    try {
        await page.goto(`${baseUrl}/download/`);
        const fallback = page.locator('a:has-text("All downloads (GitHub Releases)")');
        await fallback.waitFor({ timeout: 10000 });
        const href = await fallback.getAttribute('href');
        if (href !== 'https://github.com/textadventures/quest/releases/latest') {
            throw new Error(`expected releases-page fallback link, got ${href}`);
        }
        const primaryLinks = await page.locator('a:has-text("Download for")').count();
        if (primaryLinks !== 0) throw new Error('a primary download link rendered despite the API failure');
        console.log('PASS: static fallback link survives a GitHub API failure');
    } catch (err) {
        console.error('FAIL (run 2):', err.message);
        failed = true;
    } finally {
        await browser.close();
    }
}

if (failed) process.exit(1);
console.log('ALL PASS');
