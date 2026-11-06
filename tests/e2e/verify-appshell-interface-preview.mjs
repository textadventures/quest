// Checks the preview on the game's Interface tab (InterfacePreview.svelte): a miniature of the v2
// player on a wide screen and on a phone, in the game's colours, with the parts the tab turns on
// where the layout puts them. A new game has its panes beside the game (inventory, places and
// objects, compass) and in a drawer on a phone, with the exits strip; the pane order, hidden and
// added panes, the bottom bar and no-panes layouts, the phone's own layout, the location bar,
// command bar, picture frame and map (above the game or in the panes) all follow; it takes the
// page colour from the Appearance tab; clicking a part goes to its option; and the classic
// interface has none.
// With SCREENSHOT_DIR set, it saves pictures of the preview along the way.
//
// Requires the AppShell dev server (5174).
import { chromium } from './lib/tracked-chromium.mjs';
import { join } from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const screenshotDir = process.env.SCREENSHOT_DIR;

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    const errors = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Interface Preview Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await page.locator('[data-value="game"][data-part="branch-control"]').click();
    const openTab = (name) => page.getByRole('button', { name, exact: true }).click();
    await openTab('Interface');

    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));
    const preview = page.locator('[data-interface-preview]');
    const checkbox = (caption) => page.getByLabel(caption, { exact: true });
    const dropdown = (caption) => page.locator('label', { hasText: new RegExp(`^\\s*${caption}`) }).locator('select');
    const shot = async (name) => {
        if (screenshotDir) await preview.screenshot({ path: join(screenshotDir, `interface-preview-${name}.png`) });
    };
    // What each screen shows: its parts, its panes in order, and its room name
    const screens = () => preview.locator('[data-ip-screen]').evaluateAll(figs => Object.fromEntries(figs.map(fig => [
        fig.dataset.ipScreen,
        {
            parts: [...fig.querySelectorAll('[data-ip-part]')].map(el => el.dataset.ipPart),
            panes: [...fig.querySelectorAll('[data-ip-pane]')].map(el => el.dataset.ipPane),
            room: fig.querySelector('.ip-room')?.textContent.trim() ?? null,
            background: getComputedStyle(fig.querySelector('.ip-column')).backgroundColor,
        },
    ])));
    const expectEqual = (what, actual, expected) => {
        const a = JSON.stringify(actual), e = JSON.stringify(expected);
        if (a !== e) throw new Error(`${what} is ${a}, not ${e}`);
    };

    await preview.waitFor({ timeout: 5000 }).catch(() => { throw new Error('the Interface tab has no preview'); });
    let s = await screens();
    expectEqual('a new game\'s wide screen', s.wide, {
        parts: ['location', 'command', 'panes'], panes: ['inventory', 'objects', 'compass'], room: 'Study', background: 'rgb(255, 255, 255)',
    });
    expectEqual('a new game\'s phone', s.phone, {
        parts: ['location', 'drawer', 'exits', 'command'], panes: [], room: 'Study', background: 'rgb(255, 255, 255)',
    });
    const label = await preview.getAttribute('aria-label');
    if (!label?.includes('beside the game') || !label.includes('Inventory, Places and objects, Compass') || !label.includes('drawer')) {
        throw new Error(`the preview's description is ${JSON.stringify(label)}`);
    }
    console.log('PASS: a new game has its panes beside the game, and in a drawer with the exits strip on a phone, and says so');
    await shot('new-game');

    // Clicking a part goes to its option: focused, scrolled to and briefly highlighted
    const focusedAfterClicking = async (selector) => {
        await preview.locator(selector).first().click();
        await page.waitForTimeout(400);
        return page.evaluate(() => {
            const el = document.activeElement;
            const row = el?.closest('[data-control-attribute]');
            return { attribute: row?.getAttribute('data-control-attribute') ?? null, highlighted: !!row?.classList.contains('control-highlight') };
        });
    };
    for (const [selector, attribute] of [
        ['[data-ip-screen="wide"] [data-ip-pane="inventory"]', 'turnoffinventory'],
        ['[data-ip-screen="wide"] [data-ip-part="location"]', 'showlocation'],
        ['[data-ip-screen="phone"] [data-ip-part="drawer"]', 'narrowlayoutpreset'],
        ['[data-ip-screen="wide"] [data-ip-part="command"]', 'showcommandbar'],
    ]) {
        const focused = await focusedAfterClicking(selector);
        expectEqual(`what clicking ${selector} focuses`, focused, { attribute, highlighted: true });
    }
    console.log('PASS: clicking a part of the preview goes to its option, highlighted');

    // The pane order, a hidden pane and an added one
    await checkbox('Put status and compass above the inventory').check();
    await settle();
    await checkbox('Hide the inventory').check();
    await settle();
    await checkbox('Show a command pane').check();
    await settle();
    s = await screens();
    expectEqual('the panes, reordered, with the inventory hidden and a command pane', s.wide.panes, ['commands', 'compass', 'objects']);
    console.log('PASS: the panes follow the order, and hiding and adding panes');

    // The bottom bar on both, then no panes on a phone
    await dropdown('Where the panes go').selectOption({ label: 'In a sheet opened from beside the command bar' });
    await settle();
    s = await screens();
    expectEqual('the wide screen with the bottom bar', s.wide.parts, ['location', 'command', 'sheet']);
    expectEqual('the phone with the bottom bar', s.phone.parts, ['location', 'exits', 'command', 'sheet']);
    await dropdown('On a phone').selectOption({ label: 'No panes' });
    await settle();
    s = await screens();
    expectEqual('the phone with no panes', s.phone.parts, ['location', 'command']);
    expectEqual('the wide screen, still the bottom bar', s.wide.parts, ['location', 'command', 'sheet']);
    console.log('PASS: the bottom bar shows its sheet button on both, and a phone can have no panes on its own');
    await shot('bottombar');
    await dropdown('Where the panes go').selectOption({ index: 0 });
    await settle();
    await dropdown('On a phone').selectOption({ index: 0 });
    await settle();

    // The location bar's room name, the command bar, the picture frame
    await checkbox('Show location bar').uncheck();
    await settle();
    await checkbox('Show command bar').uncheck();
    await settle();
    await checkbox('Show a picture frame above the text').check();
    await settle();
    s = await screens();
    expectEqual('the room name without the location bar', s.wide.room, '');
    if (s.wide.parts.includes('command') || s.phone.parts.includes('command')) throw new Error('the command bar still shows after turning it off');
    if (!s.wide.parts.includes('picture') || !s.phone.parts.includes('picture')) throw new Error(`the picture frame doesn't show: ${JSON.stringify(s)}`);
    console.log('PASS: the room name, command bar and picture frame follow');
    await checkbox('Show location bar').check();
    await settle();
    await checkbox('Show command bar').check();
    await settle();

    // A map: above the game, then the first pane, which on a phone with no panes is above the game
    await checkbox('Show a map that draws itself as the player explores').check();
    await settle();
    s = await screens();
    if (!s.wide.parts.includes('map') || s.wide.panes.includes('map')) throw new Error(`the map isn't above the game: ${JSON.stringify(s.wide)}`);
    await dropdown('Map position').selectOption({ label: 'In the panes' });
    await settle();
    s = await screens();
    expectEqual('the panes with the map in them', s.wide.panes, ['map', 'commands', 'compass', 'objects']);
    if (s.wide.parts.includes('map')) throw new Error('the map is above the game as well as in the panes');
    await dropdown('On a phone').selectOption({ label: 'No panes' });
    await settle();
    s = await screens();
    if (!s.phone.parts.includes('map')) throw new Error(`on a phone with no panes, the map isn't above the game: ${JSON.stringify(s.phone)}`);
    console.log('PASS: the map is above the game, or the first pane, and above the game on a phone with no panes');
    await shot('map-in-panes');

    // No panes at all
    await checkbox('Show panes (inventory, places and objects, compass)').uncheck();
    await settle();
    s = await screens();
    if (s.wide.parts.includes('panes') || s.wide.parts.includes('sheet') || s.phone.parts.includes('drawer') || s.phone.parts.includes('exits')) {
        throw new Error(`with the panes turned off, the preview still has them: ${JSON.stringify(s)}`);
    }
    console.log('PASS: turning the panes off leaves none, on either screen');
    await checkbox('Show panes (inventory, places and objects, compass)').check();
    await settle();

    // The game's colours, from the Appearance tab
    await openTab('Appearance');
    await page.locator('[data-colour-grid="defaultbackground"]').getByRole('combobox', { name: 'Page colour', exact: true }).fill('#f2f1ee');
    await settle();
    await openTab('Interface');
    await preview.waitFor({ timeout: 5000 });
    s = await screens();
    expectEqual('the page in the game\'s colour', s.wide.background, 'rgb(242, 241, 238)');
    console.log('PASS: the preview is in the game\'s colours');
    await shot('own-colours');

    // The classic interface: no preview
    const summary = page.locator('summary', { hasText: 'Advanced' });
    if (!(await summary.evaluate(el => el.closest('details')?.open))) await summary.click();
    await checkbox('Use the classic interface').check();
    await settle();
    await page.waitForTimeout(300);
    if (await preview.count() !== 0) throw new Error('the classic interface still has the preview');
    console.log('PASS: the classic interface has no preview');

    if (errors.length) throw new Error(`page errors: ${errors.join('; ')}`);
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
