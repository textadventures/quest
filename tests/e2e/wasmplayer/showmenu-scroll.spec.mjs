// Checks that choosing a ShowMenu option scrolls its response into view, on
// fixtures/showmenu-scroll-test.aslx, in both player UIs. The option links send an event, not a
// command, and used not to mark where the new turn's output would start: the turn start stayed
// at the command that opened the menu, which was already in view, so nothing scrolled and the
// response was left below the window.
//
// The engine now scrolls at the end of an event as at the end of a turn. Answering also hides the
// menu's options, above where the response starts, and the scroll has to allow for that or it
// overshoots the response by their height: so the response's start has to end up near the top of
// the window, not merely somewhere in view. Checked for:
//
// - a ShowMenu option (data-qv-event link), on v2 and legacy;
// - an old-style onclick="ASLEvent(...)" link, as games published before 6.1 carry in their own
//   copy of ShowMenu, on legacy;
// - typing the option's number, a command rather than an event, on v2 and legacy.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('showmenu-scroll', async ({ baseURL }) => {
    const VIEWPORT = { width: 1024, height: 500 };
    const SETTLE_MS = 2500;

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    const browser = await chromium.launch();

    // Clicks with el.click() rather than page.click, which would scroll the link into view first
    // and so hide whether the player scrolls by itself. top is where the window's content starts,
    // below v2's sticky header.
    async function run(ui, input, choice, top) {
        const page = await (await browser.newContext({ viewport: VIEWPORT })).newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));
        const label = `[${ui} ${choice}]`;
        try {
            await page.goto(`${baseURL}/?url=/e2e-fixtures/showmenu-scroll-test.aslx&ui=${ui}`);
            await page.waitForSelector(input, { state: 'visible', timeout: 30000 });
            await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
            await page.fill(input, 'talk');
            await page.press(input, 'Enter');
            await page.waitForSelector('#oldstylelink', { state: 'attached', timeout: 15000 });
            await page.waitForTimeout(SETTLE_MS);

            const before = await page.evaluate(() => window.scrollY);
            if (choice === 'typed') {
                await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
                await page.fill(input, '1');
                await page.press(input, 'Enter');
            } else {
                await page.$eval(choice, el => el.click());
            }
            await page.waitForSelector('#responsemark', { timeout: 15000 });
            await page.waitForTimeout(SETTLE_MS);

            const contentTop = await top(page);
            const responseTop = await page.$eval('#responsemark', el => el.getBoundingClientRect().top);
            const after = await page.evaluate(() => window.scrollY);
            check(after > before, `${label} the page scrolled after choosing (${before} -> ${after})`);
            check(responseTop >= contentTop && responseTop < contentTop + 60,
                `${label} the response's start is at the top of the window (at ${responseTop}, content from ${contentTop})`);
        } finally {
            await page.context().close();
        }
    }

    const v2Top = page => page.$eval('.qv-header', el => el.getBoundingClientRect().bottom);
    const legacyTop = async () => 0;

    try {
        const option = 'a[data-qv-event="ShowMenuResponse"][data-qv-param="a"]';
        await run('v2', 'qv-command-bar input', option, v2Top);
        await run('v2', 'qv-command-bar input', 'typed', v2Top);
        await run('legacy', '#txtCommand', option, legacyTop);
        await run('legacy', '#txtCommand', '#oldstylelink', legacyTop);
        await run('legacy', '#txtCommand', 'typed', legacyTop);
    } finally {
        await browser.close();
    }
});
