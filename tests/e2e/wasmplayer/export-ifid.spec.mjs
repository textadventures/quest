// The single-file HTML export stamps the game's <gameid> as a Babel Treaty ifiction:ifid <meta>
// (uppercase UUID, RDFa prefix on <head>), loads the player from jsDelivr through <base href>,
// and replaces the shell's Quest Viva link-preview tags with the game's own title and description.
//
// Drives `qv publish --format html` (PlayerExport in PlayerCore), which the Editor's "single file"
// export shares. The qv built in this checkout pins the CDN with --cdn-version, as a released one
// would pin to its own version.
//
// Usage: npx playwright test --project=wasmplayer export-ifid
// Requires `dotnet build src/Cli` and a WasmPlayer build (the page is taken from its index.html).
import { test } from '@playwright/test';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawnSync } from 'node:child_process';

test('export-ifid', async () => {
    const repoRoot = path.resolve(import.meta.dirname, '..', '..', '..');
    const qv = process.env.QV?.split(' ') ?? ['dotnet', path.join(repoRoot, 'src/Cli/bin/Debug/net11.0/qv.dll')];
    const cdnVersion = '6.0.0-beta.59';

    const IFID_LOWER = '448e73df-2d2f-47e7-a494-a46b40d4cfb3';
    const IFID_UPPER = '448E73DF-2D2F-47E7-A494-A46B40D4CFB3';

    const game = (name, extra) => `<asl version="580">
      <include ref="English.aslx" />
      <include ref="Core.aslx" />
      <game name="${name}">
    ${extra}
      </game>
      <object name="room"><object name="player" /></object>
    </asl>
    `;
    const withId = game('IFID Test', `    <gameid>${IFID_LOWER}</gameid>
        <description>A "tiny" test &amp; nothing
          more.</description>`);
    const noId = game('No Id', '');

    function publishHtml(gamePath) {
        const result = spawnSync(qv[0], [...qv.slice(1), 'publish', gamePath, '--format', 'html', '--cdn-version', cdnVersion],
            { encoding: 'utf8' });
        if (result.status !== 0) {
            throw new Error(`qv publish failed (${result.status}):\n${result.stderr || result.stdout}`);
        }

        return { html: fs.readFileSync(gamePath.replace(/\.aslx$/, '.html'), 'utf8'), output: result.stdout };
    }

    function assertPreviewMeta(html, label, { title, description }) {
        if (html.includes('Play and create text adventure games')) {
            throw new Error(`${label}: still carries the Quest Viva site description`);
        }
        if (html.includes('og:site_name')) throw new Error(`${label}: unexpected og:site_name`);
        if (html.includes('og:image')) throw new Error(`${label}: unexpected og:image`);
        if (!html.includes(`<meta property="og:title" content="${title}" />`)) {
            throw new Error(`${label}: expected og:title "${title}"`);
        }
        if (description) {
            for (const tag of [`name="description" content="${description}"`, `property="og:description" content="${description}"`]) {
                if (!html.includes(tag)) throw new Error(`${label}: missing <meta ${tag}>`);
            }
        } else if (html.includes('name="description"') || html.includes('og:description')) {
            throw new Error(`${label}: unexpected description meta for a game without one`);
        }
    }

    function assertIfidMeta(html, label, ifid) {
        const prefix = 'prefix="ifiction: http://babel.ifarchive.org/protocol/iFiction/"';
        if (!html.includes(prefix)) throw new Error(`${label}: missing RDFa prefix on <head>`);
        if (!html.includes(`<meta property="ifiction:ifid" content="${ifid}" />`)) {
            throw new Error(`${label}: expected ifiction:ifid content="${ifid}"`);
        }
    }

    let tmp = null;
    try {
        tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'qv-export-ifid-'));
        const withIdPath = path.join(tmp, 'game.aslx');
        fs.writeFileSync(withIdPath, withId);
        fs.mkdirSync(path.join(tmp, 'noid'));
        const noIdPath = path.join(tmp, 'noid', 'noid.aslx');
        fs.writeFileSync(noIdPath, noId);

        const { html } = publishHtml(withIdPath);
        assertIfidMeta(html, 'export', IFID_UPPER);
        if (html.includes(`content="${IFID_LOWER}"`)) throw new Error('export: IFID was left lowercase');
        console.log('PASS: the export includes the uppercase ifiction:ifid meta and RDFa prefix');
        assertPreviewMeta(html, 'export', { title: 'IFID Test', description: 'A &quot;tiny&quot; test &amp; nothing more.' });
        console.log('PASS: the export swaps the site preview tags for the game title and description');
        const cdnBase = `<base href="https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@${cdnVersion}/" />`;
        if (!html.includes(cdnBase)) throw new Error(`export: missing ${cdnBase}`);
        if (!/window\.QuestVivaEmbeddedGame = "[A-Za-z0-9+/=]{100,}";\n\s*window\.QuestVivaEmbeddedGameFilename = "game\.quest";/.test(html)) {
            throw new Error('export: the game package isn\'t embedded');
        }
        if (html.includes('quest-config.js')) throw new Error('export: still loads quest-config.js');
        console.log('PASS: the export embeds the package and loads the player from the pinned CDN version');

        // A game with no gameid gets one in the package, which is the IFID the page carries too; qv
        // says what it is, so the author can keep it.
        const noIdResult = publishHtml(noIdPath);
        const generated = /<gameid>([0-9a-f-]{36})<\/gameid>/.exec(noIdResult.output)?.[1];
        if (!generated) throw new Error(`noid: qv didn't report the gameid it gave the game:\n${noIdResult.output}`);
        assertIfidMeta(noIdResult.html, 'noid', generated.toUpperCase());
        assertPreviewMeta(noIdResult.html, 'noid', { title: 'No Id', description: null });
        console.log('PASS: a game without <gameid> gets one, reported by qv and carried by the page');

        console.log('PASS');
    } catch (err) {
        console.error('FAIL:', err.message);
        throw err;
    } finally {
        if (tmp) fs.rmSync(tmp, { recursive: true, force: true });
    }
});
