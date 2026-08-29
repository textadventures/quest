// Drop-in replacement for `import { chromium } from 'playwright'`, used by every
// verify-appshell-*.mjs script instead of importing 'playwright' directly.
//
// Always mocks GitHub's releases API (see mockReleasesApi below) on every
// context/page a launched browser creates. Beyond that, when
// QUEST_TOUCHED_FILES_DIR is unset (every normal run, local or CI) this adds no
// other behavior over the real `chromium` export.
//
// When QUEST_TOUCHED_FILES_DIR is set, records real V8 JS coverage for every page
// a launched browser opens and, on browser.close(), writes the set of AppShell
// source files (src/AppShell/src/**) that were actually *executed* — not merely
// requested — to `<dir>/<script-basename>.json`. That's the raw material
// build-coverage-map.mjs turns into a source-file -> tests lookup for
// find-affected-tests.mjs.
//
// This deliberately uses function-level execution, not "was the file fetched":
// an earlier version tracked request URLs instead, but SvelteKit's /edit route
// statically imports its entire component library up front (modals included,
// even ones gated behind `{#if}`), so nearly every AppShell test "requests"
// nearly every component regardless of what it actually exercises - a modal's
// module still gets fetched and its top-level module-eval code still runs even
// when the modal is never opened. What doesn't run unless the component is
// actually instantiated is its compiled *named* functions (Svelte's create/
// instance/mount functions etc.), so a file only counts as touched here if V8
// recorded a non-zero call count for at least one named function in it - the
// anonymous top-level module-scope range (which always executes on import) is
// ignored on purpose.
//
// Coverage is snapshotted periodically (stop + immediately restart) rather than
// once at browser.close(), for two reasons found empirically on a real test
// (verify-preview-custom-library.mjs): a single end-of-test stopJSCoverage() call
// silently loses coverage for scripts V8 has already evicted after a long-enough
// session (confirmed - a 7-file result for a page that should have had 70+, with
// only the most-recently-touched files surviving), and it loses ephemeral popup
// pages entirely if they close themselves before the final aggregation runs (the
// Preview flow's player window does exactly this). Snapshotting every ~800ms
// keeps each window short enough that nothing gets evicted before it's recorded.
import { chromium as realChromium } from 'playwright';
import { writeFileSync, mkdirSync } from 'node:fs';
import { join, basename } from 'node:path';

const outDir = process.env.QUEST_TOUCHED_FILES_DIR;
const SNAPSHOT_INTERVAL_MS = 4000;

// HomeHeader mounts DownloadButton (src/AppShell/src/lib/download-links.ts)
// on every page load when PUBLIC_SHOW_HOME is set (every appshell_chromium
// e2e script does, via the workflow's dev-server env), which fires this
// exact request unauthenticated. Almost every verify-appshell-*.mjs script
// navigates to / or /open at least once, so left unmocked here this was
// hitting GitHub's 60-req/hour unauthenticated rate limit across a full
// suite run - default-mocked for every context/page so individual scripts
// don't each need their own copy. A script that wants specific release data
// (or to test the failure path) can still override with its own page.route()
// for the same URL - Playwright always prefers a page-level route over this
// context-level one, regardless of registration order.
const RELEASES_API_URL = 'https://api.github.com/repos/textadventures/quest/releases/latest';
const SAMPLE_RELEASE = {
    tag_name: 'v6.0.0-beta.42',
    published_at: '2026-07-19T07:30:55Z',
    assets: [
        { name: 'Quest.Viva.Setup.6.0.0-beta.42.exe', browser_download_url: 'https://example.com/win.exe' },
        { name: 'Quest.Viva-6.0.0-beta.42-arm64.dmg', browser_download_url: 'https://example.com/mac.dmg' },
        { name: 'Quest.Viva-6.0.0-beta.42.deb', browser_download_url: 'https://example.com/linux.deb' },
        { name: 'Quest.Viva-6.0.0-beta.42.AppImage', browser_download_url: 'https://example.com/linux.AppImage' },
    ],
};

