// Ad-hoc manual verification for the editor side of Text Adventure "Pages"
// (dialogue trees) — see issue #2040. Checks that "Add Page" is always available
// (no feature toggle) in a text adventure, page creation via the toolbar, the
// dialoguepage Page tab (page type dropdown + Options control) replacing the
// standard object tabs, the options control's page-only source dropdown with
// inline "+ New page" creation, and the single "Show a page" script adder
// under a "Pages" category.
// Requires the AppShell dev server running locally:
//   cd src/AppShell && npm run dev
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Dialogue Pages Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
    console.log('PASS: text adventure local draft created');

    const tree = page.locator('.overflow-y-auto.p-1.text-xs');

    // "Add Page" is available immediately - no feature toggle to enable first.
    await page.click('button[title="Add element"]');
    await page.getByRole('button', { name: 'Add Page', exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "Add Page" available without any feature toggle');
    await page.getByRole('button', { name: 'Add Page', exact: true }).click();
    await page.fill('#element-name', 'guard_intro');
    await page.getByRole('button', { name: 'Add Page', exact: true }).click();
    await tree.getByText('guard_intro', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: page created via toolbar');

    // The new page shows the Page tab (page type dropdown + Options control) and
    // not the standard object tabs.
    await tree.getByText('guard_intro', { exact: true }).click();
    await page.getByRole('button', { name: 'Page', exact: true }).waitFor({ state: 'visible', timeout: 10000 });
    await page.getByRole('button', { name: 'Page', exact: true }).click();
    await page.locator('text=Page type').waitFor({ timeout: 10000 });
    await page.locator('text=Options').first().waitFor({ timeout: 5000 });
    console.log('PASS: Page tab with page type dropdown and Options control');
    for (const label of ['Setup', 'Inventory', 'Features']) {
        const visible = await page.getByRole('button', { name: label, exact: true }).isVisible().catch(() => false);
        if (visible) throw new Error(`Standard object tab "${label}" should be hidden for a dialogue page`);
    }
    console.log('PASS: standard object tabs hidden for the page');

    // Inline "+ New page" creation from the options control, which should also
    // link to the new page.
    await page.click('button:has-text("+ New page")');
    await page.fill('#element-name', 'guard_weather');
    await page.getByRole('button', { name: 'Add Page', exact: true }).click();
    await page.locator('span[title="guard_weather"]').waitFor({ timeout: 5000 });
    console.log('PASS: inline "+ New page" created and linked guard_weather');

    // The inline-created page is a real dialoguepage: it must appear in the
    // sibling page's key dropdown (page-typed source), and creating it must not
    // have moved the selection away from guard_intro.
    await tree.getByText('guard_weather', { exact: true }).click();
    await page.getByRole('button', { name: 'Page', exact: true }).waitFor({ state: 'visible', timeout: 10000 });
    await page.getByRole('button', { name: 'Page', exact: true }).click();
    const keyDropdownOptions = await page.locator('select.select option').allTextContents();
    if (!keyDropdownOptions.includes('guard_intro')) {
        throw new Error(`Expected guard_intro in the options key dropdown, got: ${keyDropdownOptions.join(', ')}`);
    }
    console.log('PASS: options key dropdown lists sibling pages (type-filtered source)');

    // Script adder: a single "Show a page" entry under the "Pages" category (no
    // separate confusing "Go to page" entry).
    await tree.getByText('game', { exact: true }).first().click();
    await page.click('button:has-text("Script")');
    await page.waitForSelector('button:has-text("Add script")', { timeout: 10000 });
    await page.click('button:has-text("Add script")');
    const dialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await dialog.waitFor({ timeout: 10000 });
    await dialog.locator('button:has-text("Pages")').first().waitFor({ timeout: 5000 });
    await dialog.locator('button:has-text("Pages")').first().click();
    await dialog.locator('text=Show a page').waitFor({ timeout: 5000 });
    const goToPageCount = await dialog.locator('text=Go to page').count();
    if (goToPageCount > 0) {
        throw new Error('Expected no separate "Go to page" adder entry (merged into "Show a page")');
    }
    console.log('PASS: single "Show a page" adder under the Pages category, no separate "Go to page"');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-dialogue-pages-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
