// Ad-hoc manual verification: questviva.com's /download page
// (site/src/components/DownloadButton.astro) — OS detection, primary
// button, other-platform links, and the static "all downloads" fallback
// when the GitHub releases API call fails (or before JS runs).
// Requires the site built and served: cd site && npm run build && npx serve dist -l 4321
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:4321';

const sampleRelease = {
    tag_name: 'v6.0.0-beta.42',
    published_at: '2026-07-19T07:30:55Z',
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

        // Regression: the download links are created at runtime by the
        // component's <script> (document.createElement), not present in the
        // .astro template — Astro's per-component CSS scoping only tags
        // template elements with its build-time hash class, so a scoped
        // style block would silently never match these and every link would
        // render identically (which is what "why does it list every
        // platform with no visual hierarchy" looked like before the fix).
        const primaryBg = await primary.evaluate(el => getComputedStyle(el).backgroundColor);
        if (primaryBg === 'rgba(0, 0, 0, 0)' || primaryBg === 'transparent') {
            throw new Error(`primary link has no background styling (got ${primaryBg}) — component CSS isn't applying to runtime-created links`);
        }
        console.log('PASS: primary link is visually styled (component CSS applies to runtime-created elements)');

        const versionLine = page.locator('text=v6.0.0-beta.42 · released');
        await versionLine.waitFor({ timeout: 5000 });
        console.log('PASS: version and release date shown');

        const winLink = page.locator('a:has-text("Windows")').first();
        const winHref = await winLink.getAttribute('href');
        if (winHref !== 'https://example.com/win.exe') throw new Error(`expected win.exe link, got ${winHref}`);
        console.log('PASS: Windows listed as another platform');

        // Both Linux package formats are listed (not one silently dropped in
        // favor of the other) — Linux users get to pick.
        const linuxDebLink = page.locator('a:has-text("Linux (.deb)")');
        const linuxDebHref = await linuxDebLink.getAttribute('href');
        if (linuxDebHref !== 'https://example.com/linux.deb') throw new Error(`expected linux.deb link, got ${linuxDebHref}`);
        const linuxAppImageLink = page.locator('a:has-text("Linux (.AppImage)")');
        const linuxAppImageHref = await linuxAppImageLink.getAttribute('href');
        if (linuxAppImageHref !== 'https://example.com/linux.AppImage') throw new Error(`expected linux.AppImage link, got ${linuxAppImageHref}`);
        console.log('PASS: both Linux (.deb) and Linux (.AppImage) listed as other-platform options');

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

// ── Run 3: Linux UA, both formats published — .deb is primary, AppImage still listed ──
{
    const browser = await chromium.launch();
    const page = await browser.newPage({ userAgent: 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36' });
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.route('https://api.github.com/repos/textadventures/quest/releases/latest', route => route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(sampleRelease),
    }));

    try {
        await page.goto(`${baseUrl}/download/`);
        const primary = page.locator('a:has-text("Download for Linux")');
        await primary.waitFor({ timeout: 10000 });
        const text = await primary.textContent();
        if (text !== 'Download for Linux (.deb)') throw new Error(`expected primary label "Download for Linux (.deb)", got "${text}"`);
        const href = await primary.getAttribute('href');
        if (href !== 'https://example.com/linux.deb') throw new Error(`expected linux.deb link, got ${href}`);

        const appImageLink = page.locator('a:has-text("Linux (.AppImage)")');
        const appImageHref = await appImageLink.getAttribute('href');
        if (appImageHref !== 'https://example.com/linux.AppImage') throw new Error(`expected linux.AppImage link, got ${appImageHref}`);
        console.log('PASS: Linux UA gets .deb as the primary button, AppImage still offered as another option');
    } catch (err) {
        console.error('FAIL (run 3):', err.message);
        failed = true;
    } finally {
        await browser.close();
    }
}

// ── Run 4: release has no .deb — Linux label must say AppImage, not .deb ───
{
    const browser = await chromium.launch();
    const page = await browser.newPage();
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
        await page.goto(`${baseUrl}/download/`);
        const linuxLink = page.locator('a:has-text("Linux")');
        await linuxLink.waitFor({ timeout: 10000 });
        const text = await linuxLink.textContent();
        if (text !== 'Linux (.AppImage)') throw new Error(`expected label "Linux (.AppImage)", got "${text}"`);
        const href = await linuxLink.getAttribute('href');
        if (href !== 'https://example.com/linux.AppImage') throw new Error(`expected the x64 AppImage link, got ${href}`);
        console.log('PASS: Linux label reflects the AppImage fallback when no .deb is published');

        // No published_at on this release — version line should degrade to
        // just the tag, not "released Invalid Date" or similar.
        const versionText = await page.locator('p.download-version').textContent();
        if (versionText !== 'v6.0.0-beta.42') throw new Error(`expected bare version with no release date, got "${versionText}"`);
        console.log('PASS: version line degrades gracefully with no published_at');
    } catch (err) {
        console.error('FAIL (run 4):', err.message);
        failed = true;
    } finally {
        await browser.close();
    }
}

if (failed) process.exit(1);
console.log('ALL PASS');
