// Gamebooks previously had exactly one creation template (English captions,
// baked-in EditorEnglish.aslx include, no <languageid>) — Text Adventures
// already had a per-language template picker (English.template/Deutsch.template/...,
// each pairing an EditorXxx.aslx caption set with a <languageid> value). This
// adds the same picker for Gamebooks (Gamebook.template / Gamebook-Deutsch.template)
// and exposes the resulting game.languageid as an editable Properties field
// (dropdown + freetext, so any language code can be entered even without a
// dedicated Editor*.aslx caption set).
//
// Run against a dev server started with:
//   npm --prefix src/AppShell run dev -- --port 5174
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
page.on('pageerror', err => console.log('[pageerror]', err.message));

function assertEqual(label, actual, expected) {
    if (actual !== expected) throw new Error(`[${label}] expected "${expected}", got "${actual}"`);
    console.log(`PASS: [${label}] "${actual}"`);
}

async function createGame(name, { locale, templateLabel }) {
    await page.goto(`${baseUrl}/open`);
    await page.evaluate(loc => localStorage.setItem('questviva-ui-language', loc), locale);
    await page.reload();

    await page.waitForSelector('input[type="text"]', { timeout: 30000 });
    await page.fill('input[type="text"]', name);
    await page.waitForSelector('input[name="gametype"]', { timeout: 10000 });
    // Second gametype radio is Gamebook (Textadventure, Gamebook — see open/+page.svelte).
    await page.click('input[name="gametype"] >> nth=1');

    await page.waitForSelector('select', { timeout: 10000 });
    if (templateLabel) await page.selectOption('select', { label: templateLabel });

    // "Create local draft" is the last filled-primary button on this branch of /open.
    await page.locator('button.preset-filled-primary-500').last().click();
    await page.waitForSelector('header .toolbar-icon-btn', { timeout: 30000 });
}

async function run() {
    // --- German UI locale: Gamebook picker should default to "Gamebook (Deutsch)" ---
    await page.goto(`${baseUrl}/open`);
    await page.evaluate(() => localStorage.setItem('questviva-ui-language', 'de'));
    await page.reload();
    await page.waitForSelector('input[type="text"]', { timeout: 30000 });
    await page.fill('input[type="text"]', 'DeutschGamebookTest');
    await page.waitForSelector('input[name="gametype"]', { timeout: 10000 });
    await page.click('input[name="gametype"] >> nth=1');
    await page.waitForSelector('select', { timeout: 10000 });
    const options = await page.$$eval('select option', els => els.map(el => el.textContent ?? ''));
    assertEqual('Gamebook template option count', options.length, 2);
    if (!options.includes('Gamebook') || !options.includes('Gamebook (Deutsch)')) {
        throw new Error(`Gamebook template options missing expected labels: ${JSON.stringify(options)}`);
    }
    console.log(`PASS: [Gamebook template options] ${JSON.stringify(options)}`);
    const selectedLabel = await page.$eval('select', el => el.selectedOptions[0]?.textContent ?? '');
    assertEqual('Gamebook template default (de locale)', selectedLabel, 'Gamebook (Deutsch)');

    await page.locator('button.preset-filled-primary-500').last().click();
    await page.waitForSelector('header .toolbar-icon-btn', { timeout: 30000 });

    // "Sprache:" (Language) field should default to "de" via the new
    // <languageid>[LanguageId]</languageid> line in GamebookCore.aslx's defaultgame.
    const langLabels = await page.$$eval('label, span, p', els =>
        els.map(el => el.textContent?.trim() ?? '').filter(t => t === 'Sprache:'));
    if (langLabels.length === 0) throw new Error('German gamebook: "Sprache:" label not found — EditorGBLanguage caption missing?');
    console.log('PASS: [German gamebook] "Sprache:" label present');

    // Locate the language combobox by its current value "de" among the Setup tab's text inputs.
    const langValue = await page.evaluate(() => {
        const inputs = Array.from(document.querySelectorAll('input[type="text"]'));
        const match = inputs.find(i => i.value === 'de');
        return match ? match.value : null;
    });
    assertEqual('German gamebook languageid default', langValue, 'de');

    // Freetext path: type an arbitrary code not in the LanguageCodes list.
    await page.evaluate(() => {
        const inputs = Array.from(document.querySelectorAll('input[type="text"]'));
        const match = inputs.find(i => i.value === 'de');
        match.focus();
    });
    await page.keyboard.press('Control+A');
    await page.keyboard.type('cy');
    await page.keyboard.press('Tab');
    const freetextValue = await page.evaluate(() => {
        const inputs = Array.from(document.querySelectorAll('input[type="text"]'));
        const match = inputs.find(i => i.value === 'cy');
        return match ? match.value : null;
    });
    assertEqual('German gamebook languageid freetext ("Other" language)', freetextValue, 'cy');

    // Default Page2 text should be German (Gamebook-Deutsch.template).
    await page.click('span:text-is("Page2")');
    await page.waitForSelector('#richtext-description', { timeout: 10000 });
    const page2Text = await page.inputValue('#richtext-description');
    if (!page2Text.includes('Seite 2')) {
        throw new Error(`German gamebook Page2 description not translated: "${page2Text}"`);
    }
    console.log(`PASS: [German gamebook Page2 description] "${page2Text.trim()}"`);

    // --- English UI locale + explicit "Gamebook" (English) template ---
    await createGame('EnglishGamebookTest', { locale: 'en', templateLabel: 'Gamebook' });
    const enLangValue = await page.evaluate(() => {
        const inputs = Array.from(document.querySelectorAll('input[type="text"]'));
        const match = inputs.find(i => i.value === 'en');
        return match ? match.value : null;
    });
    assertEqual('English gamebook languageid default', enLangValue, 'en');
    const enLangLabels = await page.$$eval('label, span, p', els =>
        els.map(el => el.textContent?.trim() ?? '').filter(t => t === 'Language:'));
    if (enLangLabels.length === 0) throw new Error('English gamebook: "Language:" label not found');
    console.log('PASS: [English gamebook] "Language:" label present');

    console.log('PASS');
}

try {
    await run();
} catch (err) {
    console.error('FAIL:', err.message);
    await page.screenshot({ path: '/tmp/appshell-gamebook-language-picker-failure.png' });
    process.exitCode = 1;
} finally {
    await browser.close();
}
