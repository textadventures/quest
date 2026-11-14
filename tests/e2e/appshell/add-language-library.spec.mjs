// Issue #2383: adding a language library with "Add Library" used to put its <include> after
// Core.aslx. Core takes its command patterns from the language templates as it loads, so a
// language library included after it only half-translates the game. The editor now recognises a
// language library by its LanguageId template and includes it before Core.aslx. This adds one
// through the real Add Library dialog, then checks the include order in the raw XML, before and
// after the editor reloads the game.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

test('add-language-library', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const fixturesDir = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', 'fixtures');
    const LANGUAGE_LIBRARY = 'language-library-test.aslx';
    const OTHER_LIBRARY = 'custom-library-test.aslx';

    const browser = await chromium.launch();

    function treeItem(page, name) {
        return page.locator('[data-scope="tree-view"] [data-part="item"]').filter({ hasText: name });
    }

    // Included Libraries live under the "Advanced" header, which is collapsed by default.
    async function expandBranch(page, value) {
        const content = page.locator(`[data-value="${value}"][data-part="branch-content"]`);
        for (let i = 0; i < 4; i++) {
            if (await content.isVisible()) return;
            await page
                .locator(`[data-value="${value}"][data-part="branch-control"] button[aria-label="Expand"], [data-value="${value}"][data-part="branch-control"] button[aria-label="Collapse"]`)
                .first()
                .click();
            await page.waitForTimeout(200);
        }
        throw new Error(`Failed to expand tree branch "${value}"`);
    }

    async function addLibrary(page, filename) {
        await expandBranch(page, '_advanced');
        await expandBranch(page, '_include');
        await page.locator('[data-value="_include"][data-part="branch-control"] .node-actions button').click();
        await page.click('.tree-dropdown button:has-text("Add library")');
        await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
        await page.setInputFiles('[role="dialog"] input[type="file"]', path.join(fixturesDir, filename));
        await page.locator(`[role="dialog"] span[title="${filename}"]`).waitFor({ timeout: 5000 });
        await page.click('[role="dialog"] button:has-text("Add library")');
        await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 });
        await treeItem(page, filename).first().waitFor({ state: 'visible', timeout: 10000 });
    }

    // The include refs at the top of the game's own file, in order, from the raw XML code view.
    async function includeOrder(page) {
        await page.click('button[title="Raw XML code view"]');
        await page.waitForSelector('.cm-editor', { timeout: 10000 });
        const xml = await page.locator('.cm-content').first().innerText();
        await page.click('button[title="Raw XML code view"]');
        return [...xml.matchAll(/<include ref="([^"]+)"/g)].map(m => m[1]);
    }

    function assertOrder(actual, expected, when) {
        if (JSON.stringify(actual) !== JSON.stringify(expected)) {
            throw new Error(`${when}: expected includes ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
        }
    }

    try {
        const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
        const page = await ctx.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseUrl}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Language Library Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 30000 });
        console.log('PASS: local draft created and opened in the editor');

        // An ordinary library still goes at the end; the language library goes before Core
        await addLibrary(page, OTHER_LIBRARY);
        await addLibrary(page, LANGUAGE_LIBRARY);
        const expected = ['English.aslx', LANGUAGE_LIBRARY, 'Core.aslx', OTHER_LIBRARY];
        assertOrder(await includeOrder(page), expected, 'After Add Library');
        console.log('PASS: the language library is included before Core.aslx, the other library after it');

        // Reloading proves the saved file has that order, and that the game still loads with it
        await page.click('button:has-text("Reload")');
        await page.waitForSelector('button[title="More"]', { timeout: 90000 });
        await page.waitForSelector('text=Reload the editor to apply your library changes', { state: 'hidden', timeout: 10000 });
        assertOrder(await includeOrder(page), expected, 'After reload');
        console.log('PASS: after the editor reloads the game, the include order is unchanged');

        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
