// Checks room pictures on player UI v2's map (#2627, PR 2; #2199) by playing
// fixtures/v2-map-pictures-test.aslx (610). The game is loaded as if from textadventures.co.uk
// (?id=, with the API mocked), so the hall's grid_image comes over the network from a resourceRoot:
//
// - 610 CoreGrid passes the picture's URL to QuestUI.map.drawBox, and v2 draws it over the hall's
//   fill, covering the room and cropped to it, under its border and label.
// - A room without a picture (the cellar) gets none.
// - Legacy ignores the new argument: the same game plays on ?ui=legacy without errors.
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

// A solid RGB PNG of the given size and colour.
function makePng(width, height, [r, g, b]) {
    const ihdr = Buffer.alloc(13);
    ihdr.writeUInt32BE(width, 0);
    ihdr.writeUInt32BE(height, 4);
    ihdr[8] = 8; // bit depth
    ihdr[9] = 2; // colour type: RGB
    const row = Buffer.alloc(1 + width * 3);
    for (let x = 0; x < width; x++) row.set([r, g, b], 1 + x * 3);
    const raw = Buffer.concat(Array(height).fill(row));
    return Buffer.concat([
        Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
        pngChunk('IHDR', ihdr),
        pngChunk('IDAT', deflateSync(raw)),
        pngChunk('IEND', Buffer.alloc(0)),
    ]);
}

// Wider than the hall (4×3 squares), so covering it means cropping the sides.
const hallPng = makePng(400, 200, [0x22, 0x88, 0x44]);

let browser;
try {
    browser = await chromium.launch();

    async function open(width, ui) {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        const requested = [];
        await page.route('**/api/game/**', (route) => route.fulfill({
            contentType: 'application/json',
            body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/v2-map-pictures-test.aslx`, resourceRoot }),
        }));
        await page.route(`${resourceRoot}**`, (route) => {
            requested.push(route.request().url());
            return route.fulfill({ contentType: 'image/png', body: hallPng });
        });
        await page.goto(`${baseUrl}/?id=v2-map-pictures-test&ui=${ui}`);
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && (document.querySelector('qv-output, #divOutput')?.textContent ?? '').includes('A hall.'), null, { timeout: 30000 })
            .catch(() => { throw new Error(`the game never started on ${ui}`); });
        await page.addScriptTag({ path: axePath });
        return { page, errors, requested };
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

    // Each level's rooms: its fill rectangle, any picture, and where they sit in the drawing order.
    const rooms = (page) => page.evaluate(() => Object.fromEntries(
        [...document.querySelectorAll('[data-qv-part="map"] svg g[data-z]:not(.qv-map-custom)')].map((level) => {
            const order = [...level.querySelectorAll('rect, image, path, text')];
            const rect = level.querySelector('rect');
            const image = level.querySelector('image');
            const label = level.querySelector('text');
            const border = level.querySelector('path');
            const attrs = (el) => el && Object.fromEntries(['x', 'y', 'width', 'height'].map((a) => [a, el.getAttribute(a)]));
            return [level.dataset.z, {
                label: label?.textContent ?? null,
                rect: attrs(rect),
                image: image ? { ...attrs(image), href: image.getAttribute('href'), aspect: image.getAttribute('preserveAspectRatio') } : null,
                imageAfterFill: !!image && order.indexOf(image) > order.indexOf(rect),
                imageUnderBorderAndLabel: !!image && order.indexOf(image) < order.indexOf(border) && order.indexOf(image) < order.indexOf(label),
            }];
        })));

    const drawn = (page) => page.waitForFunction(() => document.querySelector('[data-qv-part="map"] svg text'), null, { timeout: 10000 })
        .catch(() => { throw new Error('the map never drew'); });

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors, requested } = await open(1200, 'v2');
        await drawn(page);
        let r = await rooms(page);
        const hall = r['0'];
        check(hall.label === 'Hall' && hall.image?.href === `${resourceRoot}hall.png`,
            `the hall's picture is drawn from its URL (${hall.image?.href})`);
        check(JSON.stringify({ x: hall.image.x, y: hall.image.y, width: hall.image.width, height: hall.image.height }) === JSON.stringify(hall.rect)
            && hall.image.width === '160' && hall.image.height === '120',
            `it covers the room exactly (${hall.image.width}×${hall.image.height} at ${hall.image.x},${hall.image.y})`);
        check(hall.image.aspect === 'xMidYMid slice', 'it keeps its shape, cropped to the room');
        check(hall.imageAfterFill && hall.imageUnderBorderAndLabel, "it's over the room's fill, under its border and label");
        await page.waitForTimeout(300);
        check(requested.some((u) => u.endsWith('hall.png')), 'the picture was fetched');
        await audit(page, 'with a room picture');

        await send(page, 'down', 'A cellar.');
        r = await rooms(page);
        check(r['-1']?.label === 'Cellar' && r['-1'].image === null, 'a room without a picture gets none');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(390, 'v2');
        await drawn(page);
        check((await rooms(page))['0'].image?.href === `${resourceRoot}hall.png`, 'the picture is drawn on a narrow layout too');
        await audit(page, 'with a room picture, narrow');
        if (errors.length) throw new Error(`narrow: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Legacy ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200, 'legacy');
        await page.waitForFunction(() => {
            const panel = document.getElementById('gridPanel');
            return panel && getComputedStyle(panel).display !== 'none';
        }, null, { timeout: 10000 }).catch(() => { throw new Error("legacy's map never showed"); });
        check(errors.length === 0, `legacy plays the same game and ignores the picture argument (${errors.length} errors)`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
