// Verifies that the legacy UI's focus style for the command box doesn't override a game's
// borderless or shadow command box. Published games carry Core.aslx inlined, and Core up to 6.0
// adds its own <style>#txtCommand {...}</style> for these options instead of calling
// QuestUI.setCommandBarPreset, so the check covers both routes.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
//
// Run: node verify-wasmplayer-command-box-focus.mjs [baseUrl]

import { chromium } from 'playwright';

const BASE = process.argv.slice(2).find(a => !a.startsWith('--')) || 'http://localhost:5175';
const GAME = '/e2e-fixtures/command-box-focus-test.aslx';

const browser = await chromium.launch();
const page = await browser.newPage();

async function load() {
    await page.goto(`${BASE}/?url=${encodeURIComponent(GAME)}`);
    await page.locator('#divOutput').getByText('You are in a room.').first().waitFor({ timeout: 60000 });
}

async function send(command, reply) {
    const input = page.locator('#txtCommand');
    await input.fill(command);
    await input.press('Enter');
    await page.locator('#divOutput').getByText(reply).first().waitFor({ timeout: 15000 });
}

async function focusedStyle() {
    return page.evaluate(() => {
        const input = document.querySelector('#txtCommand');
        input.focus();
        const s = getComputedStyle(input);
        return {
            focused: document.activeElement === input,
            borderStyle: s.borderTopStyle,
            borderColor: s.borderTopColor,
            outlineStyle: s.outlineStyle,
            outlineColor: s.outlineColor,
            boxShadow: s.boxShadow,
        };
    });
}

function expectBorderless(label, s) {
    if (!s.focused) throw new Error(`${label}: command box didn't take focus`);
    if (s.borderStyle !== 'none') throw new Error(`${label}: focused box has a border (${s.borderStyle} ${s.borderColor})`);
    if (s.outlineStyle !== 'none') throw new Error(`${label}: focused box has an outline (${s.outlineStyle})`);
    if (s.boxShadow !== 'none') throw new Error(`${label}: focused box has a shadow (${s.boxShadow})`);
    console.log(`PASS: ${label} keeps no border, outline or ring on focus`);
}

function expectShadow(label, s) {
    if (!s.focused) throw new Error(`${label}: command box didn't take focus`);
    if (s.borderStyle !== 'none') throw new Error(`${label}: focused box has a border (${s.borderStyle} ${s.borderColor})`);
    if (s.outlineStyle !== 'none') throw new Error(`${label}: focused box has an outline (${s.outlineStyle})`);
    if (!s.boxShadow.includes('15px 5px')) throw new Error(`${label}: focused box lost its shadow (${s.boxShadow})`);
    console.log(`PASS: ${label} keeps its shadow and no border on focus`);
}

try {
    await load();
    const plain = await focusedStyle();
    if (!plain.focused) throw new Error('default: command box didn\'t take focus');
    if (plain.outlineStyle !== 'solid' || plain.outlineColor !== 'rgb(74, 144, 217)') {
        throw new Error(`default: expected a blue focus outline, got ${plain.outlineStyle} ${plain.outlineColor}`);
    }
    if (plain.borderStyle !== 'solid') throw new Error(`default: focused box lost its border (${plain.borderStyle})`);
    const blurred = await page.evaluate(() => {
        document.querySelector('#txtCommand').blur();
        return getComputedStyle(document.querySelector('#txtCommand')).outlineStyle;
    });
    if (blurred !== 'none') throw new Error(`default: unfocused box has an outline (${blurred})`);
    console.log('PASS: default box shows its focus outline only when focused');

    await send('oldborderless', 'Old borderless style added.');
    expectBorderless('borderless from 6.0 Core', await focusedStyle());

    await load();
    await send('oldshadow', 'Old shadow style added.');
    expectShadow('shadow from 6.0 Core', await focusedStyle());

    await load();
    await send('presetborderless', 'Borderless preset set.');
    expectBorderless('borderless preset', await focusedStyle());

    await load();
    await send('presetshadow', 'Shadow preset set.');
    expectShadow('shadow preset', await focusedStyle());

    console.log('PASS: all checks passed');
} catch (e) {
    console.error('FAIL:', e.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
