// Checks when WasmPlayer asks for a click (the Begin card) before revealing a game. The game's
// start runs behind the start screen, and Begin only shows if that played a sound the browser
// refused, because nobody has clicked the page yet. The sound then plays on the click.
//
// Media elements' play() is replaced by a fake that's refused (NotAllowedError) until the page
// has had a click or keypress, as a browser's autoplay rules do.
//
// - fixtures/begin-gate-no-sound.aslx: a gamebook with "play sound" in its text, as every
//   published gamebook has from its copy of GamebookCore, but no sound. No Begin.
// - fixtures/begin-gate-sound-later.aslx: a sound played only on a command. No Begin, and the
//   command's sound plays.
// - fixtures/v2-gamebook-test.aslx: a first page with a sound, loaded as if from
//   textadventures.co.uk (?id=, API mocked). Begin shows, as a good-sized button with a hand
//   cursor, and the sound plays on its click - not on a click anywhere else.
// - fixtures/begin-gate-sync-sound.aslx: a start script that waits for its sound to finish.
//   Begin shows, and the script carries on once the sound has played.
// - fixtures/begin-gate-autoplay.aslx: a start script that writes an <audio autoplay>. Begin
//   shows, and the music plays on the click.
// - fixtures/begin-gate-intro.aslx: an intro whose second line comes from a 1-second timer.
//   Game time stands still while Begin shows, so the line only comes after the click.
// - fixtures/begin-gate-v4-wait.asl: a Quest 4 start script that waits for a key. The game is
//   revealed at the wait rather than left behind the start screen.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('begin-gate', async ({ baseURL }) => {
    const baseUrl = baseURL;

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    let browser;
    try {
        browser = await chromium.launch();

        // Opens a game and returns 'begin' if the Begin card shows, or 'started' if the game is
        // revealed without it.
        async function open(fixture, { viaApi = false } = {}) {
            const page = await browser.newPage();
            const errors = [];
            page.on('pageerror', (err) => errors.push(String(err)));
            await page.addInitScript(() => {
                let activated = false;
                const activate = () => { activated = true; };
                document.addEventListener('pointerdown', activate, { capture: true });
                document.addEventListener('keydown', activate, { capture: true });
                window.__played = [];
                HTMLMediaElement.prototype.play = function () {
                    if (!activated) return Promise.reject(new DOMException('No click yet', 'NotAllowedError'));
                    window.__played.push(this.getAttribute('src')?.split('/').pop() ?? this.src.split('/').pop());
                    setTimeout(() => this.dispatchEvent(new Event('ended')), 100);
                    return Promise.resolve();
                };
            });
            if (viaApi) {
                // The player reports how far the game got (GameProgress); nothing to check here.
                await page.route('**/api/gameprogress/**', (route) => route.fulfill({ status: 204 }));
                await page.route('**/api/game/**', (route) => route.fulfill({
                    contentType: 'application/json',
                    body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/${fixture}`, resourceRoot: `${baseUrl}/e2e-fixtures/` }),
                }));
                await page.goto(`${baseUrl}/?id=${fixture}`);
            } else {
                await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}`);
            }
            const outcome = await page.waitForFunction(() => {
                if (document.getElementById('qv-clicktobegin')?.offsetParent) return 'begin';
                if (!document.getElementById('qv-start')) return 'started';
                return null;
            }, null, { timeout: 30000 }).then((h) => h.jsonValue()).catch(() => { throw new Error(`${fixture} never loaded`); });
            return { page, outcome, errors };
        }

        const output = (page) => page.evaluate(() => document.body.innerText);
        const played = (page) => page.evaluate(() => window.__played);
        const waitForText = (page, text) => page.waitForFunction((t) => document.body.innerText.includes(t), text, { timeout: 10000 })
            .catch(() => { throw new Error(`"${text}" never appeared`); });

        {
            const { page, outcome } = await open('begin-gate-no-sound.aslx', { viaApi: true });
            check(outcome === 'started', 'a gamebook with no sound starts without Begin');
            await waitForText(page, 'This is the first page.');
            await page.close();
        }

        {
            const { page, outcome } = await open('begin-gate-sound-later.aslx');
            check(outcome === 'started', 'a game that only plays a sound on a command starts without Begin');
            await waitForText(page, 'The game has started.');
            await page.waitForFunction(() => window.canSendCommand, null, { timeout: 10000 });
            await page.keyboard.type('ring');
            await page.keyboard.press('Enter');
            await waitForText(page, 'Ding.');
            await page.waitForFunction(() => window.__played.length > 0, null, { timeout: 5000 }).catch(() => {});
            check((await played(page)).join() === 'bell.wav', 'the command\'s sound plays');
            await page.close();
        }

        {
            const { page, outcome, errors } = await open('v2-gamebook-test.aslx', { viaApi: true });
            check(outcome === 'begin', 'a gamebook whose first page has a sound shows Begin');
            check((await played(page)).length === 0, 'the sound waits for the click');
            const button = await page.evaluate(() => {
                const el = document.getElementById('qv-clicktobegin-btn');
                const box = el.getBoundingClientRect();
                return { cursor: getComputedStyle(el).cursor, width: box.width, height: box.height };
            });
            check(button.cursor === 'pointer' && button.width >= 120 && button.height >= 44,
                `Begin is a good-sized button with a hand cursor (${JSON.stringify(button)})`);
            await page.mouse.click(5, 5);
            await page.keyboard.press('Shift');
            await page.waitForTimeout(300);
            check((await played(page)).length === 0, 'a click or key anywhere else doesn\'t play the sound behind Begin');
            await page.click('#qv-clicktobegin-btn');
            await page.waitForFunction(() => !document.getElementById('qv-start'), null, { timeout: 10000 });
            check((await played(page)).join() === 'short-beep.wav', 'clicking Begin plays the sound and reveals the game');
            check((await output(page)).includes('This is the first page.'), 'the first page is on screen');
            check(errors.length === 0, `no page errors (${errors.join('; ')})`);
            await page.close();
        }

        {
            const { page, outcome } = await open('begin-gate-sync-sound.aslx');
            check(outcome === 'begin', 'a start script that waits for a sound shows Begin');
            await page.keyboard.press('Enter'); // Begin has the focus
            await waitForText(page, 'After the sound.');
            check((await played(page)).join() === 'intro.wav', 'the start script carries on once its sound has played');
            await page.close();
        }

        {
            const { page, outcome } = await open('begin-gate-autoplay.aslx');
            check(outcome === 'begin', 'a start script that writes an <audio autoplay> shows Begin');
            await page.click('#qv-clicktobegin-btn');
            await page.waitForFunction(() => window.__played.length > 0, null, { timeout: 5000 }).catch(() => {});
            check((await played(page)).join() === 'theme.mp3', 'clicking Begin plays the music');
            await page.close();
        }

        {
            const { page, outcome } = await open('begin-gate-intro.aslx');
            check(outcome === 'begin', 'an intro that plays a sound shows Begin');
            await page.waitForTimeout(3000);
            check(!(await output(page)).includes('Intro line two.'), 'its timer doesn\'t run while Begin shows');
            await page.click('#qv-clicktobegin-btn');
            const shownAt = Date.now();
            await waitForText(page, 'Intro line two.');
            check(Date.now() - shownAt >= 800, 'the timer starts when Begin is clicked');
            await page.close();
        }

        {
            const { page, outcome } = await open('begin-gate-v4-wait.asl');
            check(outcome === 'started', 'a Quest 4 start script that waits for a key is revealed at the wait');
            await waitForText(page, 'Before the wait.');
            check(!(await output(page)).includes('After the wait.'), 'the start script is still waiting');
            await page.keyboard.press('Space');
            await waitForText(page, 'After the wait.');
            check(true, 'a key carries the start script on');
            await page.close();
        }
    } catch (err) {
        console.error(`FAIL: ${err.message}`);
        throw err;
    } finally {
        await browser?.close();
    }
});
