// Checks the start screen shows the game being loaded, and that a game from textadventures.co.uk
// (?id=, with its API mocked) reports how far it got to that site's GameProgress API.
//
// - The name, author and cover from api/Game/{id} show while the game file is still downloading,
//   in place of the big Quest Viva heading.
// - A game with no API (?url=) shows its own name on the Begin card, and reports nothing.
// - Progress reports: "started" once the game appears, "begin" first if the Begin card shows,
//   "failed" if the game can't be started. Each carries the playId sent with api/Game/{id}, and
//   "started" names the player UI. After a Begin card, "started" also says how long it waited.
// - Each report says when every stage of the load it reached finished, the game file's size and
//   whether the runtime came from the cache (#2857). After the Continue/New Game prompt, it says
//   how long that waited too.
// - They're sent with fetch, not sendBeacon, which content blockers drop without the page being
//   able to tell (#2783). Only a fetch that's blocked falls back to a beacon.
//
// Media elements' play() is replaced by a fake that's refused until the page has been clicked, so
// fixtures/begin-gate-sync-sound.aslx shows the Begin card (see begin-gate.spec.mjs).
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';
import { statSync } from 'node:fs';

const fixtureSize = (name) => statSync(new URL(`../fixtures/${name}`, import.meta.url)).size;

