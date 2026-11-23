// Verifies the game stylesheet editor (#2613 PR 7): a CSS code editor that edits game.stylesheet, on
// a Stylesheet node of its own under Advanced in the tree, which the Appearance tab's Advanced
// section links to. The CSS is pasted in (a single CodeMirror transaction, as code-view.spec.mjs
// explains), committed on blur, still there after going to another node and back,
// syntax-highlighted, saved into the game's XML with its line breaks, and removed again when the
// editor is emptied.
//
// Run: build WasmEditor (Debug) first, then
//   npx playwright test --project=appshell game-stylesheet
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('game-stylesheet', async ({ baseURL }) => {
    const baseUrl = baseURL;
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
        await page.getByRole('button', { name: 'Appearance', exact: true }).click();

        const editorGroup = page.getByRole('group', { name: 'Stylesheet' });
        const summary = page.locator('summary', { hasText: 'Advanced' });
        await summary.waitFor({ state: 'visible', timeout: 5000 });
        if (!(await summary.evaluate(el => el.closest('details')?.open))) await summary.click();
        if (await editorGroup.count()) throw new Error('The Appearance tab still has the stylesheet editor');
        await page.getByRole('button', { name: /^The stylesheet has a page of its own/ }).click();
        await editorGroup.waitFor({ state: 'visible', timeout: 5000 });
        const treeNode = page.locator('[data-value="_stylesheet"]');
        await page.waitForSelector('[data-value="_stylesheet"][data-selected]', { timeout: 5000 })
            .catch(() => { throw new Error('The link didn\'t select the Stylesheet node in the tree'); });
        const cm = editorGroup.locator('.cm-content');
        if (!(await cm.count())) throw new Error('The Stylesheet node has no code editor');
        console.log('PASS: the Appearance tab links to the Stylesheet node under Advanced, which has the code editor');

        const openStylesheet = async () => {
            await page.locator('[data-value="game"][data-part="branch-control"]').click();
            await page.getByRole('button', { name: 'Appearance', exact: true }).waitFor();
            await treeNode.first().click();
            await editorGroup.waitFor({ state: 'visible', timeout: 5000 });
        };

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

        await openStylesheet();
        const shown = await cm.innerText();
        if (shown.replace(/\n+/g, '\n').trim() !== css) throw new Error(`The stylesheet didn't persist: ${JSON.stringify(shown)}`);
        console.log('PASS: the CSS is kept after going to another node and back');

        // HighlightStyle.define() generates obfuscated per-rule class names, so check that the
        // selector and the custom property land in classed token spans rather than naming a class.
        // With no CSS language they're bare text in .cm-line (#2626).
        for (const token of ['qv-player', '--qv-link']) {
            const cls = await editorGroup.locator('.cm-line span[class]', { hasText: token }).count();
            if (cls === 0) throw new Error(`"${token}" isn't syntax-highlighted in the stylesheet editor`);
        }
        console.log('PASS: the CSS is syntax-highlighted');

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

        await openStylesheet();
        await setCss('');
        xml = await rawXml();
        if (xml.includes('<stylesheet')) throw new Error('Emptying the editor left a <stylesheet> attribute behind');
        console.log('PASS: emptying the editor removes the attribute');

        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
