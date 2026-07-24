// Ad-hoc manual verification, Firefox only (the real non-FSA target browser —
// see verify-appshell-local-drafts.mjs for why WebKit can't stand in here in a
// headless sandbox): two scenarios for the OPFS local-drafts feature that aren't
// covered by the basic create/reload/asset-persistence check.
//
// 1. GameId rekey: regenerating the gameid field (PropertyEditor.svelte's
//    "Generate" button) and saving must move the OPFS draft to the new key, not
//    fork a second empty draft under it — asserted by the drafts list staying at
//    exactly one entry.
// 2. Backup/Import round trip: Backup bundles the game + assets into a .zip;
//    deleting the local draft and re-importing that same .zip must restore both
//    the game content and the asset.
//
// Requires the AppShell dev server running against a Release WasmEditor build.
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
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
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
    // No explicit Save button anymore (debounced autosave-on-edit replaced
    // it — see verify-appshell-autosave.mjs) — wait for the pill to cycle
    // through the same Saving…/Saved lifecycle instead of clicking one. The
    // debounce-to-save-start latency varies more than the ~300ms the pill
    // itself stays visible for (editor-store.ts's MIN_SAVING_VISIBLE_MS); a
    // tighter budget here measurably flaked (confirmed on main too).
    await page.waitForSelector('text=Saving…', { timeout: 8000 });
    await page.waitForSelector('text=Saved', { timeout: 10000 });

    const after = await draftCount();
    console.log('drafts after rekey + save (expect 1, not 2):', after);
    if (after !== 1) throw new Error(`Rekey forked the draft: expected 1, got ${after}`);
    console.log('rekey PASS');

    // --- Scenario 2: Backup / Import round trip ---
    await createDraft('Backup Test');
    await page.click('button[title="Manage assets"]');
    const dialog = page.locator('div[role="dialog"]');
    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        dialog.locator('button:has-text("Upload")').click(),
    ]);
    await fileChooser.setFiles(fileURLToPath(new URL('./fixtures/restart-test.js', import.meta.url)));
    await dialog.locator('text=restart-test.js').waitFor({ timeout: 10000 });
    await dialog.locator('button:has-text("Close")').click();

    // Backup… lives inside the File ▾ menu, not a standalone toolbar button
    // (see the responsive Toolbar rework, a50f713a) — open it first. The
    // BackupBanner's own "Backup…" button isn't a reliable alternative here:
    // it only appears after a couple of saves (ACTIVITY_THRESHOLD_SAVES in
    // local-adapter.ts), which this draft hasn't hit yet at this point.
    await page.click('button:has-text("File")');
    const [download] = await Promise.all([
        page.waitForEvent('download'),
        page.click('button:has-text("Backup…")'),
    ]);
    const zipPath = `/tmp/appshell-backup-test-${Date.now()}.zip`;
    await download.saveAs(zipPath);
    console.log('backup zip saved to', zipPath);

    // Delete the draft, then re-import the backed-up zip. Deletion is
    // confirmed through the app's own ConfirmDialog.svelte (a promise-based
    // in-page modal — see lib/confirm.ts), not a native window.confirm(), so
    // there's no browser "dialog" event to auto-accept; the confirm button
    // (also labelled "Delete", as the primary/danger action) must be clicked
    // directly, scoped to the dialog to avoid matching a different draft
    // row's own "Delete" button.
    await page.goto(`${baseUrl}/open`);
    await page.click('button:has-text("Delete")');
    await page.locator('div[role="dialog"] button:has-text("Delete")').click();
    await page.waitForTimeout(500);

    const [fileChooser2] = await Promise.all([
        page.waitForEvent('filechooser'),
        page.click('button:has-text("Import game file")'),
    ]);
    await fileChooser2.setFiles(zipPath);
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    await page.click('button[title="Manage assets"]');
    const assetRestored = await page.locator('div[role="dialog"]').locator('text=restart-test.js').isVisible();
    console.log('asset restored after backup/import round trip:', assetRestored);
    if (!assetRestored) throw new Error('Asset missing after re-importing backed-up zip');
    console.log('backup/import PASS');

    console.log('ALL PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-rekey-backup-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
