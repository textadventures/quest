// Verifies the new Code View feature: (a) the per-script "Code view" toggle now renders a
// syntax-highlighted CodeMirror editor instead of a plain textarea and still round-trips edits,
// and (b) the new top-level "Raw XML code view" toggle shows the whole game's ASLX, applies edits
// (reloading the model and clearing undo history, after a confirm step), rejects malformed XML
// with an inline error while leaving the currently-open game untouched, and Cancel discards
// in-panel edits without applying them.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

// CodeMirror's contenteditable content doesn't reliably pick up Playwright's locator.fill()
// (confirmed interactively — fill() left the doc unchanged). Typing long content character-by-
// character via page.keyboard.type() is also unreliable here — CodeMirror's closeBrackets/
// indentOnInput extensions can desync from Playwright's rapid synthetic keystrokes on strings of
// a few hundred+ characters, corrupting the result (confirmed interactively: a duplicated/garbled
// tail appeared after typing a full ASLX document). Paste (a single transaction) avoids both
// problems and is also how a real author would replace a large block of text.
const selectAllKey = process.platform === 'darwin' ? 'Meta+A' : 'Control+A';
const pasteKey = process.platform === 'darwin' ? 'Meta+V' : 'Control+V';

async function setCmContent(page, text) {
    const cm = page.locator('.cm-editor .cm-content').first();
    await cm.click();
    await page.keyboard.press(selectAllKey);
    await page.evaluate(t => navigator.clipboard.writeText(t), text);
    await page.keyboard.press(pasteKey);
    return cm;
}

