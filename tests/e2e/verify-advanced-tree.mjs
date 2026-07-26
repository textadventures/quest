import { chromium } from "playwright";

const BASE = "http://localhost:5174";

const browser = await chromium.launch();
const page = await browser.newPage();
page.on("console", msg => console.log("[console]", msg.type(), msg.text()));
page.on("pageerror", err => console.log("[pageerror]", err));

await page.goto(`${BASE}/open`, { waitUntil: "networkidle" });
await page.getByPlaceholder("Game name").fill("Advanced Tree Test");
await page.waitForSelector("select");
await page.waitForTimeout(1000);
await page.getByText("Create local draft").click();
await page.waitForSelector("text=Advanced", { timeout: 15000 });
await page.waitForTimeout(500);
await page.screenshot({ path: "/tmp/1-editor.png", fullPage: true });

// Toolbar Add dropdown should now be trimmed down.
await page.locator('button[title="Add element"]').click();
await page.waitForTimeout(300);
await page.screenshot({ path: "/tmp/2-add-menu.png" });
console.log("--- Add menu items ---");
console.log(await page.locator("body").innerText());
await page.keyboard.press("Escape");

// Click the Advanced tree node.
await page.getByRole("button", { name: "Expand Advanced" }).click();
await page.waitForTimeout(500);
await page.screenshot({ path: "/tmp/3-advanced-panel.png", fullPage: true });
console.log("--- Advanced panel ---");
console.log(await page.locator("body").innerText());

// Click "+ Add Function" and confirm it lands under Advanced > Functions in the tree.
await page.getByRole("button", { name: "+ Add Function" }).click();
await page.waitForTimeout(300);
await page.getByLabel("Name").fill("MyTestFunction");
await page.getByRole("button", { name: "Add Function", exact: true }).last().click();
await page.waitForTimeout(500);
await page.screenshot({ path: "/tmp/4-after-add-function.png", fullPage: true });
console.log("--- After Add Function ---");
console.log(await page.locator("body").innerText());

await browser.close();
