// Manual verification for the new WasmPlayer Debugger (element browser +
// attribute override + walkthrough runner), added alongside the existing
// WebPlayer Blazor Debugger. Checks:
//   1. The Debug button stays hidden for a normal (non-preview) session.
//   2. It appears for an editor-preview session (?source=editor handoff,
//      same BroadcastChannel protocol AppShell's live preview uses).
//   3. The dialog's tabs/object list/attribute table populate correctly.
//   4. Overriding an attribute (moving the player between rooms via `parent`)
//      actually changes live game state.
//   5. A bad override expression surfaces an inline error, not a crash.
//   6. The walkthrough runner runs a walkthrough to completion.
//
// Requires the WasmPlayer dev server running locally:
//   node ../../src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();
let failed = false;

function check(label, condition) {
    console.log(`${condition ? 'PASS' : 'FAIL'}: ${label}`);
    if (!condition) failed = true;
}

// ── 1. Non-preview session: Debug button must stay hidden ──────────────────
{
    const page = await browser.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/debugger-test.aslx`);
    await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
    await page.waitForTimeout(300);
    const display = await page.$eval('#cmdDebug', el => getComputedStyle(el).display);
    check('Debug button hidden in a normal play session', display === 'none');
    await page.close();
}

// ── 2. Editor-preview session: hand off game bytes over BroadcastChannel ───
// BroadcastChannel is scoped per browser-context storage partition — each
// browser.newPage() call creates a brand-new isolated context in Playwright
// (unlike two ordinary tabs in the same real browser window/profile, which
// is what AppShell's editor + its live-preview tab actually are), so both
// pages here must share one explicit context or the channel can't bridge them.
// A generous viewport, so the movable/resizable dialog checks below have
// enough room that clamping (staying within the viewport, see
// applyDebuggerRect/wireDebuggerMoveResize) never kicks in and confounds an
// exact-pixel-delta assertion.
const context = await browser.newContext({ viewport: { width: 1400, height: 1000 } });
const previewPage = await context.newPage();
previewPage.on('pageerror', err => console.log('[pageerror]', err.message));
previewPage.on('console', msg => {
    if (msg.type() === 'error') console.log('[console.error]', msg.text());
});

const senderPage = await context.newPage();
const fixtureUrl = `${baseUrl}/e2e-fixtures/debugger-test.aslx`;
await senderPage.goto(baseUrl);

// Registers the BroadcastChannel listener *before* previewPage navigates —
// otherwise previewPage's boot IIFE can post 'ready' before anyone is
// listening (BroadcastChannel drops messages with no live listener, it
// doesn't queue them), and the handoff below would hang forever.
const handoffDone = senderPage.evaluate(async (url) => {
    const bytes = new Uint8Array(await (await fetch(url)).arrayBuffer());
    await new Promise((resolve) => {
        const bc = new BroadcastChannel('quest-preview');
        bc.onmessage = ({ data }) => {
            if (data.type === 'ready') {
                bc.postMessage({ type: 'game', bytes, filename: 'debugger-test.aslx' });
                resolve();
            }
        };
    });
}, fixtureUrl);

await previewPage.goto(`${baseUrl}/?source=editor`);
await handoffDone;

await previewPage.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
await previewPage.waitForTimeout(300);

const previewDisplay = await previewPage.$eval('#cmdDebug', el => getComputedStyle(el).display);
check('Debug button visible in an editor-preview session', previewDisplay !== 'none');

// ── 3. Open the dialog, check tabs ──────────────────────────────────────────
await previewPage.click('#cmdDebug');
await previewPage.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });

const tabLabels = await previewPage.$$eval('#qv-debugger-tabs button', els => els.map(el => el.textContent));
check('Tabs include Walkthrough/Objects/Game', ['Walkthrough', 'Objects', 'Game'].every(t => tabLabels.includes(t)));

// The dialog used to visibly resize on every tab switch (max-height instead
// of a fixed height on the two panes) — Walkthrough starts selected with
// nothing chosen yet (shortest content); Objects, once player/parent is
// selected below, renders a table + override panel (much taller content).
// The dialog's own box should stay put regardless.
const sizeOnWalkthroughTab = await previewPage.$eval('#questVivaDebugger', el => {
    const r = el.getBoundingClientRect();
    return { width: r.width, height: r.height };
});

// ── 4. Objects tab → select player → override parent to move rooms ─────────
await previewPage.click('#qv-debugger-tabs button:text("Objects")');
const objectItems = await previewPage.$$eval('#qv-debugger-list [data-item]', els => els.map(el => el.dataset.item));
check('Objects list includes player/kitchen/livingroom', ['player', 'kitchen', 'livingroom'].every(o => objectItems.includes(o)));

// The left-hand list should read as a selectable list, not a list of
// hyperlinks (no more Skeleton .anchor class).
const listItemClasses = await previewPage.$$eval('#qv-debugger-list [data-item]', els => els.map(el => el.className));
check('Object list items are not styled as hyperlinks', listItemClasses.every(c => !c.includes('anchor')));

await previewPage.click('#qv-debugger-list [data-item="player"]');
await previewPage.waitForSelector('[data-attr-row="parent"]');

// Column headers should be visually differentiated from the body rows.
const headerStyle = await previewPage.$eval('.qv-debugger-table thead th', el => {
    const s = getComputedStyle(el);
    return { textTransform: s.textTransform, borderBottomWidth: s.borderBottomWidth };
});
check(
    'Table headers are visually differentiated (uppercase + bottom border)',
    headerStyle.textTransform === 'uppercase' && parseFloat(headerStyle.borderBottomWidth) > 0
);

// player (inherits defaultplayer) has a list-valued attribute (pov_alt) —
// no simple literal syntax to write one of those back, so no override
// textbox should be offered for it.
await previewPage.waitForSelector('[data-attr-row="pov_alt"]');
await previewPage.click('[data-attr-row="pov_alt"]');
await previewPage.waitForSelector('#qv-debugger-override');
const povAltHasInput = await previewPage.$('#qv-debugger-override-input');
check('No override textbox for a list-valued attribute (pov_alt)', povAltHasInput === null);

// pov_alt is inherited from the defaultplayer type — its selected-row text
// color used to fade toward the selected blue background (opacity-60
// combined with .qv-debugger-row-selected's light contrast text), reading as
// low-contrast light-grey-on-light-blue. Selected should win over inherited.
const povAltSelectedColor = await previewPage.$eval('[data-attr-row="pov_alt"]', el => getComputedStyle(el).color);
await previewPage.click('#qv-debugger-list [data-item="kitchen"]');
await previewPage.click('#qv-debugger-list [data-item="player"]');
await previewPage.waitForSelector('[data-attr-row="pov_alt"]');
const povAltUnselectedColor = await previewPage.$eval('[data-attr-row="pov_alt"]', el => getComputedStyle(el).color);
check(
    'Selected+inherited row keeps readable (non-muted) text color',
    povAltSelectedColor !== povAltUnselectedColor
);

// Hovering a *selected* list item used to fall back to the plain hover's
// neutral grey background (higher specificity than the plain selected-blue
// rule) while keeping the selected item's light contrast-color text.
await previewPage.click('#qv-debugger-list [data-item="player"]');
await previewPage.hover('#qv-debugger-list [data-item="player"]');
const selectedHoverBackground = await previewPage.$eval(
    '#qv-debugger-list [data-item="player"]',
    el => getComputedStyle(el).backgroundColor
);
await previewPage.hover('#qv-debugger-list [data-item="kitchen"]');
const unselectedHoverBackground = await previewPage.$eval(
    '#qv-debugger-list [data-item="kitchen"]',
    el => getComputedStyle(el).backgroundColor
);
check(
    'Hovering the selected list item does not fall back to the plain (low-contrast) hover background',
    selectedHoverBackground !== unselectedHoverBackground
);

await previewPage.click('[data-attr-row="parent"]');
await previewPage.waitForSelector('#qv-debugger-override-input');

const sizeWithOverridePanelOpen = await previewPage.$eval('#questVivaDebugger', el => {
    const r = el.getBoundingClientRect();
    return { width: r.width, height: r.height };
});
check(
    'Dialog size stays constant across tabs/content (no more per-tab resize)',
    sizeOnWalkthroughTab.width === sizeWithOverridePanelOpen.width
        && sizeOnWalkthroughTab.height === sizeWithOverridePanelOpen.height
);

// The table's Value column still shows the human-readable display string
// (Element.ToString()'s "Type: name", e.g. "Object: kitchen"), but the
// override input is pre-filled from DebugDataItem.EditValue — the same
// value pre-formatted as valid script syntax (Fields.cs's s_editFormatters) —
// so it should show just the bare name, already appliable as-is.
const parentValueCell = await previewPage.$eval('[data-attr-row="parent"] td:nth-child(2)', el => el.textContent);
check('Value column shows the display form ("Object: kitchen")', /Object:\s*kitchen/i.test(parentValueCell));

const parentBefore = await previewPage.$eval('#qv-debugger-override-input', el => el.value);
check('Override input is pre-filled with the bare, appliable name ("kitchen")', parentBefore === 'kitchen');

await previewPage.fill('#qv-debugger-override-input', 'livingroom');
await previewPage.click('#qv-debugger-override-apply');
await previewPage.waitForFunction(
    () => /livingroom/i.test(document.querySelector('#qv-debugger-override-input')?.value ?? '')
);
const parentAfter = await previewPage.$eval('#qv-debugger-override-input', el => el.value);
check('Override applied: player.parent is now livingroom', /livingroom/i.test(parentAfter));

// Confirm it's a *real* game-state change, not just a UI re-render: close the
// dialog and ask the game itself where the player is.
await previewPage.click('#qv-debugger-close');
await previewPage.fill('#txtCommand', 'look');
await previewPage.press('#txtCommand', 'Enter');
await previewPage.waitForTimeout(300);
const outputText = await previewPage.$eval('#divOutput', el => el.textContent);
console.log('  output after look:', JSON.stringify(outputText).slice(0, 300));
check('The live game reflects the move (output mentions livingroom)', /livingroom/i.test(outputText));

// ── String attributes pre-fill correctly quoted ─────────────────────────────
await previewPage.click('#cmdDebug');
await previewPage.waitForSelector('#questVivaDebugger[open]');
await previewPage.click('#qv-debugger-tabs button:text("Objects")');
await previewPage.click('#qv-debugger-list [data-item="kitchen"]');
await previewPage.waitForSelector('[data-attr-row="description"]');
await previewPage.click('[data-attr-row="description"]');
await previewPage.waitForSelector('#qv-debugger-override-input');
const descriptionEditValue = await previewPage.$eval('#qv-debugger-override-input', el => el.value);
check('String attribute pre-fills quoted (kitchen.description)', descriptionEditValue === '"A kitchen."');
await previewPage.click('#qv-debugger-close');

// ── Selecting a row shouldn't reset the attribute table's scroll position ──
// player (inherits defaultplayer) has enough attributes to actually scroll.
await previewPage.click('#cmdDebug');
await previewPage.waitForSelector('#questVivaDebugger[open]');
await previewPage.click('#qv-debugger-tabs button:text("Objects")');
await previewPage.click('#qv-debugger-list [data-item="player"]');
await previewPage.waitForSelector('.qv-debugger-scroll');
await previewPage.$eval('.qv-debugger-scroll', el => { el.scrollTop = 50; });
const scrollBeforeRowClick = await previewPage.$eval('.qv-debugger-scroll', el => el.scrollTop);
// Dispatch the click directly rather than page.click() — Playwright's
// actionability checks scroll a target into view first if it isn't fully
// visible, which would confound this specific assertion (scrollTop changing
// because Playwright itself scrolled it, not because of a rendering bug).
await previewPage.$eval('[data-attr-row="type"]', el => el.click());
await previewPage.waitForSelector('#qv-debugger-override-input');
const scrollAfterRowClick = await previewPage.$eval('.qv-debugger-scroll', el => el.scrollTop);
check(
    'Clicking an attribute row preserves scroll position',
    scrollBeforeRowClick > 0 && scrollAfterRowClick === scrollBeforeRowClick
);

// ── Attribute table: sortable columns + search box ──────────────────────────
const attrColumn = colIndex => previewPage.$$eval(
    `#qv-debugger-attr-tbody tr td:nth-child(${colIndex})`,
    els => els.map(el => el.textContent)
);

