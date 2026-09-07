// Regenerates the 6 screenshots embedded in site/src/content/docs/intro.md - a broad,
// illustrative tour of the editor/player rather than tutorial-specific steps, so the exact
// scenes below are representative rather than transcribed from prose. See
// .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { tmpdir } from 'node:os';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab, addVerb,
    toggleFeature, addScriptCommand, ifExpressionSelect, ifObjectSelect,
    setScriptCodeView, openPreview, capture,
} from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

// The "room picture" upload for the Picture frame feature. The doc only needs to show that
// the feature displays *a* picture, but a flat single-colour rectangle reads as a broken or
// missing image rather than as a photo, so draw an actual scene. A small flat illustration
// also survives Starlight's downscaling better than a photo would, and keeping it as SVG
// source here (rasterised through the browser this script already drives, rather than by
// hand-rolling a PNG encoder or adding an image dependency) means the scene stays editable.
// It matches the room the rest of this capture builds: a lounge with a TV, which is what the
// player then sees described as "You can see a TV and Bob".
const LOUNGE_SVG = `<svg xmlns="http://www.w3.org/2000/svg" width="480" height="320" viewBox="0 0 480 320">
  <rect width="480" height="320" fill="#e6d9c3"/>
  <rect x="30" y="36" width="120" height="96" rx="4" fill="#f6efe4"/>
  <rect x="38" y="44" width="104" height="80" fill="#9cc6dd"/>
  <circle cx="66" cy="66" r="11" fill="#f4dc99"/>
  <path d="M38 110 l26-22 18 14 16-18 44 30z" fill="#86a98f"/>
  <path d="M90 44 v80 M38 84 h104" stroke="#f6efe4" stroke-width="7"/>
  <rect x="330" y="50" width="96" height="72" rx="3" fill="#f6efe4"/>
  <rect x="338" y="58" width="80" height="56" fill="#c7b295"/>
  <path d="M338 114 l24-30 17 20 15-17 24 27z" fill="#7f9a86"/>
  <rect y="226" width="480" height="8" fill="#f6efe4"/>
  <rect y="234" width="480" height="86" fill="#b98d5f"/>
  <ellipse cx="252" cy="278" rx="196" ry="30" fill="#cb8f74" opacity="0.5"/>
  <rect x="62" y="212" width="126" height="28" rx="3" fill="#8b6a49"/>
  <rect x="118" y="200" width="14" height="12" fill="#2d333d"/>
  <rect x="76" y="136" width="98" height="66" rx="5" fill="#2d333d"/>
  <rect x="83" y="143" width="84" height="52" rx="2" fill="#52697b"/>
  <rect x="204" y="146" width="5" height="104" fill="#6d5c46"/>
  <path d="M188 148 h34 l-8 -26 h-18 z" fill="#f0cf8f"/>
  <ellipse cx="206" cy="250" rx="17" ry="5" fill="#6d5c46"/>
  <rect x="238" y="172" width="192" height="60" rx="12" fill="#6f8d80"/>
  <rect x="252" y="182" width="80" height="40" rx="7" fill="#82a094"/>
  <rect x="338" y="182" width="80" height="40" rx="7" fill="#82a094"/>
  <rect x="232" y="208" width="204" height="42" rx="12" fill="#7f9c8f"/>
  <rect x="230" y="190" width="30" height="62" rx="12" fill="#628074"/>
  <rect x="408" y="190" width="30" height="62" rx="12" fill="#628074"/>
  <rect x="248" y="250" width="12" height="12" rx="2" fill="#6d5c46"/>
  <rect x="408" y="250" width="12" height="12" rx="2" fill="#6d5c46"/>
</svg>`;
const loungePicture = join(tmpdir(), 'docs-screenshot-lounge.png');

// Rasterise an SVG string to a PNG on disk via a throwaway page in the browser this capture
// is already running, so the harness needs no image library of its own.
async function renderSvgToPng(context, svg, outputPath, width, height) {
    const svgPage = await context.newPage();
    await svgPage.setViewportSize({ width, height });
    await svgPage.setContent(`<body style="margin:0">${svg}</body>`);
    await svgPage.screenshot({ path: outputPath });
    await svgPage.close();
}

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
    await renderSvgToPng(page.context(), LOUNGE_SVG, loungePicture, 480, 320);
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
    await ifObjectSelect(page, { selectIndex: 3 }).selectOption({ label: 'Bob' });
    const flagNameInput = page.locator('xpath=(//span[text()="if"]/following::input)[1]');
    await flagNameInput.fill('alive');
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').first());
    const thenMsg = page.locator('xpath=//span[text()="Print"]/following-sibling::textarea[1]');
    await thenMsg.fill('Bob says he feels kind of fuzzy.');
    await page.getByRole('button', { name: '+ else', exact: true }).click();
    await addScriptCommand(page, page.locator('button:has-text("+ Add script")').nth(1));
    const elseMsg = page.locator('xpath=(//span[text()="Print"])[2]/following-sibling::textarea[1]');
    await elseMsg.fill('Bob stares at you blankly.');
    await capture(page, out('overview-script.png'), { untilLocator: elseMsg, padding: 40 });

    // --- overview-customui.png: a distinct atmospheric custom pane style ---
    await selectTreeNode(page, 'game');
    await openTab(page, 'Features');
    await toggleFeature(page, 'Show advanced scripts for the game object');
    await openTab(page, 'Advanced Scripts');
    // Two halves, matching what the page's prose promises: the game's own colour/font settings,
    // then CSS for the panes. The transcript's colours have to come from the former - every line
    // of output is emitted already wrapped in a <span style="color:..."> built from
    // game.defaultforeground (CoreOutput.aslx's OutputTextNoBr), so a stylesheet can't reach it.
    // The pane chrome is the other way round: it's static markup, so CSS is the only lever.
    await setScriptCodeView(page, page.locator('button:has-text("Code view")').first(), `SetBackgroundColour ("#141c26")
SetForegroundColour ("#dde6f0")
SetLinkForegroundColour ("#7cc3d8")
SetFontName ("Georgia, serif")
panel = "background:#1b2430;border:1px solid #35485f"
JS.setCss ("#qv-status", "background:#1b2430;border:none;color:#f0c987")
JS.setCss ("#txtCommand", "background:#1b2430;color:#dde6f0;border:1px solid #35485f")
JS.setCss ("#gamePanes", "margin-top:16px")
JS.setCss (".ui-accordion-header", "border-radius:4px 4px 0 0;" + panel)
JS.setCss (".ui-accordion-content", "border-radius:0 0 4px 4px;border-top:none;color:#dde6f0;" + panel)
JS.setCss (".accordion-header-text", "color:#f0c987;letter-spacing:0.04em;padding-left:0.9em")
// The accordion's expand/collapse triangle only - NOT a bare ".ui-icon", which would also hide
// the compass's direction arrows (they're .ui-icon too, see playercore.js's compass buttons).
JS.setCss (".ui-accordion-header > .ui-icon", "display:none")
JS.setCss (".compassbutton", "background:#25344a;border:1px solid #35485f")
JS.setCss (".compassbutton .ui-icon", "filter:invert(1) brightness(1.7)")
JS.setCss ("#compassTable .ui-button", "color:#7cc3d8")`);
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
