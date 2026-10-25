// Checks player UI v2's gamebook layout (#2627, PR 7) by playing two 610 gamebooks under ?ui=v2,
// loaded as if from textadventures.co.uk (?id=, with the API mocked) so their pictures come from a
// resourceRoot. YouTube is served as a blank page and window.Audio replaced by a fake.
//
// - fixtures/v2-gamebook-test.aslx: GamebookCore's setLayoutPreset("gamebook") gives a page in a
//   centred reading column, with no panes, location, command bar, exits strip or pinned status
//   bar, and Save/Load still in the top bar. The first page shows its picture, YouTube video and
//   sound, and its options are full-width choices at least 44px high. Choosing one turns the page:
//   the old choices go, the chosen one is echoed, and keyboard focus moves to the new page's first
//   choice. A page that asks for input (GamebookCore's GetInput) shows the command bar until it's
//   answered. A web link option is drawn as a choice too.
// - fixtures/v2-gamebook-clear-test.aslx (clearlastpage): each page replaces the last.
// - With the editor's default colours it follows dark mode (610 GamebookCore sends no colour
//   tokens then).
// - Phones get the same layout, with no sideways scroll; axe-core finds no WCAG A/AA violations.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { createRequire } from 'node:module';
import { chromium } from 'playwright';

const require = createRequire(import.meta.url);
const axePath = require.resolve('axe-core/axe.min.js');
const baseUrl = process.argv[2] || 'http://localhost:5175';
const resourceRoot = 'https://images.example.test/res/';
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==', 'base64');

function check(condition, what) {
    if (!condition) throw new Error(what);
    console.log(`PASS: ${what}`);
}

