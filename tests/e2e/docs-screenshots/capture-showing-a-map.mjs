// Regenerates all 7 images from site/src/content/docs/howto/tasks/showing_a_map.md.
// See .claude/skills/docs-screenshots/SKILL.md.
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

function saveTrimmed(gridPanelScreenshotPath, { chopTop } = {}) {
    // #gridPanel is a large fixed-size canvas mostly empty around the drawn rooms - the old
    // screenshots were tightly cropped to just the map itself, so trim the whitespace border
    // ImageMagick leaves after drawing (matches this repo's other post-processing conventions).
    const args = [gridPanelScreenshotPath];
    if (chopTop) {
        // The panel has its own thin coloured top border, only reachable by -trim (and thus
        // wrongly kept as "content") on a tall enough canvas/layout that nothing else reaches
        // close to true y=0 - confirmed only needed for map6.png's up/down-level layout.
        args.push('-chop', `0x${chopTop}+0+0`);
    }
    args.push('-trim', '+repage', '-bordercolor', 'white', '-border', '15', gridPanelScreenshotPath);
    execFileSync('magick', args);
    console.log(`SAVED: ${gridPanelScreenshotPath}`);
}

// Creates a same-named exit from the currently-selected room's Exits tab (must already be open)
// to `destName`, with an optional grid `length` (0 = rooms drawn adjacent, no connecting line).
async function createExit(page, roomName, direction, destName, { length } = {}) {
    await page.getByRole('button', { name: direction, exact: true }).click();
    const destCombobox = page.locator('[role="combobox"]');
    await destCombobox.click();
    await destCombobox.fill(destName);
    await page.waitForSelector(`[role="option"]:has-text("${destName}")`, { timeout: 5000 });
    await page.click(`[role="option"]:has-text("${destName}")`);
    await page.click('button:has-text("Create exit")');
    await page.waitForSelector(`text=${direction} → ${destName}`, { timeout: 10000 });
    if (length !== undefined) {
        await setExitLength(page, roomName, destName, length);
    }
}

// Sets the grid length on a specific exit, selected via its own TREE ROW ("Exit: <destName>"),
// not the "direction → destName" summary link on the Exits tab - that link selects the
// *destination room*, not the exit itself (see capture-exits.mjs's Lockedexit.png note). The
// tree row lookup is scoped to `roomName`'s own treeitem (via a `has:` filter on its
// [data-value] node), confirmed live via read_page that the tree really does nest each room's
// exits as DOM descendants of that room's own treeitem/group - a page-wide text search breaks
// as soon as two different rooms have an exit to the same destination name, since both render
// an identically-labelled "Exit: <destName>" row.
async function setExitLength(page, roomName, destName, length) {
    // A room's `[data-value]` is shared by 3 different elements: the real ARIA treeitem
    // (data-part="branch"), the clickable label row selectTreeNode() targets
    // (data-part="branch-control"), and the (possibly hidden) content container holding its
    // children (data-part="branch-content") - confirmed live via the strict-mode error listing
    // all three. Selecting the room (branch-control) does NOT itself expand it - aria-expanded
    // stayed "false" even once selected/re-created, so the branch-content stayed hidden and
    // unclickable. Expand explicitly via its own chevron button first.
    await selectTreeNode(page, roomName);
    const branchControl = page.locator(`[data-value="${roomName}"][data-part="branch-control"]`);
    if ((await branchControl.getAttribute('data-state')) !== 'open') {
        await branchControl.getByRole('button', { name: 'Expand' }).click();
    }
    const branchContent = page.locator(`[data-value="${roomName}"][data-part="branch-content"]`);
    await branchContent.getByText(`Exit: ${destName}`, { exact: true }).click();
    await page.getByRole('button', { name: 'Map', exact: true }).click();
    await mapField(page, 'Length:').fill(String(length));
}

// Sets the grid length on an exit's own reciprocal side - e.g. after creating "east" from
// roomA to roomB, this selects roomB and its own auto-created exit back to roomA, and sets its
// length too. Exit length is per-direction, not shared, so the doc's "remember to change both
// directions" instruction has to be done as two separate edits.
async function setReciprocalExitLength(page, roomName, destName, length) {
    await selectTreeNode(page, roomName);
    await page.getByRole('button', { name: 'Exits', exact: true }).click();
    await setExitLength(page, roomName, destName, length);
}

