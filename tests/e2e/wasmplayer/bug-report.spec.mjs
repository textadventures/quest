// The Save/Load dialog's "Copy for a bug report" (#2673) copies what an author needs to replay a
// player's game with a walkthrough: the game's random seed and the player's commands as steps, and,
// when the game was loaded from a save, a note that the save is needed too.
//
// Checks the copied text after two commands, the fallback that shows the text when the clipboard
// can't be written, and that the steps start again (and the note appears) after loading a save.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

test('bug-report', async ({ baseURL }) => {
    const baseUrl = baseURL;

    const browser = await chromium.launch();
    const context = await browser.newContext();
    await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: new URL(baseUrl).origin });
    const page = await context.newPage();
    const pageErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));

    const commandInput = page.locator('#txtCommand:visible, qv-command-bar input:visible').first();

    async function sendCommand(command) {
        await page.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
        await commandInput.fill(command);
        await commandInput.press('Enter');
        await page.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
    }

    // Opens the Save/Load dialog, clicks "Copy for a bug report" and returns the status line and the
    // copied (or shown) text.
    async function copyReport() {
        await page.evaluate(() => openSavesDialog('manage'));
        const button = page.locator('#qv-saves-bug-report');
        await button.waitFor({ state: 'visible', timeout: 5000 });
        await button.click();
        const status = page.locator('#qv-saves-bug-report-status');
        await status.waitFor({ state: 'visible', timeout: 5000 });
        const result = {
            status: await status.textContent(),
            textareaVisible: await page.locator('#qv-saves-bug-report-text').isVisible(),
            shown: await page.locator('#qv-saves-bug-report-text').inputValue(),
            clipboard: await page.evaluate(() => navigator.clipboard.readText().catch(() => '')),
        };
        await page.evaluate(() => document.getElementById('qv-saves').close());
        return result;
    }

    try {
        await page.goto(`${baseUrl}/?url=/examples/simple.aslx`);
        await page.waitForSelector('#txtCommand, qv-command-bar input', { timeout: 30000, state: 'attached' });

        await sendCommand('look');
        await sendCommand('inventory');
        const seed = await page.evaluate(() => Bridge.GetRandomSeed());

        const first = await copyReport();
        if (!first.status.includes('Copied')) throw new Error(`Expected a "Copied" status, got "${first.status}"`);
        if (first.textareaVisible) throw new Error('The text box should stay hidden when the clipboard write worked');
        const report = first.clipboard;
        if (!report.includes(`Random seed: ${seed}`)) throw new Error(`Report doesn't give the game's seed ${seed}:\n${report}`);
        if (!report.includes('Started from: the beginning')) throw new Error(`Report should say the game started from the beginning:\n${report}`);
        if (!report.endsWith('Steps (2), to paste into a walkthrough with the same random seed:\nlook\ninventory')) {
            throw new Error(`Report's steps aren't the two commands:\n${report}`);
        }
        console.log('PASS: the copied report has the seed and the commands as steps');

        // No clipboard access (e.g. an iframe that isn't allowed it): the text is shown to copy by hand.
        await page.evaluate(() => { navigator.clipboard.writeText = () => Promise.reject(new Error('blocked')); });
        const blocked = await copyReport();
        if (!blocked.textareaVisible) throw new Error('The text box should appear when the clipboard write fails');
        if (blocked.shown !== report) throw new Error(`The shown text differs from the copied report:\n${blocked.shown}`);
        console.log('PASS: without clipboard access the report is shown to copy by hand');

        // After loading a save, the steps start again from the save and the report says the save is needed.
        await page.evaluate(async () => {
            await Bridge.PrepareSaveGame('');
            await restartGameCore(Bridge.GetSaveGameBytes());
        });
        await sendCommand('look');
        const loaded = await copyReport();
        if (!loaded.shown.includes('Started from: a saved game')) throw new Error(`Report should say the game was loaded from a save:\n${loaded.shown}`);
        if (!loaded.shown.includes(`Random seed: ${seed}`)) throw new Error(`The loaded save should keep its seed ${seed}:\n${loaded.shown}`);
        if (!loaded.shown.endsWith('Steps (1), to paste into a walkthrough with the same random seed:\nlook')) {
            throw new Error(`After loading, the steps should start again:\n${loaded.shown}`);
        }
        console.log('PASS: after loading a save the report starts its steps there and asks for the save');

        if (pageErrors.length) throw new Error(`Page errors: ${JSON.stringify(pageErrors)}`);
        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
