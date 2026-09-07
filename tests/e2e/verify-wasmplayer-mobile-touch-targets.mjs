// Verifies the narrow-window (phone/tablet) chrome sizing added for issue #2181:
// the hamburger button was 22px tall, side-pane rows 15px at 12px text, and the
// hyperlink verb pop-up 12px text in ~22px rows - all well under WCAG 2.2's
// 44x44 CSS px pointer target (2.5.5) and well under the 16px body text.
//
// The threshold itself lives in playercore.js's doLayout(), which publishes it
// as body.qv-narrow for playercore.css to hang the larger sizes off, so this
// checks the class actually tracks the window and that the sizes follow it -
// including back down again when the window widens, since the desktop layout
// must be untouched.
//
// The menu font size is the interesting one: game.menufontsize (9pt = 12px by
// default) is inlined into every published game, so SetMenuFontSize applies a
// floor rather than overriding the author. Both halves are checked - a default
// game gets lifted to 16px, an author asking for 20pt keeps 20pt.
//
// Two long-standing bugs in the same chrome are covered here too, since both
// only really bite once the bar is holding touch-sized controls:
//   - #qv-status is position: fixed, so its "width: 100%" resolved against the
//     viewport while the bar still started at #gameBorder's 1px left border -
//     with its own borders outside a content-box width it overran the window by
//     3px and clipped its rightmost button on a phone.
//   - the multi-open accordion's arrow is a bare .ui-icon rather than the
//     .ui-accordion-header-icon jQuery UI's centring rule targets, so it stayed
//     pinned to the top-left of a header that is now 44px tall.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

// Comfortably inside the 950px game width, so doLayout() reports narrow.
const PHONE = { width: 390, height: 844 };
// Wider than the 950px game width, so doLayout() reports wide.
const DESKTOP = { width: 1280, height: 900 };

const MIN_TARGET = 44;
const MIN_TEXT = 16;

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: PHONE });
page.on('pageerror', err => console.log('[pageerror]', err.message));

const box = sel => page.$eval(sel, el => el.getBoundingClientRect().height);
const font = sel => page.$eval(sel, el => parseFloat(getComputedStyle(el).fontSize));

