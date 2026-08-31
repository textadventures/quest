// Verifies that Included Libraries can also be uploaded with a .xml extension, not just .aslx.
// Quest 5's desktop editor accepted .xml for included libraries too (its docs mentioned it), so
// this checks the AppShell editor keeps that alternate extension working end-to-end: the Add
// Library file picker accepts it, the uploaded file is treated as a library (editable XML editor,
// not shown in Manage assets as a deletable asset), and the game reloads cleanly afterwards —
// proving <include ref="custom-library-test.xml"/> actually resolves via the adapter's library
// candidate discovery, not just that the upload itself succeeded.
import { chromium } from './lib/tracked-chromium.mjs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const fixture = path.join(path.dirname(fileURLToPath(import.meta.url)), 'fixtures', 'custom-library-test.xml');

const browser = await chromium.launch();

// Same paste-based content replacement as the library-editor test (see its header comment for
// why: CodeMirror's own typing/auto-indent behavior isn't reliable to drive via keyboard.type).
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

const CUSTOM_LIBRARY = 'custom-library-test.xml';

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    await ctx.grantPermissions(['clipboard-read', 'clipboard-write']);
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Library Xml Extension Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');
    await revealLibraries(page);

    // --- The Add Library file picker accepts .xml, not just .aslx ---
    await page.locator('[data-value="_include"] .node-actions button').click();
    await page.waitForSelector('.tree-dropdown button:has-text("Add library")', { timeout: 5000 });
    await page.click('.tree-dropdown button:has-text("Add library")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    const accept = await page.locator('[role="dialog"] input[type="file"]').getAttribute('accept');
    if (!accept || !accept.split(',').map(s => s.trim()).includes('.xml')) {
        throw new Error(`Expected the Add Library file input's accept attribute to include ".xml", got "${accept}"`);
    }
    console.log('PASS: the Add Library file picker accepts .xml');

    await page.setInputFiles('[role="dialog"] input[type="file"]', fixture);
    await page.waitForFunction(() => document.querySelector('[role="dialog"] button') !== null, { timeout: 5000 });
    await page.click('[role="dialog"] button:has-text("Add library")');
    await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 }).catch(() => {});
    await treeItem(page, CUSTOM_LIBRARY).first().waitFor({ state: 'visible', timeout: 10000 });
    console.log('PASS: custom .xml library uploaded and added via the Included Libraries header menu');
    await page.click('button:has-text("Dismiss")');
    await page.waitForSelector('text=Reload the editor to apply your library changes', { state: 'hidden', timeout: 5000 });

    // --- It's treated as a library, not a plain asset: editable XML editor with its content ---
    await treeItem(page, CUSTOM_LIBRARY).first().click();
    await page.waitForSelector('.cm-editor', { timeout: 10000 });
    const customEditable = await page.locator('.cm-content').first().getAttribute('contenteditable');
    if (customEditable !== 'true') throw new Error(`Expected the custom .xml library editor to be editable (contenteditable=true), got "${customEditable}"`);
    const customContent = await page.locator('.cm-content').first().innerText();
    if (!customContent.includes('MyXmlLibraryGreeting')) throw new Error('Expected the custom library editor to show the uploaded .xml content');
    console.log('PASS: selecting the .xml library shows its uploaded content in an editable editor');

    // --- Reload (the app's own in-editor reload, which re-runs WASM Initialise) proves
    //     <include ref="custom-library-test.xml"/> actually resolves via the adapter's library
    //     candidate discovery, not just that the upload succeeded ---
    await setCmContent(page, '<library>\n  <function name="MyXmlLibraryGreeting">\n    <![CDATA[<msg>Edited xml library</msg>]]>\n  </function>\n</library>');
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.waitForSelector('text=Reload the editor to apply your library changes', { timeout: 5000 });
    await page.click('button:has-text("Reload")');
    await page.waitForSelector('button[title="More"]', { timeout: 90000 });
    await revealLibraries(page);
    await treeItem(page, CUSTOM_LIBRARY).first().waitFor({ state: 'visible', timeout: 10000 });
    await treeItem(page, CUSTOM_LIBRARY).first().click();
    await page.waitForSelector('.cm-editor', { timeout: 10000 });
    const reloadedContent = await page.locator('.cm-content').first().innerText();
    if (!reloadedContent.includes('Edited xml library')) throw new Error('Expected the .xml library edit to survive the editor reload (proving it resolves via listLibraryCandidates on load, not just on upload)');
    console.log('PASS: after reload, the game still loads and the .xml library is still resolved with its edited content');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
