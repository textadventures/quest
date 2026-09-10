// Ad-hoc verification: single-file HTML export stamps the game's <gameid> as a
// Babel Treaty ifiction:ifid <meta> (uppercase UUID + RDFa prefix on <head>).
// Drives the CLI exporter (src/WasmPlayer/scripts/export-embedded.mjs) — the
// AppShell "Export as single file…" path applies the same head transforms.
//
// Requires generated/index.html: (cd src/WasmPlayer && npm run build)
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import { zipSync } from 'fflate';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '..', '..');
const exporter = path.join(repoRoot, 'src/WasmPlayer/scripts/export-embedded.mjs');
const template = path.join(repoRoot, 'src/WasmPlayer/generated/index.html');

const IFID_LOWER = '448e73df-2d2f-47e7-a494-a46b40d4cfb3';
const IFID_UPPER = '448E73DF-2D2F-47E7-A494-A46B40D4CFB3';

const aslxWithId = `<asl version="580">
  <game name="IFID Test">
    <gameid>${IFID_LOWER}</gameid>
  </game>
  <object name="room"><object name="player" /></object>
</asl>
`;
const aslxNoId = `<asl version="580">
  <game name="No Id"></game>
  <object name="room"><object name="player" /></object>
</asl>
`;

function exportHtml(gamePath, outPath) {
    const result = spawnSync(process.execPath, [exporter, gamePath, '6.0.0-beta.59', outPath], {
        encoding: 'utf8',
    });
    if (result.status !== 0) {
        throw new Error(`export-embedded failed:\n${result.stderr || result.stdout}`);
    }
}

function assertIfidMeta(html, label) {
    const prefix = 'prefix="ifiction: http://babel.ifarchive.org/protocol/iFiction/"';
    if (!html.includes(prefix)) throw new Error(`${label}: missing RDFa prefix on <head>`);
    if (!html.includes(`property="ifiction:ifid"`)) throw new Error(`${label}: missing ifiction:ifid property`);
    if (!html.includes(`content="${IFID_UPPER}"`)) {
        throw new Error(`${label}: expected uppercase IFID content="${IFID_UPPER}"`);
    }
    if (html.includes(`content="${IFID_LOWER}"`)) {
        throw new Error(`${label}: IFID was left lowercase`);
    }
}

try {
    if (!fs.existsSync(template)) {
        throw new Error(`Missing ${template} — run "npm run build" in src/WasmPlayer first.`);
    }

    const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'qv-export-ifid-'));
    const aslxPath = path.join(tmp, 'game.aslx');
    const questPath = path.join(tmp, 'game.quest');
    const noIdPath = path.join(tmp, 'noid.aslx');
    fs.writeFileSync(aslxPath, aslxWithId);
    fs.writeFileSync(noIdPath, aslxNoId);
    // Packager writes a deflated game.aslx entry — exercise the same shape.
    const questBytes = zipSync({ 'game.aslx': new TextEncoder().encode(aslxWithId) });
    fs.writeFileSync(questPath, questBytes);

    const fromAslx = path.join(tmp, 'from-aslx.html');
    exportHtml(aslxPath, fromAslx);
    assertIfidMeta(fs.readFileSync(fromAslx, 'utf8'), 'aslx export');
    console.log('PASS: .aslx export includes uppercase ifiction:ifid meta + RDFa prefix');

    const fromQuest = path.join(tmp, 'from-quest.html');
    exportHtml(questPath, fromQuest);
    assertIfidMeta(fs.readFileSync(fromQuest, 'utf8'), 'quest export');
    console.log('PASS: .quest export reads gameid from game.aslx inside the zip');

    const fromNoId = path.join(tmp, 'noid.html');
    exportHtml(noIdPath, fromNoId);
    const noIdHtml = fs.readFileSync(fromNoId, 'utf8');
    if (noIdHtml.includes('ifiction:ifid')) throw new Error('noid: unexpected ifiction:ifid meta');
    if (!noIdHtml.includes('<base href=')) throw new Error('noid: missing <base href>');
    console.log('PASS: game without <gameid> omits the meta but still gets <base href>');

    console.log('PASS');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
}
