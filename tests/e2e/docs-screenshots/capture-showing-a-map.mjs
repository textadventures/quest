// Regenerates Map.png and Map2.png from site/src/content/docs/howto/tasks/showing_a_map.md -
// the remaining 5 images on that page (map3-7) illustrate specific multi-room layout edge cases
// (matched-loop distances, path border types, up/down levels) that need much more elaborate
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
