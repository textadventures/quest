// Ad-hoc manual verification for the editor side of Text Adventure "Pages"
// (dialogue trees) — see issue #2040. Checks that "Add Page" is always available
// (no feature toggle) in a text adventure, page creation via the toolbar, the
// dialoguepage Page tab (page type dropdown + Options control) replacing the
// standard object tabs, the options control's page-only source dropdown with
// inline "+ New page" creation, and the single "Show a page" script adder
// under a "Pages" category.
//
// Also covers a follow-up review round: inline "+ New page" creation from the
// options list must use the CURRENT page's parent (not always top-level), the
// rich-text "Insert" toolbar's pinned Page Link picker must be filtered to
// pages only (not every object), the "Show page" script adder's page dropdown
// must likewise be pages-only (not objects/rooms), and its two checkboxes
// ("Allow the player to leave the dialogue" / "Run turn scripts") must render
// on separate lines (<breakbefore/>, wired up end-to-end for this fix — it was
// previously a dead XML tag with no effect anywhere in the WASM editor).
// Requires the AppShell dev server running locally:
//   cd src/AppShell && npm run dev
import { chromium } from './lib/tracked-chromium.mjs';

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
    // The pinned rich-text "Page Link" button also has an accessible name of
    // "Page", so property-tab buttons need to be scoped to the tab bar itself
    // to disambiguate from it.
    const tabBar = page.locator('.flex.border-b.border-surface-200-800.overflow-x-auto.flex-shrink-0');

    // "Add Page" is available immediately - no feature toggle to enable first.
    // Menu items carry role="menuitem" (see DropdownMenu.svelte), not the
    // default "button" role, per the WAI-ARIA menu pattern.
    await page.click('button[title="Add element"]');
    await page.getByRole('menuitem', { name: 'Add Page', exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "Add Page" available without any feature toggle');
    await page.keyboard.press('Escape');

    // Create guard_intro NESTED under "room" (not top-level) via the tree's own
    // "…" context menu, so the parent-inheritance fix below has a non-null
    // parent to actually test against.
    const roomLabel = tree.getByText('room', { exact: true });
    const roomOptionsButton = roomLabel.locator('xpath=ancestor::*[contains(@class,"group")][1]').locator('button[title="Options"]');
    await roomOptionsButton.click();
    await page.getByRole('button', { name: 'Add Page here', exact: true }).click();
    await page.locator('text=Add Page in "room"').waitFor({ timeout: 5000 });
    console.log('PASS: adding a page under "room" prompts "Add Page in \\"room\\""');
    await page.fill('#element-name', 'guard_intro');
    await page.getByRole('button', { name: 'Add Page', exact: true }).click();
    await tree.getByText('guard_intro', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });

    // Nesting check: guard_intro's row must be indented further right than
    // room's own row (i.e. actually nested under it, not a top-level sibling).
    const roomBox = await roomLabel.boundingBox();
    const guardIntroBox = await tree.getByText('guard_intro', { exact: true }).boundingBox();
    if (!(guardIntroBox.x > roomBox.x)) {
        throw new Error(`Expected guard_intro (x=${guardIntroBox.x}) to be indented further than room (x=${roomBox.x})`);
    }
    console.log('PASS: page created nested under "room" via context menu');

    // The new page shows the Page tab (page type dropdown + Options control) and
    // not the standard object tabs.
    await tree.getByText('guard_intro', { exact: true }).click();
    await tabBar.getByRole('button', { name: 'Page', exact: true }).waitFor({ state: 'visible', timeout: 10000 });
    await tabBar.getByRole('button', { name: 'Page', exact: true }).click();
    await page.locator('text=Page type').waitFor({ timeout: 10000 });
    await page.locator('text=Options').first().waitFor({ timeout: 5000 });
    console.log('PASS: Page tab with page type dropdown and Options control');
    for (const label of ['Setup', 'Inventory', 'Features']) {
        const visible = await page.getByRole('button', { name: label, exact: true }).isVisible().catch(() => false);
        if (visible) throw new Error(`Standard object tab "${label}" should be hidden for a dialogue page`);
    }
    console.log('PASS: standard object tabs hidden for the page');

    // Rich-text "Insert" toolbar: the pinned Page Link button (previously
    // missing entirely in TA mode) must be present, and its picker must list
    // guard_intro but not room/player/game (pages only, not every object).
    const descriptionContainer = page.locator('#richtext-description').locator('xpath=..');
    const pageLinkButton = descriptionContainer.getByRole('button', { name: 'Page', exact: true });
    await pageLinkButton.waitFor({ state: 'visible', timeout: 5000 });
    await pageLinkButton.click();
    const linkDialog = page.locator('[role="dialog"]').filter({ hasText: 'Page' }).first();
    await linkDialog.waitFor({ timeout: 5000 });
    await linkDialog.getByRole('combobox').first().click();
    let pageLinkOptions = await page.locator('[role="option"]').allTextContents();
    if (!pageLinkOptions.includes('guard_intro')) {
        throw new Error(`Expected the Page Link picker to list guard_intro, got: ${pageLinkOptions.join(', ')}`);
    }
    for (const notAPage of ['room', 'player', 'game']) {
        if (pageLinkOptions.includes(notAPage)) {
            throw new Error(`Page Link picker should not list "${notAPage}" (not a dialoguepage), got: ${pageLinkOptions.join(', ')}`);
        }
    }
    console.log('PASS: rich-text "Insert" toolbar has a Page Link picker, filtered to pages only');
    await page.keyboard.press('Escape');
    await linkDialog.waitFor({ state: 'hidden', timeout: 5000 });

    // Inline "+ New page" creation from the options control must inherit the
    // CURRENT page's parent ("room"), not always create at the top level.
    await page.click('button:has-text("+ New page")');
    await page.locator('text=Add Page in "room"').waitFor({ timeout: 5000 });
    console.log('PASS: "+ New page" from the options list also prompts "Add Page in \\"room\\"" (inherits current page\'s parent)');
    await page.fill('#element-name', 'guard_weather');
    await page.getByRole('button', { name: 'Add Page', exact: true }).click();
    await page.locator('span[title="guard_weather"]').waitFor({ timeout: 5000 });
    console.log('PASS: inline "+ New page" created and linked guard_weather');

    const guardWeatherBox = await tree.getByText('guard_weather', { exact: true }).boundingBox();
    if (!(guardWeatherBox.x > roomBox.x)) {
        throw new Error(`Expected guard_weather (x=${guardWeatherBox.x}) to be nested under room (x=${roomBox.x}), not top-level`);
    }
    console.log('PASS: inline-created page is nested under the same parent ("room") as the current page');

    // The inline-created page is a real dialoguepage: it must appear in the
    // sibling page's key dropdown (page-typed source), and creating it must not
    // have moved the selection away from guard_intro.
    await tree.getByText('guard_weather', { exact: true }).click();
    await tabBar.getByRole('button', { name: 'Page', exact: true }).waitFor({ state: 'visible', timeout: 10000 });
    await tabBar.getByRole('button', { name: 'Page', exact: true }).click();
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
    await dialog.locator('text=Show a page').click();
    await dialog.getByRole('button', { name: 'OK' }).click();

    // The added script's page picker must list guard_intro/guard_weather only,
    // not rooms/objects/commands.
    const showPageLabel = page.getByText('Show page', { exact: true });
    const pageValueSelect = showPageLabel.locator('xpath=following-sibling::select[2]');
    const scriptPageOptions = (await pageValueSelect.locator('option').allTextContents()).filter(Boolean);
    if (!(scriptPageOptions.includes('guard_intro') && scriptPageOptions.includes('guard_weather'))) {
        throw new Error(`Expected "Show page"'s picker to list both pages, got: ${scriptPageOptions.join(', ')}`);
    }
    for (const notAPage of ['room', 'player', 'game']) {
        if (scriptPageOptions.includes(notAPage)) {
            throw new Error(`"Show page"'s picker should not list "${notAPage}" (not a dialoguepage), got: ${scriptPageOptions.join(', ')}`);
        }
    }
    console.log('PASS: "Show page" script adder\'s page dropdown is pages-only, not objects/rooms');

    // The two checkboxes must render on separate lines, not run together.
    const allowCancelLabel = page.getByText('Allow the player to leave the dialogue', { exact: false });
    const runTurnScriptsLabel = page.getByText('Run turn scripts during the dialogue', { exact: false });
    const allowCancelBox = await allowCancelLabel.boundingBox();
    const runTurnScriptsBox = await runTurnScriptsLabel.boundingBox();
    if (!(runTurnScriptsBox.y > allowCancelBox.y + 5)) {
        throw new Error(`Expected the two checkbox rows on separate lines, got y=${allowCancelBox.y} and y=${runTurnScriptsBox.y}`);
    }
    console.log('PASS: the two checkboxes render on separate lines');

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
