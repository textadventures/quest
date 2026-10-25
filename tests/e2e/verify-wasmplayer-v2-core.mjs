// Checks player UI v2's core loop (#2595) by playing games end to end under ?ui=v2:
//
// - examples/simple.aslx (pre-610, with live Core): the game starts in the v2 bundle with no
//   jQuery, the output is a live region, commands get responses, up/down steps through the
//   command history, and a command typed while a turn is in progress isn't sent or lost.
// - fixtures/v2-core-test.aslx (610, live Core): the location bar follows the player, QuestUI's
//   turnStarted/turnEnded/outputAdded/locationChanged events fire, alignment, output sections
//   (hidden afterwards) and clear screen work, setTokens sets only custom properties on the UI
//   root (a bare length as px), setVisible hides the location bar, contract members v2 doesn't
//   implement yet are accepted, and a transcript restored with loadHtml carries on.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const input = 'qv-command-bar input';

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch();

    async function open(query, startText) {
        const page = await browser.newPage();
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        const bundles = new Set();
        page.on('request', (request) => {
            const match = new URL(request.url()).pathname.match(/^\/(legacy|v2)\//);
            if (match) bundles.add(match[1]);
        });
        await page.goto(`${baseUrl}/${query}`);
        await page.waitForFunction(
            (text) => !document.getElementById('qv-start') && document.querySelector('qv-output')?.textContent.includes(text),
            startText, { timeout: 30000 })
            .catch(() => { throw new Error(`${query}: the game never started on v2`); });
        return { page, errors, bundles };
    }

    const outputText = (page) => page.evaluate(() => document.querySelector('qv-output').textContent);

    // Types a command and waits until its turn is over and its output is on screen: by default
    // the command's echo, which a command that clears the screen needs to replace.
    async function send(page, command, expect = `> ${command}`) {
        await page.fill(input, command);
        await page.press(input, 'Enter');
        await page.waitForFunction(
            (echo) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(echo),
            expect, { timeout: 10000 })
            .catch(() => { throw new Error(`"${command}" got no response`); });
    }

    // ── examples/simple.aslx ────────────────────────────────────────────────

    {
        const { page, errors, bundles } = await open('?url=/examples/simple.aslx&ui=v2', 'You are in');

        const shell = await page.evaluate(() => ({
            ui: Object.keys(window.QuestPlayerUis ?? {}).join(','),
            jquery: typeof window.jQuery,
            questUi: typeof window.QuestUI,
            role: document.querySelector('qv-output').getAttribute('role'),
            live: document.querySelector('qv-output').getAttribute('aria-live'),
            part: document.querySelector('qv-output').dataset.qvPart,
            focused: document.activeElement?.matches('qv-command-bar input') ?? false,
        }));
        check([...bundles].join(',') === 'v2', 'simple.aslx under ?ui=v2 requests only the v2 bundle');
        check(shell.ui === 'v2' && shell.jquery === 'undefined' && shell.questUi === 'object',
            'v2 registers its adapter and QuestUI, with no jQuery');
        check(shell.role === 'log' && shell.live === 'polite' && shell.part === 'output',
            'the output is a polite live region carrying data-qv-part="output"');
        check(shell.focused, 'the command input has focus once the game starts');

        await send(page, 'look');
        check((await outputText(page)).includes('You can see a thing'), '"look" gets the room description');
        check(await page.inputValue(input) === '', 'the input is cleared once a command is sent');

        await send(page, 'x thing');
        await page.press(input, 'ArrowUp');
        check(await page.inputValue(input) === 'x thing', 'up recalls the last command');
        await page.press(input, 'ArrowUp');
        check(await page.inputValue(input) === 'look', 'up again recalls the one before');
        await page.press(input, 'ArrowUp');
        check(await page.inputValue(input) === 'look', 'up stops at the oldest command');
        await page.press(input, 'ArrowDown');
        await page.press(input, 'ArrowDown');
        check(await page.inputValue(input) === '', 'down past the newest command returns to an empty line');

        // While a turn is in progress (the host sets canSendCommand again once it's over), a
        // command is neither sent nor thrown away.
        const before = await outputText(page);
        await page.evaluate(() => { window.canSendCommand = false; });
        await page.fill(input, 'inventory');
        await page.press(input, 'Enter');
        await page.waitForTimeout(500);
        check(await outputText(page) === before && await page.inputValue(input) === 'inventory',
            'a command typed during a turn is held in the input, not sent');
        await page.evaluate(() => { window.canSendCommand = true; });
        await page.press(input, 'Enter');
        await page.waitForFunction(() => window.canSendCommand
            && document.querySelector('qv-output').textContent.includes('> inventory'), null, { timeout: 10000 });
        check(await page.inputValue(input) === '', 'the held command is sent once the turn is over');

        if (errors.length) throw new Error(`simple.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── A 610 game ──────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('?url=/e2e-fixtures/v2-core-test.aslx&ui=v2', 'A plain hall');

        const location = () => page.evaluate(() => document.querySelector('[data-qv-part="location"]').textContent);
        check((await location()).includes('hall'), 'the location bar shows the starting room');

        const tokens = await page.evaluate(() => {
            const style = document.querySelector('qv-player').style;
            return { text: style.getPropertyValue('--qv-text'), font: style.getPropertyValue('--qv-font-body') };
        });
        // Colours only once the game has changed one (verify-wasmplayer-v2-themes.mjs), which this
        // one hasn't.
        check(tokens.text === '' && tokens.font !== '', `610 Core's InitInterface sets the game's default tokens, but not its default colours (${JSON.stringify(tokens)})`);

        await page.evaluate(() => {
            window.seen = { turnStarted: 0, turnEnded: 0, outputAdded: 0, locations: [] };
            QuestUI.on('turnStarted', () => window.seen.turnStarted++);
            QuestUI.on('turnEnded', () => window.seen.turnEnded++);
            QuestUI.on('outputAdded', (detail) => { if (detail?.html) window.seen.outputAdded++; });
            QuestUI.on('locationChanged', (detail) => window.seen.locations.push(detail.location));
            // A listener that throws mustn't stop the others or the turn.
            window.brokenListener = () => { throw new Error('author bug'); };
            QuestUI.on('turnEnded', window.brokenListener);
        });
        await send(page, 'n');
        const seen = await page.evaluate(() => window.seen);
        check((await location()).includes('kitchen'), 'the location bar follows the player');
        check(seen.turnStarted === 1 && seen.turnEnded === 1, `turnStarted and turnEnded fire once per command (${JSON.stringify(seen)})`);
        check(seen.outputAdded > 0, 'outputAdded fires with the HTML added');
        check(seen.locations.some((l) => l.includes('kitchen')), 'locationChanged reports the new location');
        // The throwing listener is logged as an error by design; it's not a page error.
        errors.splice(0, errors.length, ...errors.filter((e) => !e.includes('author bug')));
        await page.evaluate(() => {
            QuestUI.off('turnEnded', window.brokenListener);
            QuestUI.off('locationChanged', () => {});
        });

        await send(page, 'centre');
        check(!errors.some((e) => e.includes('author bug')) && await page.evaluate(() => window.seen.turnEnded) === 2,
            'off removes a listener, leaving the others');
        const centred = await page.evaluate(() => [...document.querySelectorAll('qv-output .qv-block')]
            .find((block) => block.textContent.includes('Centred text.'))?.style.textAlign);
        check(centred === 'center', 'SetAlignment puts text in a centred block');

        await send(page, 'section');
        check((await outputText(page)).includes('Inside the section.'), 'text inside an output section shows');
        await send(page, 'hidesection');
        await page.waitForFunction(() => !document.querySelector('qv-output').textContent.includes('Inside the section.'),
            null, { timeout: 5000 }).catch(() => { throw new Error('HideOutputSection left the section on screen'); });
        const afterHide = await outputText(page);
        check(afterHide.includes('After the section.') && afterHide.includes('Section hidden.'),
            'HideOutputSection removes only that section');

        await send(page, 'tokens');
        const root = await page.evaluate(() => {
            const player = document.querySelector('qv-player');
            return {
                width: player.style.getPropertyValue('--qv-output-width'),
                link: player.style.getPropertyValue('--qv-link'),
                author: player.style.getPropertyValue('--author-colour'),
                color: player.style.color,
                frameWidth: getComputedStyle(document.querySelector('[data-qv-part="border"]')).maxWidth,
            };
        });
        check(root.width === '600px' && root.frameWidth === '600px', 'setTokens reads a bare length as px, and the frame uses it');
        check(root.link === 'darkred' && root.author === 'teal', "setTokens sets contract and authors' custom properties");
        check(root.color === '', 'setTokens ignores names that are not custom properties');

        await send(page, 'hidelocation');
        check(await page.evaluate(() => {
            const bar = document.querySelector('[data-qv-part="location"]');
            return bar.hidden && bar.offsetHeight === 0;
        }), 'setVisible("location", false) hides the location bar, and an unknown part is ignored');

        await send(page, 'unimplemented');
        check((await outputText(page)).includes('Still playing.'), 'contract calls with nothing to do (a transcript that isn\'t recording) are accepted');

        // A save's transcript is restored through the adapter's loadHtml (WorldModel's restore).
        const restored = await page.evaluate(() => {
            const adapter = window.QuestPlayerUis.v2;
            const html = adapter.getOutputHtml();
            adapter.resetOutput();
            const emptied = document.querySelector('qv-output').textContent === '';
            adapter.loadHtml(html);
            return { emptied, same: adapter.getOutputHtml() === html };
        });
        check(restored.emptied && restored.same, 'loadHtml restores a transcript getOutputHtml returned');
        await send(page, 'look');
        const lastBlock = await page.evaluate(() => document.querySelector('qv-output').lastElementChild.textContent);
        check(lastBlock.includes('You are in a kitchen'), 'output carries on after a restored transcript');

        await send(page, 'clear', 'Fresh page.');
        const cleared = await outputText(page);
        check(!cleared.includes('Centred text.') && cleared.includes('Fresh page.'), 'ClearScreen empties the output');

        if (errors.length) throw new Error(`v2-core-test.aslx: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
