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

const scriptsOnDisk = readdirSync(here).filter(f => f.startsWith('verify-') && f.endsWith('.mjs')).sort();
const referenced = [...new Set(
    [...readFileSync(workflowPath, 'utf8').matchAll(/\bnode\s+(verify-[A-Za-z0-9.-]+\.mjs)/g)]
        .map(m => m[1]),
)].sort();

const notWired = scriptsOnDisk.filter(s => !referenced.includes(s));
const dangling = referenced.filter(s => !scriptsOnDisk.includes(s));

if (notWired.length || dangling.length) {
    if (notWired.length) {
        console.error(`Not wired into e2e.yml (add a \`node <script>.mjs\` step to the right job):\n  ${notWired.join('\n  ')}`);
    }
    if (dangling.length) {
        console.error(`Referenced by e2e.yml but missing on disk (typo, or a script renamed without updating the workflow):\n  ${dangling.join('\n  ')}`);
    }
    process.exit(1);
}
console.log(`OK: all ${scriptsOnDisk.length} verify-*.mjs scripts are wired into .github/workflows/e2e.yml`);
