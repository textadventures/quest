// Checks player UI v2's layout presets (#2613, PR 5) by playing fixtures/v2-layout-test.aslx (610)
// under ?ui=v2. The game sets layoutpreset to bottombar and narrowlayoutpreset to nopanes, which
// 610 Core's InitInterface passes to QuestUI.setLayoutPreset:
//
// - Wide, the bottom bar: the panes are a sheet opened from a button beside the command bar, the
//   exits strip and pinned status bar show, and the top bar has no panes button. The sheet
//   closes on Escape, back to its button.
// - Narrow, no panes: no panes, no panes buttons, no exits strip and no pinned status bar.
// - setLayoutPreset("sidepanes") from script gives the default layouts: a column when wide and
//   the drawer when narrow, following the width as the window is resized.
// - setLayoutPreset(wide, narrow) sets the two widths separately; an unknown preset changes
//   nothing.
// - axe-core finds no WCAG A/AA violations in the sheet and no-panes layouts.
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

    async function open(width) {
        const page = await browser.newPage({ viewport: { width, height: 700 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-layout-test.aslx&ui=v2`);
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('A hall.'), null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started on v2'); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
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
    // Where everything the layout moves around is, and whether it's on screen.
    const layout = (page) => page.evaluate(() => {
        const onScreen = (el) => {
            if (!el) return false;
            const style = getComputedStyle(el);
            const r = el.getBoundingClientRect();
            return style.display !== 'none' && style.visibility !== 'hidden' && r.width > 0 && r.height > 0
                && r.bottom > 0 && r.top < innerHeight && r.right > 0 && r.left < innerWidth;
        };
        const q = (sel) => document.querySelector(sel);
        const panes = q('[data-qv-part="panes"]');
        const output = q('[data-qv-part="output"]');
        return {
            layout: q('qv-player').dataset.qvLayout,
            panes: onScreen(panes),
            panesBeside: onScreen(panes) && panes.getBoundingClientRect().left >= output.getBoundingClientRect().right,
            topButton: onScreen(q('.qv-topbar .qv-panes-button')),
            bottomButton: onScreen(q('.qv-footer .qv-panes-button')),
            exits: onScreen(q('[data-qv-part="exits"]')),
            statusBar: onScreen(q('[data-qv-part="status-bar"]')),
            footerSticky: getComputedStyle(q('.qv-footer')).position === 'sticky',
        };
    });
    const settle = (page) => page.waitForTimeout(350);

    // ── Wide: the bottom bar, from the game's layoutpreset ──────────────────

    {
        const { page, errors } = await open(1200);
        let l = await layout(page);
        check(l.layout === 'sheet', `layoutpreset bottombar gives the sheet layout when wide (${l.layout})`);
        check(!l.panes && l.bottomButton && !l.topButton,
            'the panes are put away, with their button in the bottom bar rather than the top bar');
        check(l.exits && l.statusBar && l.footerSticky, 'the exits strip and pinned status bar show, and the bottom bar stays in view');

        await page.click('.qv-footer .qv-panes-button');
        await settle(page);
        l = await layout(page);
        const opened = await page.evaluate(() => ({
            expanded: document.querySelector('.qv-footer .qv-panes-button').getAttribute('aria-expanded'),
            focusInside: document.querySelector('[data-qv-part="panes"]').contains(document.activeElement),
            atBottom: Math.round(document.querySelector('[data-qv-part="panes"]').getBoundingClientRect().bottom) === innerHeight,
        }));
        check(l.panes && opened.expanded === 'true' && opened.focusInside && opened.atBottom,
            'its button opens the panes as a sheet from the bottom, with focus in it');
        await audit(page, 'with the sheet open');
        await page.keyboard.press('Escape');
        await settle(page);
        const closed = await page.evaluate(() => document.activeElement === document.querySelector('.qv-footer .qv-panes-button'));
        check(!(await layout(page)).panes && closed, 'Escape closes the sheet, back to its button');

        await send(page, 'sidepanes', 'Side panes.');
        l = await layout(page);
        check(l.layout === 'side' && l.panesBeside && !l.topButton && !l.bottomButton && !l.exits && !l.statusBar,
            'setLayoutPreset("sidepanes") puts the panes beside the game, with no strip or pinned bar');

        await send(page, 'nosuchpreset', 'Unknown preset.');
        check((await layout(page)).layout === 'side', 'an unknown preset changes nothing');

        await page.setViewportSize({ width: 390, height: 700 });
        await settle(page);
        l = await layout(page);
        check(l.layout === 'drawer' && !l.panes && l.topButton && l.exits && l.statusBar,
            'narrowing the window moves the side panes into the drawer');

        await send(page, 'widebottombar', 'Bottom bar when wide.');
        check((await layout(page)).layout === 'drawer', 'setLayoutPreset("bottombar", "sidepanes") keeps the drawer when narrow');
        await page.setViewportSize({ width: 1200, height: 700 });
        await settle(page);
        check((await layout(page)).layout === 'sheet', '…and gives the sheet once the window is wide');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow: no panes, from the game's narrowlayoutpreset ────────────────

    {
        const { page, errors } = await open(390);
        let l = await layout(page);
        check(l.layout === 'none' && !l.panes && !l.topButton && !l.bottomButton && !l.exits && !l.statusBar,
            `narrowlayoutpreset nopanes leaves out the panes and everything standing in for them (${l.layout})`);
        await audit(page, 'with no panes');

        await page.setViewportSize({ width: 1200, height: 700 });
        await settle(page);
        l = await layout(page);
        check(l.layout === 'sheet' && l.bottomButton, 'widening the window brings in the wide preset, the bottom bar');

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
