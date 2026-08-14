// Regression test for user-reported feedback: on a narrow properties pane,
// the Attributes tab's "Inherited types" section had a fixed size and the
// Attributes list + Assignment panel had their own independent scroll
// regions, so after selecting a row the Assignment panel could render off
// the bottom with no way to scroll back up to see the attributes list (or
// down to reach the panel) — everything past the fold was clipped by an
// ancestor's overflow-hidden instead of being reachable. Fixed by making the
// whole stacked column (Inherited types + Attributes list + Assignment
// panel + Add-attribute footer) share one scroll container below @2xl,
// instead of each sub-region trying to manage its own. Also verifies the new
// close ("X") button on the Assignment/Behaviour panel deselects the current
// attribute/verb.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Attrs Stack Scroll Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

try {
    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const page = await createDraftAndGetPage(mobileCtx);
    await page.click('text=room');
    await page.getByRole('button', { name: /^Attributes$/ }).click();
    await page.waitForSelector('text=Inherited types', { timeout: 5000 });

    // Select a row and confirm both the row itself (scrolled-to) and the
    // "Inherited types" header above it are still reachable by scrolling —
    // i.e. one shared scroll container, nothing clipped off permanently.
    await page.locator('[data-attr="name"]').click();
    await page.waitForSelector('text=Assignment', { timeout: 5000 });
    const assignmentVisible = await page.locator('text=Assignment').isVisible();
    if (!assignmentVisible) throw new Error('Assignment panel should be visible after selecting a row');
    console.log('PASS: Assignment panel visible after selecting a row');

    await page.evaluate(() => window.scrollTo(0, 0));
    const scrollParent = page.locator('div.overflow-y-auto').first();
    await scrollParent.evaluate(el => { el.scrollTop = 0; });
    const inheritedVisibleAfterScrollUp = await page.locator('text=Inherited types').isVisible();
    if (!inheritedVisibleAfterScrollUp) throw new Error('Scrolling the shared container to the top should reveal "Inherited types" again');
    console.log('PASS: scrolling back up reveals "Inherited types" — not permanently clipped');

    // Close (X) button on the Assignment panel deselects the attribute.
    await scrollParent.evaluate(el => { el.scrollTop = el.scrollHeight; });
    await page.getByRole('button', { name: 'Close', exact: true }).click();
    await page.waitForSelector('text=Select an attribute to edit it.', { timeout: 5000 });
    console.log('PASS: Assignment panel close button deselects the attribute');
    await page.screenshot({ path: '/tmp/attributes-stack-scroll-fix.png' });

    // Same close affordance on VerbsEditor's Behaviour panel.
    await page.getByRole('button', { name: 'room', exact: true }).click(); // back to tree pane
    await page.waitForSelector('text=GAME OBJECTS', { timeout: 5000 });
    await page.click('text=player');
    await page.getByRole('button', { name: /^Verbs$/ }).click();
    await page.waitForSelector('text=No verbs added yet', { timeout: 5000 });
    const verbInput = page.locator('button:has-text("Add Verb")').locator('..').locator('input');
    await verbInput.fill('smell');
    await page.click('button:has-text("Add Verb")');
    await page.waitForSelector('text=Behaviour', { timeout: 5000 });
    await page.getByRole('button', { name: 'Close', exact: true }).click();
    await page.waitForSelector('text=Select a verb to edit its behaviour.', { timeout: 5000 });
    console.log('PASS: Behaviour panel close button deselects the verb');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
