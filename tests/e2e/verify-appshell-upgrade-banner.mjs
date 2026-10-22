// #2339: opening a game saved by an older Quest version shows a banner saying that saving will
// upgrade it, with a download of the file as it was loaded. The download must still be the
// original after an autosave has upgraded the stored copy, and reopening the upgraded copy
// shows no banner.
import { chromium } from './lib/tracked-chromium.mjs';
import { readFileSync, writeFileSync } from 'node:fs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

// restart-test.aslx is version 580 and has no gameid, which import requires.
const original = readFileSync(new URL('./fixtures/restart-test.aslx', import.meta.url), 'utf8')
    .replace('<game name="Simple">', '<game name="Simple">\n    <gameid>23390000-0000-0000-0000-000000000580</gameid>');
const gamePath = '/tmp/appshell-upgrade-banner.aslx';
writeFileSync(gamePath, original);

const browser = await chromium.launch();
const page = await browser.newPage({ acceptDownloads: true });
page.on('pageerror', err => console.log('[pageerror]', err.message));

const banner = page.getByText('This game was saved by an older version of Quest.');

async function downloadOriginal() {
    const [download] = await Promise.all([
        page.waitForEvent('download'),
        page.click('button:has-text("Download original")'),
    ]);
    const text = readFileSync(await download.path(), 'utf8');
    return { name: download.suggestedFilename(), text };
}

try {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Import game file")', { timeout: 30000 });
    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        page.click('button:has-text("Import game file")'),
    ]);
    await fileChooser.setFiles(gamePath);
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });

    await banner.waitFor({ timeout: 10000 });
    const href = await page.getAttribute('a:has-text("Learn more")', 'href');
    if (href !== 'https://questviva.com/reference/asl-versions/') throw new Error(`"Learn more" links to ${href}`);
    console.log('PASS: banner shown for a version 580 game, linking to the versions page');

    // Edit the description and wait for the autosave, which upgrades the stored copy.
    const descBox = page.locator('textarea, input[type="text"]:not([placeholder="Filter..."])').first();
    await descBox.waitFor({ timeout: 10000 });
    await descBox.fill(`Upgrade marker ${Date.now()}`);
    await descBox.dispatchEvent('input');
    await descBox.dispatchEvent('change');
    await page.click('.toolbar-divider');
    await page.locator('.save-chip-unsaved').waitFor({ state: 'visible', timeout: 5000 });
    await page.locator('.save-chip-saved').waitFor({ state: 'visible', timeout: 10000 });
    if (!await banner.isVisible()) throw new Error('Banner disappeared after the autosave');
    console.log('PASS: banner still shown after the autosave');

    const { name, text } = await downloadOriginal();
    if (name !== 'appshell-upgrade-banner.v580.aslx') throw new Error(`Downloaded as ${name}`);
    if (text !== original) throw new Error('Download is not the file as it was loaded');
    await banner.waitFor({ state: 'hidden', timeout: 5000 });
    console.log('PASS: download is the original version 580 file, and closes the banner');

    // The stored draft is now version 610, so reopening it shows no banner.
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('text=Your local drafts', { timeout: 10000 });
    await page.click('button:has-text("appshell-upgrade-banner.aslx")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    await page.waitForTimeout(500);
    if (await banner.isVisible()) throw new Error('Banner shown for the upgraded draft');
    console.log('PASS: no banner once the game has been saved at the current version');

    console.log('PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-upgrade-banner-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
