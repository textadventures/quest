// Checks that a game from textadventures.co.uk (?id=, with its API mocked) reports how far it got
// to that site's GameProgress API, and that a game from anywhere else reports nothing.
//
// - "started" once the game appears, "begin" first if the Begin card shows, "failed" if the game
//   can't be started. Each carries the playId sent with api/Game/{id}.
//
// Media elements' play() is replaced by one that resolves at once, so the fixture's missing sound
// file doesn't matter. Playwright's page has had no click, so a game whose start plays a sound
// shows the Begin card (6.0 shows it for any such game when the page has had no click yet).
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    // Opens a page, with the API mocked to point at fixture (or a missing file), and records
    // the api/Game/{id} request's playId and every GameProgress report.
    async function open(path, { fixture } = {}) {
        const page = await browser.newPage();
        await page.addInitScript(() => {
            HTMLMediaElement.prototype.play = function () {
                setTimeout(() => this.dispatchEvent(new Event('ended')), 100);
                return Promise.resolve();
            };
        });
        const seen = { playId: null, reports: [] };
        await page.route('**/api/game/**', (route) => {
            seen.playId = new URL(route.request().url()).searchParams.get('playId');
            return route.fulfill({
                contentType: 'application/json',
                body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/${fixture}`, resourceRoot: null }),
            });
        });
        await page.route('**/api/gameprogress/**', (route) => {
            const url = new URL(route.request().url());
            seen.reports.push({ method: route.request().method(), path: url.pathname, ...Object.fromEntries(url.searchParams) });
            return route.fulfill({ status: 204 });
        });
        await page.goto(`${baseUrl}/${path}`);
        return { page, seen };
    }

    const waitForReports = async (seen, n) => {
        for (let i = 0; i < 100 && seen.reports.length < n; i++) await new Promise((r) => setTimeout(r, 100));
    };

    {
        const { page, seen } = await open('?id=listed', { fixture: 'progress-no-sound.aslx' });
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand, null, { timeout: 30000 });
        await waitForReports(seen, 1);
        await new Promise((r) => setTimeout(r, 300));
        const started = seen.reports.find((r) => r.stage === 'started');
        check(seen.reports.length === 1 && started?.method === 'POST', `the game reports "started", once (${JSON.stringify(seen.reports)})`);
        check(started.path.endsWith('/api/gameprogress/listed'), `to the game's GameProgress URL (${started.path})`);
        check(Boolean(seen.playId) && started.playId === seen.playId, 'with the playId it sent to api/Game');
        check(started.ui === 'legacy' && Number(started.ms) > 0 && started.source === 'web', 'and the player UI, time and source');
        await page.close();
    }

    {
        // Nothing may touch the page until Begin shows: Playwright runs evaluate() as a user
        // gesture, which would count as a click and skip the Begin card. So wait for the report.
        const { page, seen } = await open('?id=listed', { fixture: 'progress-start-sound.aslx' });
        await waitForReports(seen, 1);
        check(seen.reports.map((r) => r.stage).join() === 'begin', 'showing Begin reports "begin"');
        check(await page.evaluate(() => document.getElementById('qv-clicktobegin')?.style.display === 'flex'), 'with the Begin card showing');
        await page.click('#qv-clicktobegin-btn');
        await page.waitForFunction(() => !document.getElementById('qv-start'), null, { timeout: 10000 });
        await waitForReports(seen, 2);
        check(seen.reports.map((r) => r.stage).join() === 'begin,started', 'then "started" once it\'s clicked');
        await page.close();
    }

    {
        const { page, seen } = await open('?id=listed', { fixture: 'no-such-game.aslx' });
        await page.locator('#qv-error').waitFor({ state: 'visible', timeout: 30000 });
        await waitForReports(seen, 1);
        const failed = seen.reports.find((r) => r.stage === 'failed');
        check(seen.reports.length === 1 && failed && failed.error.length > 0, `a game that can't be loaded reports "failed", with the error (${failed?.error})`);
        await page.close();
    }

    {
        const { page, seen } = await open('?url=/e2e-fixtures/progress-no-sound.aslx');
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand, null, { timeout: 30000 });
        await new Promise((r) => setTimeout(r, 500));
        check(seen.reports.length === 0, 'a game with no API reports nothing');
        await page.close();
    }
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
