// Verifies #2394: a gamebook page's Options list shows options in the order
// they're stored (which is the order the player lists them), not an order
// that drifts once options have been removed, and its Move up / Move down
// buttons reorder them.
//
// The repro is the issue's: remove the template's options from Page1, then
// add one to "Zebra" and one to "Apple". The editor used to list them as
// Apple, Zebra (the second option landed in the first freed slot of a plain
// Dictionary), though the player shows Zebra first.
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
    await page.fill('input[placeholder="Game name"]', `Option Order ${Date.now()}`);
    await page.waitForSelector('text=Game type', { timeout: 10000 });
    await page.locator('label:has-text("Gamebook") input[type="radio"]').check();
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    const tree = page.locator('.overflow-y-auto.p-1.text-xs');

    for (const name of ['Zebra', 'Apple']) {
        await page.click('button[title="Add element"]');
        await page.getByRole('menuitem', { name: 'Add Page', exact: true }).click();
        await page.fill('#element-name', name);
        await page.getByRole('button', { name: 'Add Page', exact: true }).click();
        await tree.getByText(name, { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    }

    await tree.getByText('Page1', { exact: true }).click();

    // The options control: its "add" row holds the page-name dropdown, and
    // every row before it is an option.
    const keySelect = page.locator('select[aria-label="Please enter the page name"]');
    await keySelect.waitFor({ state: 'visible', timeout: 5000 });
    const list = keySelect.locator('xpath=../..');
    const optionRows = list.locator(':scope > div').filter({ has: page.locator('span[title]') });
    const optionKeys = async () => list.locator(':scope > div > span[title]').evaluateAll(spans => spans.map(s => s.getAttribute('title')));

    // Remove the template's options.
    const templateKeys = await optionKeys();
    if (templateKeys.length === 0) throw new Error('Expected the Gamebook template\'s Page1 to have options');
    for (let i = 0; i < templateKeys.length; i++) {
        await optionRows.first().locator('button:has-text("✕")').click();
    }
    await page.waitForFunction(() => !document.querySelector('select[aria-label="Please enter the page name"]')
        ?.parentElement?.parentElement?.querySelector(':scope > div > span[title]'), null, { timeout: 5000 });

    for (const [name, text] of [['Zebra', 'Zebra first'], ['Apple', 'Apple second']]) {
        await keySelect.selectOption(name);
        await page.fill('input[placeholder="Please enter the link text"]', text);
        await list.getByRole('button', { name: 'Add', exact: true }).click();
        await list.locator(`span[title="${name}"]`).waitFor({ state: 'visible', timeout: 5000 });
    }

    let keys = await optionKeys();
    if (keys.join(',') !== 'Zebra,Apple') throw new Error(`Expected options in stored order Zebra,Apple, got ${keys.join(',')}`);
    console.log('PASS: options are listed in the order they were added after earlier removals');

    // The first row can't move up, and the last can't move down.
    const zebraRow = optionRows.filter({ has: page.locator('span[title="Zebra"]') });
    const appleRow = optionRows.filter({ has: page.locator('span[title="Apple"]') });
    if (!await zebraRow.getByRole('button', { name: 'Move up' }).isDisabled()) throw new Error('First option\'s Move up should be disabled');
    if (!await appleRow.getByRole('button', { name: 'Move down' }).isDisabled()) throw new Error('Last option\'s Move down should be disabled');

    await zebraRow.getByRole('button', { name: 'Move down' }).click();
    await page.waitForFunction(() => {
        const spans = document.querySelector('select[aria-label="Please enter the page name"]')
            ?.parentElement?.parentElement?.querySelectorAll(':scope > div > span[title]') ?? [];
        return [...spans].map(s => s.getAttribute('title')).join(',') === 'Apple,Zebra';
    }, null, { timeout: 5000 });
    console.log('PASS: Move down puts Zebra after Apple');

    await zebraRow.getByRole('button', { name: 'Move up' }).click();
    await page.waitForFunction(() => {
        const spans = document.querySelector('select[aria-label="Please enter the page name"]')
            ?.parentElement?.parentElement?.querySelectorAll(':scope > div > span[title]') ?? [];
        return [...spans].map(s => s.getAttribute('title')).join(',') === 'Zebra,Apple';
    }, null, { timeout: 5000 });
    keys = await optionKeys();
    if (keys.join(',') !== 'Zebra,Apple') throw new Error(`Expected Zebra,Apple after Move up, got ${keys.join(',')}`);
    // The link text moves with its option.
    const zebraText = await zebraRow.locator('button.text-left').innerText();
    if (zebraText !== 'Zebra first') throw new Error(`Expected Zebra's link text to stay "Zebra first", got "${zebraText}"`);
    console.log('PASS: Move up puts Zebra back first, with its link text');

    console.log('PASS: all gamebook option order checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
    try {
        const pages = browser.contexts().flatMap(c => c.pages());
        if (pages[0]) await pages[0].screenshot({ path: '/tmp/gamebook-option-order-failure.png' });
    } catch { /* best-effort */ }
} finally {
    await browser.close();
}
