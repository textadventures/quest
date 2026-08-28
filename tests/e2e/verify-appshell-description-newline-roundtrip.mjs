// Verifies the fix for room descriptions collapsing linebreaks in the player: a multi-line
// richtext field (e.g. Room > Description) must be saved as literal <br/> tags in the underlying
// ASLX (FieldSaver.StringSaver, src/Engine/GameLoader/FieldSaver.cs), matching the old Quest 5
// desktop editor's RichTextControl save format - and, symmetrically, must show real linebreaks
// (not literal <br/> markup) when the field is redisplayed for editing (WasmEditorBridge's
// AddDropdownTypeValues richtext-reversal, src/WasmEditor/WasmEditorBridge.cs), matching
// RichTextControl.Populate.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function openRoomDescription(page) {
    await page.click('text=room');
    await page.getByRole('button', { name: /^Room$/ }).click();
    await page.waitForSelector('#richtext-description', { timeout: 5000 });
}

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    const gameName = `Description Newline Test ${Date.now()}`;
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', gameName);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    await openRoomDescription(page);
    await page.locator('#richtext-description').fill('Line one\nLine two');
    await page.keyboard.press('Tab'); // blur to fire the textarea's onchange handler
    console.log('PASS: typed a multi-line description into the richtext field');

    // --- Saved form: underlying ASLX must contain literal <br/>, not a raw newline ---
    await page.click('button[title="Raw XML code view"]');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    const xmlText = await page.locator('.cm-content').first().innerText();
    if (!xmlText.includes('Line one<br/>Line two')) {
        throw new Error(`Expected the raw XML to contain "Line one<br/>Line two", got description area: ${xmlText.match(/<description>[\s\S]{0,80}/)?.[0]}`);
    }
    if (xmlText.includes('Line one\nLine two') && !xmlText.includes('Line one<br/>Line two')) {
        throw new Error('Raw XML contains a literal unconverted newline in the description field');
    }
    console.log('PASS: saved ASLX stores the description with a literal <br/> tag, not a raw newline');
    await page.click('button[title="Raw XML code view"]'); // close (no edits made, so this shouldn't prompt)
    await page.waitForSelector('[data-value="game"][data-part="branch-control"]', { timeout: 5000 });

    // --- Editing UX: navigating away and back must show real linebreaks, not "<br/>" markup ---
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await openRoomDescription(page);
    const reopenedValue = await page.locator('#richtext-description').inputValue();
    if (reopenedValue.includes('<br')) {
        throw new Error(`Expected the richtext textarea to show real linebreaks, but found literal markup: ${JSON.stringify(reopenedValue)}`);
    }
    if (reopenedValue !== 'Line one\nLine two') {
        throw new Error(`Expected textarea value "Line one\\nLine two", got ${JSON.stringify(reopenedValue)}`);
    }
    console.log('PASS: reopening the field shows real linebreaks, not <br/> markup (matches v5 desktop editor UX)');

    // --- Persists across a full reload (autosave -> OPFS -> reopen -> re-populate) ---
    // A bare page.reload() just redirects to /open (isLoaded is in-memory SPA state), so reopen
    // the draft by name via /open instead, matching verify-appshell-autosave.mjs's convention.
    await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('text=Your local drafts', { timeout: 10000 });
    await page.click(`button:has-text("${gameName}.aslx")`);
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await openRoomDescription(page);
    const afterReloadValue = await page.locator('#richtext-description').inputValue();
    if (afterReloadValue !== 'Line one\nLine two') {
        throw new Error(`Expected textarea value "Line one\\nLine two" after reload, got ${JSON.stringify(afterReloadValue)}`);
    }
    console.log('PASS: round-trip survives a full page reload (autosaved draft reloaded from OPFS)');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
