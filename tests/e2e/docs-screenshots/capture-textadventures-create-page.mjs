// Captures the editor screenshot shown in the Quest Viva panel of textadventures.co.uk's
// /create page (TextAdventures.Web/wwwroot/images/create/ in that repo), in light and dark.
// Not a docs screenshot, but it uses the same harness. The scene matches the page's little
// "what players see" demo: a dusty study with a desk, and a door north to the hallway.
//
// It ends on the desk's Verbs tab, with the "search" verb the demo's desk link offers.
// Wider and shorter than the docs captures, and at 2x, because the page shows it beside a
// Squiffy editor screenshot cropped to the same shape.
//
//   cd tests/e2e && node docs-screenshots/capture-textadventures-create-page.mjs http://localhost:5174 <outputDir>
import { chromium } from 'playwright';
import { join } from 'node:path';
import { mkdirSync } from 'node:fs';
import {
    DEFAULT_BASE_URL, createLocalDraft, selectTreeNode, addElement, openTab,
    setLabeledField, selectLabeledField, addVerb,
} from './lib.mjs';

const outDir = process.argv[3] || '/tmp/textadventures-create-page';
mkdirSync(outDir, { recursive: true });

const VIEWPORT = { width: 1120, height: 520 };

const browser = await chromium.launch();
try {
    const context = await browser.newContext({ viewport: VIEWPORT, deviceScaleFactor: 2, reducedMotion: 'reduce' });
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    const page = await context.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await createLocalDraft(page, DEFAULT_BASE_URL, 'The Old House');

    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'study');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="study"]', { timeout: 10000 });

    await addElement(page, 'Add Room', 'hallway');

    await selectTreeNode(page, 'study');
    await addElement(page, 'Add Object in "study"', 'desk');
    await openTab(page, 'Setup');
    await selectLabeledField(page, 'object description:', 'string');
    await page.locator('textarea').fill('A heavy oak desk, with a single drawer.');

    await selectTreeNode(page, 'study');
    await addElement(page, 'Add Object in "study"', 'lamp');

    await selectTreeNode(page, 'study');
    await openTab(page, 'Exits');
    await page.getByRole('button', { name: 'north', exact: true }).click();
    const combobox = page.locator('[role="combobox"]');
    await combobox.click();
    await combobox.fill('hallway');
    await page.click('[role="option"]:has-text("hallway")');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=north → hallway', { timeout: 10000 });

    await openTab(page, 'Room');
    await page.locator('textarea').first().fill(
        'You are in a dusty study. A heavy oak desk sits beneath the window, and a door leads north.'
    );
    await openTab(page, 'Setup'); // commits the description

    await selectTreeNode(page, 'desk');
    await openTab(page, 'Verbs');
    await page.waitForSelector('text=No verbs added yet', { timeout: 15000 });
    await addVerb(page, 'search');
    await page.locator('textarea').fill('You rummage through the drawer, and find a small brass key.');
    // Put the focus somewhere that draws no ring in the shot.
    await page.mouse.click(VIEWPORT.width - 5, VIEWPORT.height - 5);
    await page.waitForTimeout(1500); // autosave settles, so the title bar says Saved

    // A backup reminder card can cover part of the editor; dismiss it if it's there.
    const later = page.getByRole('button', { name: 'Remind me later', exact: true });
    if (await later.count() > 0) await later.click();

    await page.screenshot({ path: join(outDir, 'quest-viva-editor.png') });
    console.log('SAVED light');

    await page.emulateMedia({ colorScheme: 'dark' });
    await page.waitForTimeout(500);
    await page.screenshot({ path: join(outDir, 'quest-viva-editor-dark.png') });
    console.log('SAVED dark');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
