// Ad-hoc manual verification for the new object Verbs editor tab: previously it rendered
// nothing. Checks adding a custom verb (default "Print a message" behaviour), switching its
// behaviour to "Run a script" and "Require another object", the clash-message shown when a
// pattern collides with a built-in command, delete, and undo.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage();
page.on('pageerror', err => console.log('[pageerror]', err.message));
page.on('console', msg => { if (msg.type() === 'error') console.log('[console.error]', msg.text()); });

async function addRoom(name) {
    await page.click('button[title="Add element"]');
    await page.click('button:has-text("Add Room")', { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

// Objects can only be added under a room/object (there's no bare top-level "Add Object"), so a
// room must be selected first — the toolbar's Add menu then offers "Add Object in <room>".
async function addObjectIn(parentRoom, name) {
    await selectTreeNode(parentRoom);
    await page.click('button[title="Add element"]');
    await page.click(`button:has-text("Add Object in")`, { timeout: 5000 });
    await page.waitForSelector('#element-name');
    await page.fill('#element-name', name);
    await page.click('[role="dialog"] button:has-text("Add")');
    await page.waitForSelector(`text=${name}`, { timeout: 10000 });
}

async function selectTreeNode(name) {
    await page.locator(`[data-value="${name}"][data-part="item"], [data-value="${name}"][data-part="branch-control"]`).first().click();
}

async function run() {
    await page.goto(`${baseUrl}/open`);
    await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
    await page.fill('input[placeholder="Game name"]', `Verbs Editor Test ${Date.now()}`);
    await page.waitForSelector('text=Text adventure', { timeout: 10000 });
    await page.click('button:has-text("Create local draft")');
    await page.waitForSelector('button[title="Manage assets"]', { timeout: 30000 });
    console.log('PASS: local draft created and opened in the editor');

    await addRoom('Room One');
    await addObjectIn('Room One', 'Statue');
    console.log('PASS: created Room One and a Statue object inside it');

    await selectTreeNode('Statue');
    await page.waitForSelector('button:has-text("Verbs")', { timeout: 10000 });
    await page.click('button:has-text("Verbs")');
    await page.waitForSelector('text=No verbs added yet', { timeout: 10000 });
    console.log('PASS: Verbs tab renders (previously rendered nothing) and starts empty');

    // Typing a pattern that clashes with a built-in command ("take") must surface the
    // game-specific clash message wired through from CoreEditorObjectVerbs.aslx's
    // <clashmessages>, not just a generic error.
    const comboInput = page.locator('[role="combobox"]');
    await comboInput.click();
    await comboInput.fill('take');
    await page.keyboard.press('Tab'); // blur commits the Combobox's free-typed value
    await page.click('button:has-text("Add Verb")');
    await page.waitForSelector('text=Use the Inventory tab', { timeout: 10000 });
    console.log('PASS: adding a verb that clashes with a built-in command shows the specific clash message');

    // Add a genuine custom verb.
    await comboInput.click();
    await comboInput.fill('smell');
    await page.keyboard.press('Tab');
    await page.click('button:has-text("Add Verb")');
    await page.waitForSelector('table >> text=smell', { timeout: 10000 });
    console.log('PASS: adding a new custom verb ("smell") succeeds');

    // New verb defaults to "Print a message" and is auto-selected in the behaviour panel.
    await page.waitForSelector('text=Print a message', { timeout: 5000 });
    const typeSelect = page.locator('select').first();
    if (await typeSelect.inputValue() !== 'string') throw new Error('Expected new verb to default to the "string" (Print a message) type');
    console.log('PASS: new verb defaults to "Print a message" behaviour, auto-selected for editing');

    // Regression check: typing real keystrokes into the message box must actually accumulate —
    // a reactive effect previously re-synced this buffer from the committed value on every
    // keystroke (not just on selection changes), which silently discarded everything typed.
    const messageBox = page.locator('textarea');
    await messageBox.click();
    await page.keyboard.type('You give it a sniff. Nothing.', { delay: 20 });
    const typedValue = await messageBox.inputValue();
    if (typedValue !== 'You give it a sniff. Nothing.') throw new Error(`Expected typed keystrokes to accumulate in the message box, got: ${JSON.stringify(typedValue)}`);
    console.log('PASS: message text field accepts real keystrokes without resetting itself');

    // And it must actually commit (on blur) — reselecting the verb should show the saved text.
    await page.click('text=Behaviour');
    const smellListRow = page.locator('tr', { hasText: 'smell' });
    await smellListRow.click();
    await messageBox.waitFor({ timeout: 5000 });
    const persistedValue = await messageBox.inputValue();
    if (persistedValue !== 'You give it a sniff. Nothing.') throw new Error(`Expected the message to persist after reselecting the verb, got: ${JSON.stringify(persistedValue)}`);
    console.log('PASS: message commits on blur and persists across reselection');

    // Switch behaviour to "Run a script".
    await typeSelect.selectOption('script');
    await page.waitForSelector('button:has-text("Add script")', { timeout: 10000 });
    console.log('PASS: switching behaviour to "Run a script" shows the script editor');

    // Switch behaviour to "Require another object" — the entry-key field must be an object
    // picker (not free text), since the dictionary key has to be a real object name.
    await typeSelect.selectOption('scriptdictionary');
    const entryKeySelect = page.getByLabel('Add entry key');
    await entryKeySelect.waitFor({ timeout: 10000 });
    if ((await entryKeySelect.evaluate(el => el.tagName)) !== 'SELECT') throw new Error('Expected "Require another object" to offer an object-picker dropdown, not a free-text field');
    console.log('PASS: switching behaviour to "Require another object" shows an object-picker dropdown');

    const objectOptions = await entryKeySelect.locator('option').allTextContents();
    if (!objectOptions.includes('Room One')) throw new Error(`Expected the object picker to list "Room One", got: ${JSON.stringify(objectOptions)}`);
    if (objectOptions.includes('Statue')) throw new Error('Expected the object picker to exclude the object itself ("Statue")');
    console.log('PASS: object picker lists other objects and excludes the object itself');

    // Scoped to the entry-key select's own row — a bare "Add" text match would also hit the
    // toolbar's "+ Add" dropdown trigger and the "Add Verb" button.
    const addEntryButton = entryKeySelect.locator('..').getByRole('button', { name: 'Add', exact: true });
    if (!(await addEntryButton.isDisabled())) throw new Error('Expected "Add" to be disabled before an object is chosen');
    await entryKeySelect.selectOption('Room One');
    if (await addEntryButton.isDisabled()) throw new Error('Expected "Add" to enable once an object is chosen');
    await addEntryButton.click();
    // Scoped to the dictionary editor's own container (two levels up from the entry-key select)
    // — a bare "Room One" role match also hits its tree node, which has the same accessible name.
    const dictEditor = entryKeySelect.locator('..').locator('..');
    await dictEditor.getByRole('button', { name: 'Room One' }).waitFor({ timeout: 10000 });
    console.log('PASS: can add an object-keyed entry under "Require another object"');

    const objectOptionsAfterAdd = await entryKeySelect.locator('option').allTextContents();
    if (objectOptionsAfterAdd.includes('Room One')) throw new Error('Expected the object picker to exclude "Room One" now that it has been added');
    console.log('PASS: object picker excludes objects already added as entries');

    // Delete the verb.
    const smellRow = page.locator('tr', { hasText: 'smell' });
    await smellRow.locator('button[title="Remove verb"]').click();
    await page.waitForSelector('text=No verbs added yet', { timeout: 10000 });
    console.log('PASS: deleting a verb removes it from the list');

    // Undo restores it.
    await page.click('button[title="Undo"]');
    await page.waitForSelector('table >> text=smell', { timeout: 10000 });
    console.log('PASS: Undo restores the deleted verb');

    // "smell" above matched an existing library verb, so it never exercised AddVerb's other
    // branch: a genuinely novel pattern (no existing match) creates a brand-new verb command
    // element (visible under the game's Commands tree node) as well as setting the attribute.
    await comboInput.click();
    await comboInput.fill('juggle');
    await page.keyboard.press('Tab');
    await page.click('button:has-text("Add Verb")');
    await page.waitForSelector('table >> text=juggle', { timeout: 10000 });
    console.log('PASS: adding a genuinely novel verb pattern ("juggle") succeeds');

    // The new verb command element lands under the game's synthetic "Verbs" tree node (key
    // "_gameVerbs" — see EditorController's k_verbs), alongside any library-defined verbs.
    await selectTreeNode('_gameVerbs');
    await page.waitForSelector('text=juggle', { timeout: 10000 });
    console.log('PASS: the novel verb also created a new command element under the Verbs tree node');

    console.log('PASS: all checks passed');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-verbs-editor-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
