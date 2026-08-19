// Regenerates Map.png, Map2.png and map7.png from
// site/src/content/docs/howto/tasks/showing_a_map.md - map3/map4/map5/map6 illustrate matched-
// loop-distance and up/down-level edge cases that need even more elaborate, carefully-measured
// room graphs and are left for a follow-up pass. See .claude/skills/docs-screenshots/SKILL.md.
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
import {
    runCapture, createLocalDraft, selectTreeNode, addElement, openTab, toggleFeature,
    sendCommand,
} from './lib.mjs';

// The Map tab's "Width"/"Length" labels are exact-text substrings of "Border width", so use
// an exact match rather than lib.mjs's fieldByLabel (a plain substring match).
const mapField = (page, label) => page.getByText(label, { exact: true }).locator('xpath=..').locator('input, select, textarea');

const imagesDir = join(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', 'site', 'public', 'images');
const out = name => join(imagesDir, name);

function saveTrimmed(gridPanelScreenshotPath) {
    // #gridPanel is a large fixed-size canvas mostly empty around the drawn rooms - the old
    // screenshots were tightly cropped to just the map itself, so trim the whitespace border
    // ImageMagick leaves after drawing (matches this repo's other post-processing conventions).
    execFileSync('magick', [gridPanelScreenshotPath, '-trim', '+repage', '-bordercolor', 'white', '-border', '15', gridPanelScreenshotPath]);
    console.log(`SAVED: ${gridPanelScreenshotPath}`);
}

// Creates a same-named exit from the currently-selected room's Exits tab (must already be open)
// to `destName`, with an optional grid `length` (0 = rooms drawn adjacent, no connecting line).
async function createExit(page, direction, destName, { length } = {}) {
    await page.getByRole('button', { name: direction, exact: true }).click();
    const destCombobox = page.locator('[role="combobox"]');
    await destCombobox.click();
    await destCombobox.fill(destName);
    await page.waitForSelector(`[role="option"]:has-text("${destName}")`, { timeout: 5000 });
    await page.click(`[role="option"]:has-text("${destName}")`);
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector(`text=${direction} → ${destName}`, { timeout: 10000 });
    if (length !== undefined) {
        // The newly-created exit's own tree row - select it via the summary link, which lands
        // straight on the exit's own Exit/Map tabs (see capture-exits.mjs's Lockedexit.png note
        // on why the summary link itself, not a separate tree click, is enough here).
        await page.getByText(`${direction} → ${destName}`, { exact: true }).click();
        await page.getByRole('button', { name: 'Map', exact: true }).click();
        await mapField(page, 'Length:').fill(String(length));
    }
}

async function freshPreview(context, page) {
    const [playerPage] = await Promise.all([
        context.waitForEvent('page', { timeout: 15000 }),
        page.click('button:has-text("Preview")'),
    ]);
    await playerPage.waitForSelector('#txtCommand', { state: 'visible', timeout: 60000 });
    await playerPage.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
    return playerPage;
}

await runCapture(async ({ page, baseUrl }) => {
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Interface');
    await toggleFeature(page, 'Map and Drawing Grid:');

    await selectTreeNode(page, 'room');
    await addElement(page, 'Add Room', 'kitchen');
    await selectTreeNode(page, 'room');
    await openTab(page, 'Exits');
    await page.getByRole('button', { name: 'south', exact: true }).click();
    const destCombobox = page.locator('[role="combobox"]');
    await destCombobox.click();
    await destCombobox.fill('kitchen');
    await page.waitForSelector('[role="option"]:has-text("kitchen")', { timeout: 5000 });
    await page.click('[role="option"]:has-text("kitchen")');
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector('text=south → kitchen', { timeout: 10000 });

    // openPreview() itself waits for #txtCommand, which is fine here (command bar is on) -
    // inlined rather than imported since we need the raw context/page anyway for a screenshot
    // of just the grid canvas afterwards.
    const context = page.context();
    const [playerPage] = await Promise.all([
        context.waitForEvent('page', { timeout: 15000 }),
        page.click('button:has-text("Preview")'),
    ]);
    await playerPage.waitForSelector('#txtCommand', { state: 'visible', timeout: 60000 });
    await playerPage.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
    await sendCommand(playerPage, 'south');

    const gridPanel = playerPage.locator('#gridPanel');
    await gridPanel.waitFor({ state: 'visible', timeout: 10000 });
    const mapPath = out('Map.png');
    await gridPanel.screenshot({ path: mapPath });
    saveTrimmed(mapPath);
    await playerPage.close();

    // --- Map2.png: same two rooms, resized/coloured/labelled (5x3 yellow lounge, 2x2 sky
    // blue kitchen) ---
    await selectTreeNode(page, 'room');
    await openTab(page, 'Map');
    await mapField(page, 'Width:').fill('5');
    await mapField(page, 'Length:').fill('3');
    await mapField(page, 'Fill colour:').fill('Yellow');
    await mapField(page, 'Label:').fill('Lounge');
    await selectTreeNode(page, 'kitchen');
    await openTab(page, 'Map');
    await mapField(page, 'Width:').fill('2');
    await mapField(page, 'Length:').fill('2');
    await mapField(page, 'Fill colour:').fill('SkyBlue');
    await mapField(page, 'Label:').fill('Kitchen');

    const [playerPage2] = await Promise.all([
        context.waitForEvent('page', { timeout: 15000 }),
        page.click('button:has-text("Preview")'),
    ]);
    await playerPage2.waitForSelector('#txtCommand', { state: 'visible', timeout: 60000 });
    await playerPage2.waitForFunction(() => window.canSendCommand === true, { timeout: 30000 });
    await sendCommand(playerPage2, 'south');
    const gridPanel2 = playerPage2.locator('#gridPanel');
    await gridPanel2.waitFor({ state: 'visible', timeout: 10000 });
    const map2Path = out('Map2.png');
    await gridPanel2.screenshot({ path: map2Path });
    saveTrimmed(map2Path);
});

await runCapture(async ({ page, baseUrl }) => {
    // --- map7.png: a huge lobby split into two locations, "Path" border types make them read
    // as one continuous room on the map despite being separate rooms with a zero-length exit
    // between them; border size 3 on both to show the effect (per the doc's own instructions).
    // Own fresh draft (not the Map.png/Map2.png one above) so no leftover kitchen/exit pollutes
    // this map. ---
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Interface');
    await toggleFeature(page, 'Map and Drawing Grid:');

    await selectTreeNode(page, 'room');
    await page.locator('span:has-text("Name:")').locator('..').locator('input').fill('Lobby W');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="Lobby W"]', { timeout: 10000 });
    await addElement(page, 'Add Room', 'Lobby E');
    await addElement(page, 'Add Room', 'Lounge');
    await addElement(page, 'Add Room', 'Kitchen');

    await selectTreeNode(page, 'Lobby W');
    await openTab(page, 'Exits');
    await createExit(page, 'east', 'Lobby E', { length: 0 });
    await selectTreeNode(page, 'Lobby E');
    await openTab(page, 'Exits');
    await createExit(page, 'north', 'Lounge');
    await selectTreeNode(page, 'Lobby E');
    await openTab(page, 'Exits');
    await createExit(page, 'east', 'Kitchen');

    // Sized generously (not the default 1x1) so the "merged" combined rectangle the Path border
    // types produce is actually visible as a wide room, matching the doc's own illustration.
    for (const [room, borderType, label] of [['Lobby W', 'Path East', 'Lobby W'], ['Lobby E', 'Path West', 'Lobby E']]) {
        await selectTreeNode(page, room);
        await openTab(page, 'Map');
        await mapField(page, 'Width:').fill('3');
        await mapField(page, 'Length:').fill('2');
        await mapField(page, 'Border width:').fill('3');
        await mapField(page, 'Border type:').selectOption({ label: borderType });
        await mapField(page, 'Label:').fill(label);
    }

    const context = page.context();
    const playerPage3 = await freshPreview(context, page);
    await sendCommand(playerPage3, 'east');
    await sendCommand(playerPage3, 'north');
    await sendCommand(playerPage3, 'south');
    await sendCommand(playerPage3, 'east');
    await sendCommand(playerPage3, 'west');
    const gridPanel3 = playerPage3.locator('#gridPanel');
    await gridPanel3.waitFor({ state: 'visible', timeout: 10000 });
    const map7Path = out('map7.png');
    await gridPanel3.screenshot({ path: map7Path });
    saveTrimmed(map7Path);
});
