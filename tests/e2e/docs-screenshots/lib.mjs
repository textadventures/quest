// Shared helpers for capturing AppShell editor screenshots for the docs site
// (site/src/content/docs/). Mirrors the DOM patterns already proven out in the
// tests/e2e/verify-appshell-*.mjs scripts — see .claude/skills/verify/SKILL.md
// and .claude/skills/docs-screenshots/SKILL.md for context and conventions.
import { chromium } from 'playwright';

export const DEFAULT_BASE_URL = process.argv[2] || 'http://localhost:5174';

// Fixed viewport so every capture is the same size/framing. Kept fairly narrow
// (well above AppShell's mobile breakpoint, but well below 1280) because Starlight
// scales images down to its content column width (~700-800px) — a narrower source
// image means less downscaling, so on-screen text stays legible. Height is a
// generous ceiling; capture() crops down to actual content height per shot, so it
// doesn't matter that most states don't fill it.
const VIEWPORT = { width: 960, height: 800 };

export async function launch(baseUrl = DEFAULT_BASE_URL) {
    const browser = await chromium.launch();
    const page = await browser.newPage({ viewport: VIEWPORT });
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });
    return { browser, page, baseUrl };
}

// Creates a local (browser-only) draft game with a fixed, human-readable name and
// waits for the editor to load. The AppShell title bar shows this name in captured
// screenshots, so it needs to read cleanly (not e.g. "Tutorial Game 1786726809698"),
// which means reusing the same name across every run — so any existing draft with
// that name is deleted first, keeping repeat runs idempotent.
export async function createLocalDraft(page, baseUrl, name) {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });

    const existingDraft = page.locator(`text=${name}.aslx`);
    if (await existingDraft.count() > 0) {
        await existingDraft.locator('..').locator('button[title="Delete draft"]').click();
        await page.getByRole('button', { name: 'Delete', exact: true }).click();
        await existingDraft.waitFor({ state: 'detached', timeout: 10000 });
    }

    await page.fill('input[placeholder="Game name"]', name);
    // Game type defaults to Text Adventure once the name field triggers the extra
    // fields to appear — no need to explicitly select it for a text-adventure capture.
    await page.waitForSelector('text=Text Adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
}

