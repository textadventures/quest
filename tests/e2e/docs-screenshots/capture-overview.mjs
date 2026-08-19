// Regenerates the 6 screenshots embedded in site/src/content/docs/overview.md - a broad,
// illustrative tour of the editor/player rather than tutorial-specific steps, so the exact
// scenes below are representative rather than transcribed from prose. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addVerb,
    toggleFeature, addScriptCommand, ifExpressionSelect, ifObjectSelect,
    setScriptCodeView, openPreview, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);
const loungePicture = '/private/tmp/claude-501/-Users-alexwarren-Code-quest/ab5b1524-2956-4e1f-a0ed-ddffb535b27f/scratchpad/lounge.png';

const checkboxFor = (page, label) => page.getByText(label, { exact: true }).locator('xpath=..').locator('input[type="checkbox"]');

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'TV');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Object in "room"', 'Bob');
    // Bob defaults to the generic "Inanimate object" type, which prints "a Bob" in room
    // descriptions (the default "a"/"an" prefix) - switch to "Male character (named)" so he
    // reads as a proper name instead, same as the tutorial's own "Creating a Character" step
    // (interacting_with_objects.md) instructs. The Setup tab has two "Type" dropdowns with the
    // same caption (Room/Object/Object-and-or-room, then Inanimate/Male/Female character) - the
    // second one is the only one offering a "namedmale" option value, so select on that instead
    // of relying on label text order.
    await selectTreeNode(page, 'Bob');
    await openTab(page, 'Setup');
    await page.locator('select:has(option[value="namedmale"])').selectOption('namedmale');
    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Room', 'kitchen');
    await selectTreeNode(page, 'room');
    await openTab(page, 'Exits');
    await page.getByRole('button', { name: 'east', exact: true }).click();
    const destCombobox = page.locator('[role="combobox"]');
    await destCombobox.click();
    await destCombobox.fill('kitchen');
    await page.waitForSelector('[role="option"]:has-text("kitchen")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("kitchen")');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=east → kitchen', { timeout: 10000 });
    await selectTreeNode(page, 'room');
    await openTab(page, 'Setup');

    // --- overview-editor.png: room selected, Setup tab, objects/exit visible in the tree ---
    await capture(page, out('overview-editor.png'), { untilLocator: page.locator('text=Use default prefix and suffix'), padding: 220 });

    // --- overview-textadventure.png: initial player view of that same room ---
    const p1 = await openPreview(page);
    await capture(p1, out('overview-textadventure.png'), { untilLocator: p1.locator('#txtCommand') });
    await p1.close();

    // --- overview-multimedia.png: room picture (Picture frame feature) shown in the game pane ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Interface');
    await toggleFeature(page, 'Picture frame:');
    await selectTreeNode(page, 'room');
    await openTab(page, 'Room');
    await page.waitForSelector('text=Room picture:', { timeout: 10000 });
    const uploadInput = page.getByText('Room picture:', { exact: true }).locator('xpath=following::input[@type="file"][1]');
    await uploadInput.setInputFiles(loungePicture);
    await page.waitForTimeout(500);
    const p2 = await openPreview(page);
    await capture(p2, out('overview-multimedia.png'), { untilLocator: p2.locator('#txtCommand') });
    await p2.close();

    // --- overview-script.png: Bob's "speak to" verb, an if/else script ---
    await selectTreeNode(page, 'Bob');
    await openTab(page, 'Verbs');
    await addVerb(page, 'speak to');
    await page.locator('select').first().selectOption('script');
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first(), { category: 'Scripts', item: 'If...' });
    await ifExpressionSelect(page).selectOption('object has flag');
    await ifObjectSelect(page).selectOption({ label: 'Bob' });
    const flagNameInput = page.locator('xpath=(//span[text()="if"]/following::input)[1]');
    await flagNameInput.fill('alive');
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const thenMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::input[1]');
    await thenMsg.fill('Bob says he feels kind of fuzzy.');
    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').nth(1));
    const elseMsg = page.locator('xpath=(//span[text()="Print"])[2]/following-sibling::input[1]');
    await elseMsg.fill('Bob stares at you blankly.');
    await capture(page, out('overview-script.png'), { untilLocator: elseMsg, padding: 40 });

    // --- overview-customui.png: a distinct atmospheric custom pane style ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Show advanced scripts for the game object');
    await openTab(page, 'Advanced Scripts');
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `backandborder = "border: 1px solid #4a5568;background:#1a202c"
text = "color:#e2e8f0;font-family:georgia, serif"
JS.setCss ("body", "background:#0f1117")
JS.setCss (".ui-accordion-header", "border-radius: 3px;" + backandborder)
JS.setCss (".ui-accordion-content", "border-radius: 3px;" + backandborder + ";border-top:none")
JS.setCss (".accordion-header-text", text)
JS.setCss (".ui-icon", "display:none")
JS.setCss ("#gamePanes", "margin-top: 16px")`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });
    const p3 = await openPreview(page);
    await capture(p3, out('overview-customui.png'), { untilLocator: p3.locator('#txtCommand') });
    await p3.close();
});

await runCapture(async ({ page, baseUrl }) => {
    // --- overview-gamebook.png: a fresh Gamebook draft's default Page1, unmodified - its
    // scaffolded content ("This is page 1... This link goes to page 2 / And this link goes to
    // page 3") already matches what the old screenshot showed, so no editing is needed ---
    await createLocalDraft(page, baseUrl, 'Gamebook Example', { gameType: 'Gamebook' });
    // Gamebooks have no #txtCommand box (link-driven, not command-driven) - lib.mjs's
    // openPreview() would hang forever waiting for it, same class of issue as the command-bar-
    // off case in capture-ui-style.mjs. #location also never gets populated for a gamebook
    // (no room concept), confirmed live it stays whitespace-only forever, so wait for the
    // page's own scaffolded text to actually appear instead.
    const context = page.context();
    const [p] = await Promise.all([
        context.waitForEvent('page', { timeout: 15000 }),
        page.click('button:has-text("Preview")'),
    ]);
    await p.waitForFunction(() => document.body?.innerText.includes('This is page 1'), { timeout: 20000 });
    await capture(p, out('overview-gamebook.png'), { untilLocator: p.getByText('And this link goes to page 3'), padding: 60 });
});
