// Regenerates the 7 editor screenshots embedded in
// site/src/content/docs/ask_simple_question.md. See .claude/skills/docs-screenshots/SKILL.md.
//
// Every image is a progressively-built-up version of the same Cindy-the-flower-seller "speak"
// verb script, built by typing raw quest-script into its Code view at each stage (much faster
// and more reliable than reconstructing show-menu/switch nesting via addScriptCommand). menu4a.png
// captures the full 4-case switch rather than "only the lower half" like the old Quest 5
// screenshot — capture() always crops from the top of the panel, and there's no reason to hide
// the upper cases; showing the whole thing is strictly more informative.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { runCapture, createLocalDraft, selectTreeNode, addElement, openTab, setScriptCodeView, capture } from './lib.mjs';

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

const lastAddScript = page => page.locator('button:has-text("+ Add script")').last();

// Switch cases render collapsed ("▶") by default — expand every one so the doc's screenshot
// shows the actual per-case script content, not just the case labels.
async function expandAllSwitchCases(page) {
    const toggles = page.getByRole('button', { name: '▶' });
    while (await toggles.count() > 0) {
        await toggles.first().click();
    }
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');

    for (const name of ['Cindy', 'roses', 'lavender', 'lilies', 'orchids']) {
        await selectTreeNode(page, 'room');
        await addElement(page, 'Add Object in "room"', name);
    }

    await selectTreeNode(page, 'Cindy');
    await openTab(page, 'Verbs');
    await page.waitForSelector('text=No verbs added yet', { timeout: 10000 });
    const verbInput = page.locator('button:has-text("Add Verb")').locator('..').locator('input');
    await verbInput.fill('speak');
    await page.click('button:has-text("Add Verb")');
    await page.waitForSelector('td:has-text("speak")', { timeout: 10000 });
    await page.click('td:has-text("speak")');
    const behaviourTypeSelect = page.getByRole('combobox').filter({ hasText: 'Print a message' });
    await behaviourTypeSelect.selectOption({ label: 'Run a script' });

    const codeViewBtn = () => page.locator('button:has-text("Code view")').first();

    // --- menu1.png: options list built from three fixed entries ---
    await setScriptCodeView(page, codeViewBtn(), `options = NewStringList()
list add (options, "Red roses")
list add (options, "Lavender")
list add (options, "Lilies")`);
    await page.waitForSelector('text=Set variable', { timeout: 5000 });
    await capture(page, out('menu1.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- menu1a.png: + conditional Orchids option ---
    await setScriptCodeView(page, codeViewBtn(), `options = NewStringList()
list add (options, "Red roses")
list add (options, "Lavender")
list add (options, "Lilies")
if (GetBoolean(Cindy, "orchids in stock")) {
list add (options, "Orchids")
}`);
    await page.waitForSelector('xpath=//span[text()="if"]', { timeout: 5000 });
    await capture(page, out('menu1a.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- menu2.png: + Show a menu (empty response body so far) ---
    await setScriptCodeView(page, codeViewBtn(), `options = NewStringList()
list add (options, "Red roses")
list add (options, "Lavender")
list add (options, "Lilies")
if (GetBoolean(Cindy, "orchids in stock")) {
list add (options, "Orchids")
}
ShowMenu ("What flowers do you want to buy?", options, true) {
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await capture(page, out('menu2.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- menu3.png: + switch(result) with the first case (Red roses) filled in ---
    await setScriptCodeView(page, codeViewBtn(), `options = NewStringList()
list add (options, "Red roses")
list add (options, "Lavender")
list add (options, "Lilies")
if (GetBoolean(Cindy, "orchids in stock")) {
list add (options, "Orchids")
}
ShowMenu ("What flowers do you want to buy?", options, true) {
switch (result) {
case ("Red roses") {
msg ("You buy some red roses from Cindy.")
MoveObject (roses, player)
}
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await expandAllSwitchCases(page);
    await capture(page, out('menu3.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- menu4.png: + second case (Lavender), third still missing ---
    await setScriptCodeView(page, codeViewBtn(), `options = NewStringList()
list add (options, "Red roses")
list add (options, "Lavender")
list add (options, "Lilies")
if (GetBoolean(Cindy, "orchids in stock")) {
list add (options, "Orchids")
}
ShowMenu ("What flowers do you want to buy?", options, true) {
switch (result) {
case ("Red roses") {
msg ("You buy some red roses from Cindy.")
MoveObject (roses, player)
}
case ("Lavender") {
msg ("You buy some lavender from Cindy.")
MoveObject (lavender, player)
}
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await expandAllSwitchCases(page);
    await capture(page, out('menu4.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- menu4a.png: all four cases, including the conditional Orchids one ---
    await setScriptCodeView(page, codeViewBtn(), `options = NewStringList()
list add (options, "Red roses")
list add (options, "Lavender")
list add (options, "Lilies")
if (GetBoolean(Cindy, "orchids in stock")) {
list add (options, "Orchids")
}
ShowMenu ("What flowers do you want to buy?", options, true) {
switch (result) {
case ("Red roses") {
msg ("You buy some red roses from Cindy.")
MoveObject (roses, player)
}
case ("Lavender") {
msg ("You buy some lavender from Cindy.")
MoveObject (lavender, player)
}
case ("Lilies") {
msg ("You buy some lilies from Cindy.")
MoveObject (lilies, player)
}
case ("Orchids") {
msg ("You buy some orchids from Cindy.")
MoveObject (orchids, player)
}
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await expandAllSwitchCases(page);
    await capture(page, out('menu4a.png'), { untilLocator: lastAddScript(page), padding: 40 });

    // --- menu5.png: separate simpler "Are you sure?" yes/no example (same verb slot) ---
    await setScriptCodeView(page, codeViewBtn(), `ShowMenu ("Are you sure?", Split("Yes;No", ";"), false) {
if (result = "Yes") {
msg ("You buy some red roses from Cindy.")
}
}`);
    await page.waitForSelector('text=Show menu with caption', { timeout: 5000 });
    await capture(page, out('menu5.png'), { untilLocator: lastAddScript(page), padding: 40 });
});
