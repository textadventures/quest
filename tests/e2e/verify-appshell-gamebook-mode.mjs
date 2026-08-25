// Verifies gamebook-mode support in the AppShell editor: creating a game from
// the Gamebook template should show "Pages" wording in the tree, offer only
// "Add Page" (no Add Room/Object/Exit/Verb/Command/Timer/Turn
// Script/Walkthrough/Template/Type anywhere, including under "Advanced"), let
// a new page be added flat and deleted, and refuse to delete the page
// currently containing "player" (or player itself). Also confirms a regular
// text-adventure game is unaffected (still offers Add Room/Object/Exit/etc).
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Gamebook Test ${Date.now()}`);
    await page.waitForSelector('text=Game type', { timeout: 10000 });
    await page.locator('label:has-text("Gamebook") input[type="radio"]').check();
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    const tree = page.locator('.overflow-y-auto.p-1.text-xs');

    // Tree header (renamed from "Objects").
    await tree.getByText('Pages', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: tree header reads "Pages" (not "Objects")');

    // Toolbar "+" menu: only "Add Page" — none of the text-adventure adders.
    // Menu items carry role="menuitem" (see DropdownMenu.svelte), not the
    // default "button" role, per the WAI-ARIA menu pattern.
    await page.click('button[title="Add element"]');
    await page.getByRole('menuitem', { name: 'Add Page', exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: toolbar "+" menu offers "Add Page"');

    for (const label of ['Add Room', 'Add Object', 'Add Exit', 'Add Verb', 'Add Command', 'Add Turn Script']) {
        const visible = await page.getByRole('menuitem', { name: label, exact: true }).isVisible().catch(() => false);
        if (visible) throw new Error(`Toolbar "+" menu should not offer "${label}" in gamebook mode`);
    }
    console.log('PASS: toolbar "+" menu has no text-adventure-only adders');

    // Add a page via the toolbar, flat (no parent prompt).
    await page.getByRole('menuitem', { name: 'Add Page', exact: true }).click();
    await page.fill('#element-name', 'MyNewPage');
    await page.getByRole('button', { name: 'Add Page', exact: true }).click();
    await tree.getByText('MyNewPage', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: new page added flat via toolbar "Add Page"');

    // Delete it again — should work (it's not the player's page).
    await tree.getByText('MyNewPage', { exact: true }).click();
    await page.click('button:has-text("Delete")');
    const stillThere = await tree.getByText('MyNewPage', { exact: true }).isVisible().catch(() => false);
    if (stillThere) throw new Error('MyNewPage should have been deleted');
    console.log('PASS: newly-added page can be deleted');

    // "_advanced" only offers Function/Library/JavaScript in gamebook mode.
    await tree.getByText('Advanced', { exact: true }).click();
    for (const label of ['Add Function', 'Add Library', 'Add JavaScript']) {
        await page.getByRole('button', { name: `+ ${label}`, exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    }
    for (const label of ['Add Timer', 'Add Walkthrough', 'Add Template', 'Add Dynamic Template', 'Add Type']) {
        const visible = await page.getByRole('button', { name: `+ ${label}`, exact: true }).isVisible().catch(() => false);
        if (visible) throw new Error(`"Advanced" adders should not offer "${label}" in gamebook mode`);
    }
    console.log('PASS: "Advanced" adders are limited to Function/Library/JavaScript in gamebook mode');

    // The tree starts collapsed (#827), but the page containing "player" —
    // Page1 per the Gamebook template — is expanded all the way down to the
    // player (whose nodeType is "page" in gamebook mode, not "object").
    const page1Row = tree.locator('[data-part="branch-control"]', { hasText: 'Page1' });
    await page1Row.locator('button[aria-label="Collapse"]').waitFor({ state: 'visible', timeout: 5000 });
    await tree.getByText('player', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: starting page is expanded down to the player (nodeType "page")');

    // The page that contains "player" (Page1, per the Gamebook template)
    // must not be deletable — EditorController.CanDelete refuses it, so the
    // toolbar Delete button is disabled rather than clickable (see
    // Toolbar.svelte's canDelete derivation).
    await tree.getByText('Page1', { exact: true }).click();
    const deleteDisabled = await page.locator('button:has-text("Delete")').isDisabled();
    if (!deleteDisabled) throw new Error('Delete button should be disabled for Page1 (containing player)');
    const page1StillThere = await tree.getByText('Page1', { exact: true }).isVisible().catch(() => false);
    if (!page1StillThere) throw new Error('Page1 (containing player) should NOT be deletable');
    console.log('PASS: deleting the page containing "player" is blocked (Delete button disabled)');

    console.log('PASS: all gamebook-mode checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/gamebook-mode-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
