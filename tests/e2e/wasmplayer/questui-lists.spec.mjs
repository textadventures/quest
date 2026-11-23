// Checks QuestUI.lists() and the list events (inventoryChanged, objectsChanged, exitsChanged,
// statusChanged), playing fixtures/questui-lists-test.aslx on v2 and on legacy:
//
// - At the start, lists() has what the panes show, places and objects without the exits, and
//   each event has fired once with the same list.
// - A turn that changes nothing fires none of them, though the game sends its status every turn.
// - Taking an object fires inventoryChanged and objectsChanged.
// - A timer that changes the lists fires the events between turns.
// - lists() is a copy.
// - After loading a saved game, the game's .js file runs again and the events fire again with
//   the saved lists.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('questui-lists', async ({ baseURL }) => {
    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    const uis = {
        v2: {
            output: 'qv-output',
            input: 'qv-command-bar input',
            ready: () => window.canSendCommand && !document.querySelector('qv-command-bar form').hidden,
        },
        legacy: {
            output: '#divOutput',
            input: '#txtCommand',
            ready: () => window.canSendCommand,
        },
    };

    async function playOn(browser, ui) {
        const { output, input, ready } = uis[ui];
        const page = await browser.newPage({ viewport: { width: 1200, height: 800 } });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });

        await page.goto(`${baseURL}/?url=/e2e-fixtures/questui-lists-test.aslx&ui=${ui}`);
        await page.waitForFunction((sel) => !document.getElementById('qv-start') && document.querySelector(sel)?.textContent.includes('A hall.')
            && window.listEvents?.some((e) => e.name === 'statusChanged'), output, { timeout: 30000 })
            .catch(() => { throw new Error(`[${ui}] the game never started, or no list events came`); });

        async function waitForText(text, what) {
            await page.waitForFunction(([sel, t]) => document.querySelector(sel).textContent.includes(t), [output, text], { timeout: 10000 })
                .catch(() => { throw new Error(`[${ui}] ${what}: "${text}" never appeared`); });
        }
        async function type(command) {
            await page.waitForFunction(ready, null, { timeout: 10000 });
            await page.fill(input, command);
            await page.press(input, 'Enter');
        }
        const lists = () => page.evaluate(() => window.QuestUI.lists());
        const events = () => page.evaluate(() => window.listEvents);
        const clearEvents = () => page.evaluate(() => { window.listEvents = []; });
        const names = (items) => items.map((item) => item.name).join(',');
        const last = (record, name) => record.filter((e) => e.name === name).at(-1)?.detail;

        // At the start
        let l = await lists();
        let record = await events();
        const torch = l.objects.find((item) => item.name === 'torch');
        check(l.inventory.length === 0, `[${ui}] lists().inventory starts empty (${names(l.inventory)})`);
        check(!!torch && torch.elementId === 'torch' && torch.text.includes('torch') && torch.verbs.includes('Take'),
            `[${ui}] lists().objects has the torch, with its element ID, text and verbs (${JSON.stringify(torch)})`);
        check(!l.objects.some((item) => item.text === 'north'), `[${ui}] lists().objects leaves out the exits (${names(l.objects)})`);
        check(l.exits.join(',') === 'north', `[${ui}] lists().exits has the exits (${l.exits})`);
        check(l.status.length === 1 && l.status[0].text === 'Gold: 5', `[${ui}] lists().status has the status attributes (${JSON.stringify(l.status)})`);
        for (const name of ['inventoryChanged', 'objectsChanged', 'exitsChanged', 'statusChanged']) {
            check(record.filter((e) => e.name === name).length === 1, `[${ui}] ${name} fires once at the start`);
        }
        check(JSON.stringify(last(record, 'objectsChanged').items) === JSON.stringify(l.objects)
            && JSON.stringify(last(record, 'exitsChanged').exits) === JSON.stringify(l.exits),
            `[${ui}] the events carry the same lists as lists()`);

        const copied = await page.evaluate(() => {
            const first = window.QuestUI.lists();
            first.inventory.push({ name: 'fake' });
            first.exits.length = 0;
            return window.QuestUI.lists();
        });
        check(copied.inventory.length === 0 && copied.exits.length === 1, `[${ui}] lists() gives a copy`);

        // A turn that changes nothing
        await clearEvents();
        await type('look');
        await page.waitForFunction((sel) => (document.querySelector(sel).textContent.match(/A hall\./g) ?? []).length >= 2, output, { timeout: 10000 });
        await page.waitForFunction(ready, null, { timeout: 10000 });
        record = await events();
        check(record.every((e) => e.name === 'turnStarted'), `[${ui}] a turn that changes nothing fires no list events (${record.map((e) => e.name)})`);

        // Taking the torch
        await clearEvents();
        await type('take torch');
        await waitForText('You pick it up.', 'taking the torch');
        await page.waitForFunction(ready, null, { timeout: 10000 });
        record = await events();
        check(names(last(record, 'inventoryChanged')?.items ?? []) === 'torch', `[${ui}] taking the torch fires inventoryChanged with it`);
        check(!(last(record, 'objectsChanged')?.items ?? [{ name: 'torch' }]).some((item) => item.name === 'torch'),
            `[${ui}] and objectsChanged without it`);
        check(!record.some((e) => e.name === 'exitsChanged' || e.name === 'statusChanged'), `[${ui}] the exits and status didn't change`);

        // A timer
        await type('summon');
        await waitForText('Wait for it.', 'starting the timer');
        await page.waitForFunction(ready, null, { timeout: 10000 });
        await clearEvents();
        await page.waitForFunction(() => window.listEvents.some((e) => e.name === 'inventoryChanged'), null, { timeout: 10000 })
            .catch(() => { throw new Error(`[${ui}] the timer's coin never fired inventoryChanged`); });
        await page.waitForFunction(() => window.listEvents.some((e) => e.name === 'statusChanged'), null, { timeout: 5000 })
            .catch(() => { throw new Error(`[${ui}] the timer's gold never fired statusChanged`); });
        record = await events();
        check(names(last(record, 'inventoryChanged').items) === 'torch,coin', `[${ui}] a timer's change fires inventoryChanged (${names(last(record, 'inventoryChanged').items)})`);
        check(last(record, 'statusChanged').items[0]?.text === 'Gold: 6', `[${ui}] and statusChanged`);
        check(!record.some((e) => e.name === 'turnStarted'), `[${ui}] between turns`);

        // Save, drop the torch, load
        await page.waitForFunction(ready, null, { timeout: 10000 });
        await page.click('#cmdSave');
        await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
            .catch(() => { throw new Error(`[${ui}] Save / Load didn't open the host's dialog`); });
        await page.click('#qv-saves-save-new');
        await waitForText('Game saved.', 'saving');
        await page.evaluate(() => document.getElementById('qv-saves')?.close());

        await type('drop torch');
        await waitForText('You drop it.', 'dropping the torch');
        await page.waitForFunction(ready, null, { timeout: 10000 });
        check(names((await lists()).inventory) === 'coin', `[${ui}] dropping the torch changes lists()`);

        await page.click('#cmdSave');
        await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
            .catch(() => { throw new Error(`[${ui}] the save never appeared in the Save/Load list`); });
        await page.click('#qv-saves-list [data-slot]');
        // The page stays as it is through a load, so the record has to go, or what's in it would do.
        await page.evaluate(() => { delete window.listEvents; });
        await page.click('#qv-saves-confirm-yes');
        await page.waitForFunction((sel) => {
            const text = document.querySelector(sel)?.textContent ?? '';
            return !text.includes('You drop it.') && window.canSendCommand
                && window.listEvents?.some((e) => e.name === 'inventoryChanged');
        }, output, { timeout: 30000 })
            .catch(() => { throw new Error(`[${ui}] loading didn't bring back the game with its list events`); });
        record = await events();
        l = await lists();
        check(names(last(record, 'inventoryChanged').items) === 'torch,coin' && names(l.inventory) === 'torch,coin',
            `[${ui}] after loading, inventoryChanged fires again with the saved inventory`);
        check(last(record, 'exitsChanged')?.exits.join(',') === 'north' && last(record, 'statusChanged')?.items[0]?.text === 'Gold: 6',
            `[${ui}] and so do exitsChanged and statusChanged`);

        check(errors.length === 0, `[${ui}] no page errors (${errors.join(' | ')})`);
        await page.close();
    }

    const browser = await chromium.launch();
    try {
        await playOn(browser, 'v2');
        await playOn(browser, 'legacy');
    } finally {
        await browser.close();
    }
});
