// Verifies the help link shown above an element editor tab's contents, driven
// by <helpurl> in the editor definitions (src/Engine/Core/CoreEditor*.aslx).
// Only tabs with a <helpurl> get one, so the absence of the link on other tabs
// is part of the contract, not an oversight.
//
// It names the guide it opens ("Help: Exits"), with the title generated from
// the docs' own frontmatter (see site/scripts/build-docs-index.mjs) rather than
// repeated in the .aslx, so it can't drift from the page it names. It sits
// above the tab contents rather than in the tab strip, which keeps the full
// title readable at any width - titles run up to "Items that can be switched
// on and off" and the strip already scrolls for space on narrow screens.
//
// Editor definitions live in <library type="editor">, which GameSaver excludes
// from both package and editor saves - so unlike Core.aslx game libraries these
// are never frozen into a .quest file, and a <helpurl> applies to every game.
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

const helpLink = () => page.locator('a:has-text("Help:")');

// Scoped to the tab strip: other controls share tab names (the text processor's
// "Page" insert button, for one), and a tab that's already active needs no click.
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
    await page.fill('input[placeholder="Game name"]', `Tab Help ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // --- game element: the Script tab has a <helpurl>, Setup does not ---
    await page.click('text=game');
    await selectTab('Setup');
    check('no help link on a tab without a <helpurl> (game > Setup)', await helpLink().count(), 0);

    await selectTab('Scripts');
    check('help link on game > Scripts', await helpLink().getAttribute('href'),
        'https://questviva.com/howto/scripting/advanced-game-scripts/');
    check('help link names the guide it opens',
        (await helpLink().textContent()).trim(), 'Help: Advanced game scripts');

    // --- a room: Exits carries its own target ---
    await page.click('text=room');
    await selectTab('Exits');
    check('help link on room > Exits', await helpLink().getAttribute('href'),
        'https://questviva.com/howto/world/exits/');
    check('help link title tracks the tab', (await helpLink().textContent()).trim(), 'Help: Exits');

    // The link tracks the active tab rather than being fixed per element.
    await selectTab('Room');
    check('no help link after switching to a tab without one (room > Room)', await helpLink().count(), 0);

    // The gamebook editor reuses two caption keys from the Text Adventure editor,
    // so a naive curation pass pointed its Page tab at the TA dialogue-pages
    // guide. A gamebook page is Name/Page type/Picture - a different thing.
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Tab Help GB ${Date.now()}`);
    await page.waitForSelector('text=Gamebook', { timeout: 10000 });
    await page.getByText('Gamebook', { exact: true }).first().click();
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await page.click('text=Page1');
    await selectTab('Page');
    check('gamebook Page tab points at the gamebook guide, not TA dialogue pages',
        await helpLink().getAttribute('href'), 'https://questviva.com/tutorial/creating-a-gamebook/');

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-tab-help-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