async function loadGame(fixture) {
    await page.goto(`${baseUrl}/?url=/e2e-fixtures/${fixture}`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    // #txtCommand attaches before InitInterface has finished issuing its interop
    // calls (single-threaded WASM - see CLAUDE.md), and SetMenuFontSize is one of
    // them, so poll for the room description rather than racing it.
    await page.waitForFunction(
        () => document.querySelectorAll('#divOutput .elementmenu').length > 0,
        { timeout: 15000 },
    );
}

// Opens the verb pop-up on the first object hyperlink in the transcript.
async function openVerbMenu() {
    await page.click('#divOutput .elementmenu');
    await page.waitForSelector('#jjmenu_main .jj_menu_item', { timeout: 5000 });
}

async function closeVerbMenu() {
    await page.keyboard.press('Escape');
    await page.waitForFunction(() => !document.querySelector('#jjmenu_main'), { timeout: 5000 });
}

async function run() {
    await loadGame('mobile-touch-targets-test.aslx');

    // --- narrow window ----------------------------------------------------
    const narrowClass = await page.evaluate(() => document.body.classList.contains('qv-narrow'));
    if (!narrowClass) {
        throw new Error(`body.qv-narrow should be set at ${PHONE.width}px (inside the 950px game width), but it is not`);
    }
    console.log('PASS: body.qv-narrow set on a phone-width window');

    const hamburgerVisible = await page.$eval('#cmdShowPanes', el => getComputedStyle(el).display !== 'none');
    if (!hamburgerVisible) {
        throw new Error('#cmdShowPanes should be visible on a narrow window');
    }

    const hamburger = await box('#cmdShowPanes');
    if (hamburger < MIN_TARGET) {
        throw new Error(`#cmdShowPanes is ${hamburger}px tall, expected at least ${MIN_TARGET}px (issue #2181 reported 22px)`);
    }
    console.log(`PASS: hamburger is ${hamburger}px tall (>= ${MIN_TARGET}px)`);

    const save = await box('#cmdSave');
    if (save < MIN_TARGET) {
        throw new Error(`#cmdSave is ${save}px tall, expected at least ${MIN_TARGET}px`);
    }
    console.log(`PASS: Save / Load button is ${save}px tall (>= ${MIN_TARGET}px)`);

    // ...and it has to fit inside the window, rather than hanging 3px off the
    // right-hand edge with the hamburger's border clipped off.
    const overflow = await page.evaluate(() => {
        const vw = window.innerWidth;
        return [...document.querySelectorAll('body *')]
            .filter(e => e.getBoundingClientRect().right > vw + 0.5 && getComputedStyle(e).display !== 'none')
            .map(e => `${e.tagName}#${e.id || ''} right=${e.getBoundingClientRect().right.toFixed(1)}`);
    });
    if (overflow.length > 0) {
        throw new Error(`nothing should overhang a ${PHONE.width}px window, but these do: ${overflow.join(', ')}`);
    }
    console.log('PASS: no chrome overhangs the right-hand edge of a phone-width window');

    // The status bar has to have grown to hold them, and the panels below it
    // have to follow that measured height rather than the old 30px constant.
    const statusHeight = await box('#qv-status');
    if (statusHeight < MIN_TARGET) {
        throw new Error(`#qv-status is ${statusHeight}px tall, too short to hold a ${MIN_TARGET}px button`);
    }
    // #qv-status is fixed, so #divOutput's top margin is the only thing holding
    // the game text clear of it. That margin used to be a flat 20px chosen
    // against the 32px desktop bar, so a 50px bar ran the text right up under it.
    const textGap = async () => page.evaluate(() => {
        const b = s => document.querySelector(s).getBoundingClientRect();
        return +(b('#divOutput').top - b('#qv-status').bottom).toFixed(1);
    });
    const narrowGap = await textGap();
    if (Math.abs(narrowGap - 18) > 0.5) {
        throw new Error(`game text sits ${narrowGap}px below the status bar on a narrow window, expected the same 18px of daylight the desktop layout has (0px would mean it is flush against the bar)`);
    }
    console.log(`PASS: game text keeps ${narrowGap}px of daylight below the taller status bar`);

    const sidebarTop = await page.$eval('#sidebar', el => parseFloat(el.style.top));
    if (sidebarTop !== statusHeight) {
        throw new Error(`#sidebar top is ${sidebarTop}px, expected the measured #qv-status height of ${statusHeight}px - the pane overlay is still using a hard-coded offset`);
    }
    console.log(`PASS: #qv-status grew to ${statusHeight}px and #sidebar tracks it at ${sidebarTop}px`);

    // Side pane.
    await page.click('#cmdShowPanes');
    await page.waitForFunction(() => getComputedStyle(document.querySelector('#sidebar')).display === 'block', { timeout: 5000 });

    // Nothing in the opened overlay may overlap the bar it drops down from, and
    // it has to stop at the bottom of the window rather than running off it.
    const overlay = await page.evaluate(() => {
        const b = s => document.querySelector(s).getBoundingClientRect();
        const sb = document.querySelector('#sidebar');
        return {
            barBottom: b('#qv-status').bottom,
            sidebar: { top: b('#sidebar').top, bottom: b('#sidebar').bottom },
            panesTop: b('#gamePanes').top,
            viewportHeight: window.innerHeight,
            scrollableBy: sb.scrollHeight - sb.clientHeight,
        };
    });
    if (overlay.sidebar.top < overlay.barBottom - 0.5) {
        throw new Error(`#sidebar starts at ${overlay.sidebar.top}px, overlapping the status bar that ends at ${overlay.barBottom}px`);
    }
    if (overlay.panesTop < overlay.barBottom - 0.5) {
        throw new Error(`#gamePanes starts at ${overlay.panesTop}px, overlapping the status bar that ends at ${overlay.barBottom}px`);
    }
    if (overlay.sidebar.bottom > overlay.viewportHeight + 0.5) {
        throw new Error(`#sidebar runs ${(overlay.sidebar.bottom - overlay.viewportHeight).toFixed(1)}px past the bottom of the window, putting the end of a long pane out of reach`);
    }
    console.log(`PASS: pane overlay spans ${overlay.sidebar.top}-${overlay.sidebar.bottom}px, clear of the bar and inside the window`);
    if (overlay.scrollableBy !== 0) {
        throw new Error(`#sidebar is scrollable by ${overlay.scrollableBy}px with only two short panes open - its content should fit exactly`);
    }
    console.log('PASS: pane overlay has no spurious scroll with short panes');

    // ...but when the panes genuinely are taller than the window, the overlay
    // must actually scroll them. It couldn't before: #gamePanes was position:
    // fixed, so it sat outside #sidebar's flow and there was nothing for its
    // overflow-y to move, leaving the bottom of a long pane unreachable.
    await page.setViewportSize({ width: PHONE.width, height: 380 });
    await page.waitForTimeout(300);
    const scrolled = await page.evaluate(() => {
        const sb = document.querySelector('#sidebar');
        const overflow = sb.scrollHeight - sb.clientHeight;
        sb.scrollTop = sb.scrollHeight;
        return { overflow, scrollTop: sb.scrollTop };
    });
    if (scrolled.overflow <= 0) {
        throw new Error(`the pane overlay should overflow a ${380}px window, but scrollHeight matches clientHeight - the test can't prove it scrolls`);
    }
    if (scrolled.scrollTop <= 0) {
        throw new Error(`the pane overlay overflows by ${scrolled.overflow}px but will not scroll (scrollTop stayed at 0) - the bottom of a long pane is unreachable`);
    }
    console.log(`PASS: pane overlay scrolls when its content overflows (${scrolled.overflow}px of overflow, scrolled to ${scrolled.scrollTop}px)`);
    await page.setViewportSize(PHONE);
    await page.waitForTimeout(300);

    const invRow = await box('#lstInventory li');
    if (invRow < MIN_TARGET) {
        throw new Error(`inventory row is ${invRow}px tall, expected at least ${MIN_TARGET}px (issue #2181 reported 15px)`);
    }
    const invFont = await font('#lstInventory li');
    if (invFont < MIN_TEXT) {
        throw new Error(`inventory row text is ${invFont}px, expected at least ${MIN_TEXT}px (issue #2181 reported 12px)`);
    }
    console.log(`PASS: inventory rows are ${invRow}px tall at ${invFont}px text`);

    const headerHeight = await box('#inventoryLabel button.accordion-header-text');
    if (headerHeight < MIN_TARGET) {
        throw new Error(`accordion header button is ${headerHeight}px tall, expected at least ${MIN_TARGET}px`);
    }
    console.log(`PASS: accordion header button is ${headerHeight}px tall`);

    const arrow = await page.evaluate(() => {
        const h3 = document.querySelector('#inventoryLabel');
        const i = h3.querySelector('.ui-icon').getBoundingClientRect();
        const b = h3.querySelector('button.accordion-header-text').getBoundingClientRect();
        return { iconCentre: i.top + i.height / 2, btnCentre: b.top + b.height / 2, iconHeight: i.height };
    });
    if (Math.abs(arrow.iconCentre - arrow.btnCentre) > 1) {
        throw new Error(`accordion arrow centre is ${arrow.iconCentre.toFixed(1)}px but its header button's is ${arrow.btnCentre.toFixed(1)}px - the arrow is stranded against the top of the header rather than centred on it`);
    }
    if (arrow.iconHeight < 20) {
        throw new Error(`accordion arrow renders at ${arrow.iconHeight}px, expected it scaled up from the 16px sprite to suit touch chrome`);
    }
    console.log(`PASS: accordion arrow is centred on its header and renders at ${arrow.iconHeight}px`);

    await page.click('#cmdShowPanes');

    // Hyperlink verb pop-up.
    await openVerbMenu();
    const menuFont = await font('#jjmenu_main .jj_menu_item span');
    if (menuFont < MIN_TEXT) {
        throw new Error(`verb menu text is ${menuFont}px, expected at least ${MIN_TEXT}px - the game asked for 9pt (12px) and the narrow-window floor should have lifted it`);
    }
    const menuRow = await box('#jjmenu_main .jj_menu_item');
    if (menuRow < MIN_TARGET) {
        throw new Error(`verb menu row is ${menuRow}px tall, expected at least ${MIN_TARGET}px (issue #2181 reported 22px)`);
    }
    console.log(`PASS: verb menu rows are ${menuRow}px tall at ${menuFont}px text (floored up from the game's 9pt)`);
    await closeVerbMenu();

    // --- widening back out ------------------------------------------------
    // The desktop layout must be exactly as it was, so everything above has to
    // come back down again rather than being a one-way change.
    await page.setViewportSize(DESKTOP);
    await page.waitForFunction(() => !document.body.classList.contains('qv-narrow'), { timeout: 5000 });
    console.log('PASS: body.qv-narrow cleared when the window widens past the game width');

    // 32px = the 30px of CSS height plus .ui-widget-header's 1px border top and
    // bottom, which is exactly what the old hard-coded 24/32 offsets described.
    const wideStatus = await box('#qv-status');
    if (wideStatus !== 32) {
        throw new Error(`#qv-status should be back to its 32px desktop height, got ${wideStatus}px`);
    }
    const widePanesTop = await page.$eval('#gamePanes', el => parseFloat(el.style.top));
    if (widePanesTop !== 24) {
        throw new Error(`#gamePanes top should be back to the desktop 24px, got ${widePanesTop}px`);
    }
    // #gamePanes is positioned again out here, rather than laid out inside the
    // overlay, so the wide layout's 8px tuck under the bar is real.
    const widePanesPos = await page.$eval('#gamePanes', el => getComputedStyle(el).position);
    if (widePanesPos !== 'fixed') {
        throw new Error(`#gamePanes should be position: fixed on the desktop layout, got ${widePanesPos}`);
    }
    const widePanelTop = await page.$eval('#gamePanel', el => parseFloat(el.style.top));
    if (widePanelTop !== 32) {
        throw new Error(`#gamePanel top should be back to the desktop 32px, got ${widePanelTop}px`);
    }
    const wideMargin = await page.$eval('#divOutput', el => el.style.marginTop);
    if (wideMargin !== '20px') {
        throw new Error(`#divOutput margin-top should be back to the desktop 20px, got ${wideMargin}`);
    }
    const wideGap = await textGap();
    if (Math.abs(wideGap - 18) > 0.5) {
        throw new Error(`game text should sit 18px below the bar on the desktop layout, got ${wideGap}px`);
    }
    console.log('PASS: desktop layout unchanged (#gamePanes top 24px, #gamePanel top 32px, text gap 18px)');

    // The bar should span exactly #gameBorder's content box - i.e. inset by its
    // 1px border on each side - at the default width and at a game-chosen one.
    const aligned = async label => {
        const g = await page.evaluate(() => {
            const b = s => document.querySelector(s).getBoundingClientRect();
            return { border: b('#gameBorder'), status: b('#qv-status') };
        });
        const leftGap = g.status.left - g.border.left;
        const rightGap = g.border.right - g.status.right;
        if (Math.abs(leftGap - 1) > 0.5 || Math.abs(rightGap - 1) > 0.5) {
            throw new Error(`${label}: #qv-status should sit 1px inside #gameBorder on both sides, got ${leftGap.toFixed(1)}px / ${rightGap.toFixed(1)}px`);
        }
        console.log(`PASS: ${label} - #qv-status spans #gameBorder's content box exactly`);
    };
    await aligned('default width');
    await page.evaluate(() => window.setGameWidth(700));
    await page.waitForTimeout(200);
    await aligned('setGameWidth(700)');
    await page.evaluate(() => window.setGameWidth(950));
    await page.waitForTimeout(200);

    // The sticky picture frame's budget should follow the measured bar too,
    // rather than the 30px constant it used to subtract.
    const budget = await page.evaluate(() => {
        const rule = [...document.styleSheets]
            .flatMap(s => { try { return [...s.cssRules]; } catch { return []; } })
            .find(r => r.selectorText === 'div#gamePanel img');
        const bar = document.querySelector('#qv-status').getBoundingClientRect().height;
        return { maxHeight: rule && rule.style.maxHeight, expected: `${(window.innerHeight - bar) * 0.5}px` };
    });
    if (budget.maxHeight !== budget.expected) {
        throw new Error(`picture frame max-height is ${budget.maxHeight}, expected ${budget.expected} (window height less the measured status bar, halved)`);
    }
    console.log(`PASS: picture frame budget follows the measured status bar (${budget.maxHeight})`);

    await openVerbMenu();
    const wideMenuFont = await font('#jjmenu_main .jj_menu_item span');
    if (Math.abs(wideMenuFont - 12) > 0.5) {
        throw new Error(`verb menu on desktop should be the game's own 9pt (12px), got ${wideMenuFont}px - the floor is leaking into the desktop layout`);
    }
    console.log(`PASS: verb menu back to the game's own 9pt (${wideMenuFont}px) on desktop`);
    await closeVerbMenu();

    // --- an author who asked for more keeps it ----------------------------
    await page.setViewportSize(PHONE);
    await loadGame('mobile-touch-targets-large-menu-test.aslx');
    await openVerbMenu();
    const bigMenuFont = await font('#jjmenu_main .jj_menu_item span');
    // 20pt = 26.666...px
    if (Math.abs(bigMenuFont - 80 / 3) > 0.5) {
        throw new Error(`a game asking for 20pt should still get 20pt (26.67px) on a narrow window, got ${bigMenuFont}px - the floor is clamping instead of flooring`);
    }
    console.log(`PASS: a game asking for 20pt still gets ${bigMenuFont}px on a narrow window`);
    await closeVerbMenu();

    console.log('\nAll checks passed.');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