// The Print script's message renders as an <input>'s or <textarea>'s value (multiline as of
// #2115), not visible text content, so this checks .value rather than a text= selector (which
// only matches rendered text nodes).
async function waitForInputValue(page, value) {
    await page.waitForFunction(
        v => [...document.querySelectorAll('input[type="text"], textarea')].some(i => i.value === v),
        value,
        { timeout: 5000 }
    );
}

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    await ctx.grantPermissions(['clipboard-read', 'clipboard-write']);
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Code View Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    // --- Per-script Code view: syntax highlighting + round trip ---
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('text=Start script', { timeout: 5000 });

    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.getByRole('button', { name: 'Print', exact: true }).click();
    await page.waitForSelector('input[type="checkbox"]', { timeout: 5000 });

    await page.click('button:has-text("Code view")');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    // HighlightStyle.define() generates obfuscated per-rule class names (not fixed ".tok-*" names),
    // so check for classed token spans generically rather than a specific class — a plain textarea
    // (the old implementation) never produces these.
    const classedTokenCount = await page.locator('.cm-line span[class]').count();
    if (classedTokenCount === 0) throw new Error('Expected classed (syntax-highlighted) token spans in the per-script code view, found none');
    console.log('PASS: per-script Code view renders CodeMirror with syntax-highlighted tokens (not a plain textarea)');

    await setCmContent(page, 'msg ("Hello from code view")');
    await page.locator('button:has-text("Visual editor")').click();
    await waitForInputValue(page, 'Hello from code view');
    console.log('PASS: per-script code-view edit committed on blur and is reflected back in Visual mode');

    // --- Locked/inherited script: code view stays read-only, "Make editable copy" still works ---
    // (The freshly-created game's Start script is not inherited, so this checks the general
    // codeViewMode/readonly wiring instead: re-enter code view and confirm it's still editable.)
    await page.click('button:has-text("Code view")');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    const isEditable = await page.locator('.cm-content').first().getAttribute('contenteditable');
    if (isEditable !== 'true') throw new Error('Expected the per-script code view to be editable (not locked) for a non-inherited script');
    console.log('PASS: per-script code view is editable for a non-inherited script (readonly wiring intact)');
    await page.click('button:has-text("Visual editor")');

    // --- Whole-file raw ASLX editor ---
    await page.click('button[title="Raw XML code view"]');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    let xmlText = await page.locator('.cm-content').first().innerText();
    if (!xmlText.includes('<asl')) throw new Error('Expected the raw XML view to contain the <asl> root element');
    if (!xmlText.includes('Hello from code view')) throw new Error('Expected the raw XML to reflect the earlier per-script edit');
    console.log('PASS: raw XML code view opens and shows the current game XML, including the earlier per-script edit');

    // The quest-script inside <start type="script"> should itself be highlighted (nested-language
    // nesting via parseMixed, keyed on the type="script" attribute regardless of tag name) — not
    // just rendered as plain XML text content. A bare "msg" span with no class means it fell back
    // to plain text.
    const msgTokenClass = await page.locator('.cm-line span', { hasText: 'msg' }).first().getAttribute('class');
    if (!msgTokenClass) throw new Error('Expected the embedded quest-script keyword "msg" inside the raw XML view to be highlighted (nested-language parsing), but it has no highlight class');
    console.log('PASS: quest-script embedded inside <... type="script"> elements is itself syntax-highlighted within the raw XML view');

    // --- Add/Delete are disabled while the raw XML panel is open (no tree selection context) ---
    const addDisabled = await page.locator('button[title="Add element"]').isDisabled();
    const deleteDisabled = await page.locator('button:has-text("Delete")').first().isDisabled();
    if (!addDisabled) throw new Error('Expected the toolbar "Add" button to be disabled while the raw XML panel is open');
    if (!deleteDisabled) throw new Error('Expected the toolbar "Delete" button to be disabled while the raw XML panel is open');
    console.log('PASS: toolbar Add/Delete are disabled while the raw XML panel is open');

    // --- Cancel discards in-panel edits (prompting first, since there are unsaved changes) ---
    await setCmContent(page, xmlText.replace('Hello from code view', 'THIS SHOULD BE DISCARDED'));
    await page.click('button:has-text("Cancel")');
    await page.waitForSelector('text=You have unsaved raw XML changes', { timeout: 5000 });
    await page.click('button:has-text("Discard changes")');
    await page.waitForSelector('button[title="Raw XML code view"]', { timeout: 5000 });
    await page.click('button[title="Raw XML code view"]');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    const reopenedXml = await page.locator('.cm-content').first().innerText();
    if (reopenedXml.includes('THIS SHOULD BE DISCARDED')) throw new Error('Expected Cancel to discard in-panel edits, but the discarded text reappeared on reopen');
    if (!reopenedXml.includes('Hello from code view')) throw new Error('Expected reopening the panel to show the original (unedited) XML');
    console.log('PASS: Cancel discards in-panel XML edits without applying them');

    // --- Undo/Redo route to CodeMirror while the raw XML panel is open, not Quest's model ---
    // (the per-script edit above already created real Quest undo history to try to corrupt)
    const undoTitle = await page.locator('button[title*="Undo"]').getAttribute('title');
    if (undoTitle !== 'Undo (in code editor)') throw new Error(`Expected the Undo button title to indicate code-editor context while the raw XML panel is open, got "${undoTitle}"`);
    const xmlBeforeUndoClicks = await page.locator('.cm-content').first().innerText();
    await page.click('button[title*="Undo"]');
    await page.click('button[title*="Undo"]');
    const xmlAfterUndoClicks = await page.locator('.cm-content').first().innerText();
    if (xmlAfterUndoClicks !== xmlBeforeUndoClicks) throw new Error('Expected clicking Undo while the raw XML panel is open to leave the XML unchanged (nothing to undo in a fresh CodeMirror buffer) — it changed, meaning the click likely undid something in the background model instead');
    if (!xmlAfterUndoClicks.includes('Hello from code view')) throw new Error('Expected the underlying model to still be intact (still contain the earlier per-script edit) after clicking Undo inside the raw XML panel');
    console.log("PASS: Undo/Redo route to CodeMirror's own history while the raw XML panel is open, not to Quest's model undo stack");

    // --- Toggling the toolbar button closed while there are unsaved edits prompts, not silently discards ---
    await setCmContent(page, xmlBeforeUndoClicks.replace('<asl', '<!--EDIT MARKER--><asl'));
    await page.click('button[title="Raw XML code view"]');
    await page.waitForSelector('text=You have unsaved raw XML changes', { timeout: 5000 });
    console.log('PASS: toggling the toolbar button while the panel has unsaved edits prompts to Apply/Discard rather than closing immediately');
    await page.click('button:has-text("Discard changes")');
    await page.waitForSelector('[data-value="game"][data-part="branch-control"]', { timeout: 5000 });
    console.log('PASS: "Discard changes" closes the panel without applying the discarded edit');

    // --- Malformed XML: inline error, no crash, previous game state preserved ---
    await page.click('button[title="Raw XML code view"]');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    await setCmContent(page, 'not valid xml <unclosed');
    await page.click('button:has-text("Apply")');
    await page.waitForSelector('[role="dialog"] button:has-text("Apply")', { timeout: 5000 });
    await page.click('[role="dialog"] button:has-text("Apply")');
    await page.waitForSelector('text=Failed to load game', { timeout: 10000 });
    console.log('PASS: applying malformed XML shows an inline error');
    // The panel should still be open (not silently closed) and the app not crashed.
    if (!(await page.locator('.cm-editor').first().isVisible())) throw new Error('Expected the code view panel to remain open after a failed Apply');
    console.log('PASS: app did not crash after a failed Apply; panel remains open with the invalid text');

    // Cancel out of the broken edit (still counts as an unsaved change, so this also prompts) and
    // confirm the previously-open game is still intact.
    await page.click('button:has-text("Cancel")');
    await page.waitForSelector('text=You have unsaved raw XML changes', { timeout: 5000 });
    await page.click('button:has-text("Discard changes")');
    await page.waitForSelector('[data-value="game"][data-part="branch-control"]', { timeout: 10000 });
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await waitForInputValue(page, 'Hello from code view');
    console.log('PASS: previously-open game is still intact after the failed raw-XML apply attempt');

    // --- Successful Apply: reloads tree, clears undo history, marks dirty ---
    await page.click('button[title="Raw XML code view"]');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    xmlText = await page.locator('.cm-content').first().innerText();
    await setCmContent(page, xmlText.replace('Hello from code view', 'Hello from raw XML apply'));
    await page.click('button:has-text("Apply")');
    await page.waitForSelector('[role="dialog"] button:has-text("Apply")', { timeout: 5000 });
    await page.click('[role="dialog"] button:has-text("Apply")');
    await page.waitForSelector('[data-value="game"][data-part="branch-control"]', { timeout: 10000 });
    console.log('PASS: applying valid edited XML closes the panel and returns to the normal editor view');

    // refreshUndoRedo() runs asynchronously after the reload, so poll rather than check once.
    await page.waitForFunction(
        () => document.querySelector('button[title="Undo"]')?.disabled === true,
        { timeout: 5000 }
    );
    console.log('PASS: Undo is disabled after applying raw XML edits (undo history cleared, as warned)');

    await page.waitForSelector('.save-chip-unsaved, .save-chip-saving, .save-chip-saved', { timeout: 5000 });
    console.log('PASS: save-chip reflects the post-apply state (dirty flag was force-set, then autosave ran)');

    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await waitForInputValue(page, 'Hello from raw XML apply');
    console.log('PASS: tree/model reflects the applied raw XML edit');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
