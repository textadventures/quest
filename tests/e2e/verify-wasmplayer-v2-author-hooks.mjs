// Checks the 610 Core author hook functions (#2650, PR 1), playing
// fixtures/v2-author-hooks-test.aslx on v2 and on legacy:
//
// - AddPane, SetPaneContent (an author pane and the custom status pane), SetCommandPane,
//   SetPaneVisible and SetCommandPlaceholder change the interface when a script calls them.
// - Saving, changing a pane, then loading the save brings back what they set, as it was when
//   saved: the UI starts afresh on a load, and InitInterface applies them again from game.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

// Where each UI keeps what's checked here
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

    await page.goto(`${baseUrl}/?url=/e2e-fixtures/v2-author-hooks-test.aslx&ui=${ui}`);
    await page.waitForFunction((sel) => !document.getElementById('qv-start') && document.querySelector(sel)?.textContent.includes('A hall.'),
        output, { timeout: 30000 })
        .catch(() => { throw new Error(`[${ui}] the game never started`); });

    async function waitForText(text, what) {
        await page.waitForFunction(([sel, t]) => document.querySelector(sel).textContent.includes(t), [output, text], { timeout: 10000 })
            .catch(() => { throw new Error(`[${ui}] ${what}: "${text}" never appeared`); });
    }
    async function type(command) {
        await page.waitForFunction(ready, null, { timeout: 10000 });
        await page.fill(input, command);
        await page.press(input, 'Enter');
    }
    // What the hooks set, read from the part names (docs/ui-v2-contract.md), which both UIs carry
    const state = () => page.evaluate((sel) => {
        const pane = (name) => document.querySelector(`[data-qv-part=pane][data-qv-pane="${name}"]`);
        const shown = (el) => !!el && el.checkVisibility();
        return {
            clues: pane('clues')?.textContent.trim() ?? null,
            cluesShown: shown(pane('clues')),
            customStatus: pane('customstatus')?.textContent.trim() ?? null,
            commands: [...(pane('commands')?.querySelectorAll('a') ?? [])].map((a) => `${a.textContent}:${a.dataset.command}`).join(','),
            commandsShown: shown(pane('commands')),
            compassShown: shown(pane('compass')),
            placeholder: document.querySelector(sel).placeholder,
        };
    }, input);

    const before = await state();
    check(before.clues === null && before.compassShown && !before.commandsShown, `[${ui}] a new game has no author pane, and the compass and no command pane`);

    await type('setup');
    await waitForText('Hooks set.', 'setup');
    const set = await state();
    check(set.cluesShown && set.clues.includes('No clues yet.'), `[${ui}] AddPane and SetPaneContent add a pane with its content`);
    check(set.customStatus.includes('Score: 1'), `[${ui}] SetPaneContent fills the custom status pane`);
    check(set.commandsShown && set.commands === 'Look:look,Wait:z', `[${ui}] SetCommandPane shows the command pane with its commands`);
    check(!set.compassShown, `[${ui}] SetPaneVisible hides the compass`);
    check(set.placeholder === 'What now?', `[${ui}] SetCommandPlaceholder sets the command bar's placeholder`);

    // Save, change a pane, then load
    await page.waitForFunction(ready, null, { timeout: 10000 });
    await page.click('#cmdSave');
    await page.waitForFunction(() => document.getElementById('qv-saves')?.open, null, { timeout: 5000 })
        .catch(() => { throw new Error(`[${ui}] Save / Load didn't open the host's dialog`); });
    await page.click('#qv-saves-save-new');
    await waitForText('Game saved.', 'saving');
    await page.evaluate(() => document.getElementById('qv-saves')?.close());

    await type('later');
    await waitForText('Later on.', 'changing a pane after saving');
    check((await state()).clues.includes('A muddy footprint.'), `[${ui}] the pane changes after the save`);

    await page.waitForFunction(ready, null, { timeout: 10000 });
    await page.click('#cmdSave');
    await page.waitForFunction(() => document.querySelector('#qv-saves-list [data-slot]'), null, { timeout: 5000 })
        .catch(() => { throw new Error(`[${ui}] the save never appeared in the Save/Load list`); });
    await page.click('#qv-saves-list [data-slot]');
    await page.waitForFunction((sel) => {
        const text = document.querySelector(sel)?.textContent ?? '';
        return text.includes('Hooks set.') && !text.includes('Later on.') && window.canSendCommand;
    }, output, { timeout: 30000 })
        .catch(() => { throw new Error(`[${ui}] loading didn't bring back the game as it was saved`); });

    const loaded = await state();
    check(loaded.cluesShown && loaded.clues.includes('No clues yet.') && !loaded.clues.includes('footprint'),
        `[${ui}] after loading, the author pane is back with its content as saved`);
    check(loaded.customStatus.includes('Score: 1'), `[${ui}] after loading, the custom status pane has its content`);
    check(loaded.commandsShown && loaded.commands === 'Look:look,Wait:z', `[${ui}] after loading, the command pane is back`);
    check(!loaded.compassShown, `[${ui}] after loading, the compass is still hidden`);
    check(loaded.placeholder === 'What now?', `[${ui}] after loading, the placeholder is still set`);

    if (errors.length) throw new Error(`[${ui}] page errors:\n  ${errors.join('\n  ')}`);
    await page.close();
}

let browser;
try {
    browser = await chromium.launch();
    await playOn(browser, 'v2');
    await playOn(browser, 'legacy');
    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
