// Checks player UI v2's turns and timing, with the checks legacy's verify-wasmplayer-*.mjs scripts
// for these behaviours make, on fixtures/v2-timing-test.aslx (610) and, under ?ui=v2, the fixtures
// those scripts use, which have live Core:
//
// - aslevent-timer: a JavaScript countdown updates the custom status pane and calls ASLEvent once.
// - ui-call-throws: colour names and rgb() reach the background, and a UI call that throws is
//   reported without stopping the game.
// - wait-pause-input-flicker: the command input doesn't flash between chained waits or pauses, and
//   has focus again after the last one.
// - onready-recursive-getinput: the location bar keeps up inside a recursive get input loop.
// - sound-timer: SetTimeout's script runs on time around sounds, in order, with no further command.
// - walkthrough-timer: a timer tick doesn't land inside a walkthrough step, and a blocking wait in
//   a timer script is clicked through.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';
const continueButton = 'qv-command-bar .qv-continue';
const SOUND_MS = 400;
const BURSTS = 150;

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;

async function open(fixture, query = '&ui=v2', setUp) {
    const page = await browser.newPage();
    page.errors = [];
    page.on('console', m => { if (m.type() === 'error') page.errors.push(m.text()); });
    page.on('pageerror', e => page.errors.push(e.message));
    await setUp?.(page);
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}${query}`);
    await page.waitForSelector(input, { state: 'visible', timeout: 60000 });
    return page;
}

async function send(page, command) {
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
    await page.fill(input, command);
    await page.press(input, 'Enter');
}

const outputText = page => page.$eval('qv-output', el => el.innerText);
const hasOutput = (page, text) => page.waitForFunction(
    t => document.querySelector('qv-output').textContent.includes(t), text, { timeout: 15000 });
const isVisible = (page, selector) => page.$eval(selector, el => el.checkVisibility({ visibilityProperty: true }));
const inputHasFocus = (page, selector) => page.$eval(selector, el => el === document.activeElement);

async function countdown() {
    const page = await open('v2-timing-test.aslx', '');
    const status = page.locator('[data-qv-pane="customstatus"]');
    await send(page, 'startcountdown');
    await hasOutput(page, 'Countdown started.');
    await page.waitForFunction(() => document.querySelector('[data-qv-pane="customstatus"]').textContent.includes('Time remaining'));
    const samples = [];
    for (let i = 0; i < 6; i++) {
        samples.push(/Time remaining: \d+\.\d/.exec(await status.textContent())?.[0]);
        await page.waitForTimeout(150);
    }
    const ticking = samples.filter(Boolean);
    check(new Set(ticking).size >= 3, `the custom status pane counts down (${ticking.join(', ')})`);
    await hasOutput(page, 'Out of time! (timeout)');
    await page.waitForTimeout(1500);
    const finished = (await outputText(page)).split('Out of time! (timeout)').length - 1;
    check(finished === 1, 'ASLEvent reaches the game once, with its parameter');
    check((await status.textContent()).includes('Time remaining: 0.0'), 'the pane ends on 0.0');
    check(page.errors.length === 0, `no errors along the way ${page.errors.join('; ')}`);
    await page.close();
}

async function uiCallThrows() {
    const page = await open('v2-timing-test.aslx', '');
    const background = () => page.$eval('qv-player', el => el.style.getPropertyValue('--qv-bg'));
    await send(page, 'lightgray');
    await hasOutput(page, 'Background is now light gray.');
    await send(page, 'ping');
    await hasOutput(page, 'pong');
    check(await background() === 'LightGray', 'a colour name reaches the background token');
    await send(page, 'rgbbackground');
    await hasOutput(page, 'Background is now rgb.');
    check(await background() === 'rgb(10, 20, 30)', 'so does rgb()');
    await send(page, 'throwui');
    await hasOutput(page, 'Still accepting commands after a throwing UI call.');
    await send(page, 'ping');
    await page.waitForFunction(() => document.querySelector('qv-output').textContent.split('pong').length === 3, null, { timeout: 15000 });
    check(page.errors.some(e => e.includes('v2-timing-test')), 'a UI call that throws is reported, and the game carries on');
    await page.close();
}

// Records on every animation frame whether the command input is showing.
async function sampleInput(page, ms) {
    await page.evaluate(([selector, ms]) => {
        window.inputFrames = [];
        const start = performance.now();
        const sample = () => {
            window.inputFrames.push(document.querySelector(selector).checkVisibility({ visibilityProperty: true }));
            if (performance.now() - start < ms) requestAnimationFrame(sample);
        };
        requestAnimationFrame(sample);
    }, [input, ms]);
}

async function chainedWaits() {
    const page = await open('wait-pause-input-flicker-test.aslx');
    await send(page, 'gowait');
    await page.waitForSelector(`${continueButton}:not([hidden])`, { timeout: 10000 });
    check(!await isVisible(page, input), 'the command input is hidden during a wait');
    await sampleInput(page, 3000);
    await page.click(continueButton);
    await page.waitForSelector('#waitpage2', { state: 'attached', timeout: 10000 });
    await page.waitForTimeout(3200);
    const frames = await page.evaluate(() => window.inputFrames);
    check(frames.length >= 10 && !frames.includes(true), `it doesn't flash between chained waits (${frames.length} frames)`);
    check(await isVisible(page, continueButton), "the second wait's Continue button is showing");
    await page.click(continueButton);
    await page.waitForSelector('#waitpage3', { state: 'attached', timeout: 10000 });
    await page.waitForSelector(input, { state: 'visible', timeout: 10000 });
    check(await inputHasFocus(page, input), 'the input is back, with focus, after the last wait');

    await sampleInput(page, 3500);
    await send(page, 'gopause');
    await page.waitForSelector('#pausepage3', { state: 'attached', timeout: 10000 });
    await page.waitForTimeout(1000);
    const pauseFrames = await page.evaluate(() => window.inputFrames);
    const firstHidden = pauseFrames.indexOf(false);
    const lastHidden = pauseFrames.lastIndexOf(false);
    check(firstHidden >= 0 && lastHidden < pauseFrames.length - 1 && !pauseFrames.slice(firstHidden, lastHidden).includes(true), "it doesn't flash between chained pauses");
    await page.waitForSelector(input, { state: 'visible', timeout: 10000 });
    await page.waitForFunction(selector => document.activeElement === document.querySelector(selector), input, { timeout: 5000 });
    console.log('PASS: the input is back, with focus, after the last pause');
    await page.close();
}

