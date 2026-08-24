// Given the files changed on the current branch, prints which appshell_chromium
// e2e scripts (tests/e2e/verify-appshell-*.mjs and friends) actually loaded each
// changed AppShell source file during a past coverage-collection run — i.e. the
// tests worth running locally against the dev server before opening a PR, without
// running the whole ~9-minute suite.
//
// Advisory only, not a gate: coverage-map.json is a snapshot (see
// build-coverage-map.mjs's header), and this only covers the browser-visible
// AppShell layer (src/AppShell/src/**) — a changed file under Engine/EditorCore/
// WasmEditor (compiled into the opaque WASM blob) won't appear in the map at all,
// since it's invisible to the Vite-request tracking that built it; those changes
// still need a judgment call or a manual run of the relevant e2e job.
//
// Run: node tests/e2e/find-affected-tests.mjs [base-ref]   (default: origin/main)
import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

const baseRef = process.argv[2] || 'origin/main';
const repoRoot = execFileSync('git', ['rev-parse', '--show-toplevel'], { encoding: 'utf8' }).trim();
const coverageMapPath = join(import.meta.dirname, 'coverage-map.json');

const changedFiles = execFileSync('git', ['diff', '--name-only', `${baseRef}...HEAD`], { cwd: repoRoot, encoding: 'utf8' })
    .split('\n')
    .map(f => f.trim())
    .filter(Boolean);

if (changedFiles.length === 0) {
    console.log(`No changes found against ${baseRef}.`);
    process.exit(0);
}

const coverageMap = JSON.parse(readFileSync(coverageMapPath, 'utf8'));

const appShellChanges = changedFiles.filter(f => f.startsWith('src/AppShell/src/'));
const otherChanges = changedFiles.filter(f => !f.startsWith('src/AppShell/src/'));

const affectedTests = new Map(); // script name -> Set of files that flagged it
for (const file of appShellChanges) {
    const tests = coverageMap[file];
    if (!tests) continue;
    for (const test of tests) {
        if (!affectedTests.has(test)) affectedTests.set(test, new Set());
        affectedTests.get(test).add(file);
    }
}

const unmapped = appShellChanges.filter(f => !coverageMap[f]);

console.log(`Changed files vs ${baseRef}: ${changedFiles.length}`);

if (affectedTests.size > 0) {
    console.log(`\nLikely-affected e2e scripts (run these against a local AppShell dev server before opening the PR):`);
    for (const [test, files] of [...affectedTests].sort(([a], [b]) => a.localeCompare(b))) {
        console.log(`  node tests/e2e/${test} http://localhost:5174`);
        for (const file of files) console.log(`      because of: ${file}`);
    }
} else if (appShellChanges.length > 0) {
    console.log('\nNo AppShell e2e script is recorded as touching any changed file — either coverage-map.json is stale, or these files (e.g. a brand-new component) genuinely aren\'t exercised by an existing script yet.');
}

if (unmapped.length > 0) {
    console.log(`\nChanged AppShell files with no coverage-map entry (new files, or the map needs regenerating):\n  ${unmapped.join('\n  ')}`);
}

if (otherChanges.length > 0) {
    console.log(`\n${otherChanges.length} changed file(s) outside src/AppShell/src/ (Engine/EditorCore/WasmEditor/WasmPlayer/WebPlayer/etc.) aren't covered by this map at all — it only sees the browser-visible AppShell layer. Use judgment or run the relevant e2e job's scripts manually:`);
    console.log(`  ${otherChanges.join('\n  ')}`);
}
