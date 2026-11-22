// The browser editor follows changes made to the open game outside it (by an AI agent or a text
// editor, say) — issue #2922. File System Access can't watch a file, so the editor checks it every
// couple of seconds and when the window gets the focus. Drives a game opened through "Save to
// folder…", with showDirectoryPicker stubbed to hand back a folder in the page's own
// origin-private file system, which the test also writes to from page.evaluate, standing in for
// the other program. disk-change-save-guard.spec.mjs covers a change while there are unsaved
// edits.
//
// 1. With nothing unsaved, a change reloads by itself and says so, keeping the selected element
//    and tab.
// 2. A change that can't be loaded leaves the game as it was and says why; once the file is fixed
//    it loads.
// 3. A change to an included library alone reloads the game too.
// 4. A deleted file isn't put back by autosave; "Save it again" does that on purpose.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

const FOLDER = 'disk-change-follow';
const FILE = 'Follow Test.aslx';
const LIBRARY = 'agentlib.aslx';
const RELOADED = 'Reloaded: the file was changed outside the editor.';

test('disk-change-follow', async ({ baseURL }) => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.addInitScript((folder) => {
        window.showDirectoryPicker = async () =>
            (await navigator.storage.getDirectory()).getDirectoryHandle(folder, { create: true });
    }, FOLDER);

    const readDisk = (file = FILE) => page.evaluate(async ([folder, name]) => {
        const dir = await (await navigator.storage.getDirectory()).getDirectoryHandle(folder);
        try {
            return await (await (await dir.getFileHandle(name)).getFile()).text();
        } catch {
            return null;
        }
    }, [FOLDER, file]);

    const writeDisk = (text, file = FILE) => page.evaluate(async ([folder, name, content]) => {
        const dir = await (await navigator.storage.getDirectory()).getDirectoryHandle(folder);
        const writable = await (await dir.getFileHandle(name, { create: true })).createWritable();
        await writable.write(content);
        await writable.close();
    }, [FOLDER, file, text]);

    const deleteDisk = () => page.evaluate(async ([folder, name]) => {
        const dir = await (await navigator.storage.getDirectory()).getDirectoryHandle(folder);
        await dir.removeEntry(name);
    }, [FOLDER, FILE]);

    const room = (name) => `  <object name="${name}">\n    <inherit name="editor_room" />\n  </object>\n`;

    async function changeOutside(insert) {
        const xml = await readDisk();
        if (!xml?.includes('</asl>')) throw new Error('Unexpected game XML: no </asl>');
        const changed = xml.replace('</asl>', `${insert}</asl>`);
        await writeDisk(changed);
        return changed;
    }

    const banner = page.getByTestId('file-changed-banner');
    const toast = page.getByText(RELOADED);

    // Toasts last a few seconds: waits for the last one to go, so the next wait sees a new one.
    async function expectReloaded() {
        await toast.waitFor({ timeout: 15000 });
        await toast.waitFor({ state: 'hidden', timeout: 15000 });
    }

    async function treeHas(name) {
        const filter = page.locator('input[placeholder="Filter..."]');
        await filter.fill(name);
        const found = await page.locator(`[role="treeitem"]:has-text("${name}")`).first()
            .waitFor({ timeout: 5000 }).then(() => true, () => false);
        await filter.fill('');
        await page.click('.toolbar-divider');
        return found;
    }

    // The room's Exits tab: its compass grid only shows there.
    const exitsGrid = page.getByRole('button', { name: 'north', exact: true });

    try {
        await page.goto(`${baseURL}/open`);
        await page.fill('input[placeholder="Game name"]', 'Follow Test');
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Save to folder…")');
        await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });

        // --- 1. Reloads by itself, keeping the selection and tab ---
        await page.locator('[data-value="room"][data-part="item"], [data-value="room"][data-part="branch-control"]').first().click();
        await page.click('button:has-text("Exits")');
        await exitsGrid.waitFor({ timeout: 10000 });
        const outside1 = await changeOutside(room('agentroomone'));
        await expectReloaded();
        if (await banner.isVisible()) throw new Error('Expected no banner when nothing was unsaved');
        if (!(await exitsGrid.isVisible())) throw new Error('The reload lost the selected element or its tab');
        if (!(await treeHas('agentroomone'))) throw new Error('The automatic reload did not load the outside change');
        if ((await readDisk()) !== outside1) throw new Error('The automatic reload changed the file');

        // --- 2. A change that can't be loaded, then fixed ---
        await writeDisk(outside1.replace('</asl>', '<object name="broken">\n</asl>'));
        await banner.getByText("can't be loaded").waitFor({ timeout: 15000 });
        if (!(await exitsGrid.isVisible())) throw new Error('A file that does not load should leave the game showing');
        const fixed = outside1.replace('</asl>', `${room('agentroomtwo')}</asl>`);
        await writeDisk(fixed);
        await expectReloaded();
        if (await banner.isVisible()) throw new Error('Banner still showing once the file loads again');
        if (!(await treeHas('agentroomtwo'))) throw new Error('The fixed file was not loaded');

        // --- 3. An included library changes ---
        await writeDisk(`<library>\n${room('libroomone')}</library>\n`, LIBRARY);
        await changeOutside(`  <include ref="${LIBRARY}" />\n`);
        await expectReloaded();
        await writeDisk(`<library>\n${room('libroomtwo')}</library>\n`, LIBRARY);
        await expectReloaded();
        await page.getByRole('button', { name: 'View options' }).click();
        await page.getByText('Show Library Elements').click();
        if (!(await treeHas('libroomtwo'))) throw new Error('The library change was not loaded');
        if (await treeHas('libroomone')) throw new Error('Still showing the library as it was');

        // --- 4. The file is deleted ---
        await deleteDisk();
        await banner.getByText('deleted or moved').waitFor({ timeout: 15000 });
        const box = page.locator('textarea, input[type="text"]:not([placeholder="Filter..."])').first();
        const marker = `Saved again ${Date.now()}`;
        await page.locator('[data-value="game"][data-part="branch-control"]').click({ timeout: 10000 });
        await box.waitFor({ timeout: 10000 });
        await box.fill(marker);
        await box.dispatchEvent('input');
        await box.dispatchEvent('change');
        await page.click('.toolbar-divider');
        await page.waitForTimeout(2000);
        if ((await readDisk()) !== null) throw new Error('Autosave put the deleted file back');
        await banner.getByRole('button', { name: 'Save it again' }).click();
        await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
        if (!(await readDisk())?.includes(marker)) throw new Error('"Save it again" did not save the game');
        if (await banner.isVisible()) throw new Error('Banner still showing after "Save it again"');
    } catch (err) {
        await page.screenshot({ path: '/tmp/appshell-disk-change-follow-failure.png' });
        throw err;
    } finally {
        await browser.close();
    }
});
