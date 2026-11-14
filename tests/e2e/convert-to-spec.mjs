// One-off codemod: converts a standalone `verify-*.mjs` script (top-level await,
// pass/fail signalled through process.exitCode) into a Playwright Test spec at
// `<project>/<name>.spec.mjs`. Deleted once every script has been converted.
//
// Usage: node convert-to-spec.mjs <project> verify-a.mjs [verify-b.mjs ...]
//
// What it does to each script:
// - keeps the header comment and imports at the top, adds `import { test }`;
// - wraps everything else in one `test('<name>', async ({ baseURL }) => { ... })`,
//   re-indented by four spaces;
// - `process.argv[2] || '<default>'` becomes `baseURL` (set per project in
//   playwright.config.mjs);
// - `process.exitCode = 1` becomes `throw <err>` when it sits directly in a
//   `catch (<err>)` block, otherwise `throw new Error(...)`, and
//   `process.exit(1)` becomes `throw new Error(...)`, so a failure fails the test;
// - rewrites `./lib/`, `./fixtures/` and `import.meta.dirname`-relative paths for
//   the file's new home one directory down.
// Anything else touching process.argv/exit/exitCode is reported for a manual fix
// and the file is left unconverted.
import { readFileSync, writeFileSync, mkdirSync, rmSync } from 'node:fs';
import { basename, join } from 'node:path';

const [project, ...files] = process.argv.slice(2);
if (!project || files.length === 0) {
    console.error('Usage: node convert-to-spec.mjs <project> verify-a.mjs [...]');
    process.exit(2);
}

const here = import.meta.dirname;
// The spec lives one directory below the script it came from.
const rewritePaths = (line) => line
    .replace(/(['"])\.\/(lib|fixtures)\//g, '$1../$2/')
    .replace(/(import\.meta\.dirname|dirname\(fileURLToPath\(import\.meta\.url\)\)), /g, "$1, '..', ");
const GENERIC_FAILURE = "throw new Error('One or more checks failed (see the output above)');";
let problems = 0;

for (const file of files) {
    const name = basename(file, '.mjs')
        .replace(/^verify-/, '')
        .replace(new RegExp(`^${project}-`), '')
        .replace(new RegExp(`-${project}$`), '');
    const lines = readFileSync(join(here, basename(file)), 'utf8').replace(/\n$/, '').split('\n');

    // Header: leading comments and blank lines. Then imports (possibly multi-line).
    let i = 0;
    while (i < lines.length && (lines[i].startsWith('//') || lines[i].trim() === '')) i++;
    const header = lines.slice(0, i);
    const imports = [];
    while (i < lines.length && (lines[i].startsWith('import ') || lines[i].trim() === '')) {
        if (lines[i].startsWith('import ')) {
            while (!lines[i].trimEnd().endsWith(';')) imports.push(lines[i++]);
        }
        imports.push(lines[i++]);
    }
    while (imports.length && imports.at(-1).trim() === '') imports.pop();
    for (let k = 0; k < imports.length; k++) imports[k] = rewritePaths(imports[k]);
    const body = lines.slice(i);

    const errors = [];
    const indentOf = (s) => s.match(/^ */)[0].length;
    const out = body.map((line, n) => {
        if (/^\s*import /.test(line)) errors.push(`line ${i + n + 1}: import after code`);
        let l = rewritePaths(line)
            .replace(/process\.argv\[2\]\s*(\|\||\?\?)\s*(['"`])[^'"`]*\2/, 'baseURL')
            .replace(/process\.argv\.slice\(2\)\.find\(a => !a\.startsWith\('--'\)\)\s*\|\|\s*(['"`])[^'"`]*\1/, 'baseURL')
            .replace(/process\.exit\(1\);/g, GENERIC_FAILURE);
        if (/process\.exitCode = 1;/.test(l)) {
            // The enclosing block opens on the nearest earlier line indented less than this one.
            const ind = indentOf(l);
            let opener = '';
            for (let k = n - 1; k >= 0; k--) {
                if (body[k].trim() !== '' && indentOf(body[k]) < ind) { opener = body[k]; break; }
            }
            const caught = opener.match(/catch\s*\((\w+)\)/);
            if (caught && indentOf(opener) !== 0) {
                errors.push(`line ${i + n + 1}: process.exitCode in a nested catch (would stop later checks)`);
            }
            l = l.replace('process.exitCode = 1;', caught ? `throw ${caught[1]};` : GENERIC_FAILURE);
        }
        if (/process\.(argv|exit|exitCode)/.test(l)) errors.push(`line ${i + n + 1}: ${l.trim()}`);
        // createRequire(import.meta.url) only resolves packages, which node_modules
        // one directory up still provides.
        if (/import\.meta\.url|import\.meta\.dirname/.test(l.replace('createRequire(import.meta.url)', '')) && l === line) {
            errors.push(`line ${i + n + 1}: unrewritten path relative to the script: ${l.trim()}`);
        }
        return l;
    });
    if (errors.length) {
        problems++;
        console.error(`${file}: needs a manual fix\n  ${errors.join('\n  ')}`);
        continue;
    }

    const usesBaseUrl = out.some(l => /\bbaseURL\b/.test(l));
    while (out.length && out[0].trim() === '') out.shift();
    const spec = [
        ...header,
        "import { test } from '@playwright/test';",
        ...imports,
        '',
        `test('${name}', ${usesBaseUrl ? 'async ({ baseURL })' : 'async ()'} => {`,
        ...out.map(l => (l.trim() === '' ? '' : `    ${l}`)),
        '});',
        '',
    ].join('\n');
    mkdirSync(join(here, project), { recursive: true });
    writeFileSync(join(here, project, `${name}.spec.mjs`), spec);
    rmSync(join(here, basename(file)));
    console.log(`${file} -> ${project}/${name}.spec.mjs`);
}
process.exitCode = problems ? 1 : 0;