// The player dot (and, after it, the view's pan/recentre offset) animate toward their new
// position over several requestAnimationFrame ticks rather than snapping instantly - see
// src/PlayerCore/Resources/grid.js's onFrame()/gridApi.drawPlayer (playerVector/offsetVector
// are plain top-level `var`s in a non-module script, so genuinely reachable as window globals).
// sendCommand() only waits for the game-logic turn to finish, not this separate canvas
// animation, so a screenshot taken right after the last movement command can catch the dot
// mid-slide or the view mid-pan. Wait for both vectors to null out (onFrame's own "arrived"
// signal) before every grid screenshot in this file.
async function waitForGridAnimation(playerPage) {
    await playerPage.waitForFunction(
        () => window.playerVector == null && window.offsetVector == null,
        { timeout: 10000 },
    );
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
    await waitForGridAnimation(playerPage);
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
    await waitForGridAnimation(playerPage2);
    const map2Path = out('Map2.png');
    await gridPanel2.screenshot({ path: map2Path });
    saveTrimmed(map2Path);
});

await runCapture(async ({ page, baseUrl }) => {
    // --- map7.png: a huge lobby split into two locations, "Path" border types make them read
    // as one continuous room on the map despite being separate rooms with a zero-length exit
    // between them; border size 3 on both to show the effect (per the doc's own instructions).
    // Rebuilt against the actual original screenshot (site/public/images/map7.png as it existed
    // before this track started, pulled from commit 9c6f2cc5) after the first pass here missed
    // several details the user caught: Lobby needs BOTH directions of its exit to Lobby E set to
    // length 0 (only one side was set before, leaving a stray visible connector - the doc's own
    // "remember to change both directions" lesson, missed here even after applying it correctly
    // elsewhere on this same page); Lobby/Lounge/Kitchen all have fill colours in the original;
    // the Lobby E -> Lounge exit is diagonal (northeast), not straight north; Lounge and Kitchen
    // are labelled; and Lounge/Kitchen/Lobby W each have one extra stray exit to an offscreen
    // room, which is why the original shows extra line stubs trailing off the crop edges. Own
    // fresh draft (not the Map.png/Map2.png one above) so no leftover kitchen/exit pollutes this
    // map. ---
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
    // Stub destinations for the original's extra stray exit stubs (Lobby W north, Lounge east,
    // Kitchen east/south) - never visited long enough to matter what's in them, just need to
    // exist so their connecting line has somewhere to point.
    await addElement(page, 'Add Room', 'Corridor');
    await addElement(page, 'Add Room', 'Study');
    await addElement(page, 'Add Room', 'Pantry');
    await addElement(page, 'Add Room', 'Cellar');

    await selectTreeNode(page, 'Lobby W');
    await openTab(page, 'Exits');
    await createExit(page, 'Lobby W', 'east', 'Lobby E', { length: 0 });
    await setReciprocalExitLength(page, 'Lobby E', 'Lobby W', 0);
    await selectTreeNode(page, 'Lobby W');
    await openTab(page, 'Exits');
    await createExit(page, 'Lobby W', 'north', 'Corridor');
    await selectTreeNode(page, 'Lobby E');
    await openTab(page, 'Exits');
    await createExit(page, 'Lobby E', 'northeast', 'Lounge');
    await selectTreeNode(page, 'Lobby E');
    await openTab(page, 'Exits');
    await createExit(page, 'Lobby E', 'east', 'Kitchen');
    await selectTreeNode(page, 'Lounge');
    await openTab(page, 'Exits');
    await createExit(page, 'Lounge', 'east', 'Study');
    await selectTreeNode(page, 'Kitchen');
    await openTab(page, 'Exits');
    await createExit(page, 'Kitchen', 'east', 'Pantry');
    await selectTreeNode(page, 'Kitchen');
    await openTab(page, 'Exits');
    await createExit(page, 'Kitchen', 'south', 'Cellar');

    // Sized generously (not the default 1x1) so the "merged" combined rectangle the Path border
    // types produce is actually visible as a wide room, matching the doc's own illustration.
    for (const [room, borderType, label, colour] of [
        ['Lobby W', 'Path East', 'Lobby W', 'PeachPuff'],
        ['Lobby E', 'Path West', 'Lobby E', 'PeachPuff'],
    ]) {
        await selectTreeNode(page, room);
        await openTab(page, 'Map');
        await mapField(page, 'Width:').fill('3');
        await mapField(page, 'Length:').fill('2');
        await mapField(page, 'Border width:').fill('3');
        await mapField(page, 'Border type:').selectOption({ label: borderType });
        await mapField(page, 'Label:').fill(label);
        await mapField(page, 'Fill colour:').fill(colour);
    }
    for (const [room, colour] of [['Lounge', 'Yellow'], ['Kitchen', 'SkyBlue']]) {
        await selectTreeNode(page, room);
        await openTab(page, 'Map');
        await mapField(page, 'Width:').fill('2');
        await mapField(page, 'Length:').fill('2');
        await mapField(page, 'Label:').fill(room);
        await mapField(page, 'Fill colour:').fill(colour);
    }

    const context = page.context();
    const playerPage3 = await freshPreview(context, page);
    await sendCommand(playerPage3, 'north');
    await sendCommand(playerPage3, 'south');
    await sendCommand(playerPage3, 'east');
    await sendCommand(playerPage3, 'northeast');
    await sendCommand(playerPage3, 'east');
    await sendCommand(playerPage3, 'west');
    await sendCommand(playerPage3, 'southwest');
    await sendCommand(playerPage3, 'east');
    await sendCommand(playerPage3, 'east');
    await sendCommand(playerPage3, 'west');
    await sendCommand(playerPage3, 'south');
    await sendCommand(playerPage3, 'north');
    await sendCommand(playerPage3, 'west');
    const gridPanel3 = playerPage3.locator('#gridPanel');
    await gridPanel3.waitFor({ state: 'visible', timeout: 10000 });
    await waitForGridAnimation(playerPage3);
    const map7Path = out('map7.png');
    await gridPanel3.screenshot({ path: map7Path });
    saveTrimmed(map7Path);
});

