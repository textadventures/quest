// Regression test for issue #1734: in the editor, "Make editable copy" on an
// inherited script attribute whose body contains a function call (defaultobject's
// `changedparent`, which calls OnEnterRoom / MergePOVCoordinates) needed *two*
// Undo clicks to revert, while an inherited script without one (`changedisopen`)
// needed only one.
//
// Cause was engine-side: FunctionCallParameters attached its parameter QuestList
// to the undo logger before populating it, so cloning such a script inside the
// "Make editable copy" transaction logged a spurious undo action per parameter.
// Undoing those raised FunctionCallParametersUpdated on a script with no editor
// wrapper subscribed, throwing a NullReferenceException part-way through the
// undo — after the model had already reverted, but before WasmEditorBridge.Undo
// returned, so editor-store's undo() never ran its post-undo refresh and the UI
// carried on showing the attribute as an owned copy.
//
// This drives the real UI: make the copy, click Undo once, and assert the
// "This script is inherited — read-only." banner (with its "Make editable copy"
// button) is back for both attributes.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
    const page = await context.newPage();
    const pageErrors = [];
    page.on('pageerror', err => { pageErrors.push(err.message); console.log('[pageerror]', err.message); });
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Make Editable Undo Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.click('text=room');
    await page.getByRole('button', { name: /^Attributes$/ }).click();
    await page.waitForSelector('text=Inherited types', { timeout: 5000 });

    const inheritedBanner = page.getByText('This script is inherited', { exact: false });
    const makeEditable = page.getByRole('button', { name: 'Make editable copy' });
    const undoButton = page.getByRole('button', { name: 'Undo' });

    // changedisopen is the control (no function call in its body, was never affected);
    // changedparent is the reported case.
    for (const attribute of ['changedisopen', 'changedparent']) {
        const row = page.locator(`[data-attr="${attribute}"]`);
        await row.scrollIntoViewIfNeeded();
        await row.click();

        if (!await inheritedBanner.isVisible()) {
            throw new Error(`'${attribute}': expected the inherited/read-only banner before making a copy`);
        }

        await makeEditable.click();
        await page.waitForTimeout(200);
        if (await inheritedBanner.isVisible()) {
            throw new Error(`'${attribute}': banner should be gone once an editable copy has been made`);
        }
        console.log(`PASS: '${attribute}' became an editable copy`);

        const errorsBefore = pageErrors.length;
        if (await undoButton.isDisabled()) {
            throw new Error(`'${attribute}': Undo button unexpectedly disabled after making the copy`);
        }
        await undoButton.click();
        await page.waitForTimeout(300);

        if (pageErrors.length > errorsBefore) {
            throw new Error(`'${attribute}': Undo raised an uncaught error: ${pageErrors.slice(errorsBefore).join('; ')}`);
        }
        if (!await inheritedBanner.isVisible()) {
            throw new Error(`'${attribute}': a single Undo should have restored the inherited/read-only state`);
        }
        console.log(`PASS: '${attribute}' reverted to inherited after exactly one Undo`);
    }

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
