// Regression test for #2586: a script nested ~15 blocks deep (e.g. a chain of
// "wait for key press, then" blocks) never loaded in the visual editor - the
// bridge's JSON serializer hit its default MaxDepth of 64, the exception aborted
// ScriptEditor's load effect, and the editor sat on "Loading commands..." forever.
// Each level also indented by ~56px, so even a script that did load was squeezed
// to an unreadable sliver a dozen levels in.
//
// Checks that a 20-deep script loads with room to spare at its innermost level,
// and that one too deep to show at all gets an error pointing at Code view
// instead of hanging.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

function nestedWaits(depth, innermost) {
    let code = '';
    for (let i = 0; i < depth; i++) code += 'msg ("")\nwait {\n';
    code += `msg ("${innermost}")\n`;
    for (let i = 0; i < depth; i++) code += '}\n';
    return code;
}

// Replaces the first script editor's code via Code view, then switches back to
// the visual editor.
async function setScriptViaCodeView(page, code) {
    await page.locator('button:has-text("Code view")').first().click();
    await page.waitForSelector('.cm-content', { timeout: 5000 });
    await page.click('.cm-content');
    await page.keyboard.press('ControlOrMeta+a');
    await page.keyboard.insertText(code);
    await page.keyboard.press('ControlOrMeta+s');
    await page.locator('button:has-text("Visual editor")').first().click();
}

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Deep Nesting Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('text=Game name:', { timeout: 30000 });

    await page.getByText('room', { exact: true }).first().click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('button:has-text("Code view")', { timeout: 5000 });

    // 20 levels: past the old ~15-level serializer limit.
    await setScriptViaCodeView(page, nestedWaits(20, 'innermost'));
    await page.waitForFunction(
        () => [...document.querySelectorAll('input, textarea')].some(el => el.value === 'innermost'),
        null,
        { timeout: 30000 },
    );
    const innermost = await page.evaluate(() => {
        const el = [...document.querySelectorAll('input, textarea')].find(e => e.value === 'innermost');
        const r = el.getBoundingClientRect();
        return { width: Math.round(r.width), right: Math.round(r.right) };
    });
    // With ~56px of indent a level the innermost box hit its 64px minimum width
    // here; at ~18px it's over 300px.
    if (innermost.width < 250) throw new Error(`Innermost text box at depth 20 should keep a usable width, was ${innermost.width}px`);
    if (innermost.right > 1280) throw new Error(`Innermost text box at depth 20 should fit in the viewport, its right edge was at ${innermost.right}px`);
    if (await page.locator('text=couldn\'t be shown in the visual editor').count() > 0) throw new Error('A 20-deep script should load without the load-failure message');
    console.log(`PASS: a 20-deep script loads in the visual editor (innermost box ${innermost.width}px wide)`);

    // Deep enough (~1100 JSON levels) to exceed even the raised MaxDepth, which
    // stands in for any bridge failure: the editor must say so rather than hang.
    await setScriptViaCodeView(page, nestedWaits(270, 'too deep'));
    await page.waitForSelector('text=couldn\'t be shown in the visual editor', { timeout: 30000 });
    if (await page.locator('text=Loading commands').count() > 0) throw new Error('The load-failure message should replace "Loading commands…", not sit alongside it');
    if (await page.locator('button:has-text("Code view")').count() === 0) throw new Error('Code view should still be offered when the visual editor can\'t load a script');
    console.log('PASS: a script too deep to show reports an error and still offers Code view');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
