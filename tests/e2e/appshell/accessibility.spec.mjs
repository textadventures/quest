// Checks the editor's main views with axe-core's WCAG 2.0-2.2 A and AA rules (contrast, names,
// roles...), in light and dark mode (#2948): the Play and Create tabs, the Settings dialog, and in
// the editor the game and a room, a script in the visual and code views, and Raw XML code view.
// axe finds only some problems; the rest need a screen reader and the keyboard.
//
// Requires the AppShell dev server running:
//   ./dev.sh
import { test } from '@playwright/test';
import { createRequire } from 'node:module';
import { chromium } from '../lib/tracked-chromium.mjs';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');

for (const colorScheme of ['light', 'dark']) {
    test(`accessibility (${colorScheme})`, async ({ baseURL }) => {
        const browser = await chromium.launch();
        const found = [];
        try {
            const ctx = await browser.newContext({ colorScheme, viewport: { width: 1280, height: 800 } });
            const page = await ctx.newPage();
            page.on('pageerror', err => console.log('[pageerror]', err.message));

            // Records each WCAG A/AA violation axe finds, so one run lists every state's.
            async function audit(state) {
                // A colour part-way through a transition (a button fading in as it's enabled)
                // would otherwise be measured
                await page.evaluate(() => Promise.allSettled(document.getAnimations()
                    .filter(a => a.effect?.getTiming().iterations !== Infinity).map(a => a.finished)));
                if (!await page.evaluate(() => 'axe' in window)) await page.addScriptTag({ path: axePath });
                const violations = await page.evaluate(async () => {
                    const result = await window.axe.run(
                        // The game in the preview frame is the author's, not the editor's
                        { exclude: [['iframe']] },
                        {
                            runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
                            // Not yet: maximum-scale=1 stops iOS zooming in when a text box with
                            // text under 16px gets the focus, but also stops pinch zoom on Android
                            rules: { 'meta-viewport': { enabled: false } },
                        });
                    return result.violations.map(v => `${v.id} (${v.impact}): ${v.help}\n${v.nodes.slice(0, 8).map(n =>
                        `        ${n.target.join(' ')}: ${(n.failureSummary ?? '').split('\n').slice(1).join(' ').trim()}`).join('\n')}${v.nodes.length > 8 ? `\n        ...and ${v.nodes.length - 8} more` : ''}`);
                });
                for (const v of violations) found.push(`[${state}] ${v}`);
                console.log(`${violations.length ? 'FAIL' : 'PASS'}: ${state}${violations.length ? ` (${violations.length} rules)` : ''}`);
            }

            // Play tab: the catalog comes from textadventures.co.uk, so check the page as it is
            // once the header is up, whether or not the catalog has arrived
            await page.goto(`${baseURL}/`);
            await page.waitForSelector('button.home-header-link', { timeout: 30000 });
            await audit('Play tab');

            await page.click('button.home-header-link');
            await page.waitForSelector('#settings-theme', { timeout: 10000 });
            await audit('Settings dialog');
            await page.keyboard.press('Escape');
            await page.waitForSelector('#settings-theme', { state: 'detached', timeout: 5000 });

            await page.goto(`${baseURL}/open`);
            await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
            await page.fill('input[placeholder="Game name"]', `Accessibility ${colorScheme}`);
            await page.waitForSelector('text=Text adventure', { timeout: 10000 });
            await audit('Create tab');

            await page.click('button:has-text("Create local draft")');
            await page.waitForSelector('button[title="More"]', { timeout: 60000 });
            await audit('editor, game');

            await page.locator('[data-value="game"][data-part="branch-control"]').click();
            await page.getByRole('button', { name: /^Scripts$/ }).click();
            await page.waitForSelector('text=Start script', { timeout: 5000 });
            await page.click('button:has-text("Add script")');
            await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
            await audit('Add script dialog');
            await page.getByRole('button', { name: 'Print', exact: true }).click();
            await page.waitForSelector('[role="dialog"]', { state: 'detached', timeout: 5000 });
            await audit('editor, script');

            await page.click('button:has-text("Code view")');
            await page.waitForSelector('.cm-editor', { timeout: 5000 });
            await page.locator('.cm-content').first().click();
            await page.keyboard.insertText('if (game.pov.parent = room and 1 < 2) {\n  msg ("Hello " + player.alias)\n}\n// a comment\n');
            await audit('script code view');
            await page.click('button:has-text("Visual editor")');

            await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
            await page.waitForSelector('[data-value="room"][data-selected]', { timeout: 5000 });
            await audit('editor, room');

            await page.click('button[title="Raw XML code view"]');
            await page.waitForSelector('.cm-editor', { timeout: 5000 });
            await audit('Raw XML code view');
        } catch (err) {
            if (found.length) console.log(`Violations found before the failure:\n${found.join('\n')}`);
            throw err;
        } finally {
            await browser.close();
        }
        if (found.length) throw new Error(`axe found WCAG violations in ${colorScheme} mode:\n${found.join('\n')}`);
    });
}
