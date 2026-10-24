// Checks player UI v2's picture frame (#2613, PR 4) by playing fixtures/v2-picture-frame-test.aslx
// (610) under ?ui=v2. The game is loaded as if from textadventures.co.uk (?id=, with the API
// mocked), so its pictures come over the network from a resourceRoot, where page.route serves the
// cellar's picture slower than wasm-player.js's preload waits:
//
// - The room's picture (SetFramePicture) shows in the picture-frame part, above the output and
//   part of the sticky header, marked decorative.
// - A frame picture that's still loading holds the turn's scroll back, and once it arrives the
//   turn's first line is in view just below the frame rather than behind it (legacy's
//   setPanelHeight).
// - A room with no picture empties the frame (clearframe). The engine's SetPanelContents request
//   sets it too, and QuestUI.setVisible("picture-frame", false) hides it.
// - Loading a save brings back the frame the saved game had.
// - On a narrow layout the picture fits the width and takes at most half the height.
// - axe-core finds no WCAG A/AA violations at either width.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { createRequire } from 'node:module';
import { deflateSync, crc32 } from 'node:zlib';
import { chromium } from 'playwright';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');
const baseUrl = process.argv[2] || 'http://localhost:5175';
const resourceRoot = 'https://images.example.test/res/';
// Longer than wasm-player.js's IMAGE_PRELOAD_TIMEOUT_MS (3000), so the picture is still loading
// when it's put in the frame.
const SLOW_DELAY_MS = 5000;
const input = 'qv-command-bar input';

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

// A solid RGB PNG of the given size and grey level.
function makePng(width, height, grey) {
    const ihdr = Buffer.alloc(13);
    ihdr.writeUInt32BE(width, 0);
    ihdr.writeUInt32BE(height, 4);
    ihdr[8] = 8; // bit depth
    ihdr[9] = 2; // colour type: RGB
    const row = Buffer.alloc(1 + width * 3, grey);
    row[0] = 0; // filter: none
    const raw = Buffer.concat(Array(height).fill(row));
    return Buffer.concat([
        Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
        pngChunk('IHDR', ihdr),
        pngChunk('IDAT', deflateSync(raw)),
        pngChunk('IEND', Buffer.alloc(0)),
    ]);
}

const hallPng = makePng(400, 300, 0x80);
const cellarPng = makePng(400, 300, 0x40);

