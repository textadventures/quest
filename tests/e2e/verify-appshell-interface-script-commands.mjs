// Checks the script editor's Interface category (CoreEditorScriptsInterface.aslx, #2650 PR 2):
//   - it lists a command for each author hook function, with the map one only once the game
//     has a map
//   - adding "Show/hide part of the interface" puts it in the script with its own editor (a
//     part box with the parts as suggestions, and a yes/no choice), not the generic function
//     call one
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npx vite dev --port 5174   (in src/AppShell)
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const expected = [
    'Show/hide part of the interface',
    'Add a pane',
    'Set the content of a pane',
    'Set the command pane',
    'Set the command bar hint',
    'Scroll to new output',
    'Continue on a click anywhere',
    'Fade earlier output',
];
const mapCommand = 'Map can be dragged and zoomed';

const browser = await chromium.launch();

try {
    const page = await browser.newPage({ viewport: { width: 1400, height: 900 } });
    const errors = [];
    page.on('pageerror', (err) => errors.push(err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Interface Script Commands ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });
    await page.locator('[data-value="game"][data-part="branch-control"]').click();

    async function interfaceCommands() {
        await page.getByText('+ Add script', { exact: false }).first().click();
        const sidebar = page.locator('[role="listbox"][aria-label="Script command categories"]');
        await sidebar.getByText('Interface', { exact: true }).click({ timeout: 5000 })
            .catch(() => { throw new Error('the script adder has no Interface category'); });
        const list = page.locator('[role="listbox"][aria-label^="Script commands in Interface"]');
        await list.locator('button').first().waitFor({ timeout: 5000 });
        return { list, labels: (await list.locator('button').allTextContents()).map((s) => s.replace(/^[^A-Za-z]+/, '').trim()) };
    }

    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    let { labels } = await interfaceCommands();
    for (const label of expected) {
        if (!labels.includes(label)) throw new Error(`the Interface category has no "${label}", got: ${labels.join(', ')}`);
    }
    if (labels.includes(mapCommand)) throw new Error(`"${mapCommand}" shows for a game with no map`);
    console.log(`PASS: the Interface category lists the ${expected.length} hook commands, and no map command without a map`);
    await page.keyboard.press('Escape');
    await page.getByRole('dialog').waitFor({ state: 'hidden', timeout: 3000 });

    await page.getByRole('button', { name: 'Interface', exact: true }).click();
    await page.getByText('Map and Drawing Grid', { exact: false }).click();
    await page.getByRole('button', { name: 'Scripts', exact: true }).click();
    let list;
    ({ list, labels } = await interfaceCommands());
    if (!labels.includes(mapCommand)) throw new Error(`"${mapCommand}" is missing once the game has a map`);
    console.log('PASS: the map command shows once the game has a map');

    const selectsBefore = await page.locator('select').count();
    await list.locator('button', { hasText: 'Show/hide part of the interface' }).click();
    await page.getByRole('button', { name: 'OK', exact: true }).click();
    await page.getByRole('dialog').waitFor({ state: 'hidden', timeout: 3000 });
    await page.waitForFunction((before) => document.querySelectorAll('select').length >= before + 2, selectsBefore, { timeout: 5000 })
        .catch(() => { throw new Error('adding Show/hide gave no part dropdown and yes/no choice'); });
    // The part is a text box with the parts as suggestions (<freetext/>), so an author pane's
    // ID can be typed too; the yes/no choice is a select.
    const part = await page.evaluate(() => [...document.querySelectorAll('input')].some((i) => i.value === 'panes'));
    if (!part) throw new Error("the part box doesn't hold \"panes\"");
    const choice = await page.evaluate(() => [...document.querySelectorAll('select')].some((s) => s.selectedOptions[0]?.textContent.trim() === 'no'));
    if (!choice) throw new Error('there is no yes/no choice set to "no"');
    console.log('PASS: Show/hide is added with its own editor: a part box holding "panes" and a yes/no choice');

    if (errors.length) throw new Error(`page errors:\n  ${errors.join('\n  ')}`);
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