await runCapture(async ({ page, baseUrl }) => {
    // --- map3/map4/map5: a growing loop. map3 is a 3-room diagonal loop (Lounge/Lobby/Kitchen,
    // all 2x2, "so the diagonal is fine") plus Garden hanging off Kitchen, not yet part of any
    // loop. map4 adds a second loop back through Garden->Gazebo->Garage->Kitchen, but with
    // mismatched distances so the two new exits don't visually meet ("the lengths do not
    // match"). map5 is the fixed version, matching the doc's own worked horizontal arithmetic
    // (Garden half-width 3 + exit 1 + Gazebo half-width 1 = 5; Kitchen half-width 1 + exit +
    // Garage half-width, adjusting Garage to width 2 and the exit to length 3, gives the same
    // 5) - the vertical pairing (Kitchen-Garden, already existing, vs the new Garage-Gazebo) is
    // matched too, by sizing Garage/Gazebo the same 2x2 as Kitchen so the *default* exit length
    // already lines up on that axis without needing its own explicit adjustment. ---
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Interface');
    await toggleFeature(page, 'Map and Drawing Grid:');
    // Default grid canvas height (300px) clips the top of this layout (Lounge ends up out of
    // frame above Kitchen) - confirmed live by screenshotting the raw player page, not just the
    // cropped #gridPanel. Give it more room.
    await mapField(page, 'Height (pixels):').fill('550');

    // Player starts here - rename the default room to "Kitchen" to match where the doc's own
    // map3.png shows the player dot.
    await selectTreeNode(page, 'room');
    await page.locator('span:has-text("Name:")').locator('..').locator('input').fill('Kitchen');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="Kitchen"]', { timeout: 10000 });
    await addElement(page, 'Add Room', 'Lounge');
    await addElement(page, 'Add Room', 'Lobby');
    await addElement(page, 'Add Room', 'Garden');

    for (const [room, size] of [['Kitchen', [2, 2]], ['Lounge', [2, 2]], ['Lobby', [2, 2]], ['Garden', [6, 2]]]) {
        await selectTreeNode(page, room);
        await openTab(page, 'Map');
        await mapField(page, 'Width:').fill(String(size[0]));
        await mapField(page, 'Length:').fill(String(size[1]));
        await mapField(page, 'Label:').fill(room);
    }

    await selectTreeNode(page, 'Kitchen');
    await openTab(page, 'Exits');
    await createExit(page, 'Kitchen', 'north', 'Lounge');
    await selectTreeNode(page, 'Kitchen');
    await openTab(page, 'Exits');
    await createExit(page, 'Kitchen', 'west', 'Lobby');
    await selectTreeNode(page, 'Kitchen');
    await openTab(page, 'Exits');
    await createExit(page, 'Kitchen', 'south', 'Garden');
    await selectTreeNode(page, 'Lobby');
    await openTab(page, 'Exits');
    await createExit(page, 'Lobby', 'northeast', 'Lounge');

    const context = page.context();

    // --- map3.png: the 3-room loop plus Garden, player ends back in Kitchen ---
    const playerPage3a = await freshPreview(context, page);
    await sendCommand(playerPage3a, 'north');
    await sendCommand(playerPage3a, 'south');
    await sendCommand(playerPage3a, 'west');
    await sendCommand(playerPage3a, 'east');
    await sendCommand(playerPage3a, 'south');
    await sendCommand(playerPage3a, 'north');
    const gridPanel3a = playerPage3a.locator('#gridPanel');
    await gridPanel3a.waitFor({ state: 'visible', timeout: 10000 });
    await waitForGridAnimation(playerPage3a);
    const map3Path = out('map3.png');
    await gridPanel3a.screenshot({ path: map3Path });
    saveTrimmed(map3Path);
    await playerPage3a.close();

    // --- map4.png: add Gazebo/Garage closing a second loop (Garden-Gazebo-Garage-Kitchen),
    // Garage left at its original 1x1 default size and both new exits at default length, so the
    // distances don't match and the exits don't meet ---
    await addElement(page, 'Add Room', 'Gazebo');
    await addElement(page, 'Add Room', 'Garage');
    await selectTreeNode(page, 'Gazebo');
    await openTab(page, 'Map');
    await mapField(page, 'Width:').fill('2');
    await mapField(page, 'Length:').fill('2');
    await mapField(page, 'Label:').fill('Gazebo');

    await selectTreeNode(page, 'Garden');
    await openTab(page, 'Exits');
    await createExit(page, 'Garden', 'east', 'Gazebo');
    await selectTreeNode(page, 'Kitchen');
    await openTab(page, 'Exits');
    await createExit(page, 'Kitchen', 'east', 'Garage');
    await selectTreeNode(page, 'Gazebo');
    await openTab(page, 'Exits');
    await createExit(page, 'Gazebo', 'north', 'Garage');

    const playerPage4 = await freshPreview(context, page);
    await sendCommand(playerPage4, 'north');
    await sendCommand(playerPage4, 'south');
    await sendCommand(playerPage4, 'west');
    await sendCommand(playerPage4, 'east');
    await sendCommand(playerPage4, 'south');
    await sendCommand(playerPage4, 'east');
    await sendCommand(playerPage4, 'north');
    const gridPanel4 = playerPage4.locator('#gridPanel');
    await gridPanel4.waitFor({ state: 'visible', timeout: 10000 });
    await waitForGridAnimation(playerPage4);
    const map4Path = out('map4.png');
    await gridPanel4.screenshot({ path: map4Path });
    saveTrimmed(map4Path);
    await playerPage4.close();

    // --- map5.png: fixed - Garage resized to 2x2 (half-width 1, matching the doc's "we will
    // make the garage 2 units wide") and the Kitchen<->Garage exit set to length 3 on both
    // sides (1 + 3 + 1 = 5, matching Garden<->Gazebo's 3 + 1 + 1 = 5) ---
    await selectTreeNode(page, 'Garage');
    await openTab(page, 'Map');
    await mapField(page, 'Width:').fill('2');
    await mapField(page, 'Length:').fill('2');
    await mapField(page, 'Label:').fill('Garage');
    await setReciprocalExitLength(page, 'Kitchen', 'Garage', 3);
    await setReciprocalExitLength(page, 'Garage', 'Kitchen', 3);

    const playerPage5 = await freshPreview(context, page);
    await sendCommand(playerPage5, 'north');
    await sendCommand(playerPage5, 'south');
    await sendCommand(playerPage5, 'west');
    await sendCommand(playerPage5, 'east');
    await sendCommand(playerPage5, 'south');
    await sendCommand(playerPage5, 'east');
    await sendCommand(playerPage5, 'north');
    const gridPanel5 = playerPage5.locator('#gridPanel');
    await gridPanel5.waitFor({ state: 'visible', timeout: 10000 });
    await waitForGridAnimation(playerPage5);
    const map5Path = out('map5.png');
    await gridPanel5.screenshot({ path: map5Path });
    saveTrimmed(map5Path);
});

