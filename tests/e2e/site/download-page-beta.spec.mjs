// beta.questviva.com's /download page (site/src/components/DownloadButton.astro built with
// PUBLIC_BETA_SITE=true) offers the newest prerelease, not GitHub's releases/latest, which is
// always the newest stable release.
// Needs a beta build served separately from the stable one, e.g.:
//   cd site && PUBLIC_BETA_SITE=true npm run build -- --outDir dist-beta && python3 -m http.server 4322 -d dist-beta
// Point it elsewhere with BETA_SITE_URL.
import { test } from '@playwright/test';

const baseUrl = process.env.BETA_SITE_URL ?? 'http://localhost:4322';
const LIST_URL = 'https://api.github.com/repos/textadventures/quest/releases?per_page=30';
const LATEST_URL = 'https://api.github.com/repos/textadventures/quest/releases/latest';
const MAC_UA = 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15';

function release(tag, prerelease) {
    return {
        tag_name: tag,
        html_url: `https://github.com/textadventures/quest/releases/tag/${tag}`,
        published_at: '2026-11-14T10:00:00Z',
        prerelease,
        draft: false,
        assets: [
            { name: `Quest.Viva.Beta-${tag}-arm64.dmg`, browser_download_url: `https://example.com/${tag}.dmg` },
            { name: `Quest.Viva.Beta.Setup.${tag}.exe`, browser_download_url: `https://example.com/${tag}.exe` },
        ],
    };
}

function json(body) {
    return (route) => route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(body) });
}

test('beta download page offers the newest prerelease', async ({ browser }) => {
    const page = await browser.newPage({ userAgent: MAC_UA });
    // A stable patch release newer than the prerelease, as release/6.0 keeps shipping 6.0.x.
    await page.route(LIST_URL, json([release('v6.0.5', false), release('v6.1.0-beta.2', true), release('v6.1.0-beta.1', true)]));
    await page.route(LATEST_URL, json(release('v6.0.5', false)));

    await page.goto(`${baseUrl}/download/`);
    const primary = page.locator('a:has-text("Download for Mac")');
    await primary.waitFor({ timeout: 10000 });
    const href = await primary.getAttribute('href');
    if (href !== 'https://example.com/v6.1.0-beta.2.dmg') throw new Error(`expected the beta.2 dmg, got ${href}`);

    const version = await page.locator('p.download-version').textContent();
    if (!version?.startsWith('v6.1.0-beta.2')) throw new Error(`expected version v6.1.0-beta.2, got "${version}"`);

    const allHref = await page.locator('a:has-text("All downloads")').getAttribute('href');
    if (allHref !== 'https://github.com/textadventures/quest/releases/tag/v6.1.0-beta.2') {
        throw new Error(`expected "All downloads" to link to the beta.2 release page, got ${allHref}`);
    }

    const playHref = await page.locator('a:has-text("play-beta.questviva.com")').getAttribute('href');
    if (playHref !== 'https://play-beta.questviva.com') throw new Error(`expected a play-beta link, got ${playHref}`);
});

test('beta download page falls back to the stable release when there is no prerelease', async ({ browser }) => {
    const page = await browser.newPage({ userAgent: MAC_UA });
    await page.route(LIST_URL, json([release('v6.0.5', false)]));
    await page.route(LATEST_URL, json(release('v6.0.5', false)));

    await page.goto(`${baseUrl}/download/`);
    const primary = page.locator('a:has-text("Download for Mac")');
    await primary.waitFor({ timeout: 10000 });
    const href = await primary.getAttribute('href');
    if (href !== 'https://example.com/v6.0.5.dmg') throw new Error(`expected the stable dmg, got ${href}`);
});

test('beta download page keeps a link to all releases when the API fails', async ({ browser }) => {
    const page = await browser.newPage({ userAgent: MAC_UA });
    await page.route(LIST_URL, (route) => route.fulfill({ status: 500 }));
    await page.route(LATEST_URL, (route) => route.fulfill({ status: 500 }));

    await page.goto(`${baseUrl}/download/`);
    const fallback = page.locator('a:has-text("All downloads (GitHub Releases)")');
    await fallback.waitFor({ timeout: 10000 });
    const href = await fallback.getAttribute('href');
    if (href !== 'https://github.com/textadventures/quest/releases') throw new Error(`expected the releases list, got ${href}`);
});
