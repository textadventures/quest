// Regression test for issue #2439: a script or expression-template field with a simple and an
// expression mode used a <select> whose simple option named the field ("flag name", "caption"),
// so the name disappeared once the field was switched to "expression". Each field now has a
// plain name label that stays in both modes, and an "fx" toggle button that only switches mode.
// A field right after a label control (e.g. Print's message) is already named by that label, so
// it gets no extra label, but the label's text gives the toggle its accessible name.
//
// Also covers choosing "yes" in an ordinary script command's yes/no dropdown (play sound's
// "Wait for sound to finish"), which used to store false.
//
// Requires the AppShell dev server running locally (WasmEditor Debug build first):
//   cd src/AppShell && npm run dev
// Run: npx playwright test --project=appshell field-mode-toggle
// Set SCREENSHOT_DIR to also save screenshots of both modes there.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('field-mode-toggle', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const screenshotDir = process.env.SCREENSHOT_DIR;

    const browser = await chromium.launch();

    try {
        const ctx = await browser.newContext({ viewport: { width: 1400, height: 1000 } });
        const page = await ctx.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseUrl}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', `Field Mode Toggle Test ${Date.now()}`);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="Add element"]', { timeout: 30000 });
        console.log('PASS: local draft created and opened in the editor');

        await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
        await page.click('button[title="Add element"]');
        await page.click('button:has-text("Add Command to")', { timeout: 5000 });
        await page.waitForSelector('text=Command:', { timeout: 10000 });

        await page.click('button:has-text("Code view")');
        await page.locator('.cm-content').first().click();
        await page.keyboard.insertText([
            'if (GetBoolean(player, "lit")) {',
            '}',
            'player.class = ShowMenu("Your class?", Split("a;b", ";"), false)',
            'msg ("Hello")',
            'play sound ("", false, false)',
        ].join('\n'));
        await page.click('button:has-text("Visual editor")');
        await page.waitForSelector('button:has-text("Code view")', { timeout: 5000 });
        console.log('PASS: scripts entered via code view');

        const toggle = name => page.getByRole('button', { name: `Use an expression for ${name}`, exact: true });
        const label = text => page.getByText(text, { exact: true });
        const pressed = async name => (await toggle(name).getAttribute('aria-pressed')) === 'true';

        // ── Template fields: named by a label, toggled by fx ─────────────────────
        for (const name of ['object', 'flag name', 'caption']) {
            if (await toggle(name).count() !== 1) throw new Error(`Expected one fx toggle for "${name}", found ${await toggle(name).count()}`);
            if (await pressed(name)) throw new Error(`Expected "${name}" to start in simple mode`);
            if (!await label(name).isVisible()) throw new Error(`Expected a visible "${name}" label`);
        }
        if (await page.locator('select').evaluateAll(els => els.filter(e => e.value === 'flag name' || e.value === 'caption').length) !== 0) {
            throw new Error('Expected no <select> mode toggles naming a field');
        }
        console.log('PASS: template fields have a name label and an fx toggle, starting in simple mode');

        // #2442: the template pickers' option values are fixed keys rather than the (translated)
        // template names, so only the expression option itself has the value "expression".
        const pickerValues = await page.locator('select:has(option[value="expression"])').evaluateAll(
            els => els.map(e => [...e.options].map(o => o.value)));
        const templatePickers = pickerValues.filter(values => values.some(v => v.startsWith('template:')));
        if (templatePickers.length !== 2) throw new Error(`Expected 2 template pickers, found ${templatePickers.length}`);
        for (const values of templatePickers) {
            if (values.filter(v => v === 'expression').length !== 1 || !values.slice(1).every(v => v.startsWith('template:'))) {
                throw new Error(`Expected one "expression" option and template:N keys, got ${JSON.stringify(values)}`);
            }
        }
        console.log('PASS: template pickers use fixed option values');

        if (screenshotDir) await page.screenshot({ path: `${screenshotDir}/field-mode-toggle-simple.png` });

        const flagBox = await toggle('flag name').boundingBox();
        await toggle('flag name').click();
        if (!await pressed('flag name')) throw new Error('Expected "flag name" to switch to expression mode');
        if (!await label('flag name').isVisible()) throw new Error('Expected the "flag name" label to stay visible in expression mode');
        const flagBoxAfter = await toggle('flag name').boundingBox();
        if (Math.abs(flagBox.x - flagBoxAfter.x) > 1 || Math.abs(flagBox.y - flagBoxAfter.y) > 1) {
            throw new Error(`Expected the fx toggle to stay put, moved from ${JSON.stringify(flagBox)} to ${JSON.stringify(flagBoxAfter)}`);
        }
        const exprValues = await page.locator('input[type=text]').evaluateAll(els => els.map(e => e.value.trim()));
        if (!exprValues.includes('"lit"')) throw new Error(`Expected an expression box holding "lit" (quoted), got ${JSON.stringify(exprValues)}`);
        console.log('PASS: switching "flag name" to an expression keeps its label and toggle in place');

        await toggle('caption').click();
        if (!await pressed('caption') || !await label('caption').isVisible()) {
            throw new Error('Expected "caption" in expression mode with its label still visible');
        }
        console.log('PASS: ShowMenu "caption" keeps its name in expression mode');

        if (screenshotDir) await page.screenshot({ path: `${screenshotDir}/field-mode-toggle-expression.png` });

        await toggle('flag name').click();
        if (await pressed('flag name')) throw new Error('Expected "flag name" to switch back to simple mode');
        const simpleValues = await page.locator('input[type=text]').evaluateAll(els => els.map(e => e.value.trim()));
        if (!simpleValues.includes('lit')) throw new Error(`Expected the simple "flag name" box to hold lit, got ${JSON.stringify(simpleValues)}`);
        console.log('PASS: switching back to simple mode restores the plain value');

        // ── Ordinary command: named by its own label control ─────────────────────
        const printToggle = toggle('Print');
        if (await printToggle.count() !== 1) throw new Error('Expected Print\'s fx toggle to take its name from the "Print" label');
        if (await label('message').count() !== 0) throw new Error('Expected no extra "message" label next to Print\'s own label');
        console.log('PASS: Print\'s message field is named by its label control, with no extra label');

        // ── Ordinary yes/no field: choosing yes stores true ──────────────────────
        const waitSelect = page.getByRole('combobox', { name: 'Wait for sound to finish before continuing', exact: true });
        if (await waitSelect.count() !== 1) throw new Error('Expected the wait-for-sound dropdown to be named by its label');
        await waitSelect.selectOption('true');

        await page.click('button:has-text("Code view")');
        const code = (await page.locator('.cm-content').first().innerText()).trim();
        for (const expected of [
            'if (GetBoolean(player, "lit")) {',
            'player.class = ShowMenu("Your class?", Split("a;b", ";"), false)',
            'msg ("Hello")',
            'play sound ("", true, false)',
        ]) {
            if (!code.includes(expected)) throw new Error(`Expected code to contain:\n  ${expected}\ngot:\n${code}`);
        }
        console.log('PASS: mode toggles leave the code unchanged, and choosing yes stores true');

        console.log('PASS: all checks passed');
    } catch (err) {
        console.error('FAIL:', err.message);
        try {
            const pages = browser.contexts().flatMap(c => c.pages());
            if (pages[0]) await pages[0].screenshot({ path: '/tmp/appshell-field-mode-toggle-failure.png' });
        } catch { /* best-effort */ }
        throw err;
    } finally {
        await browser.close();
    }
});
