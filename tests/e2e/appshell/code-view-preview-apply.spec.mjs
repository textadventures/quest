// Regression test: edits in the raw XML code view only reached the game when the author clicked
// Apply, and Preview opened the player on the game as it was before them. People kept testing
// "their" change, seeing nothing, and only then noticing the Apply button. Preview (like Save,
// Save As, Backup and Publish) now applies pending code view edits first, keeps the panel open,
// and doesn't open the player at all when the edits are invalid XML.
import { test, expect } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('code-view-preview-apply', async ({ baseURL }) => {
    const browser = await chromium.launch();

    // Paste rather than fill()/type(): see code-view.spec.mjs for why neither works on CodeMirror.
    const selectAllKey = process.platform === 'darwin' ? 'Meta+A' : 'Control+A';
    const pasteKey = process.platform === 'darwin' ? 'Meta+V' : 'Control+V';

    async function setCmContent(page, text) {
        const cm = page.locator('.cm-editor .cm-content').first();
        await cm.click();
        await page.keyboard.press(selectAllKey);
        await page.evaluate(t => navigator.clipboard.writeText(t), text);
        await page.keyboard.press(pasteKey);
    }

    try {
        const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
        await ctx.grantPermissions(['clipboard-read', 'clipboard-write']);
        const page = await ctx.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        // Preview opens the player with window.open(); count those tabs rather than load them.
        let previewTabs = 0;
        ctx.on('page', p => {
            if (p !== page) {
                previewTabs++;
                void p.close().catch(() => {});
            }
        });

        await page.goto(`${baseURL}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Code View Preview Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="Preview game"]', { timeout: 30000 });

        await page.click('button[title="Raw XML code view"]');
        await page.waitForSelector('.cm-editor', { timeout: 5000 });
        const xml = await page.locator('.cm-content').first().innerText();
        if (!xml.includes('</game>')) throw new Error('Expected the raw XML view to contain the <game> element');

        // --- A valid edit, then straight to Preview without clicking Apply ---
        await setCmContent(page, xml.replace('</game>', '<author>Applied by Preview</author>\n</game>'));
        // The panel learns about edits when the editor loses focus.
        await page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); });
        await page.waitForSelector('[data-testid="code-view-not-applied"]', { timeout: 5000 });
        console.log('PASS: the panel marks edits that have not been applied yet');

        await page.click('button[title="Preview game"]');
        // The player opens only after the apply finished, so once it has, the panel is back.
        await expect.poll(() => previewTabs, { timeout: 15000 }).toBeGreaterThan(0);
        await page.waitForTimeout(500);
        if (await page.locator('[role="dialog"]').count() > 0) throw new Error('Expected Preview to apply without asking, but a dialog is open');
        if (previewTabs !== 1) throw new Error(`Expected Preview to open one player tab after applying, got ${previewTabs}`);
        await page.waitForSelector('.cm-editor', { timeout: 5000 });
        if (await page.locator('[data-testid="code-view-not-applied"]').count() > 0) throw new Error('Expected no "not applied" marker after Preview applied the edits');
        console.log('PASS: Preview applied the pending edits without asking, kept the panel open and opened the player');

        // Nothing left to apply, so Cancel closes straight away, and the game holds the edit.
        await page.click('button:has-text("Cancel")');
        await page.waitForSelector('[data-value="game"][data-part="branch-control"]', { timeout: 5000 });
        await page.click('button[title="Raw XML code view"]');
        await page.waitForSelector('.cm-editor', { timeout: 5000 });
        const reopened = await page.locator('.cm-content').first().innerText();
        if (!reopened.includes('Applied by Preview')) throw new Error('Expected the reopened raw XML to contain the edit Preview applied');
        console.log('PASS: the edit Preview applied is in the game');

        // --- Invalid XML: Preview shows the error and doesn't open the player ---
        await setCmContent(page, 'not valid xml <unclosed');
        await page.click('button[title="Preview game"]');
        await page.waitForSelector('text=Failed to load game', { timeout: 10000 });
        await page.waitForTimeout(500);
        if (previewTabs !== 1) throw new Error(`Expected Preview not to open the player when the XML is invalid, but ${previewTabs - 1} more tab(s) opened`);
        if (!(await page.locator('[data-testid="code-view-not-applied"]').isVisible())) throw new Error('Expected the invalid edit to still be marked as not applied');
        console.log('PASS: Preview with invalid XML shows the error, keeps the text and opens nothing');

        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
