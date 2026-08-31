// Follow-up to workstream 3 of docs/editor-progressive-disclosure.md: the
// category-level divider (verify-appshell-scriptadder-advanced-categories.mjs)
// only demotes categories where *every* command is advanced — Darkness/Drawing,
// both gated behind off-by-default features, so the effect was invisible on a
// stock game. This extends the same idea one level down: within a mixed
// category, advanced commands sort after a divider too, e.g. "Scripts" (If,
// First time non-advanced; Comment/For/For each/Switch/While/etc. advanced) —
// visible on any fresh game, no feature flags required.
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
    await page.fill('input[placeholder="Game name"]', `ScriptAdder Advanced Cmds Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    await page.getByText('+ Add script', { exact: false }).first().click();
    await page.getByRole('option', { name: 'Scripts', exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    await page.getByRole('option', { name: 'Scripts', exact: true }).click();

    const commandList = page.locator('[role="listbox"][aria-label^="Script commands in Scripts"]');
    await commandList.getByText('If...', { exact: false }).waitFor({ state: 'visible', timeout: 5000 });

    const rowTexts = await commandList.locator('button, div').allTextContents();
    const cleaned = rowTexts.map(t => t.replace('●', '').trim()).filter(Boolean);
    const dividerIndex = cleaned.indexOf('Advanced');
    const ifIndex = cleaned.indexOf('If...');
    const forIndex = cleaned.indexOf('For...');
    if (dividerIndex === -1) throw new Error(`Expected an "Advanced" divider within the mixed "Scripts" category, got: ${cleaned.join(', ')}`);
    if (!(ifIndex < dividerIndex && dividerIndex < forIndex)) {
        throw new Error(`Expected order If... < divider < For..., got: ${cleaned.join(', ')}`);
    }
    console.log('PASS: "Scripts" category shows non-advanced commands (If...) before the divider and advanced ones (For...) after');

    // The category itself must NOT be demoted below the category-level divider —
    // "Scripts" has non-advanced commands, so it's not an all-advanced category.
    const sidebar = page.locator('[role="listbox"][aria-label="Script command categories"]');
    const categoryDividerVisible = await sidebar.getByText('Advanced', { exact: true }).isVisible().catch(() => false);
    if (categoryDividerVisible) throw new Error('No category should be fully-advanced with default game content — the category-level divider should not appear here');
    console.log('PASS: mixed categories like "Scripts" stay out of the category-level divider');

    // Advanced commands are still directly clickable/addable, not hidden behind anything.
    await commandList.getByText('For...', { exact: false }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.getByRole('dialog').waitFor({ state: 'hidden', timeout: 3000 });
    console.log('PASS: an advanced command below the divider is still directly clickable and addable');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/scriptadder-advanced-commands-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
