// #2662: make it clear what a script selection covers.
//
// Checks that a ticked row is tinted (so everything nested inside it visibly goes
// with it), that Copy and Cut say how many scripts they took and how many were nested
// inside them, that shift-click ticks a range, and that "Select to end" ticks every
// row from the first ticked one down.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const code = `msg ("one")
wait {
  msg ("in one")
  msg ("in two")
}
msg ("three")
msg ("four")
`;

async function setScriptViaCodeView(page, script) {
    await page.locator('button:has-text("Code view")').first().click();
    await page.waitForSelector('.cm-content', { timeout: 5000 });
    await page.click('.cm-content');
    await page.keyboard.press('ControlOrMeta+a');
    await page.keyboard.insertText(script);
    await page.keyboard.press('ControlOrMeta+s');
    await page.locator('button:has-text("Visual editor")').first().click();
}

const rootRow = (page, i) => page.locator('div[role="region"]').first().locator(':scope > div.script-row').nth(i);
const rowCheckbox = (page, i) => rootRow(page, i).locator(':scope > label input[type="checkbox"]');

async function checkedRows(page) {
    const result = [];
    for (let i = 0; i < 4; i++) if (await rowCheckbox(page, i).isChecked()) result.push(i);
    return result;
}

async function expectChecked(page, expected, when) {
    const actual = await checkedRows(page);
    if (JSON.stringify(actual) !== JSON.stringify(expected)) {
        throw new Error(`${when}: expected rows ${JSON.stringify(expected)} ticked, got ${JSON.stringify(actual)}`);
    }
}

async function clearTicks(page) {
    for (const i of await checkedRows(page)) await rowCheckbox(page, i).uncheck();
}

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Selection Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('text=Game name:', { timeout: 30000 });

    await page.getByText('room', { exact: true }).first().click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Code view")', { timeout: 5000 });

    await setScriptViaCodeView(page, code);
    await page.waitForFunction(
        () => [...document.querySelectorAll('input, textarea')].some(el => el.value === 'in two'),
        null,
        { timeout: 30000 },
    );

    // A ticked row is tinted, and the tint covers its nested body.
    const before = await rootRow(page, 1).evaluate(el => getComputedStyle(el).backgroundColor);
    await rowCheckbox(page, 1).check();
    const after = await rootRow(page, 1).evaluate(el => getComputedStyle(el).backgroundColor);
    if (before === after) throw new Error(`A ticked row should be tinted (background stayed ${before})`);
    console.log(`PASS: a ticked row is tinted (${before} → ${after})`);

    // Copy the wait: the toast counts the block and what's inside it.
    await page.locator('div[role="region"] button:text-is("Copy")').first().click();
    await page.getByText('Copied 1 script (and 2 scripts inside)').waitFor({ timeout: 5000 });
    console.log('PASS: Copy says "Copied 1 script (and 2 scripts inside)"');

    // Shift-click ticks a range.
    await clearTicks(page);
    await rowCheckbox(page, 0).click();
    await rowCheckbox(page, 2).click({ modifiers: ['Shift'] });
    await expectChecked(page, [0, 1, 2], 'after shift-click from row 0 to row 2');
    console.log('PASS: shift-click ticks a range');

    // Select to end, from row 2.
    await clearTicks(page);
    await rowCheckbox(page, 2).click();
    await page.locator('div[role="region"] button:text-is("Select to end")').first().click();
    await expectChecked(page, [2, 3], 'after Select to end from row 2');
    if (await page.locator('div[role="region"] button:text-is("Select to end")').count() !== 0) {
        throw new Error('"Select to end" should be hidden once the selection already reaches the end');
    }
    console.log('PASS: Select to end ticks every row from the first ticked one');

    // Cut the two plain scripts: no nested count.
    await page.locator('div[role="region"] button:text-is("Cut")').first().click();
    await page.getByText('Cut 2 scripts', { exact: true }).waitFor({ timeout: 5000 });
    console.log('PASS: Cut says "Cut 2 scripts"');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
