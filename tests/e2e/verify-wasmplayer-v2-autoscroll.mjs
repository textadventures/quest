// Checks player UI v2's autoscroll, with the checks legacy's verify-wasmplayer-autoscroll-*.mjs
// and verify-wasmplayer-output-image-loading.mjs make, on fixtures/v2-autoscroll-test.aslx (610)
// and fixtures/output-image-test.aslx (under ?ui=v2):
//
// - A burst of long turns ends with the last turn's start in view, not at the bottom of the page,
//   and the scroll position has settled by the time the game is ready.
// - A turn's start isn't left behind the sticky picture frame, including when the frame grows
//   after the turn has ended with the command input unfocused.
// - Clearing the screen after a wait doesn't keep the old page's scroll position, and on a small
//   screen the new page's start isn't left behind a tall frame.
// - Scrolling only ever moves forwards, and SetAutoScroll (false) leaves the position alone.
// - An image gets its real size when it's printed, and one slower than wasm-player.js's preload
//   timeout still ends up in view, with the command bar, once it loads.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { deflateSync, crc32 } from 'node:zlib';
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';
const frame = '[data-qv-part="picture-frame"]';
const resourceRoot = 'https://images.example.test/res/';
const FAST_DELAY_MS = 1000;
// Longer than wasm-player.js's IMAGE_PRELOAD_TIMEOUT_MS (3000).
const SLOW_DELAY_MS = 5000;
const IMAGE_WIDTH = 400;
const IMAGE_HEIGHT = 300;
const SETTLE_MS = 2000;

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

function pngChunk(type, data) {
    const length = Buffer.alloc(4);
    length.writeUInt32BE(data.length);
    const typeAndData = Buffer.concat([Buffer.from(type, 'ascii'), data]);
    const crc = Buffer.alloc(4);
    crc.writeUInt32BE(crc32(typeAndData));
    return Buffer.concat([length, typeAndData, crc]);
}

function makePng(width, height) {
    const ihdr = Buffer.alloc(13);
    ihdr.writeUInt32BE(width, 0);
    ihdr.writeUInt32BE(height, 4);
    ihdr[8] = 8; // bit depth
    ihdr[9] = 2; // colour type: RGB
    const row = Buffer.alloc(1 + width * 3, 0x80);
    row[0] = 0; // filter: none
    const raw = Buffer.concat(Array(height).fill(row));
    return Buffer.concat([
        Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
        pngChunk('IHDR', ihdr),
        pngChunk('IDAT', deflateSync(raw)),
        pngChunk('IEND', Buffer.alloc(0)),
    ]);
}

let browser;

