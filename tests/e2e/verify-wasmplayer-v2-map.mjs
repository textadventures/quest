// Checks player UI v2's map (#2627, PR 1) by playing fixtures/v2-map-test.aslx (610) under ?ui=v2:
//
// - The map is drawn in SVG in the "map" part, under the picture frame in the sticky header, as
//   high as game.mapsize. Its renderer (v2/map.js) is loaded only once a game uses the map, and a
//   game without one never loads it.
// - Rooms are drawn with their labels as real text, and the player's dot is kept in the middle as
//   it moves. Going up a level fades the level below to 0.2.
// - A click sends the grid square to GridSquareClick; dragging pans without clicking; the wheel,
//   the zoom buttons and the keyboard zoom and pan.
// - The custom layer is an overlay: shown, it sits above the map (which stays visible), drawing a
//   room doesn't hide it, and a square redrawn under the same id replaces it. An SVG symbol's
//   script and event handlers never run.
// - setMapInteractive(false) stops panning and zooming but not clicks; setVisible("map", false)
//   hides the map.
// - Loading a save redraws the map the saved game had.
// - On a narrow layout the map takes at most 40% of the height and a top bar button collapses it;
//   under reduced motion the dot moves at once.
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

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(width, { fixture = 'v2-map-test', expect = 'A hall.', reducedMotion } = {}) {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        if (reducedMotion) await page.emulateMedia({ reducedMotion: 'reduce' });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}.aslx&ui=v2`);
        await page.waitForFunction((t) => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes(t), expect, { timeout: 30000 })
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

    const map = (page) => page.evaluate(() => {
        const region = document.querySelector('[data-qv-part="map"]');
        const svg = region?.querySelector('svg');
        const frame = document.querySelector('[data-qv-part="picture-frame"]');
        const box = svg?.getBoundingClientRect();
        const dot = svg?.querySelector('.qv-map-player')?.getBoundingClientRect();
        const viewport = svg?.querySelector(':scope > g');
        const custom = svg?.querySelector('.qv-map-custom');
        const button = document.querySelector('.qv-map-button');
        return {
            shown: !!region && !region.hidden && region.offsetHeight > 0,
            height: region?.offsetHeight ?? 0,
            inHeader: !!region?.closest('.qv-header'),
            afterFrame: !!frame && !!region && (frame.compareDocumentPosition(region) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0,
            role: svg?.getAttribute('role') ?? null,
            label: svg?.getAttribute('aria-label') ?? null,
            tabindex: svg?.getAttribute('tabindex') ?? null,
            labels: [...(svg?.querySelectorAll('g[data-z]:not(.qv-map-custom) text') ?? [])].map((t) => t.textContent),
            levels: Object.fromEntries([...(svg?.querySelectorAll('g[data-z]:not(.qv-map-custom)') ?? [])]
                .map((g) => [g.dataset.z, g.getAttribute('opacity')])),
            dot: dot && box ? { dx: dot.x + dot.width / 2 - (box.x + box.width / 2), dy: dot.y + dot.height / 2 - (box.y + box.height / 2) } : null,
            box: box ? { x: box.x, y: box.y, width: box.width, height: box.height } : null,
            transform: viewport?.getAttribute('transform') ?? '',
            custom: custom ? {
                display: custom.getAttribute('display'),
                squares: [...custom.querySelectorAll('rect')].length,
                squareText: [...custom.querySelectorAll('text')].map((t) => t.textContent),
                arrows: custom.querySelectorAll('line').length,
                polygons: custom.querySelectorAll('polygon').length,
                uses: custom.querySelectorAll('use').length,
                images: custom.querySelectorAll('image').length,
            } : null,
            zoomButtons: [...(region?.querySelectorAll('.qv-map-zoom button') ?? [])]
                .map((b) => ({ label: b.getAttribute('aria-label'), width: b.offsetWidth, height: b.offsetHeight })),
            button: button ? { shown: !button.hidden && button.offsetWidth > 0, expanded: button.getAttribute('aria-expanded') } : null,
        };
    });
    const zoomOf = (transform) => parseFloat(/scale\(([^)]+)\)/.exec(transform)?.[1] ?? 'NaN');
    // The dot settles in the middle once it has moved and the map has recentred.
    const settled = (page) => page.waitForFunction(() => {
        const svg = document.querySelector('[data-qv-part="map"] svg');
        const box = svg?.getBoundingClientRect();
        const dot = svg?.querySelector('.qv-map-player')?.getBoundingClientRect();
        return !!box && !!dot && Math.abs(dot.x + dot.width / 2 - (box.x + box.width / 2)) < 1.5
            && Math.abs(dot.y + dot.height / 2 - (box.y + box.height / 2)) < 1.5;
    }, null, { timeout: 5000 }).catch(() => { throw new Error("the player's dot never settled in the middle of the map"); });
    // Waits until no animation is moving the map: its transform is the same 300ms apart.
    const still = (page) => page.waitForFunction(() => {
        const read = () => document.querySelector('[data-qv-part="map"] svg > g')?.getAttribute('transform');
        const first = read();
        return new Promise((r) => setTimeout(() => r(read() === first), 300));
    }, null, { timeout: 5000 }).catch(() => { throw new Error('the map never stopped moving'); });
    const mapScriptLoads = (page) => page.evaluate(() => performance.getEntriesByType('resource')
        .filter((e) => new URL(e.name).pathname.endsWith('/v2/map.js')).length);
    const clicks = (page) => page.evaluate(() => (document.querySelector('qv-output').textContent.match(/Clicked/g) ?? []).length);

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        await page.waitForFunction(() => document.querySelector('[data-qv-part="map"] svg text'), null, { timeout: 20000 })
            .catch(async () => {
                const diag = await page.evaluate(() => {
                    const region = document.querySelector('[data-qv-part="map"]');
                    const svg = region?.querySelector('svg');
                    const resources = performance.getEntriesByType('resource')
                        .filter((e) => e.name.includes('map'))
                        .map((e) => e.name);
                    return {
                        hasRegion: !!region,
                        regionHidden: region?.hidden,
                        regionHeight: region?.offsetHeight,
                        hasSvg: !!svg,
                        textCount: svg?.querySelectorAll('text')?.length || 0,
                        mapScriptLoaded: !!window.QuestUIV2Map,
                        resources,
                    };
                });
                throw new Error(`the map never drew\n${JSON.stringify(diag, null, 2)}`);
            });
        await settled(page);
        let m = await map(page);
        check(m.shown && m.height === 300, `the map shows, as high as game.mapsize (${m.height}px)`);
        check(m.inHeader && m.afterFrame, 'the map is in the sticky header, under the picture frame');
        check(await mapScriptLoads(page) === 1, 'map.js loaded once');
        check(m.role === 'group' && m.label === 'Map', `the map is a labelled group (${m.role}, "${m.label}")`);
        check(m.labels.join() === 'Hall', `the hall is drawn with its label as text (${m.labels})`);
        check(m.zoomButtons.length === 2 && m.zoomButtons.every((b) => b.width >= 44 && b.height >= 44)
            && m.zoomButtons.map((b) => b.label).join() === 'Zoom in,Zoom out', 'zoom in and out buttons, 44px');
        check(!m.button.shown, 'no map button on a wide layout');
        await audit(page, 'with the map');

        await send(page, 'north', 'A library.');
        await settled(page);
        m = await map(page);
        check(m.labels.join() === 'Hall,Library', `the library is drawn after going north (${m.labels})`);
        check(m.dot && Math.abs(m.dot.dx) < 1.5 && Math.abs(m.dot.dy) < 1.5, "the map recentres on the player's dot");

        await send(page, 'south', 'A hall.');
        await send(page, 'up', 'An attic.');
        await settled(page);
        m = await map(page);
        check(m.levels['1'] === '1' && m.levels['0'] === '0.2', `going up fades the level below (${JSON.stringify(m.levels)})`);
        await send(page, 'down', 'A hall.');
        await settled(page);
        m = await map(page);
        check(m.levels['0'] === '1' && m.levels['1'] === '0.2', `coming back down fades the attic's level (${JSON.stringify(m.levels)})`);

        // A click in the middle is in the hall, squares 0 to 2 across and down.
        const centre = { x: m.box.x + m.box.width / 2, y: m.box.y + m.box.height / 2 };
        await page.mouse.click(centre.x, centre.y);
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Clicked 1,1.'), null, { timeout: 5000 })
            .catch(() => { throw new Error('clicking the map never reached GridSquareClick with 1,1'); });
        check(true, 'a click sends its grid square to GridSquareClick (1,1)');
        await page.waitForFunction(() => window.canSendCommand, null, { timeout: 5000 });

        const before = await clicks(page);
        await page.mouse.move(centre.x, centre.y);
        await page.mouse.down();
        await page.mouse.move(centre.x - 100, centre.y - 50, { steps: 5 });
        await page.mouse.up();
        await page.waitForTimeout(300);
        m = await map(page);
        check(Math.abs(m.dot.dx + 100) < 2 && Math.abs(m.dot.dy + 50) < 2, `dragging pans the map (dot now at ${Math.round(m.dot.dx)},${Math.round(m.dot.dy)})`);
        check(await clicks(page) === before, "a drag isn't a click");

        let zoom = zoomOf(m.transform);
        await page.mouse.move(centre.x, centre.y);
        await page.mouse.wheel(0, -200);
        await page.waitForTimeout(100);
        check(zoomOf((await map(page)).transform) > zoom, 'the wheel zooms in');
        zoom = zoomOf((await map(page)).transform);
        await page.click('.qv-map-zoom button[aria-label="Zoom out"]');
        check(zoomOf((await map(page)).transform) < zoom, 'the zoom out button zooms out');
        zoom = zoomOf((await map(page)).transform);
        await page.focus('[data-qv-part="map"] svg');
        await page.keyboard.press('+');
        check(zoomOf((await map(page)).transform) > zoom, '+ zooms in from the keyboard');
        const dx = (await map(page)).dot.dx;
        await page.keyboard.press('ArrowLeft');
        check((await map(page)).dot.dx > dx + 30, 'an arrow key pans the map');
        check(m.tabindex === '0', 'the map takes focus while it can be panned');
        await audit(page, 'with the map focused');

        // Dragging while the dot is still moving stops the map recentring on it afterwards.
        await send(page, 'north', 'A library.');
        const b = (await map(page)).box;
        await page.mouse.move(b.x + b.width / 2, b.y + b.height / 2);
        await page.mouse.down();
        await page.mouse.move(b.x + b.width / 2 + 60, b.y + b.height / 2, { steps: 3 });
        await page.mouse.up();
        const dragged = (await map(page)).transform;
        await page.waitForTimeout(600);
        check((await map(page)).transform === dragged, "a drag during the dot's move isn't undone by the map recentring");

        // ── Custom layer ──
        await send(page, 'custom', 'Custom drawn.');
        m = await map(page);
        check(m.custom.display === 'inline', 'showCustomLayer(true) shows the custom layer');
        check(m.labels.join() === 'Hall,Library,Attic', 'the map stays visible under it');
        check(m.custom.squares === 1 && m.custom.squareText.join() === 'X' && m.custom.arrows === 1 && m.custom.polygons === 1
            && m.custom.uses === 1 && m.custom.images === 1,
            `the square, arrow, shape, SVG symbol and image are on the custom layer (${JSON.stringify(m.custom)})`);
        const svgSafe = await page.evaluate(() => {
            const symbol = document.querySelector('[data-qv-part="map"] svg defs symbol');
            return { ran: window.svgScriptRan === true, script: !!symbol?.querySelector('script'), handler: !!symbol?.querySelector('[onclick]') };
        });
        check(!svgSafe.ran && !svgSafe.script && !svgSafe.handler, "an SVG symbol's script and handlers are removed, and never run");
        const imageBox = await page.evaluate(() => {
            const image = document.querySelector('.qv-map-custom image');
            return [image.getAttribute('x'), image.getAttribute('width')];
        });
        check(imageBox.join() === '15,15', `half-square positions and sizes aren't truncated (${imageBox})`);

        await send(page, 'redraw', 'Square redrawn.');
        m = await map(page);
        check(m.custom.squares === 1 && m.custom.squareText.join() === 'Y', 'a square redrawn under the same id replaces the old one');

        await send(page, 'south', 'A hall.');
        m = await map(page);
        check(m.custom.display === 'inline' && m.custom.arrows === 1,
            "drawing a room keeps the custom layer showing, and the room's exits go on the map, not on it");

        await send(page, 'hidecustom', 'Custom hidden.');
        check((await map(page)).custom.display === 'none', 'showCustomLayer(false) hides the custom layer');

        // ── Interaction off, then hidden ──
        await send(page, 'nointeract', 'Map fixed.');
        await settled(page);
        await still(page);
        m = await map(page);
        check(m.tabindex === null && m.zoomButtons.every((b) => b.width === 0), 'setMapInteractive(false) takes away focus and the zoom buttons');
        const transform = m.transform;
        const c = { x: m.box.x + m.box.width / 2, y: m.box.y + m.box.height / 2 };
        await page.mouse.move(c.x, c.y);
        await page.mouse.down();
        await page.mouse.move(c.x - 80, c.y, { steps: 5 });
        await page.mouse.up();
        await page.mouse.wheel(0, -200);
        await page.waitForTimeout(200);
        check((await map(page)).transform === transform, "with interaction off, dragging and the wheel don't move the map");
        const clicksBefore = await clicks(page);
        await page.mouse.click(c.x, c.y);
        await page.waitForFunction((n) => (document.querySelector('qv-output').textContent.match(/Clicked/g) ?? []).length > n,
            clicksBefore, { timeout: 5000 }).catch(() => { throw new Error('a click stopped reaching GridSquareClick with interaction off'); });
        check(true, 'clicks still reach GridSquareClick with interaction off');
        await page.waitForFunction(() => window.canSendCommand, null, { timeout: 5000 });

        await send(page, 'hidemap', 'Map hidden.');
        check(!(await map(page)).shown, 'setVisible("map", false) hides the map');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Restore ─────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        await send(page, 'north', 'A library.');
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
            .catch(() => { throw new Error("Save / Load didn't open the host's dialog"); });
        await page.click('#qv-saves-save-new');
        await page.waitForFunction(() => document.querySelector('qv-output').textContent.includes('Game saved.'), null, { timeout: 10000 });
        await page.evaluate(() => document.getElementById('qv-saves')?.close());

        await send(page, 'south', 'A hall.');
        await send(page, 'up', 'An attic.');
        check((await map(page)).labels.includes('Attic'), 'the attic is on the map after saving');

        await page.click('#cmdSave');
        await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
            .catch(() => { throw new Error('the save never appeared in the Save/Load list'); });
        await page.click('#qv-saves-list [data-slot]');
        await page.waitForFunction(() => !document.querySelector('qv-output').textContent.includes('An attic.') && window.canSendCommand,
            null, { timeout: 30000 })
            .catch(() => { throw new Error("loading didn't restore the saved game"); });
        await page.waitForFunction(() => document.querySelectorAll('[data-qv-part="map"] svg text').length === 2, null, { timeout: 10000 })
            .catch(() => { throw new Error('the map was never redrawn after loading'); });
        await settled(page);
        const m = await map(page);
        check(m.labels.join() === 'Hall,Library' && m.shown, `loading a save redraws the saved game's map (${m.labels})`);
        check(await mapScriptLoads(page) === 1, 'map.js is still only loaded once');

        if (errors.length) throw new Error(`restore: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow, with reduced motion ─────────────────────────────────────────

    {
        const { page, errors } = await open(390, { reducedMotion: true });
        await page.waitForFunction(() => document.querySelector('[data-qv-part="map"] svg text'), null, { timeout: 10000 })
            .catch(() => { throw new Error('the map never drew'); });
        let m = await map(page);
        check(m.shown && m.height === 280, `on a narrow layout the map takes at most 40% of the height (${m.height}px)`);
        check(m.button.shown && m.button.expanded === 'true', 'the top bar has a map button, expanded');
        await audit(page, 'with the map, narrow');

        await send(page, 'north', 'A library.');
        m = await map(page);
        check(Math.abs(m.dot.dx) < 1.5 && Math.abs(m.dot.dy) < 1.5, 'under reduced motion the dot is in its new room at once');

        await page.click('.qv-map-button');
        m = await map(page);
        check(!m.shown && m.button.expanded === 'false', 'the map button collapses the map');
        await audit(page, 'with the map collapsed, narrow');
        await page.click('.qv-map-button');
        m = await map(page);
        check(m.shown && m.button.expanded === 'true', 'and expands it again');
        await settled(page);

        if (errors.length) throw new Error(`narrow: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── A game without a map ────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200, { fixture: 'v2-core-test', expect: 'A plain hall' });
        const m = await map(page);
        check(!m.shown && !m.button.shown, "a game without a map shows neither the map nor its button");
        check(await mapScriptLoads(page) === 0, 'and never loads map.js');
        if (errors.length) throw new Error(`no map: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
