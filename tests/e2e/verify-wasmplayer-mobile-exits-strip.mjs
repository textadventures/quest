// Verifies the mobile exits strip added for issue #2183: on a narrow window the
// compass is behind the hamburger overlay, so #qv-exits repeats the available
// exits as buttons under the transcript.
//
// Checks that it:
//   - shows exactly the available exits (compass and in/out), as 44px targets,
//     without widening the page
//   - moves the player when tapped, and follows the new room's exits
//   - goes away while a get input is pending, so a tap can't become the answer
//   - stays off on the wide layout, where the compass is on screen already
//   - stays off for a game that turns the compass off (game.turnoffcompass)
//   - works for a legacy .asl game, whose compass comes from V4Game instead
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

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: PHONE });
page.on('pageerror', err => console.log('[pageerror]', err.message));

async function loadGame(url, readyText) {
    await page.goto(`${baseUrl}/?url=${url}`);
    await page.waitForSelector('#txtCommand', { timeout: 30000, state: 'attached' });
    await page.waitForFunction(
        text => document.querySelector('#divOutput').innerText.includes(text),
        readyText,
        { timeout: 15000 },
    );
}

const stripLabels = () => page.$$eval('#qv-exits .qv-exit', els => els.map(e => e.innerText.replace(/[←-⇿]/g, '').trim()));
const stripShowing = () => page.$eval('#qv-exits', el => getComputedStyle(el).display !== 'none');

async function waitForStrip(expected) {
    try {
        await page.waitForFunction(
            want => {
                const strip = document.querySelector('#qv-exits');
                if (getComputedStyle(strip).display === 'none') return false;
                const got = [...strip.querySelectorAll('.qv-exit')].map(e => e.innerText.replace(/[←-⇿]/g, '').trim());
                return JSON.stringify(got) === JSON.stringify(want);
            },
            expected,
            { timeout: 10000 },
        );
    } catch {
        const showing = await stripShowing();
        throw new Error(`exits strip should show ${JSON.stringify(expected)}, but it is ${showing ? 'showing' : 'hidden'} with ${JSON.stringify(await stripLabels())}`);
    }
}

async function sendCommand(text) {
    // The previous command's output can land before the player re-enables
    // input, and a command sent in that gap is silently dropped.
    await page.waitForFunction(() => canSendCommand, null, { timeout: 10000 });
    await page.fill('#txtCommand', text);
    await page.press('#txtCommand', 'Enter');
}

async function run() {
    // --- narrow window ----------------------------------------------------
    await loadGame('/e2e-fixtures/exits-strip-test.aslx', 'You are in a hall');

    await waitForStrip(['north', 'in']);
    console.log('PASS: strip shows the hall\'s available exits (north, in) and nothing else');

    const heights = await page.$$eval('#qv-exits .qv-exit', els => els.map(e => e.getBoundingClientRect().height));
    if (heights.some(h => h < MIN_TARGET)) {
        throw new Error(`exit buttons should be at least ${MIN_TARGET}px tall, got ${heights.join(', ')}`);
    }
    console.log(`PASS: exit buttons are ${MIN_TARGET}px+ touch targets (${heights.join(', ')})`);

    const pageOverflow = await page.evaluate(() => document.scrollingElement.scrollWidth - window.innerWidth);
    if (pageOverflow > 0) {
        throw new Error(`the strip should scroll inside itself, not widen the page - page is ${pageOverflow}px wider than the window`);
    }
    console.log('PASS: page is no wider than the window');

    // Tapping an exit moves the player, and the strip follows the new room.
    await page.click('#qv-exits .qv-exit:has-text("north")');
    await page.waitForFunction(
        () => document.querySelector('#divOutput').innerText.includes('You are in a kitchen'),
        null,
        { timeout: 10000 },
    );
    await waitForStrip(['south']);
    console.log('PASS: tapping north moves to the kitchen, and the strip now shows south');

    await page.click('#qv-exits .qv-exit:has-text("south")');
    await waitForStrip(['north', 'in']);
    await page.click('#qv-exits .qv-exit:has-text("in")');
    await waitForStrip(['out']);
    console.log('PASS: in/out exits work from the strip too');
    await page.click('#qv-exits .qv-exit:has-text("out")');
    await waitForStrip(['north', 'in']);

    // While a get input is outstanding the strip has to go: sendCommand() would
    // otherwise hand "north" to the script as its answer.
    await sendCommand('askme');
    await page.waitForFunction(
        () => document.querySelector('#divOutput').innerText.includes('What is your name?'),
        null,
        { timeout: 10000 },
    );
    await page.waitForFunction(
        () => getComputedStyle(document.querySelector('#qv-exits')).display === 'none',
        null,
        { timeout: 5000 },
    ).catch(() => {
        throw new Error('exits strip should be hidden while a get input is pending');
    });
    console.log('PASS: strip hidden while get input is pending');

    await sendCommand('Bob');
    await page.waitForFunction(
        () => document.querySelector('#divOutput').innerText.includes('Hello, Bob'),
        null,
        { timeout: 10000 },
    );
    await waitForStrip(['north', 'in']);
    console.log('PASS: strip back once the get input has been answered');

    // --- wide window ------------------------------------------------------
    await page.setViewportSize(DESKTOP);
    await page.waitForFunction(() => !document.body.classList.contains('qv-narrow'), null, { timeout: 5000 });
    if (await stripShowing()) {
        throw new Error('exits strip should not show on the wide layout, where the compass is on screen');
    }
    console.log('PASS: strip not shown on the wide layout');
    await page.setViewportSize(PHONE);

    // --- compass turned off -------------------------------------------------
    await loadGame('/e2e-fixtures/exits-strip-compass-off-test.aslx', 'You are in a hall');
    // Give the first turn's setTurnPending(false) - the call that would show it -
    // time to land before checking it didn't.
    await page.waitForFunction(() => _turnPendingReported && !_turnPending, null, { timeout: 10000 });
    if (await stripShowing()) {
        throw new Error(`exits strip should stay off when the game turns the compass off, but shows ${JSON.stringify(await stripLabels())}`);
    }
    console.log('PASS: strip stays off for a game with turnoffcompass');

    // --- legacy .asl game ---------------------------------------------------
    await loadGame('/examples/test.asl', 'room');
    await waitForStrip(['east']);
    console.log('PASS: strip shows a legacy .asl game\'s exits');

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
