// A save a game's own script asks for (request (RequestSave, "")) prints "Game saved." where it
// asked, between what the script printed before and after, and saves the whole turn (#2985).
// It used to be sent to the UI straight away, ahead of the turn's buffered text, and saved 100ms
// later, so the message landed before, inside or after the turn's text depending on timing, and
// the save could miss the end of the turn.
//
// Plays fixtures/save-request-order-test.aslx on both player UIs. Its savenow command prints
// BEFORE-SAVE, asks for a save, does slow work, then prints AFTER-SAVE.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { test } from '@playwright/test';
import { chromium } from 'playwright';

const uis = [
    { ui: 'legacy', output: '#divOutput', input: '#txtCommand' },
    { ui: 'v2', output: 'qv-output', input: 'qv-command-bar input' },
];

for (const { ui, output, input } of uis) {
    test(`save-request-order (${ui})`, async ({ baseURL }) => {
        const browser = await chromium.launch();
        try {
            const page = await browser.newPage();
            const errors = [];
            page.on('pageerror', (err) => errors.push(String(err)));

            const outputText = () => page.$eval(output, el => el.textContent);

            await page.goto(`${baseURL}/?url=/e2e-fixtures/save-request-order-test.aslx&ui=${ui}`);
            await page.waitForFunction(
                (sel) => !document.getElementById('qv-start') && document.querySelector(sel)?.textContent.includes('You are in'),
                output, { timeout: 30000 })
                .catch(() => { throw new Error(`the game never started on ${ui}`); });

            await page.fill(input, 'savenow');
            await page.press(input, 'Enter');

            await page.waitForFunction(
                (sel) => document.querySelector(sel)?.textContent.includes('AFTER-SAVE'),
                output, { timeout: 15000 })
                .catch(() => { throw new Error('the savenow turn never finished'); });

            // The save finishes after the turn: wait for it rather than for a fixed time.
            const saves = await page.waitForFunction(async () => {
                const list = await GameSaver.listSaves();
                return list.length ? list : null;
            }, null, { timeout: 10000, polling: 100 })
                .then(handle => handle.jsonValue())
                .catch(() => { throw new Error('no save was made'); });
            if (saves.length !== 1) throw new Error(`expected one save, got ${saves.length}`);

            const text = await outputText();
            const before = text.indexOf('BEFORE-SAVE');
            const saved = text.indexOf('Game saved.');
            const after = text.indexOf('AFTER-SAVE');
            if (saved < 0) throw new Error(`no "Game saved." in the output: ${text.slice(-200)}`);
            if (text.indexOf('Game saved.', saved + 1) >= 0) throw new Error('"Game saved." was printed twice');
            if (!(before < saved && saved < after)) {
                throw new Error(`expected BEFORE-SAVE, Game saved., AFTER-SAVE in that order, got: ${text.slice(-200)}`);
            }

            // The save has the whole turn: its transcript ends with AFTER-SAVE.
            const savedXml = await page.evaluate(async (slotIndex) => {
                const bytes = await GameSaver.load(slotIndex);
                return new TextDecoder().decode(bytes);
            }, saves[0].slotIndex);
            if (!savedXml.includes('AFTER-SAVE')) throw new Error("the save doesn't have the end of the turn it was asked for in");

            if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
        } finally {
            await browser.close();
        }
    });
}
