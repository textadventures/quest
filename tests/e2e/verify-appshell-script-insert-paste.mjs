// #2663 / #2664: adding, pasting and moving scripts in the middle of a block, and
// seeing which block an Add script button belongs to.
//
// Checks that:
// - a row's ⋯ menu adds a script above or below that row, not at the end
// - Copy clears the ticks, and the ⋯ menu's Paste above puts the copy beside a row
// - the selection toolbar's Paste below pastes after the ticked row
// - Move up/down move a run of several ticked rows together, as one undo step
// - a nested block's Add script button names its block, and focusing a script inside
//   the block highlights the block's frame
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const code = `msg ("one")
msg ("two")
msg ("three")
if (true) {
  msg ("in then")
}
else {
  msg ("in else")
}
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

// The first text box's value in each top-level row of the first script editor, in order
// ("(if)" for an if row).
async function rootRows(page) {
    return page.evaluate(() => {
        const region = document.querySelector('div[role="region"]');
        return [...region.querySelectorAll(':scope > div.script-row')].map(row => {
            if (row.querySelector(':scope > div.flex-1 > div.text-xs > div > span')?.textContent === 'if') return '(if)';
            const input = row.querySelector('input[type="text"], textarea, .cm-content');
            return input ? (input.value ?? input.textContent ?? '') : '?';
        });
    });
}

async function expectRows(page, expected, when) {
    // Each edit reloads the script data; give the re-render a moment to settle.
    let actual;
    for (let attempt = 0; attempt < 20; attempt++) {
        actual = await rootRows(page);
        if (JSON.stringify(actual) === JSON.stringify(expected)) return;
        await page.waitForTimeout(100);
    }
    throw new Error(`${when}: expected rows ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
}

const rootRow = (page, i) => page.locator('div[role="region"]').first().locator(':scope > div.script-row').nth(i);
const rowCheckbox = (page, i) => rootRow(page, i).locator(':scope > label input[type="checkbox"]');

async function openRowMenu(page, i) {
    const row = rootRow(page, i);
    await row.hover();
    await row.locator(':scope > div.flex-1 > div.absolute button[aria-label="More actions"]').click();
    return page.locator('[role="menu"]');
}

async function addComment(page) {
    const dialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await dialog.waitFor({ timeout: 10000 });
    const filter = dialog.getByPlaceholder('Filter commands...');
    await filter.fill('Comment');
    await page.waitForTimeout(200);
    await filter.press('Enter');
    await dialog.waitFor({ state: 'hidden', timeout: 5000 });
}

