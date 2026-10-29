// Checks player UI v2's panes (#2613, PR 1) by playing fixtures/v2-panes-test.aslx (610) under
// ?ui=v2:
//
// - Wide: the panes are a column beside the game, with no drawer button. Inventory and places and
//   objects list their items as buttons (exits left out), labelled by the game's interface
//   strings. An item opens the verb menu, which sends "<verb> <name>" with metadata naming the
//   element, as legacy's verb buttons do. An empty list shows EmptyListLabel. The command pane,
//   custom status pane and an author's pane are set through QuestUI. setPaneOrder, collapsing a
//   pane, and setVisible for one pane and for all of them work.
// - Narrow: the panes are a drawer from the top bar's button. It opens with focus inside, closes
//   on Escape (focus back on the button) or a press outside, and closes once a command chosen
//   in it is sent.
// - axe-core finds no WCAG A/AA violations with the panes beside the game or in an open drawer.
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
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-panes-test.aslx&ui=v2`);
        await page.waitForFunction(
            () => !document.getElementById('qv-start') && window.canSendCommand
                && document.querySelector('qv-output')?.textContent.includes('A hall.')
                && document.querySelector('qv-pane[data-qv-pane="inventory"] .qv-pane-item'),
            null, { timeout: 30000 })
            .catch(() => { throw new Error('the game never started on v2 with its panes filled'); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
    }

    async function send(page, command, expect) {
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await waitForTurn(page, expect, `"${command}"`);
    }
    async function waitForTurn(page, expect, what) {
        await page.waitForFunction((t) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(t),
            expect, { timeout: 10000 })
            .catch(() => { throw new Error(`${what}: "${expect}" never appeared`); });
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
    const items = (page, name) => page.evaluate((n) => [...document.querySelectorAll(`qv-pane[data-qv-pane="${n}"] .qv-pane-item`)]
        .map((b) => b.textContent), name);
    // A pane's heading as written (v2 shows it in capitals), then its body as shown.
    const paneText = (page, name) => page.evaluate((n) => {
        const element = document.querySelector(`qv-pane[data-qv-pane="${n}"]`);
        if (!element) return null;
        const heading = element.querySelector('summary');
        const label = heading && heading.offsetParent !== null ? heading.textContent.trim() : '';
        return [label, element.querySelector('.qv-pane-body').innerText.trim()].filter(Boolean).join('\n');
    }, name);

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(1200);
        const layout = await page.evaluate(() => {
            const main = document.querySelector('main').getBoundingClientRect();
            const aside = document.querySelector('[data-qv-part="panes"]').getBoundingClientRect();
            return {
                beside: aside.left >= main.right - 1 && aside.width > 0,
                buttonShown: getComputedStyle(document.querySelector('.qv-panes-button')).display !== 'none',
                labels: [...document.querySelectorAll('qv-pane[label] .qv-pane-heading')].filter((s) => s.offsetHeight > 0).map((s) => s.textContent),
            };
        });
        check(layout.beside && !layout.buttonShown, 'on a wide layout the panes are a column beside the game, with no drawer button');
        check(layout.labels.join('/') === 'Inventory/Places and Objects/Compass', `panes are labelled by the game's interface strings (${layout.labels.join('/')})`);
        check((await items(page, 'inventory')).join('/') === 'coin', 'the inventory lists what the player carries');
        check((await items(page, 'objects')).join('/') === 'brass lamp', 'places and objects lists the room, leaving out exits');
        check(await page.evaluate(() => document.querySelector('qv-pane[data-qv-pane="objects"] .qv-pane-item').getAttribute('aria-haspopup')) === 'menu',
            'a pane item is a button that opens a menu');
        await audit(page, 'with the panes beside the game');

        await page.click('qv-pane[data-qv-pane="objects"] .qv-pane-item');
        const verbs = await page.evaluate(() => [...document.querySelectorAll('qv-verb-menu [role="menuitem"]')].map((b) => b.textContent));
        check(verbs.includes('Take'), `a pane item opens the verb menu (${verbs.join('/')})`);
        await page.click('qv-verb-menu [role="menuitem"]:text-is("Take")');
        await waitForTurn(page, 'You pick it up.', 'choosing Take');
        check(await page.evaluate(() => document.querySelector('qv-output').textContent.includes('> take brass lamp')),
            'choosing a verb sends "<verb> <name>", and the game acts on that object');
        await page.waitForFunction(() => [...document.querySelectorAll('qv-pane[data-qv-pane="inventory"] .qv-pane-item')].some((b) => b.textContent === 'brass lamp'),
            null, { timeout: 5000 }).catch(() => { throw new Error("the inventory didn't update after taking the lamp"); });
        const objects = await page.evaluate(() => document.querySelector('qv-pane[data-qv-pane="objects"] .qv-pane-empty')?.textContent ?? '');
        check(objects.trim() !== '', `an empty list shows EmptyListLabel ("${objects.trim()}")`);

        await send(page, 'setuppanes', 'Panes set up.');
        check(await page.evaluate(() => [...document.querySelectorAll('qv-pane[data-qv-pane="commands"] a.cmdlink')].map((a) => a.textContent).join('/')) === 'Look/Wait',
            'setCommands fills the command pane');
        check((await paneText(page, 'customstatus')) === 'Health: 10', 'setPaneContent fills the custom status pane');
        check((await paneText(page, 'notes')) === 'Notes\nRemember the lamp.' || (await paneText(page, 'notes'))?.replace(/\s+/g, ' ') === 'Notes Remember the lamp.',
            "addPane adds an author's pane, and setPaneContent changes it");
        check((await items(page, 'inventory')).length === 2, "setPaneContent leaves a built-in list pane alone");
        await page.click('qv-pane[data-qv-pane="commands"] a.cmdlink:text-is("Wait")');
        await waitForTurn(page, '> wait', 'the command pane');
        console.log("PASS: a command pane link sends its command");

        await send(page, 'reorder', 'Panes reordered.');
        const order = await page.evaluate(() => [...document.querySelectorAll('[data-qv-part="panes"] > qv-pane')].map((p) => p.dataset.qvPane).join(','));
        check(order.startsWith('notes,objects,inventory'), `setPaneOrder puts the named panes first (${order})`);

        await page.click('qv-pane[data-qv-pane="inventory"] summary');
        check(await page.evaluate(() => !document.querySelector('qv-pane[data-qv-pane="inventory"] details').open), 'a pane collapses from its heading');

        await send(page, 'hideinventory', 'Inventory hidden.');
        check(await page.evaluate(() => document.querySelector('qv-pane[data-qv-pane="inventory"]').offsetHeight === 0), 'setVisible hides one pane');
        await send(page, 'hidepanes', 'Panes hidden.');
        check(await page.evaluate(() => document.querySelector('[data-qv-part="panes"]').offsetWidth === 0), 'setVisible("panes", false) hides them all');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open(390);
        const drawer = () => page.evaluate(() => {
            const aside = document.querySelector('[data-qv-part="panes"]');
            const button = document.querySelector('.qv-panes-button');
            const rect = aside.getBoundingClientRect();
            return {
                open: getComputedStyle(aside).visibility === 'visible' && rect.left < window.innerWidth,
                expanded: button.getAttribute('aria-expanded'),
                focusInside: aside.contains(document.activeElement),
                focusOnButton: document.activeElement === button,
                buttonShown: button.offsetWidth > 0,
                buttonHeight: button.offsetHeight,
            };
        });
        let state = await drawer();
        check(!state.open && state.buttonShown && state.expanded === 'false', 'on a narrow layout the panes start closed, behind a drawer button');
        check(state.buttonHeight >= 44, `the drawer button is a 44px target (${state.buttonHeight}px)`);

        await page.click('.qv-panes-button');
        await page.waitForTimeout(300);
        state = await drawer();
        check(state.open && state.expanded === 'true' && state.focusInside, 'the button opens the drawer, with focus inside it');
        await audit(page, 'with the panes drawer open');
        await page.keyboard.press('Escape');
        await page.waitForTimeout(300);
        state = await drawer();
        check(!state.open && state.focusOnButton && state.expanded === 'false', 'Escape closes the drawer and returns focus to its button');

        await page.click('.qv-panes-button');
        await page.waitForTimeout(300);
        await page.mouse.click(20, 400);
        await page.waitForTimeout(300);
        check(!(await drawer()).open, 'a press outside closes the drawer');

        await page.click('.qv-panes-button');
        await page.waitForTimeout(300);
        await page.click('qv-pane[data-qv-pane="inventory"] .qv-pane-item');
        await page.click('qv-verb-menu [role="menuitem"]:text-is("Look at")');
        await waitForTurn(page, 'A shiny coin.', 'choosing Look at from the drawer');
        await page.waitForTimeout(300);
        check(!(await drawer()).open, 'the drawer closes once a command chosen in it is sent');

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
