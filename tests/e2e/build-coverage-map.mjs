// Aggregates the per-script "touched files" JSON dumped by lib/tracked-chromium.mjs
// (one file per verify-appshell-*.mjs script, written when QUEST_TOUCHED_FILES_DIR
// is set) into a single source-file -> tests lookup, coverage-map.json. That map is
// what find-affected-tests.mjs reads.
//
// Regenerate after a significant AppShell restructuring (new components, renamed
// files) so the map doesn't drift too far from reality. There's no automated
// regeneration yet - to refresh it, run the appshell_chromium job's script list
// locally against a dev server with QUEST_TOUCHED_FILES_DIR set, then:
//
//   node tests/e2e/build-coverage-map.mjs <touched-files-dir> [output-path]
import { readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const [, , touchedFilesDir, outputPath = join(import.meta.dirname, 'coverage-map.json')] = process.argv;

if (!touchedFilesDir) {
    console.error('Usage: node build-coverage-map.mjs <touched-files-dir> [output-path]');
    process.exit(1);
}

const map = {};
const files = readdirSync(touchedFilesDir).filter(f => f.endsWith('.json'));

for (const file of files) {
    const scriptName = file.replace(/\.json$/, '');
    const touchedFiles = JSON.parse(readFileSync(join(touchedFilesDir, file), 'utf8'));
    for (const sourceFile of touchedFiles) {
        (map[sourceFile] ??= []).push(scriptName);
    }
}

for (const sourceFile of Object.keys(map)) {
    map[sourceFile].sort();
}

const sortedMap = Object.fromEntries(Object.entries(map).sort(([a], [b]) => a.localeCompare(b)));

writeFileSync(outputPath, JSON.stringify(sortedMap, null, 2) + '\n');
console.log(`Wrote ${outputPath}: ${Object.keys(sortedMap).length} source files mapped from ${files.length} test scripts`);
