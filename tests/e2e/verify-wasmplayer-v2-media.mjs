// Checks player UI v2's media (#2627, PR 3) by playing fixtures/v2-media-test.aslx (610) under
// ?ui=v2, with YouTube and Vimeo served as blank pages and window.Audio replaced by a fake whose
// sound lasts SOUND_MS (as verify-wasmplayer-sound-timer.mjs does):
//
// - ShowYouTube embeds youtube-nocookie.com, autoplaying, with a title; addVimeo embeds Vimeo with
//   dnt=1, sandboxed, and an unlisted video's hash as h=. Both fit the output at 16:9, at most
//   560px wide, with no sideways scroll on a phone.
// - An id with markup in it stays inside the embed's URL: nothing is injected.
// - Loading a save brings the embeds back without autoplay, and none is ever requested with it.
// - A synchronous sound holds the turn until it ends; a looping one plays until stop sound.
// - axe-core finds no WCAG A/AA violations at either width.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { createRequire } from 'node:module';
import { chromium } from 'playwright';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');
const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';
const SOUND_MS = 1500;

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch({ args: ['--autoplay-policy=no-user-gesture-required'] });

    async function open(width) {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        const videoRequests = [];
        await page.route(/youtube-nocookie\.com|youtube\.com|vimeo\.com/, (route) => {
            videoRequests.push(route.request().url());
            return route.fulfill({ status: 200, contentType: 'text/html', body: '<!doctype html><title>video</title>' });
        });
        await page.addInitScript((soundMs) => {
            window.__sounds = [];
            window.Audio = class FakeAudio {
                constructor(src) {
                    this.src = src;
                    this.loop = false;
                    this.paused = true;
                    this._listeners = {};
                    this._timer = null;
                    window.__sounds.push(this);
                }
                addEventListener(name, fn) { (this._listeners[name] ||= []).push(fn); }
                removeEventListener(name, fn) {
                    this._listeners[name] = (this._listeners[name] || []).filter((f) => f !== fn);
                }
                play() {
                    this.paused = false;
                    if (!this.loop) {
                        this._timer = setTimeout(() => {
                            this.paused = true;
                            for (const fn of this._listeners.ended || []) fn();
                        }, soundMs);
                    }
                    return Promise.resolve();
                }
                pause() {
                    this.paused = true;
                    if (this._timer) {
                        clearTimeout(this._timer);
                        this._timer = null;
                    }
                }
            };
        }, SOUND_MS);
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-media-test.aslx&ui=v2`);
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('A screening room.'), null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started on v2'); });
        await page.addScriptTag({ path: axePath });
        return { page, errors, videoRequests };
    }

    async function send(page, command, expect) {
        await page.waitForFunction(() => window.canSendCommand, null, { timeout: 15000 });
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await page.waitForFunction((t) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(t),
            expect, { timeout: 15000 })
            .catch(() => { throw new Error(`"${command}" never printed "${expect}"`); });
    }
    async function audit(page, state) {
        const violations = await page.evaluate(async () => {
            const result = await window.axe.run(document, {
                runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
            });
            return result.violations.map((v) => `${v.id}: ${v.help} (${v.nodes.map((n) => n.target.join(' ')).slice(0, 3).join(', ')})`);
        });
        if (violations.length) throw new Error(`axe found WCAG violations ${state}:\n    ${violations.join('\n    ')}`);
        console.log(`PASS: no WCAG A/AA violations ${state}`);
    }

    const videos = (page) => page.evaluate(() => {
        const output = document.querySelector('qv-output').getBoundingClientRect();
        return [...document.querySelectorAll('qv-output iframe')].map((f) => {
            const box = f.getBoundingClientRect();
            return {
                kind: f.dataset.qvVideo, src: f.getAttribute('src'), title: f.getAttribute('title'),
                allow: f.getAttribute('allow') ?? '', fullscreen: f.hasAttribute('allowfullscreen'), sandbox: f.getAttribute('sandbox'),
                width: box.width, height: box.height, outputWidth: output.width,
            };
        });
    });
    const sideways = (page) => page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth);

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors, videoRequests } = await open(1200);
        await send(page, 'youtube', 'YouTube added.');
        await send(page, 'vimeo', 'Vimeo added.');
        const [youtube, vimeo, unlisted] = await videos(page);
        check(youtube.kind === 'youtube' && youtube.src === 'https://www.youtube-nocookie.com/embed/7vIi0U4rSX4?autoplay=1&rel=0',
            `ShowYouTube embeds youtube-nocookie.com, autoplaying (${youtube.src})`);
        check(youtube.title === 'YouTube video' && youtube.allow.includes('autoplay') && youtube.fullscreen,
            'the YouTube embed has a title and may autoplay and go full screen');
        check(vimeo.src === 'https://player.vimeo.com/video/76979871?autoplay=1&dnt=1' && vimeo.title === 'Vimeo video' && !!vimeo.sandbox,
            `addVimeo embeds Vimeo with dnt=1, sandboxed (${vimeo.src})`);
        check(unlisted.src === 'https://player.vimeo.com/video/76979871?h=abc123&autoplay=1&dnt=1',
            `an unlisted Vimeo video's hash goes in h= (${unlisted.src})`);
        check([youtube, vimeo].every((v) => v.width === 560 && Math.abs(v.height - 315) < 1),
            `each fits at 16:9, at most 560px wide (${youtube.width}×${youtube.height})`);
        check(videoRequests.some((u) => u.includes('youtube-nocookie.com')) && !videoRequests.some((u) => /\/\/(www\.)?youtube\.com/.test(u)),
            'only youtube-nocookie.com is contacted, never youtube.com');

        await send(page, 'markup', 'Markup tried.');
        const injected = await page.evaluate(() => ({
            img: !!document.getElementById('injected'),
            src: [...document.querySelectorAll('qv-output iframe')].at(-1).getAttribute('src'),
        }));
        check(!injected.img && injected.src.startsWith('https://www.youtube-nocookie.com/embed/x%22%3E%3Cimg'),
            "an id with markup in it stays in the embed's URL, and nothing is injected");
        await audit(page, 'with video embeds');

        // ── Sound ──
        await page.fill(input, 'sound');
        await page.press(input, 'Enter');
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Before the sound.'), null, { timeout: 15000 });
        await page.waitForTimeout(SOUND_MS / 3);
        const during = await page.evaluate(() => ({
            after: document.querySelector('qv-output').textContent.includes('After the sound.'),
            // The engine's turn is suspended for the sound: v2 holds the command bar and turns commands away.
            held: document.querySelector('qv-player').hasAttribute('data-qv-input-held'),
            refused: window.QuestUI.sendCommand('look') === false,
            playing: window.__sounds.some((s) => !s.paused && s.src.endsWith('short-beep.wav')),
        }));
        check(during.playing && !during.after && during.held && during.refused, `a synchronous sound plays and holds the turn (${JSON.stringify(during)})`);
        await page.waitForFunction(() => window.canSendCommand && document.querySelector('qv-output').textContent.includes('After the sound.'),
            null, { timeout: SOUND_MS * 4 }).catch(() => { throw new Error('the turn never carried on after the sound'); });
        check(true, 'the turn carries on once the sound ends');

        await send(page, 'music', 'Music playing.');
        check(await page.evaluate(() => window.__sounds.at(-1).loop && !window.__sounds.at(-1).paused), 'a looping sound plays without holding the turn');
        await send(page, 'stopmusic', 'Music stopped.');
        check(await page.evaluate(() => window.__sounds.at(-1).paused), 'stop sound stops it');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Restore ─────────────────────────────────────────────────────────────

    {
        const { page, errors, videoRequests } = await open(1200);
        await send(page, 'youtube', 'YouTube added.');
        await send(page, 'vimeo', 'Vimeo added.');
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
            .catch(() => { throw new Error("Save / Load didn't open the host's dialog"); });
        await page.click('#qv-saves-save-new');
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 10000 });
        await page.evaluate(() => document.getElementById('qv-saves')?.close());
        await send(page, 'look', 'A screening room.');

        const before = videoRequests.length;
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
            .catch(() => { throw new Error('the save never appeared in the Save/Load list'); });
        await page.click('#qv-saves-list [data-slot]');
        await page.waitForFunction(() => window.canSendCommand && document.querySelectorAll('qv-output iframe').length === 3
            && !document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 30000 })
            .catch(() => { throw new Error("loading didn't restore the saved game's embeds"); });
        await page.waitForTimeout(500);
        const restored = await videos(page);
        check(restored.every((v) => !v.src.includes('autoplay')), `restored embeds don't autoplay (${restored.map((v) => v.src).join(', ')})`);
        const since = videoRequests.slice(before);
        check(since.length > 0 && since.every((u) => !u.includes('autoplay')), `none was requested with autoplay while restoring (${since.length} requests)`);

        if (errors.length) throw new Error(`restore: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(390);
        await send(page, 'youtube', 'YouTube added.');
        await send(page, 'vimeo', 'Vimeo added.');
        const v = await videos(page);
        check(v.every((x) => x.width <= x.outputWidth && Math.abs(x.height - x.width * 9 / 16) < 1) && !(await sideways(page)),
            `on a phone the embeds fit the output at 16:9 with no sideways scroll (${Math.round(v[0].width)}×${Math.round(v[0].height)})`);
        await audit(page, 'with video embeds, narrow');
        if (errors.length) throw new Error(`narrow: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
