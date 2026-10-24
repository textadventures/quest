// Verifies the game stylesheet editor (#2613 PR 7): a CSS code editor under the game's Interface
// tab, in its Advanced section, that edits game.stylesheet. The CSS is pasted in (a single
// CodeMirror transaction, as verify-appshell-code-view.mjs explains), committed on blur, still
// there after switching tabs, saved into the game's XML with its line breaks, and removed again
// when the editor is emptied.
//
// Run: build WasmEditor (Debug) first, then
//   node tests/e2e/verify-appshell-game-stylesheet.mjs [baseUrl]
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const selectAllKey = process.platform === 'darwin' ? 'Meta+A' : 'Control+A';
const pasteKey = process.platform === 'darwin' ? 'Meta+V' : 'Control+V';
const css = 'qv-player {\n    --qv-link: sienna;\n}';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 } });
    await ctx.grantPermissions(['clipboard-read', 'clipboard-write']);
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Game Stylesheet Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: 'Interface', exact: true }).click();

    const editorGroup = page.getByRole('group', { name: 'Stylesheet' });
    const openAdvanced = async () => {
        const summary = page.locator('summary', { hasText: 'Advanced' });
        await summary.waitFor({ state: 'visible', timeout: 5000 });
        if (!(await summary.evaluate(el => el.closest('details')?.open))) await summary.click();
        await editorGroup.waitFor({ state: 'visible', timeout: 5000 });
    };
    await openAdvanced();
    const cm = editorGroup.locator('.cm-content');
    if (!(await cm.count())) throw new Error('The Stylesheet control has no code editor');
    console.log('PASS: the Interface tab\'s Advanced section has a Stylesheet code editor');

    const setCss = async (text) => {
        await cm.click();
        await page.keyboard.press(selectAllKey);
        if (text) {
            await page.evaluate(t => navigator.clipboard.writeText(t), text);
            await page.keyboard.press(pasteKey);
        } else {
            await page.keyboard.press('Backspace');
        }
        await page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); });
        await page.waitForTimeout(300);
    };
    await setCss(css);

    await page.getByRole('button', { name: 'Setup', exact: true }).click();
    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    await openAdvanced();
    const shown = await cm.innerText();
    if (shown.replace(/\n+/g, '\n').trim() !== css) throw new Error(`The stylesheet didn't persist: ${JSON.stringify(shown)}`);
    console.log('PASS: the CSS is kept after switching tabs');

    const rawXml = async () => {
        await page.click('button[title="Raw XML code view"]');
        await page.waitForSelector('.cm-editor', { timeout: 5000 });
        const text = await page.locator('.cm-content').first().innerText();
        await page.click('button[title="Raw XML code view"]');
        return text;
    };
    let xml = await rawXml();
    if (!xml.includes('<stylesheet>qv-player {') || !xml.includes('--qv-link: sienna;')) {
        throw new Error('The game XML has no <stylesheet> with the CSS in it');
    }
    console.log('PASS: the CSS is saved as the game\'s stylesheet attribute, line breaks and all');

    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    await openAdvanced();
    await setCss('');
    xml = await rawXml();
    if (xml.includes('<stylesheet')) throw new Error('Emptying the editor left a <stylesheet> attribute behind');
    console.log('PASS: emptying the editor removes the attribute');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
