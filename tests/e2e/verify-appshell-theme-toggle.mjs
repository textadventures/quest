// Verifies the AppShell theme setting (Settings modal → Light/Dark/Match
// system) introduced by the theme-toggle feature:
//
//  - Default is "system": the app follows the OS via prefers-color-scheme,
//    flipping a .dark class on <html> (app.css's @custom-variant dark) that
//    drives both Tailwind's dark: utilities and the light-dark() paired
//    tokens (compiled by Lightning CSS to class-keyed --lightningcss-light/
//    --lightningcss-dark vars on :root/:root.dark).
//  - Selecting Dark/Light toggles that class live (no reload) and persists
//    to localStorage (questviva-ui-theme).
//  - A reload re-applies the stored theme from app.html's pre-paint inline
//    script, so there's no flash of the wrong theme.
//  - "system" keeps reacting to a live OS colour-scheme change (the
//    matchMedia listener in theme-store.ts).
//
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5180';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

function assert(cond, label) {
    if (!cond) throw new Error(`FAIL: ${label}`);
    console.log(`PASS: ${label}`);
}

async function htmlState(label) {
    return page.evaluate(() => ({
        dark: document.documentElement.classList.contains('dark'),
        colorScheme: document.documentElement.style.colorScheme,
        bodyBg: getComputedStyle(document.body).backgroundColor,
    })).then(s => { console.log(`[${label}] html.dark=${s.dark} colorScheme=${s.colorScheme} bodyBg=${s.bodyBg}`); return s; });
}

await page.goto(`${baseUrl}/`);
await page.waitForSelector('button.home-header-link', { timeout: 30000 });

// --- 1. Default = "system", OS light → light theme ---
let state = await htmlState('initial');
const initial = state;
assert(state.dark === false, 'default (system, OS light) renders light');
assert(state.colorScheme === 'light', 'default colorScheme is light');

// --- 2. Settings modal shows the theme control with the right options ---
await page.click('button.home-header-link');
await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
assert(await page.textContent('div[role="dialog"] label[for="settings-theme"]') === 'Theme', 'theme label reads "Theme"');
const options = await page.$$eval('#settings-theme option', els => els.map(o => o.textContent));
assert(JSON.stringify(options) === JSON.stringify(['Light', 'Dark', 'Match system']), `theme options are Light/Dark/Match system, got ${JSON.stringify(options)}`);
assert(await page.inputValue('#settings-theme') === 'system', 'theme select defaults to system');

// --- 3. Select Dark → .dark class applies live, persisted ---
await page.selectOption('#settings-theme', 'dark');
state = await htmlState('after Dark');
assert(state.dark === true, 'selecting Dark adds .dark to <html>');
assert(state.colorScheme === 'dark', 'selecting Dark sets colorScheme dark');
assert(await page.inputValue('#settings-theme') === 'dark', 'select value follows the store');
const stored = await page.evaluate(() => localStorage.getItem('questviva-ui-theme'));
assert(stored === 'dark', `theme persisted to localStorage, got ${JSON.stringify(stored)}`);
const lightBg = initial.bodyBg;
assert(lightBg !== 'rgba(0, 0, 0, 0)' && lightBg !== '', `captured a real light body background, got ${lightBg}`);
assert(state.bodyBg !== lightBg, `body background actually re-rendered on Dark (${lightBg} → ${state.bodyBg})`);

// --- 4. Reload → stored Dark re-applied pre-paint (no flash) ---
await page.reload();
await page.waitForSelector('button.home-header-link', { timeout: 30000 });
state = await htmlState('after reload (Dark)');
assert(state.dark === true, 'Dark survives reload');
assert(state.colorScheme === 'dark', 'Dark colorScheme survives reload');

// --- 5. Select Light → .dark removed ---
await page.click('button.home-header-link');
await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
await page.selectOption('#settings-theme', 'light');
state = await htmlState('after Light');
assert(state.dark === false, 'selecting Light removes .dark from <html>');
assert(state.colorScheme === 'light', 'selecting Light sets colorScheme light');

