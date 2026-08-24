// Manual verification for three TreePanel default-state tweaks:
//   1. game node starts collapsed
//   2. starting room is expanded all the way down to the player object
//   3. Move up/down (swapElements) is undoable/redoable
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5180';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

const tree = page.locator('div.border-r.border-surface-200-800.bg-surface-50-950').first();
const branch = (name) => tree.locator('[data-part="branch-control"]', { hasText: name });

async function addViaToolbar(menuLabel, name) {
    await page.click('button[title="Add element"]');
    await page.locator('button', { hasText: menuLabel }).click();
    await page.fill('#element-name', name);
    await page.keyboard.press('Enter');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

async function rowOrder() {
    const texts = await tree.evaluateAll(
        els => els.map(e => e.textContent.replace('⋯', '').trim()),
        '[data-part="branch-control"], [data-part="item"]'
    );
    return texts;
}

async function openNodeMenu(name) {
    const row = tree.locator('[data-part="branch-control"], [data-part="item"]', { hasText: name }).last();
    await row.hover();
    const opts = row.locator('button[title="Options"]');
    await opts.waitFor({ state: 'visible', timeout: 5000 });
    await opts.click();
    await page.waitForSelector('.tree-dropdown', { timeout: 5000 });
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Default Tree State ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    await page.waitForTimeout(600);
    console.log('PASS: local draft created and opened in the editor');

    // 1. game node collapsed by default → Verbs/Commands hidden
    const gameRow = branch('game');
    await gameRow.waitFor({ timeout: 10000 });
    await gameRow.locator('button[aria-label="Expand"]').waitFor({ timeout: 5000 });
    const treeText = await tree.innerText();
    if (treeText.includes('Verbs') || treeText.includes('Commands')) {
        throw new Error('Expected "Verbs"/"Commands" (under game) to be hidden when game starts collapsed');
    }
    console.log('PASS: game node starts collapsed (Verbs/Commands hidden)');

    // 2. starting room expanded all the way down to player
    const roomRow = branch('room');
    await roomRow.waitFor({ timeout: 10000 });
    await roomRow.locator('button[aria-label="Collapse"]').waitFor({ timeout: 5000 });
    const playerRow = tree.locator('[data-part="item"]', { hasText: 'player' });
    await playerRow.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: starting room is expanded down to the player object');

    // 3. Move down is undoable
    await addViaToolbar('Add Room', 'Kitchen');
    await addViaToolbar('Add Room', 'Garden');
    let order = await rowOrder();
    const kIdx = order.findIndex(t => t.includes('Kitchen'));
    const gIdx = order.findIndex(t => t.includes('Garden'));
    if (kIdx < 0 || gIdx < 0 || kIdx > gIdx) throw new Error('Expected Kitchen before Garden before the move');

    await openNodeMenu('Kitchen');
    await page.locator('.tree-dropdown button', { hasText: 'Move down' }).click();
    await page.waitForTimeout(400);
    order = await rowOrder();
    if (order.findIndex(t => t.includes('Kitchen')) < order.findIndex(t => t.includes('Garden'))) {
        throw new Error('Expected Kitchen to have moved after Garden');
    }
    console.log('PASS: Move down reordered Kitchen after Garden');

    const undoBtn = page.locator('button[title="Undo"]');
    await undoBtn.waitFor({ state: 'visible', timeout: 5000 });
    await undoBtn.click();
    await page.waitForTimeout(400);
    order = await rowOrder();
    if (order.findIndex(t => t.includes('Kitchen')) > order.findIndex(t => t.includes('Garden'))) {
        throw new Error('Expected undo to restore Kitchen before Garden');
    }
    console.log('PASS: Undo restored the original order');

    const redoBtn = page.locator('button[title="Redo"]');
    await redoBtn.click();
    await page.waitForTimeout(400);
    order = await rowOrder();
    if (order.findIndex(t => t.includes('Kitchen')) < order.findIndex(t => t.includes('Garden'))) {
        throw new Error('Expected redo to re-apply the move (Kitchen after Garden)');
    }
    console.log('PASS: Redo re-applied the move');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-default-tree-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
