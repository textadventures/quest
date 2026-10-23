// Checks QuestUI on the legacy UI (legacy/questui.js): the 610 UI contract (docs/ui-v2-contract.md)
// implemented over playercore.js, which 610 games play on until v2 becomes their default.
//
// Covers:
// - every member the contract lists exists
// - the calls whose arguments come from ASL structures (a string dictionary, a list of
//   dictionaries with numbers in it, a string list), made from a real 610 game, which depend on
//   WasmPlayerBridge serialising nested lists and dictionaries as JSON
// - each member's effect on the legacy DOM, including that a hidden part stays hidden when legacy
//   code re-shows it, and that unknown names are ignored
// - the new link markup: data-qv-event links and {popup:} buttons
// - the author hooks: continue-on-click, fading, placeholder, panes and events
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const contractMembers = [
    'sendCommand', 'sendEvent', 'restart',
    'addText', 'clearScreen', 'setAlignment', 'startSection', 'endSection', 'hideSection',
    'disableSection', 'disableCommandLinks', 'updateLinks', 'scrollToEnd', 'addYouTube', 'addVimeo',
    'textEffect',
    'setVisible', 'setPaneOrder', 'setLayoutPreset', 'setPictureFrame', 'setInterfaceString',
    'setCompassDirections', 'setLocation', 'setStatusAttributes',
    'setTokens', 'setCommandBarStyle', 'setCommandBarPreset', 'setCommandPrompt', 'addStylesheet',
    'addPane', 'setPaneContent', 'setCommands', 'setAutoScroll', 'setContinueOnClick',
    'fadePreviousOutput', 'setCommandPlaceholder', 'setMapInteractive', 'on', 'off',
];
const mapMembers = [
    'show', 'setInteractive', 'respondToClicks', 'setScale', 'setCentre', 'drawBox', 'drawLabel',
    'drawPlayer', 'drawLine', 'drawArrow', 'drawSquare', 'drawShape', 'addShapePoint', 'drawImage',
    'drawSvg', 'loadSvg', 'drawGridLines', 'showCustomLayer', 'clearCustomLayer', 'clearAllLayers',
];
const transcriptMembers = [
    'write', 'enable', 'disable', 'kill', 'revive', 'setName', 'setPrefix', 'stampDateAndTime',
    'promptForName', 'show',
];

