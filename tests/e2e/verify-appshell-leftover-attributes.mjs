// Verifies that the editor never hides a setting that still takes effect in the game:
//   - A feature tab (one shown by a "feature_..." checkbox) stays visible while the object still
//     has settings for it, with a banner offering to turn the feature back on or remove them.
//     Unticking a feature that has settings asks whether to remove them too, in one undo step.
//   - A control tagged <alsodisplayifset> stays visible while the object has its own value for
//     it, with a warning and a Remove button.
//
// Hiding a control never stopped its attribute working at runtime, so authors were left with
// values they couldn't see:
//   - #1586: unticking "Run an initialisation script for this object" hid the Initialisation
//     script tab, but Core.aslx's StartGame still ran the object's _initialise_ script.
//   - #2513: switching a container from Openable/Closable to Container hid "Script to run when
//     opening object", but TryOpenClose still ran that openscript instead of opening the
//     container normally.
//
// Requires the AppShell dev server running locally (WasmEditor Debug build first):
//   cd src/AppShell && npm run dev
// Run: node tests/e2e/verify-appshell-leftover-attributes.mjs [baseUrl]
import { chromium } from './lib/tracked-chromium.mjs';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const warningText = 'still takes effect when the game runs';
const bannerText = "turned off in this object's Features tab";

const browser = await chromium.launch();

function check(label, actual, expected) {
    if (actual !== expected) throw new Error(`[${label}] got ${JSON.stringify(actual)}, expected ${JSON.stringify(expected)}`);
    console.log(`PASS: ${label}`);
}

