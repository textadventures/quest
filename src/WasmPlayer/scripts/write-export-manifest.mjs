#!/usr/bin/env node
// Writes export-manifest.json into a WasmPlayer AppBundle: every non-_framework
// file the zip HTML export needs to pack. Generated at build time so AppShell's
// exportHtmlZip doesn't hard-code a shell file list that can drift from the
// layout CopyPlayerAssetsToAppBundle produces (plus SDK-added package.json /
// runtimeconfig). _framework/* is listed separately from dotnet.boot.js at
// export time — those filenames are content-hashed and already enumerated there.
import fs from 'node:fs';
import path from 'node:path';

const appBundle = process.argv[2];
if (!appBundle) {
    console.error('Usage: node write-export-manifest.mjs <AppBundle-dir>');
    process.exit(1);
}

const SKIP_DIRS = new Set(['_framework']);
const SKIP_FILES = new Set(['.stamp', 'export-manifest.json']);

function walk(dir, base) {
    const out = [];
    for (const ent of fs.readdirSync(dir, { withFileTypes: true })) {
        if (ent.isDirectory()) {
            if (SKIP_DIRS.has(ent.name)) continue;
            out.push(...walk(path.join(dir, ent.name), base));
            continue;
        }
        if (SKIP_FILES.has(ent.name)) continue;
        out.push(path.relative(base, path.join(dir, ent.name)).split(path.sep).join('/'));
    }
    return out;
}

const files = walk(appBundle, appBundle).sort();
fs.writeFileSync(path.join(appBundle, 'export-manifest.json'), JSON.stringify(files) + '\n');
console.log(`Wrote export-manifest.json (${files.length} shell files)`);