test('splash-progress', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==', 'base64');

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    let browser;
    try {
        browser = await chromium.launch();

        // Opens a page, with the API mocked to point at fixture (or a missing file), and records
        // the api/Game/{id} request's playId and every GameProgress report.
        async function open(path, { fixture, holdGameFile = false, blockFetch = false } = {}) {
            const page = await browser.newPage();
            await page.addInitScript(() => {
                let activated = false;
                document.addEventListener('pointerdown', () => { activated = true; }, { capture: true });
                HTMLMediaElement.prototype.play = function () {
                    if (!activated) return Promise.reject(new DOMException('No click yet', 'NotAllowedError'));
                    setTimeout(() => this.dispatchEvent(new Event('ended')), 100);
                    return Promise.resolve();
                };
            });
            const seen = { playId: null, reports: [] };
            let releaseGameFile;
            const gameFileHeld = new Promise((resolve) => { releaseGameFile = resolve; });
            await page.route('**/api/game/**', (route) => {
                seen.playId = new URL(route.request().url()).searchParams.get('playId');
                return route.fulfill({
                    contentType: 'application/json',
                    body: JSON.stringify({
                        sourceGameUrl: `${baseUrl}/e2e-fixtures/${fixture}`,
                        resourceRoot: 'https://images.example.test/res/',
                        name: 'The Listed Name',
                        author: 'A. Author',
                        cover: 'https://images.example.test/cover.png',
                    }),
                });
            });
            await page.route('**/api/gameprogress/**', (route) => {
                const request = route.request();
                const type = request.resourceType();
                if (blockFetch && type === 'fetch') {
                    seen.blocked = (seen.blocked ?? 0) + 1;
                    return route.abort('blockedbyclient');
                }
                const url = new URL(request.url());
                seen.reports.push({ method: request.method(), type, ...Object.fromEntries(url.searchParams) });
                return route.fulfill({ status: 204 });
            });
            await page.route('https://images.example.test/**', (route) => route.fulfill({ contentType: 'image/png', body: png }));
            if (holdGameFile) {
                await page.route(`**/e2e-fixtures/${fixture}`, async (route) => {
                    await gameFileHeld;
                    await route.continue();
                });
            }
            await page.goto(`${baseUrl}/${path}`);
            return { page, seen, releaseGameFile };
        }

        const visible = (page, selector) => page.locator(selector).isVisible();
        const waitForReports = (seen, n) => (async () => {
            for (let i = 0; i < 100 && seen.reports.length < n; i++) await new Promise((r) => setTimeout(r, 100));
        })();

        {
            const { page, seen, releaseGameFile } = await open('?id=listed', { fixture: 'begin-gate-no-sound.aslx', holdGameFile: true });
            await page.waitForFunction(() => document.getElementById('qv-game-name')?.textContent === 'The Listed Name', null, { timeout: 15000 })
                .catch(() => { throw new Error('the listed name never showed on the start screen'); });
            check(await visible(page, '#qv-loading-msg'), 'the listed name shows while the game is still downloading');
            check((await page.textContent('#qv-game-author')) === 'by A. Author' && await visible(page, '#qv-game-author'), 'so does its author');
            await page.waitForFunction(() => document.getElementById('qv-game-cover')?.offsetParent !== null, null, { timeout: 5000 })
                .catch(() => { throw new Error('the cover never showed'); });
            check(true, 'so does its cover');
            check(!(await visible(page, '#qv-brand')) && await visible(page, '#qv-brand-small'), 'the big Quest Viva heading gives way to a small one');
            check((await page.title()) === 'The Listed Name', 'the page title is the game\'s name');
            releaseGameFile();
            await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand, null, { timeout: 30000 });
            await waitForReports(seen, 1);
            const started = seen.reports.find((r) => r.stage === 'started');
            check(seen.reports.length === 1 && started?.method === 'POST', `the game reports "started", once (${JSON.stringify(seen.reports)})`);
            check(started.type === 'fetch', `with fetch, not a beacon (${started.type})`);
            check(Boolean(seen.playId) && started.playId === seen.playId, 'with the playId it sent to api/Game');
            check(started.ui === 'v2' && Number(started.ms) > 0 && started.source === 'web', 'and the player UI, time and source');
            check(started.waited === undefined, 'with no Begin wait when there was no Begin card');

            // How long each stage took (#2857): every stage's time, in the order they happen.
            const stages = ['api', 'gameBytes', 'runtimeDownloaded', 'runtimeReady', 'initialised', 'begun'];
            const missing = stages.filter((s) => !(Number(started[s]) > 0));
            check(missing.length === 0, `"started" says when each stage finished (missing: ${missing.join() || 'none'})`);
            const at = Object.fromEntries(stages.map((s) => [s, Number(started[s])]));
            check(at.api <= at.gameBytes && at.runtimeDownloaded <= at.runtimeReady && at.gameBytes <= at.initialised
                && at.runtimeReady <= at.initialised && at.initialised <= at.begun && at.begun <= Number(started.ms),
                `in the order they happen (${JSON.stringify(at)}, started at ${started.ms})`);
            check(started.gameSize === String(fixtureSize('begin-gate-no-sound.aslx')), `and the game file's size (${started.gameSize})`);
            // The dev server sends the runtime with no-store, so it's never from the cache here.
            check(started.runtimeCached === '0', `and that the runtime wasn't from the cache (${started.runtimeCached})`);
            check(started.savesWaited === undefined, 'with no saves prompt wait when there were no saves');

            // With a save, the next load asks Continue or start again first. That wait is reported
            // separately, like Begin's, since it falls inside the stages after it.
            await page.evaluate(() => openSavesDialog('manage'));
            await page.locator('#qv-saves[open]').waitFor({ timeout: 5000 });
            await page.click('#qv-saves-save-new');
            await page.waitForFunction(() => document.querySelectorAll('#qv-saves-list [data-row-slot]').length === 1, null, { timeout: 10000 });
            seen.reports.length = 0;
            await page.reload();
            await page.locator('#qv-saves[open][data-mode="boot"]').waitFor({ timeout: 30000 });
            await new Promise((r) => setTimeout(r, 1000));
            await page.click('#qv-saves-start-new');
            await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand, null, { timeout: 30000 });
            await waitForReports(seen, 1);
            const savesWaited = Number(seen.reports[0]?.savesWaited);
            check(seen.reports[0]?.stage === 'started' && savesWaited >= 1000 && savesWaited <= Number(seen.reports[0].initialised),
                `after a saves prompt, "started" says how long it waited (${JSON.stringify(seen.reports)})`);
            await page.close();
        }

        {
            const { page, seen } = await open('?id=listed', { fixture: 'begin-gate-sync-sound.aslx' });
            await page.waitForFunction(() => document.getElementById('qv-clicktobegin')?.offsetParent, null, { timeout: 30000 });
            check((await page.textContent('#qv-game-name')) === 'The Listed Name', 'the Begin card keeps the listed name, not the game\'s own');
            await waitForReports(seen, 1);
            check(seen.reports.map((r) => r.stage).join() === 'begin', 'showing Begin reports "begin"');
            check(seen.reports[0].waited === undefined, 'with no Begin wait yet');
            await new Promise((r) => setTimeout(r, 1500));
            await page.click('#qv-clicktobegin-btn');
            await page.waitForFunction(() => !document.getElementById('qv-start'), null, { timeout: 10000 });
            await waitForReports(seen, 2);
            check(seen.reports.map((r) => r.stage).join() === 'begin,started', 'then "started" once it\'s clicked');
            const [begin, started] = seen.reports;
            const waited = Number(started.waited);
            check(waited >= 1500 && waited <= Number(started.ms) - Number(begin.ms),
                `which says how long Begin waited (${waited} ms, begin at ${begin.ms}, started at ${started.ms})`);
            await page.close();
        }

        {
            const { page, seen } = await open('?id=listed', { fixture: 'no-such-game.aslx' });
            await page.waitForFunction(() => document.getElementById('qv-error')?.offsetParent, null, { timeout: 30000 });
            await waitForReports(seen, 1);
            const failed = seen.reports.find((r) => r.stage === 'failed');
            check(seen.reports.length === 1 && failed && failed.error.length > 0, `a game that can't be loaded reports "failed", with the error (${failed?.error})`);
            check(Number(failed.api) > 0 && failed.gameBytes === undefined && failed.initialised === undefined && failed.gameSize === undefined,
                `with the stages it reached, and none it didn't (${JSON.stringify(failed)})`);
            await page.close();
        }

        {
            const { page, seen } = await open('?id=listed', { fixture: 'begin-gate-no-sound.aslx', blockFetch: true });
            await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand, null, { timeout: 30000 });
            await waitForReports(seen, 1);
            await new Promise((r) => setTimeout(r, 500));
            check(seen.blocked === 1 && seen.reports.length === 1 && seen.reports[0].stage === 'started' && seen.reports[0].type !== 'fetch',
                `a blocked fetch is sent again as a beacon, once (${seen.blocked} blocked, ${JSON.stringify(seen.reports)})`);
            await page.close();
        }

        {
            const { page, seen } = await open('?url=/e2e-fixtures/begin-gate-sync-sound.aslx');
            await page.waitForFunction(() => document.getElementById('qv-clicktobegin')?.offsetParent, null, { timeout: 30000 });
            check((await page.textContent('#qv-game-name')) === 'BeginGateSyncSound', 'a game with no API shows its own name on the Begin card');
            check(!(await visible(page, '#qv-game-author')) && !(await visible(page, '#qv-game-cover')), 'with no author or cover');
            await page.click('#qv-clicktobegin-btn');
            await page.waitForFunction(() => !document.getElementById('qv-start'), null, { timeout: 10000 });
            await new Promise((r) => setTimeout(r, 500));
            check(seen.reports.length === 0, 'and reports nothing');
            await page.close();
        }
    } catch (err) {
        console.error(`FAIL: ${err.message}`);
        throw err;
    } finally {
        await browser?.close();
    }
});
