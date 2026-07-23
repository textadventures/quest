// Ad-hoc manual verification for the new draggable splitter between TreePanel
// (element tree, left) and PropertyEditor (right) on the /edit page — the
// tree pane used to be a fixed w-60 (240px) with no way to widen it to see
// long room/object names.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Splitter Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    const separator = page.locator('[role="separator"][aria-orientation="vertical"]');
    await separator.waitFor({ timeout: 10000 });

    const treePanel = page.locator('div.border-r.border-surface-200-800.bg-surface-50-950').first();
    const startBox = await treePanel.boundingBox();
    if (!startBox) throw new Error('Could not find TreePanel bounding box');
    if (Math.abs(startBox.width - 240) > 2) throw new Error(`Expected default tree width ~240px, got ${startBox.width}`);
    console.log(`PASS: TreePanel starts at default width (${startBox.width}px)`);
    await page.screenshot({ path: '/tmp/appshell-splitter-before.png' });

    // Drag the splitter to the right by 150px.
    const sepBox = await separator.boundingBox();
    await page.mouse.move(sepBox.x + sepBox.width / 2, sepBox.y + sepBox.height / 2);
    await page.mouse.down();
    await page.mouse.move(sepBox.x + sepBox.width / 2 + 150, sepBox.y + sepBox.height / 2, { steps: 10 });
    await page.screenshot({ path: '/tmp/appshell-splitter-during.png' });
    await page.mouse.up();

    const widerBox = await treePanel.boundingBox();
    if (widerBox.width < startBox.width + 100) throw new Error(`Expected tree panel to widen by ~150px, got ${widerBox.width - startBox.width}px delta`);
    console.log(`PASS: dragging right widened TreePanel to ${widerBox.width}px`);
    await page.screenshot({ path: '/tmp/appshell-splitter-after.png' });

    // Drag far left, past the min clamp (180px).
    const sepBox2 = await separator.boundingBox();
    await page.mouse.move(sepBox2.x + sepBox2.width / 2, sepBox2.y + sepBox2.height / 2);
    await page.mouse.down();
    await page.mouse.move(sepBox2.x + sepBox2.width / 2 - 400, sepBox2.y + sepBox2.height / 2, { steps: 10 });
    await page.mouse.up();
    const minBox = await treePanel.boundingBox();
    if (minBox.width < 178 || minBox.width > 182) throw new Error(`Expected width to clamp at ~180px, got ${minBox.width}`);
    console.log(`PASS: dragging far left clamps at min width (${minBox.width}px)`);

    // Drag far right, past the max clamp (600px).
    const sepBox3 = await separator.boundingBox();
    await page.mouse.move(sepBox3.x + sepBox3.width / 2, sepBox3.y + sepBox3.height / 2);
    await page.mouse.down();
    await page.mouse.move(sepBox3.x + sepBox3.width / 2 + 1000, sepBox3.y + sepBox3.height / 2, { steps: 10 });
    await page.mouse.up();
    const maxBox = await treePanel.boundingBox();
    if (maxBox.width < 598 || maxBox.width > 602) throw new Error(`Expected width to clamp at ~600px, got ${maxBox.width}`);
    console.log(`PASS: dragging far right clamps at max width (${maxBox.width}px)`);

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-splitter-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