async function open(viewport) {
    const page = await (await browser.newContext({ viewport })).newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-autoscroll-test.aslx`);
    await page.waitForSelector(input, { state: 'visible', timeout: 30000 });
    return page;
}

async function send(page, command) {
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
    await page.fill(input, command);
    await page.press(input, 'Enter');
}

async function sendAll(page, ...commands) {
    for (const command of commands) await send(page, command);
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
}

const rect = (page, selector) => page.$eval(selector, el => el.getBoundingClientRect().toJSON());
const scrollY = page => page.evaluate(() => window.scrollY);
const maxScrollY = page => page.evaluate(() => document.scrollingElement.scrollHeight - window.innerHeight);

try {
    browser = await chromium.launch();

    // A burst of long turns
    let page = await open({ width: 1024, height: 768 });
    await page.evaluate(() => {
        window.scrollSamples = [];
        setInterval(() => window.scrollSamples.push(window.scrollY), 50);
    });
    await sendAll(page, 'shout1', 'shout2', 'shout3', 'shout4', 'shout5', 'shout6');
    await page.waitForTimeout(SETTLE_MS);
    const header = await rect(page, '.qv-header');
    const turn6 = await rect(page, '#turnmark6');
    check(turn6.top >= header.bottom && turn6.top < 768, `the last turn's start is in view, below the header (at ${turn6.top})`);
    check(await scrollY(page) < await maxScrollY(page) - 200, 'the page stops well short of its bottom');
    const tail = (await page.evaluate(() => window.scrollSamples)).slice(-6);
    check(Math.max(...tail) - Math.min(...tail) < 5, 'the scroll position has settled');
    await page.context().close();

    // The picture frame, there from the start
    page = await open({ width: 1024, height: 768 });
    await sendAll(page, 'showframe', 'shout1', 'shout2');
    await page.waitForTimeout(SETTLE_MS);
    let frameRect = await rect(page, frame);
    let start = await rect(page, '#turnmark2');
    check(frameRect.height >= 200, 'the frame is showing');
    check(start.top >= frameRect.bottom && start.top < 768, `a turn's start isn't behind the frame (${start.top} below ${frameRect.bottom})`);
    await page.context().close();

    // The picture frame, growing after the turn with the command input unfocused
    page = await open({ width: 375, height: 500 });
    await send(page, 'shout1');
    await page.waitForSelector('#turnmark1');
    await page.waitForTimeout(SETTLE_MS);
    await page.evaluate(sel => document.querySelector(sel)?.blur(), input);
    await page.evaluate(() => QuestPlayerUis.v2.setPanelContents("<div style='height:150px;background:red;'>FRAME</div>"));
    await page.waitForTimeout(SETTLE_MS);
    frameRect = await rect(page, frame);
    start = await rect(page, '#turnmark1');
    check(start.top >= frameRect.bottom && start.top < 500, `a frame that grows later doesn't cover the turn's start (${start.top} below ${frameRect.bottom})`);
    await page.context().close();

    // Clearing the screen after a wait
    page = await open({ width: 375, height: 500 });
    await send(page, 'gopage');
    await page.waitForSelector('#page1mark');
    await page.click('qv-command-bar button:has-text("Continue")');
    await page.waitForSelector('#page2mark');
    await page.waitForTimeout(SETTLE_MS);
    check(await page.$('#page1mark') === null, 'the old page has gone');
    start = await rect(page, '#page2mark');
    check(start.top >= 0 && start.top < 500, `the new page's start is in view (at ${start.top})`);
    check(await scrollY(page) < 100, 'the short new page is scrolled back to the top');
    await page.context().close();

    // Clearing the screen under a tall frame
    page = await open({ width: 375, height: 500 });
    await sendAll(page, 'showframe', 'shout1', 'clearpage');
    await page.waitForSelector('#page2mark');
    await page.waitForTimeout(SETTLE_MS);
    frameRect = await rect(page, frame);
    start = await rect(page, '#page2mark');
    check(start.top >= frameRect.bottom && start.top < 500, `the new page's start isn't behind the frame (${start.top} below ${frameRect.bottom})`);
    await page.context().close();

    // Forwards only, and SetAutoScroll (false)
    page = await open({ width: 1024, height: 768 });
    await send(page, 'shout1');
    await page.waitForSelector('#turnmark1');
    await send(page, 'shout2');
    await page.waitForSelector('#turnmark2');
    await page.waitForTimeout(SETTLE_MS);
    const bottom = await maxScrollY(page);
    await page.evaluate(top => window.scrollTo({ top, behavior: 'instant' }), bottom);
    await page.evaluate(() => document.querySelector('qv-output').rescroll());
    await page.waitForTimeout(500);
    check(await scrollY(page) === bottom, 'a scroll never moves a player back up the page');
    await send(page, 'noscroll');
    await page.waitForTimeout(SETTLE_MS);
    await page.evaluate(() => window.scrollTo({ top: 100, behavior: 'instant' }));
    await send(page, 'shout3');
    await page.waitForSelector('#turnmark3');
    await page.waitForTimeout(SETTLE_MS);
    check(await scrollY(page) === 100, 'SetAutoScroll (false) leaves the scroll position alone');
    await page.context().close();

    // Images printed into the output, from a resourceRoot over the network
    const context = await browser.newContext({ viewport: { width: 500, height: 500 } });
    page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    const png = makePng(IMAGE_WIDTH, IMAGE_HEIGHT);
    // The player reports how far the game got (GameProgress); nothing to check here.
    await page.route('**/api/gameprogress/**', (route) => route.fulfill({ status: 204 }));
    await page.route('**/api/game/**', route => route.fulfill({
        contentType: 'application/json',
        body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/output-image-test.aslx`, resourceRoot }),
    }));
    await page.route(`${resourceRoot}**`, async route => {
        await new Promise(r => setTimeout(r, route.request().url().endsWith('slow.png') ? SLOW_DELAY_MS : FAST_DELAY_MS));
        await route.fulfill({ contentType: 'image/png', body: png });
    });
    await page.goto(`${baseUrl}/?id=output-image-test&ui=v2`);
    await page.waitForSelector(input, { state: 'visible', timeout: 30000 });
    await send(page, 'filler');
    await page.waitForFunction(() => document.querySelector('qv-output')?.textContent.includes('Filler line 40'));

    await send(page, 'fast');
    await page.waitForSelector('#afterfast', { timeout: 15000 });
    const printed = await page.$eval(`img[src="${resourceRoot}fast.png"]`, img => ({
        width: img.getAttribute('width'),
        height: img.getAttribute('height'),
        gap: document.getElementById('afterfast').getBoundingClientRect().top - img.getBoundingClientRect().top,
    }));
    check(printed.width === String(IMAGE_WIDTH) && printed.height === String(IMAGE_HEIGHT), 'an image has its real size as soon as it is printed');
    await page.waitForFunction(src => document.querySelector(`img[src="${src}"]`)?.complete, `${resourceRoot}fast.png`);
    const loadedGap = await page.$eval(`img[src="${resourceRoot}fast.png"]`, img =>
        document.getElementById('afterfast').getBoundingClientRect().top - img.getBoundingClientRect().top);
    check(Math.abs(printed.gap - loadedGap) <= 1, 'the text below it stays put when it loads');
    await page.waitForTimeout(SETTLE_MS);

    await send(page, 'slow');
    await page.waitForFunction(src => {
        const img = document.querySelector(`img[src="${src}"]`);
        return img?.complete && img.naturalWidth > 0;
    }, `${resourceRoot}slow.png`, { timeout: 20000 });
    await page.waitForTimeout(SETTLE_MS);
    const image = await rect(page, `img[src="${resourceRoot}slow.png"]`);
    const bar = await rect(page, input);
    check(image.height > 0 && image.bottom <= 500, `a slow image ends up in view (bottom at ${image.bottom})`);
    check(bar.bottom <= 500, `so does the command bar (bottom at ${bar.bottom})`);

    console.log('PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