async function recursiveGetInput() {
    const page = await open('onready-recursive-getinput-test.aslx');
    const locationBecomes = (text, what) => page.waitForFunction(
        t => document.querySelector('[data-qv-part="location"]').textContent.trim() === t, text, { timeout: 5000 })
        .then(() => console.log(`PASS: ${what}`));
    await locationBecomes('A RoomA', 'the location shows when the game starts');
    await send(page, 'startloop');
    await hasOutput(page, 'prompt');
    await send(page, 'b');
    await locationBecomes('A RoomB', 'it updates on a move from inside a recursive get input loop');
    await send(page, 'a');
    await locationBecomes('A RoomA', 'and on the next one');
    await send(page, 'stop');
    await page.close();
}

// Headless Chromium has no audio output, so 'ended' never fires. See verify-wasmplayer-sound-timer.mjs.
async function stubAudio(page) {
    await page.addInitScript(ms => {
        window.fakeAudioPlays = 0;
        window.Audio = class {
            constructor(src) { this.src = src; this.loop = false; this.paused = true; this.listeners = {}; }
            addEventListener(name, fn) { (this.listeners[name] ||= []).push(fn); }
            removeEventListener(name, fn) { this.listeners[name] = (this.listeners[name] ?? []).filter(f => f !== fn); }
            play() {
                window.fakeAudioPlays++;
                this.paused = false;
                if (!this.loop) {
                    this.timer = setTimeout(() => {
                        this.paused = true;
                        for (const fn of this.listeners.ended ?? []) fn();
                    }, ms);
                }
                return Promise.resolve();
            }
            pause() { this.paused = true; clearTimeout(this.timer); }
        };
    }, SOUND_MS);
}