const defaultOrder = await attrColumn(1);
check('Attribute column starts sorted ascending by default', JSON.stringify(defaultOrder) === JSON.stringify([...defaultOrder].sort((a, b) => a.localeCompare(b))));

await previewPage.click('[data-sort-col="attribute"]');
const descOrder = await attrColumn(1);
check('Clicking the active sort column reverses direction', JSON.stringify(descOrder) === JSON.stringify([...defaultOrder].reverse()));

const sortHeaderText = await previewPage.$eval('[data-sort-col="attribute"]', el => el.textContent);
check('Sort indicator shows on the active column', sortHeaderText.includes('▼'));

await previewPage.click('[data-sort-col="value"]');
const valueOrder = await attrColumn(2);
check('Sorting by Value column actually sorts by value', JSON.stringify(valueOrder) === JSON.stringify([...valueOrder].sort((a, b) => a.localeCompare(b))));

await previewPage.fill('#qv-debugger-attr-search', 'pov');
const filteredAttrs = await attrColumn(1);
// The filter matches on attribute *value* too (not just name), so this
// isn't just the pov_* attributes — some other attribute apparently holds
// script/text referencing "pov" in its value. Assert the search actually
// narrowed the list and definitely includes the obvious name matches,
// rather than assuming every result's own name contains "pov".
check(
    'Search box filters the attribute table',
    filteredAttrs.length > 0
        && filteredAttrs.length < defaultOrder.length
        && filteredAttrs.includes('pov_alt')
        && filteredAttrs.includes('pov_used')
);

