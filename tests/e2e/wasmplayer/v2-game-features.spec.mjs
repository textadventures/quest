// Checks game features on player UI v2, with the checks that apply there from legacy's
// inline-sync-prompts.spec.mjs, dialogue-pages, scenery-inventory, media-embeds and
// debugger, under ?ui=v2 on those scripts' fixtures, which have live Core:
//
// - Ask and ShowMenu are drawn as numbered links in the transcript, answered by a click or by
//   typing the number. A cancellable menu is dismissed by other input, and an uncancellable one
//   ignores it.
// - Text adventure Pages: {page:} links, numbered options, Save staying enabled mid-dialogue, a
//   refused command, a page with no options ending the dialogue, and undoing back into one.
// - Carried scenery is in the inventory pane and the inventory command, hidefrominventory is in
//   neither but can still be dropped, and scenery stays out of room descriptions.
// - YouTube, Vimeo and an author's own iframe and video fit the output at phone and desktop widths.
// - The debugger: the Debug button only shows in a preview session, an attribute override changes
//   the live game, a walkthrough runs and can be cancelled, and Restart restarts with its overlay
//   above v2's own chrome.
//
// The debugger's dialog itself (tabs, sorting, moving, resizing) belongs to the host, so only
// debugger.spec.mjs checks those.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';
import { handOffPreviewGame } from '../lib/preview-handoff.mjs';