let browser;
try {
    browser = await chromium.launch({ args: ['--autoplay-policy=no-user-gesture-required'] });

    async function open(fixture, width = 1200, colorScheme = 'light') {
        const page = await browser.newPage({ viewport: { width, height: 800 }, colorScheme });
        const errors = [];
        page.on('pageerror', (err) => errors.push(String(err)));
        page.on('console', (msg) => {
            if (msg.type() === 'error') errors.push(msg.text());
        });
        await page.addInitScript(() => {
            window.__sounds = [];
            window.Audio = class FakeAudio {
                constructor(src) {
                    this.src = src;
                    this.paused = true;
                    window.__sounds.push(this);
                }
                addEventListener() {}
                removeEventListener() {}
                play() {
                    this.paused = false;
                    return Promise.resolve();
                }
                pause() { this.paused = true; }
            };
        });
        await page.route('**/api/game/**', (route) => route.fulfill({
            contentType: 'application/json',
            body: JSON.stringify({ sourceGameUrl: `${baseUrl}/e2e-fixtures/${fixture}.aslx`, resourceRoot }),
        }));
        await page.route(`${resourceRoot}**`, (route) => route.fulfill({ contentType: 'image/png', body: png }));
        await page.route(/youtube-nocookie\.com|example\.com/, (route) => route.fulfill({ contentType: 'text/html', body: '<!doctype html><title>page</title>' }));
        await page.goto(`${baseUrl}/?id=${fixture}&ui=v2`);
        // A game that plays a sound asks for a click first, for the browser's autoplay rules.
        await page.waitForFunction(() => {
            const gate = document.getElementById('qv-clicktobegin');
            return (gate && !gate.hidden && gate.offsetParent !== null) || document.querySelector('qv-output')?.textContent.includes('first page');
        }, null, { timeout: 30000 }).catch(() => { throw new Error(`${fixture} never loaded`); });
        if (await page.locator('#qv-clicktobegin-btn').isVisible()) await page.click('#qv-clicktobegin-btn');
        await page.waitForFunction(() => !document.getElementById('qv-start') && window.canSendCommand
            && document.querySelector('qv-output')?.textContent.includes('This is the first page.'), null, { timeout: 30000 })
            .catch(() => { throw new Error(`${fixture} never started on v2`); });
        await page.addScriptTag({ path: axePath });
        return { page, errors };
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

    const shown = (el) => !!el && !el.hidden && el.offsetParent !== null && getComputedStyle(el).display !== 'none';
    const layout = (page) => page.evaluate((shownSrc) => {
        const isShown = new Function(`return ${shownSrc}`)();
        const q = (sel) => document.querySelector(sel);
        const frame = q('[data-qv-part="border"]').getBoundingClientRect();
        const output = q('[data-qv-part="output"]').getBoundingClientRect();
        const choices = [...document.querySelectorAll('[data-qv-part="output"] [data-qv-sections] a')]
            .filter((a) => a.isConnected && !a.closest('.qv-hiding'))
            .map((a) => {
                const box = a.getBoundingClientRect();
                return { text: a.textContent.trim(), height: box.height, width: box.width, display: getComputedStyle(a).display, disabled: a.classList.contains('disabled') };
            });
        return {
            gamebook: q('qv-player').hasAttribute('data-qv-gamebook'),
            panes: isShown(q('[data-qv-part="panes"]')),
            commandBar: isShown(q('[data-qv-part="command-bar"]')),
            location: isShown(q('[data-qv-part="location"]')),
            exits: isShown(q('[data-qv-part="exits"]')),
            statusBar: isShown(q('[data-qv-part="status-bar"]')),
            save: isShown(q('#cmdSave')),
            frameWidth: frame.width, leftGap: frame.left, rightGap: window.innerWidth - frame.right,
            outputWidth: output.width,
            choices,
            focused: document.activeElement?.textContent?.trim() ?? '',
            text: q('[data-qv-part="output"]').textContent,
        };
    }, shown.toString());
    const choose = async (page, text, expect) => {
        await page.click(`[data-qv-part="output"] [data-qv-sections] a:has-text("${text}")`);
        await page.waitForFunction((t) => window.canSendCommand && document.querySelector('qv-output').textContent.includes(t), expect, { timeout: 15000 })
            .catch(() => { throw new Error(`choosing "${text}" never showed "${expect}"`); });
        // HideOutputSection fades the old choices out before removing them.
        await page.waitForTimeout(400);
    };

    // ── Wide ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-gamebook-test');
        let l = await layout(page);
        check(l.gamebook && !l.panes && !l.commandBar && !l.location && !l.exits && !l.statusBar,
            'the gamebook layout has no panes, location, command bar, exits strip or pinned status bar');
        check(l.save, 'Save/Load stays in the top bar');
        check(l.frameWidth <= 702 && Math.abs(l.leftGap - l.rightGap) < 2,
            `the page is a centred reading column (${Math.round(l.frameWidth)}px wide)`);
        const media = await page.evaluate(() => ({
            picture: document.querySelector('qv-output img')?.getAttribute('src'),
            video: document.querySelector('qv-output iframe[data-qv-video="youtube"]')?.getAttribute('src'),
            sound: window.__sounds.some((s) => s.src.endsWith('short-beep.wav') && !s.paused),
        }));
        check(media.picture === `${resourceRoot}cover.png` && media.video?.includes('7vIi0U4rSX4') && media.sound,
            'the first page has its picture, YouTube video and sound');
        check(l.choices.map((c) => c.text).join('|') === 'Turn to page two|Give your name|Visit the website',
            `its options are the choices (${l.choices.map((c) => c.text).join(', ')})`);
        check(l.choices.every((c) => c.display === 'flex' && c.height >= 44 && Math.abs(c.width - (l.outputWidth - 32)) < 40),
            `each choice is full width and at least 44px high (${Math.round(l.choices[0].width)}×${Math.round(l.choices[0].height)})`);
        await audit(page, 'with a gamebook page');

        await choose(page, 'Turn to page two', 'This is page two.');
        l = await layout(page);
        check(l.text.includes('This is the first page.') && l.text.includes('Turn to page two'),
            'without clearlastpage the last page stays, with the chosen option echoed');
        check(l.choices.map((c) => c.text).join('|') === 'Back to the first page', `the old choices go (${l.choices.map((c) => c.text).join(', ')})`);
        check(l.focused === 'Back to the first page', `keyboard focus moves to the new page's first choice ("${l.focused}")`);

        await page.keyboard.press('Enter');
        await page.waitForFunction(() => window.canSendCommand && document.querySelectorAll('qv-output [data-qv-sections] a:not(.disabled)').length === 3,
            null, { timeout: 15000 }).catch(() => { throw new Error('Enter on the focused choice never turned the page'); });
        check(true, 'Enter on the focused choice turns the page');
        await page.waitForTimeout(400);

        await choose(page, 'Give your name', 'What is your name?');
        l = await layout(page);
        check(l.commandBar, "a page that asks for input (GetInput) shows the command bar");
        await page.fill('qv-command-bar input', 'Ada');
        await page.press('qv-command-bar input', 'Enter');
        await page.waitForFunction(() => window.canSendCommand && document.querySelector('qv-output').textContent.includes('Hello, Ada.'),
            null, { timeout: 15000 }).catch(() => { throw new Error('the answer never reached the page'); });
        check(!(await layout(page)).commandBar, 'and hides it again once answered');

        if (errors.length) throw new Error(`wide: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── clearlastpage ───────────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-gamebook-clear-test');
        await choose(page, 'Turn to page two', 'This is page two.');
        const l = await layout(page);
        check(!l.text.includes('This is the first page.'), 'with clearlastpage each page replaces the last');
        check(l.focused === 'Back to the first page', `and focus is on its first choice ("${l.focused}")`);
        if (errors.length) throw new Error(`clear: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Dark ────────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-gamebook-test', 1200, 'dark');
        const colours = await page.evaluate(() => {
            const frame = getComputedStyle(document.querySelector('[data-qv-part="border"]'));
            return { background: getComputedStyle(document.querySelector('qv-player')).getPropertyValue('--qv-bg').trim(), text: frame.color };
        });
        check(colours.background === '#18181b' && colours.text === 'rgb(228, 228, 231)',
            `a gamebook with the default colours follows dark mode (${colours.text} on ${colours.background})`);
        await audit(page, 'with a gamebook page, dark');
        if (errors.length) throw new Error(`dark: page errors:\n  ${errors.join('\n  ')}`);
        await page.close();
    }

    // ── Narrow ──────────────────────────────────────────────────────────────

    {
        const { page, errors } = await open('v2-gamebook-test', 390);
        const l = await layout(page);
        check(l.gamebook && !l.panes && !l.commandBar && !l.exits && !l.statusBar, 'a phone gets the same layout');
        check(l.choices.every((c) => c.height >= 44 && c.width <= l.outputWidth), 'with full-width choices');
        check(!(await page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth)),
            'and no sideways scroll');
        await audit(page, 'with a gamebook page, narrow');
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
