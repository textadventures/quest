// Regression test for user-reported feedback: in the mobile master-detail
// view, after using the back button to return to the tree, tapping the
// already-selected node did nothing — TreeView's onSelectionChange only
// fires when the selected value actually changes, so re-tapping the same
// node never pushed back into the properties pane. You had to tap a
// *different* node first (which worked) to get anywhere.
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
    await page.fill('input[placeholder="Game name"]', `Tap Same Node Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // "room" is not selected yet (game load auto-selects "game") — first tap
    // changes selection, which already worked before this fix.
    await page.click('text=room');
    await page.waitForSelector('button:has-text("room")', { timeout: 5000 }); // back header
    console.log('PASS: first tap on a different node pushes into the properties pane');

    // Back to the tree — "room" stays selected/highlighted.
    await page.click('button:has-text("room")');
    await page.waitForSelector('text=GAME OBJECTS', { timeout: 5000 });
    console.log('PASS: back button returns to the tree pane');

    // Tap the SAME node again (still selected) — this is the reported bug:
    // previously nothing happened because no selection-change event fires.
    await page.click('text=room');
    await page.waitForSelector('button:has-text("room")', { timeout: 5000 });
    console.log('PASS: tapping the already-selected node pushes into the properties pane');

    // Same check on a leaf node (no children, different TreeView.Item path).
    await page.click('button:has-text("room")'); // back to tree
    await page.waitForSelector('text=GAME OBJECTS', { timeout: 5000 });
    await page.click('text=player');
    await page.waitForSelector('button:has-text("player")', { timeout: 5000 });
    await page.click('button:has-text("player")'); // back to tree
    await page.waitForSelector('text=GAME OBJECTS', { timeout: 5000 });
    await page.click('text=player');
    await page.waitForSelector('button:has-text("player")', { timeout: 5000 });
    console.log('PASS: same fix applies to a leaf node (TreeView.Item, not TreeView.BranchControl)');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
