// #2669: the code editor's completion list opens by itself as you type, and Enter used to accept
// whatever it had selected. So typing "game.pov" and pressing Enter to end the line silently
// became "game.pov_alias". Checks that Enter now always inserts a newline, leaving the typed word
// alone, and that Tab still accepts the selected completion.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('code-view-completion-enter', async ({ baseURL }) => {
    const baseUrl = baseURL;

    const browser = await chromium.launch();

    const selectAllKey = process.platform === 'darwin' ? 'Meta+A' : 'Control+A';
    const completionPopup = '.cm-tooltip-autocomplete';

    // CodeMirror ignores accept keys for interactionDelay (75ms) after the list opens, so wait past
    // that before pressing one; otherwise a key could be ignored for that reason instead.
    async function waitForCompletionList(page) {
        await page.waitForSelector(completionPopup, { timeout: 5000 });
        await page.waitForTimeout(300);
    }

    async function clearCm(page) {
        await page.locator('.cm-editor .cm-content').first().click();
        await page.keyboard.press(selectAllKey);
        await page.keyboard.press('Backspace');
    }

    async function cmText(page) {
        return page.locator('.cm-content').first().innerText();
    }

    try {
        const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
        const page = await ctx.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseUrl}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Completion Enter Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="More"]', { timeout: 30000 });

        await page.locator('[data-value="game"][data-part="branch-control"]').click();
        await page.getByRole('button', { name: /^Scripts$/ }).click();
        await page.waitForSelector('text=Start script', { timeout: 5000 });
        await page.click('button:has-text("Code view")');
        await page.waitForSelector('.cm-editor', { timeout: 5000 });

        // "paren" is a prefix of the "parent" attribute every game has, so the list opens on it.
        // --- Enter with the list open inserts a newline and keeps the typed word ---
        await clearCm(page);
        await page.keyboard.type('x = player.paren');
        await waitForCompletionList(page);
        await page.keyboard.press('Enter');
        await page.keyboard.type('y = 1');
        const afterEnter = await cmText(page);
        if (afterEnter.includes('player.parent')) {
            throw new Error(`Enter accepted the completion:\n${afterEnter}`);
        }
        if (!/x = player\.paren\s*\n\s*y = 1/.test(afterEnter)) {
            throw new Error(`Expected "x = player.paren" then "y = 1" on the next line, got:\n${afterEnter}`);
        }
        if (await page.locator(completionPopup).count() > 0) {
            throw new Error('The completion list is still open on the new line');
        }
        console.log('PASS: Enter with the completion list open inserts a newline and keeps the typed word');

        // --- Tab still accepts the selected completion ---
        await clearCm(page);
        await page.keyboard.type('x = player.paren');
        await waitForCompletionList(page);
        await page.keyboard.press('Tab');
        const afterTab = await cmText(page);
        if (!/^x = player\.parent\s*$/.test(afterTab)) {
            throw new Error(`Expected Tab to complete to "x = player.parent", got:\n${afterTab}`);
        }
        console.log('PASS: Tab accepts the selected completion');

        // --- The arrow keys still move through the open list rather than the cursor ---
        await clearCm(page);
        await page.keyboard.type('x = player.');
        await waitForCompletionList(page);
        const firstSelected = await page.locator(`${completionPopup} li[aria-selected]`).innerText();
        await page.keyboard.press('ArrowDown');
        const secondSelected = await page.locator(`${completionPopup} li[aria-selected]`).innerText();
        if (firstSelected === secondSelected) {
            throw new Error(`ArrowDown didn't move the completion selection (still "${firstSelected}")`);
        }
        console.log('PASS: ArrowDown moves through the completion list');

        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
