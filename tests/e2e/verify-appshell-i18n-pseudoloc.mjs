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
// PR 2 added TreePanel, ElementsList, AddJavascriptModal, AddLibraryModal,
// MoveElementModal, LinkPickerModal, and PropertyEditor's chrome strings
// (see the elementAdders.* shared namespace reused across TreePanel,
// ElementsList, and PropertyEditor's advanced adders). AddScriptModal and
// LinkPickerModal aren't exercised here yet — reaching them needs a
// script-editor/richtext-field flow that's fragile to automate reliably;
// both were verified by svelte-check/eslint plus manual browser review, and
// are good candidates to add here once that flow is worth the investment.
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

// For buttons like "+ {t(...)}" / "↑ {t(...)}" — a literal decorative prefix
// kept outside the translated string by design (see ElementsList.svelte,
// PropertyEditor.svelte's advanced adders). Only the part after the prefix
// needs to be pseudo-localized.
function assertPseudoWithPrefix(label, text, prefix) {
    if (text == null) throw new Error(`[${label}] element not found`);
    const trimmed = text.trim();
    if (!trimmed.startsWith(prefix)) {
        throw new Error(`[${label}] expected literal prefix "${prefix}": "${trimmed}"`);
    }
    assertPseudo(label, trimmed.slice(prefix.length));
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

    // --- TreePanel: filter box + view-options button ---
    // Tree *node labels* (room, Verbs, Advanced, …) come from the WASM bridge
    // (EditorController-assigned names), not our t() dictionary, so they stay
    // plain English throughout this whole section even under the xx locale —
    // only TreePanel's own chrome (buttons/menus/placeholders) is pseudo here.
    assertPseudo('TreePanel filter placeholder', await page.getAttribute('input[placeholder]', 'placeholder'));
    assertPseudo('TreePanel filter aria-label', await page.getAttribute('input[aria-label]', 'aria-label'));
    const viewOptionsBtn = page.locator('button[aria-label]').first();
    assertPseudo('TreePanel view-options title', await viewOptionsBtn.getAttribute('title'));
    await viewOptionsBtn.click();
    assertPseudo('TreePanel "Show Library Elements" menu item', await page.textContent('.absolute button'));
    await page.keyboard.press('Escape');

    // --- TreePanel context menu (via "room", the only movable node in a
    // fresh draft) — exercises elementAdders.* reuse + treePanel.deleteNamed
    // interpolation + common.moveTo/cut/copy, then opens MoveElementModal. ---
    await page.evaluate(() => {
        const roomText = Array.from(document.querySelectorAll('span')).find(el => el.textContent.trim() === 'room');
        const control = roomText.closest('[data-part="branch-control"]') ?? roomText.parentElement;
        control.querySelector('.node-actions button').click();
    });
    await page.waitForSelector('.tree-dropdown', { timeout: 10000 });
    const treeMenuItems = await page.$$eval('.tree-dropdown button', els => els.map(el => el.textContent ?? ''));
    if (treeMenuItems.length !== 10) throw new Error(`TreePanel context menu: expected 10 items (room, movable), found ${treeMenuItems.length}`);
    for (const text of treeMenuItems) assertPseudo('TreePanel context menu item', text);
    await page.click('.tree-dropdown button >> nth=6'); // "Move to…" — 7th item, see the 10-item assertion above
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('MoveElementModal heading', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('MoveElementModal "New parent" label', await page.textContent('div[role="dialog"] span >> nth=0'));
    const moveModalButtons = await page.$$eval('div[role="dialog"] button', els => els.map(el => el.textContent ?? ''));
    for (const text of moveModalButtons) assertPseudo('MoveElementModal button', text);
    await closeDialog();

    // --- ElementsList — via the always-present "Verbs" header node ---
    await page.click('text="Verbs"');
    await page.waitForSelector('.flex.items-center.gap-1.mb-2 button', { timeout: 10000 });
    assertPseudoWithPrefix('ElementsList add button', await page.textContent('.flex.items-center.gap-1.mb-2 button'), '+ ');
    assertPseudo('ElementsList "No items" copy', await page.textContent('p.italic'));

    // --- Advanced adders (PropertyEditor.ALL_ADVANCED_ADDERS, shared
    // elementAdders.* keys, fixed order: Function/Timer/Walkthrough/Library/
    // Template/DynamicTemplate/Type/JavaScript) — also opens
    // AddLibraryModal/AddJavascriptModal (indices 3 and 7). ---
    await page.click('text="Advanced" >> nth=0');
    const advancedAddersScope = '.flex.flex-col.items-start.gap-1\\.5.px-3.py-3';
    await page.waitForSelector(`${advancedAddersScope} button`, { timeout: 10000 });
    const advancedAdders = await page.$$eval(`${advancedAddersScope} button`, els => els.map(el => el.textContent ?? ''));
    if (advancedAdders.length !== 8) throw new Error(`Advanced adders: expected 8 "+ Add …" buttons, found ${advancedAdders.length}`);
    for (const text of advancedAdders) assertPseudoWithPrefix('Advanced adder button', text, '+ ');

    await page.click(`${advancedAddersScope} button >> nth=3`); // Library
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('AddLibraryModal heading', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('AddLibraryModal help text', await page.textContent('div[role="dialog"] span >> nth=0'));
    assertPseudo('AddLibraryModal "No file chosen"', await page.textContent('div[role="dialog"] span >> nth=1'));
    const libModalButtons = await page.$$eval('div[role="dialog"] button', els => els.map(el => el.textContent ?? ''));
    for (const text of libModalButtons) assertPseudo('AddLibraryModal button', text);
    // Not closeDialog(): this modal's own "Upload…" button (AddLibraryModal.svelte)
    // comes before Cancel in DOM order, unlike AddElementModal/AssetManagerModal/
    // SettingsModal where the dismiss button is genuinely first.
    await page.click('div[role="dialog"] button >> nth=1');
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });

    await page.click(`${advancedAddersScope} button >> nth=7`); // JavaScript
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('AddJavascriptModal heading', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('AddJavascriptModal help text', await page.textContent('div[role="dialog"] span >> nth=0'));
    // Scoped to the modal's own footer (Cancel/Add JavaScript), not AssetPicker's
    // upload button — AssetPicker.svelte is a separate component, PR 3 scope.
    const jsModalButtons = await page.$$eval('div[role="dialog"] .flex.justify-end.gap-2 button', els => els.map(el => el.textContent ?? ''));
    if (jsModalButtons.length !== 2) throw new Error(`AddJavascriptModal footer: expected 2 buttons, found ${jsModalButtons.length}`);
    for (const text of jsModalButtons) assertPseudo('AddJavascriptModal button', text);
    // Same reason as AddLibraryModal above: AssetPicker's own "Upload…" button
    // (inside AddJavascriptModal.svelte) is first, Cancel is second.
    await page.click('div[role="dialog"] button >> nth=1');
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });

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
