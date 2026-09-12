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

    // --- game element ---
    await page.click('text=game');
    await selectTab('Setup');
    check('help link on game > Setup', await helpLink().getAttribute('href'),
        'https://questviva.com/publishing/game-details/');

    // game > Scripts is start/room-enter/turn scripts. "Advanced game scripts"
    // is about inituserinterface and friends, which live on the Advanced Scripts
    // tab beside it - so this one is anchored at its own section instead.
    await selectTab('Scripts');
    check('help link on game > Scripts is anchored at its section',
        await helpLink().getAttribute('href'),
        'https://questviva.com/howto/scripting/when-scripts-run/#the-game-scripts-tab');
    check('game > Scripts help link names the section',
        (await helpLink().textContent()).trim(), 'Help: The game Scripts tab');

    // /howto/ux/ui-style/ has a section per tab, so the link is anchored and
    // names the section rather than the whole page.
    await selectTab('Display');
    check('help link on game > Display is anchored at its section',
        await helpLink().getAttribute('href'), 'https://questviva.com/howto/ux/ui-style/#the-display-tab');
    check('anchored help link names the section, not the page',
        (await helpLink().textContent()).trim(), 'Help: The display tab');

    // --- a room: Exits carries its own target ---
    await page.click('text=room');
    await selectTab('Exits');
    check('help link on room > Exits', await helpLink().getAttribute('href'),
        'https://questviva.com/howto/world/exits/');
    check('help link title tracks the tab', (await helpLink().textContent()).trim(), 'Help: Exits');

    // Attributes is anchored at the section about that tab, so the reader
    // doesn't land on the tutorial page's unrelated opening example.
    await selectTab('Attributes');
    check('Attributes tab is anchored at its section',
        await helpLink().getAttribute('href'),
        'https://questviva.com/tutorial/custom-attributes/#the-attributes-tab');

    // room > Scripts holds before/after-entering and turn scripts - a different
    // set from the game object's, so it gets its own section of the same guide.
    await selectTab('Scripts');
    check('help link on room > Scripts is anchored at its section',
        await helpLink().getAttribute('href'),
        'https://questviva.com/howto/scripting/when-scripts-run/#the-room-scripts-tab');

    // The link tracks the active tab rather than being fixed per element.
    await selectTab('Room');
    check('help link follows the active tab (room > Room)', await helpLink().getAttribute('href'),
        'https://questviva.com/howto/world/objects-and-rooms/#the-room-tab');

    // Absence is still part of the contract: every tab of the object and game
    // editors now carries a <helpurl>, but the Function editor has none, so its
    // one tab shows no link at all.
    const tree = page.locator('.overflow-y-auto.p-1.text-xs');
    await tree.getByText('Advanced', { exact: true }).click();
    await page.getByRole('button', { name: '+ Add Function', exact: true }).click();
    await page.fill('#element-name', 'HelpLinkTestFunction');
    await page.getByRole('button', { name: 'Add Function', exact: true }).click();
    await page.waitForTimeout(300);
    check('no help link on a tab without a <helpurl> (Function editor)', await helpLink().count(), 0);

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