let browser;
try {
    browser = await chromium.launch();
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', (err) => errors.push(String(err)));
    page.on('console', (msg) => {
        if (msg.type() === 'error') errors.push(msg.text());
    });

    const outputText = () => page.$eval('#divOutput', (el) => el.textContent);
    async function waitForOutput(text, what) {
        await page.waitForFunction(
            (t) => document.querySelector('#divOutput').textContent.includes(t), text, { timeout: 10000 })
            .catch(() => { throw new Error(`${what}: output never showed "${text}"`); });
    }
    async function sendCommand(command, expectedText) {
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 10000 })
            .catch(() => { throw new Error(`can't send "${command}": player never became ready for a command`); });
        await page.fill('#txtCommand', command);
        await page.press('#txtCommand', 'Enter');
        await waitForOutput(expectedText, `"${command}"`);
    }
    const isVisible = (selector) => page.$eval(selector, (el) => getComputedStyle(el).display !== 'none'
        && el.getClientRects().length > 0);
    function expect(actual, expected, what) {
        if (actual !== expected) throw new Error(`${what}: expected ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
        console.log(`PASS: ${what}`);
    }

    await page.goto(`${baseUrl}/?url=/e2e-fixtures/legacy-questui-test.aslx`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 30000 });

    // ── The surface ────────────────────────────────────────────────────────
    const missing = await page.evaluate(([members, mapM, transcriptM]) => {
        const out = [];
        for (const m of members) if (typeof QuestUI[m] !== 'function') out.push(m);
        for (const m of mapM) if (typeof QuestUI.map?.[m] !== 'function') out.push(`map.${m}`);
        for (const m of transcriptM) if (typeof QuestUI.transcript?.[m] !== 'function') out.push(`transcript.${m}`);
        return out;
    }, [contractMembers, mapMembers, transcriptMembers]);
    expect(missing.join(', '), '', 'every contract member exists');

    const parts = await page.$$eval('[data-qv-part]', (els) => els.map((el) =>
        el.dataset.qvPane ? `${el.dataset.qvPart}:${el.dataset.qvPane}` : el.dataset.qvPart).sort().join(' '));
    expect(parts,
        'border command-bar exits location map output pane:commands pane:compass pane:customstatus pane:inventory pane:objects pane:status panes picture-frame',
        'legacy elements carry the contract part names');

    // ── ASL-structured arguments, from the game ────────────────────────────
    await sendCommand('tokens', 'Tokens set.');
    expect(await page.$eval('#gameBorder', (el) => el.style.maxWidth), '700px', 'setTokens: --qv-output-width sets the game width');
    expect(await page.$eval('#inventoryAccordion', (el) => getComputedStyle(el).color), 'rgb(255, 255, 255)',
        'setTokens: pane tokens apply through setPanes');
    expect(await page.$eval('#gameBorder', (el) => getComputedStyle(el).borderLeftColor), 'rgb(255, 0, 0)',
        'setTokens: --qv-border-colour');
    expect(await page.evaluate(() => document.documentElement.style.getPropertyValue('--author-colour')), 'teal',
        "setTokens: an author's own custom property is set on the root");
    expect(await page.evaluate(() => document.documentElement.style.getPropertyValue('color')), '',
        'setTokens: a name without the -- prefix is ignored');

    await sendCommand('status', 'Status set.');
    expect(await page.$eval('#statusVars', (el) => el.innerHTML), 'Health: 7/10<br>Score: 3',
        'setStatusAttributes: a list of dictionaries from ASL shows each item\'s text');
    // Today's Core runs UpdateStatusAttributes at the end of every turn, which empties the pane
    // again in a game with no status attributes, so check the pane showing with a direct call.
    await page.evaluate(() => QuestUI.setStatusAttributes([{ name: 'gold', text: 'Gold: 5' }]));
    expect(await isVisible('#statusVarsAccordion'), true, 'setStatusAttributes: the status pane shows');

    await sendCommand('order', 'Panes reordered.');
    const order = await page.$$eval('#gamePanesRunning > *', (els) => els.slice(0, 4).map((el) => el.id).join(' '));
    expect(order, 'compassLabel compassAccordion statusVarsLabel statusVarsAccordion',
        'setPaneOrder: a string list from ASL moves those panes to the top, in order');

    await sendCommand('hidecompass', 'Compass hidden.');
    expect(await isVisible('#compassAccordion'), false, 'setVisible: the compass pane hides');
    expect(await isVisible('#location'), false, 'setVisible: the location bar hides');
    await page.evaluate(() => $('#compassAccordion, #compassLabel').show());
    expect(await isVisible('#compassAccordion'), false, 'setVisible: a hidden pane stays hidden when legacy code re-shows it');
    await page.evaluate(() => QuestUI.setVisible('compass', true));
    expect(await isVisible('#compassAccordion'), true, 'setVisible: the compass pane shows again');

    // ── Link markup ────────────────────────────────────────────────────────
    await sendCommand('links', 'peek');
    await page.click('a[data-qv-event="PingEvent"]');
    await waitForOutput('Event 42', 'data-qv-event link');
    console.log('PASS: a data-qv-event link sends its event and parameter');
    expect(await isVisible('#pop1'), false, '{popup:} content starts hidden');
    await page.click('[data-qv-popup="pop1"]');
    expect(await isVisible('#pop1'), true, 'clicking a data-qv-popup button opens its content');
    await page.click('#pop1');
    expect(await isVisible('#pop1'), false, 'clicking the popup content closes it');

    // ── Author hooks ───────────────────────────────────────────────────────
    await page.evaluate(() => {
        window.__events = [];
        QuestUI.on('turnStarted', () => window.__events.push('start'));
        QuestUI.on('turnEnded', () => window.__events.push('end'));
        QuestUI.on('outputAdded', () => window.__events.push('output'));
        QuestUI.on('locationChanged', (d) => window.__events.push(`location:${d.location}`));
        QuestUI.fadePreviousOutput(true);
    });
    await sendCommand('ping', 'pong');
    await page.waitForFunction(() => window.__events.includes('end'), null, { timeout: 10000 });
    const events = await page.evaluate(() => window.__events.join(' '));
    if (!events.startsWith('start') || !events.includes('output') || !events.endsWith('end')) {
        throw new Error(`turn events out of order: ${events}`);
    }
    console.log(`PASS: turnStarted, outputAdded and turnEnded fire (${events})`);
    const faded = await page.$$eval('#divOutput .qv-previous-output', (els) => els.length);
    if (faded === 0) throw new Error('fadePreviousOutput: no earlier output was faded');
    console.log(`PASS: fadePreviousOutput fades earlier output (${faded} elements)`);

    await page.evaluate(() => QuestUI.setLocation('Somewhere else'));
    expect(await page.$eval('#location', (el) => el.textContent), 'Somewhere else', 'setLocation');
    expect(await page.evaluate(() => window.__events.at(-1)), 'location:Somewhere else', 'locationChanged fires');

    await page.evaluate(() => {
        QuestUI.addPane('notes', 'Notes', '<p id="notes-body">Hello</p>');
        QuestUI.setPaneContent('notes', '<p id="notes-body">Updated</p>');
        QuestUI.setCommandPlaceholder('What now?');
        QuestUI.setPaneContent('customstatus', 'Custom status');
        QuestUI.setVisible('customstatus', true);
    });
    expect(await page.$eval('#notes-body', (el) => el.textContent), 'Updated', 'addPane and setPaneContent');
    expect(await page.$eval('#qv-pane-notes', (el) => el.dataset.qvPane), 'notes', 'an author pane carries its part name');
    await page.evaluate(() => QuestUI.setPaneOrder(['notes']));
    expect(await page.$eval('#gamePanesRunning > *', (el) => el.id), 'qv-pane-notes-label', 'setPaneOrder moves an author pane');
    expect(await page.$eval('#txtCommand', (el) => el.placeholder), 'What now?', 'setCommandPlaceholder');
    expect(await isVisible('#customStatusPane'), true, 'setVisible shows the custom status pane');
    expect(await page.$eval('#customStatusPane', (el) => el.textContent), 'Custom status', 'setPaneContent: customstatus');

    await sendCommand('waitclick', 'Waiting.');
    await page.waitForSelector('#endWaitLink', { state: 'visible', timeout: 10000 });
    await page.mouse.click(5, 5);
    await waitForOutput('Continued.', 'setContinueOnClick');
    console.log('PASS: setContinueOnClick ends a wait on a click anywhere');

    // ── Unknown names and kinds are ignored ────────────────────────────────
    await page.evaluate(() => {
        QuestUI.setVisible('no-such-part', false);
        QuestUI.textEffect('no-such-effect', 'x');
        QuestUI.updateLinks('no-such-kind', {});
        QuestUI.setLayoutPreset('no-such-preset');
        QuestUI.setCommandBarPreset('no-such-preset');
        QuestUI.setPaneContent('no-such-pane', 'x');
    });
    console.log('PASS: unknown names are ignored');

    // ── The gamebook preset, last, since it hides the command bar ──────────
    await page.evaluate(() => QuestUI.setLayoutPreset('gamebook'));
    expect(await isVisible('#gamePanes'), false, 'setLayoutPreset("gamebook") hides the panes');
    expect(await isVisible('#txtCommandDiv'), false, 'setLayoutPreset("gamebook") hides the command bar');

    if (errors.length) throw new Error(`page errors:\n  ${errors.join('\n  ')}`);
    console.log('PASS: no page errors');
    console.log('\nAll checks passed.');
} catch (err) {
    console.error(`FAIL: ${err.message}`);
    process.exitCode = 1;
} finally {
    await browser?.close();
}
