// Checks player UI v2's links (#2595) by playing fixtures/v2-links-test.aslx (610, live Core)
// under ?ui=v2:
//
// - An object link opens its verb menu (the "menu" part, a popover) by click or by Enter; the
//   arrow keys move through it, Escape closes it and returns focus to the link, a click
//   elsewhere closes it, and choosing a verb sends "<verb> <object>".
// - Exit and command links send their commands; a command link with data-deactivateonclick
//   only works once, and links to things no longer in scope are disabled after each turn.
// - ShowMenu's options send ShowMenuResponse and are all disabled straight away; an author's
//   data-qv-event link keeps working; {popup:} opens and closes; an ordinary link to another
//   site opens a new tab rather than leaving the game.
// - Links still work in a transcript restored with loadHtml.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';
const lastLink = (text) => `qv-output a.cmdlink:text-is("${text}") >> nth=-1`;

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', (err) => errors.push(String(err)));
    page.on('console', (msg) => {
        if (msg.type() === 'error') errors.push(msg.text());
    });

    await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-links-test.aslx&ui=v2`);
    await page.waitForFunction(
        () => !document.getElementById('qv-start') && document.querySelector('qv-output')?.textContent.includes('A hall with a bell'),
        null, { timeout: 30000 })
        .catch(() => { throw new Error('the game never started on v2'); });
    // The verbs arrive with the first turn's updateLinks.
    await page.waitForFunction(() => document.querySelector('qv-output a.elementmenu')?.dataset.verbs, null, { timeout: 10000 })
        .catch(() => { throw new Error("the lamp link never got its verbs (updateLinks('object', …))"); });

    const outputText = () => page.evaluate(() => document.querySelector('qv-output').textContent);
    const turnOver = () => page.waitForFunction(() => window.canSendCommand, null, { timeout: 10000 });
    async function waitForText(text, what) {
        await page.waitForFunction((t) => document.querySelector('qv-output').textContent.includes(t), text, { timeout: 10000 })
            .catch(() => { throw new Error(`${what}: "${text}" never appeared`); });
        await turnOver();
    }
    async function send(command, expect) {
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await waitForText(expect, `"${command}"`);
    }
    const menuState = () => page.evaluate(() => {
        const menu = document.querySelector('qv-verb-menu');
        return {
            open: menu.classList.contains('qv-open'),
            popover: menu.matches(':popover-open'),
            part: menu.dataset.qvPart,
            role: menu.getAttribute('role'),
            items: [...menu.querySelectorAll('[role="menuitem"]')].map((item) => item.textContent),
            focused: document.activeElement?.textContent ?? '',
            minItemHeight: Math.min(...[...menu.querySelectorAll('[role="menuitem"]')].map((item) => item.offsetHeight)),
        };
    });

    // ── Object links and the verb menu ──────────────────────────────────────

    const lamp = lastLink('lamp');
    const lampAttrs = await page.locator(lamp).evaluate((a) => ({
        role: a.getAttribute('role'), popup: a.getAttribute('aria-haspopup'), tabindex: a.tabIndex,
    }));
    check(lampAttrs.role === 'button' && lampAttrs.popup === 'menu' && lampAttrs.tabindex === 0,
        'an object link is a focusable button that opens a menu');

    await page.click(lamp);
    let menu = await menuState();
    check(menu.open && menu.popover && menu.part === 'menu' && menu.role === 'menu',
        'clicking an object link opens the verb menu as a popover with the "menu" part');
    check(menu.items.join('/') === 'Look at/Take', `the menu lists the object's verbs (${menu.items.join('/')})`);
    check(menu.focused === 'Look at', 'focus moves to the first verb');
    check(menu.minItemHeight >= 44, `verbs are 44px touch targets (${menu.minItemHeight}px)`);
    check(await page.locator(lamp).getAttribute('aria-expanded') === 'true', 'the link reports its menu as expanded');

    await page.keyboard.press('ArrowDown');
    check((await menuState()).focused === 'Take', 'down moves to the next verb');
    await page.keyboard.press('ArrowDown');
    check((await menuState()).focused === 'Look at', 'down from the last verb wraps to the first');
    await page.keyboard.press('Escape');
    menu = await menuState();
    check(!menu.open && !menu.popover, 'Escape closes the menu');
    check(await page.locator(lamp).evaluate((a) => document.activeElement === a && a.getAttribute('aria-expanded') === 'false'),
        'focus returns to the link, which reports the menu as collapsed');

    await page.keyboard.press('Enter');
    check((await menuState()).open, 'Enter on a focused object link opens its menu');
    await page.mouse.click(5, 5);
    check(!(await menuState()).open, 'a click elsewhere closes the menu');

    await page.click(lamp);
    await page.click('qv-verb-menu [role="menuitem"]:text-is("Look at")');
    await waitForText('A brass lamp.', 'choosing "Look at"');
    check((await outputText()).includes('> look at lamp'), 'choosing a verb sends "<verb> <object>"');
    check(!(await menuState()).open, 'the menu closes once a verb is chosen');

    // ── Command links ───────────────────────────────────────────────────────

    const bell = lastLink('Ring the bell');
    await page.click(bell);
    await waitForText('Ding!', 'the command link');
    check((await outputText()).includes('> ring bell'), 'a command link sends its command');
    const bellState = await page.locator(bell).evaluate((a) => ({
        disabled: a.classList.contains('disabled'), aria: a.getAttribute('aria-disabled'), deactivated: a.hasAttribute('data-qv-deactivated'),
    }));
    check(bellState.disabled && bellState.aria === 'true' && bellState.deactivated,
        'a command link with data-deactivateonclick is disabled once used');
    // "look" prints the room again, with a fresh link; the used one is the first.
    await send('look', '> look');
    const usedBell = 'qv-output a.cmdlink:text-is("Ring the bell") >> nth=0';
    check(await page.locator(usedBell).evaluate((a) => a.classList.contains('disabled')),
        "updateLinks doesn't re-enable a deactivated command link");
    const before = (await outputText()).split('Ding!').length;
    await page.locator(usedBell).dispatchEvent('click');
    await page.waitForTimeout(500);
    check((await outputText()).split('Ding!').length === before, 'a disabled link does nothing when clicked');

    // ── {popup:} ────────────────────────────────────────────────────────────

    await send('note', 'There is a');
    const trigger = 'qv-output [data-qv-popup] >> nth=-1';
    const popup = () => page.locator(trigger).evaluate((button) => {
        const content = document.getElementById(button.dataset.qvPopup);
        return { open: content.matches(':popover-open'), visible: content.offsetHeight > 0, expanded: button.getAttribute('aria-expanded') };
    });
    check(!(await popup()).visible, "a popup's content starts hidden");
    await page.click(trigger);
    let state = await popup();
    check(state.open && state.visible && state.expanded === 'true', 'its trigger opens the popup as a popover');
    await page.keyboard.press('Escape');
    state = await popup();
    check(!state.open && !state.visible && state.expanded === 'false', 'Escape closes the popup');
    await page.click(trigger);
    await page.locator(trigger).evaluate((button) => document.getElementById(button.dataset.qvPopup).click());
    check(!(await popup()).visible, 'a click on the popup closes it');

    // ── ShowMenu options and author event links ─────────────────────────────

    await send('choose', 'Pick a colour');
    const options = 'qv-output a[data-qv-event="ShowMenuResponse"]';
    check(await page.locator(options).count() === 2, 'ShowMenu prints its options as data-qv-event links');
    await page.click(`${options}:text-is("Blue")`);
    await waitForText('You chose Blue.', 'choosing a ShowMenu option');
    check(await page.locator(options).evaluateAll((links) => links.every((a) => a.classList.contains('disabled'))),
        "a ShowMenu's options are all disabled once one is chosen");

    await send('links', 'a website');
    const ping = lastLink('ping');
    await page.click(ping);
    await waitForText('Ping 42.', 'the data-qv-event link');
    await page.click(ping);
    await page.waitForFunction(() => document.querySelector('qv-output').textContent.split('Ping 42.').length === 3,
        null, { timeout: 10000 }).catch(() => { throw new Error("an author's event link stopped working after one click"); });
    await turnOver();
    console.log("PASS: an author's data-qv-event link sends its event each time");

    await page.evaluate(() => {
        window.opened = [];
        window.open = (url, target) => { window.opened.push([url, target]); return null; };
    });
    const url = page.url();
    await page.click('qv-output a[href]:text-is("a website")');
    const opened = await page.evaluate(() => window.opened);
    check(opened.length === 1 && opened[0][0] === 'https://example.com/' && opened[0][1] === '_blank' && page.url() === url,
        'a link to another site opens in a new tab and leaves the game where it is');

    // ── Exit links, scope, and a restored transcript ────────────────────────

    const html = await page.evaluate(() => {
        const adapter = window.QuestPlayerUis.v2;
        const saved = adapter.getOutputHtml();
        adapter.resetOutput();
        adapter.loadHtml(saved);
        return saved;
    });
    check(html.includes('elementmenu'), 'the transcript is restored with its links');

    await page.click(lastLink('north'));
    await waitForText('A small kitchen.', 'the exit link');
    check((await outputText()).includes('> go north'), 'an exit link in a restored transcript sends its command');
    const stale = await page.evaluate(() => ({
        lamp: [...document.querySelectorAll('qv-output a.elementmenu')].every((a) => a.classList.contains('disabled')),
        north: [...document.querySelectorAll('qv-output a.exitlink')].filter((a) => a.textContent === 'north')
            .every((a) => a.classList.contains('disabled')),
        south: [...document.querySelectorAll('qv-output a.exitlink')].filter((a) => a.textContent === 'south')
            .every((a) => !a.classList.contains('disabled')),
    }));
    check(stale.lamp && stale.north, 'links to an object and an exit no longer in scope are disabled');
    check(stale.south, 'a link to an exit in scope stays enabled');
    await page.locator(lamp).dispatchEvent('click');
    check(!(await menuState()).open, "a disabled object link doesn't open its menu");

    if (errors.length) throw new Error(`page errors:\n  ${errors.join('\n  ')}`);
    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
