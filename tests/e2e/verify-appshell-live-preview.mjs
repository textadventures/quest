// Checks the editor's live Preview (#2650 part 5): with a game running in Preview, changing the
// accent colour and the stylesheet (Appearance tab) and the layout (Interface tab) in the editor shows
// in the Preview straight away, without it restarting or losing what's been played so far.
// Turning an option back off takes it away again, since the Preview sets up a fresh UI each time.
//
// Requires both dev servers running (./dev.sh): AppShell on 5174, proxying /player to WasmPlayer,
// so BroadcastChannel (same-origin only) reaches the Preview tab.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const selectAllKey = process.platform === 'darwin' ? 'Meta+A' : 'Control+A';
const pasteKey = process.platform === 'darwin' ? 'Meta+V' : 'Control+V';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 }, colorScheme: 'light' });
    await ctx.grantPermissions(['clipboard-read', 'clipboard-write']);
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[editor pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Live Preview Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 30000 });
    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });

    const [preview] = await Promise.all([
        ctx.waitForEvent('page'),
        page.click('button[title="Preview game"]'),
    ]);
    const previewErrors = [];
    preview.on('pageerror', err => previewErrors.push(err.message));
    preview.on('console', msg => { if (msg.type() === 'error') previewErrors.push(msg.text()); });
    const commandInput = preview.locator('qv-command-bar input');
    await commandInput.waitFor({ state: 'visible', timeout: 60000 });

    // Something played, to check it's kept, and a marker that's lost if the page reloads.
    await commandInput.fill('x me');
    await commandInput.press('Enter');
    const output = preview.locator('[data-qv-part="output"]');
    await preview.waitForFunction(() => document.querySelector('[data-qv-part="output"]')?.textContent.includes('x me'),
        null, { timeout: 10000 });
    await preview.evaluate(() => { window.__livePreviewMarker = true; });
    const outputBefore = await output.innerText();
    console.log('PASS: Preview is running, with a command played');

    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));
    const openGameTab = async (tab) => {
        await page.locator('[data-value="game"][data-part="branch-control"]').click();
        await page.getByRole('button', { name: tab, exact: true }).click();
    };
    const playerState = () => preview.evaluate(() => {
        const player = document.querySelector('qv-player');
        return {
            accent: player?.style.getPropertyValue('--qv-accent').trim() ?? null,
            layout: player?.dataset.qvLayout ?? null,
            stylesheet: document.getElementById('qv-game-stylesheet')?.textContent ?? null,
            marker: window.__livePreviewMarker === true,
            output: document.querySelector('[data-qv-part="output"]')?.innerText ?? '',
        };
    });
    const waitForPreview = async (check, what) => {
        try {
            await preview.waitForFunction(check, null, { timeout: 10000 });
        } catch {
            throw new Error(`Preview didn't pick up ${what}: ${JSON.stringify(await playerState())}`);
        }
    };
    const assertKept = async (after) => {
        const state = await playerState();
        if (!state.marker) throw new Error(`Preview reloaded after ${after}`);
        if (!state.output.includes(outputBefore.trim())) {
            throw new Error(`Preview lost its transcript after ${after}: ${JSON.stringify(state.output)}`);
        }
    };

    // The accent colour, on the Appearance tab: a dropdown of colour names that also takes free text.
    await openGameTab('Appearance');
    const accent = page.locator('label', { hasText: /^\s*Accent colour/ }).locator('input, select').first();
    await accent.waitFor({ timeout: 5000 });
    if (await accent.evaluate(el => el.tagName === 'SELECT')) {
        await accent.selectOption({ label: 'Red' });
    } else {
        await accent.fill('red');
    }
    await settle();
    await waitForPreview(() => document.querySelector('qv-player')?.style.getPropertyValue('--qv-accent').trim() === 'red',
        'the accent colour');
    await assertKept('changing the accent colour');
    console.log('PASS: the accent colour shows in Preview without restarting it');

    // The stylesheet, in the Appearance tab's Advanced section.
    const summary = page.locator('summary', { hasText: 'Advanced' });
    if (!(await summary.evaluate(el => el.closest('details')?.open))) await summary.click();
    const cm = page.getByRole('group', { name: 'Stylesheet' }).locator('.cm-content');
    await cm.click();
    await page.keyboard.press(selectAllKey);
    await page.evaluate(t => navigator.clipboard.writeText(t), 'qv-player { --qv-link: sienna; }');
    await page.keyboard.press(pasteKey);
    await settle();
    await waitForPreview(() => document.getElementById('qv-game-stylesheet')?.textContent.includes('sienna'), 'the stylesheet');
    await assertKept('changing the stylesheet');
    console.log('PASS: the stylesheet shows in Preview without restarting it');

    // The layout, on the Interface tab.
    await openGameTab('Interface');
    const layout = page.locator('label', { hasText: /^\s*Where the panes go/ }).locator('select');
    await layout.selectOption({ label: 'In a sheet opened from beside the command bar' });
    await settle();
    await waitForPreview(() => document.querySelector('qv-player')?.dataset.qvLayout === 'sheet', 'the layout');
    await assertKept('changing the layout');
    console.log('PASS: the layout shows in Preview without restarting it');

    // Back to the default: a fresh UI each time means it goes away again.
    await layout.selectOption({ label: 'Beside the game (in a drawer on a phone)' });
    await settle();
    await waitForPreview(() => document.querySelector('qv-player')?.dataset.qvLayout === 'side', 'the default layout');
    await assertKept('changing the layout back');
    console.log('PASS: going back to the default layout shows in Preview too');

    // The game carries on as before.
    await commandInput.fill('look');
    await commandInput.press('Enter');
    await preview.waitForFunction(() => document.querySelector('[data-qv-part="output"]')?.textContent.includes('look'),
        null, { timeout: 10000 });
    const final = await playerState();
    if (final.accent !== 'red' || !final.stylesheet?.includes('sienna')) {
        throw new Error(`The changes didn't last into the next turn: ${JSON.stringify(final)}`);
    }
    console.log('PASS: the game carries on, with the changes, after them');

    if (previewErrors.length) throw new Error(`Preview reported errors: ${previewErrors.join(' | ')}`);
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
