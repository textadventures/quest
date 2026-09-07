// Verifies that every static asset WasmPlayer fetches from its own origin
// carries the `?v=<version>` cache key, exactly once.
//
// This is a guard, not a feature test. play.questviva.com serves those paths
// `Cache-Control: public, max-age=31536000, immutable` (see the _headers block
// in .github/workflows/deploy-play.yml), which is only sound because the query
// makes every deploy a distinct cache key. An asset that reaches the browser
// *without* one gets pinned in every visitor's cache for a year, and no
// subsequent deploy can recall it — so a new asset added to index.html or
// fetched from wasm-player.js without going through the stamping needs to fail
// here rather than in production.
//
// The _framework/ half matters twice over: those filenames aren't
// content-hashed and AOT output isn't byte-reproducible, so pairing a cached
// dotnet.boot.js (the manifest of per-file SHA-256 hashes) with a binary from a
// different deploy fails the SRI integrity check and blocks the resource.
//
// Run against the WasmPlayer dev server:
//   node src/WasmPlayer/dev-server.mjs
//   node tests/e2e/verify-wasmplayer-asset-versioning.mjs http://localhost:5175
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const browser = await chromium.launch();

try {
    const page = await browser.newPage();
    const requested = [];
    const consoleErrors = [];
    page.on('request', r => requested.push(r.url()));
    page.on('console', m => { if (m.type() === 'error') consoleErrors.push(m.text()); });
    page.on('pageerror', e => consoleErrors.push(`pageerror: ${e.message}`));

    await page.goto(`${baseUrl}/?url=/examples/simple.aslx`, { waitUntil: 'load' });
    await page.waitForFunction(
        () => document.querySelector('#divOutput')?.textContent?.includes('You are in a room'),
        { timeout: 120000 });

    const version = await page.evaluate(() => window.QuestVivaVersion);
    if (!version) throw new Error('window.QuestVivaVersion is not set — scripts/inject-version.mjs did not run, so nothing would be version-stamped');
    const query = `?v=${encodeURIComponent(version)}`;

    // Same-origin static assets only: the game file itself (/examples/...) is
    // author content fetched by URL, not part of the deployed bundle.
    const assets = requested.filter(u => u.startsWith(baseUrl))
        .filter(u => !u.startsWith(`${baseUrl}/examples/`))
        .filter(u => new URL(u).pathname !== '/');

    const framework = assets.filter(u => u.includes('/_framework/'));
    if (framework.length < 100) {
        throw new Error(`expected the whole runtime to be downloaded, saw only ${framework.length} _framework requests`);
    }

    // lib/images/* are referenced by relative URL from inside jquery-ui.min.css,
    // so they can't be stamped and are deliberately excluded from the immutable
    // rules too — they must stay excluded from both, together.
    const stampable = assets.filter(u => !new URL(u).pathname.startsWith('/lib/images/'));

    const unstamped = stampable.filter(u => !u.endsWith(query));
    if (unstamped.length) {
        throw new Error(`${unstamped.length} of ${stampable.length} same-origin assets were requested without ${query}:\n  `
            + unstamped.slice(0, 10).join('\n  '));
    }

    const doubled = stampable.filter(u => (u.match(/[?&]v=/g) || []).length > 1);
    if (doubled.length) {
        throw new Error(`${doubled.length} assets carry the version query more than once (the runtime's own modulesUniqueQuery and withResourceLoader are both stamping them):\n  `
            + doubled.slice(0, 10).join('\n  '));
    }

    const sriErrors = consoleErrors.filter(e => /integrity|digest/i.test(e));
    if (sriErrors.length) {
        throw new Error(`SRI integrity failures — the manifest and the binaries disagree:\n  ${sriErrors.join('\n  ')}`);
    }

    // The headers are only half the fix: the runtime otherwise fetches every
    // asset with `cache: "no-cache"`, which forces a conditional request no
    // matter how long the CDN said the response was fresh for.
    const noCacheFetch = await page.evaluate(() => globalThis.getDotnetRuntime?.(0)?.getConfig?.()?.disableNoCacheFetch);
    if (noCacheFetch !== true) {
        throw new Error(`expected disableNoCacheFetch to be true in the runtime config, got ${JSON.stringify(noCacheFetch)} — `
            + 'without it the browser revalidates every asset on every load and the immutable headers buy nothing');
    }

    console.log(`PASS: ${stampable.length} same-origin assets (${framework.length} under _framework/) all carry ${query} exactly once, `
        + 'no SRI failures, disableNoCacheFetch is on.');
} catch (e) {
    process.exitCode = 1;
    console.error(`FAIL: ${e.message}`);
} finally {
    await browser.close();
}
