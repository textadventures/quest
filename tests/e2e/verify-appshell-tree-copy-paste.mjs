// Regression test for #2586's "no right-click menu for functions, so I can't copy/paste
// them": the tree only offered Cut/Copy/Paste for rooms/objects/pages, only via each
// row's hover "..." button, and had no keyboard shortcuts. Checks that a function can be
// copied and pasted via a right-click menu and via Ctrl/Cmd+C/V, and that Paste is only
// offered where the clipboard's element type belongs.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Tree Copy Paste Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    const tree = page.locator('.overflow-y-auto.p-1.text-xs');
    await tree.getByText('Advanced', { exact: true }).click();
    await page.getByRole('button', { name: '+ Add Function', exact: true }).click();
    await page.fill('#element-name', 'BlankOV');
    await page.getByRole('button', { name: 'Add Function', exact: true }).click();
    await tree.getByText('BlankOV', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });

    const menu = page.locator('.tree-dropdown');
    const menuLabels = async () => (await menu.locator('button').allInnerTexts()).map(s => s.trim());

    // Right-click a function: its menu, with Cut and Copy.
    await tree.getByText('BlankOV', { exact: true }).click({ button: 'right' });
    await menu.waitFor({ state: 'visible', timeout: 5000 });
    let labels = await menuLabels();
    if (!labels.includes('Cut') || !labels.includes('Copy')) throw new Error(`A function's right-click menu should offer Cut and Copy, got: ${labels.join(', ')}`);
    if (labels.includes('Paste')) throw new Error(`Nothing has been copied yet, so there should be no Paste, got: ${labels.join(', ')}`);
    await menu.getByRole('button', { name: 'Copy', exact: true }).click();
    console.log('PASS: right-clicking a function offers Cut/Copy');

    // A room isn't somewhere a function can go.
    await tree.getByText('room', { exact: true }).click({ button: 'right' });
    await menu.waitFor({ state: 'visible', timeout: 5000 });
    labels = await menuLabels();
    if (labels.includes('Paste')) throw new Error(`A copied function shouldn't be pasteable onto a room, got: ${labels.join(', ')}`);
    await page.keyboard.press('Escape');
    await menu.waitFor({ state: 'hidden', timeout: 5000 });
    console.log('PASS: a copied function is not offered for pasting onto a room, and Escape closes the menu');

    // ...but the Functions header is.
    await tree.getByText('Functions', { exact: true }).click({ button: 'right' });
    await menu.waitFor({ state: 'visible', timeout: 5000 });
    await menu.getByRole('button', { name: 'Paste', exact: true }).click();
    await tree.getByText('BlankOV1', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: pasting onto the Functions header adds a copy named BlankOV1');

    // Keyboard: select the original, copy, paste.
    await tree.getByText('BlankOV', { exact: true }).click();
    await page.keyboard.press('ControlOrMeta+c');
    await page.keyboard.press('ControlOrMeta+v');
    await tree.getByText('BlankOV2', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    if (!(await tree.getByText('BlankOV', { exact: true }).isVisible())) throw new Error('Copying should leave the original function in place');
    console.log('PASS: Ctrl/Cmd+C then Ctrl/Cmd+V on a selected function adds another copy');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
