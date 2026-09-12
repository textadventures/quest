// Regression test for issue #2258: the Attributes editor's "add type" list was offering Core's
// built-in default types (defaultobject, defaultexit, defaultcommand, defaultverb, defaultgame),
// which the engine already applies implicitly to every element of the matching kind - adding one
// explicitly is meaningless. WasmEditorBridge.GetTypeNames now filters these out via
// EditorController.GetAddableTypeNames (also covered by an EditorCoreTests unit test).
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const defaultTypeNames = ['defaultobject', 'defaultexit', 'defaultcommand', 'defaultverb', 'defaultgame'];

const browser = await chromium.launch();

try {
    const context = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Add Type Defaults Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.click('text=room');
    await page.getByRole('button', { name: /^Attributes$/ }).click();
    await page.waitForSelector('text=Inherited types', { timeout: 5000 });

    const addTypeSelect = page.locator('select').filter({ has: page.locator('option', { hasText: 'Add type…' }) });
    await addTypeSelect.waitFor({ timeout: 5000 });
    const optionValues = await addTypeSelect.locator('option').evaluateAll(opts => opts.map(o => o.value));

    for (const defaultTypeName of defaultTypeNames) {
        if (optionValues.includes(defaultTypeName)) {
            throw new Error(`"add type" list should not offer built-in default type "${defaultTypeName}", got: ${optionValues.join(', ')}`);
        }
    }
    console.log(`PASS: "add type" list excludes Core default types (offered: ${optionValues.filter(v => v).join(', ')})`);

    await context.close();
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