async function mockReleasesApi(context) {
    await context.route(RELEASES_API_URL, route => route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(SAMPLE_RELEASE),
    }));
}

function withDefaultMocks(real) {
    return {
        ...real,
        async launch(...args) {
            const browser = await real.launch(...args);
            for (const ctx of browser.contexts()) await mockReleasesApi(ctx);
            const origNewContext = browser.newContext.bind(browser);
            browser.newContext = async (...a) => {
                const ctx = await origNewContext(...a);
                await mockReleasesApi(ctx);
                return ctx;
            };
            const origNewPage = browser.newPage.bind(browser);
            browser.newPage = async (...a) => {
                const page = await origNewPage(...a);
                await mockReleasesApi(page.context());
                return page;
            };
            return browser;
        },
    };
}

const withDefaults = withDefaultMocks(realChromium);
export const chromium = outDir ? withTracking(withDefaults, outDir) : withDefaults;

function withTracking(real, outDir) {
    return {
        ...real,
        async launch(...args) {
            const browser = await real.launch(...args);
            const touched = new Set();
            const intervals = new Set();

            const recordEntries = (entries) => {
                for (const entry of entries) {
                    const match = entry.url.match(/^https?:\/\/[^/]+(\/src\/[^?]+)/);
                    if (!match) continue;
                    const executed = entry.functions.some(
                        (fn) => fn.functionName && fn.ranges.some((r) => r.count > 0),
                    );
                    if (executed) touched.add(`src/AppShell${match[1]}`);
                }
            };

            const trackPage = async (page) => {
                try {
                    await page.coverage.startJSCoverage({ resetOnNavigation: false, reportAnonymousScripts: false });
                } catch {
                    return; // page already closed/navigated away before we could start
                }
                const snapshot = async () => {
                    try {
                        const entries = await page.coverage.stopJSCoverage();
                        recordEntries(entries);
                        await page.coverage.startJSCoverage({ resetOnNavigation: false, reportAnonymousScripts: false });
                    } catch {
                        // Page navigated/closed between the stop and restart - the 'close'
                        // handler below covers the closed case.
                    }
                };
                const interval = setInterval(snapshot, SNAPSHOT_INTERVAL_MS);
                intervals.add(interval);
                page.once('close', async () => {
                    clearInterval(interval);
                    intervals.delete(interval);
                    try {
                        recordEntries(await page.coverage.stopJSCoverage());
                    } catch {
                        // Coverage already flushed by a snapshot just before close - fine.
                    }
                });
            };

            const trackContext = (ctx) => {
                for (const page of ctx.pages()) trackPage(page);
                ctx.on('page', trackPage);
            };
            for (const ctx of browser.contexts()) trackContext(ctx);
            const origNewContext = browser.newContext.bind(browser);
            browser.newContext = async (...a) => {
                const ctx = await origNewContext(...a);
                trackContext(ctx);
                return ctx;
            };
            const origNewPage = browser.newPage.bind(browser);
            browser.newPage = async (...a) => {
                const page = await origNewPage(...a);
                await trackPage(page);
                return page;
            };

            const origClose = browser.close.bind(browser);
            browser.close = async (...a) => {
                for (const interval of intervals) clearInterval(interval);
                for (const ctx of browser.contexts()) {
                    for (const page of ctx.pages()) {
                        try {
                            recordEntries(await page.coverage.stopJSCoverage());
                        } catch {
                            // Already flushed by its own snapshot/close handler.
                        }
                    }
                }
                if (touched.size > 0) {
                    mkdirSync(outDir, { recursive: true });
                    const scriptName = basename(process.argv[1] ?? 'unknown-script');
                    writeFileSync(join(outDir, `${scriptName}.json`), JSON.stringify([...touched].sort(), null, 2));
                }
                return origClose(...a);
            };

            return browser;
        },
    };
}
