// Issue #2384: a game function named after one of the engine's own functions, such as CapFirst,
// is never called from an expression - the engine's function runs instead - and nothing used to
// say so. The function editor now shows a warning while the function returns a value and its name
// is one the engine uses. This adds a CapFirst function through the real editor and checks the
// warning appears once it has a return type, and goes again when the function is renamed.
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const WARNING = 'built-in function with this name';

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Engine Function Name Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    const tree = page.locator('.overflow-y-auto.p-1.text-xs');
    await tree.getByText('Advanced', { exact: true }).click();
    await page.getByRole('button', { name: '+ Add Function', exact: true }).click();
    await page.fill('#element-name', 'CapFirst');
    await page.getByRole('button', { name: 'Add Function', exact: true }).click();
    await tree.getByText('CapFirst', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });

    // The function editor's controls, found by their captions.
    const control = (caption, tag) => page.locator('label').filter({ hasText: new RegExp(`^${caption}:`) })
        .locator(`xpath=(.//${tag} | following::${tag})[1]`);
    const nameInput = control('Name', 'input');
    await nameInput.waitFor({ state: 'visible', timeout: 10000 });
    const warning = page.getByText(WARNING);

    // No return type yet: a statement reaches it, so there's nothing to warn about.
    await page.waitForTimeout(500);
    if (await warning.isVisible()) {
        throw new Error('Warning shown for a function with no return type');
    }
    console.log('PASS: no warning while the function has no return type');

    await control('Return type', 'select').selectOption({ label: 'String' });
    await warning.waitFor({ state: 'visible', timeout: 5000 });
    const href = await warning.getAttribute('href');
    if (!href || !href.includes('/customise/overriding/#what-cant-be-overridden')) {
        throw new Error(`Warning should link to the overriding docs, got href ${href}`);
    }
    console.log('PASS: warning shown, with its docs link, once CapFirst returns a string');

    await nameInput.fill('MyCapFirst');
    await nameInput.press('Enter');
    await tree.getByText('MyCapFirst', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });
    await warning.waitFor({ state: 'hidden', timeout: 5000 });
    console.log('PASS: warning gone after renaming the function to a name the engine does not use');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
