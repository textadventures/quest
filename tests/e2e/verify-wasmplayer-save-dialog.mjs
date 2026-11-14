// The Save/Load dialog: one-click saves titled by where the player was, newest first; each save's
// menu (Overwrite, Rename, Save to file, Delete); the questions it asks before losing unsaved
// progress, overwriting or deleting; the "Continue your game?" prompt's Continue button; and no
// WCAG A/AA violations, light or dark, with the menu open.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';
import { createRequire } from 'node:module';

const axePath = createRequire(import.meta.url).resolve('axe-core/axe.min.js');

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
const context = await browser.newContext({ acceptDownloads: true });
const page = await context.newPage();
const pageErrors = [];
page.on('pageerror', err => pageErrors.push(err.message));
// The dialog asks its questions itself; a native confirm() would mean it had fallen back to one.
page.on('dialog', dialog => {
    pageErrors.push(`unexpected native ${dialog.type()}: ${dialog.message()}`);
    dialog.dismiss();
});

const commandInput = page.locator('#txtCommand:visible, qv-command-bar input:visible').first();

async function sendCommand(command) {
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 30000 });
    await commandInput.fill(command);
    await commandInput.press('Enter');
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 30000 });
}

async function openDialog() {
    await page.evaluate(() => openSavesDialog('manage'));
    await page.locator('#qv-saves[open]').waitFor({ timeout: 5000 });
}

// Each row's title and the line under it, top to bottom.
async function rows() {
    return page.$$eval('#qv-saves-list [data-row-slot]', els => els.map(el => ({
        slot: Number(el.dataset.rowSlot),
        title: el.querySelector('.qv-save-title')?.textContent ?? null,
        meta: el.querySelector('.qv-save-meta')?.textContent ?? '',
    })));
}

function check(condition, message) {
    if (!condition) throw new Error(message);
    console.log(`PASS: ${message}`);
}

const row = (slot) => page.locator(`#qv-saves-list [data-row-slot="${slot}"]`);
const menu = page.locator('#qv-saves-menu');

// Opens a save's menu and picks an item from it.
async function pick(slot, action) {
    await row(slot).locator('[data-slot-options]').click();
    await menu.locator(`[data-menu-action="${action}"]`).click();
}

async function axe(what) {
    if (!await page.evaluate(() => !!window.axe)) await page.addScriptTag({ path: axePath });
    const violations = await page.evaluate(async () => (await window.axe.run(document.getElementById('qv-saves'), {
        runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] },
    })).violations.flatMap(v => v.nodes.map(n => `${v.id}: ${n.target.join(' ')}`)));
    check(violations.length === 0, `no WCAG A/AA violations ${what} (${violations.slice(0, 5).join('; ')})`);
}

