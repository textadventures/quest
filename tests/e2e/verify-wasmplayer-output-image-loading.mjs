// Regression test for images printed into the output by a game played from
// textadventures.co.uk (?id=), whose resources come from the API's
// resourceRoot over the network rather than out of a .quest package.
//
// #2303: an <img> printed before its image had downloaded rendered at 0x0,
// then shoved everything below it (a Continue link, say) down once it
// arrived. getResourceUrl now downloads the image first and playercore.js
// gives the <img> its real width/height, so the space is reserved even while
// the <img>'s own request is still in flight.
//
// #2304: an image still downloading when its turn scrolled left the page
// scrolled for a 0-height image, so the rest of it and the command bar ended
// up off the bottom of the screen. The output now re-scrolls once an image
// loads. Exercised with an image slower than the preload timeout, so it's
// printed unsized.
//
// Every image request is delayed via page.route.
//
// Requires the WasmPlayer dev server running locally:
//   node ../../src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';
import { deflateSync, crc32 } from 'node:zlib';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const resourceRoot = 'https://images.example.test/res/';
const FAST_DELAY_MS = 1000;
// Longer than wasm-player.js's IMAGE_PRELOAD_TIMEOUT_MS (3000).
const SLOW_DELAY_MS = 5000;
const IMAGE_WIDTH = 400;
const IMAGE_HEIGHT = 300;

function pngChunk(type, data) {
    const length = Buffer.alloc(4);
    length.writeUInt32BE(data.length);
    const typeAndData = Buffer.concat([Buffer.from(type, 'ascii'), data]);
    const crc = Buffer.alloc(4);
    crc.writeUInt32BE(crc32(typeAndData));
    return Buffer.concat([length, typeAndData, crc]);
}

// A solid grey RGB PNG of the given size.
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

const png = makePng(IMAGE_WIDTH, IMAGE_HEIGHT);
const browser = await chromium.launch();

async function sendCommand(page, command) {
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
}

try {
    const page = await browser.newPage({ viewport: { width: 500, height: 500 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.route('**/api/game/**', route => route.fulfill({
        contentType: 'application/json',
        body: JSON.stringify({
            sourceGameUrl: `${baseUrl}/e2e-fixtures/output-image-test.aslx`,
            resourceRoot,
        }),
    }));
    await page.route(`${resourceRoot}**`, async route => {
        const url = route.request().url();
        // never.png is left pending until the page closes.
        if (url.endsWith('never.png')) return;
        const delay = url.endsWith('slow.png') ? SLOW_DELAY_MS : FAST_DELAY_MS;
        await new Promise(r => setTimeout(r, delay));
        await route.fulfill({ contentType: 'image/png', body: png });
    });

    await page.goto(`${baseUrl}/?id=output-image-test`);
    await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });

    await sendCommand(page, 'filler');
    await page.waitForFunction(() => document.querySelector('#divOutput')?.textContent.includes('Filler line 40'));

    // #2303 — the <img> has its real size as soon as it's printed.
    await sendCommand(page, 'fast');
    await page.waitForSelector('#afterfast', { timeout: 15000 });
    const printed = await page.$eval(`img[src="${resourceRoot}fast.png"]`, img => ({
        width: img.getAttribute('width'),
        height: img.getAttribute('height'),
        sized: img.classList.contains('qv-sized'),
        rect: img.getBoundingClientRect().toJSON(),
        afterTop: document.getElementById('afterfast').getBoundingClientRect().top,
    }));
    console.log('fast.png as printed:', JSON.stringify(printed));
    if (printed.width !== String(IMAGE_WIDTH) || printed.height !== String(IMAGE_HEIGHT) || !printed.sized) {
        throw new Error(`Expected width/height ${IMAGE_WIDTH}x${IMAGE_HEIGHT} and .qv-sized on the printed <img>, got ${printed.width}x${printed.height}, sized=${printed.sized}`);
    }

    await page.waitForFunction(src => document.querySelector(`img[src="${src}"]`)?.complete, `${resourceRoot}fast.png`);
    const loaded = await page.$eval(`img[src="${resourceRoot}fast.png"]`, img => ({
        rect: img.getBoundingClientRect().toJSON(),
        afterTop: document.getElementById('afterfast').getBoundingClientRect().top,
    }));
    const printedGap = printed.afterTop - printed.rect.top;
    const loadedGap = loaded.afterTop - loaded.rect.top;
    console.log(`text below fast.png: ${printedGap}px below the image's top as printed, ${loadedGap}px once loaded`);
    if (Math.abs(printedGap - loadedGap) > 1) {
        throw new Error(`The text below fast.png moved by ${loadedGap - printedGap}px when the image loaded`);
    }
    await page.waitForTimeout(2500);

    // #2304 — slower than the preload timeout, so printed unsized; the page
    // must still end up scrolled to show all of it and the command bar.
    await sendCommand(page, 'slow');
    await page.waitForFunction(src => {
        const img = document.querySelector(`img[src="${src}"]`);
        return img?.complete && img.naturalWidth > 0;
    }, `${resourceRoot}slow.png`, { timeout: 20000 });
    // The re-scroll waits 100ms, then animates for up to 2s.
    await page.waitForTimeout(2500);
    const after = await page.evaluate(src => ({
        img: document.querySelector(`img[src="${src}"]`).getBoundingClientRect().toJSON(),
        input: document.getElementById('txtCommand').getBoundingClientRect().toJSON(),
        viewportHeight: window.innerHeight,
    }), `${resourceRoot}slow.png`);
    console.log('slow.png after loading:', JSON.stringify(after));
    if (after.img.height === 0) {
        throw new Error('slow.png never took up any space');
    }
    if (after.img.bottom > after.viewportHeight) {
        throw new Error(`slow.png runs ${after.img.bottom - after.viewportHeight}px off the bottom of the screen`);
    }
    if (after.input.bottom > after.viewportHeight) {
        throw new Error(`The command bar is ${after.input.bottom - after.viewportHeight}px off the bottom of the screen`);
    }

    // The preload usually leaves the image in the browser's memory cache, so
    // fast.png is often already complete when printed. Check the reserved box
    // with an image that never arrives at all instead. Done last, since
    // addText pins #divOutput's min-height at its new size, which would hide
    // the #2304 problem above.
    const unloaded = await page.evaluate(([url, w, h]) => {
        rememberImageSize(url, w, h);
        addText(`<img id="neverloads" src="${url}" />`);
        const img = document.getElementById('neverloads');
        const result = { complete: img.complete, rect: img.getBoundingClientRect().toJSON() };
        img.remove();
        return result;
    }, [`${resourceRoot}never.png`, IMAGE_WIDTH, IMAGE_HEIGHT]);
    console.log('never.png as printed:', JSON.stringify(unloaded));
    if (unloaded.complete) {
        throw new Error('never.png reported complete, so the test cannot tell whether its space was reserved');
    }
    const ratio = unloaded.rect.height / unloaded.rect.width;
    if (unloaded.rect.height === 0 || Math.abs(ratio - IMAGE_HEIGHT / IMAGE_WIDTH) > 0.02) {
        throw new Error(`Expected an unloaded sized <img> to reserve a ${IMAGE_WIDTH}:${IMAGE_HEIGHT} box, got ${unloaded.rect.width}x${unloaded.rect.height}`);
    }

    console.log('PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