await runCapture(async ({ page, baseUrl }) => {
    // --- map6.png: up/down exits - going up a level shows the room(s) on the previous level
    // faded in the background. Own fresh draft. Kitchen (ground floor, coloured to match
    // Map2.png's own Kitchen/Lounge scheme, since the doc's own map6.png appears to carry that
    // styling over) is the starting room; "up" leads to Landing, which has Bedroom (east) and
    // Lounge (north, same colour scheme again) on the new level - player ends in Bedroom,
    // directly over where Kitchen was, so Kitchen should render faded behind it. ---
    await createLocalDraft(page, baseUrl, 'Tutorial Game');
    await selectTreeNode(page, 'game');
    await openTab(page, 'Interface');
    await toggleFeature(page, 'Map and Drawing Grid:');
    await mapField(page, 'Height (pixels):').fill('450');

    await selectTreeNode(page, 'room');
    await page.locator('span:has-text("Name:")').locator('..').locator('input').fill('Kitchen');
    await openTab(page, 'Room');
    await page.waitForSelector('[data-value="Kitchen"]', { timeout: 10000 });
    await addElement(page, 'Add Room', 'Landing');
    await addElement(page, 'Add Room', 'Bedroom');
    await addElement(page, 'Add Room', 'Lounge');

    // Only Kitchen/Lounge are labelled+coloured here (matching Map2.png's own scheme) -
    // confirmed live that adding Landing/Bedroom into this same loop, even unlabelled, somehow
    // knocks out Kitchen's faded-behind-Bedroom rendering entirely (and the crop framing) in a
    // way not worth chasing further; the whole point of this capture is the faded-level effect,
    // so keep the sequence that reliably produces it.
    for (const [room, colour] of [['Kitchen', 'SkyBlue'], ['Lounge', 'Yellow']]) {
        await selectTreeNode(page, room);
        await openTab(page, 'Map');
        await mapField(page, 'Fill colour:').fill(colour);
        await mapField(page, 'Label:').fill(room);
    }

    await selectTreeNode(page, 'Kitchen');
    await openTab(page, 'Exits');
    await createExit(page, 'Kitchen', 'up', 'Landing');
    await selectTreeNode(page, 'Landing');
    await openTab(page, 'Exits');
    await createExit(page, 'Landing', 'east', 'Bedroom');
    await selectTreeNode(page, 'Landing');
    await openTab(page, 'Exits');
    await createExit(page, 'Landing', 'north', 'Lounge');

    const context = page.context();
    const playerPage6 = await freshPreview(context, page);
    await sendCommand(playerPage6, 'up');
    await sendCommand(playerPage6, 'north');
    await sendCommand(playerPage6, 'south');
    await sendCommand(playerPage6, 'east');
    const gridPanel6 = playerPage6.locator('#gridPanel');
    await gridPanel6.waitFor({ state: 'visible', timeout: 10000 });
    await waitForGridAnimation(playerPage6);
    const map6Path = out('map6.png');
    await gridPanel6.screenshot({ path: map6Path });
    saveTrimmed(map6Path, { chopTop: 6 });
});
