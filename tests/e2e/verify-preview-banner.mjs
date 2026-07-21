import { chromium } from "playwright";

const BASE = "http://localhost:5180";

const browser = await chromium.launch();
const page = await browser.newPage();

await page.goto(`${BASE}/open`);
await page.waitForSelector("text=Quest Viva Editor");

const banner = page.locator("text=Quest Viva's editor is still in preview");
await banner.waitFor({ state: "visible" });
console.log("Banner visible:", await banner.isVisible());

await page.screenshot({ path: "/tmp/preview-banner.png", fullPage: true });

await page.reload();
await page.waitForSelector("text=Quest Viva Editor");
await banner.waitFor({ state: "visible" });
console.log("Banner still visible after reload:", await banner.isVisible());

await browser.close();