await previewPage.fill('#qv-debugger-attr-search', 'nonexistent-attribute-xyz');
const noMatchText = await previewPage.$eval('#qv-debugger-attr-tbody', el => el.textContent);
check('Search box shows a no-match message rather than an empty table', noMatchText.includes('No matching attributes'));

// Selecting a *different* object should reset the leftover filter, or the
// new object could appear to have no attributes at all for no visible reason.
await previewPage.click('#qv-debugger-list [data-item="kitchen"]');
await previewPage.waitForSelector('#qv-debugger-attr-tbody [data-attr-row]');
const searchAfterObjectSwitch = await previewPage.$eval('#qv-debugger-attr-search', el => el.value);
check('Switching objects resets the search filter', searchAfterObjectSwitch === '');
await previewPage.click('#qv-debugger-close');

// ── 5. Bad override expression → inline error, no crash ────────────────────
await previewPage.click('#cmdDebug');
await previewPage.waitForSelector('#questVivaDebugger[open]');
await previewPage.click('#qv-debugger-tabs button:text("Objects")');
await previewPage.click('#qv-debugger-list [data-item="player"]');
await previewPage.waitForSelector('[data-attr-row="parent"]');
await previewPage.click('[data-attr-row="parent"]');
await previewPage.waitForSelector('#qv-debugger-override-input');
await previewPage.fill('#qv-debugger-override-input', 'this is not valid script syntax {{{');
await previewPage.click('#qv-debugger-override-apply');
await previewPage.waitForFunction(() => {
    const el = document.querySelector('#qv-debugger-override-error');
    return el && !el.classList.contains('hidden') && el.textContent.length > 0;
}, { timeout: 5000 });
const errorText = await previewPage.$eval('#qv-debugger-override-error', el => el.textContent);
check('Bad override shows an inline error', errorText.length > 0);
console.log('  error text:', errorText);