test('v2-game-features', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const input = 'qv-command-bar input';

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    let browser;

    async function open(url, context = browser) {
        const page = await context.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));
        await page.goto(`${baseUrl}/?url=${url}&ui=v2`);
        await page.waitForSelector(input, { state: 'visible', timeout: 60000 });
        return page;
    }

    async function send(page, command) {
        await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 15000 });
        await page.fill(input, command);
        await page.press(input, 'Enter');
        if (await page.inputValue(input) !== '') throw new Error(`"${command}" wasn't accepted`);
    }

    const output = page => page.$eval('qv-output', el => el.textContent);
    const hasOutput = (page, text, since = 0) => page.waitForFunction(
        ([t, start]) => document.querySelector('qv-output').textContent.slice(start).includes(t),
        [text, since], { timeout: 15000 });

    async function prompts() {
        const page = await open('/examples/sync-and-async/v600.aslx');
        const noDialog = async () => (await page.$$('dialog.qv-dialog[open]')).length === 0;
        await send(page, 'menufn');
        await hasOutput(page, 'What is your favourite colour?');
        await hasOutput(page, '4: Yellow');
        check(await noDialog(), 'ShowMenu() draws numbered options in the transcript, not a dialog');
        const links = page.locator('qv-output a.commandlink:visible');
        check(await links.count() === 4, 'each option is a link');
        await links.nth(2).click();
        await hasOutput(page, 'You chose Blue');
        await page.waitForFunction(() => ![...document.querySelectorAll('qv-output a.commandlink')].some(a => a.checkVisibility()), null, { timeout: 5000 });
        console.log('PASS: clicking one chooses it, and the links go');

        await send(page, 'menufn');
        await hasOutput(page, '4: Yellow', (await output(page)).lastIndexOf('You chose Blue'));
        await send(page, '2');
        await hasOutput(page, 'You chose Green');
        console.log('PASS: typing an option number chooses it');

        await send(page, 'menufn');
        await page.waitForTimeout(300);
        let before = (await output(page)).length;
        await send(page, 'xyzzy');
        await page.waitForTimeout(800);
        check(!(await output(page)).slice(before).includes("I don't understand"), 'other input is ignored while an uncancellable menu is open');
        await send(page, '1');
        await hasOutput(page, 'You chose Red');

        await send(page, 'menufncancel');
        await page.waitForTimeout(300);
        await send(page, 'xyzzy');
        await hasOutput(page, "You didn't choose.");
        console.log('PASS: other input dismisses a cancellable menu');

        await send(page, 'askfn');
        await hasOutput(page, '2: No');
        check(await noDialog(), 'Ask() draws its options in the transcript too');
        await send(page, '1');
        await hasOutput(page, 'You hand over $50.');

        await send(page, 'menucmd');
        before = (await output(page)).length;
        await page.waitForTimeout(300);
        check(await noDialog(), 'so does the show menu script command');
        await send(page, '4');
        await hasOutput(page, 'You chose Yellow');
        await page.close();
    }

    async function dialoguePages() {
        const page = await open('/e2e-fixtures/dialogue-pages-test.aslx');
        const link = command => page.locator(`qv-output a.cmdlink[data-command="${command}"]`);

        await send(page, 'guard_intro');
        await hasOutput(page, "don't understand");
        check(!(await output(page)).includes('The guard eyes you suspiciously.'), "a page's name isn't a command before its link has been shown");

        await send(page, 'look at signpost');
        await hasOutput(page, 'talk to the guard');
        let before = (await output(page)).length;
        await link('guard_intro').first().click();
        await hasOutput(page, 'The guard eyes you suspiciously.', before);
        const opened = (await output(page)).slice(before);
        check(!opened.includes("don't understand") && !opened.includes('guard_intro'), 'a {page:} link starts the dialogue, without echoing the internal name');
        await page.waitForFunction(() => window.canSendCommand === true);
        await send(page, '2');
        await hasOutput(page, 'The castle is closed.');

        await send(page, 'talk');
        await hasOutput(page, '1: Ask about the weather', (await output(page)).lastIndexOf('The castle is closed.'));
        check(await link('guard_weather').count() > 0, 'options are links');
        check(!await page.$eval('#cmdSave', el => el.disabled), 'Save stays enabled mid-dialogue');

        await page.waitForFunction(() => window.canSendCommand === true);
        await link('guard_weather').last().click();
        await hasOutput(page, 'Nice weather today.');
        await page.waitForFunction(() => {
            const old = [...document.querySelectorAll('qv-output a.cmdlink[data-command="guard_castle"]')];
            return !old.some(a => a.checkVisibility());
        }, null, { timeout: 5000 });
        console.log('PASS: choosing an option moves on, and hides the old options');

        before = (await output(page)).length;
        await send(page, '1');
        await hasOutput(page, '2: Ask about the castle', before);
        console.log('PASS: a typed option number works');

        before = (await output(page)).length;
        await send(page, 'marker');
        await hasOutput(page, 'Please choose one of the options above.', before);
        check(!(await output(page)).slice(before).includes('marker ran'), 'other commands are refused during the dialogue');

        await send(page, '2');
        await hasOutput(page, 'The castle is closed.', before);
        await send(page, 'marker');
        await hasOutput(page, 'marker ran');
        console.log('PASS: a page with no options ends the dialogue');

        await send(page, 'undo');
        await page.waitForFunction(() => window.canSendCommand === true);
        await send(page, 'undo');
        await page.waitForFunction(() => window.canSendCommand === true);
        await page.waitForTimeout(700);
        const restored = link('guard_weather').last();
        check(await restored.isVisible(), 'undoing back into the dialogue shows its options again');
        before = (await output(page)).length;
        await restored.click();
        await hasOutput(page, 'Nice weather today.', before);
        console.log('PASS: and they still work');
        await page.close();
    }

    async function sceneryAndInventory() {
        const page = await open('/e2e-fixtures/scenery-inventory-test.aslx');
        const item = 'qv-pane[data-qv-pane="inventory"] .qv-pane-item';
        await page.waitForSelector(item, { timeout: 20000 });
        const pane = (await page.$$eval(item, els => els.map(e => e.textContent))).join(', ');
        check(pane.includes('lamp') && pane.includes('poster'), `the inventory pane lists carried objects and scenery (${pane})`);
        check(!pane.includes('ghost') && !pane.includes('bladder'), 'and leaves out what is hidden from it');

        let before = (await output(page)).length;
        await send(page, 'inventory');
        await hasOutput(page, 'lamp', before);
        await page.waitForFunction(() => window.canSendCommand === true);
        const listing = (await output(page)).slice(before);
        check(listing.includes('poster') && !listing.includes('ghost') && !listing.includes('bladder'), 'the inventory command agrees');

        before = (await output(page)).length;
        await send(page, 'drop bladder');
        await hasOutput(page, "You can't drop your bladder.", before);
        console.log('PASS: a hidden object is still held');

        before = (await output(page)).length;
        await send(page, 'look');
        await hasOutput(page, 'rock', before);
        await page.waitForFunction(() => window.canSendCommand === true);
        check(!(await output(page)).slice(before).includes('mural'), 'scenery stays out of the room description');
        await page.close();
    }

    async function mediaEmbeds() {
        for (const [label, viewport] of [['phone', { width: 390, height: 844 }], ['desktop', { width: 1280, height: 900 }]]) {
            const page = await browser.newPage({ viewport });
            await page.route(/youtube-nocookie\.com|vimeo\.com/, route => route.fulfill({ status: 200, contentType: 'text/html', body: '' }));
            await page.goto(`${baseUrl}/?url=/e2e-fixtures/media-embeds-test.aslx&ui=v2`);
            await page.waitForFunction(() => document.querySelector('qv-output')?.textContent.includes('Embeds done.'), null, { timeout: 60000 });
            // The fixture's JS.AddVimeo is legacy's, which v2 doesn't define.
            await page.evaluate(() => QuestUI.addVimeo('76979871'));
            await page.waitForSelector('qv-output iframe[src*="vimeo.com"]', { state: 'attached' });
            const m = await page.evaluate(() => {
                const box = el => el.getBoundingClientRect().toJSON();
                const output = document.querySelector('qv-output');
                const iframes = [...output.querySelectorAll('iframe')];
                return {
                    outputWidth: output.clientWidth,
                    scrollWidth: document.documentElement.scrollWidth,
                    windowWidth: window.innerWidth,
                    youtube: box(iframes.find(f => f.src.includes('youtube-nocookie.com'))),
                    vimeo: box(iframes.find(f => f.src.includes('vimeo.com'))),
                    authorIframe: box(document.getElementById('authoriframe')),
                    authorVideo: box(document.getElementById('authorvideo')),
                };
            });
            check(m.scrollWidth <= m.windowWidth, `${label}: no sideways scrolling`);
            for (const name of ['youtube', 'vimeo', 'authorIframe', 'authorVideo']) {
                check(m[name].width <= m.outputWidth + 1, `${label}: ${name} fits the output (${Math.round(m[name].width)} of ${m.outputWidth}px)`);
            }
            for (const name of ['youtube', 'vimeo']) {
                check(Math.abs(m[name].width / m[name].height - 16 / 9) <= 0.02, `${label}: ${name} is 16:9`);
            }
            await page.close();
        }
    }

    async function debuggerSession() {
        let page = await open('/e2e-fixtures/debugger-test.aslx');
        await page.waitForTimeout(300);
        check(await page.$eval('#cmdDebug', el => getComputedStyle(el).display) === 'none', 'no Debug button in a normal session');
        await page.close();

        const context = await browser.newContext({ viewport: { width: 1280, height: 900 } });
        page = await context.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));
        const sender = await context.newPage();
        await sender.goto(baseUrl);
        await handOffPreviewGame(sender, page, {
            previewUrl: `${baseUrl}/?source=editor&ui=v2`,
            fixtureUrl: `${baseUrl}/e2e-fixtures/debugger-test.aslx`,
            filename: 'debugger-test.aslx',
        });
        await page.waitForSelector(input, { state: 'visible', timeout: 60000 });
        await hasOutput(page, 'You are in a kitchen');
        await sender.close();

        check(await page.isVisible('#cmdDebug') && await page.isVisible('#cmdRestart'), 'Debug and Restart show in a preview session');
        await page.click('#cmdDebug');
        await page.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });
        check(!await page.$eval('#questVivaDebugger', el => el.matches(':modal')), 'the debugger opens non-modally');
        const box = await page.$eval('#questVivaDebugger', el => {
            const r = el.getBoundingClientRect();
            return { x: r.x + r.width / 2, y: r.y + r.height / 2 };
        });
        check(await page.evaluate(({ x, y }) => document.getElementById('questVivaDebugger').contains(document.elementFromPoint(x, y)), box), "it's above v2's chrome");

        await page.click('#qv-debugger-tabs button:text("Objects")');
        await page.click('#qv-debugger-list [data-item="player"]');
        await page.click('[data-attr-row="parent"]');
        await page.fill('#qv-debugger-override-input', 'livingroom');
        await page.click('#qv-debugger-override-apply');
        await page.waitForFunction(() => /livingroom/i.test(document.querySelector('#qv-debugger-override-input')?.value ?? ''));
        await page.click('#qv-debugger-close');
        await send(page, 'look');
        await hasOutput(page, 'livingroom');
        console.log('PASS: an attribute override changes the live game');

        await page.click('#cmdDebug');
        await page.waitForSelector('#questVivaDebugger[open]');
        await page.click('#qv-debugger-tabs button:text("Walkthrough")');
        await page.click('#qv-debugger-list [data-item="basic"]');
        await page.click('[data-run-walkthrough="basic"]');
        await page.waitForFunction(() => document.querySelector('[data-walkthrough-status]')?.textContent === 'Done', null, { timeout: 15000 });
        console.log('PASS: a walkthrough runs');
        await page.click('#qv-debugger-list [data-item="slow"]');
        await page.click('[data-run-walkthrough="slow"]');
        await page.waitForFunction(() => document.querySelector('[data-walkthrough-status]')?.textContent === 'Running…');
        await page.click('[data-cancel-walkthrough]');
        await page.waitForFunction(() => document.querySelector('[data-walkthrough-status]')?.textContent !== 'Running…', null, { timeout: 15000 });
        console.log('PASS: and can be cancelled');
        await page.click('#qv-debugger-close');

        await page.click('#cmdRestart');
        await page.waitForSelector('#qv-restarting:not(.hidden)', { timeout: 5000 });
        const covered = await page.evaluate(() => {
            const overlay = document.getElementById('qv-restarting');
            const panes = document.elementFromPoint(window.innerWidth - 20, 200);
            const bar = document.elementFromPoint(window.innerWidth - 20, 10);
            return overlay.contains(panes) && overlay.contains(bar);
        });
        check(covered, "the restart overlay covers v2's panes and top bar");
        await page.waitForFunction(() => document.getElementById('qv-restarting')?.classList.contains('hidden'), null, { timeout: 30000 });
        await page.waitForFunction(() => {
            const text = document.querySelector('qv-output')?.textContent ?? '';
            return /You are in a kitchen/i.test(text) && !/livingroom/i.test(text);
        }, null, { timeout: 15000 });
        console.log('PASS: Restart goes back to the start of the game');
        await context.close();
    }

    try {
        browser = await chromium.launch();
        await prompts();
        await dialoguePages();
        await sceneryAndInventory();
        await mediaEmbeds();
        await debuggerSession();
        console.log('PASS');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser?.close();
    }
});
