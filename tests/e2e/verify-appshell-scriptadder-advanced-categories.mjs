// Verifies workstream 3 of docs/editor-progressive-disclosure.md: script
// command categories where *every* visible command is <advanced/> (mirroring
// v5's GetCategories filter in reverse) are demoted below a divider in
// AddScriptModal's category sidebar, instead of presented as peers of
// everyday categories like "Output". Every command stays reachable — the
// filter box searches all categories regardless of the divider.
//
// "Drawing" (CoreEditorScriptsDrawing.aslx) is entirely <advanced/> but
// gated behind game.gridmap, off by default — enable it via the "Map and
// Drawing Grid" checkbox on the Interface tab so the category actually has
// visible commands and appears at all (a category with zero visible
// commands doesn't show up either way).
//
// NOTE: "Darkness" (CoreEditorScriptsDarkness.aslx) used to be this test's
// example instead, but PR #1937's progressive-disclosure metadata audit
// stripped <advanced/> from all of its commands, so it no longer qualifies
// as an entirely-advanced category. Drawing is the only category left where
// every command is still <advanced/> (checked via `grep -c` against every
// CoreEditorScripts*.aslx file).
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `ScriptAdder Advanced Cats Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // "game" is auto-selected but doesn't push into the mobile properties pane; tap it.
    await page.locator('[data-value="game"][data-part="branch-control"]').click();

    // Enable "Map and Drawing Grid" on the Interface tab so the (entirely-advanced) Drawing
    // script category actually has visible commands. The checkbox itself isn't <advanced/>,
    // so it renders directly without needing to open an expander.
    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    await page.getByText('Map and Drawing Grid', { exact: false }).click();

    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    await page.getByText('+ Add script', { exact: false }).first().click();

    const sidebar = page.locator('[role="listbox"][aria-label="Script command categories"]');
    await sidebar.getByText('Output', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });

    const categoryLabels = await sidebar.locator('button').allTextContents();
    const divider = sidebar.getByText('Advanced', { exact: true });
    await divider.waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: "Advanced" divider is present once an entirely-advanced category has visible commands');

    const dividerBox = await divider.boundingBox();
    const outputIndex = categoryLabels.indexOf('Output');
    const drawingIndex = categoryLabels.indexOf('Drawing');
    if (outputIndex === -1 || drawingIndex === -1) {
        throw new Error(`Expected both "Output" and "Drawing" categories, got: ${categoryLabels.join(', ')}`);
    }
    if (!(outputIndex < drawingIndex)) {
        throw new Error(`Expected "Output" before "Drawing", got order: ${categoryLabels.join(', ')}`);
    }
    console.log('PASS: non-advanced "Output" is ordered before advanced "Drawing"');

    const outputBox = await sidebar.getByText('Output', { exact: true }).boundingBox();
    const drawingBox = await sidebar.getByText('Drawing', { exact: true }).boundingBox();
    if (!(outputBox.y < dividerBox.y && dividerBox.y < drawingBox.y)) {
        throw new Error('Expected the divider to sit visually between "Output" and "Drawing"');
    }
    console.log('PASS: divider sits visually between non-advanced and advanced categories');

    // Every command is still reachable: select Drawing and confirm it has commands,
    // then create one.
    await sidebar.getByText('Drawing', { exact: true }).click();
    const commandList = page.locator('[role="listbox"][aria-label^="Script commands in Drawing"]');
    const commandCount = await commandList.locator('button').count();
    if (commandCount === 0) throw new Error('Drawing category should list its commands');
    console.log(`PASS: Drawing category lists ${commandCount} command(s)`);

    const startScriptSection = page.locator('text=Start script:').locator('..');
    const selectCountBefore = await startScriptSection.locator('select').count();
    await commandList.locator('button').first().click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.getByRole('dialog').waitFor({ state: 'hidden', timeout: 3000 });
    // Adding "Show/hide custom drawing layer" inserts a new yes/no/expression <select> into
    // the previously-empty Start script — its presence confirms the command was added.
    await page.waitForFunction(
        (before) => document.querySelectorAll('select').length > before,
        selectCountBefore,
        { timeout: 5000 }
    );
    console.log('PASS: an advanced-category command can still be added and appears in the script');

    // The filter box searches everything regardless of the divider.
    await page.getByText('+ Add script', { exact: false }).first().click();
    await page.fill('input[placeholder="Filter commands..."]', 'draw');
    await page.waitForTimeout(200);
    const filteredMatches = page.locator('[role="listbox"][aria-label="Matching script commands"] button');
    const filteredCount = await filteredMatches.count();
    if (filteredCount === 0) throw new Error('Filtering for "draw" should still surface Drawing commands');
    console.log(`PASS: filter box surfaces ${filteredCount} advanced-category match(es) regardless of the divider`);

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/scriptadder-advanced-categories-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
