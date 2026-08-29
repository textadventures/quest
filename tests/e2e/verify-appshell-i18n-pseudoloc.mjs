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
// PR 4 added banners/modals: DownloadButton, LibraryElementBanner, and
// PublishModal are exercised below. BackupBanner, UpdateBanner,
// FileChangedExternallyBanner, and LibraryReloadBanner are NOT — each needs
// state that's impractical to fake reliably from a dev-server session
// (a real save-count threshold, live update-catalog data, Electron-only file
// watching, and a genuine library edit, respectively). All four were verified
// by svelte-check/eslint plus manual code review instead.
//
// PR 5 added the Home/Play/Create routes: HomeTabs, PlayCatalog's
// unconditional chrome (search bar, "Open a game file…"), and open/+page.svelte's
// h1/section labels/Import-help popover/create-form. GameCard/RecentGameCard's
// "by {author}"/Gamebook badge, PlayCatalog's category grid and "Recently
// Played" section, and the play/[id] and play/category/[slug] detail routes
// all depend on live textadventures.co.uk catalog data or prior-session
// history that a fresh dev-server context doesn't have — same
// honest-scope-boundary treatment as PR 4's banners, these are verified via
// svelte-check/eslint plus manual browser review instead of asserted here.
//
// Run against a dev server started with:
//   PUBLIC_SHOW_HOME=true npm --prefix src/AppShell run dev -- --port 5180
import { chromium } from './lib/tracked-chromium.mjs';

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

    // --- HomeTabs (PR 5) — Play/Create nav labels ---
    const homeTabLabels = await page.$$eval('nav a', els => els.map(el => el.textContent ?? ''));
    if (homeTabLabels.length !== 2) throw new Error(`HomeTabs: expected 2 nav links, found ${homeTabLabels.length}`);
    for (const text of homeTabLabels) assertPseudo('HomeTabs nav link', text);

    // --- PlayCatalog (PR 5) — chrome that renders unconditionally, i.e. not
    // dependent on the live textadventures.co.uk catalog fetch having
    // succeeded or on any Recently Played history existing in this fresh
    // browser context. Category grid content (GameCard "by {author}"/
    // Gamebook badge, "See all", category/game detail pages) depends on that
    // live catalog data, so — same honest-scope-boundary treatment as PR 4's
    // banners — those are verified via svelte-check/eslint plus manual
    // browser review instead of asserted here. ---
    await page.waitForSelector('input[type="search"]', { timeout: 15000 });
    assertPseudo('PlayCatalog search placeholder', await page.getAttribute('input[type="search"]', 'placeholder'));
    assertPseudo('PlayCatalog search title (hint)', await page.getAttribute('input[type="search"]', 'title'));
    assertPseudo('PlayCatalog search button', await page.textContent('form button[type="submit"]'));
    assertPseudo('PlayCatalog "Open a game file…" button', await page.textContent('button.preset-outlined-surface-500.whitespace-nowrap'));

    // --- DownloadButton — HomeHeader's compact "Desktop app" dropdown (only
    // rendered outside Electron, which this dev-server check always is). ---
    const downloadBtn = page.locator('.download-dropdown button').first();
    await downloadBtn.waitFor({ timeout: 15000 });
    assertPseudo('DownloadButton title', await downloadBtn.getAttribute('title'));
    assertPseudo('DownloadButton aria-label', await downloadBtn.getAttribute('aria-label'));
    await downloadBtn.click();
    await page.waitForSelector('.download-dropdown .absolute', { timeout: 10000 });
    assertPseudo('DownloadButton version line', await page.textContent('.download-dropdown .absolute p'));
    const downloadLinks = page.locator('.download-dropdown .absolute a');
    assertPseudo('DownloadButton "All downloads" link', await downloadLinks.last().textContent());
    await downloadBtn.click(); // close the dropdown again

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

    // --- open/+page.svelte chrome (PR 5) — h1, section labels, the Import
    // button + its help popover, and the create-form labels below. All of
    // these render unconditionally on a fresh browser-build /open (no
    // server, no FSA), unlike Recent/local-drafts which depend on prior state. ---
    // h1.text-3xl (not a bare "h1") — HomeHeader has its own "Quest Viva" <h1>
    // (a brand name, deliberately left untranslated) earlier in the DOM.
    await page.waitForSelector('h1.text-3xl', { timeout: 30000 });
    assertPseudo('open/+page title', await page.textContent('h1.text-3xl'));
    assertPseudo('open/+page "Open existing game" label', await page.textContent('p.self-start >> nth=0'));
    // Scoped to the Import row's own container — a generic text/class match risks
    // catching HomeHeader's DownloadButton (also pseudo-bracketed) earlier in the DOM.
    assertPseudo('open/+page Import game file button', await page.textContent('.flex.items-center.gap-2.w-full > button.preset-filled-primary-500'));
    assertPseudo('open/+page "About importing games" aria-label', await page.getAttribute('.import-help button', 'aria-label'));
    await page.click('.import-help button');
    const importHelpLines = await page.$$eval('.import-help .absolute p', els => els.map(el => el.textContent ?? ''));
    if (importHelpLines.length !== 2) throw new Error(`open/+page import help: expected 2 lines, found ${importHelpLines.length}`);
    for (const text of importHelpLines) assertPseudo('open/+page import help line', text);
    await page.click('.import-help button'); // close the popover again

    // --- Reach the editor via a local draft, to exercise Toolbar.svelte.
    // Selectors below are locale-agnostic (type/name attributes, DOM position)
    // rather than by translated text, since openPage.* strings are pseudo here. ---
    await page.waitForSelector('input[type="text"]', { timeout: 30000 });
    await page.fill('input[type="text"]', 'i18n Pseudoloc Test');
    await page.waitForSelector('input[name="gametype"]', { timeout: 10000 });
    // Scoped to its own wrapper — "Open existing game" (self-start, earlier in
    // the DOM) shares the exact same p.text-sm.text-surface-600-400 classes,
    // and a bare class match would silently pick that up instead.
    assertPseudo('open/+page "Game type" label', await page.textContent('.flex.flex-col.gap-1 > p.text-sm'));
    const gameTypeLabels = await page.$$eval('input[name="gametype"] ~ span', els => els.map(el => el.textContent ?? ''));
    if (gameTypeLabels.length !== 2) throw new Error(`open/+page game type radios: expected 2 labels, found ${gameTypeLabels.length}`);
    for (const text of gameTypeLabels) assertPseudo('open/+page game type radio label', text);
    // "Create local draft" is the last filled-primary button on this branch of
    // /open (after the earlier "Import game file" button) — not selected by
    // its own (now pseudo) text, same reasoning as the DOM-position selectors
    // used throughout the rest of this script.
    assertPseudo('open/+page Create-local-draft button', await page.locator('button.preset-filled-primary-500').last().textContent());
    assertPseudo('open/+page "Stored in this browser only" copy', await page.textContent('.flex-col.gap-1.flex-1 p.text-xs'));
    await page.locator('button.preset-filled-primary-500').last().click();
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
    // Scoped to #workspace-content (everything below Toolbar) — #2139 added
    // aria-label to Toolbar's Back/Forward/BackToHome buttons too, and those
    // render first in the DOM (and start disabled, with no history yet), so
    // an unscoped `button[aria-label]` locator grabs one of those instead.
    const viewOptionsBtn = page.locator('#workspace-content button[aria-label]').first();
    assertPseudo('TreePanel view-options title', await viewOptionsBtn.getAttribute('title'));
    await viewOptionsBtn.click();
    assertPseudo('TreePanel "Show Library Elements" menu item', await page.textContent('[role="menu"] button'));
    await page.keyboard.press('Escape');

    // --- TreePanel context menu (via "room", the only movable node in a
    // fresh draft) — exercises elementAdders.* reuse + treePanel.deleteNamed
    // interpolation + common.moveTo/cut/copy, then opens MoveElementModal.
    // 13 items: the 11 at HEAD plus the two new expand/collapse-all-below
    // entries; "Move to…" is index 7. ---
    await page.evaluate(() => {
        const roomText = Array.from(document.querySelectorAll('span')).find(el => el.textContent.trim() === 'room');
        const control = roomText.closest('[data-part="branch-control"]') ?? roomText.parentElement;
        control.querySelector('.node-actions button').click();
    });
    await page.waitForSelector('.tree-dropdown', { timeout: 10000 });
    const treeMenuItems = await page.$$eval('.tree-dropdown button', els => els.map(el => el.textContent ?? ''));
    if (treeMenuItems.length !== 13) throw new Error(`TreePanel context menu: expected 13 items (room, movable), found ${treeMenuItems.length}`);
    for (const text of treeMenuItems) assertPseudo('TreePanel context menu item', text);
    await page.click('.tree-dropdown button >> nth=7'); // "Move to…" — 8th item, see the 13-item assertion above
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('MoveElementModal heading', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('MoveElementModal "New parent" label', await page.textContent('div[role="dialog"] span >> nth=0'));
    const moveModalButtons = await page.$$eval('div[role="dialog"] button', els => els.map(el => el.textContent ?? ''));
    for (const text of moveModalButtons) assertPseudo('MoveElementModal button', text);
    await closeDialog();

    // --- ElementsList — via the "Commands" header node (chosen over "Verbs":
    // verbsEditor.header IS in xx.json so it renders "[Vérbs]", while
    // treePanel.header.commands isn't and falls back to English). The "game"
    // node starts collapsed (#827), so expand it first. ---
    await page.locator('[data-part="branch-control"]:has-text("game") >> button').first().click();
    await page.click('text="Commands"');
    await page.waitForSelector('.flex.items-center.gap-1.mb-2 button', { timeout: 10000 });
    assertPseudoWithPrefix('ElementsList add button', await page.textContent('.flex.items-center.gap-1.mb-2 button'), '+ ');
    assertPseudo('ElementsList "No items" copy', await page.textContent('p.italic'));

    // --- Advanced adders (PropertyEditor.ALL_ADVANCED_ADDERS, shared
    // elementAdders.* keys, fixed order: Function/Timer/Walkthrough/Library/
    // Template/DynamicTemplate/Type/JavaScript) — also opens
    // AddLibraryModal/AddJavascriptModal (indices 3 and 7). Select the
    // "_advanced" header: its label is common.advanced ("[Ádváncéd]" under xx),
    // so pick it as the only tree branch whose text starts with "[" rather
    // than clicking English text that can't match under the pseudo-locale. ---
    const advancedBranch = page.locator('[data-part="branch-control"]').filter({ hasText: /^\s*\[/ });
    await advancedBranch.click();
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

    // --- AttributesEditor — via "room" (tab captions like "Attributes" are
    // WASM-bridge-sourced, not ours, so they stay plain English and are safe
    // to click on by text even under the xx locale). ---
    await page.click('span:text-is("room")');
    await page.click('button:has-text("Attributes")');
    await page.waitForSelector('[data-attr="isroom"]', { timeout: 10000 });
    // #2144 removed PropertyEditor's own static "Properties" fallback header
    // (same span classes), so AttributesEditor's own headers now start at 0.
    const attrHeaders = await page.$$eval('span.font-semibold.uppercase', els => els.map(el => el.textContent ?? ''));
    if (attrHeaders.length < 2) throw new Error(`AttributesEditor: expected 2+ headers, found ${attrHeaders.length}`);
    assertPseudo('AttributesEditor "Inherited types" header', attrHeaders[0]);
    assertPseudo('AttributesEditor "Attributes" header', attrHeaders[1]);
    const attrThs = await page.$$eval('th', els => els.map(el => el.textContent?.trim() ?? '').filter(Boolean));
    if (attrThs.length === 0) throw new Error('AttributesEditor: no table headers found');
    for (const text of attrThs) assertPseudo('AttributesEditor table header', text);
    await page.click('[data-attr="isroom"]');
    await page.waitForSelector('div.justify-between span', { timeout: 10000 });
    assertPseudo('AttributesEditor "Assignment" header', await page.textContent('div.justify-between span'));
    // "Add Change Script" (or "Go to Change Script") is the first button after the
    // Assignment header — scoped so it can't match Toolbar's own pseudo buttons.
    assertPseudo('AttributesEditor Add/Go-to Change Script button', await page.textContent('div.justify-between + div button'));

    // --- ExitsEditor — via "room"'s Exits tab ---
    await page.click('button:has-text("Exits")');
    await page.waitForSelector('button:has-text("+")', { timeout: 10000 });
    assertPseudoWithPrefix('ExitsEditor add-exit button', await page.textContent('button:has-text("+")'), '+ ');
    assertPseudo('ExitsEditor "No exits" copy', await page.textContent('p.italic'));
    // The 9-cell compass grid's first real (non-empty) direction cell — click
    // it to open the create-exit panel and check its own chrome.
    await page.click('.grid-cols-3 button >> nth=0');
    await page.waitForSelector('label:has-text("[")', { timeout: 10000 });
    const exitsHeading = (await page.textContent('.border-primary-300-700 span')) ?? '';
    if (!exitsHeading.startsWith('[') || !exitsHeading.includes(':')) {
        throw new Error(`ExitsEditor create-exit heading not pseudo-localized: "${exitsHeading}"`);
    }
    console.log(`PASS: [ExitsEditor create-exit heading] "${exitsHeading}"`);
    assertPseudo('ExitsEditor "Also create the return exit" label', await page.textContent('label:has-text("[")'));
    // Scoped to the "Create exit" / "Create a look exit instead" action row —
    // excludes the panel's own bare "✕" close glyph, which (like every other
    // ×/✕/↑/↓ glyph-only button across the app) is deliberately not translated.
    const exitPanelButtons = await page.$$eval('.border-primary-300-700 .gap-3 button', els => els.map(el => el.textContent ?? ''));
    if (exitPanelButtons.length !== 2) throw new Error(`ExitsEditor create-exit actions: expected 2 buttons, found ${exitPanelButtons.length}`);
    for (const text of exitPanelButtons) assertPseudo('ExitsEditor create-exit panel action', text);

    // --- ScriptEditor + AddScriptModal — via "room"'s Scripts tab ---
    await page.click('button:has-text("Scripts")');
    await page.waitForSelector('button:has-text("+")', { timeout: 10000 });
    await page.click('button:has-text("+") >> nth=0'); // first "+ Add script" row
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('AddScriptModal title', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('AddScriptModal filter placeholder', await page.getAttribute('div[role="dialog"] input[type="text"]', 'placeholder'));
    const addScriptFooter = await page.$$eval('div[role="dialog"] .border-t button', els => els.map(el => el.textContent ?? ''));
    for (const text of addScriptFooter) assertPseudo('AddScriptModal footer button', text);
    // Quick-add shortcuts (Print/Inventory/Move/Show/Hide/If) — their labels come from
    // the <common> field on each command's .aslx editor definition (Engine/Core/
    // CoreEditorScripts*.aslx), the same WASM-bridge source as every other script-command
    // label, so — like TreePanel node labels and command names elsewhere in this file —
    // they stay plain English even under the xx pseudo-locale; only checked for presence
    // here. Click the last one (If) to exercise ScriptEditor's if/then/else rendering below.
    const shortcutButtons = page.locator('div[role="dialog"] .rounded-full');
    const shortcutCount = await shortcutButtons.count();
    if (shortcutCount === 0) throw new Error('AddScriptModal: no quick-add shortcut buttons found');
    await shortcutButtons.last().click();
    await page.waitForSelector('div[role="dialog"]', { state: 'detached', timeout: 10000 });

    const ifBlock = page.locator('.border.border-surface-200-800.rounded.mb-1').first();
    const ifThenSpans = await ifBlock.locator('span.select-none').allTextContents();
    if (ifThenSpans.length === 0) throw new Error('ScriptEditor: no if/then keyword spans found');
    for (const text of ifThenSpans) assertPseudo('ScriptEditor if/then keyword', text);
    // Filtered to "+"-prefixed buttons only — ifBlock's outer wrapper also contains the
    // row's Move-up/Move-down/Delete (↑/↓/×) buttons, which a plain .first()/.last() would
    // catch instead (their visible text is just the glyph, title attribute isn't textContent).
    const plusButtons = ifBlock.locator('button', { hasText: '+' });
    assertPseudoWithPrefix('ScriptEditor "+ Add script" (nested)', (await plusButtons.first().textContent()) ?? '', '+ ');
    // "+ else" is the last "+"-prefixed button inside the if-block — click it, then
    // re-read the if/then spans (now includes "else" as the last one).
    await plusButtons.last().click();
    await page.waitForTimeout(200);
    const elseSpans = await ifBlock.locator('span.select-none').allTextContents();
    assertPseudo('ScriptEditor "else" keyword', elseSpans[elseSpans.length - 1]);
    // Select the if-block's own checkbox to reveal the Cut/Copy/Delete/Move/count toolbar,
    // rendered as the block's next sibling <div>.
    await ifBlock.locator('input[type="checkbox"]').click();
    const scriptToolbar = ifBlock.locator('xpath=following-sibling::div[1]');
    await scriptToolbar.waitFor({ timeout: 10000 });
    const scriptToolbarButtons = await scriptToolbar.locator('button').allTextContents();
    if (scriptToolbarButtons.length === 0) throw new Error('ScriptEditor: selection toolbar not found');
    for (const text of scriptToolbarButtons) {
        // "↑ Move up" / "↓ Move down" keep their arrow as a literal prefix outside t(),
        // same convention as the "+ Add script" buttons throughout this app.
        const arrowPrefix = text.trim().startsWith('↑') ? '↑ ' : text.trim().startsWith('↓') ? '↓ ' : null;
        if (arrowPrefix) assertPseudoWithPrefix('ScriptEditor selection-toolbar button', text, arrowPrefix);
        else assertPseudo('ScriptEditor selection-toolbar button', text);
    }
    assertPseudo('ScriptEditor "N selected" copy', await scriptToolbar.locator('span').last().textContent());

    // --- VerbsEditor — via "player" (only "object" elements get a Verbs tab;
    // rooms don't) — static labels only, no Combobox interaction. "player"
    // lives under the "room" branch, which now starts expanded all the way
    // down to the player (#827), so it's already visible. ---
    await page.click('span:text-is("player")');
    await page.click('button:has-text("Verbs")');
    await page.waitForSelector('table', { timeout: 10000 });
    const verbHeaders = await page.$$eval('span.font-semibold.uppercase', els => els.map(el => el.textContent ?? ''));
    if (verbHeaders.length === 0) throw new Error('VerbsEditor: no section headers found');
    for (const text of verbHeaders) assertPseudo('VerbsEditor header', text);
    const verbThs = await page.$$eval('th', els => els.map(el => el.textContent?.trim() ?? '').filter(Boolean));
    for (const text of verbThs) assertPseudo('VerbsEditor table header', text);
    assertPseudo('VerbsEditor "No verbs added yet" copy', await page.textContent('td.italic'));
    // Scoped to VerbsEditor's own bottom row, not Toolbar's pseudo buttons elsewhere on screen.
    assertPseudo('VerbsEditor add-verb button', await page.textContent('.border-t.border-surface-200-800.flex.flex-col.gap-1 button'));
    assertPseudo('VerbsEditor select-a-verb prompt', await page.textContent('p.italic'));

    // --- LibraryElementBanner — via a library-origin verb. Library verbs are
    // hidden from the tree by default, so enable "Show Library Elements" first
    // (TreePanel's view-options menu — the same one checked earlier). Scoped to
    // [role="menu"] rather than a bare ".absolute" class, which by this point in
    // the flow also matches unrelated hover-action overlays in other components. ---
    await page.locator('#workspace-content button[aria-label]').first().click();
    await page.click('[role="menu"] button');
    // With libraries shown, the Verbs header has children — but CoreCommands.aslx
    // alone defines 20+ consecutive <verb> elements including "drink", so
    // groupLibraryChildren folds them into a nested, separately-collapsed
    // "librarygroup" node (see TreePanel.svelte). A single expand click on the
    // Verbs branch itself only opens the outer level; "drink" stays hidden one
    // level deeper. Use the node's own "Expand all below" context action
    // instead, so it doesn't matter how many levels are actually collapsed.
    await page.evaluate(() => {
        // Scoped to branch-control spans, not a bare span search — "Vérbs" is
        // also the room's own VerbsEditor tab header text (same locale key),
        // which would otherwise be an ambiguous first match.
        const verbsText = Array.from(document.querySelectorAll('[data-part="branch-control"] span'))
            .find(el => el.textContent.trim() === '[Vérbs]');
        const control = verbsText?.closest('[data-part="branch-control"]');
        control?.querySelector('.node-actions button')?.click();
    });
    await page.waitForSelector('.tree-dropdown', { timeout: 10000 });
    await page.click('.tree-dropdown button:has-text("[Éxpánd áll bélów]")');
    await page.click('text="drink"');
    await page.waitForSelector('.bg-warning-100-900', { timeout: 10000 });
    assertPseudo('LibraryElementBanner message', await page.textContent('.bg-warning-100-900 span'));
    assertPseudo('LibraryElementBanner copy button', await page.textContent('.bg-warning-100-900 button'));

    // --- PublishModal — via the toolbar's File menu (9th header button; see
    // the Toolbar DOM-order comment near the top of this file). ---
    await page.click('header button >> nth=9');
    await page.waitForSelector('.absolute button', { timeout: 10000 });
    await page.click('.absolute button >> nth=1'); // "Publish…" (Backup…, Publish…, Export as single file…)
    await page.waitForSelector('div[role="dialog"]', { timeout: 10000 });
    assertPseudo('PublishModal title', await page.textContent('div[role="dialog"] h2'));
    assertPseudo('PublishModal Close button', await page.textContent('div[role="dialog"] button >> nth=0'));
    assertPseudo('PublishModal description', await page.textContent('div[role="dialog"] p'));
    assertPseudo('PublishModal "Include walkthrough" label', await page.textContent('div[role="dialog"] label'));
    assertPseudo('PublishModal Publish button', await page.textContent('div[role="dialog"] button >> nth=1'));
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
