// Verifies the richtext + text-processor side panel stacks vertically in a
// narrow properties pane (container query, not just viewport) and stays
// side-by-side on a wide desktop pane.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

async function createDraftAndGetPage(context) {
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Richtext Stack Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    return page;
}

async function openRoomDescription(page) {
    // "room" -> "Room" tab has the Description richtext field with text-processor commands.
    // Exact + case-sensitive regex to avoid matching the mobile back header's
    // "← room" button, whose accessible name otherwise substring/case matches.
    await page.click('text=room');
    await page.getByRole('button', { name: /^Room$/ }).click();
    await page.waitForSelector('.richtext-wrap', { timeout: 5000 });
}

try {
    const desktopCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const desktopPage = await createDraftAndGetPage(desktopCtx);
    await openRoomDescription(desktopPage);
    const desktopFlexDir = await desktopPage.locator('.richtext-wrap').first().evaluate(el => getComputedStyle(el).flexDirection);
    if (desktopFlexDir !== 'row') throw new Error(`Expected wide properties pane to keep richtext-wrap as row, got ${desktopFlexDir}`);
    console.log('PASS: wide properties pane keeps richtext + text-processor side by side (row)');
    await desktopPage.screenshot({ path: '/tmp/richtext-wide.png' });
    await desktopCtx.close();

    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const mobilePage = await createDraftAndGetPage(mobileCtx);
    await mobilePage.click('text=room');
    await mobilePage.getByRole('button', { name: /^Room$/ }).waitFor({ timeout: 5000 });
    await mobilePage.getByRole('button', { name: /^Room$/ }).click();
    await mobilePage.waitForSelector('.richtext-wrap', { timeout: 5000 });
    const mobileFlexDir = await mobilePage.locator('.richtext-wrap').first().evaluate(el => getComputedStyle(el).flexDirection);
    if (mobileFlexDir !== 'column') throw new Error(`Expected narrow properties pane to stack richtext-wrap as column, got ${mobileFlexDir}`);
    console.log('PASS: narrow properties pane stacks richtext + text-processor (column)');
    await mobilePage.screenshot({ path: '/tmp/richtext-narrow.png' });
    await mobileCtx.close();

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