try {
    await page.goto(`${baseUrl}/?url=/examples/simple.aslx`);
    await sendCommand('look');
    await page.waitForFunction(() => document.getElementById('location')?.textContent.trim() === 'A room', null, { timeout: 10000 });

    await openDialog();
    check(await page.locator('#qv-saves-list .qv-saves-empty').isVisible(), 'with no saves, the list says so');
    check(await page.evaluate(() => document.activeElement?.id) === 'qv-saves-save-new', 'the dialog opens on the Save button');

    // An unnamed save is titled by where the player was.
    await page.click('#qv-saves-save-new');
    await page.waitForFunction(() => document.querySelectorAll('#qv-saves-list [data-row-slot]').length === 1);
    let list = await rows();
    check(list[0].title === 'A room' && /^Today, /.test(list[0].meta), `an unnamed save is titled by its location, with when it was made (${JSON.stringify(list[0])})`);
    const unnamed = list[0].slot;

    // A named save comes first, with its location under it.
    await page.fill('#qv-saves-name-input', 'Before the cellar');
    await page.press('#qv-saves-name-input', 'Enter');
    await page.waitForFunction(() => document.querySelectorAll('#qv-saves-list [data-row-slot]').length === 2);
    list = await rows();
    check(list[0].title === 'Before the cellar' && list[0].meta.endsWith(' · A room'), `a named save is listed first, with its location underneath (${JSON.stringify(list[0])})`);
    check(await page.inputValue('#qv-saves-name-input') === '', 'the name box empties after saving');
    const named = list[0].slot;

    // The menu: its first item is focused, the arrow keys move through it, Escape closes it without
    // closing the dialog and puts focus back on its button, and pressing the button again closes it.
    const namedOptions = row(named).locator('[data-slot-options]');
    await namedOptions.click();
    check(await menu.isVisible() && await namedOptions.getAttribute('aria-expanded') === 'true', "a save's ⋯ button opens its menu");
    const focused = () => page.evaluate(() => document.activeElement?.dataset.menuAction ?? document.activeElement?.dataset.slotOptions ?? null);
    check(await focused() === 'overwrite', 'the menu opens on its first item');
    await page.keyboard.press('ArrowDown');
    check(await focused() === 'rename', 'Down moves to the next item');
    await page.keyboard.press('ArrowUp');
    await page.keyboard.press('ArrowUp');
    check(await focused() === 'delete', 'Up from the first item wraps to the last');
    await axe('with saves listed and a menu open');
    await page.keyboard.press('Escape');
    check(!(await menu.isVisible()) && await page.locator('#qv-saves[open]').isVisible(), 'Escape closes the menu, not the dialog');
    // The popover's toggle event, which resets aria-expanded, comes after the key press.
    await page.waitForFunction(slot => document.querySelector(`[data-slot-options="${slot}"]`).getAttribute('aria-expanded') === 'false', named, { timeout: 2000 });
    check(await focused() === String(named), 'focus goes back to the ⋯ button, which says the menu is closed');
    await namedOptions.click();
    await namedOptions.click();
    check(!(await menu.isVisible()), "pressing a menu's ⋯ button again closes it");
    await namedOptions.click();
    await row(unnamed).locator('[data-slot-options]').click();
    check(await menu.isVisible() && await page.evaluate(() => document.getElementById('qv-saves-menu').dataset.slot) === String(unnamed),
        "another save's ⋯ button opens the menu for that save");
    await page.keyboard.press('Escape');

    // Rename: Enter keeps the new name; an empty name makes it unnamed again; Escape keeps the old one.
    await pick(unnamed, 'rename');
    await row(unnamed).locator('input.qv-save-rename').fill('By the thing');
    await row(unnamed).locator('input.qv-save-rename').press('Enter');
    await page.waitForFunction(slot => document.querySelector(`#qv-saves-list [data-row-slot="${slot}"] .qv-save-title`)?.textContent === 'By the thing', unnamed);
    check(true, 'renaming a save changes its title');
    await pick(unnamed, 'rename');
    await row(unnamed).locator('input.qv-save-rename').fill('Never kept');
    await page.keyboard.press('Escape');
    await page.waitForFunction(slot => document.querySelector(`#qv-saves-list [data-row-slot="${slot}"] .qv-save-title`)?.textContent === 'By the thing', unnamed);
    check(await page.locator('#qv-saves[open]').isVisible(), 'Escape while renaming keeps the old name and the dialog open');
    await pick(unnamed, 'rename');
    await row(unnamed).locator('input.qv-save-rename').fill('');
    await row(unnamed).locator('input.qv-save-rename').press('Enter');
    await page.waitForFunction(slot => document.querySelector(`#qv-saves-list [data-row-slot="${slot}"] .qv-save-title`)?.textContent === 'A room', unnamed);
    check(true, 'renaming to nothing makes the save unnamed again');

    // Save to file from a save's menu downloads that save.
    const [download] = await Promise.all([
        page.waitForEvent('download', { timeout: 5000 }),
        pick(named, 'download'),
    ]);
    check(download.suggestedFilename().endsWith('.quest-save'), `a save's "Save to file" downloads it (${download.suggestedFilename()})`);

    // Overwrite asks first; Cancel leaves the save alone, and the save keeps its name and slot.
    const before = await page.evaluate(slot => GameSaver.listSaves().then(s => s.find(x => x.slotIndex === slot).timestamp.valueOf()), named);
    await pick(named, 'overwrite');
    check(await page.locator('#qv-saves-confirm-text').textContent() === 'Replace "Before the cellar" with your game as it is now?',
        'Overwrite asks before replacing a save');
    await page.click('#qv-saves-confirm-cancel');
    check(await page.locator('#qv-saves-confirm-yes').count() === 0 && await focused() === String(named),
        "cancelling puts focus back on the save's ⋯ button");
    await page.waitForTimeout(20);
    await pick(named, 'overwrite');
    await page.click('#qv-saves-confirm-yes');
    await page.waitForFunction(([slot, before]) => GameSaver.listSaves().then(s => s.find(x => x.slotIndex === slot).timestamp.valueOf() > before), [named, before]);
    list = await rows();
    check(list.length === 2 && list[0].slot === named && list[0].title === 'Before the cellar', 'overwriting keeps the save\'s name and place, and makes no new save');

    // Loading with unsaved progress asks first. Escape cancels the question, not the dialog.
    await page.click('#qv-saves-close');
    await sendCommand('inventory');
    await openDialog();
    await row(unnamed).locator('[data-slot]').click();
    check((await page.locator('#qv-saves-confirm-text').textContent()).startsWith('Load "A room"?'), 'loading a save after playing on asks first');
    await page.keyboard.press('Escape');
    check(await page.locator('#qv-saves-confirm-yes').count() === 0 && await page.locator('#qv-saves[open]').isVisible(),
        'Escape cancels the question and keeps the dialog open');

    // Delete asks first, then removes just that save.
    await pick(unnamed, 'delete');
    check(await page.locator('#qv-saves-confirm-text').textContent() === 'Delete "A room"? This can\'t be undone.', 'Delete asks first');
    await page.click('#qv-saves-confirm-yes');
    await page.waitForFunction(() => document.querySelectorAll('#qv-saves-list [data-row-slot]').length === 1);
    check((await rows())[0].slot === named, 'deleting removes just that save');

    // Start again asks first when there's unsaved progress.
    await page.click('#qv-saves-restart');
    check((await page.locator('#qv-saves-confirm-text').textContent()).startsWith('Start again from the beginning?'), 'Start again asks first after playing on');
    await page.click('#qv-saves-confirm-cancel');
    check(await page.locator('#qv-saves[open]').isVisible(), 'cancelling Start again leaves the game alone');

    // "Save to file" keeps its label while it works, and counts as saving: Start again no longer asks.
    const footerLabel = await page.locator('#qv-saves-download').textContent();
    const [footerDownload] = await Promise.all([page.waitForEvent('download', { timeout: 5000 }), page.click('#qv-saves-download')]);
    check(footerDownload && await page.locator('#qv-saves-download').textContent() === footerLabel, '"Save to file" downloads the game without changing its label');
    check(await page.evaluate(() => stepsSinceSave) === 0, '"Save to file" counts as saving');

    // Loading a save with nothing since the last save doesn't ask.
    await page.click('#qv-saves-save-new');
    await page.waitForFunction(() => document.querySelectorAll('#qv-saves-list [data-row-slot]').length === 2);
    await row(named).locator('[data-slot]').click();
    await page.locator('#qv-restarting').waitFor({ state: 'hidden', timeout: 30000 });
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 30000 });
    check(await page.locator('#qv-saves-confirm-yes').count() === 0, 'loading straight after saving loads without asking');

    // A save from before saves could be unnamed, with a made-up "Saved game — <date>" name, is
    // shown like an unnamed one.
    const legacyTitle = await page.evaluate(() => {
        renderSavesList([{ slotIndex: 99, name: 'Saved game — 13/08/2026, 14:30:00', location: 'Kitchen', timestamp: new Date() }], 'manage');
        return document.querySelector('#qv-saves-list .qv-save-title').textContent;
    });
    check(legacyTitle === 'Kitchen', `an old save's made-up name isn't shown (got "${legacyTitle}")`);

    // Dark, on the v2 UI: the dialog and the menu have no WCAG violations there either.
    await page.goto(`${baseUrl}/?url=/examples/simple.aslx&ui=v2`);
    await page.locator('#qv-saves[open][data-mode="boot"]').waitFor({ timeout: 30000 });
    await page.click('#qv-saves-start-new');
    await page.waitForFunction(() => window.canSendCommand === true, null, { timeout: 30000 });
    await page.evaluate(() => { localStorage.setItem('questviva-colour-scheme', 'dark'); });
    await page.emulateMedia({ colorScheme: 'dark' });
    await openDialog();
    await page.waitForFunction(() => getComputedStyle(document.getElementById('qv-saves')).colorScheme === 'dark', null, { timeout: 5000 });
    await page.locator('#qv-saves-list [data-slot-options]').first().click();
    await axe('in dark, with saves listed and a menu open');
    await page.keyboard.press('Escape');
    await page.click('#qv-saves-close');
    await page.emulateMedia({ colorScheme: 'light' });

    // The boot prompt: Continue loads the newest save, the rest are listed, and it can't be dismissed.
    await page.reload();
    await page.locator('#qv-saves[open][data-mode="boot"]').waitFor({ timeout: 30000 });
    const detail = await page.locator('#qv-saves-continue-detail').textContent();
    check(/^A room · Today, /.test(detail), `Continue names the newest save (${detail})`);
    list = await rows();
    check(list.length === 1 && list[0].title === 'Before the cellar' && await page.locator('#qv-saves-other-heading').isVisible(),
        'the other saves are listed under it');
    check(await page.locator('#qv-saves-list [data-slot-options]').count() === 0, 'the boot prompt has no options buttons');
    await page.keyboard.press('Escape');
    check(await page.locator('#qv-saves[open]').isVisible(), "Escape doesn't dismiss the boot prompt");
    await page.click('#qv-saves-continue');
    const loaded = await page.waitForFunction(() => window.canSendCommand === true && bugReportFromSave, null, { timeout: 30000 })
        .then(() => true, () => false);
    check(loaded, 'Continue loads the save');

    if (pageErrors.length) throw new Error(`Page errors: ${JSON.stringify(pageErrors)}`);
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