// A leaf's row is [data-part="item"], a branch's own row is [data-part="branch-control"].
async function selectTreeNode(page, name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

async function addViaMenu(page, buttonText, name) {
    await page.click('button[title="Add element"]');
    await page.click(`button:has-text("${buttonText}")`, { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

// Scoped to the tab strip, since some tab names also appear as control captions.
function tabButton(page, name) {
    return page.locator('.flex.border-b.border-surface-200-800.overflow-x-auto.flex-shrink-0')
        .getByRole('button', { name, exact: true });
}

async function selectTab(page, name) {
    await tabButton(page, name).click();
    await page.waitForTimeout(200);
}

function featureBox(page, caption) {
    return page.locator('label', { hasText: caption }).locator('input[type="checkbox"]');
}

async function setFeature(page, caption, checked) {
    await selectTab(page, 'Features');
    const box = featureBox(page, caption);
    if ((await box.isChecked()) !== checked) await box.click();
    await page.waitForTimeout(300);
}

// Unticks a feature that still has settings, and answers the prompt that follows.
async function untickFeatureAndAnswer(page, caption, answer) {
    await selectTab(page, 'Features');
    await featureBox(page, caption).click();
    const dialog = page.locator('[role="dialog"]').filter({ hasText: 'Remove them too?' });
    await dialog.waitFor({ timeout: 5000 });
    const text = await dialog.innerText();
    await dialog.getByRole('button', { name: answer, exact: true }).click();
    await dialog.waitFor({ state: 'hidden', timeout: 5000 });
    await page.waitForTimeout(300);
    return text;
}

// Adds the default "Print a message" command via the first "+ Add script" button on the tab.
async function addPrintScript(page, message) {
    await page.locator('button:has-text("+ Add script")').first().click();
    const dialog = page.locator('[role="dialog"]').filter({ hasText: 'Add Script Command' });
    await dialog.waitFor({ timeout: 10000 });
    await dialog.getByRole('button', { name: 'OK' }).click();
    await dialog.waitFor({ state: 'hidden', timeout: 5000 });
    const input = page.locator('input[type="text"], textarea').last();
    await input.fill(message);
    await input.blur();
    await page.waitForTimeout(300);
}

const warnings = page => page.getByText(warningText);

try {
    const ctx = await browser.newContext({ viewport: { width: 1400, height: 900 } });
    const page = await ctx.newPage();
    page.on('pageerror', err => console.log('[pageerror]', err.message));

    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Leftover Attributes ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="More"]', { timeout: 30000 });

    // ── #1586: initialisation script ────────────────────────────────────────────
    await selectTreeNode(page, 'room');
    await addViaMenu(page, 'Add Object in', 'lamp');
    const initFeature = 'Run an initialisation script for this object';

    check('Initialisation script tab hidden by default', await tabButton(page, 'Initialisation script').count(), 0);

    await setFeature(page, initFeature, true);
    await selectTab(page, 'Initialisation script');
    await addPrintScript(page, 'The lamp flickers.');
    check('no warning while the feature is ticked', await warnings(page).count(), 0);

    // Cancel leaves the feature ticked.
    const prompt = await untickFeatureAndAnswer(page, initFeature, 'Cancel');
    if (!prompt.includes('lamp: This script will run once, as the game starts')) {
        throw new Error(`Prompt should list the lamp's initialisation script, got: ${prompt}`);
    }
    console.log('PASS: unticking lists what is still set');
    check('Cancel leaves the feature ticked', await featureBox(page, initFeature).isChecked(), true);

    // Keep: the tab stays, with a banner, since the script still runs.
    await untickFeatureAndAnswer(page, initFeature, 'Keep them');
    check('Keep unticks the feature', await featureBox(page, initFeature).isChecked(), false);
    check('tab stays while the object has its own script', await tabButton(page, 'Initialisation script').count(), 1);
    await selectTab(page, 'Initialisation script');
    check('banner shown on the leftover feature tab', await page.getByText(bannerText).count(), 1);

    await page.getByRole('button', { name: 'Turn back on', exact: true }).click();
    await page.waitForTimeout(500);
    check('Turn back on removes the banner', await page.getByText(bannerText).count(), 0);
    await selectTab(page, 'Features');
    check('Turn back on ticks the feature', await featureBox(page, initFeature).isChecked(), true);

    await untickFeatureAndAnswer(page, initFeature, 'Keep them');
    await selectTab(page, 'Initialisation script');
    await page.getByRole('button', { name: 'Remove settings', exact: true }).click();
    await page.waitForTimeout(500);
    check('Remove settings deletes the script, so the tab goes', await tabButton(page, 'Initialisation script').count(), 0);

    // Remove: unticks and deletes in one undo step.
    await setFeature(page, initFeature, true);
    await selectTab(page, 'Initialisation script');
    await addPrintScript(page, 'The lamp flickers again.');
    await untickFeatureAndAnswer(page, initFeature, 'Remove them');
    check('Remove them deletes the script, so the tab goes', await tabButton(page, 'Initialisation script').count(), 0);
    await page.click('button[title="Undo"]');
    await page.waitForTimeout(500);
    check('one Undo brings the script back', await tabButton(page, 'Initialisation script').count(), 1);
    await selectTab(page, 'Features');
    check('...and ticks the feature again', await featureBox(page, initFeature).isChecked(), true);

    // ── #2513: openscript left over from Openable/Closable ──────────────────────
    await selectTreeNode(page, 'room');
    await addViaMenu(page, 'Add Object in', 'chest');
    await setFeature(page, 'Container: object is a container', true);
    await selectTab(page, 'Container');
    const typeSelect = page.locator('label', { hasText: 'Container type' }).locator('select');
    await typeSelect.selectOption({ label: 'Openable/Closable' });
    await page.waitForTimeout(500);
    await addPrintScript(page, 'The lid creaks.');
    check('no warning while the object is openable', await warnings(page).count(), 0);

    await typeSelect.selectOption({ label: 'Container' });
    await page.waitForTimeout(500);
    check('opening script still shown after switching to Container',
        await page.getByText('Script to run when opening object').count(), 1);
    check('warning shown for the leftover opening script', await warnings(page).count(), 1);

    await page.getByRole('button', { name: 'Remove', exact: true }).click();
    await page.waitForTimeout(500);
    check('Remove deletes the opening script',
        await page.getByText('Script to run when opening object').count(), 0);
    check('no warnings left', await warnings(page).count(), 0);

    console.log('\nAll checks passed.');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
