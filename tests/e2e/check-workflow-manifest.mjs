// Manifest check: makes sure every e2e spec actually runs in the nightly e2e workflow.
// Playwright Test only runs a spec that sits in one of playwright.config.mjs's project
// directories, and CI only runs a project that .github/workflows/e2e.yml has a
// `npx playwright test --project=<name>` step for. A spec anywhere else, or a project
// with no step, would pass every check by never running at all. So this fails if:
// - a *.spec.mjs file isn't in a project's testDir;
// - a project in the config has no `--project=<name>` step in e2e.yml (or a step names
//   a project the config doesn't have);
// - a standalone verify-*.mjs script is left in tests/e2e (the old format, which
//   nothing runs any more).
// Runs on every PR as the required check_e2e_manifest job, with no npm install, so it
// reads the config as text rather than importing it.
//
// Run: node tests/e2e/check-workflow-manifest.mjs  (node builtins only, no deps)
import { readFileSync, readdirSync } from 'node:fs';
import { join, relative, sep } from 'node:path';

const here = import.meta.dirname;
const workflow = readFileSync(join(here, '..', '..', '.github', 'workflows', 'e2e.yml'), 'utf8');
const config = readFileSync(join(here, 'playwright.config.mjs'), 'utf8');

const projects = [...config.matchAll(/name:\s*'([^']+)',\s*testDir:\s*'\.\/([^']+)'/g)]
    .map(([, name, dir]) => ({ name, dir }));
const ciProjects = new Set([...workflow.matchAll(/npx playwright test --project=([\w-]+)/g)].map(m => m[1]));

function specsUnder(dir) {
    return readdirSync(dir, { withFileTypes: true }).flatMap(entry => {
        if (entry.name === 'node_modules' || entry.name.startsWith('.')) return [];
        const path = join(dir, entry.name);
        if (entry.isDirectory()) return specsUnder(path);
        return entry.name.endsWith('.spec.mjs') ? [relative(here, path).split(sep).join('/')] : [];
    });
}

const problems = [];
if (projects.length === 0) problems.push('Found no projects in playwright.config.mjs (expected `name: \'...\', testDir: \'./...\'`).');

const specs = specsUnder(here);
const outside = specs.filter(spec => !projects.some(p => spec.startsWith(`${p.dir}/`)));
if (outside.length) {
    problems.push(`Not in any project's directory, so Playwright never runs them (move each into one of ${projects.map(p => `${p.dir}/`).join(', ')}):\n  ${outside.join('\n  ')}`);
}

const notInCi = projects.filter(p => !ciProjects.has(p.name)).map(p => p.name);
if (notInCi.length) {
    problems.push(`No \`npx playwright test --project=<name>\` step in e2e.yml for:\n  ${notInCi.join('\n  ')}`);
}
const unknown = [...ciProjects].filter(name => !projects.some(p => p.name === name));
if (unknown.length) {
    problems.push(`e2e.yml runs projects that playwright.config.mjs doesn't define:\n  ${unknown.join('\n  ')}`);
}

const oldScripts = readdirSync(here).filter(f => f.startsWith('verify-') && f.endsWith('.mjs'));
if (oldScripts.length) {
    problems.push(`Standalone verify-*.mjs scripts don't run any more; make each a <topic>.spec.mjs in a project directory:\n  ${oldScripts.join('\n  ')}`);
}

if (problems.length) {
    console.error(problems.join('\n\n'));
    process.exit(1);
}
console.log(`OK: all ${specs.length} specs are in a project, and all ${projects.length} projects run in e2e.yml`);
