// Issue #2192's second concern: getCSSRule returns `false` when a selector
// isn't found (including cross-origin stylesheets it can't read), and every
// caller (SetMenuBackground etc.) dereferenced that unchecked, throwing
// TypeError. Fixed by having those setters call addCSSRule instead, which
// inserts the rule if missing and never returns false.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';
const url = `${baseUrl}/?url=/examples/simple.aslx`;
let browser;
try {
    browser = await chromium.launch();
    const page = await browser.newPage();
    const consoleErrors = [];
    page.on('console', (msg) => {
        if (msg.type() === 'error') consoleErrors.push(msg.text());
    });
    page.on('pageerror', (err) => consoleErrors.push(String(err)));

    await page.goto(url, { waitUntil: 'load' });
    // playercore.js is a plain <script> tag, so these are on window as soon
    // as it parses - no need to wait for the WASM game itself to boot.
    await page.waitForFunction(() => typeof window.SetMenuBackground === 'function');

    const result = await page.evaluate(() => {
        const out = {};

        // 1. Existing selectors that ship in playercore.css/chrome.css - the
        //    original bug's repro path (getCSSRule finding a real rule).
        try {
            window.SetMenuBackground('rgb(1, 2, 3)');
            out.setMenuBackgroundOk = true;
        } catch (e) {
            out.setMenuBackgroundOk = false;
            out.setMenuBackgroundError = String(e);
        }
        const jjMenuItem = window.getCSSRule('div.jj_menu_item');
        out.menuBackgroundApplied = jjMenuItem && jjMenuItem.style.backgroundColor;

        try {
            window.TurnOffHyperlinksUnderline();
            out.turnOffHyperlinksOk = true;
        } catch (e) {
            out.turnOffHyperlinksOk = false;
            out.turnOffHyperlinksError = String(e);
        }

        // 2. A selector that genuinely doesn't exist in any stylesheet.
        //    Before this change, getCSSRule would return false and the
        //    caller's unchecked .style dereference would throw; addCSSRule
        //    must insert an empty rule and return it instead.
        const fakeSelector = 'div.qv-test-nonexistent-rule-2192';
        out.fakeRuleMissingBeforehand = window.getCSSRule(fakeSelector) === false;
        try {
            const created = window.addCSSRule(fakeSelector);
            out.addCSSRuleOk = !!created && typeof created.style === 'object';
            if (created) {
                created.style.color = 'rgb(9, 9, 9)';
                out.fakeRuleStyleSettable = created.style.color === 'rgb(9, 9, 9)';
            }
        } catch (e) {
            out.addCSSRuleOk = false;
            out.addCSSRuleError = String(e);
        }
        out.fakeRuleFoundAfterInsert = window.getCSSRule(fakeSelector) !== false;

        return out;
    });

    console.log(JSON.stringify(result, null, 2));
    console.log('console/page errors captured:', consoleErrors);

    const failures = [];
    if (!result.setMenuBackgroundOk) failures.push('SetMenuBackground threw');
    if (result.menuBackgroundApplied !== 'rgb(1, 2, 3)') failures.push(`menu background not applied: ${result.menuBackgroundApplied}`);
    if (!result.turnOffHyperlinksOk) failures.push('TurnOffHyperlinksUnderline threw');
    if (!result.fakeRuleMissingBeforehand) failures.push('test selector unexpectedly pre-existed');
    if (!result.addCSSRuleOk) failures.push('addCSSRule threw or returned no rule for a missing selector');
    if (!result.fakeRuleStyleSettable) failures.push('inserted rule style was not settable/readable');
    if (!result.fakeRuleFoundAfterInsert) failures.push('getCSSRule still cannot find the rule after addCSSRule inserted it');

    if (failures.length) {
        throw new Error('Verification failed:\n' + failures.join('\n'));
    }
    console.log('PASS');
} catch (err) {
    console.error('FAIL:', err);
    process.exitCode = 1;
} finally {
    if (browser) await browser.close();
}