// Selects a node in the left tree by its element name. A leaf's clickable row is
// [data-part="item"]; once it gains children it becomes a branch whose own
// [role="treeitem"] wrapper carries the same data-value on [data-part="branch-control"]
// instead — match both (see tests/e2e/verify-appshell-exits-editor.mjs).
export async function selectTreeNode(page, name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

// Adds a new element via the toolbar "+ Add" menu. `menuLabel` must match the
// visible menu item text exactly, e.g. "Add Room", `Add Object in "lounge"`,
// `Add Exit from "lounge"`. Uses exact role matching, not has-text substring
// matching — "Add Room" is itself a substring of "Add Room in \"lounge\"", so a
// plain :has-text() click here is ambiguous once a room is selected.
export async function addElement(page, menuLabel, name) {
    await page.click('button[title="Add element"]');
    await page.getByRole('button', { name: menuLabel, exact: true }).click({ timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

// Switches the properties panel to the given tab (Setup/Room/Exits/Object/Verbs/...).
// Exact match matters: an object's tab bar has both "Object" and "Objects" tabs, so
// a has-text("Object") click would be ambiguous between the two.
export async function openTab(page, tabLabel) {
    await page.getByRole('button', { name: tabLabel, exact: true }).click({ timeout: 10000 });
}

// Most single-line property fields render as a `<span>label</span>` immediately
// followed by the input/select, inside a shared flex-row parent — no id/name/
// aria-label to hook into directly. See tests/e2e/verify-appshell-exits-editor.mjs's
// `Name:` lookup for the pattern this generalizes.
export function fieldByLabel(page, labelText) {
    return page.locator(`span:has-text("${labelText}")`).locator('..').locator('input, select, textarea');
}

export async function setLabeledField(page, labelText, value) {
    await fieldByLabel(page, labelText).fill(value);
}

export async function selectLabeledField(page, labelText, optionValue) {
    await fieldByLabel(page, labelText).selectOption(optionValue);
}

// Adds a verb to the currently-selected object/room and selects it in the Verbs table,
// which opens its BEHAVIOUR panel on the right (defaults to "Print a message").
export async function addVerb(page, verbName) {
    const addVerbButton = page.locator('button:has-text("Add Verb")');
    await addVerbButton.locator('..').locator('input').fill(verbName);
    await addVerbButton.click();
    const row = page.locator(`td:has-text("${verbName}")`);
    await row.waitFor({ timeout: 10000 });
    await row.click();
}

// Adds an element that lives under the "Advanced" tree node (Timer, Function, ...) —
// these aren't in the toolbar "+ Add element" menu (see Toolbar.svelte's ADVANCED_ADDERS
// comment), instead the node's own properties panel has one "+ Add <Type>" button per type.
export async function addAdvancedElement(page, elementType, name) {
    await selectTreeNode(page, '_advanced');
    await page.getByRole('button', { name: `+ Add ${elementType}`, exact: true }).click();
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click(`[role="dialog"] button:has-text("Add ${elementType}")`);
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

// Opens the "Add Script Command" modal from a given "+ Add script" button (there can be
// several on screen at once — root level plus one per nested then/else block — so pass the
// specific Locator for the one you mean, e.g. `page.locator('button:has-text("+ Add
// script")').first()` for the outermost one). `category` navigates the left sidebar
// (Output/Scripts/Objects/...); `item` picks a specific command by its exact visible label.
// Both are optional — omitting them accepts the dialog's default ("Print a message").
export async function addScriptCommand(page, addButtonLocator, { category, item } = {}) {
    await addButtonLocator.click();
    await page.waitForSelector('text=Add Script Command');
    // Category sidebar and command list are both plain buttons with role="option"
    // (a custom listbox, not a native <select>) — see AddScriptModal.svelte. Command rows
    // (not categories) render with a leading "●" marker baked into the accessible name, and
    // some labels are prefixes of others (e.g. "Move object" / "Move object to the current
    // room") — anchor a regex against the bullet-prefixed full name instead of a plain
    // substring match, which would hit both.
    if (category) {
        await page.getByRole('option', { name: category, exact: true }).click();
    }
    if (item) {
        await page.getByRole('option', { name: new RegExp(`^●\\s*${item.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}$`) }).click();
    }
    await page.getByRole('button', { name: 'OK', exact: true }).click();
}

// Locators for the two dropdowns on an "if" (or "else if") condition row: the expression
// type (e.g. "object is switched on") and, once that's chosen, the object it applies to.
// Found via the literal "if"/"else if" label span rather than a container class, since
// ScriptEditor.svelte nests these blocks recursively with no other stable hook — the
// expression select is the label's first following <select> sibling, the object picker
// (when the chosen expression takes a simple object argument) is the second. Pass `nth`
// (0-based, in document order) when a page has more than one if-block on screen at once.
export function ifExpressionSelect(page, { label = 'if', nth = 0 } = {}) {
    return page.locator(`xpath=(//span[text()="${label}"]/following-sibling::select[1])[${nth + 1}]`);
}
export function ifObjectSelect(page, { label = 'if', nth = 0 } = {}) {
    return page.locator(`xpath=(//span[text()="${label}"]/following-sibling::select[2])[${nth + 1}]`);
}

// Ticks a Features-tab checkbox by its row's leading label text (e.g. "Container:",
// "Switchable:") to reveal that feature's own properties tab.
export async function toggleFeature(page, labelPrefix) {
    await page.locator(`text=${labelPrefix}`).locator('..').locator('input[type="checkbox"]').check();
}

// Clicks the toolbar Preview button and returns the WasmPlayer tab it opens, once booted
// and ready to accept a command. Requires `baseUrl` (the page's own origin, i.e. the
// AppShell dev server, not WasmPlayer's own port) to proxy '/player' — see vite.config.ts's
// "Proxy WasmPlayer through the same origin so BroadcastChannel works between editor and
// player tabs" comment; that same-origin requirement is also why this returns a second
// Playwright `page` in the same browser context rather than navigating in place — Preview
// always opens via `window.open`, and the original editor tab has to stay alive throughout
// (it's the BroadcastChannel sender: it answers WasmPlayer's "ready" with the current game
// bytes, and again on every subsequent "ready" if the player reloads) — closing or
// navigating it away mid-capture strands the player on its boot screen.
// `capture()` (this module's own) works unchanged against the returned page — pass
// `page.locator('#txtCommand')` as `untilLocator` to crop to the bottom of the transcript.
export async function openPreview(page) {
    const context = page.context();
    const [playerPage] = await Promise.all([
        context.waitForEvent('page', { timeout: 15000 }),
        page.click('button:has-text("Preview")'),
    ]);
    await playerPage.waitForSelector('#txtCommand', { state: 'visible', timeout: 60000 });
    await playerPage.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
    return playerPage;
}

// Types a command into a WasmPlayer tab (as returned by openPreview) and submits it,
// waiting for the previous command's turn to fully finish first — sendCommand() in
// player.js silently drops a command while canSendCommand is still false from the last
// one's round-trip, so a fixed sleep between commands would be flaky.
export async function sendCommand(playerPage, command) {
    await playerPage.waitForFunction(() => window.canSendCommand === true, { timeout: 10000 });
    await playerPage.fill('#txtCommand', command);
    await playerPage.press('#txtCommand', 'Enter');
    await playerPage.waitForFunction(() => window.canSendCommand === true, { timeout: 10000 });
}

const CURSOR_ELEMENT_ID = '__docs-capture-cursor';

// Classic arrow-cursor glyph, hotspot (the point it's "pointing at") at the path's own
// (0,0) — so positioning the wrapper's top-left corner at a target coordinate puts the
// tip exactly there, the same convention as a real OS cursor image. White fill with a
// black outline keeps it legible over both light and dark editor chrome.
const CURSOR_SVG = `<svg width="26" height="26" viewBox="0 0 26 26" xmlns="http://www.w3.org/2000/svg" style="filter: drop-shadow(0 1px 2px rgba(0,0,0,0.5))">
  <path d="M 0 0 L 0 18 L 5 14 L 8 20.5 L 11 19 L 8 12.5 L 13.5 12.5 Z" fill="white" stroke="black" stroke-width="1.3" stroke-linejoin="round"/>
</svg>`;

// Injects a synthetic cursor image into the page at a target locator's position — real
// page.screenshot() never captures the actual OS mouse cursor, so this fakes one in for
// screenshots that need to show the reader where to click. `at` picks the point within
// the target's box the cursor tip lands on: 'center' (default), or a corner/edge name
// ('left' = vertical-center of the left edge, etc.) matching typical "pointing at a
// dropdown/button" framing. Returns a cleanup function — call it after the screenshot is
// taken so the fake cursor never leaks into later captures or lingers in the live DOM.
async function injectCursor(page, targetLocator, at = 'center') {
    const box = await targetLocator.boundingBox();
    if (!box) return async () => {};
    const points = {
        center: { x: box.x + box.width / 2, y: box.y + box.height / 2 },
        left: { x: box.x + 6, y: box.y + box.height / 2 },
        right: { x: box.x + box.width - 6, y: box.y + box.height / 2 },
        top: { x: box.x + box.width / 2, y: box.y + 6 },
        bottom: { x: box.x + box.width / 2, y: box.y + box.height - 6 },
    };
    const { x, y } = points[at] ?? points.center;
    await page.evaluate(({ id, left, top, svg }) => {
        const el = document.createElement('div');
        el.id = id;
        el.style.cssText = `position:fixed; left:${left}px; top:${top}px; z-index:2147483647; pointer-events:none; margin:0; padding:0; line-height:0;`;
        el.innerHTML = svg;
        document.body.appendChild(el);
    }, { id: CURSOR_ELEMENT_ID, left: x, top: y, svg: CURSOR_SVG });
    return () => page.evaluate((id) => document.getElementById(id)?.remove(), CURSOR_ELEMENT_ID);
}

// Captures a screenshot at the current state and reports the save path. Most editor
// states only use the top portion of the fixed VIEWPORT height, leaving a lot of
// blank space below (e.g. a short "rename this room" form) — pass `untilLocator` for
// the last element relevant to what this particular screenshot is illustrating (the
// field just filled in, the button about to be clicked, ...) and the capture crops to
// that element's bottom edge + padding instead of the full viewport height. Omit it
// for states where the full viewport genuinely is the content (rare).
//
// Pass `cursorAt` to draw a synthetic cursor pointing at a specific control — a locator
// on its own (tip lands centered on it), or `{ locator, at }` for one of the named
// points `injectCursor` supports (e.g. `{ locator: dropdown, at: 'left' }` to point at a
// dropdown about to be opened, without the cursor covering its label text). Useful for
// "click here" framing where the surrounding prose alone doesn't make the target obvious
// (e.g. a screenshot showing a still-closed native `<select>`, which can't be captured
// mid-open — see docs-screenshots/README or ask the docs-screenshots skill for why).
export async function capture(page, outputPath, { untilLocator, padding = 24, cursorAt } = {}) {
    let removeCursor;
    if (cursorAt) {
        const { locator, at } = typeof cursorAt.boundingBox === 'function' ? { locator: cursorAt, at: 'center' } : cursorAt;
        removeCursor = await injectCursor(page, locator, at);
    }
    let clip;
    if (untilLocator) {
        const box = await untilLocator.boundingBox();
        if (box) {
            clip = { x: 0, y: 0, width: VIEWPORT.width, height: Math.min(VIEWPORT.height, Math.ceil(box.y + box.height + padding)) };
        }
    }
    await page.screenshot({ path: outputPath, ...(clip ? { clip } : {}) });
    if (removeCursor) await removeCursor();
    console.log(`SAVED: ${outputPath}`);
}

// Standard try/run/finally wrapper matching the existing verify-*.mjs convention:
// screenshot to /tmp on failure, always close the browser, non-zero exit on error.
export async function runCapture(fn) {
    const { browser, page, baseUrl } = await launch();
    try {
        await fn({ page, baseUrl });
        console.log('PASS: all captures saved');
    } catch (err) {
        console.error('FAIL:', err.message);
        await page.screenshot({ path: '/tmp/docs-screenshot-failure.png' });
        process.exitCode = 1;
    } finally {
        await browser.close();
    }
}
