// Dragging a file onto the Play tab (PlayCatalog.svelte's handleDrop). In the
// browser build a dropped game file is treated as picked, so the Start button
// appears, just as after "Open a game file…"; anything that isn't a Quest game
// is refused with a message naming the file. Also checks the "drop to play"
// overlay shows while dragging and goes away after.
//
// Requires the AppShell dev server (see play-open-file.spec.mjs).
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('play-drop-file', async ({ baseURL }) => {
    const browser = await chromium.launch();
    try {
        const page = await browser.newPage();
        page.on('pageerror', err => console.log('[appshell] [pageerror]', err.message));
        await page.goto(`${baseURL}/`);
        await page.waitForSelector('[data-testid="open-file-bar"]', { timeout: 15000 });

        // Fires dragenter/dragover/drop on the page with a real File in the
        // DataTransfer, which is what a drag from the desktop produces.
        async function dropFile(name, content, { drop = true } = {}) {
            const dataTransfer = await page.evaluateHandle(([n, c]) => {
                const dt = new DataTransfer();
                dt.items.add(new File([c], n));
                return dt;
            }, [name, content]);
            const target = page.locator('#discover-heading');
            await target.dispatchEvent('dragenter', { dataTransfer });
            await target.dispatchEvent('dragover', { dataTransfer });
            if (drop) await target.dispatchEvent('drop', { dataTransfer });
        }

        await dropFile('notes.txt', 'hello', { drop: false });
        const overlay = page.locator('text=Drop a game file to play it');
        await overlay.waitFor({ timeout: 5000 });
        console.log('PASS: overlay shown while dragging a file');
        await page.locator('#discover-heading').dispatchEvent('drop', {
            dataTransfer: await page.evaluateHandle(() => {
                const dt = new DataTransfer();
                dt.items.add(new File(['hello'], 'notes.txt'));
                return dt;
            }),
        });
        await overlay.waitFor({ state: 'detached', timeout: 5000 });
        console.log('PASS: overlay gone after the drop');

        const refused = page.locator('[role="alert"]', { hasText: 'notes.txt' });
        await refused.waitFor({ timeout: 5000 });
        const refusedText = await refused.innerText();
        if (!refusedText.includes("isn't a Quest game")) throw new Error(`Unexpected refusal text: ${refusedText}`);
        console.log('PASS: non-game file refused:', refusedText);

        await dropFile('dropped.aslx', '<asl version="580"></asl>');
        const bar = page.locator('[data-testid="open-file-bar"]');
        await bar.locator('text=dropped.aslx').waitFor({ timeout: 5000 });
        await bar.getByRole('button', { name: /Start/ }).waitFor({ timeout: 5000 });
        if (await refused.count() !== 0) throw new Error('Earlier refusal still showing after a good drop');
        console.log('PASS: dropped .aslx is picked and ready to Start');
    } finally {
        await browser.close();
    }
});
