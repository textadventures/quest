// Regression test for user-reported feedback: the "game" element's
// Attributes tab has a "Status attributes:" string-dictionary control
// rendered before the Attributes control itself — it was a separate
// fixed-size row above AttributesEditor entirely, taking dedicated space the
// same way "Inherited types" used to. PropertyEditor now passes any such
// extra controls into AttributesEditor via an `extraControls` snippet, which
// renders them inside the same shared scroll region as Inherited
// types/Attributes, instead of pinning them above it.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const landscapeCtx = await browser.newContext({ viewport: { width: 667, height: 375 }, hasTouch: true, isMobile: true });
    const page = await landscapeCtx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Game Status Attrs Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // "game" is auto-selected on load, but that programmatic selection
    // doesn't push into the mobile properties pane — tap it explicitly.
    // A plain `text=game` also matches the "Game Objects" tree header
    // (case-insensitive substring, actual DOM text is "Game Objects" —
    // only visually uppercased via CSS) and, appearing first in the DOM,
    // wins that ambiguous match — so scope to the actual tree node.
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    await page.getByRole('button', { name: /^Attributes$/ }).click();
    await page.waitForSelector('text=Status attributes', { timeout: 5000 });
    await page.waitForSelector('text=Inherited types', { timeout: 5000 });

    const statusLabel = page.locator('span:text("Status attributes:")');
    const inheritedLabel = page.locator('span:text("Inherited types")');

    const scrollParentOf = (loc) => loc.evaluate(el => {
        let p = el.parentElement;
        while (p && getComputedStyle(p).overflowY !== 'auto' && getComputedStyle(p).overflowY !== 'scroll') p = p.parentElement;
        return p ? p.className : null;
    });

    const statusScrollParent = await scrollParentOf(statusLabel);
    const inheritedScrollParent = await scrollParentOf(inheritedLabel);
    if (!statusScrollParent || statusScrollParent !== inheritedScrollParent) {
        throw new Error(`Expected "Status attributes" and "Inherited types" to share one scroll region, got "${statusScrollParent}" vs "${inheritedScrollParent}"`);
    }
    console.log('PASS: "Status attributes" shares the same scroll region as "Inherited types" / Attributes list');

    // The attributes list itself must still get real, reachable space.
    const nameRow = page.locator('[data-attr="name"]');
    const nameRowBox = await nameRow.boundingBox();
    if (!nameRowBox || nameRowBox.height < 10) throw new Error(`Attributes row should have real height, got ${JSON.stringify(nameRowBox)}`);
    console.log(`PASS: attributes list still gets real space on a short viewport (row height ${nameRowBox.height}px)`);

    await page.screenshot({ path: '/tmp/game-status-attributes.png' });

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