let browser;
try {
    browser = await chromium.launch();

    async function open(width) {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.route('**/api/game/**', (route) => route.fulfill({
            contentType: 'application/json',
            body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/v2-picture-frame-test.aslx`, resourceRoot }),
        }));
        await page.route(`${resourceRoot}**`, async (route) => {
            const url = route.request().url();
            if (url.endsWith('cellar.png')) {
                await new Promise((r) => setTimeout(r, SLOW_DELAY_MS));
                await route.fulfill({ contentType: 'image/png', body: cellarPng });
            } else {
                await route.fulfill({ contentType: 'image/png', body: hallPng });
            }
        });
        await page.goto(`${baseUrl}/?id=v2-picture-frame-test&ui=v2`);
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('A hall.'), null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started on v2'); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    // A turn's output is on screen just before canSendCommand comes back, so wait for both.
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
    const frame = (page) => page.evaluate(() => {
        const el = document.querySelector('[data-qv-part="picture-frame"]');
        const img = el?.querySelector('img');
        const output = document.querySelector('[data-qv-part="output"]');
        const header = el?.closest('.qv-header');
        return {
            shown: !!el && el.offsetHeight > 0,
            html: el?.innerHTML ?? '',
            src: img?.getAttribute('src') ?? null,
            alt: img?.getAttribute('alt') ?? null,
            loaded: !!img && img.complete && img.naturalWidth > 0,
            imgHeight: img?.getBoundingClientRect().height ?? 0,
            imgWidth: img?.getBoundingClientRect().width ?? 0,
            frameWidth: el?.getBoundingClientRect().width ?? 0,
            aboveOutput: !!el && !!output && (el.compareDocumentPosition(output) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0,
            inStickyHeader: !!header && getComputedStyle(header).position === 'sticky',
            headerBottom: header?.getBoundingClientRect().bottom ?? 0,
        };
    });
    const frameLoaded = (page, name) => page.waitForFunction((n) => {
        const img = document.querySelector('[data-qv-part="picture-frame"] img');
        return img?.getAttribute('src')?.endsWith(n) && img.complete && img.naturalWidth > 0;
    }, name, { timeout: 15000 }).catch(() => { throw new Error(`${name} never loaded in the frame`); });

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        await frameLoaded(page, 'hall.png');
        let f = await frame(page);
        check(f.shown && f.src === `${resourceRoot}hall.png`, `the room's picture shows in the frame (${f.src})`);
        check(f.alt === '', 'the frame picture is marked decorative');
        check(f.aboveOutput && f.inStickyHeader, 'the frame is above the output, in the sticky header');
        check(f.imgHeight === 300, `the picture is drawn at its own size when it fits (${f.imgHeight}px tall)`);
        await audit(page, 'with a picture in the frame');

        // Scroll the page well down, so the next turn has somewhere to scroll to.
        await send(page, 'filler', 'Filler line 40');
        await page.waitForTimeout(800);

        // The turn starts where the output's text ends now (its echo of the command comes first).
        const turnStart = await page.evaluate(() => {
            const output = document.querySelector('qv-output');
            return output.lastElementChild.getBoundingClientRect().bottom - output.getBoundingClientRect().top;
        });
        await send(page, 'north', 'Cellar line 40');
        f = await frame(page);
        check(f.src === `${resourceRoot}cellar.png` && !f.loaded, "the cellar's picture is still loading after its turn");
        const before = await page.evaluate(() => window.scrollY);
        await page.waitForTimeout(800);
        const during = await page.evaluate(() => window.scrollY);
        check(during === before, `the turn doesn't scroll while the frame's picture is loading (${before} → ${during})`);

        await frameLoaded(page, 'cellar.png');
        // Let the smooth scroll that follows the picture start, then settle.
        await page.waitForTimeout(300);
        await page.waitForFunction(() => {
            const y = window.scrollY;
            return new Promise((r) => setTimeout(() => r(window.scrollY === y), 300));
        }, null, { timeout: 5000 });
        f = await frame(page);
        const start = await page.evaluate((offset) => document.querySelector('qv-output').getBoundingClientRect().top + offset, turnStart);
        check(f.imgHeight > 200 && start >= f.headerBottom && start < f.headerBottom + 30,
            `once it loads, the turn's start is just below the frame (at ${Math.round(start)}px, frame ends at ${Math.round(f.headerBottom)}px)`);

        await send(page, 'east', 'A garden.');
        check(!(await frame(page)).shown, 'a room with no picture empties the frame (clearframe)');

        await send(page, 'panel', 'Panel set.');
        f = await frame(page);
        check(f.shown && f.html.includes('A custom panel.'), "the engine's SetPanelContents request sets the frame");

        await send(page, 'hideframe', 'Frame hidden.');
        check(!(await frame(page)).shown, 'QuestUI.setVisible("picture-frame", false) hides it');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Restore ─────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        await frameLoaded(page, 'hall.png');
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
            .catch(() => { throw new Error("Save / Load didn't open the host's dialog"); });
        await page.click('#qv-saves-save-new');
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 10000 });
        await page.evaluate(() => document.getElementById('qv-saves')?.close());

        await send(page, 'panel', 'Panel set.');
        check((await frame(page)).html.includes('A custom panel.'), 'the frame changes after saving');

        await page.click('#cmdSave');
        await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
            .catch(() => { throw new Error('the save never appeared in the Save/Load list'); });
        await page.click('#qv-saves-list [data-slot]');
        await page.waitForFunction(() => !document.querySelector('qv-output').textContent.includes('Panel set.') && window.canSendCommand,
            null, { timeout: 30000 })
            .catch(() => { throw new Error("loading didn't restore the saved game"); });
        await frameLoaded(page, 'hall.png');
        check(!(await frame(page)).html.includes('A custom panel.'), 'loading a save brings back the frame the saved game had');

        if (errors.length) throw new Error(`restore: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(390);
        await frameLoaded(page, 'hall.png');
        const f = await frame(page);
        check(f.shown && f.imgWidth <= f.frameWidth && f.imgHeight <= 350,
            `on a narrow layout the picture fits the width and at most half the height (${Math.round(f.imgWidth)}×${Math.round(f.imgHeight)})`);
        await audit(page, 'with a picture in the frame, narrow');
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
