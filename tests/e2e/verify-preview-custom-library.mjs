// Regression test: after the recent Included-Library editor fixes (SaveMode.Editor now excludes
// unmodified library-origin elements, leaving <include ref="..."/> for the engine to resolve at
// load time — see WorldModel.GetLibraryStream), clicking Preview on a game with a *custom*
// (uploaded, non-built-in) Included Library sent WasmPlayer just the game bytes with no way to
// resolve that <include>, so WasmPlayerBridge.Initialise failed with "Library file not found".
// Fixed by having editor-store.ts's previewInWasmPlayer resolve the library's bytes (same
// candidate discovery preloadAdjacentLibraryAssets uses for the editor's own reload) and hand
// them to WasmPlayer over the same BroadcastChannel handoff, which stages them into
// WasmPlayerBridge via a new AddAdjacentFile JSExport before Initialise runs.
//
// Requires both dev servers running (./dev.sh): AppShell on 5174 (proxies /player to WasmPlayer
// on 5175, so BroadcastChannel — same-origin only — actually works between the two tabs).
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const CUSTOM_LIBRARY_ASLX = `<library>
  <function name="CustomLibraryFunction"><![CDATA[
    msg ("CUSTOM_LIBRARY_LOADED_OK")
  ]]></function>
</library>`;

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 } });
    const page = await ctx.newPage();
    const pageErrors = [];
    const consoleErrors = [];
    page.on('pageerror', err => pageErrors.push(err.message));
    page.on('console', msg => { if (msg.type() === 'error') consoleErrors.push(msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Preview Custom Library Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 30000 });
    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });
    console.log('PASS: local draft created');

    // "Included Libraries" stays hidden from the tree until the game has its first one
    // (TreePanel's HIDE_WHEN_EMPTY) — "Advanced" is the documented entry point for adding the
    // very first element of any such category (see PropertyEditor's ADVANCED_ADDERS comment).
    await page.click('text=Advanced');
    await page.click('button:has-text("+ Add Library")');
    console.log('PASS: Add Included Library modal opened');

    await page.waitForSelector('text=Add Included Library', { timeout: 5000 });
    // Substring match, not exact - #2123 widened this to accept=".aslx,.xml".
    const fileInput = page.locator('input[type="file"][accept*=".aslx"]');
    await fileInput.setInputFiles({
        name: 'custom.aslx',
        mimeType: 'application/octet-stream',
        buffer: Buffer.from(CUSTOM_LIBRARY_ASLX, 'utf8'),
    });
    await page.waitForSelector('text=custom.aslx', { timeout: 5000 });
    await page.click('div[role="dialog"] button:has-text("Add Library"):not([disabled])');
    await page.waitForSelector('text=Add Included Library', { state: 'detached', timeout: 5000 });
    console.log('PASS: custom.aslx uploaded and Included Library element created');

    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });

    // Preview opens a new tab (window.open) — capture it.
    const [previewPage] = await Promise.all([
        ctx.waitForEvent('page'),
        page.click('button[title="Preview game"]'),
    ]);
    const previewErrors = [];
    const previewConsoleErrors = [];
    previewPage.on('pageerror', err => previewErrors.push(err.message));
    previewPage.on('console', msg => { if (msg.type() === 'error') previewConsoleErrors.push(msg.text()); });

    await previewPage.waitForLoadState('domcontentloaded');

    // Wait for either the game's command box to show up (success) or a reasonable timeout.
    let booted = false;
    try {
        await previewPage.waitForSelector('#txtCommandDiv', { state: 'visible', timeout: 15000 });
        booted = true;
    } catch {
        booted = false;
    }

    const allErrors = [...previewErrors, ...previewConsoleErrors];
    const libraryError = allErrors.find(e => e.includes('Library file not found') || e.includes('Failed to initialise game'));

    if (libraryError) {
        throw new Error(`Preview failed to load the game with a custom Included Library: ${libraryError}`);
    }
    if (!booted) {
        throw new Error(`Preview never reached a booted game UI (#txtCommandDiv). Errors seen: ${JSON.stringify(allErrors)}`);
    }
    console.log('PASS: Preview booted the game with a custom Included Library, no "Library file not found" error');

    await previewPage.close();
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
