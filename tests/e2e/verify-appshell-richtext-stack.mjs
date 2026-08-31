// Verifies the richtext field's text-processor toolbar (icon buttons + grouped Insert
// menu, PropertyEditor.svelte's textProcessorPanel snippet) renders above the textarea and
// doesn't overflow the properties pane on a narrow mobile viewport.
//
// This used to check a side-by-side "richtext-wrap" panel that switched between row (desktop)
// and column (mobile) via a container query — PR #1967 ("redesign rich text toolbar as icons +
// grouped Insert menu") removed that layout entirely in favor of a fixed icon toolbar always
// rendered above the textarea (see PropertyEditor.svelte's textProcessorTextareaId / the
// `flex flex-col gap-1 w-full` wrapper), so there's no row/column distinction left to assert on.
import { chromium } from './lib/tracked-chromium.mjs';

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
    await page.getByRole('button', { name: 'Bold' }).waitFor({ timeout: 5000 });
}

try {
    const desktopCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const desktopPage = await createDraftAndGetPage(desktopCtx);
    await openRoomDescription(desktopPage);
    const desktopOverflow = await desktopPage.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
    if (desktopOverflow > 1) throw new Error(`Desktop pane has horizontal overflow of ${desktopOverflow}px with the richtext toolbar visible`);
    console.log('PASS: wide properties pane renders the richtext toolbar with no horizontal overflow');
    await desktopPage.screenshot({ path: '/tmp/richtext-wide.png' });
    await desktopCtx.close();

    const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 }, hasTouch: true });
    const mobilePage = await createDraftAndGetPage(mobileCtx);
    await mobilePage.click('text=room');
    await mobilePage.getByRole('button', { name: /^Room$/ }).waitFor({ timeout: 5000 });
    await mobilePage.getByRole('button', { name: /^Room$/ }).click();
    await mobilePage.getByRole('button', { name: 'Bold' }).waitFor({ timeout: 5000 });
    const mobileOverflow = await mobilePage.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
    if (mobileOverflow > 1) throw new Error(`Mobile pane has horizontal overflow of ${mobileOverflow}px with the richtext toolbar visible`);
    console.log('PASS: narrow properties pane renders the richtext toolbar with no horizontal overflow');
    // Insert dropdown and help link must stay reachable, not clipped off-screen.
    await mobilePage.getByRole('button', { name: 'Insert' }).waitFor({ state: 'visible', timeout: 5000 });
    await mobilePage.getByRole('link', { name: 'Text Processor help' }).waitFor({ state: 'visible', timeout: 5000 });
    console.log('PASS: Insert menu and help link stay visible on a narrow viewport');
    await mobilePage.screenshot({ path: '/tmp/richtext-narrow.png' });
    await mobileCtx.close();

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
