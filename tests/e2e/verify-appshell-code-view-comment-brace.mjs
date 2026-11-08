// #2695: a commented-out line holding a brace, such as "//if (x) {", inside a block used to make
// the Editor's parser miss the block's closing brace. The script loaded as unparsed text, every
// edit to it failed to parse, and the per-script Code view dropped the edit without a word when
// the author clicked away. Checks that such a script now parses and takes edits, and that an edit
// which really doesn't parse is reported in a toast when leaving Code view or the element, rather
// than vanishing along with its inline error.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

// CodeMirror doesn't reliably take locator.fill() or long typed text (see
// verify-appshell-code-view.mjs), so replace its content with a single paste.
const selectAllKey = process.platform === 'darwin' ? 'Meta+A' : 'Control+A';
const pasteKey = process.platform === 'darwin' ? 'Meta+V' : 'Control+V';

async function setCmContent(page, text) {
    const cm = page.locator('.cm-editor .cm-content').first();
    await cm.click();
    await page.keyboard.press(selectAllKey);
    await page.evaluate(t => navigator.clipboard.writeText(t), text);
    await page.keyboard.press(pasteKey);
}

async function blurCodeView(page) {
    await page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); });
}

async function cmText(page) {
    return page.locator('.cm-content').first().innerText();
}

async function openStartScript(page) {
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('text=Start script', { timeout: 5000 });
}

const notAppliedToast = '[role="alert"]:has-text("wasn\'t applied")';

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    await ctx.grantPermissions(['clipboard-read', 'clipboard-write']);
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Comment Brace Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await openStartScript(page);
    await page.click('button:has-text("Code view")');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });

    // --- A commented-out "if (...) {" inside a block parses, and takes edits ---
    await setCmContent(page, 'if (true) {\n//if (false) {\nmsg ("first")\n}');
    await blurCodeView(page);
    // Give the blur save a moment to come back with an error, if it's going to.
    await page.waitForTimeout(500);
    if (await page.locator("text=Missing '}'").count() > 0) {
        throw new Error("A commented-out '{' inside a block still fails to parse with \"Missing '}'\"");
    }
    console.log('PASS: a block holding a commented-out "if (...) {" parses');

    await page.click('button:has-text("Visual editor")');
    await page.click('button:has-text("Code view")');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    const code = await cmText(page);
    if (!code.includes('msg ("first")')) throw new Error(`Expected the saved script back in Code view, got:\n${code}`);
    // The comment's '{' must not indent what follows it an extra level.
    if (!/\n {2}msg \("first"\)/.test(code) || /\n {4}msg \("first"\)/.test(code)) {
        throw new Error(`Expected the line after the comment at one level of indent, got:\n${code}`);
    }
    console.log('PASS: the line after the comment is not indented an extra level');

    await setCmContent(page, code.replace('msg ("first")', 'msg ("first")\n  msg ("second")'));
    await page.click('button:has-text("Visual editor")');
    await page.click('button:has-text("Code view")');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    if (!(await cmText(page)).includes('msg ("second")')) {
        throw new Error('An edit to a script holding a commented-out brace was not kept');
    }
    if (await page.locator(notAppliedToast).count() > 0) throw new Error('Unexpected "wasn\'t applied" toast after a good edit');
    console.log('PASS: an edit to that script is kept');

    // --- An edit that really doesn't parse is reported when leaving Code view ---
    await setCmContent(page, 'if (x) {\nmsg ("lost")');
    // Blur first: a direct click on the toggle blurs on mousedown, and the error banner that
    // appears then pushes the toggle down so the click misses it (leaving the author in Code view
    // with the error on screen, which is fine, but not what this step checks).
    await blurCodeView(page);
    await page.waitForSelector("text=Missing '}'", { timeout: 5000 });
    await page.click('button:has-text("Visual editor")');
    await page.waitForSelector(notAppliedToast, { timeout: 5000 });
    console.log('PASS: switching to the visual editor after an edit that failed to apply shows a toast');
    await page.locator(`${notAppliedToast} button`).click();

    // --- ...and when picking another element ---
    await page.click('button:has-text("Code view")');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    await setCmContent(page, 'if (x) {\nmsg ("lost again")');
    await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
    await page.waitForSelector(notAppliedToast, { timeout: 5000 });
    console.log('PASS: picking another element after an edit that failed to apply shows a toast');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
