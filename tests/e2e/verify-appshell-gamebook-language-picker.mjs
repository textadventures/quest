// Gamebooks previously had exactly one creation template (English captions,
// baked-in EditorEnglish.aslx include, no <languageid>) — Text Adventures
// already had a per-language template picker (English.template/Deutsch.template/...,
// each pairing an EditorXxx.aslx caption set with a <languageid> value). This
// adds the same picker for Gamebooks (Gamebook.template / Gamebook-Deutsch.template)
// and exposes the resulting game.languageid as an editable Properties field
// (dropdown + freetext, so any language code can be entered even without a
// dedicated Editor*.aslx caption set).
//
// GamebookCore.aslx must keep including EditorEnglish.aslx itself (as a
// baseline, same as before this change) — it's loaded live while editing, not
// frozen, so any pre-existing gamebook file (which only has
// <include ref="GamebookCore.aslx"/>, never its own top-level Editor*.aslx
// include) would otherwise lose all its editor captions and its languageid
// default the moment it's reopened. See the "pre-existing gamebook" section
// below, which regression-tests exactly that scenario.
//
// Run against a dev server started with:
//   npm --prefix src/AppShell run dev -- --port 5174
import { chromium } from './lib/tracked-chromium.mjs';
import { writeFileSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

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
    // --- German UI locale: Gamebook picker should default to "Deutsch" ---
    await page.goto(`${baseUrl}/open`);
    await page.evaluate(() => localStorage.setItem('questviva-ui-language', 'de'));
    await page.reload();
    await page.waitForSelector('input[type="text"]', { timeout: 30000 });
    await page.fill('input[type="text"]', 'DeutschGamebookTest');
    await page.waitForSelector('input[name="gametype"]', { timeout: 10000 });
    await page.click('input[name="gametype"] >> nth=1');
    await page.waitForSelector('select', { timeout: 10000 });
    const options = await page.$$eval('select option', els => els.map(el => el.textContent ?? ''));
    assertEqual('Gamebook template option count', options.length, 3);
    if (!options.includes('English') || !options.includes('Deutsch') || !options.includes('Español')) {
        throw new Error(`Gamebook template options missing expected labels: ${JSON.stringify(options)}`);
    }
    console.log(`PASS: [Gamebook template options] ${JSON.stringify(options)}`);
    const selectedLabel = await page.$eval('select', el => el.selectedOptions[0]?.textContent ?? '');
    assertEqual('Gamebook template default (de locale)', selectedLabel, 'Deutsch');

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

    // --- English UI locale + explicit "English" Gamebook template ---
    await createGame('EnglishGamebookTest', { locale: 'en', templateLabel: 'English' });
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

    // --- Pre-existing gamebook (created before this change): only
    // <include ref="GamebookCore.aslx"/>, no top-level Editor*.aslx include,
    // no <template name="LanguageId">. Opening it must not show unresolved
    // "[EditorXxx]"/"[LanguageId]" bracket placeholders. ---
    const oldGamebookAslx = `﻿<asl version="600" templatetype="gamebook">

  <include ref="GamebookCore.aslx"/>

  <game name="OldGamebookTest">
    <gameid>12345678-1234-1234-1234-123456789012</gameid>
    <version>1.0</version>
    <firstpublished>2024</firstpublished>
  </game>

  <object name="Page1">
    <object name="player">
      <inherit name="defaultplayer"/>
    </object>
    <description>This is page 1. Type a description here, and then create links to other pages below.</description>
    <options type="simplestringdictionary">Page2 = This link goes to page 2;Page3 = And this link goes to page 3</options>
  </object>

  <object name="Page2">
    <description>This is page 2. Type a description here, and then create links to other pages below.</description>
  </object>

  <object name="Page3">
    <description>This is page 3. Type a description here, and then create links to other pages below.</description>
  </object>

</asl>`;
    const tmpDir = mkdtempSync(join(tmpdir(), 'quest-e2e-'));
    const oldGamebookPath = join(tmpDir, 'OldGamebookTest.aslx');
    writeFileSync(oldGamebookPath, oldGamebookAslx, 'utf8');

    await page.goto(`${baseUrl}/open`);
    await page.evaluate(() => localStorage.setItem('questviva-ui-language', 'en'));
    await page.reload();
    await page.waitForSelector('button.preset-filled-primary-500', { timeout: 30000 });
    const [fileChooser] = await Promise.all([
        page.waitForEvent('filechooser'),
        page.click('button.preset-filled-primary-500'), // "Import game file" — first such button on a fresh /open
    ]);
    await fileChooser.setFiles(oldGamebookPath);
    await page.waitForSelector('header .toolbar-icon-btn', { timeout: 30000 });

    const bodyText = await page.textContent('body');
    const unresolvedBrackets = bodyText.match(/\[(Editor|LanguageId)\w*\]/g);
    if (unresolvedBrackets) {
        throw new Error(`Pre-existing gamebook: unresolved template placeholder(s) found: ${JSON.stringify(unresolvedBrackets)}`);
    }
    console.log('PASS: [Pre-existing gamebook] no unresolved [EditorXxx]/[LanguageId] placeholders');

    const oldLangValue = await page.evaluate(() => {
        const inputs = Array.from(document.querySelectorAll('input[type="text"]'));
        const match = inputs.find(i => i.value === 'en');
        return match ? match.value : null;
    });
    assertEqual('Pre-existing gamebook languageid default', oldLangValue, 'en');

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
