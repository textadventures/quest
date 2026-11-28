// Checks the start screen keeps its own look while the game loads behind it, by comparing it on
// its Begin card for fixtures/splash-isolation.aslx (a game whose stylesheet restyles <body> and
// plain elements, with dark colours) with fixtures/begin-gate-sync-sound.aslx (no styling).
//
// - The game's stylesheet changes none of the start screen's text: the font, weight, style,
//   colour, spacing, case, shadow or underline of its name, Begin button and "Quest Viva" line.
// - The start screen follows the device's light or dark setting, not the game's scheme: on a light
//   device a dark game leaves it light, and on a dark device a legacy game (always light) leaves
//   it dark.
//
// Media elements' play() is replaced by a fake that's refused until the page has been clicked, so
// both fixtures show the Begin card (see begin-gate.spec.mjs).
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('splash-isolation', async ({ baseURL }) => {
    const baseUrl = baseURL;

    function check(condition, what) {
        if (!condition) throw new Error(what);
        console.log(`PASS: ${what}`);
    }

    let browser;
    try {
        browser = await chromium.launch();

        // The start screen's computed styles on the Begin card for fixture.
        async function splashStyles(fixture, { colorScheme, ui }) {
            const page = await browser.newPage({ colorScheme });
            await page.addInitScript(() => {
                let activated = false;
                document.addEventListener('pointerdown', () => { activated = true; }, { capture: true });
                HTMLMediaElement.prototype.play = function () {
                    if (!activated) return Promise.reject(new DOMException('No click yet', 'NotAllowedError'));
                    setTimeout(() => this.dispatchEvent(new Event('ended')), 100);
                    return Promise.resolve();
                };
            });
            await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}${ui ? `&ui=${ui}` : ''}`);
            await page.waitForFunction(() => document.getElementById('qv-clicktobegin')?.offsetParent, null, { timeout: 30000 })
                .catch(() => { throw new Error(`${fixture}: the Begin card never showed`); });
            const styles = await page.evaluate(() => {
                const props = ['font-family', 'font-size', 'font-weight', 'font-style', 'color', 'letter-spacing',
                    'text-transform', 'text-shadow', 'text-decoration-line'];
                const result = {};
                for (const id of ['qv-game-name', 'qv-clicktobegin-btn', 'qv-brand-small']) {
                    const style = getComputedStyle(document.getElementById(id));
                    result[id] = Object.fromEntries(props.map((p) => [p, style.getPropertyValue(p)]));
                }
                result.card = getComputedStyle(document.querySelector('#qv-start > .card')).backgroundColor;
                result.scheme = document.documentElement.getAttribute('data-qv-scheme');
                return result;
            });
            await page.close();
            return styles;
        }

        for (const [colorScheme, ui] of [['light', undefined], ['dark', 'legacy']]) {
            const label = `${colorScheme} device${ui ? `, ${ui} UI` : ''}`;
            const plain = await splashStyles('begin-gate-sync-sound.aslx', { colorScheme, ui });
            const styled = await splashStyles('splash-isolation.aslx', { colorScheme, ui });
            const otherScheme = colorScheme === 'light' ? 'dark' : 'light';
            check(styled.scheme === otherScheme, `${label}: the game has set the page's scheme to ${otherScheme} (${styled.scheme})`);
            for (const id of ['qv-game-name', 'qv-clicktobegin-btn', 'qv-brand-small']) {
                for (const [prop, value] of Object.entries(plain[id])) {
                    check(styled[id][prop] === value,
                        `${label}: the game's stylesheet leaves #${id}'s ${prop} alone (${styled[id][prop]}, plain ${value})`);
                }
            }
            check(styled.card === plain.card, `${label}: the card keeps the device's scheme (${styled.card}, plain ${plain.card})`);
        }
        {
            const light = await splashStyles('begin-gate-sync-sound.aslx', { colorScheme: 'light' });
            const dark = await splashStyles('begin-gate-sync-sound.aslx', { colorScheme: 'dark' });
            check(light.card !== dark.card, `the card does follow the device's scheme (${light.card} / ${dark.card})`);
        }
    } finally {
        await browser?.close();
    }
});