// ── Movable/resizable dialog, splitter, resizable columns ──────────────────
async function dragBy(page, handleSelector, dx, dy) {
    const box = await page.$eval(handleSelector, el => {
        const r = el.getBoundingClientRect();
        return { x: r.x + r.width / 2, y: r.y + r.height / 2 };
    });
    await page.mouse.move(box.x, box.y);
    await page.mouse.down();
    await page.mouse.move(box.x + dx, box.y + dy, { steps: 5 });
    await page.mouse.up();
}

const rectBefore = await previewPage.$eval('#questVivaDebugger', el => el.getBoundingClientRect().toJSON());

await dragBy(previewPage, '#qv-debugger-titlebar', -120, 60);
const rectAfterMove = await previewPage.$eval('#questVivaDebugger', el => el.getBoundingClientRect().toJSON());
check(
    'Dragging the title bar moves the dialog',
    rectAfterMove.left === rectBefore.left - 120 && rectAfterMove.top === rectBefore.top + 60
);

await dragBy(previewPage, '#qv-debugger-resize-handle', 150, 100);
const rectAfterResize = await previewPage.$eval('#questVivaDebugger', el => el.getBoundingClientRect().toJSON());
check(
    'Dragging the resize handle grows the dialog',
    rectAfterResize.width === rectAfterMove.width + 150 && rectAfterResize.height === rectAfterMove.height + 100
);

