// The backup reminder for a local draft (BackupReminderCard.svelte, issue #2957):
// - it waits until the draft has been edited on two different local days, so a game made a few
//   minutes ago never gets it;
// - it's a card at the foot of the tree, so showing it never moves the editor;
// - while the tree isn't shown (here, Code View), the toolbar's "Saved" chip gets a download icon
//   at the same width, which brings the tree back and focuses the card's Download button;
// - Remind me later keeps it away until time has passed *and* the draft has changed since.
// The "different day" is faked by moving the dates kept in the draft's meta.json back.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

const GAME_NAME = 'Backup Reminder Test';
const DAY = 24 * 60 * 60 * 1000;

test('backup-reminder', async ({ baseURL }) => {
    const browser = await chromium.launch();
    const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    const card = page.getByTestId('backup-reminder');
    const chipButton = page.getByRole('button', { name: 'Saved in this browser only. Show the backup reminder' });

    // Changes the game's description and waits for the autosave to finish.
    let editCount = 0;
    async function edit() {
        const descBox = page.locator('textarea, input[type="text"]:not([placeholder="Filter..."])').first();
        await descBox.fill(`Edit ${++editCount}`);
        await descBox.dispatchEvent('input');
        await descBox.dispatchEvent('change');
        await page.click('.toolbar-divider');
        await page.locator('.save-chip-unsaved').waitFor({ state: 'visible', timeout: 5000 });
        await page.locator('.save-chip-unsaved').waitFor({ state: 'hidden', timeout: 10000 });
        await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
    }

    // Reads or rewrites this draft's meta.json in OPFS.
    async function updateMeta(change) {
        return page.evaluate(async ({ filename, change }) => {
            const games = await (await navigator.storage.getDirectory()).getDirectoryHandle('games');
            for await (const [, dir] of games.entries()) {
                if (dir.kind !== 'directory') continue;
                const fh = await dir.getFileHandle('meta.json');
                const meta = JSON.parse(await (await fh.getFile()).text());
                if (meta.filename !== filename) continue;
                for (const [key, ago] of Object.entries(change)) meta[key] = Date.now() - ago;
                const w = await fh.createWritable();
                await w.write(JSON.stringify(meta));
                await w.close();
                return meta;
            }
            throw new Error(`No draft meta for ${filename}`);
        }, { filename: `${GAME_NAME}.aslx`, change });
    }

    async function reopen() {
        await page.goto(`${baseURL}/open`);
        await page.waitForSelector('text=Your local drafts', { timeout: 10000 });
        await page.click(`button:has-text("${GAME_NAME}.aslx")`);
        await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
        await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
    }

    async function expectHidden(when) {
        // Long enough for the store's async re-check after a save or load to have run.
        await page.waitForTimeout(500);
        if (await card.count() !== 0) throw new Error(`Backup reminder showed ${when}`);
    }

    try {
        await page.goto(`${baseURL}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        await page.fill('input[placeholder="Game name"]', GAME_NAME);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });

        // Edited today only: no reminder, however many saves.
        await edit();
        await edit();
        await expectHidden('for a draft only edited today');
        const meta = await updateMeta({});
        if (meta.firstEditedAt == null || meta.editedOnLaterDay) {
            throw new Error(`Expected firstEditedAt set and editedOnLaterDay unset, got ${JSON.stringify(meta)}`);
        }

        // First edited two days ago, and edited again now: it shows, without moving the editor.
        await updateMeta({ firstEditedAt: 2 * DAY });
        const workspaceTop = (await page.locator('#workspace-content').boundingBox()).y;
        await edit();
        await card.waitFor({ state: 'visible', timeout: 5000 });
        const workspaceTopAfter = (await page.locator('#workspace-content').boundingBox()).y;
        if (workspaceTopAfter !== workspaceTop) {
            throw new Error(`Editor moved from y=${workspaceTop} to y=${workspaceTopAfter} when the reminder showed`);
        }
        if (await chipButton.count() !== 0) throw new Error('Saved chip shows the reminder while the tree is visible');

        // With the tree hidden by Code View, the Saved chip stands in for the card, at the same width.
        const plainChipWidth = (await page.locator('.save-chip-saved').boundingBox()).width;
        await page.click('button[aria-label="Raw XML code view"]');
        await chipButton.waitFor({ state: 'visible', timeout: 10000 });
        const chipWidth = (await chipButton.boundingBox()).width;
        if (Math.abs(chipWidth - plainChipWidth) > 0.5) {
            throw new Error(`Saved chip changed width from ${plainChipWidth} to ${chipWidth} with the download icon`);
        }
        await chipButton.click();
        await card.waitFor({ state: 'visible', timeout: 10000 });
        const download = card.getByRole('button', { name: 'Download backup' });
        await page.waitForFunction(() => document.activeElement?.textContent?.trim() === 'Download backup', null, { timeout: 5000 });
        if (!(await download.evaluate(el => el === document.activeElement))) {
            throw new Error('Download backup was not focused after clicking the Saved chip');
        }

        // Remind me later: stays away on reopening.
        await card.getByRole('button', { name: 'Remind me later' }).click();
        await card.waitFor({ state: 'detached', timeout: 5000 });
        await reopen();
        await expectHidden('straight after Remind me later');

        // Long past the wait, but unchanged since: still away. Changed since: back.
        await updateMeta({ backupBannerResolvedAt: 40 * DAY, lastModified: 41 * DAY });
        await reopen();
        await expectHidden('for a draft not changed since Remind me later');
        await edit();
        await card.waitFor({ state: 'visible', timeout: 5000 });
    } catch (err) {
        await page.screenshot({ path: '/tmp/appshell-backup-reminder-failure.png' });
        throw err;
    } finally {
        await browser.close();
    }
});
