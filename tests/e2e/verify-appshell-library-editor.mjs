// Verifies the included-library tree editor: selecting an Included Library node in the tree shows
// a CodeMirror editor for the library's .aslx file (like the JavaScript editor), built-in
// engine-shipped libraries are read-only, custom uploaded libraries are editable, and editing one
// writes the file and shows the LibraryReloadBanner (the edit only takes effect after reload).
// Also checks that malformed edits are refused before anything is written, so the game can't be
// reloaded into an unloadable state.
import { chromium } from './lib/tracked-chromium.mjs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const fixture = path.join(path.dirname(fileURLToPath(import.meta.url)), 'fixtures', 'custom-library-test.aslx');

const browser = await chromium.launch();

// Same paste-based content replacement as the code-view test (see its header comment for why).
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

function treeItem(page, name) {
    return page.locator('[data-scope="tree-view"] [data-part="item"]').filter({ hasText: name });
}

// Included Libraries live under the "Advanced" header, which is collapsed by default. The expand
// toggle is the chevron button inside the branch-control (not the control itself), so click that.
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

async function revealLibraries(page) {
    await expandBranch(page, '_advanced');
    await expandBranch(page, '_include');
}

const CUSTOM_LIBRARY = 'custom-library-test.aslx';
const EDITED_MARKER = 'Edited in the library tree editor';

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    await ctx.grantPermissions(['clipboard-read', 'clipboard-write']);
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Library Editor Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');
    await revealLibraries(page);

    // --- Built-in library: selecting the Core.aslx include node shows a read-only XML editor ---
    await treeItem(page, 'Core.aslx').first().click();
    await page.waitForSelector('.cm-editor', { timeout: 10000 });
    const readOnly = await page.locator('.cm-content').first().getAttribute('contenteditable');
    if (readOnly !== 'false') throw new Error(`Expected the built-in library editor to be read-only (contenteditable=false), got "${readOnly}"`);
    const coreContent = await page.locator('.cm-content').first().innerText();
    if (!coreContent.includes('<library')) throw new Error('Expected the built-in library editor to show the Core.aslx XML (starting with <library>)');
    if (coreContent.trim().length < 1000) throw new Error(`Expected the built-in Core.aslx content to be substantial, got only ${coreContent.trim().length} chars`);
    console.log('PASS: selecting the Core.aslx include node shows the built-in library XML in a read-only editor');

    await page.waitForSelector(`text=${'This is one of the built-in libraries that ships with the editor'}`);
    console.log('PASS: the read-only notice explains the library is built-in and not part of the game file');

    // --- English.aslx is also read-only (all engine-shipped libraries) ---
    await treeItem(page, 'English.aslx').first().click();
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    const englishReadOnly = await page.locator('.cm-content').first().getAttribute('contenteditable');
    if (englishReadOnly !== 'false') throw new Error(`Expected the English.aslx library editor to be read-only too, got "${englishReadOnly}"`);
    const englishContent = await page.locator('.cm-content').first().innerText();
    if (!englishContent.includes('<library')) throw new Error('Expected the English.aslx editor to show library XML');
    console.log('PASS: the English.aslx built-in library is also read-only');

    // --- Add a custom library via the Included Libraries header menu ---
    await page.locator('[data-value="_include"] .node-actions button').click();
    await page.waitForSelector('.tree-dropdown button:has-text("Add library")', { timeout: 5000 });
    await page.click('.tree-dropdown button:has-text("Add library")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.setInputFiles('[role="dialog"] input[type="file"]', fixture);
    await page.waitForFunction(() => document.querySelector('[role="dialog"] button') !== null, { timeout: 5000 });
    await page.click('[role="dialog"] button:has-text("Add library")');
    await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 }).catch(() => {});
    await treeItem(page, CUSTOM_LIBRARY).first().waitFor({ state: 'visible', timeout: 10000 });
    console.log('PASS: custom .aslx library uploaded and added via the Included Libraries header menu');
    // Adding a library legitimately raises the reload banner; dismiss it so the later no-edit-blur
    // regression check can distinguish a spurious banner from this one.
    await page.click('button:has-text("Dismiss")');
    await page.waitForSelector('text=Reload the editor to apply your library changes', { state: 'hidden', timeout: 5000 });

    // --- Custom library: editable XML editor with the uploaded content ---
    await treeItem(page, CUSTOM_LIBRARY).first().click();
    await page.waitForSelector('.cm-editor', { timeout: 10000 });
    const customEditable = await page.locator('.cm-content').first().getAttribute('contenteditable');
    if (customEditable !== 'true') throw new Error(`Expected the custom library editor to be editable (contenteditable=true), got "${customEditable}"`);
    let customContent = await page.locator('.cm-content').first().innerText();
    if (!customContent.includes('MyLibraryGreeting')) throw new Error('Expected the custom library editor to show the uploaded .aslx content');
    console.log('PASS: selecting the custom library shows the uploaded .aslx content in an editable editor');

    // --- Regression: focus/blur with no edit must NOT write the file or raise the banner ---
    // (CodeEditor's onChange fires on every blur; the editor-store write must skip a no-op.)
    // Blur via the tree's filter input: it takes focus without changing the tree selection, so
    // the library editor stays on screen (blurring via a tree-node click would select that node).
    await page.locator('.cm-editor .cm-content').first().click();
    await page.locator('input[aria-label="Filter game objects"]').click();
    await page.waitForTimeout(1000);
    if (await page.locator('text=Reload the editor to apply your library changes').count() > 0) {
        throw new Error('Expected no reload banner after blurring the library editor without making any changes');
    }
    console.log('PASS: blurring the library editor without changes does not raise the reload banner');

    // --- Edit it, then blur to commit; the reload banner should appear (edit only takes effect after reload) ---
    // Function scripts inside library files use the CDATA form (see GameXmlWriter.UseCDATA), as in
    // Core.aslx itself — a literal child <msg> element is not valid there.
    await setCmContent(page, `<library>\n  <function name="MyLibraryGreeting">\n    <![CDATA[<msg>${EDITED_MARKER}</msg>]]>\n  </function>\n</library>`);
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.waitForSelector('text=Reload the editor to apply your library changes', { timeout: 5000 });
    console.log('PASS: editing the custom library writes the file and shows the LibraryReloadBanner on blur');

    // --- Reload and confirm the edit persisted (file was actually written) ---
    await page.click('button:has-text("Reload")');
    // WASM re-initialisation + game reload can take a while on slower first runs, so allow longer.
    await page.waitForSelector('button[title="More"]', { timeout: 90000 });
    await page.waitForSelector('text=Reload the editor to apply your library changes', { state: 'hidden', timeout: 5000 });
    await revealLibraries(page);
    await treeItem(page, CUSTOM_LIBRARY).first().click();
    await page.waitForSelector('.cm-editor', { timeout: 10000 });
    customContent = await page.locator('.cm-content').first().innerText();
    if (!customContent.includes(EDITED_MARKER)) throw new Error('Expected the edited library content to survive a reload (proving the file was written to the adapter)');
    if (!customContent.includes('MyLibraryGreeting')) throw new Error('Expected the reloaded library to still be a valid library (function retained)');
    console.log('PASS: after Reload the edited library content is still there, proving the file write persisted');

    // --- Regression: a malformed edit is refused, so the game can't be reloaded into an
    //     unloadable state ---
    // The tree editor validates the same way as the Code View panel's Apply (putLibraryAssetText
    // funnels through the throwaway-controller check), so broken XML must never reach the adapter.
    await setCmContent(page, 'not valid xml <unclosed');
    await page.locator('input[aria-label="Filter game objects"]').click();
    await page.waitForTimeout(1500);
    if (await page.locator('text=Reload the editor to apply your library changes').count() > 0) {
        throw new Error('Expected no reload banner after an invalid library edit: a malformed edit must be refused, not committed');
    }
    console.log('PASS: a malformed library edit is refused (no banner, nothing written)');

    // Had the invalid edit above actually been committed to the adapter, the reload below would
    // fail to load the game. Coming back up healthy with the valid content is the regression check.
    await setCmContent(page, `<library>\n  <function name="MyLibraryGreeting">\n    <![CDATA[<msg>${EDITED_MARKER} after a rejected edit</msg>]]>\n  </function>\n</library>`);
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.waitForSelector('text=Reload the editor to apply your library changes', { timeout: 5000 });
    await page.click('button:has-text("Reload")');
    await page.waitForSelector('button[title="More"]', { timeout: 90000 });
    await page.waitForSelector('text=Reload the editor to apply your library changes', { state: 'hidden', timeout: 5000 });
    await revealLibraries(page);
    await treeItem(page, CUSTOM_LIBRARY).first().click();
    await page.waitForSelector('.cm-editor', { timeout: 10000 });
    customContent = await page.locator('.cm-content').first().innerText();
    if (!customContent.includes('after a rejected edit')) throw new Error('Expected the game to reload cleanly after the malformed edit was refused, with the valid library content intact');
    console.log('PASS: the game still reloads cleanly after the malformed edit was refused');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
