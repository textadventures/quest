// Verifies the Setup-tab Version Code field (<versioncode>): number input on the
// same horizontal row as Version, default 1 from defaultgame, and
// persisted as an int in the game XML.
//
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5180';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1400, height: 900 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

function check(label, actual, expected) {
    if (actual !== expected) throw new Error(`[${label}] got ${JSON.stringify(actual)}, expected ${JSON.stringify(expected)}`);
    console.log(`PASS: ${label}`);
}

const tabStrip = () => page.locator('.flex.border-b.border-surface-200-800.overflow-x-auto.flex-shrink-0');

async function selectTab(name) {
    const tab = tabStrip().getByRole('button', { name: new RegExp(`^${name}$`) });
    await tab.waitFor({ state: 'visible', timeout: 10000 });
    await tab.click();
    await page.waitForTimeout(150);
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Version Code ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 60000 });

    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await selectTab('Setup');

    const versionCaption = page.locator('span', { hasText: /^Version:$/ });
    const versionCodeCaption = page.locator('span', { hasText: /^Version Code:$/ });
    await versionCaption.waitFor({ state: 'visible', timeout: 10000 });
    await versionCodeCaption.waitFor({ state: 'visible', timeout: 10000 });

    const sharedParent = await versionCaption.evaluate((el) => {
        const versionOuter = el.closest('.flex.flex-col.gap-1');
        const codeOuter = [...document.querySelectorAll('span')]
            .find(s => /^Version Code:$/.test(s.textContent.trim()))
            ?.closest('.flex.flex-col.gap-1');
        return !!(versionOuter && codeOuter && versionOuter.parentElement === codeOuter.parentElement
            && [...(versionOuter.parentElement?.classList ?? [])].some(c => c === 'flex'));
    });
    check('Version and Version Code share a horizontal flex row', sharedParent, true);

    const versionCodeField = versionCodeCaption.locator('xpath=ancestor::label[1]//input[@type="number"]');
    check('Version Code uses a number input', await versionCodeField.count(), 1);
    check('Version Code min is 0', await versionCodeField.getAttribute('min'), '0');
    check('default Version Code is 1', await versionCodeField.inputValue(), '1');

    await versionCodeField.fill('42');
    await versionCodeField.dispatchEvent('change');
    await page.waitForTimeout(300);

    await page.click('button[title="Raw XML code view"]');
    await page.waitForSelector('.cm-editor', { timeout: 5000 });
    const xmlText = await page.locator('.cm-content').first().innerText();
    if (!xmlText.includes('<versioncode type="int">42</versioncode>')) {
        throw new Error(`Expected <versioncode type="int">42</versioncode> in raw XML; got:\n${xmlText.slice(0, 800)}`);
    }
    console.log('PASS: versioncode 42 persisted in game XML');

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-versioncode-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
