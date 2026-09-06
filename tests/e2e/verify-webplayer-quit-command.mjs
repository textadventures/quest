// Verifies GitHub issue #1530: a legacy Quest 5 game whose "quit" command
// script calls `request (Quit, "")` (rather than the modern `finish`) used to
// throw "The method or operation is not implemented." — WebPlayer's
// IPlayer.Quit() was a bare `throw new NotImplementedException()` stub, which
// aborted the request script before the WorldModel.Finish() call after it
// could run. Every IPlayer implementation's Quit() was a no-op anyway (game-
// over is fully driven by WorldModel.Finish() raising the Finished event), so
// Quit() was removed from IPlayer entirely and the RequestScript Quit case
// now just calls Finish(). examples/blank.aslx is a real Quest 5-saved game
// whose "quit" command still uses `request (Quit, "")`, making it a natural
// fixture here.
//
// Requires WebPlayer running locally in Development mode:
//   ASPNETCORE_ENVIRONMENT=Development dotnet run --configuration Release \
//     --no-launch-profile --urls http://localhost:5099 --project src/WebPlayer
import { chromium } from 'playwright';
import path from 'node:path';

const baseUrl = process.argv[2] || 'http://localhost:5099';

const browser = await chromium.launch();
const page = await browser.newPage();
const bootErrors = [];
const pageErrors = [];
let booted = false;
page.on('pageerror', err => (booted ? pageErrors : bootErrors).push(err.message));
page.on('console', msg => { if (msg.type() === 'error') (booted ? pageErrors : bootErrors).push(msg.text()); });

try {
    await page.goto(`${baseUrl}/dev/open`);
    await page.waitForTimeout(1000);
    const fileInput = await page.$('input[type=file]');
    await fileInput.setInputFiles(path.resolve('../../examples/blank.aslx'));

    await page.waitForSelector('#txtCommand', { state: 'visible', timeout: 30000 });
    await page.waitForTimeout(1000); // let unrelated async boot errors (e.g. grid/PaperScript loading) settle before we start watching
    booted = true;
    console.log('Legacy game booted via /dev/open. Pre-existing boot errors ignored:', bootErrors);

    await page.fill('#txtCommand', 'quit');
    await page.press('#txtCommand', 'Enter');
    await page.waitForTimeout(1000);

    const finishedPaneVisible = await page.isVisible('#gamePanesFinished');
    const commandBarVisible = await page.isVisible('#txtCommandDiv');
    console.log('Finished pane visible after quit (expect true):', finishedPaneVisible);
    console.log('Command bar visible after quit (expect false):', commandBarVisible);
    console.log('Page/console errors seen (expect none):', pageErrors);

    if (pageErrors.some(e => e.toLowerCase().includes('not implemented'))) {
        throw new Error(`FAIL: "not implemented" error was raised by request (Quit, ""): ${JSON.stringify(pageErrors)}`);
    }
    if (pageErrors.length > 0) {
        throw new Error(`FAIL: unexpected page/console error(s) during quit: ${JSON.stringify(pageErrors)}`);
    }
    if (!finishedPaneVisible) {
        throw new Error('FAIL: #gamePanesFinished was not shown after the quit command — game did not reach the finished state.');
    }
    if (commandBarVisible) {
        throw new Error('FAIL: #txtCommandDiv is still visible after the quit command — interface was not disabled.');
    }
    console.log('PASS');
} catch (err) {
    console.error(err.message || err);
    process.exitCode = 1;
} finally {
    await browser.close();
}
