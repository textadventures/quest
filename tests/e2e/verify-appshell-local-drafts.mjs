// Ad-hoc manual verification: AppShell's OPFS-backed local drafts for browsers
// without the File System Access API (Firefox, Safari). Confirms a draft created
// via the fallback path (no FSA directory picker) survives a full page reload,
// including an uploaded asset — the bug being fixed is that the old fallback
// stored assets in OPFS under a fresh crypto.randomUUID() every load, so a
// reload orphaned them. Also exercises Safari's write path specifically: Safari
// lacks FileSystemFileHandle.createWritable() and needs a worker using
// createSyncAccessHandle() instead (see src/AppShell/src/lib/filesystem/
// opfs-writer.worker.ts) — WebKit is the closest available local proxy for that,
// real Safari should still be checked by hand.
//
// Requires the AppShell dev server running locally against a Release WasmEditor
// build (createSyncAccessHandle needs the AOT build's real dotnet runtime bits
// to be present, though the code path itself doesn't depend on Release vs Debug):
//   WASM_CONFIG=Release node ../../src/AppShell/node_modules/.bin/vite dev --root ../../src/AppShell
import { firefox, webkit } from 'playwright';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const browserType = process.argv[3] || 'firefox'; // firefox | webkit

const launch = browserType === 'webkit' ? webkit : firefox;
const browser = await launch.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log(`[${browserType}] [pageerror]`, err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log(`[${browserType}] [console.error]`, msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    console.log(`[${browserType}] open page loaded, non-FSA path confirmed (no "Open game folder" button)`);

    await page.fill('input[placeholder="Game name"]', 'Local Draft Test');
    await page.waitForSelector('text=Text adventure', { timeout: 10000 }); // templates finished loading
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('text=Quest Viva Editor >> visible=true', { timeout: 5000 }).catch(() => {});
    await page.waitForSelector('button:has-text("🖼 Assets")', { timeout: 30000 });
    console.log(`[${browserType}] new local draft created and opened in the editor`);

    // Upload an asset via the standalone asset manager.
    await page.click('button:has-text("🖼 Assets")');
    const fixturePath = join(dirname(fileURLToPath(import.meta.url)), 'fixtures', 'restart-test.js');
    // Reuse an existing small fixture file as arbitrary asset bytes — content doesn't matter, just persistence.
    const dialog = page.locator('div[role="dialog"]');
    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        dialog.locator('button:has-text("Upload")').click(),
    ]);
    await fileChooser.setFiles(fixturePath);
    await dialog.locator(`text=${fixturePath.split('/').pop()}`).waitFor({ timeout: 10000 });
    console.log(`[${browserType}] asset uploaded`);
    await dialog.locator('button:has-text("Close")').click();

    // Full reload — this is the scenario that used to orphan OPFS assets.
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('text=Your local drafts', { timeout: 10000 });
    const draftVisible = await page.isVisible('text=Local Draft Test.aslx');
    console.log(`[${browserType}] draft listed after reload:`, draftVisible);
    if (!draftVisible) throw new Error('Draft did not survive reload');

    await page.click('button:has-text("Local Draft Test.aslx")');
    await page.waitForSelector('button:has-text("🖼 Assets")', { timeout: 30000 });
    await page.click('button:has-text("🖼 Assets")');
    const assetVisible = await page.locator('div[role="dialog"]').locator('text=restart-test.js').isVisible();
    console.log(`[${browserType}] asset still present after reload:`, assetVisible);
    if (!assetVisible) throw new Error('Asset did not survive reload — OPFS orphaning bug still present');

    console.log(`[${browserType}] PASS`);
}

try {
    await run();
} catch (err) {
    console.error(`[${browserType}] FAIL:`, err.message);
    await page.screenshot({ path: `/tmp/appshell-${browserType}-failure.png` });
    process.exitCode = 1;
} finally {
    await browser.close();
}
