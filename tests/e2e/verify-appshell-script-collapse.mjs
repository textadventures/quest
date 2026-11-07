// #2661: each branch of an if (then, else if, else) and a command's own nested script
// block (wait, for, show menu ...) can be collapsed, so nested ifs are readable.
//
// Checks that collapsing hides just that block's scripts and shows how many it holds,
// that a collapsed block stays collapsed after an edit reloads the script data and
// shifts its index (the state is keyed by script id, not position), and that
// expanding brings the scripts back.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const code = `msg ("top")
if (true) {
  msg ("then one")
  msg ("then two")
}
else if (false) {
  msg ("elseif one")
}
else {
  msg ("else one")
  wait {
    msg ("in wait")
  }
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

// Whether a script text box holding exactly this value is on the page.
async function hasValue(page, value) {
    return page.evaluate(
        v => [...document.querySelectorAll('input, textarea')].some(el => el.value === v),
        value,
    );
}

async function expectValues(page, present, absent, when) {
    for (const v of present) {
        if (!(await hasValue(page, v))) throw new Error(`${when}: "${v}" should be shown`);
    }
    for (const v of absent) {
        if (await hasValue(page, v)) throw new Error(`${when}: "${v}" should be hidden`);
    }
}

// The collapse toggles, in document order: then, else if, else, then the wait's block.
const toggles = page => page.locator('button[aria-label="Collapse block"], button[aria-label="Expand block"]');

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Collapse Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('text=Game name:', { timeout: 30000 });

    await page.getByText('room', { exact: true }).first().click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Code view")', { timeout: 5000 });

    await setScriptViaCodeView(page, code);
    await page.waitForFunction(
        () => [...document.querySelectorAll('input, textarea')].some(el => el.value === 'in wait'),
        null,
        { timeout: 30000 },
    );

    const count = await toggles(page).count();
    if (count < 4) throw new Error(`Expected at least 4 collapse toggles (then, else if, else, wait), found ${count}`);
    for (let i = 0; i < 4; i++) {
        if (await toggles(page).nth(i).getAttribute('aria-expanded') !== 'true') {
            throw new Error(`Toggle ${i} should start expanded`);
        }
    }
    console.log(`PASS: ${count} collapse toggles, all expanded to begin with`);

    // Collapse then: its two scripts go, the rest stay, and the line says how many.
    await toggles(page).nth(0).click();
    await expectValues(page, ['top', 'elseif one', 'else one', 'in wait'], ['then one', 'then two'], 'then collapsed');
    if (await page.locator('text=(2 scripts)').count() !== 1) throw new Error('Collapsed then should show "(2 scripts)"');
    if (await toggles(page).nth(0).getAttribute('aria-expanded') !== 'false') throw new Error('Collapsed toggle should have aria-expanded=false');
    console.log('PASS: collapsing then hides its scripts and shows "(2 scripts)"');

    // Collapse the wait's nested block inside else.
    await toggles(page).nth(3).click();
    await expectValues(page, ['else one'], ['in wait'], 'wait collapsed');
    if (await page.locator('text=(1 script)').count() !== 1) throw new Error('Collapsed wait block should show "(1 script)"');
    console.log('PASS: collapsing a nested script block hides it and shows "(1 script)"');

    // Move "top" below the if: an edit that reloads the script data and moves the if
    // from index 1 to 0. Both blocks should stay collapsed.
    // The first row's hover cluster is the root "top" row's.
    const moveDown = page.locator('div.absolute.top-1 button[title="Move down"]').first();
    await moveDown.hover();
    await moveDown.click();
    await page.waitForFunction(() => {
        const vals = [...document.querySelectorAll('input, textarea')].map(el => el.value);
        return vals.indexOf('top') > vals.indexOf('else one');
    }, null, { timeout: 10000 });
    await expectValues(page, ['top', 'elseif one', 'else one'], ['then one', 'then two', 'in wait'], 'after moving a script');
    console.log('PASS: collapsed blocks stay collapsed after an edit moves the if');

    // Expand both again.
    await toggles(page).nth(0).click();
    await toggles(page).nth(3).click();
    await expectValues(page, ['top', 'then one', 'then two', 'elseif one', 'else one', 'in wait'], [], 'after expanding');
    if (await page.locator('text=/\\(\\d+ scripts?\\)/').count() !== 0) throw new Error('No block counts should show once everything is expanded');
    console.log('PASS: expanding brings the scripts back');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
