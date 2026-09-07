// Regenerates the 2 editor screenshots embedded in
// site/src/content/docs/howto/tasks/shop.md. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, openTab, toggleFeature,
    addAdvancedElement, setScriptCodeView, addScriptCommand, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    // --- SetUpShop.png: a Function, params "shop"/"stock" (in that order), no return type ---
    await addAdvancedElement(page, 'Function', 'SetUpShop');
    const paramInput = page.locator('input[placeholder^="Add parameter name"]');
    await paramInput.fill('shop');
    await paramInput.locator('xpath=following-sibling::button[1]').click();
    await page.waitForSelector('text=shop', { timeout: 5000 });
    await paramInput.fill('stock');
    await paramInput.locator('xpath=following-sibling::button[1]').click();
    await page.waitForSelector('text=stock', { timeout: 5000 });

    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `shop.stock = stock
foreach (o, GetDirectChildren(stock)) {
  SetUpMerchandise (o)
}`);
    await page.waitForSelector('text=For each: loop variable', { timeout: 5000 });
    await capture(page, out('SetUpShop.png'), { untilLocator: page.locator('button:has-text("+ Add script")').last(), padding: 40 });

    // --- StartShop.png: game's Start script, a single SetUpShop(Cake Shop, Cake Shop Stock)
    // call - built through the picker (not Code view) so that SetUpShop, being a real function
    // already defined in this same draft, gets recognised and rendered with named shop/stock
    // fields rather than the generic +param form ---
    await selectTreeNode(page, 'game');
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    await page.waitForSelector('text=Start script:', { timeout: 10000 });
    const startAddBtn = page.getByText('Start script:', { exact: true }).locator('xpath=following::button[contains(.,"+ Add script")][1]');
    await addScriptCommand(page, startAddBtn, { category: 'Scripts', item: 'Call function' });
    const callFnInput = page.locator('xpath=//*[contains(text(), "Call function")]/following::input[@type="text"][1]');
    await callFnInput.fill('SetUpShop');
    await page.waitForSelector('text=SetUpShop', { timeout: 5000 });
    // Select the autocomplete suggestion (not just typing the name) so the generic +param UI
    // swaps to named shop/stock fields matching the function's real signature.
    const suggestion = page.getByRole('option', { name: 'SetUpShop', exact: true });
    if (await suggestion.count() > 0) {
        await suggestion.click();
    }
    await page.waitForSelector('text=shop:', { timeout: 5000 });
    const shopInput = page.getByText('shop:', { exact: true }).locator('xpath=following::input[1]');
    await shopInput.fill('Cake Shop');
    const stockInput = page.getByText('stock:', { exact: true }).locator('xpath=following::input[1]');
    await stockInput.fill('Cake Shop Stock');
    // Narrow field shows the fill()'d text's tail, not its start - scroll back to the beginning
    // before capturing (see docs-screenshots skill's known gotcha).
    await stockInput.evaluate(el => { el.scrollLeft = 0; });
    await capture(page, out('StartShop.png'), { untilLocator: stockInput, padding: 40 });
});
