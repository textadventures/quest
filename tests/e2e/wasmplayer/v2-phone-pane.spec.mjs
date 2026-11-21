// Checks the "phone" recipe in the panes docs (site/src/content/docs/customise/panes.md), playing
// fixtures/v2-phone-pane-test.aslx on v2:
//
// - The game's own pane, styled by its stylesheet, takes the place of the Inventory pane and the
//   compass, which it hides.
// - It lists what the player carries ("Nothing" at first) and the exits, and a turn script keeps
//   it up to date: taking the torch adds it.
// - Its exits are links: clicking one moves the player, and the pane then shows the new exits.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('v2-phone-pane', async ({ baseURL }) => {
    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    const browser = await chromium.launch();
    try {
        const page = await browser.newPage({ viewport: { width: 1200, height: 800 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });

        await page.goto(`${baseURL}/?url=/e2e-fixtures/v2-phone-pane-test.aslx`);
        await page.waitForFunction(() => !document.getElementById('qv-start')
            && document.querySelector('qv-output')?.textContent.includes('A hall.'), null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started'); });

        const ready = () => window.canSendCommand && !document.querySelector('qv-command-bar form').hidden;
        const phone = () => page.evaluate(() => {
            const pane = (name) => document.querySelector(`[data-qv-part=pane][data-qv-pane="${name}"]`);
            const element = pane('phone');
            const screen = element?.querySelector('.phone-screen');
            const lists = [...(screen?.querySelectorAll('ul') ?? [])];
            return {
                shown: !!element && element.checkVisibility(),
                background: element ? getComputedStyle(element).backgroundColor : null,
                carrying: lists[0]?.textContent.trim() ?? null,
                exits: [...(lists[1]?.querySelectorAll('a.cmdlink') ?? [])].map((a) => a.textContent.trim()),
                inventoryShown: !!pane('inventory')?.checkVisibility(),
                compassShown: !!pane('compass')?.checkVisibility(),
            };
        });

        let p = await phone();
        check(p.shown && p.background === 'rgb(28, 28, 30)', `the phone pane is shown, in the stylesheet's colours (${p.background})`);
        check(!p.inventoryShown && !p.compassShown, 'the Inventory pane and the compass are hidden');
        check(p.carrying === 'Nothing', `it says the player carries nothing (${p.carrying})`);
        check(p.exits.join(',') === 'north', `its exits are links (${p.exits})`);

        await page.waitForFunction(ready, null, { timeout: 10000 });
        await page.fill('qv-command-bar input', 'take torch');
        await page.press('qv-command-bar input', 'Enter');
        await page.waitForFunction(() => document.querySelector('[data-qv-pane="phone"] .phone-screen ul')?.textContent.includes('torch'),
            null, { timeout: 10000 })
            .catch(() => { throw new Error('the phone never listed the torch after taking it'); });
        p = await phone();
        check(p.carrying === 'torch', `the turn script updates it after taking the torch (${p.carrying})`);

        await page.waitForFunction(ready, null, { timeout: 10000 });
        await page.click('[data-qv-pane="phone"] .phone-exits a.cmdlink');
        await page.waitForFunction(() => document.querySelector('qv-output')?.textContent.includes('A kitchen.'), null, { timeout: 10000 })
            .catch(() => { throw new Error('clicking the exit in the phone never moved the player'); });
        await page.waitForFunction(() => document.querySelector('[data-qv-pane="phone"] .phone-exits')?.textContent.includes('south'),
            null, { timeout: 10000 })
            .catch(() => { throw new Error('the phone never showed the kitchen\'s exits'); });
        check(true, 'clicking an exit in the phone moves the player, and the phone shows the new exits');

        check(errors.length === 0, `no page errors (${errors.join(' | ')})`);
    } finally {
        await browser.close();
    }
});
