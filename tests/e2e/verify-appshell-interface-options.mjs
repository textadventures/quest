// Checks the game's Interface tab options for the 610 UI contract (#2650 PR 3): the layout
// presets, the author hook settings and the classic interface. Each is set in the editor and
// checked in the game's raw XML.
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npx vite dev --port 5174   (in src/AppShell)
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    const errors = [];
    page.on('pageerror', (err) => errors.push(err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Interface Options Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    const openInterfaceTab = async () => {
        await page.locator('[data-value="game"][data-part="branch-control"]').click();
        await page.getByRole('button', { name: 'Interface', exact: true }).click();
    };
    const rawXml = async () => {
        await page.click('button[title="Raw XML code view"]');
        await page.waitForSelector('.cm-editor', { timeout: 5000 });
        const text = await page.locator('.cm-content').first().innerText();
        await page.click('button[title="Raw XML code view"]');
        return text;
    };
    const settle = () => page.evaluate(() => { if (document.activeElement instanceof HTMLElement) document.activeElement.blur(); })
        .then(() => page.waitForTimeout(300));

    await openInterfaceTab();
    // Each dropdown sits inside its label, so its accessible name includes its options
    const dropdown = (caption) => page.locator('label', { hasText: new RegExp(`^\s*${caption}`) }).locator('select');
    const layout = dropdown('Where the panes go');
    const narrow = dropdown('On a phone');
    await layout.waitFor({ timeout: 5000 }).catch(() => { throw new Error('the Interface tab has no "Where the panes go" option'); });
    const layoutShown = await layout.evaluate((s) => s.selectedOptions[0]?.textContent.trim());
    const narrowShown = await narrow.evaluate((s) => s.selectedOptions[0]?.textContent.trim());
    if (layoutShown !== 'Beside the game (in a drawer on a phone)' || narrowShown !== 'Same as above') {
        throw new Error(`a new game's layout options show "${layoutShown}" and "${narrowShown}"`);
    }
    console.log('PASS: a new game shows the default layout options');

    const scroll = page.getByLabel('Scroll to new text as it appears', { exact: true });
    if (!(await scroll.isChecked())) throw new Error('"Scroll to new text as it appears" is unticked for a new game');
    if (await page.getByLabel('A click anywhere continues after a wait', { exact: true }).isChecked()) {
        throw new Error('"A click anywhere continues after a wait" is ticked for a new game');
    }
    console.log('PASS: a new game scrolls to new text and doesn\'t continue on a click');

    await layout.selectOption({ label: 'In a sheet opened from beside the command bar' });
    await settle();
    await narrow.selectOption({ label: 'No panes' });
    await settle();
    await page.locator('label', { hasText: /^\s*Hint shown in the empty command bar/ }).locator('input').fill('What now?');
    await settle();
    await scroll.uncheck();
    await settle();
    await page.getByLabel('Fade the text from earlier turns', { exact: true }).check();
    await settle();

    let xml = await rawXml();
    for (const expected of ['<layoutpreset>bottombar</layoutpreset>', '<narrowlayoutpreset>nopanes</narrowlayoutpreset>',
        '<commandplaceholder>What now?</commandplaceholder>', '<autoscroll type="boolean">false</autoscroll>', '<fadepreviousoutput />']) {
        if (!xml.includes(expected)) throw new Error(`the game XML has no ${expected}`);
    }
    console.log('PASS: the layout presets, the command bar hint and the output options are saved on game');

    await openInterfaceTab();
    const summary = page.locator('summary', { hasText: 'Advanced' });
    if (!(await summary.evaluate((el) => el.closest('details')?.open))) await summary.click();
    await page.getByLabel('Use the classic interface', { exact: true }).check();
    await settle();
    await layout.selectOption({ label: 'Beside the game (in a drawer on a phone)' });
    await settle();
    xml = await rawXml();
    if (!xml.includes('<classicinterface />')) throw new Error('the game XML has no <classicinterface />');
    if (xml.includes('<layoutpreset>bottombar')) throw new Error('choosing the default layout again left bottombar saved');
    console.log('PASS: "Use the classic interface" is saved, and the default layout replaces bottombar');

    if (errors.length) throw new Error(`page errors:\n  ${errors.join('\n  ')}`);
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
