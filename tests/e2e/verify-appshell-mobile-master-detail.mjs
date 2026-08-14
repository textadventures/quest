// Verifies the mobile master-detail pane switch: tree is the full-screen
// home on a phone viewport, tapping an element pushes a full-screen
// properties view with a back button, and desktop keeps the side-by-side +
// splitter layout untouched.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Master Detail Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

try {
    // Desktop: unaffected — both panes visible, splitter present.
    const desktopCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const desktopPage = await createDraftAndGetPage(desktopCtx);
    await desktopPage.waitForSelector('[role="separator"][aria-orientation="vertical"]', { timeout: 5000 });
    console.log('PASS: desktop still shows the splitter');
    const treeVisible = await desktopPage.locator('text=GAME OBJECTS').isVisible();
    const propsVisible = await desktopPage.locator('text=PROPERTIES').isVisible();
    if (!treeVisible || !propsVisible) throw new Error('Desktop should show both tree and properties panes');
    console.log('PASS: desktop shows tree and properties panes side by side');
    await desktopCtx.close();

    // Mobile: tree is home, no splitter, tapping a node pushes properties.
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const mobilePage = await createDraftAndGetPage(mobileCtx);
    const splitterCount = await mobilePage.locator('[role="separator"][aria-orientation="vertical"]').count();
    if (splitterCount !== 0) throw new Error('Mobile should not render the splitter');
    console.log('PASS: mobile hides the splitter');
    if (!(await mobilePage.locator('text=GAME OBJECTS').isVisible())) throw new Error('Mobile should start on the tree pane');
    if (await mobilePage.locator('text=PROPERTIES').isVisible()) throw new Error('Mobile should not show the properties pane yet');
    console.log('PASS: mobile starts on the tree pane, full width');
    await mobilePage.screenshot({ path: '/tmp/mobile-tree-pane.png' });

    // "game" is already selected by the load-time auto-select (programmatic —
    // must NOT trigger onactivate), so tap "room" to actually change selection.
    await mobilePage.click('text=room');
    await mobilePage.waitForSelector('button:has-text("room")', { timeout: 5000 }); // back header
    if (await mobilePage.locator('text=GAME OBJECTS').isVisible()) throw new Error('Tree pane should be hidden after activating a node');
    console.log('PASS: tapping a tree node pushes the properties pane with a back header showing its name');
    await mobilePage.screenshot({ path: '/tmp/mobile-props-pane.png' });

    // Back button returns to tree, with expansion state intact (game/room still expanded).
    await mobilePage.click('button:has-text("room")');
    await mobilePage.waitForSelector('text=GAME OBJECTS', { timeout: 5000 });
    const playerVisibleAfterBack = await mobilePage.locator('text=player').first().isVisible();
    if (!playerVisibleAfterBack) throw new Error('Tree expansion state should survive switching panes');
    console.log('PASS: back button returns to tree pane with expansion state intact');
    await mobileCtx.close();

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
