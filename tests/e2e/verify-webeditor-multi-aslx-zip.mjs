// Ad-hoc manual verification, Firefox: importing a .zip that contains more than
// one .aslx file (split-file games, custom libraries, or just a zip a user put
// together by hand rather than one produced by Backup) should show a picker to
// choose which one to open as the main game — same UX as the FSA folder-open
// flow's "multiple game files found" picker — instead of only ever recognizing
// a fixed "game.aslx" entry name. Also simulates a real macOS Finder "Compress"
// zip of a folder: entries prefixed with the folder name, plus a __MACOSX/
// sidecar directory of AppleDouble resource-fork files and a .DS_Store — both
// of which used to make it into the picker and then fail to load with
// "TypeError: Name is invalid" (OPFS filenames can't contain "/").
//
// Requires the WebEditor dev server running against a Release WasmEditor build.
import { firefox } from 'playwright';
import { zipSync } from 'fflate';
import { writeFileSync, readFileSync } from 'node:fs';

const baseUrl = process.argv[2] || 'http://localhost:5174';

// Two distinct games, each with their own gameid, zipped together under
// non-default names — deliberately not "game.aslx" for either. Based on the
// known-working restart-test.aslx fixture (with a <gameid> injected — that
// fixture has none, but our import flow requires one) rather than hand-rolled
// minimal XML, so a real Initialise() call succeeds.
const baseAslx = readFileSync(new URL('./fixtures/restart-test.aslx', import.meta.url), 'utf8');
function withGameId(gameId) {
    return baseAslx.replace('<game name="Simple">', `<game name="Simple">\n    <gameid>${gameId}</gameid>`);
}
// Folder-prefixed, like a real "Compress" of a folder, plus __MACOSX/ junk —
// this is what actually broke: OPFS can't take "/" in a filename, and the
// AppleDouble ._*.aslx sidecars were showing up in the picker as if real.
const zipPath = '/tmp/webeditor-multi-aslx-test.zip';
writeFileSync(zipPath, zipSync({
    'Test Folder/Main.aslx': new TextEncoder().encode(withGameId('11111111-1111-1111-1111-111111111111')),
    'Test Folder/Library.aslx': new TextEncoder().encode(withGameId('22222222-2222-2222-2222-222222222222')),
    'Test Folder/readme.txt': new TextEncoder().encode('not a game file'),
    'Test Folder/restart-test.js': readFileSync(new URL('./fixtures/restart-test.js', import.meta.url)),
    '__MACOSX/Test Folder/._Main.aslx': new Uint8Array([0, 5, 22, 7]),
    '__MACOSX/Test Folder/._Library.aslx': new Uint8Array([0, 5, 22, 7]),
    '.DS_Store': new Uint8Array([0, 1, 2, 3]),
}));

const browser = await firefox.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));

try {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Import game file")', { timeout: 30000 });

    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        page.click('button:has-text("Import game file")'),
    ]);
    await fileChooser.setFiles(zipPath);

    await page.waitForSelector('text=Multiple game files found', { timeout: 10000 });
    const mainVisible = await page.isVisible('button:has-text("Main.aslx")');
    const libVisible = await page.isVisible('button:has-text("Library.aslx")');
    const readmeVisible = await page.isVisible('button:has-text("readme.txt")');
    console.log('picker shows Main.aslx:', mainVisible, 'Library.aslx:', libVisible, 'readme.txt (should be false):', readmeVisible);
    if (!mainVisible || !libVisible || readmeVisible) throw new Error('Picker did not list exactly the two .aslx entries');

    await page.click('button:has-text("Library.aslx")');
    await page.waitForSelector('button:has-text("🖼 Assets")', { timeout: 30000 });
    const filenameShown = await page.isVisible('text=Library.aslx');
    console.log('opened Library.aslx (preserving its real name):', filenameShown);
    if (!filenameShown) throw new Error('Did not open the chosen entry under its own filename');

    console.log('PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/webeditor-multi-aslx-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
