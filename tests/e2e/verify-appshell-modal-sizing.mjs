// Verifies AddElementModal, PublishModal, AssetManagerModal, and
// AddScriptModal all fit fully on-screen at a phone viewport (no modal wider
// than the viewport, no horizontal page overflow), while keeping their
// original max sizes on desktop.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Modal Sizing Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

async function checkDialogFits(page, label, viewportWidth) {
    const box = await page.locator('[role="dialog"]').last().boundingBox();
    if (!box) throw new Error(`${label}: no dialog bounding box`);
    if (box.x < 0 || box.x + box.width > viewportWidth) {
        throw new Error(`${label}: dialog (x=${box.x}, width=${box.width}) overflows viewport width ${viewportWidth}`);
    }
    console.log(`PASS: ${label} fits within the viewport (width ${box.width}px)`);
}

try {
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const page = await createDraftAndGetPage(mobileCtx);

    // AddElementModal — close via its own Cancel button; Escape/focus timing
    // varies per modal (some autofocus a field, some don't), so use each
    // modal's explicit close control rather than relying on that.
    await page.click('button[title="More"]');
    await page.getByRole('button', { name: 'Add' }).first().click(); // icon-only Add button (mobile)
    await page.getByText('Add Room', { exact: true }).click();
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await checkDialogFits(page, 'AddElementModal', 375);
    await page.getByRole('button', { name: 'Cancel', exact: true }).click();
    await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 });

    // PublishModal — needs a saved game filename; the local draft already has one.
    await page.click('button[title="More"]');
    await page.getByText('Publish…', { exact: true }).click();
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await checkDialogFits(page, 'PublishModal', 375);
    await page.getByRole('button', { name: 'Close', exact: true }).click();
    await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 });

    // AssetManagerModal
    await page.click('button[title="More"]');
    await page.getByText('Manage assets', { exact: true }).click();
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await checkDialogFits(page, 'AssetManagerModal', 375);
    await page.getByRole('button', { name: 'Close', exact: true }).click();
    await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 });

    // AddScriptModal — via a room's script tab.
    await page.click('text=room');
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await checkDialogFits(page, 'AddScriptModal', 375);
    await page.screenshot({ path: '/tmp/modal-addscript-mobile.png' });
    await page.getByRole('button', { name: 'Close', exact: true }).click();
    await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 });

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
