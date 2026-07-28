// Ad-hoc manual verification: the editor's "in preview" warning banner
// (PreviewBanner.svelte) shows on the /open screen and survives a reload.
// Requires the AppShell dev server running locally:
//   cd src/AppShell && npm run dev
import { chromium } from "playwright";

const BASE = process.argv[2] || "http://localhost:5174";

const browser = await chromium.launch();
const page = await browser.newPage();
page.on("console", msg => { if (msg.type() === "error") console.log("[console.error]", msg.text()); });
page.on("pageerror", err => console.log("[pageerror]", err));

async function run() {
    await page.goto(`${BASE}/open`);
    await page.waitForSelector("text=Quest Viva Editor");

    // The banner's exact wording has drifted before (this script used to look
    // for "...editor is still in preview", which no longer matches
    // PreviewBanner.svelte's "...editor is in preview" and made the whole
    // script hang until a raw Playwright timeout) - match on a stable
    // substring of the component's actual text instead of copying it in full,
    // so a future copy-edit doesn't silently break this again.
    const banner = page.locator("text=editor is in preview");
    await banner.waitFor({ state: "visible", timeout: 10000 });
    console.log("PASS: preview banner is visible on /open");

    await page.reload();
    await page.waitForSelector("text=Quest Viva Editor");
    await banner.waitFor({ state: "visible", timeout: 10000 });
    console.log("PASS: preview banner is still visible after a reload");

    console.log("PASS: all checks passed");
}

try {
    await run();
} catch (err) {
    console.error("FAIL:", err.message);
    await page.screenshot({ path: "/tmp/appshell-preview-banner-failure.png", fullPage: true });
    process.exitCode = 1;
} finally {
    await browser.close();
}
