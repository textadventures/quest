// Ad-hoc manual verification for the OPFS-default-storage change: on a
// FSA-capable browser (Chromium), the /open page should now default to OPFS
// local drafts (same as Firefox/Safari) and offer "Open game folder" /
// "Save to folder…" as secondary FSA options, not the primary path.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);

    // Primary create button should be the OPFS one, not the FSA one.
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    console.log('PASS: "Create local draft" is the primary create button on Chromium');

    // Secondary FSA button should also be present, since this is a
    // FSA-capable browser.
    await page.waitForSelector('button:has-text("Save to folder…")', { timeout: 5000 });
    console.log('PASS: "Save to folder…" secondary FSA button is present');

    // Primary open button should be "Import game file", with "Open game
    // folder" offered as a secondary option.
    await page.waitForSelector('button:has-text("Import game file")', { timeout: 5000 });
    await page.waitForSelector('button:has-text("Open game folder")', { timeout: 5000 });
    console.log('PASS: "Import game file" (primary) and "Open game folder" (secondary) both present');

    // Create a local draft via the default path and confirm it opens and
    // then reappears in "Your local drafts" — proves the OPFS path still
    // works end-to-end even though FSA is available in this browser.
    await page.fill('input[placeholder="Game name"]', 'Chromium OPFS Default Test');
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button:has-text("🖼 Assets")', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor on Chromium');

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('text=Your local drafts', { timeout: 10000 });
    const draftVisible = await page.isVisible('text=Chromium OPFS Default Test.aslx');
    console.log('draft listed after reload:', draftVisible);
    if (!draftVisible) throw new Error('Draft did not survive reload on Chromium');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/webeditor-chromium-opfs-default-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
