// Ad-hoc manual verification: the ShowMenu dialog (player.js, shared by
// WasmPlayer and WebPlayer) used jQuery UI's default 300px dialog width, so a
// menu whose options were wider than that had them silently clipped - the
// dialog's <select> kept its full intrinsic width and simply overflowed out of
// sight, with no horizontal scrollbar. Fixed by opening the dialog with
// width: "auto" (capped in playercore.css so it can't overflow the window) and
// by sizing the <select> to the number of options instead of always showing
// three. See https://github.com/alexwarren/quest/issues/1479.
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();

async function openMenu(page, command) {
    await page.fill('#txtCommand', command);
    await page.press('#txtCommand', 'Enter');
    await page.waitForSelector('#dialogOptions option', { timeout: 10000 });
    // Let jQuery UI finish sizing/positioning the dialog.
    await page.waitForTimeout(300);
    return page.evaluate(() => {
        const select = document.getElementById('dialogOptions');
        const content = document.getElementById('dialog');
        const dialog = document.querySelector('.ui-dialog');
        const rect = dialog.getBoundingClientRect();
        return {
            dialogLeft: rect.left,
            dialogRight: rect.right,
            windowWidth: window.innerWidth,
            // The dialog's content area is what clipped the oversized
            // <select>, so that's where the overflow shows up - the <select>
            // itself always reports its full intrinsic width.
            contentClientWidth: content.clientWidth,
            contentScrollWidth: content.scrollWidth,
            selectSize: select.size,
            optionCount: select.options.length,
        };
    });
}

async function run(viewport, label) {
    const page = await browser.newPage({ viewport });
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    try {
        await page.goto(`${baseUrl}/?url=/examples/test.aslx`);
        await page.waitForSelector('#txtCommand', { timeout: 30000 });
        console.log(`PASS: [${label}] game booted`);

        // "menulong" shows a menu whose options are far wider than the old
        // 300px default dialog width.
        const long = await openMenu(page, 'menulong');

        if (long.dialogRight > long.windowWidth || long.dialogLeft < 0) {
            throw new Error(
                `[${label}] Dialog is outside the window: left=${long.dialogLeft}, ` +
                `right=${long.dialogRight}, windowWidth=${long.windowWidth}`);
        }
        console.log(`PASS: [${label}] dialog fits inside the window ` +
            `(${Math.round(long.dialogLeft)}-${Math.round(long.dialogRight)} of ${long.windowWidth})`);

        // The dialog can only grow to the window's edge, so on a narrow
        // viewport some clipping is unavoidable - a <select size="n"> listbox
        // can't wrap its options. Only assert nothing is clipped where there
        // was room for it.
        if (viewport.width >= 1024) {
            if (long.contentScrollWidth > long.contentClientWidth) {
                throw new Error(
                    `[${label}] Options are clipped: the dialog's content area is ` +
                    `${long.contentClientWidth}px wide but needs ${long.contentScrollWidth}px`);
            }
            console.log(`PASS: [${label}] no options clipped ` +
                `(content area ${long.contentClientWidth}px wide, needs ${long.contentScrollWidth}px)`);
        }

        // The dialog still works: choose the (pre-selected) first option.
        await page.click('button:has-text("Select")');
        await page.waitForTimeout(300);
        const output = await page.$eval('#divOutput', el => el.textContent);
        if (!output.includes('You chose: Ask the innkeeper about the strange lights')) {
            throw new Error(
                `[${label}] Expected the chosen option to be echoed, got tail: ${output.slice(-120)}`);
        }
        console.log(`PASS: [${label}] menu choice was submitted`);

        // "menufn" has four options - more than the markup's fallback
        // size of 3, which used to scroll the last one out of sight with no
        // hint that it was there.
        const four = await openMenu(page, 'menufn');
        if (four.optionCount !== 4) {
            throw new Error(`[${label}] Expected 4 options from "menufn", got ${four.optionCount}`);
        }
        if (four.selectSize < four.optionCount) {
            throw new Error(
                `[${label}] Options list shows only ${four.selectSize} of ` +
                `${four.optionCount} options`);
        }
        console.log(`PASS: [${label}] all ${four.optionCount} options visible without scrolling`);
    } finally {
        await page.close();
    }
}

try {
    await run({ width: 1280, height: 800 }, 'desktop');
    await run({ width: 375, height: 700 }, 'mobile');
    console.log('PASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
