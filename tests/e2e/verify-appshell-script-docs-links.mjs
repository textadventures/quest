// Verifies the contextual documentation deep-links on script commands:
//   - ScriptEditor's hover "?" on a script row, and the "Help" button in the
//     selection toolbar (the touch path to the same link)
//   - AddScriptModal's footer "Learn more" link for the highlighted command
// Both resolve through docsUrlForScriptKeyword(), backed by the generated
// src/AppShell/src/lib/docs-index.generated.ts. Commands with no reference
// entry (syntax like `=` and `//`) must show no affordance at all.
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5180';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1400, height: 900 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

function check(label, actual, expected) {
    if (actual !== expected) throw new Error(`[${label}] got ${JSON.stringify(actual)}, expected ${JSON.stringify(expected)}`);
    console.log(`PASS: ${label}`);
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Script Docs Links ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // Same route into a script block as verify-appshell-scripteditor-row-buttons.mjs.
    await page.click('text=room');
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Add script")', { timeout: 10000 });

    // --- AddScriptModal: "Learn more" tracks the highlighted command ---
    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 10000 });

    // Single-click selects without confirming (double-click is what adds).
    await page.fill('input[placeholder="Filter commands..."]', 'Print a message');
    await page.locator('[role="option"]:has-text("Print a message")').first().click();
    const learnMore = page.locator('a:has-text("Learn more")');
    check('AddScriptModal "Learn more" href for "Print a message" (msg)',
        await learnMore.getAttribute('href'), 'https://questviva.com/scripts/#msg');

    // A (function)-style command resolves to a reference/functions/* page rather
    // than the scripts page - that's 88 of the index's 125 entries, and a
    // different lookup path in the generator.
    await page.fill('input[placeholder="Filter commands..."]', 'Move object');
    await page.locator('[role="option"]:has-text("Move object")').first().click();
    check('AddScriptModal "Learn more" href for "Move object" ((function)MoveObject)',
        await learnMore.getAttribute('href'), 'https://questviva.com/reference/functions/objects/#moveobject');

    // A command with no reference entry must show no link at all. "=" (set a
    // variable) is syntax, deliberately absent from the generated index.
    await page.fill('input[placeholder="Filter commands..."]', 'Set a variable or attribute');
    const setRow = page.locator('[role="option"]:has-text("Set a variable or attribute")').first();
    if (await setRow.count() !== 1) {
        throw new Error('could not find the "Set a variable or attribute" command to check the no-docs case');
    }
    await setRow.click();
    check('no "Learn more" link for a command with no docs entry (=)',
        await page.locator('a:has-text("Learn more")').count(), 0);

    await page.locator('button:has-text("Cancel")').click();

    // Add a Print command so there's a row to check in the editor.
    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 10000 });
    await page.getByRole('button', { name: 'Print', exact: true }).click();
    await page.waitForSelector('input[type="checkbox"]', { timeout: 10000 });

    // --- ScriptEditor row: hover "?" ---
    const row = page.locator('div.group.relative').first();
    await row.hover();
    const helpButton = row.locator('button[aria-label^="Help for"]');
    if (await helpButton.count() !== 1) {
        throw new Error(`expected exactly one "?" help button on the script row, found ${await helpButton.count()}`);
    }
    const [popup] = await Promise.all([
        page.waitForEvent('popup', { timeout: 10000 }),
        helpButton.click(),
    ]);
    const popupUrl = popup.url();
    await popup.close();
    check('script row "?" opens the msg reference', popupUrl, 'https://questviva.com/scripts/#msg');

    // --- Selection toolbar: "? Help" (the touch path) ---
    await row.locator('input[type="checkbox"]').check();
    const toolbarHelp = page.locator('button:has-text("Help")');
    if (await toolbarHelp.count() !== 1) {
        throw new Error(`expected a "Help" button in the selection toolbar, found ${await toolbarHelp.count()}`);
    }
    const [popup2] = await Promise.all([
        page.waitForEvent('popup', { timeout: 10000 }),
        toolbarHelp.click(),
    ]);
    const popup2Url = popup2.url();
    await popup2.close();
    check('selection toolbar "Help" opens the msg reference', popup2Url, 'https://questviva.com/scripts/#msg');

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-script-docs-links-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
