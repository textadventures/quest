import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `AssetPicker Fix Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

try {
    // --- Mobile: PropertyEditor's "Cover art" file field must not overflow the viewport ---
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 812 }, hasTouch: true });
    const mobilePage = await createDraftAndGetPage(mobileCtx);

    // "game" is auto-selected at load without pushing the mobile detail pane; select "room" first
    // then back to "game" so tapping it actually triggers navigation.
    await mobilePage.click('text=room');
    await mobilePage.waitForSelector('button:has-text("room")', { timeout: 5000 });
    await mobilePage.click('button:has-text("room")'); // back to tree
    await mobilePage.waitForSelector('text=GAME OBJECTS', { timeout: 5000 });
    // Exact match — "text=game" (substring) also matches the "GAME OBJECTS" heading.
    await mobilePage.click('text="game"');
    await mobilePage.waitForSelector('button:has-text("Setup")', { timeout: 5000 });

    const scrollWidth = await mobilePage.evaluate(() => document.documentElement.scrollWidth);
    const clientWidth = await mobilePage.evaluate(() => document.documentElement.clientWidth);
    if (scrollWidth > clientWidth) {
        throw new Error(`Page overflows horizontally before opening Cover art: scrollWidth=${scrollWidth} clientWidth=${clientWidth}`);
    }

    const coverArtLabel = await mobilePage.locator('text=Cover art:').elementHandle();
    await coverArtLabel.scrollIntoViewIfNeeded();
    const uploadBtn = mobilePage.locator('button:has-text("Upload…")').first();
    await uploadBtn.waitFor({ state: 'visible', timeout: 5000 });
    const box = await uploadBtn.boundingBox();
    if (!box) throw new Error('Upload button not found');
    if (box.x + box.width > 375) {
        throw new Error(`Upload button overflows viewport: right edge at ${box.x + box.width}px (viewport is 375px)`);
    }
    console.log('PASS: Cover art Upload button fits within 375px mobile viewport (right edge at ' + (box.x + box.width).toFixed(1) + 'px)');

    const scrollWidthAfter = await mobilePage.evaluate(() => document.documentElement.scrollWidth);
    if (scrollWidthAfter > 375) {
        throw new Error(`Page overflows horizontally with Cover art visible: scrollWidth=${scrollWidthAfter}`);
    }
    console.log('PASS: no horizontal overflow on mobile with the file-upload control visible');

    await mobileCtx.close();

    // --- Desktop: ScriptEditor's inline AssetPicker (Show picture) must stay compact, not stretch full-width ---
    const desktopCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const desktopPage = await createDraftAndGetPage(desktopCtx);
    await desktopPage.click('button:has-text("Scripts")').catch(() => {});
    // "game" node's own tabs: Setup/Features/.../Scripts - click via text since node already selected on desktop by default.
    await desktopPage.waitForSelector('button:has-text("Scripts")', { timeout: 5000 });
    await desktopPage.click('button:has-text("Scripts")');
    await desktopPage.click('button:has-text("+ Add script")');
    await desktopPage.waitForSelector('text=Add Script Command', { timeout: 5000 });
    await desktopPage.click('text=Show a picture');
    await desktopPage.click('button:has-text("OK")');
    await desktopPage.waitForSelector('text=Show picture', { timeout: 5000 });

    const assetPickerRow = desktopPage.locator('button:has-text("Upload…")').first();
    await assetPickerRow.waitFor({ state: 'visible', timeout: 5000 });
    const rowBox = await assetPickerRow.boundingBox();
    console.log(`INFO: ScriptEditor Upload button box: x=${rowBox.x.toFixed(0)} width=${rowBox.width.toFixed(0)}`);
    // A regression would force this control (and thus the button) to the full width of the
    // script row (>800px); the compact/inline width should be well under that.
    if (rowBox.width > 400) {
        throw new Error(`ScriptEditor's AssetPicker appears stretched full-width (button x=${rowBox.x}), inline layout regressed`);
    }
    console.log('PASS: ScriptEditor inline "Show picture" control stays compact, no full-width regression');

    await desktopCtx.close();

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
