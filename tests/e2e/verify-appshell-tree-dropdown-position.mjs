// Regression test for user-reported feedback: on a mobile viewport (tree
// full-width, "..." button near the right edge), the node "..." dropdown
// always anchored its left edge to the button's left edge and extended
// rightward — which put it mostly off-screen on the right. Fixed by
// flipping to right-aligned against the button when there isn't room.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const page = await mobileCtx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Tree Dropdown Position Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.locator('[title="Options"]').first().click();
    const dropdown = page.locator('.tree-dropdown');
    await dropdown.waitFor({ state: 'visible', timeout: 5000 });
    const box = await dropdown.boundingBox();
    if (!box) throw new Error('No dropdown bounding box');
    if (box.x < 0 || box.x + box.width > 375) {
        throw new Error(`Dropdown (x=${box.x}, width=${box.width}) overflows the 375px viewport`);
    }
    console.log(`PASS: node "..." dropdown fits within the viewport (x=${box.x}, width=${box.width})`);
    await page.screenshot({ path: '/tmp/tree-dropdown-mobile.png' });

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