// --- 6. "System" follows a live OS colour-scheme change ---
// The modal is still open with theme=light selected; switch it to system and
// then flip the emulated OS scheme to confirm the matchMedia listener reacts.
await page.selectOption('#settings-theme', 'system');
state = await htmlState('after system (OS still light)');
assert(state.dark === false, 'system + OS light renders light');
await page.emulateMedia({ colorScheme: 'dark' });
await page.waitForFunction(() => document.documentElement.classList.contains('dark'));
state = await htmlState('system, OS flipped dark');
assert(state.dark === true, 'system follows a live OS flip to dark');
assert(state.colorScheme === 'dark', 'system dark sets colorScheme dark');
await page.emulateMedia({ colorScheme: 'light' });
await page.waitForFunction(() => !document.documentElement.classList.contains('dark'));
state = await htmlState('system, OS flipped back light');
assert(state.dark === false, 'system follows a live OS flip back to light');

// --- 7. Fresh context with an OS-dark preference starts dark under system ---
const darkContext = await browser.newContext({ colorScheme: 'dark' });
const darkPage = await darkContext.newPage();
await darkPage.goto(`${baseUrl}/`);
await darkPage.waitForSelector('button.home-header-link', { timeout: 30000 });
const freshDark = await darkPage.evaluate(() => document.documentElement.classList.contains('dark'));
assert(freshDark === true, 'fresh context with OS dark starts dark (system default)');
await darkContext.close();

// --- 8. Play tab is always dark regardless of the app theme: the search
// input's border and the category select's ring must not follow the toggle
// (they were leaking the theme-adaptive surface-200-800 pair via app.css's
// input rules / Skeleton's .input ring — fixed with the .play-dark scope). ---
// The theme is currently "system" (light OS) from step 6; force Light, capture
// the control edges, switch to Dark, and assert nothing changed.
// Stub the catalog so the category select renders deterministically instead
// of racing a live textadventures.co.uk fetch (a tag category needs a slug).
await page.route('**/api/Catalog*', (route) => {
    route.fulfill({
        contentType: 'application/json',
        body: JSON.stringify({
            categories: [{ title: 'Test Category', slug: 'test', games: [] }],
            update: null,
        }),
    });
});
await page.evaluate(() => localStorage.setItem('questviva-ui-theme', 'light'));
await page.reload();
await page.waitForSelector('button.home-header-link', { timeout: 30000 });
const playEdges = () => page.evaluate(() => {
    const get = (sel) => {
        const el = document.querySelector(sel);
        if (!el) return null;
        const cs = getComputedStyle(el);
        // Capture the whole edge-relevant picture: border (search input) and
        // .input ring (category select) plus bg/text so any leak shows up.
        return {
            borderColor: cs.borderColor,
            borderWidth: cs.borderWidth,
            boxShadow: cs.boxShadow,
            backgroundColor: cs.backgroundColor,
            color: cs.color,
        };
    };
    return { search: get('input[type="search"]'), category: get('select.input') };
});
const lightEdges = await playEdges();
assert(lightEdges.search !== null, 'search input present on Play tab');
await page.click('button.home-header-link');
await page.waitForSelector('#settings-theme', { timeout: 10000 });
await page.selectOption('#settings-theme', 'dark');
await page.waitForTimeout(100);
const darkEdges = await playEdges();
for (const [name, a, b] of [['search input', lightEdges.search, darkEdges.search], ['category select', lightEdges.category, darkEdges.category]]) {
    // The category select only renders once the live catalog fetch succeeds,
    // which is network-timing dependent — skip it if either snapshot missed
    // it rather than fail the whole run. The search input is unconditional.
    if (a === null || b === null) { console.log(`SKIP: ${name} not rendered for both theme states (catalog fetch timing?)`); continue; }
    assert(JSON.stringify(a) === JSON.stringify(b), `${name} stays dark across the theme toggle (${JSON.stringify(a)})`);
}

console.log('ALL CHECKS PASSED');
await browser.close();
