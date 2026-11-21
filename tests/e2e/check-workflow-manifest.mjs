// Manifest check: makes sure every e2e spec actually runs in the nightly e2e workflow.
// Playwright Test only runs a spec that sits in one of playwright.config.mjs's project
// directories, and CI only runs a project that .github/workflows/e2e.yml has a
// `npx playwright test --project=<name>` step for. A spec anywhere else, or a project
// with no step, would pass every check by never running at all. So this fails if:
// - a *.spec.mjs file isn't in a project's testDir;
// - a project in the config has no `--project=<name>` step in e2e.yml (or a step names
//   a project the config doesn't have);
// - a standalone verify-*.mjs script is left in tests/e2e (the old format, which
//   nothing runs any more);
// - a script under tests/e2e passes waitForFunction's options as its second argument.
//   Playwright's signature is waitForFunction(pageFunction, arg, options), so
//   waitForFunction(fn, { timeout: N }) hands that object to fn as its argument and no
//   timeout applies: a stuck wait hangs until the whole test times out instead of
//   failing fast. The options go third: waitForFunction(fn, null, { timeout: N }).
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

// The top-level argument texts of the call whose '(' is at index `open`. Skips over
// strings, template literals and comments, which is enough for these scripts.
function callArgs(src, open) {
    const args = [];
    let depth = 0, start = open + 1;
    for (let i = open; i < src.length; i++) {
        const c = src[i];
        if (c === '"' || c === "'" || c === '`') {
            for (i++; i < src.length && src[i] !== c; i++) {
                if (src[i] === '\\') i++;
                else if (c === '`' && src[i] === '$' && src[i + 1] === '{') {
                    for (let d = 0; i < src.length; i++) {
                        if (src[i] === '{') d++;
                        else if (src[i] === '}' && --d === 0) break;
                    }
                }
            }
        } else if (c === '/' && src[i + 1] === '/') {
            i = src.indexOf('\n', i);
            if (i < 0) break;
        } else if (c === '/' && src[i + 1] === '*') {
            i = src.indexOf('*/', i + 2) + 1;
            if (i <= 0) break;
        } else if ('([{'.includes(c)) {
            depth++;
        } else if (')]}'.includes(c)) {
            if (--depth === 0) { args.push(src.slice(start, i)); break; }
        } else if (c === ',' && depth === 1) {
            args.push(src.slice(start, i));
            start = i + 1;
        }
    }
    const trimmed = args.map(a => a.trim());
    if (trimmed.at(-1) === '') trimmed.pop(); // trailing comma
    return trimmed;
}

function scriptsUnder(dir) {
    return readdirSync(dir, { withFileTypes: true }).flatMap(entry => {
        if (entry.name === 'node_modules' || entry.name.startsWith('.')) return [];
        const path = join(dir, entry.name);
        if (entry.isDirectory()) return scriptsUnder(path);
        return entry.name.endsWith('.mjs') && path !== import.meta.filename ? [path] : [];
    });
}

const misplacedOptions = scriptsUnder(here).flatMap(path => {
    const src = readFileSync(path, 'utf8');
    return [...src.matchAll(/\bwaitForFunction\s*\(/g)].flatMap(m => {
        const args = callArgs(src, m.index + m[0].length - 1);
        if (args.length !== 2 || !/^\{\s*(timeout|polling)\b/.test(args[1])) return [];
        const line = src.slice(0, m.index).split('\n').length;
        return [`${relative(here, path).split(sep).join('/')}:${line}`];
    });
});
if (misplacedOptions.length) {
    problems.push(`waitForFunction(fn, { timeout }) hands the options to fn as its argument, so no timeout applies; pass them third, as waitForFunction(fn, null, { timeout }):\n  ${misplacedOptions.join('\n  ')}`);
}

if (problems.length) {
    console.error(problems.join('\n\n'));
    process.exit(1);
}
console.log(`OK: all ${specs.length} specs are in a project, and all ${projects.length} projects run in e2e.yml`);
