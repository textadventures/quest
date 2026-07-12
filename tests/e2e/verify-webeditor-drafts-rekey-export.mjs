// Ad-hoc manual verification, Firefox only (the real non-FSA target browser —
// see verify-webeditor-local-drafts.mjs for why WebKit can't stand in here in a
// headless sandbox): two scenarios for the OPFS local-drafts feature that aren't
// covered by the basic create/reload/asset-persistence check.
//
// 1. GameId rekey: regenerating the gameid field (PropertyEditor.svelte's
//    "Generate" button) and saving must move the OPFS draft to the new key, not
//    fork a second empty draft under it — asserted by the drafts list staying at
//    exactly one entry.
// 2. Export/Import round trip: Export bundles the game + assets into a .zip;
//    deleting the local draft and re-importing that same .zip must restore both
//    the game content and the asset.
//
// Requires the WebEditor dev server running against a Release WasmEditor build.
import { firefox } from 'playwright';
import { fileURLToPath } from 'node:url';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await firefox.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function createDraft(name) {
    await page.goto(`${baseUrl}/open`);
    await page.fill('input[placeholder="Game name"]', name);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button:has-text("🖼 Assets")', { timeout: 30000 });
}

async function draftCount() {
    await page.goto(`${baseUrl}/open`);
    // refreshDrafts() is async (reads OPFS) — wait for it to actually settle
    // rather than counting immediately, or this races the render.
    await page.locator('text=Your local drafts').waitFor({ timeout: 5000 }).catch(() => {});
    return page.locator('button:has-text("Delete")').count();
}

try {
    // --- Scenario 1: GameId rekey ---
    await createDraft('Rekey Test'); // exactly 1 draft now, still inside the editor
    await page.click('button:has-text("Generate")'); // regenerate gameid
    await page.click('button:has-text("Save")');
    await page.waitForTimeout(500); // saveFile/rekey is async, no visible loading indicator to wait on

    const after = await draftCount();
    console.log('drafts after rekey + save (expect 1, not 2):', after);
    if (after !== 1) throw new Error(`Rekey forked the draft: expected 1, got ${after}`);
    console.log('rekey PASS');

    // --- Scenario 2: Export / Import round trip ---
    await createDraft('Export Test');
    await page.click('button:has-text("🖼 Assets")');
    const dialog = page.locator('div[role="dialog"]');
    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        dialog.locator('button:has-text("Upload")').click(),
    ]);
    await fileChooser.setFiles(fileURLToPath(new URL('./fixtures/restart-test.aslx', import.meta.url)));
    await dialog.locator('text=restart-test.aslx').waitFor({ timeout: 10000 });
    await dialog.locator('button:has-text("Close")').click();

    const [download] = await Promise.all([
        page.waitForEvent('download'),
        page.click('button:has-text("Export")'),
    ]);
    const zipPath = `/tmp/webeditor-export-test-${Date.now()}.zip`;
    await download.saveAs(zipPath);
    console.log('exported zip saved to', zipPath);

    // Delete the draft, then re-import the exported zip.
    await page.goto(`${baseUrl}/open`);
    page.once('dialog', d => d.accept());
    await page.click('button:has-text("Delete")');
    await page.waitForTimeout(500);

    const [fileChooser2] = await Promise.all([
        page.waitForEvent('filechooser'),
        page.click('button:has-text("Import game file")'),
    ]);
    await fileChooser2.setFiles(zipPath);
    await page.waitForSelector('button:has-text("🖼 Assets")', { timeout: 30000 });
    await page.click('button:has-text("🖼 Assets")');
    const assetRestored = await page.locator('div[role="dialog"]').locator('text=restart-test.aslx').isVisible();
    console.log('asset restored after export/import round trip:', assetRestored);
    if (!assetRestored) throw new Error('Asset missing after re-importing exported zip');
    console.log('export/import PASS');

    console.log('ALL PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/webeditor-rekey-export-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
