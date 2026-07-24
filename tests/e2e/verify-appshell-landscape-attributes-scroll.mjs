// Regression test for user-reported feedback: on an iPhone in landscape
// (short viewport height), the "Inherited types" section had its own fixed/
// dedicated space above the attributes list, which on a short viewport left
// almost no room for the attributes list itself. Fixed by having Inherited
// types and the Attributes list share one scroll region (flowing/scrolling
// together, each taking only the space its own content needs) instead of
// Inherited types reserving space regardless of how much content it has.
// Also checks the AppBar's mobile padding was trimmed.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    // iPhone SE/8-ish landscape: short and wide.
    const landscapeCtx = await browser.newContext({ viewport: { width: 667, height: 375 }, hasTouch: true, isMobile: true });
    const page = await landscapeCtx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Landscape Attrs Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // AppBar padding-block should be trimmed below md (667px is still < 768px).
    const appBarPaddingBlock = await page.locator('[data-scope="app-bar"][data-part="root"]').evaluate(el => getComputedStyle(el).paddingBlockStart);
    if (appBarPaddingBlock !== '8px') throw new Error(`Expected AppBar padding-block-start to be trimmed to 8px (0.5rem) below md, got ${appBarPaddingBlock}`);
    console.log(`PASS: AppBar vertical padding trimmed on narrow viewports (${appBarPaddingBlock})`);

    await page.click('text=room');
    await page.getByRole('button', { name: /^Attributes$/ }).click();
    await page.waitForSelector('text=Inherited types', { timeout: 5000 });

    // Both "Inherited types" and "Attributes" section labels, plus at least
    // one attribute row, should be simultaneously reachable within the same
    // scrollable ancestor — not "Inherited types" hogging a fixed block that
    // pushes Attributes rows out of any visible/scrollable space.
    const inheritedLabel = page.locator('span:text("Inherited types")');
    const attributesLabel = page.locator('span:text("Attributes")');
    const nameRow = page.locator('[data-attr="name"]');

    const inheritedScrollParent = await inheritedLabel.evaluate(el => {
        let p = el.parentElement;
        while (p && getComputedStyle(p).overflowY !== 'auto' && getComputedStyle(p).overflowY !== 'scroll') p = p.parentElement;
        return p ? p.className : null;
    });
    const attributesScrollParent = await attributesLabel.evaluate(el => {
        let p = el.parentElement;
        while (p && getComputedStyle(p).overflowY !== 'auto' && getComputedStyle(p).overflowY !== 'scroll') p = p.parentElement;
        return p ? p.className : null;
    });
    if (!inheritedScrollParent || inheritedScrollParent !== attributesScrollParent) {
        throw new Error(`Expected "Inherited types" and "Attributes" to share the same scrolling ancestor, got "${inheritedScrollParent}" vs "${attributesScrollParent}"`);
    }
    console.log('PASS: "Inherited types" and "Attributes" share one scroll region');

    // The attributes row itself must actually be reachable (nonzero space),
    // not squeezed to ~0px by "Inherited types" hogging fixed space above it.
    const nameRowBox = await nameRow.boundingBox();
    if (!nameRowBox || nameRowBox.height < 10) throw new Error(`Attributes row should have real height, got ${JSON.stringify(nameRowBox)}`);
    console.log(`PASS: attributes list has real allocated space in landscape (row height ${nameRowBox?.height}px)`);

    await page.screenshot({ path: '/tmp/attributes-landscape.png' });

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