const listWidthBefore = await previewPage.$eval('#qv-debugger-list', el => el.getBoundingClientRect().width);
await dragBy(previewPage, '#qv-debugger-splitter', 80, 0);
const listWidthAfter = await previewPage.$eval('#qv-debugger-list', el => el.getBoundingClientRect().width);
check('Dragging the splitter resizes the object list pane', Math.round(listWidthAfter - listWidthBefore) === 80);

// The table header (containing the resize handle) isn't sticky, so if the
// table happens to be scrolled a long way down — e.g. clicking "parent" a
// few steps ago auto-scrolled the (long, alphabetically-sorted) player
// attribute list a good way down to bring that row into view — the header
// itself can be scrolled out of the visible viewport entirely. Reset scroll
// first so the handle is actually reachable, same idea as the "type" row
// click earlier using el.click() directly to sidestep Playwright's
// scroll-into-view.
await previewPage.$eval('.qv-debugger-scroll', el => { el.scrollTop = 0; });

const attrColWidthBefore = await previewPage.$eval('[data-attr-row] td:first-child', el => el.getBoundingClientRect().width);
await dragBy(previewPage, '[data-resize-col="attribute"]', 60, 0);
const attrColWidthAfter = await previewPage.$eval('[data-attr-row] td:first-child', el => el.getBoundingClientRect().width);
check('Dragging a column handle resizes that column', Math.round(attrColWidthAfter - attrColWidthBefore) === 60);

// Reopening should keep the same custom rect (position/size survive a
// close/reopen, same idea as the walkthrough Running… state does).
await previewPage.click('#qv-debugger-close');
await previewPage.click('#cmdDebug');
await previewPage.waitForSelector('#questVivaDebugger[open]');
const rectAfterReopen = await previewPage.$eval('#questVivaDebugger', el => el.getBoundingClientRect().toJSON());
check(
    'Dialog position/size persist across close/reopen',
    rectAfterReopen.left === rectAfterResize.left
        && rectAfterReopen.top === rectAfterResize.top
        && rectAfterReopen.width === rectAfterResize.width
        && rectAfterReopen.height === rectAfterResize.height
);

// ── 6. Walkthrough runner ───────────────────────────────────────────────────
await previewPage.click('#qv-debugger-tabs button:text("Walkthrough")');
const walkthroughItems = await previewPage.$$eval('#qv-debugger-list [data-item]', els => els.map(el => el.dataset.item));
check('Walkthrough list includes "basic"', walkthroughItems.includes('basic'));

