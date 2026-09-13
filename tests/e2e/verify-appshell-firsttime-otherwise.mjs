// Regression test for a user-reported bug: adding a "First time..." script via the visual
// editor let you add commands to the "first time" branch, but the "Otherwise" branch's own
// "Add script" adder silently did nothing - the modal closed but no script was added. Root
// cause: FirstTimeScript's otherwise parameter starts out null (unlike "if"'s else clause,
// which has its own dedicated "Add else" step to materialize it first), so the generic
// nested-script-parameter container resolution in WasmEditorBridge.ResolveContainer returned
// null for it. Fixed by having AddScript/PasteScripts lazily materialize a missing nested
// script container instead of failing.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `FirstTime Otherwise Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.click('text=room');
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Add script")', { timeout: 5000 });

    // Add a "First time..." script at root, using the modal's filter box to find it
    // regardless of which category tab is active by default.
    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.fill('[role="dialog"] input[type="text"]', 'First time');
    await page.waitForTimeout(300);
    await page.getByRole('option', { name: /First time/ }).first().click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.waitForSelector('text=Otherwise', { timeout: 5000 });
    console.log('PASS: added a firsttime script, Otherwise branch rendered');

    // Click the "otherwise" branch's own Add script button. The "Otherwise" label is a plain
    // <span> immediately followed by a sibling wrapper div containing that branch's nested
    // ScriptEditor (with its own Add script button) - locate that specific button structurally
    // rather than guessing an ordinal among every "+ Add script" button on the page (the room's
    // Scripts tab has several independent script slots, each with their own root-level adder).
    const clickedOtherwiseAdd = await page.evaluate(() => {
        const spans = Array.from(document.querySelectorAll('span'))
            .filter(s => s.textContent?.trim() === 'Otherwise');
        for (const span of spans) {
            let sib = span.nextElementSibling;
            while (sib) {
                const btn = Array.from(sib.querySelectorAll('button'))
                    .find(b => b.textContent?.includes('Add script'));
                if (btn) {
                    btn.click();
                    return true;
                }
                sib = sib.nextElementSibling;
            }
        }
        return false;
    });
    if (!clickedOtherwiseAdd) throw new Error('Could not locate/click the otherwise branch Add script button');

    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.getByRole('button', { name: 'Print', exact: true }).click();
    await page.waitForTimeout(500);

    const dialogStillOpen = await page.locator('[role="dialog"]').count();
    if (dialogStillOpen !== 0) throw new Error('AddScriptModal did not close after adding to the otherwise branch');

    // Switch to code view to directly verify the saved script text contains an "otherwise"
    // clause with the Print command inside it - the ground truth for whether the add worked.
    await page.click('button:has-text("Code view")');
    await page.waitForSelector('.cm-content', { timeout: 5000 });
    await page.waitForTimeout(300);
    const code = await page.locator('.cm-content').innerText();
    console.log('--- code view ---');
    console.log(code);
    console.log('--- end code view ---');

    if (!/otherwise\s*\{/i.test(code)) {
        throw new Error('Expected an "otherwise {" clause in the saved script after adding to the Otherwise branch - the add silently failed');
    }
    if (!/msg\s*\(/i.test(code)) {
        throw new Error('Expected the added Print (msg) command to appear in the saved script');
    }
    console.log('PASS: otherwise branch now contains the added script');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
