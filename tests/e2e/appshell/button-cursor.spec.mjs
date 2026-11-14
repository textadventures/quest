// Verifies enabled buttons show the hand (pointer) cursor and disabled ones
// show not-allowed, on both the Play (/) and Create (/open) tabs.
// Tailwind v4's preflight gives buttons the default arrow cursor; Skeleton 4's
// globals.css put the pointer back, but Skeleton 5 dropped that rule, so
// app.css now restores it itself. This catches that rule going missing again.
// Run against a dev server started with:
//   npm --prefix src/AppShell run dev -- --port 5180
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('button-cursor', async ({ baseURL }) => {
    const baseUrl = baseURL;

    const browser = await chromium.launch();
    const page = await browser.newPage({ viewport: { width: 1200, height: 800 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    async function checkCursors(path, waitForSelector) {
        await page.goto(baseUrl + path);
        await page.waitForSelector(waitForSelector, { timeout: 30000 });
        const buttons = await page.$$eval('button', bs => bs
            .filter(b => b.offsetParent !== null)
            .map(b => ({
                label: b.getAttribute('aria-label') || b.title || b.textContent.trim().slice(0, 30),
                disabled: b.disabled,
                cursor: getComputedStyle(b).cursor,
            })));
        if (!buttons.some(b => !b.disabled)) throw new Error(`[${path}] no enabled buttons found to check`);
        for (const b of buttons) {
            const expected = b.disabled ? 'not-allowed' : 'pointer';
            if (b.cursor !== expected) {
                throw new Error(`[${path}] button "${b.label}" (disabled=${b.disabled}) has cursor ${b.cursor} (expected ${expected})`);
            }
        }
        console.log(`PASS: [${path}] ${buttons.length} visible buttons have the expected cursor`);
    }

    async function run() {
        await checkCursors('/', 'button[aria-label="Settings"], button[title="Settings"]');
        await checkCursors('/open', 'button:has-text("Open game folder")');
        console.log('PASS');
    }

    try {
        await run();
    } catch (err) {
        console.error('FAIL:', err.message);
        await page.screenshot({ path: '/tmp/appshell-button-cursor-failure.png' });
        throw err;
    } finally {
        await browser.close();
    }
});