// A newly added comment's code box shows only its placeholder.
const NEW = 'Type comment';

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Insert Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('text=Game name:', { timeout: 30000 });

    await page.getByText('room', { exact: true }).first().click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Code view")', { timeout: 5000 });

    await setScriptViaCodeView(page, code);
    await page.waitForFunction(
        () => [...document.querySelectorAll('input, textarea')].some(el => el.value === 'in else'),
        null,
        { timeout: 30000 },
    );
    await expectRows(page, ['one', 'two', 'three', '(if)'], 'initial script');

    // ⋯ → Add script below on "one" inserts at index 1.
    let menu = await openRowMenu(page, 0);
    await menu.getByRole('menuitem', { name: 'Add script below' }).click();
    await addComment(page);
    await expectRows(page, ['one', NEW, 'two', 'three', '(if)'], 'after Add script below "one"');
    console.log('PASS: ⋯ → Add script below inserts after the row');

    // ⋯ → Add script above on "three".
    menu = await openRowMenu(page, 3);
    await menu.getByRole('menuitem', { name: 'Add script above' }).click();
    await addComment(page);
    await expectRows(page, ['one', NEW, 'two', NEW, 'three', '(if)'], 'after Add script above "three"');
    console.log('PASS: ⋯ → Add script above inserts before the row');

    // Remove the two comments again with the selection toolbar's Delete.
    await rowCheckbox(page, 1).check();
    await rowCheckbox(page, 3).check();
    await page.locator('div[role="region"] button:text-is("Delete")').first().click();
    await expectRows(page, ['one', 'two', 'three', '(if)'], 'after deleting the comments');

    // Copy "one": the tick is cleared, then ⋯ → Paste above on "three".
    await rowCheckbox(page, 0).check();
    await page.locator('div[role="region"] button:text-is("Copy")').first().click();
    if (await rowCheckbox(page, 0).isChecked()) throw new Error('Copy should clear the ticked rows');
    menu = await openRowMenu(page, 2);
    await menu.getByRole('menuitem', { name: 'Paste above' }).click();
    await expectRows(page, ['one', 'two', 'one', 'three', '(if)'], 'after ⋯ → Paste above "three"');
    console.log('PASS: Copy clears the ticks, and ⋯ → Paste above pastes before the row');

    // Toolbar Paste below, with "two" ticked.
    await rowCheckbox(page, 1).check();
    await page.locator('div[role="region"] button:text-is("Paste below")').first().click();
    await expectRows(page, ['one', 'two', 'one', 'one', 'three', '(if)'], 'after toolbar Paste below "two"');
    console.log('PASS: the selection toolbar\'s Paste below pastes after the ticked row');

    // Tick rows 2-3 (the two pasted "one"s) and move them up together, then undo once.
    await page.keyboard.press('Escape');
    for (let i = 0; i < 6; i++) if (await rowCheckbox(page, i).isChecked()) await rowCheckbox(page, i).uncheck();
    await rowCheckbox(page, 2).check();
    await rowCheckbox(page, 3).check();
    await page.locator('div[role="region"] button:has-text("Move up")').first().click();
    await expectRows(page, ['one', 'one', 'one', 'two', 'three', '(if)'], 'after moving two rows up');
    if (!(await rowCheckbox(page, 1).isChecked()) || !(await rowCheckbox(page, 2).isChecked())) {
        throw new Error('The moved rows should stay ticked at their new positions');
    }
    await page.locator('div[role="region"] button:has-text("Move down")').first().click();
    await page.locator('div[role="region"] button:has-text("Move down")').first().click();
    await expectRows(page, ['one', 'two', 'three', 'one', 'one', '(if)'], 'after moving two rows down twice');
    console.log('PASS: Move up/down move a run of ticked rows together');

    // Undo should reverse one whole move of the run, not one swap.
    await page.locator('button[aria-label="Undo"]').first().click();
    await expectRows(page, ['one', 'two', 'one', 'one', 'three', '(if)'], 'after one undo');
    console.log('PASS: moving a run of rows is a single undo step');

    // A nested block's Add script button names its block.
    if (await page.locator('button[aria-label="Add script to else"]').count() !== 1) {
        throw new Error('The else block\'s Add script button should be labelled "Add script to else"');
    }
    if (await page.locator('button[aria-label="Add script to if true"]').count() !== 1) {
        throw new Error('The then block\'s Add script button should be labelled "Add script to if true"');
    }
    console.log('PASS: nested Add script buttons name their block');

    // Focusing a script inside else highlights the else block's frame, and not the then block's.
    await page.evaluate(() => {
        const el = [...document.querySelectorAll('input, textarea')].find(e => e.value === 'in else');
        el.focus();
    });
    const borders = await page.evaluate(() => {
        const block = el => el.closest('.script-block');
        const find = v => [...document.querySelectorAll('input, textarea')].find(e => e.value === v);
        return {
            elseBlock: getComputedStyle(block(find('in else'))).borderLeftColor,
            thenBlock: getComputedStyle(block(find('in then'))).borderLeftColor,
        };
    });
    if (borders.elseBlock === borders.thenBlock) {
        throw new Error(`Focusing a script in else should highlight else's frame differently from then's (both ${borders.elseBlock})`);
    }
    console.log(`PASS: focus highlights the block it's in (${borders.elseBlock} vs ${borders.thenBlock})`);

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
