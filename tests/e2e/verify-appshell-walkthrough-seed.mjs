// The editor's walkthrough Play and Record buttons start the game with a fixed random seed (#2673),
// so a walkthrough through something random replays the same way every time, and the same way
// `qv walkthrough` runs it. Plain Preview keeps a random seed.
//
// Each run asks the game for a random number once the walkthrough's steps have run: two Plays and a
// Record must agree, and two Previews must not (the chance of a clash is one in a million).
//
// Requires both dev servers running (./dev.sh): AppShell on 5174 (proxies /player to WasmPlayer
// on 5175, so BroadcastChannel — same-origin only — actually works between the two tabs).
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const RANDOM_EXPRESSION = 'GetRandomInt(1, 1000000)';

const browser = await chromium.launch();

// Opens the player tab that clicking `button` starts, waits until `ready` says the game has got to
// the point to sample, and returns the tab's URL and a random number from the game.
async function sampleRun(ctx, page, button, ready) {
    const [player] = await Promise.all([ctx.waitForEvent('page'), button.click()]);
    const errors = [];
    player.on('pageerror', err => errors.push(err.message));
    await player.waitForSelector('qv-command-bar input, #txtCommandDiv', { state: 'visible', timeout: 30000 });
    await player.waitForFunction(ready, null, { timeout: 30000 });
    const value = await player.evaluate(expr => window.QuestVivaTest.evaluate(expr), RANDOM_EXPRESSION);
    const url = player.url();
    await player.close();
    if (errors.length) throw new Error(`Player errors: ${JSON.stringify(errors)}`);
    return { url, value: String(value) };
}

const bannerHidden = () => document.getElementById('qv-recording')?.classList.contains('hidden') ?? true;
const recording = () => document.getElementById('qv-recording-status')?.textContent?.startsWith('Recording') ?? false;

try {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Walkthrough Seed Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Preview game"]', { timeout: 30000 });
    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });
    console.log('PASS: local draft created');

    const tree = page.locator('.overflow-y-auto.p-1.text-xs');
    await tree.getByText('Advanced', { exact: true }).click();
    await page.getByRole('button', { name: '+ Add Walkthrough', exact: true }).click();
    await page.fill('#element-name', 'seedtest');
    await page.getByRole('button', { name: 'Add Walkthrough', exact: true }).click();
    await tree.getByText('seedtest', { exact: true }).waitFor({ state: 'visible', timeout: 5000 });

    const stepInput = page.locator('input[data-staging]').last();
    await stepInput.fill('look');
    await stepInput.press('Enter');
    const play = page.getByRole('button', { name: /Play/ }).and(page.locator(':not([disabled])')).first();
    await play.waitFor({ state: 'visible', timeout: 5000 });
    await page.waitForSelector('.save-chip-saved', { timeout: 10000 });
    console.log('PASS: walkthrough "seedtest" created with one step');

    const play1 = await sampleRun(ctx, page, play, bannerHidden);
    const play2 = await sampleRun(ctx, page, play, bannerHidden);
    if (!/[?&]seed=\d+/.test(play1.url)) throw new Error(`Play didn't pass a seed to the player: ${play1.url}`);
    if (play1.value !== play2.value) throw new Error(`Two Plays gave different random numbers: ${play1.value} vs ${play2.value}`);
    console.log(`PASS: two Plays give the same random number (${play1.value})`);

    const record = page.getByRole('button', { name: /Record/ }).first();
    const recorded = await sampleRun(ctx, page, record, recording);
    if (recorded.value !== play1.value) throw new Error(`Record gave a different random number from Play: ${recorded.value} vs ${play1.value}`);
    console.log('PASS: Record gives the same random number as Play');

    const preview = page.locator('button[title="Preview game"]');
    const preview1 = await sampleRun(ctx, page, preview, () => true);
    const preview2 = await sampleRun(ctx, page, preview, () => true);
    if (/[?&]seed=/.test(preview1.url)) throw new Error(`Preview passed a seed to the player: ${preview1.url}`);
    if (preview1.value === preview2.value) throw new Error(`Two Previews gave the same random number (${preview1.value}), so Preview looks seeded`);
    console.log('PASS: Preview stays unseeded');

    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
