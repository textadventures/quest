// Manifest check: every tests/e2e/verify-*.mjs script must be referenced by a
// `node <script>.mjs` step in .github/workflows/e2e.yml, and every reference
// must point at a script that exists on disk. GitHub Actions `run:` steps are
// literal commands (no way to glob "all scripts matching this prefix" into a
// job, and different scripts need different jobs/servers anyway), so this check
// is the enforcement instead: run on PRs touching tests/e2e/** and on the
// nightly run, an unwired — or dangling — script turns CI red immediately.
//
// Run: node tests/e2e/check-workflow-manifest.mjs  (node builtins only, no deps)
import { readFileSync, readdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const workflowPath = join(here, '..', '..', '.github', 'workflows', 'e2e.yml');

const workflow = readFileSync(workflowPath, 'utf8');
const scriptsOnDisk = readdirSync(here).filter(f => f.startsWith('verify-') && f.endsWith('.mjs')).sort();
const referenced = [...new Set(
    [...workflow.matchAll(/\bnode\s+(verify-[A-Za-z0-9.-]+\.mjs)/g)]
        .map(m => m[1]),
)].sort();

const notWired = scriptsOnDisk.filter(s => !referenced.includes(s));
const dangling = referenced.filter(s => !scriptsOnDisk.includes(s));

// Each `- run: node verify-*.mjs` step must also carry its own
// `working-directory: tests/e2e`, or node resolves the script against the repo
// root and fails with MODULE_NOT_FOUND. Easy to break by inserting a new step
// between an existing step's `run:` line and its `working-directory:` line,
// which silently moves the key onto the new step (#2503 did exactly this).
const lines = workflow.split('\n');
const missingWorkingDir = [];
lines.forEach((line, i) => {
    const m = line.match(/^(\s*)- run: node (verify-[A-Za-z0-9.-]+\.mjs)/);
    if (!m) return;
    const stepIndent = m[1].length;
    let hasWorkingDir = false;
    for (let j = i + 1; j < lines.length; j++) {
        const next = lines[j];
        if (next.trim() === '') continue;
        if (next.search(/\S/) <= stepIndent) break;
        if (/^\s*working-directory:\s*tests\/e2e\s*$/.test(next)) hasWorkingDir = true;
    }
    if (!hasWorkingDir) missingWorkingDir.push(`${m[2]} (e2e.yml line ${i + 1})`);
});

if (notWired.length || dangling.length || missingWorkingDir.length) {
    if (notWired.length) {
        console.error(`Not wired into e2e.yml (add a \`node <script>.mjs\` step to the right job):\n  ${notWired.join('\n  ')}`);
    }
    if (dangling.length) {
        console.error(`Referenced by e2e.yml but missing on disk (typo, or a script renamed without updating the workflow):\n  ${dangling.join('\n  ')}`);
    }
    if (missingWorkingDir.length) {
        console.error(`e2e.yml step missing \`working-directory: tests/e2e\` (node would look for the script at the repo root):\n  ${missingWorkingDir.join('\n  ')}`);
    }
    process.exit(1);
}
console.log(`OK: all ${scriptsOnDisk.length} verify-*.mjs scripts are wired into .github/workflows/e2e.yml`);