async function soundsAndTimeouts() {
    const page = await open('sound-timer-test.aslx', '&ui=v2', stubAudio);
    const markers = ['A: before sound', 'B: after sound', 'C: timeout fired', 'D: nested output'];
    for (const [command, sounds] of [['timeoutonly', 0], ['syncsoundthentimeout', 1], ['timeoutthensyncsound', 1], ['asyncsoundthentimeout', 1]]) {
        const textBefore = (await outputText(page)).length;
        const playsBefore = await page.evaluate(() => window.fakeAudioPlays);
        await send(page, command);
        let text = '';
        const deadline = Date.now() + 15000;
        while (Date.now() < deadline) {
            text = (await outputText(page)).slice(textBefore);
            if (text.includes(markers[3])) break;
            await page.waitForTimeout(200);
        }
        const positions = markers.map(m => text.indexOf(m));
        check(!positions.includes(-1) && positions.every((p, i) => i === 0 || p > positions[i - 1]), `${command}: all four markers, in order, on the same turn`);
        const plays = await page.evaluate(() => window.fakeAudioPlays);
        check(plays - playsBefore === sounds, `${command}: ${sounds} sound(s) played`);
    }
    await page.close();
}

async function runWalkthrough(name) {
    const page = await open('walkthrough-timer-test.aslx');
    await page.evaluate(() => WebPlayer.setCanDebug(true));
    await page.click('#cmdDebug');
    await page.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });
    await page.click('#qv-debugger-tabs button:text("Walkthrough")');
    await page.click(`#qv-debugger-list [data-item="${name}"]`);
    await page.click(`[data-run-walkthrough="${name}"]`);
    await page.waitForFunction(() => {
        const text = document.querySelector('[data-walkthrough-status]')?.textContent;
        return text && text !== 'Running…';
    }, null, { timeout: 180000 });
    const status = await page.textContent('[data-walkthrough-status]');
    if (status !== 'Done') throw new Error(`${name}: walkthrough finished with "${status}"`);
    const lines = (await outputText(page)).split('\n').map(l => l.trim()).filter(Boolean);
    await page.close();
    return lines;
}

// Checks each block is whole and in order, and returns the timer lines between the blocks.
function checkBursts(name, lines, expected) {
    let open = null;
    let lastEnded = 0;
    const timerLines = [];
    for (const line of lines) {
        let m;
        if ((m = /^burst start (\d+)$/.exec(line))) {
            if (open !== null || Number(m[1]) !== lastEnded + 1) throw new Error(`${name}: burst ${m[1]} started out of order`);
            open = Number(m[1]);
        } else if ((m = /^burst end (\d+)$/.exec(line))) {
            if (open !== Number(m[1])) throw new Error(`${name}: burst ${m[1]} ended while ${open} was open`);
            open = null;
            lastEnded = Number(m[1]);
        } else if (/^(TICK \d+|timer (before|after) wait)$/.test(line)) {
            if (open !== null) throw new Error(`${name}: "${line}" was printed inside burst ${open}`);
            if (lastEnded > 0 && lastEnded < expected) timerLines.push(line);
        }
    }
    if (lastEnded !== expected) throw new Error(`${name}: only ${lastEnded} of ${expected} bursts completed`);
    return timerLines;
}

async function walkthroughs() {
    const ticks = checkBursts('bursts', await runWalkthrough('bursts'), BURSTS);
    check(ticks.length > 0, `all ${BURSTS} steps printed whole and in order, with ${ticks.length} timer ticks between them`);
    const timerLines = checkBursts('blockingtimer', await runWalkthrough('blockingtimer'), 30);
    check(timerLines.includes('timer before wait') && timerLines.includes('timer after wait'), 'a blocking wait in a timer script is clicked through');
}

try {
    browser = await chromium.launch({ args: ['--autoplay-policy=no-user-gesture-required'] });
    await countdown();
    await uiCallThrows();
    await chainedWaits();
    await recursiveGetInput();
    await soundsAndTimeouts();
    await walkthroughs();
    console.log('PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
