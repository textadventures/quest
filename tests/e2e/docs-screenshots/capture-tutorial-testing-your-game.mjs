// Regenerates the 2 screenshots embedded in
// site/src/content/docs/tutorial/testing-your-game.md: the Debugger showing Bob's
// attributes after he has been revived, and a walkthrough with its recorded steps.
//
// The Debugger is a WasmPlayer feature (#cmdDebug), only reachable through an editor
// preview session - see capture-debugging-your-game.mjs. Bob is revived here by a
// defibrillator whose "use on its own" script sets the flag directly, rather than by
// rebuilding the tutorial's whole Use/Give setup: only Bob's attribute list ends up in
// the screenshot, so the route that sets the flag doesn't show.
// See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab,
    setLabeledField, toggleFeature, setScriptCodeView, openPreview, sendCommand, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    await selectTreeNode(page, 'room');
    await setLabeledField(page, 'Name:', 'lounge');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="lounge"]', { timeout: 10000 });

    await selectTreeNode(page, 'lounge');
    await addElement(page, 'Add Object in "lounge"', 'Bob');
    await selectTreeNode(page, 'lounge');
    await addElement(page, 'Add Object in "lounge"', 'defibrillator');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Use/Give:');
    await openTab(page, 'Use/Give');
    await page.locator('select').first().selectOption({ label: 'Run script' });
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(),
        `msg ("Bob is now alive again.")
Bob.alive = true`);
    await page.waitForSelector('text=Set variable', { timeout: 10000 });

    // --- TutorialWalkthrough.png: a walkthrough with steps, before Play/Record ---
    // "Add Walkthrough" lives under the Advanced tree node, not the toolbar "+ Add" menu
    // (see Toolbar.svelte's ADVANCED_ADDERS comment).
    await selectTreeNode(page, '_advanced');
    await page.getByRole('button', { name: '+ Add Walkthrough', exact: true }).click();
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', 'win game');
    await page.click('[role="dialog"] button:has-text("Add Walkthrough")');
    await page.waitForSelector('text=Record', { timeout: 10000 });
    const stepInput = page.locator('input[placeholder^="Add walkthrough command"]');
    for (const step of ['take defibrillator', 'use defibrillator', 'south', 'open cupboard',
        'take key', 'unlock door', 'south', 'assert:game.score = 10']) {
        await stepInput.fill(step);
        await stepInput.locator('..').locator('button:has-text("Add")').click();
        await page.waitForTimeout(150);
    }
    await capture(page, out('TutorialWalkthrough.png'), { untilLocator: stepInput, padding: 24 });

    // --- TutorialDebugger.png: Bob's attributes in the Debugger, after reviving him ---
    const playerPage = await openPreview(page);
    await sendCommand(playerPage, 'use defibrillator');
    await playerPage.click('#cmdDebug');
    await playerPage.waitForSelector('#questVivaDebugger[open]', { timeout: 5000 });
    await playerPage.click('#qv-debugger-tabs button:text("Objects")');
    await playerPage.waitForSelector('#qv-debugger-list [data-item]');
    await playerPage.click('#qv-debugger-list [data-item="Bob"]');
    await playerPage.waitForSelector('[data-attr-row]');
    await capture(playerPage, out('TutorialDebugger.png'), {
        untilLocator: playerPage.locator('#questVivaDebugger'),
    });
});
