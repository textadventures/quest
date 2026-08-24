// Ad-hoc manual verification for the Advanced-tree progressive-disclosure
// change: the toolbar's "Add element" dropdown should be trimmed down to
// just "Add Room" and "Add Page" (the advanced element types - functions,
// timers, etc. - moved under the "Advanced" tree node instead of cluttering
// the toolbar, and text-adventure dialogue Pages get their own dropdown
// entry), and adding a function via that Advanced panel should actually
// produce a real, selected tree node under Advanced > Functions, not just a
// silently-dropped click.
import { chromium } from './lib/tracked-chromium.mjs';

const BASE = process.argv[2] || "http://localhost:5174";

const browser = await chromium.launch();
const page = await browser.newPage();
page.on("console", msg => { if (msg.type() === "error") console.log("[console.error]", msg.text()); });
page.on("pageerror", err => console.log("[pageerror]", err));

async function run() {
    await page.goto(`${BASE}/open`, { waitUntil: "networkidle" });
    await page.getByPlaceholder("Game name").fill("Advanced Tree Test");
    await page.waitForSelector("select");
    await page.waitForTimeout(1000);
    await page.getByText("Create local draft").click();
    await page.waitForSelector("text=Advanced", { timeout: 15000 });
    await page.waitForTimeout(500);
    console.log("PASS: local draft created, editor loaded");

    // The dropdown is a sibling <div> rendered immediately after the toggle
    // button when open - scope the check to it, not the whole page, since
    // "Add Function" etc. legitimately appear elsewhere (the Advanced panel).
    // A text adventure offers both "Add Room" and "Add Page" here; the
    // advanced element types are what got trimmed out, not Pages.
    const addToolbarBtn = page.locator('button[title="Add element"]');
    await addToolbarBtn.click();
    await page.waitForTimeout(300);
    const addMenu = addToolbarBtn.locator("xpath=following-sibling::div[1]");
    const addMenuText = (await addMenu.innerText()).trim();
    if (addMenuText !== "Add Room\nAdd Page") {
        throw new Error(`Expected toolbar Add menu to contain only "Add Room" and "Add Page", got: ${JSON.stringify(addMenuText)}`);
    }
    console.log('PASS: toolbar Add dropdown is trimmed to "Add Room" and "Add Page"');
    await page.keyboard.press("Escape");

    // Select the Advanced tree node and confirm its panel offers the element
    // types that used to live directly in the toolbar dropdown. (The node no
    // longer needs expanding — branch expansion now starts collapsed-by-default
    // per #827, but the Advanced header itself stays open on load).
    // Exact match + .first() so the tree node's label wins over the game-name
    // header ("Advanced Tree Test.aslx" contains but isn't equal to "Advanced").
    await page.getByText("Advanced", { exact: true }).first().click();
    await page.waitForTimeout(500);
    const addFunctionBtn = page.getByRole("button", { name: "+ Add Function" });
    if (!(await addFunctionBtn.isVisible())) {
        throw new Error('Expected "+ Add Function" button in the Advanced panel');
    }
    console.log('PASS: Advanced panel offers "+ Add Function"');

    // Add a function and confirm it actually lands as a real, selected tree
    // node under Advanced > Functions.
    await addFunctionBtn.click();
    await page.waitForTimeout(300);
    await page.getByLabel("Name").fill("MyTestFunction");
    await page.getByRole("button", { name: "Add Function", exact: true }).last().click();
    await page.waitForTimeout(500);

    const treeNode = page.getByRole("treeitem", { name: /MyTestFunction/ });
    if (!(await treeNode.isVisible())) {
        throw new Error('Expected a "MyTestFunction" tree item to appear after adding it');
    }
    console.log('PASS: "MyTestFunction" appears as a tree item under Advanced > Functions');

    // Confirm the properties panel actually opened on the new function (not
    // some other stale/default panel) by checking its Name field's live value
    // - Svelte sets the DOM property, not the "value" attribute, so read via
    // evaluateAll rather than an attribute selector.
    const nameValue = await page.locator("input").evaluateAll(
        inputs => inputs.map(i => i.value).find(v => v === "MyTestFunction")
    );
    if (nameValue !== "MyTestFunction") {
        throw new Error('Expected the properties panel Name field to show "MyTestFunction" after adding it');
    }
    console.log('PASS: properties panel shows the new function selected, named "MyTestFunction"');

    console.log("PASS: all checks passed");
}

try {
    await run();
} catch (err) {
    console.error("FAIL:", err.message);
    await page.screenshot({ path: "/tmp/appshell-advanced-tree-failure.png", fullPage: true });
    process.exitCode = 1;
} finally {
    await browser.close();
}
