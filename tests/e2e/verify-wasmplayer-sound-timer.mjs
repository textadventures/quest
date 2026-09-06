// Ad-hoc manual verification for issue #891 ("Conflict - Playing a Sound and
// Timers", reported 2017 against Quest 5): a sound played just before a
// "Run script after a number of seconds" (SetTimeout) script was reported to
// make the timeout script - and anything nested inside it - get skipped
// entirely, or delayed until the following turn.
//
// The engine's turn/timer machinery has been rewritten substantially since
// then (PlaySoundScript's synchronous path now suspends the turn via
// BeginPrompt/BeginPendingCallback/SignalTurnSuspended, and WorldModel's
// SendNextTimerRequest ordering was fixed so a timer script doesn't stop the
// chain), so this checks whether the reported behaviour still happens.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// AUDIO CLOCK STUB. wasm-player.js's synchronous "play sound" ends the engine's
// wait from the HTMLAudioElement's 'ended' event (see playSound/finishSync).
// Chromium under Playwright - headless AND headed - has no functioning audio
// output sink here: the element reaches readyState 4 and fires 'playing', but
// currentTime never advances and 'ended' never fires, so a synchronous sound
// would hang forever for reasons that have nothing to do with Quest. This
// script therefore replaces window.Audio with a fake whose only job is to fire
// 'ended' after a real 400ms delay. Everything actually under test is still
// real: wasm-player.js's playSound/stopSound/finishSync/uiEndWait, the
// JSImport boundary, PlaySoundScript's suspend/resume, and the timer
// scheduling in WorldModel. Pass --real-audio to skip the stub and use the
// browser's own audio, if you are running somewhere playback genuinely works.
import { chromium } from 'playwright';

const args = process.argv.slice(2);
const useRealAudio = args.includes('--real-audio');
const baseUrl = args.find(a => !a.startsWith('--')) || 'http://localhost:5175';
const SOUND_MS = 400; // matches fixtures/short-beep.wav's real duration

const browser = await chromium.launch({
    args: ['--autoplay-policy=no-user-gesture-required'],
});
const page = await browser.newPage();
page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
page.on('pageerror', err => console.log('[pageerror]', err.message));

if (!useRealAudio) {
    await page.addInitScript((soundMs) => {
        window.__fakeAudioPlays = 0;
        window.Audio = class FakeAudio {
            constructor(src) {
                this.src = src;
                this.loop = false;
                this.paused = true;
                this._listeners = {};
                this._timer = null;
            }
            addEventListener(name, fn) { (this._listeners[name] ||= []).push(fn); }
            removeEventListener(name, fn) {
                this._listeners[name] = (this._listeners[name] || []).filter(f => f !== fn);
            }
            play() {
                window.__fakeAudioPlays++;
                this.paused = false;
                if (!this.loop) {
                    this._timer = setTimeout(() => {
                        this.paused = true;
                        for (const fn of this._listeners['ended'] || []) fn();
                    }, soundMs);
                }
                return Promise.resolve();
            }
            pause() {
                this.paused = true;
                if (this._timer) { clearTimeout(this._timer); this._timer = null; }
            }
        };
    }, SOUND_MS);
}

async function transcript() {
    return (await page.$eval('#divOutput', el => el.innerText)).trim();
}

// sendCommand() in player.js silently drops a command while canSendCommand is
// still false from the previous one's round-trip.
async function waitUntilCanSendCommand() {
    await page.waitForFunction(() => window.canSendCommand === true, { timeout: 20000 });
}

async function sendCommand(command) {
    await waitUntilCanSendCommand();
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
}

// Each command prints A/B markers synchronously and then arms a 1-second
// SetTimeout printing C and D. Asserts on the ordered marker sequence without
// sending a further command - sending one would mask the "delayed by a turn"
// half of the reported bug, since the delayed output would then appear anyway.
async function checkCase(command, label, { expectSound }) {
    await page.evaluate(() => { document.querySelector('#divOutput').innerHTML = ''; });
    const playsBefore = useRealAudio ? 0 : await page.evaluate(() => window.__fakeAudioPlays);
    await sendCommand(command);

    let text = '';
    const deadline = Date.now() + 15000;
    while (Date.now() < deadline) {
        text = await transcript();
        if (text.includes('C: timeout fired') && text.includes('D: nested output')) break;
        await page.waitForTimeout(200);
    }

    const markers = ['A: before sound', 'B: after sound', 'C: timeout fired', 'D: nested output'];
    const missing = markers.filter(m => !text.includes(m));
    if (missing.length > 0) {
        throw new Error(
            `${label}: missing output ${JSON.stringify(missing)} without sending a further command. ` +
            `Transcript was:\n${text}`);
    }

    const positions = markers.map(m => text.indexOf(m));
    for (let i = 1; i < positions.length; i++) {
        if (positions[i] < positions[i - 1]) {
            throw new Error(
                `${label}: output out of order - "${markers[i]}" appeared before "${markers[i - 1]}". ` +
                `Transcript was:\n${text}`);
        }
    }

    // Guard against a false pass where the sound never played at all, which
    // would quietly degrade every sound case into the control case.
    if (!useRealAudio) {
        const playsAfter = await page.evaluate(() => window.__fakeAudioPlays);
        const played = playsAfter - playsBefore;
        if (expectSound && played !== 1) {
            throw new Error(`${label}: expected exactly 1 sound to play, got ${played}`);
        }
        if (!expectSound && played !== 0) {
            throw new Error(`${label}: expected no sound to play, got ${played}`);
        }
    }

    console.log(`PASS: ${label} (all four markers, in order, on the same turn)`);
}

async function run() {
    // Sanity-check the fixture sound is actually served, so the fixture stays
    // honest for a --real-audio run even though the stub never fetches it.
    const soundRes = await page.request.get(`${baseUrl}/short-beep.wav`);
    if (!soundRes.ok() || !(soundRes.headers()['content-type'] || '').startsWith('audio/')) {
        throw new Error(`fixture sound not served correctly: ` +
            `${soundRes.status()} ${soundRes.headers()['content-type']}`);
    }
    console.log(`PASS: fixture sound is served (audio ${useRealAudio ? 'REAL' : 'STUBBED'})`);

    await page.goto(`${baseUrl}/?url=/e2e-fixtures/sound-timer-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 60000, state: 'attached' });
    await waitUntilCanSendCommand();
    console.log('PASS: game booted');

    await checkCase('timeoutonly', 'control: SetTimeout with no sound', { expectSound: false });
    await checkCase('syncsoundthentimeout', 'synchronous sound, then SetTimeout (the reported case)', { expectSound: true });
    await checkCase('timeoutthensyncsound', 'SetTimeout armed, then synchronous sound spanning its due time', { expectSound: true });
    await checkCase('asyncsoundthentimeout', 'asynchronous sound, then SetTimeout', { expectSound: true });

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/wasmplayer-sound-timer-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
