// Checks narrow screens and the replay test hooks on player UI v2, with the checks that apply there
// from legacy's mobile-exits-strip.spec.mjs, mobile-touch-targets, manual-clock-eval
// and seed-transcript, under ?ui=v2 on those scripts' fixtures, which have live Core:
//
// - The exits strip: in and out work from it, it's gone while a get input is pending so a tap
//   can't become the answer, it doesn't widen the page, and a game with its compass turned off
//   doesn't get one. v2-compass.spec.mjs covers the rest of the strip.
// - Touch targets: the top bar's buttons, the rows in the panes and the verb menu's items are at
//   least 44px, with 16px text. An author's larger menu font is kept. Nothing overhangs the
//   window, the text clears the top bar, and the open drawer stays inside the window and closes
//   when tapped outside.
// - ?clock=manual stops real-time timers, which then only move through QuestVivaTest's tick and
//   fireNextTimeout, and evaluate and assert work against the running game.
// - ?seed=N makes game randomness reproducible, and ?transcript=1 records the engine's text and
//   errors in order.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('v2-narrow-replay', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const input = 'qv-command-bar input';
    const PHONE = { width: 390, height: 844 };
    const MIN_TARGET = 44;
    const MIN_TEXT = 16;

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    let browser;

    async function open(url, viewport = PHONE) {
        const page = await browser.newPage({ viewport });
        page.on('pageerror', err => console.log('[pageerror]', err.message));
        await page.goto(`${baseUrl}/?url=${url}&ui=v2`);
        await page.waitForSelector(input, { state: 'visible', timeout: 60000 });
        return page;
    }

    async function send(page, command) {
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
        await page.fill(input, command);
        await page.press(input, 'Enter');
    }

    const output = page => page.$eval('qv-output', el => el.textContent);
    const hasOutput = (page, text, timeout = 15000) => page.waitForFunction(
        t => document.querySelector('qv-output').textContent.includes(t), text, { timeout });
    const stripExits = page => page.$$eval('[data-qv-part="exits"] .qv-exit', els =>
        els.filter(e => e.checkVisibility()).map(e => e.dataset.qvDirection));

    function stripBecomes(page, exits, what) {
        return page.waitForFunction(want => {
            const nav = document.querySelector('[data-qv-part="exits"]');
            const got = nav.checkVisibility() ? [...nav.querySelectorAll('.qv-exit')].map(e => e.dataset.qvDirection) : [];
            return JSON.stringify(got) === JSON.stringify(want);
        }, exits, { timeout: 10000 })
            .then(() => console.log(`PASS: ${what}`), async () => {
                throw new Error(`${what}: the strip shows ${JSON.stringify(await stripExits(page))}`);
            });
    }

    async function exitsStrip() {
        let page = await open('/e2e-fixtures/exits-strip-test.aslx');
        await hasOutput(page, 'You are in a hall');
        await stripBecomes(page, ['north', 'in'], 'the strip shows the exits, including in');
        check(await page.evaluate(() => document.scrollingElement.scrollWidth <= window.innerWidth), "it doesn't widen the page");
        await page.click('.qv-exit[data-qv-direction="in"]');
        await stripBecomes(page, ['out'], 'in works from the strip');
        await page.click('.qv-exit[data-qv-direction="out"]');
        await stripBecomes(page, ['north', 'in'], 'and so does out');
        await send(page, 'askme');
        await hasOutput(page, 'What is your name?');
        await stripBecomes(page, [], "it's gone while a get input is pending");
        await send(page, 'Bob');
        await hasOutput(page, 'Hello, Bob');
        await stripBecomes(page, ['north', 'in'], "and back once it's answered");
        await page.close();

        page = await open('/e2e-fixtures/exits-strip-compass-off-test.aslx');
        await hasOutput(page, 'You are in a hall');
        await page.waitForFunction(() => window.canSendCommand === true);
        await page.waitForTimeout(500);
        check(!await page.$eval('[data-qv-part="exits"]', el => el.checkVisibility()), 'a game with its compass turned off has no strip');
        await page.close();
    }

    async function touchTargets() {
        let page = await open('/e2e-fixtures/mobile-touch-targets-test.aslx');
        await page.waitForSelector('qv-output .elementmenu', { timeout: 15000 });
        const height = selector => page.$eval(selector, el => el.getBoundingClientRect().height);

        check(await height('.qv-topbar .qv-panes-button') >= MIN_TARGET, 'the panes button is a 44px target');
        check(await height('#cmdSave') >= MIN_TARGET, 'so is Save / Load');
        const layout = await page.evaluate(() => {
            const rect = selector => document.querySelector(selector).getBoundingClientRect();
            return {
                pageWidth: document.scrollingElement.scrollWidth,
                topBarRight: Math.max(...[...document.querySelectorAll('.qv-topbar *')]
                    .filter(el => el.checkVisibility()).map(el => el.getBoundingClientRect().right)),
                headerBottom: rect('.qv-header').bottom,
                textTop: rect('qv-output').top,
            };
        });
        check(layout.pageWidth <= PHONE.width && layout.topBarRight <= PHONE.width + 0.5, `nothing overhangs the window (top bar to ${layout.topBarRight}px)`);
        check(layout.textTop >= layout.headerBottom, 'the text clears the top bar');

        await page.click('.qv-topbar .qv-panes-button');
        await page.waitForTimeout(400);
        const drawer = await page.evaluate(() => {
            const panes = document.querySelector('[data-qv-part="panes"]').getBoundingClientRect();
            const items = [...document.querySelectorAll('[data-qv-part="panes"] .qv-pane-item')].filter(el => el.checkVisibility());
            return {
                top: panes.top,
                bottom: panes.bottom,
                left: panes.left,
                right: panes.right,
                minItem: Math.min(...items.map(el => el.getBoundingClientRect().height)),
                minFont: Math.min(...items.map(el => parseFloat(getComputedStyle(el).fontSize))),
                items: items.length,
            };
        });
        check(drawer.top >= -0.5 && drawer.bottom <= PHONE.height + 0.5 && drawer.left >= -0.5 && drawer.right <= PHONE.width + 0.5,
            `the open drawer stays inside the window (${Math.round(drawer.left)}-${Math.round(drawer.right)} across, ${Math.round(drawer.top)}-${Math.round(drawer.bottom)} down)`);
        check(drawer.items > 0 && drawer.minItem >= MIN_TARGET, `pane rows are 44px targets (${drawer.minItem}px)`);
        check(drawer.minFont >= MIN_TEXT, `with 16px text (${drawer.minFont}px)`);
        // A phone has no Escape key, so tapping outside has to close it.
        await page.mouse.click(10, 400);
        await page.waitForTimeout(400);
        check(await page.$eval('.qv-topbar .qv-panes-button', el => el.getAttribute('aria-expanded')) === 'false', 'tapping outside closes it');

        const verbMenu = async () => {
            await page.click('qv-output .elementmenu');
            await page.waitForSelector('qv-verb-menu [role="menuitem"]', { timeout: 5000 });
            const items = await page.$$eval('qv-verb-menu [role="menuitem"]', els => els.map(el => ({
                height: el.getBoundingClientRect().height,
                font: parseFloat(getComputedStyle(el).fontSize),
            })));
            await page.keyboard.press('Escape');
            return items;
        };
        const items = await verbMenu();
        check(items.every(i => i.height >= MIN_TARGET), `the verb menu's items are 44px targets (${items[0].height}px)`);
        check(items.every(i => i.font >= MIN_TEXT), `a game's default menu font is lifted to 16px (${items[0].font}px)`);
        await page.close();

        page = await open('/e2e-fixtures/mobile-touch-targets-large-menu-test.aslx');
        await page.waitForSelector('qv-output .elementmenu', { timeout: 15000 });
        await page.click('qv-output .elementmenu');
        await page.waitForSelector('qv-verb-menu [role="menuitem"]', { timeout: 5000 });
        const font = await page.$eval('qv-verb-menu [role="menuitem"]', el => parseFloat(getComputedStyle(el).fontSize));
        check(Math.abs(font - 20 * 4 / 3) < 0.5, `an author's larger menu font is kept (${font}px)`);
        await page.close();
    }

    async function manualClock() {
        const fixture = '/e2e-fixtures/manual-clock-eval-test.aslx';
        const page = await open(`${fixture}&clock=manual`, { width: 1280, height: 900 });
        await page.waitForFunction(() => window.QuestVivaTest && window.canSendCommand === true, null, { timeout: 30000 });
        for (const [expression, expected] of [['game.counter + 1', '8'], ['"a" + "b"', 'ab'], ['player.parent.name', 'room']]) {
            check(await page.evaluate(e => QuestVivaTest.evaluate(e), expression) === expected, `evaluate(${expression}) is ${expected}`);
        }
        check(await page.evaluate(() => QuestVivaTest.assert('game.counter = 7')) === true
            && await page.evaluate(() => QuestVivaTest.assert('game.counter = 8')) === false, 'assert returns true or false');
        const bad = await page.evaluate(() => QuestVivaTest.evaluate('nosuchobject.nosuchattribute').then(() => 'resolved', () => 'rejected'));
        check(bad === 'rejected', 'an invalid expression rejects');

        await send(page, 'delayed');
        await hasOutput(page, 'Timeout set.');
        await page.waitForTimeout(3500);
        check(!(await output(page)).includes('Timeout fired.'), "SetTimeout doesn't fire in real time");
        const fired = [];
        for (let i = 0; i < 5; i++) {
            if (!await page.evaluate(() => QuestVivaTest.fireNextTimeout())) break;
            fired.push((await output(page)).match(/Second timeout fired\.|Timeout fired\./g)?.length ?? 0);
        }
        check(JSON.stringify(fired) === '[1,2]', 'fireNextTimeout fires chained timeouts in order, then reports none left');

        await send(page, 'start beeper');
        await hasOutput(page, 'Beeper started.');
        await page.waitForTimeout(3000);
        check(!(await output(page)).includes('Beep 1'), "a timer doesn't fire in real time");
        check(await page.evaluate(() => QuestVivaTest.fireNextTimeout()) === false, "fireNextTimeout doesn't fire a timer");
        await page.evaluate(() => QuestVivaTest.tick(1));
        check(!(await output(page)).includes('Beep 1'), 'one second of a two-second timer does nothing');
        await page.evaluate(() => QuestVivaTest.tick(1));
        await hasOutput(page, 'Beep 1', 5000);
        check(await page.evaluate(() => QuestVivaTest.evaluate('game.beeps')) === '1', 'the second one fires it');
        await page.close();

        const realTime = await open(fixture, { width: 1280, height: 900 });
        await send(realTime, 'delayed');
        await hasOutput(realTime, 'Timeout fired.', 10000);
        console.log('PASS: without ?clock=manual, timers run in real time');
        await realTime.close();
    }

    async function seedAndTranscript() {
        const play = async query => {
            const page = await open(`/e2e-fixtures/seed-transcript-test.aslx${query}`, { width: 1280, height: 900 });
            await hasOutput(page, 'Start roll:', 30000);
            await send(page, 'roll');
            await hasOutput(page, 'Roll:');
            await send(page, 'broken');
            await hasOutput(page, 'Sorry, an error occurred');
            const rolls = [...(await output(page)).matchAll(/(?:Start roll|Roll): ([\d ]+)/g)].map(m => m[1].trim()).join(' | ');
            const transcript = await page.evaluate(() => QuestVivaTest.takeTranscript());
            const afterTake = await page.evaluate(() => QuestVivaTest.takeTranscript());
            await page.close();
            return { rolls, transcript, afterTake };
        };
        const first = await play('&seed=42&transcript=1');
        const second = await play('&seed=42&transcript=1');
        const other = await play('&seed=43&transcript=1');
        const plain = await play('');
        check(first.rolls === second.rolls, `the same seed gives the same rolls (${first.rolls})`);
        check(first.rolls !== other.rolls, `a different seed doesn't (${other.rolls})`);
        check(JSON.stringify(first.transcript) === JSON.stringify(second.transcript), 'or the same transcript');
        const chunks = first.transcript;
        const index = (type, text) => chunks.findIndex(c => c.type === type && c.text.includes(text));
        const positions = [index('text', 'Start roll:'), index('text', 'Roll:'), index('error', 'nosuchobject')];
        check(!positions.includes(-1) && positions[0] <= positions[1] && positions[1] < positions[2], 'the transcript has the text and the error, in order');
        check(!chunks.some(c => c.text.includes('Sorry, an error occurred')), "the player's own error text isn't in it");
        check(first.afterTake.length === 0, 'takeTranscript clears what it returned');
        check(plain.transcript.length === 0, 'nothing is recorded without ?transcript=');
    }

    try {
        browser = await chromium.launch();
        await exitsStrip();
        await touchTargets();
        await manualClock();
        await seedAndTranscript();
        console.log('PASS');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser?.close();
    }
});
