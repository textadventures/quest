// Ad-hoc manual verification: the Play tab's browser-build "download the
// desktop app" widget (DownloadButton.svelte / download-links.ts) — OS
// detection, primary button, other-platform links, and the "all downloads"
// fallback when the GitHub releases API call fails.
// Requires the dev server running: ./dev.sh
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

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

// ── Run 1: Windows UA — primary button + Mac/Linux as "other" ──────────────
{
    const browser = await chromium.launch();
    const page = await browser.newPage({ userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36' });
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.route('https://api.github.com/repos/textadventures/quest/releases/latest', route => route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(sampleRelease),
    }));

    try {
        await page.goto(`${baseUrl}/`);
        const primary = page.locator('a:has-text("Download for Windows")');
        await primary.waitFor({ timeout: 10000 });
        const href = await primary.getAttribute('href');
        if (href !== 'https://example.com/win.exe') throw new Error(`expected win.exe link, got ${href}`);
        console.log('PASS: primary button matches Windows UA');

        await page.locator('button:has-text("Other platforms")').click();
        const macLink = page.locator('a:has-text("Mac (Apple Silicon)")');
        await macLink.waitFor({ timeout: 5000 });
        const macHref = await macLink.getAttribute('href');
        if (macHref !== 'https://example.com/mac.dmg') throw new Error(`expected mac.dmg link, got ${macHref}`);
        console.log('PASS: other-platforms disclosure shows Mac link');

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

// ── Run 2: GitHub API failure — only the fallback link shows ───────────────
{
    const browser = await chromium.launch();
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.route('https://api.github.com/repos/textadventures/quest/releases/latest', route => route.fulfill({ status: 500 }));

    try {
        await page.goto(`${baseUrl}/`);
        await page.waitForSelector('button:has-text("Open a game file")', { timeout: 30000 });
        const allDownloads = page.locator('a:has-text("All downloads")');
        await allDownloads.waitFor({ timeout: 5000 });
        const primaryButtons = await page.locator('a:has-text("Download for")').count();
        if (primaryButtons !== 0) throw new Error('a primary download button rendered despite the API failure');
        console.log('PASS: only the "All downloads" fallback renders when the GitHub API call fails');
    } catch (err) {
        console.error('FAIL (run 2):', err.message);
        failed = true;
    } finally {
        await browser.close();
    }
}

// ── Run 3: release has no .deb — Linux label must say AppImage, not .deb ───
{
    const browser = await chromium.launch();
    const page = await browser.newPage({ userAgent: 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36' });
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    const noDebRelease = {
        tag_name: 'v6.0.0-beta.42',
        assets: [
            { name: 'Quest.Viva-6.0.0-beta.42-arm64.AppImage', browser_download_url: 'https://example.com/linux-arm64.AppImage' },
            { name: 'Quest.Viva-6.0.0-beta.42.AppImage', browser_download_url: 'https://example.com/linux.AppImage' },
        ],
    };
    await page.route('https://api.github.com/repos/textadventures/quest/releases/latest', route => route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(noDebRelease),
    }));

    try {
        await page.goto(`${baseUrl}/`);
        const primary = page.locator('a:has-text("Download for Linux")');
        await primary.waitFor({ timeout: 10000 });
        const text = await primary.textContent();
        if (text !== 'Download for Linux (.AppImage)') throw new Error(`expected label "Download for Linux (.AppImage)", got "${text}"`);
        const href = await primary.getAttribute('href');
        if (href !== 'https://example.com/linux.AppImage') throw new Error(`expected the x64 AppImage link, got ${href}`);
        console.log('PASS: Linux label reflects the AppImage fallback when no .deb is published');
    } catch (err) {
        console.error('FAIL (run 3):', err.message);
        failed = true;
    } finally {
        await browser.close();
    }
}

if (failed) process.exit(1);
console.log('ALL PASS');
