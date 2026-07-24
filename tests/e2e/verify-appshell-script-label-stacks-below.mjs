// Regression test for user-reported feedback: a control row's label and
// control render side-by-side when the label is short (<=20 chars) and
// stacked (label above, control below) when it's long — but for script
// controls specifically ("Start script:", "Script when entering a room:",
// etc.) that should always stack below regardless of label length, since a
// short label like "Start script" otherwise squeezed the script editor into
// whatever width was left after a fixed-width label column.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Script Label Stack Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Scripts$/ }).click();
    await page.waitForSelector('text=Start script', { timeout: 5000 });

    // Empty state: "+ Add script" / "Code view" buttons should be below the
    // "Start script:" label (short — 11 chars, well under the old 20-char
    // stacking threshold), not squeezed onto the same row.
    const labelBox = await page.locator('text=Start script:').boundingBox();
    const addScriptBox = await page.locator('button:has-text("Add script")').first().boundingBox();
    if (addScriptBox.y <= labelBox.y + labelBox.height - 2) {
        throw new Error(`Expected "+ Add script" below "Start script:" label, got label y=${labelBox.y} h=${labelBox.height}, button y=${addScriptBox.y}`);
    }
    console.log('PASS: empty-state "+ Add script" renders below the short "Start script:" label');

    // With actual content: a script row should also stack below, and use the
    // full row width rather than being squeezed next to a label column.
    await page.click('button:has-text("Add script")');
    await page.waitForSelector('[role="dialog"]', { timeout: 5000 });
    await page.getByRole('button', { name: 'Print', exact: true }).click();
    await page.waitForSelector('input[type="checkbox"]', { timeout: 5000 });

    const labelBox2 = await page.locator('text=Start script:').boundingBox();
    const checkboxBox = await page.locator('input[type="checkbox"]').first().boundingBox();
    if (checkboxBox.y <= labelBox2.y + labelBox2.height - 2) {
        throw new Error(`Expected the script row below "Start script:" label, got label y=${labelBox2.y} h=${labelBox2.height}, row y=${checkboxBox.y}`);
    }
    if (checkboxBox.x > labelBox2.x + 15) {
        throw new Error(`Expected the script row to start near the label's left edge (full width), not indented next to a label column — label x=${labelBox2.x}, row x=${checkboxBox.x}`);
    }
    console.log('PASS: an actual script row also stacks below the label and uses the full row width');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
