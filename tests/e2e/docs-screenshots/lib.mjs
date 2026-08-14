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

// Captures a screenshot at the current state and reports the save path. Most editor
// states only use the top portion of the fixed VIEWPORT height, leaving a lot of
// blank space below (e.g. a short "rename this room" form) — pass `untilLocator` for
// the last element relevant to what this particular screenshot is illustrating (the
// field just filled in, the button about to be clicked, ...) and the capture crops to
// that element's bottom edge + padding instead of the full viewport height. Omit it
// for states where the full viewport genuinely is the content (rare).
export async function capture(page, outputPath, { untilLocator, padding = 24 } = {}) {
    let clip;
    if (untilLocator) {
        const box = await untilLocator.boundingBox();
        if (box) {
            clip = { x: 0, y: 0, width: VIEWPORT.width, height: Math.min(VIEWPORT.height, Math.ceil(box.y + box.height + padding)) };
        }
    }
    await page.screenshot({ path: outputPath, ...(clip ? { clip } : {}) });
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
