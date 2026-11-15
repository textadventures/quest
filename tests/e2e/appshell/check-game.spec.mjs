// Issue #2706: the Editor's Check game shows what `qv check` finds, in a Problems panel under the
// tree and properties, and Publish checks the game first. This creates a draft, checks it (no
// problems), then adds broken scripts and a duplicate name through Raw XML code view. Applying it
// re-checks the game; clicking each problem selects its element and shows the attribute: the
// function's script control, highlighted, and the object's verb, selected on the Verbs tab.
// Publish then stops to say how many problems there are, and Show problems opens the panel.
// Issue #2859: in German, each problem's message is translated too, including the error inside a
// script that couldn't be added.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('check-game', async ({ baseURL }) => {
    const baseUrl = baseURL;
    const browser = await chromium.launch();

    // CodeMirror doesn't take fill(), and typing a long document is unreliable: paste instead,
    // as code-view.spec.mjs does.
    const selectAllKey = process.platform === 'darwin' ? 'Meta+A' : 'Control+A';
    const pasteKey = process.platform === 'darwin' ? 'Meta+V' : 'Control+V';

    try {
        const ctx = await browser.newContext({
            viewport: { width: 1280, height: 800 },
            permissions: ['clipboard-read', 'clipboard-write'],
        });
        const page = await ctx.newPage();
        page.on('pageerror', err => console.log('[pageerror]', err.message));

        await page.goto(`${baseUrl}/open`);
        await page.waitForSelector('button:has-text("Create local draft")', { timeout: 30000 });
        const gameName = `Check game ${Date.now()}`;
        await page.fill('input[placeholder="Game name"]', gameName);
        await page.waitForSelector('text=Text adventure', { timeout: 10000 });
        await page.click('button:has-text("Create local draft")');
        await page.waitForSelector('button[title="Preview game"]', { timeout: 60000 });

        async function fileMenu(item) {
            await page.click('button:has-text("File")');
            await page.locator('.absolute button', { hasText: item }).click();
        }

        const panel = page.locator('.check-panel');
        const status = panel.locator('.check-panel-status');

        // --- A new game has no problems ---
        await fileMenu('Check game');
        await status.getByText('No problems found.').waitFor({ timeout: 30000 });
        console.log('PASS: Check game on a new game finds no problems');

        // Adds lines to the end of the game through Raw XML code view, and applies them. The labels
        // are the code view button's title and the Apply buttons', in the Editor's language.
        async function addXml(lines, { codeView = 'Raw XML code view', apply = 'Apply' } = {}) {
            await page.click(`button[title="${codeView}"]`);
            await page.waitForSelector('.cm-editor', { timeout: 5000 });
            if (await panel.count() !== 0) throw new Error('Problems panel should be hidden while Code View is open');
            const xml = await page.locator('.cm-content').first().innerText();
            const changed = xml.replace('</asl>', [...lines, '</asl>'].join('\n'));
            if (changed === xml) throw new Error('Could not find </asl> in the code view');
            await page.locator('.cm-editor .cm-content').first().click();
            await page.keyboard.press(selectAllKey);
            await page.evaluate(t => navigator.clipboard.writeText(t), changed);
            await page.keyboard.press(pasteKey);
            await page.click(`button:has-text("${apply}")`);
            await page.click(`[role="dialog"] button:has-text("${apply}")`);
            await page.waitForSelector('.cm-editor', { state: 'detached', timeout: 15000 });
        }

        // --- Break it through Raw XML code view ---
        await addXml([
            '  <function name="Broken">NoSuchFunction (1)</function>',
            '  <function name="BadFor">for (i, 1) { msg ("too few") }</function>',
            '  <function name="Twice">msg ("first")</function>',
            '  <function name="Twice">msg ("second")</function>',
            '  <object name="lamp">',
            '    <eat type="script">AlsoMissing</eat>',
            '  </object>',
        ]);

        // Applying reloads the game, and the open panel checks it again.
        await status.getByText(/^4 problems/).waitFor({ timeout: 30000 });
        const messages = await panel.locator('.check-problem').allInnerTexts();
        const expect = [
            ['Broken', 'NoSuchFunction'],
            ['lamp', 'AlsoMissing'],
            ['Twice', 'more than one element'],
            ['BadFor', "'for' script should have 3 or 4 parameters"],
        ];
        for (const [element, text] of expect) {
            if (!messages.some(m => m.startsWith(element) && m.includes(text))) {
                throw new Error(`Expected a problem for ${element} mentioning "${text}", got ${JSON.stringify(messages)}`);
            }
        }
        console.log('PASS: applying the broken XML re-checks the game and lists its 4 problems');

        // --- A function's script: selected, its control highlighted ---
        await panel.locator('.check-problem', { hasText: 'NoSuchFunction' }).click();
        await page.locator('.control-highlight').first().waitFor({ timeout: 5000 });
        const nameValue = await page.locator('label').filter({ hasText: /^Name:/ })
            .locator('xpath=(.//input | following::input)[1]').inputValue();
        if (nameValue !== 'Broken') throw new Error(`Expected the Broken function to be shown, got "${nameValue}"`);
        console.log('PASS: clicking a function\'s problem selects it and highlights its script');

        // --- A verb: the Verbs tab, with the verb selected ---
        await panel.locator('.check-problem', { hasText: 'AlsoMissing' }).click();
        const verbRow = page.locator('tr.bg-primary-100-900', { hasText: 'eat' });
        await verbRow.waitFor({ timeout: 5000 });
        console.log('PASS: clicking a verb\'s problem shows the Verbs tab with that verb selected');

        await panel.locator('.check-panel-close').click();
        await panel.waitFor({ state: 'detached', timeout: 5000 });

        // --- Publish checks first ---
        await fileMenu('Publish');
        const dialog = page.locator('div[role="dialog"]');
        await dialog.locator('button', { hasText: 'Publish…' }).click();
        await dialog.locator('.publish-problems-warning', { hasText: 'This game has 4 problems' }).waitFor({ timeout: 30000 });
        if (await dialog.locator('.publish-anyway').count() !== 1) throw new Error('Publish anyway should be offered');
        await dialog.locator('.publish-show-problems').click();
        await dialog.waitFor({ state: 'detached', timeout: 5000 });
        await status.getByText(/^4 problems/).waitFor({ timeout: 5000 });
        console.log('PASS: Publish stops at the problems, and Show problems opens the panel');

        // --- In German, the problems are translated ---
        await page.evaluate(() => localStorage.setItem('questviva-ui-language', 'de'));
        // Reloading goes back to Create, which lists the draft to open again. The draft was saved
        // with only the last Twice, so add another for the duplicate name.
        await page.reload();
        await page.locator('button', { hasText: gameName }).first().click();
        const fileButton = page.getByRole('button', { name: 'Datei', exact: true });
        await fileButton.waitFor({ timeout: 60000 });
        await fileButton.click();
        await page.locator('.absolute button', { hasText: 'Spiel prüfen' }).click();
        await status.getByText(/^3 Probleme/).waitFor({ timeout: 30000 });
        await addXml(['  <function name="Twice">msg ("third")</function>'],
            { codeView: 'Ansicht des XML-Quellcodes', apply: 'Übernehmen' });
        await status.getByText(/^4 Probleme/).waitFor({ timeout: 30000 });
        const german = await panel.locator('.check-problem').allInnerTexts();
        const expectGerman = [
            ['Broken', 'Unbekannter Skriptbefehl oder unbekannte Funktion „NoSuchFunction“'],
            ['lamp', 'Unbekannter Skriptbefehl oder unbekannte Funktion „AlsoMissing“'],
            ['Twice', 'Es gibt mehr als ein Element namens „Twice“. Nur das letzte wird verwendet: Dieses Element (Funktion) wird durch das später definierte Element (Funktion) ersetzt.'],
            ['BadFor', 'Fehler beim Hinzufügen des Skripts „for (i, 1)'],
            ['BadFor', 'Das Skript „for“ sollte 3 oder 4 Parameter haben: „for (i, 1)“'],
        ];
        for (const [element, text] of expectGerman) {
            if (!german.some(m => m.startsWith(element) && m.includes(text))) {
                throw new Error(`Expected a German problem for ${element} saying "${text}", got ${JSON.stringify(german)}`);
            }
        }
        console.log('PASS: in German, each problem\'s message is translated');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        await browser.close();
    }
});
