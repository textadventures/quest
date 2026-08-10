// Pseudo-localization smoke test for the homegrown i18n mechanism (src/AppShell/src/lib/i18n).
//
// static/locales/xx.json wraps every English value in brackets (e.g. "[Báck]")
// and is deliberately excluded from SUPPORTED_LOCALES, so it never appears in
// the real Settings language dropdown — this script sets it directly via
// localStorage (questviva-ui-language) to walk every string PR 1 converted
// and confirm each is actually routed through t()/tPlural(), not still
// hardcoded. Extend the screens below as later PRs convert more files.
//
// A string that renders in plain English means its call site was never
// converted (still hardcoded). A string that renders as a raw "dotted.key"
// means a mismatch between the call site's key and static/locales/en.json.
// Nested brackets (e.g. "[Ádd [Róóm]]") are expected and fine — that's just
// addElementModal.addHeading interpolating an already-pseudo-localized label.
//
// Toolbar.svelte's buttons are selected by position within its one <header>
// (verified empirically: Back, Forward, BackToHome, Add, Delete, ManageAssets,
// Undo, Redo, RawXmlView, File, Preview, More — in that DOM order) rather
// than by title text, since every title here is pseudo-mutated and therefore
// not stable across xx.json regenerations.
//
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5180';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

function assertPseudo(label, text) {
    if (text == null) throw new Error(`[${label}] element not found`);
    const trimmed = text.trim();
    if (!/^\[.*\]$/.test(trimmed)) {
        throw new Error(`[${label}] not pseudo-localized (still English, or a raw key?): "${trimmed}"`);
    }
    console.log(`PASS: [${label}] "${trimmed}"`);
}

async function assertAllTitlesPseudo(label, scopeSelector) {
    const titles = await page.$$eval(`${scopeSelector} [title]`, els => els.map(el => el.getAttribute('title')).filter(t => t));
    if (titles.length === 0) throw new Error(`[${label}] no [title] elements found under "${scopeSelector}" — selector assumption broke`);
    for (const title of titles) assertPseudo(`${label}: title="${title.slice(0, 40)}"`, title);
}

// All three of our converted modals (AddElementModal, AssetManagerModal,
// SettingsModal) put their dismiss/cancel button first in DOM order.
async function closeDialog() {
    await page.click('div[role="dialog"] button >> nth=0');
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });
}

async function run() {
    // --- Home (Play tab) — set pseudo-locale, then hard-reload so
    // +layout.svelte's initI18n() picks it up from localStorage. ---
    await page.goto(`${baseUrl}/`);
    await page.evaluate(() => localStorage.setItem('questviva-ui-language', 'xx'));
    await page.reload();

    await page.waitForSelector('button.home-header-link', { timeout: 30000 });
    assertPseudo('HomeHeader Settings button (Play tab)', await page.getAttribute('button.home-header-link', 'title'));

    // --- SettingsModal ---
    await page.click('button.home-header-link');
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('SettingsModal heading', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('SettingsModal Close button', await page.textContent('div[role="dialog"] button >> nth=0'));
    assertPseudo('SettingsModal Language label', await page.textContent('div[role="dialog"] label'));
    const languageOptionText = (await page.textContent('div[role="dialog"] option'))?.trim();
    if (languageOptionText !== 'English') {
        throw new Error(`SettingsModal language option should stay "English" (Intl.DisplayNames, not our dictionary) but was "${languageOptionText}"`);
    }
    console.log(`PASS: [SettingsModal language option] untranslated native name "${languageOptionText}"`);
    await closeDialog();

    // --- Create tab — same Settings trigger, different route ---
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button.home-header-link', { timeout: 30000 });
    assertPseudo('HomeHeader Settings button (Create tab)', await page.getAttribute('button.home-header-link', 'title'));

    // --- Reach the editor via a local draft, to exercise Toolbar.svelte ---
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', 'i18n Pseudoloc Test');
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    // .toolbar-icon-btn only exists in Toolbar.svelte — waiting on the generic
    // "header [title]" here can transiently match HomeHeader's still-mounted
    // <header> (with DownloadButton's own [title]) mid-navigation to /edit.
    await page.waitForSelector('header .toolbar-icon-btn', { timeout: 30000 });
    await assertAllTitlesPseudo('Toolbar', 'header');

    // --- AddElementModal — opened via the Add dropdown (4th header button) ---
    await page.click('header button >> nth=3');
    await page.waitForSelector('div.absolute button', { timeout: 10000 });
    await page.click('div.absolute button >> nth=0'); // "Add Room" at the tree root
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('AddElementModal heading', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('AddElementModal name label', await page.textContent('div[role="dialog"] label'));
    assertPseudo('AddElementModal name placeholder', await page.getAttribute('div[role="dialog"] input', 'placeholder'));
    const addModalButtons = await page.$$eval('div[role="dialog"] button', els => els.map(el => el.textContent ?? ''));
    for (const text of addModalButtons) assertPseudo('AddElementModal button', text);
    await closeDialog();

    // --- AssetManagerModal — opened via the toolbar's image icon (6th header button) ---
    await page.click('header button >> nth=5');
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('AssetManagerModal heading', await page.textContent('div[role="dialog"] h2'));
    const assetModalButtons = await page.$$eval('div[role="dialog"] button', els => els.map(el => el.textContent ?? ''));
    for (const text of assetModalButtons) assertPseudo('AssetManagerModal button', text);
    assertPseudo('AssetManagerModal "No assets yet" copy', await page.textContent('div[role="dialog"] p'));
    await closeDialog();

    // --- Search page — validates whichever branch (results / no games / load error) renders ---
    await page.goto(`${baseUrl}/play/search?q=quest`);
    await page.waitForSelector('form', { timeout: 30000 });
    assertPseudo('Search back-to-Play link', await page.textContent('a.anchor'));
    assertPseudo('Search input placeholder', await page.getAttribute('input[type="search"]', 'placeholder'));
    assertPseudo('Search input title (hint)', await page.getAttribute('input[type="search"]', 'title'));
    assertPseudo('Search submit button', await page.textContent('form button[type="submit"]'));
    await page.waitForSelector('p.text-surface-400, p.text-error-500', { timeout: 20000 });
    assertPseudo('Search result/status message', await page.textContent('p.text-surface-400, p.text-error-500'));

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-i18n-pseudoloc-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
