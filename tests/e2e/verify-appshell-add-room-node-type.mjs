// Verifies issue #2392: a room created with "Add Room" (and a page created with "Add Page in")
// gets its real node type in the tree straight away. The object's tree node used to be emitted
// before its editor_room type was added, so it stayed an "object" - generic icon, and no
// room-only "Add Room in" / "Add Exit from" options in the toolbar's Add menu.
// Requires the AppShell dev server running locally:
//   cd src/AppShell && npm run dev
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

// Same disambiguation issue as verify-appshell-exits-editor.mjs's selectTreeNode: a leaf's row is
// [data-part="item"], a branch's own row is [data-part="branch-control"].
async function selectTreeNode(name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

async function addViaMenu(buttonText, name) {
    await page.click('button[title="Add element"]');
    await page.click(`button:has-text("${buttonText}")`, { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

async function addMenuTexts() {
    await page.click('button[title="Add element"]');
    const items = page.locator('button:has-text("Add ")');
    await items.first().waitFor({ timeout: 5000 });
    const texts = (await items.allTextContents()).map(t => t.trim());
    await page.keyboard.press('Escape');
    return texts;
}

try {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Add Room Node Type Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: text adventure local draft created');

    await addViaMenu('Add Room', 'cellar');
    await selectTreeNode('cellar');
    const roomMenu = await addMenuTexts();
    for (const expected of ['Add Room in "cellar"', 'Add Exit from "cellar"']) {
        if (!roomMenu.includes(expected)) {
            throw new Error(`Add menu for a new room is missing "${expected}": [${roomMenu.join(' | ')}]`);
        }
    }
    console.log('PASS: a room added with "Add Room" offers the room-only Add options');

    await selectTreeNode('room');
    await addViaMenu('Add Page in', 'Test Page');
    await selectTreeNode('Test Page');
    const pageMenu = await addMenuTexts();
    if (pageMenu.some(t => t.startsWith('Add Exit from'))) {
        throw new Error(`Add menu for a new page offers room-only options: [${pageMenu.join(' | ')}]`);
    }
    console.log('PASS: a page added with "Add Page in" is not treated as a room');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