await previewPage.click('#qv-debugger-list [data-item="basic"]');
await previewPage.waitForSelector('[data-run-walkthrough="basic"]');

// Cancel should be hidden until a walkthrough is actually running — it was
// previously shown at all times, a dead control before/after a run.
const cancelHiddenBeforeRun = await previewPage.$eval('[data-cancel-walkthrough]', el => el.classList.contains('hidden'));
check('Cancel is hidden before a walkthrough is running', cancelHiddenBeforeRun);

await previewPage.click('[data-run-walkthrough="basic"]');
const cancelVisibleWhileRunning = await previewPage.$eval('[data-cancel-walkthrough]', el => !el.classList.contains('hidden'));
check('Cancel appears while the walkthrough is running', cancelVisibleWhileRunning);

await previewPage.waitForFunction(() => {
    const el = document.querySelector('[data-walkthrough-status]');
    return el && el.textContent && el.textContent !== 'Running…';
}, { timeout: 15000 });
const walkthroughStatus = await previewPage.$eval('[data-walkthrough-status]', el => el.textContent);
check('Walkthrough runs to completion', walkthroughStatus === 'Done');

const cancelHiddenAfterRun = await previewPage.$eval('[data-cancel-walkthrough]', el => el.classList.contains('hidden'));
check('Cancel is hidden again once the walkthrough finishes', cancelHiddenAfterRun);

// ── Closing and reopening the dialog mid-run must not lose Cancel ──────────
// #qv-debugger-detail is fully rebuilt every time the dialog (re)opens, which
// used to silently drop whatever Run/Cancel/status state only lived in the
// DOM nodes from before — a walkthrough kept running with no way to see it
// or cancel it once reopened. "slow" has a 2s delay so there's a real window
// to close/reopen while it's still going.
await previewPage.click('#qv-debugger-list [data-item="slow"]');
await previewPage.waitForSelector('[data-run-walkthrough="slow"]');
await previewPage.click('[data-run-walkthrough="slow"]');
await previewPage.waitForFunction(() => document.querySelector('[data-walkthrough-status]')?.textContent === 'Running…');

await previewPage.click('#qv-debugger-close');
await previewPage.waitForFunction(() => !document.getElementById('questVivaDebugger').open);
await previewPage.click('#cmdDebug');
await previewPage.waitForSelector('#questVivaDebugger[open]');

const reopenedState = await previewPage.evaluate(() => ({
    activeTab: document.querySelector('#qv-debugger-tabs button.preset-filled-primary-500')?.textContent,
    status: document.querySelector('[data-walkthrough-status]')?.textContent,
    cancelHidden: document.querySelector('[data-cancel-walkthrough]')?.classList.contains('hidden'),
    runDisabled: document.querySelector('[data-run-walkthrough="slow"]')?.disabled,
}));
check(
    'Reopening the dialog mid-walkthrough lands back on it with Running…/Cancel restored',
    reopenedState.activeTab === 'Walkthrough'
        && reopenedState.status === 'Running…'
        && reopenedState.cancelHidden === false
        && reopenedState.runDisabled === true
);

// Cancel should still actually work post-reopen (updateWalkthroughRunUi's
// fresh DOM queries, not stale references captured before the reopen).
await previewPage.click('[data-cancel-walkthrough]');
await previewPage.waitForFunction(() => {
    const el = document.querySelector('[data-walkthrough-status]');
    return el && el.textContent && el.textContent !== 'Running…';
}, { timeout: 15000 });
const cancelledStatus = await previewPage.$eval('[data-walkthrough-status]', el => el.textContent);
check('Cancel still works after reopening the dialog', cancelledStatus !== 'Running…');
const cancelHiddenAfterCancel = await previewPage.$eval('[data-cancel-walkthrough]', el => el.classList.contains('hidden'));
check('Cancel hides again after cancelling', cancelHiddenAfterCancel);

await browser.close();

if (failed) {
    console.error('\nFAIL: one or more checks failed.');
    process.exit(1);
}
console.log('\nPASS');
