// The editor must never autosave over a change made to the game file outside it (by an AI agent
// or a text editor, say) — issue #2922. Drives a game opened through File System Access
// ("Save to folder…"), with showDirectoryPicker stubbed to hand back a folder in the page's own
// origin-private file system: a real FileSystemDirectoryHandle that the test can also write to
// from page.evaluate, standing in for the other program.
//
// The editor checks the file every couple of seconds, and reloads by itself when nothing is
// unsaved (disk-change-follow.spec.mjs), so each scenario changes the file while an edit is still
// being typed:
// 1. The banner asks which copy to keep, and when the edit is finished autosave writes nothing.
//    "Keep my changes" then saves over the file.
// 2. Same again, but "Use the changed file": the editor loads the outside change and the edit is
//    dropped.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

const FOLDER = 'disk-change-save-guard';
const FILE = 'Guard Test.aslx';

test('disk-change-save-guard', async ({ baseURL }) => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

    await page.addInitScript((folder) => {
        window.showDirectoryPicker = async () =>
            (await navigator.storage.getDirectory()).getDirectoryHandle(folder, { create: true });
    }, FOLDER);

    const readDisk = () => page.evaluate(async ([folder, file]) => {
        const dir = await (await navigator.storage.getDirectory()).getDirectoryHandle(folder);
        return (await (await dir.getFileHandle(file)).getFile()).text();
    }, [FOLDER, FILE]);

    const writeDisk = (text) => page.evaluate(async ([folder, file, content]) => {
        const dir = await (await navigator.storage.getDirectory()).getDirectoryHandle(folder);
        const writable = await (await dir.getFileHandle(file)).createWritable();
        await writable.write(content);
        await writable.close();
    }, [FOLDER, FILE, text]);

    // Adds a room, as an agent might, so the outside change is easy to see in the tree.
    async function changeOutside(roomName) {
        const xml = await readDisk();
        if (!xml.includes('</asl>')) throw new Error('Unexpected game XML: no </asl>');
        const changed = xml.replace('</asl>', `  <object name="${roomName}">\n    <inherit name="editor_room" />\n  </object>\n</asl>`);
        await writeDisk(changed);
        return changed;
    }

    // The game node is selected on load; its first text field is the description. See
    // autosave.spec.mjs for why the Filter box is excluded and the events dispatched by hand.
    const descriptionBox = () => page.locator('textarea, input[type="text"]:not([placeholder="Filter..."])').first();
    async function startEdit(text) {
        const box = descriptionBox();
        await box.waitFor({ timeout: 10000 });
        await box.fill(text);
        await box.dispatchEvent('input');
    }
    async function finishEdit() {
        await descriptionBox().dispatchEvent('change');
        await page.click('.toolbar-divider');
        await page.locator('.save-chip-unsaved').waitFor({ state: 'visible', timeout: 5000 });
    }
    async function editDescription(text) {
        await startEdit(text);
        await finishEdit();
    }
    // Changes the file while an edit is being typed, then finishes the edit.
    async function editDuringOutsideChange(text, roomName) {
        await startEdit(text);
        const changed = await changeOutside(roomName);
        await banner.getByRole('button', { name: 'Keep my changes' }).waitFor({ timeout: 10000 });
        await finishEdit();
        return changed;
    }

    const banner = page.getByTestId('file-changed-banner');

    // Waits longer than the 800 ms autosave debounce, then checks the file is still the outside
    // version: autosave would have replaced it by now if the guard weren't there.
    async function expectHeld(expected) {
        await banner.getByRole('button', { name: 'Keep my changes' }).waitFor({ timeout: 10000 });
        await page.waitForTimeout(2000);
        const onDisk = await readDisk();
        if (onDisk !== expected) throw new Error('The editor saved over the change made outside it');
        if (!(await page.locator('.save-chip-unsaved').isVisible())) throw new Error('Expected the edit to still show as unsaved');
    }

    async function treeHas(name) {
        const filter = page.locator('input[placeholder="Filter..."]');
        await filter.fill(name);
        const found = await page.locator(`[role="treeitem"]:has-text("${name}")`).first()
            .waitFor({ timeout: 5000 }).then(() => true, () => false);
        await filter.fill('');
        return found;
    }

    try {
        await page.goto(`${baseURL}/open`);
        await page.fill('input[placeholder="Game name"]', 'Guard Test');
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Save to folder…")');
        await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });

        // --- 1. Keep my changes ---
        const marker1 = `Kept ${Date.now()}`;
        const outside1 = await editDuringOutsideChange(marker1, 'agentroomone');
        await expectHeld(outside1);
        await banner.getByRole('button', { name: 'Keep my changes' }).click();
        await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
        const afterKeep = await readDisk();
        if (!afterKeep.includes(marker1)) throw new Error('"Keep my changes" did not save the edit');
        if (afterKeep.includes('agentroomone')) throw new Error('"Keep my changes" kept the outside change too');
        if (await banner.isVisible()) throw new Error('Banner still showing after "Keep my changes"');

        // A later edit saves normally: the file is the editor's own again.
        const marker2 = `Normal ${Date.now()}`;
        await editDescription(marker2);
        await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
        if (!(await readDisk()).includes(marker2)) throw new Error('A normal save after "Keep my changes" did not happen');

        // --- 2. Use the changed file ---
        const outside2 = await editDuringOutsideChange(`Dropped ${Date.now()}`, 'agentroomtwo');
        await expectHeld(outside2);
        await banner.getByRole('button', { name: 'Use the changed file' }).click();
        await banner.waitFor({ state: 'hidden', timeout: 30000 });
        // Checked before treeHas(): typing in the tree's Filter box counts as an edit in progress.
        if (await page.locator('.save-chip-unsaved').isVisible()) throw new Error('Expected nothing unsaved after "Use the changed file"');
        if (!(await treeHas('agentroomtwo'))) throw new Error('"Use the changed file" did not load the outside change');
        if ((await readDisk()) !== outside2) throw new Error('"Use the changed file" changed the file');
    } catch (err) {
        await page.screenshot({ path: '/tmp/appshell-disk-change-save-guard-failure.png' });
        throw err;
    } finally {
        await browser.close();
    }
});
