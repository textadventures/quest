#!/usr/bin/env node
// Gap-detection for the three translation layers (AppShell UI json, Editor
// .aslx, player .aslx), driven by the language registry in
// tools/i18n/languages.json. See docs/i18n-style-guide.md for register/
// glossary decisions and the plan behind this tool.
//
// Usage:
//   node tools/i18n/audit.mjs               # markdown report to stdout, exit 0
//   node tools/i18n/audit.mjs --json        # machine-readable report, exit 0
//   node tools/i18n/audit.mjs --strict      # exit 1 if any `supported: true`
//                                           # language layer is below 100%
//                                           # coverage
//   node tools/i18n/audit.mjs --root <dir>  # read language/locale files from
//                                           # <dir> instead of this checkout
//                                           # (e.g. a different git ref's
//                                           # worktree) while still using
//                                           # this script's own registry and
//                                           # detection logic - see
//                                           # compare-duplicates.mjs, which
//                                           # uses this to diff two refs
//                                           # without needing the *other*
//                                           # ref to already have this tool.
//
// Duplicate-name detection: within a single .aslx file, a repeated
// `<template name="X">`/`<dynamictemplate name="X">` resolves LAST wins,
// not first — see Template.AddTemplate in src/Engine/Templates.cs and the
// "base .aslx file's own duplicate template names lose to the first, not
// the last" fix (commit dbd6ce28) for why. A duplicate whose occurrences
// have identical text is harmless dead weight; a duplicate whose
// occurrences differ means an earlier, intentional translation is silently
// shadowed by a later one — that's the bug class to look for.

import { readFileSync, existsSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const args = process.argv.slice(2);
const jsonOutput = args.includes('--json');
const strict = args.includes('--strict');
const rootIdx = args.indexOf('--root');
const REPO_ROOT = rootIdx === -1 ? path.resolve(__dirname, '..', '..') : path.resolve(args[rootIdx + 1]);

const LANG_DIR = path.join(REPO_ROOT, 'src/Engine/Core/Languages');
const LOCALE_DIR = path.join(REPO_ROOT, 'src/AppShell/static/locales');

// Always this checkout's own registry/detection logic, even when --root
// points elsewhere - see the --root usage note above.
const registry = JSON.parse(readFileSync(path.join(__dirname, 'languages.json'), 'utf8'));
const languages = registry.languages;

function stripBom(text) {
  return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

function stripXmlComments(text) {
  return text.replace(/<!--[\s\S]*?-->/g, '');
}

const TEMPLATE_KEY_RE = /<(?:template|dynamictemplate)\s+name="([A-Za-z0-9_]+)"[^>]*>([\s\S]*?)<\/(?:template|dynamictemplate)>/g;

// Returns an array of { key, value } in file order (values used only to
// tell harmless duplicates from ones where the shadowed text differs).
function extractAslxEntries(filePath) {
  if (!existsSync(filePath)) return null;
  const raw = stripXmlComments(stripBom(readFileSync(filePath, 'utf8')));
  const entries = [];
  let m;
  while ((m = TEMPLATE_KEY_RE.exec(raw))) {
    entries.push({ key: m[1], value: m[2].trim() });
  }
  return entries;
}

function extractJsonEntries(filePath) {
  if (!existsSync(filePath)) return null;
  const obj = JSON.parse(readFileSync(filePath, 'utf8'));
  return Object.entries(obj).map(([key, value]) => ({ key, value: String(value) }));
}

// Groups baseline (English) keys that share the exact same source text, e.g.
// EditorObjectScriptsScripts and EditorScriptsScriptsScripts both being
// "Scripts". A translation that renders one of these as "Programas" and the
// other as "Guiones" is very likely an inconsistency, not an intentional
// distinction - same source term, different tabs/contexts, should still read
// as the same word to a player/author. This is a heuristic, not a hard rule:
// some groups legitimately need context-dependent translations, so it's
// reported for human review rather than folded into --strict.
function buildValueGroups(baselineEntries) {
  const byValue = new Map();
  for (const { key, value } of baselineEntries) {
    if (!value) continue;
    if (!byValue.has(value)) byValue.set(value, []);
    byValue.get(value).push(key);
  }
  return [...byValue.entries()].filter(([, keys]) => keys.length > 1);
}

function analyzeLayer(layerName, baselineFile, fileField, extractFn) {
  const baselineEntries = extractFn(baselineFile);
  const baselineKeys = new Set(baselineEntries.map((e) => e.key));
  const valueGroups = buildValueGroups(baselineEntries);

  const rows = [];
  for (const lang of languages) {
    const fileName = lang[fileField];
    if (!fileName) {
      rows.push({ id: lang.id, displayName: lang.displayName, present: false });
      continue;
    }
    const filePath = path.join(fileName.endsWith('.json') ? LOCALE_DIR : LANG_DIR, fileName);
    const entries = extractFn(filePath);
    if (entries === null) {
      rows.push({ id: lang.id, displayName: lang.displayName, present: false, missingFile: fileName });
      continue;
    }

    const keySet = new Set(entries.map((e) => e.key));
    const missing = [...baselineKeys].filter((k) => !keySet.has(k)).sort();
    const extra = [...keySet].filter((k) => !baselineKeys.has(k)).sort();

    const byKey = new Map();
    for (const { key, value } of entries) {
      if (!byKey.has(key)) byKey.set(key, []);
      byKey.get(key).push(value);
    }
    const duplicates = [...byKey.entries()]
      .filter(([, values]) => values.length > 1)
      .map(([key, values]) => ({
        key,
        occurrences: values.length,
        harmless: new Set(values).size === 1,
      }));

    const coverage = baselineKeys.size === 0 ? 100 : (100 * (baselineKeys.size - missing.length)) / baselineKeys.size;

    // Last occurrence wins at runtime (see file header comment), so that's
    // what a terminology check should compare too.
    const lastValueByKey = new Map([...byKey.entries()].map(([k, vs]) => [k, vs[vs.length - 1]]));
    const terminologyInconsistencies = [];
    for (const [sourceValue, keys] of valueGroups) {
      const present = keys.filter((k) => lastValueByKey.has(k));
      if (present.length < 2) continue;
      const translated = present.map((k) => lastValueByKey.get(k));
      if (new Set(translated).size > 1) {
        terminologyInconsistencies.push({
          sourceValue,
          entries: present.map((k) => ({ key: k, value: lastValueByKey.get(k) })),
        });
      }
    }

    rows.push({
      id: lang.id,
      displayName: lang.displayName,
      present: true,
      total: keySet.size,
      missingCount: missing.length,
      extraCount: extra.length,
      missing,
      extra,
      duplicates,
      coverage,
      terminologyInconsistencies,
    });
  }

  return { layerName, baselineSize: baselineKeys.size, rows };
}

const layers = [
  analyzeLayer('AppShell UI (json)', path.join(LOCALE_DIR, 'en.json'), 'appShellLocale', extractJsonEntries),
  analyzeLayer('Editor (.aslx)', path.join(LANG_DIR, 'EditorEnglish.aslx'), 'editorAslx', extractAslxEntries),
  analyzeLayer('Player (.aslx)', path.join(LANG_DIR, 'English.aslx'), 'playerAslx', extractAslxEntries),
];

function pct(n) {
  return `${n.toFixed(1)}%`;
}

function printMarkdown() {
  for (const layer of layers) {
    console.log(`\n## ${layer.layerName} (baseline: ${layer.baselineSize} keys)\n`);
    console.log('| Language | Present | Coverage | Missing | Extra | Duplicates | Terminology |');
    console.log('|---|---|---|---|---|---|---|');
    for (const row of layer.rows) {
      if (!row.present) {
        console.log(`| ${row.displayName} | ${row.missingFile ? `file missing: ${row.missingFile}` : 'no'} | — | — | — | — | — |`);
        continue;
      }
      const dupSummary = row.duplicates.length === 0
        ? '—'
        : row.duplicates.map((d) => `${d.key}${d.harmless ? '' : ' ⚠️differs'}`).join(', ');
      const termSummary = row.terminologyInconsistencies.length === 0 ? '—' : `${row.terminologyInconsistencies.length} ⚠️`;
      console.log(`| ${row.displayName} | yes | ${pct(row.coverage)} | ${row.missingCount} | ${row.extraCount} | ${dupSummary} | ${termSummary} |`);
    }

    for (const row of layer.rows) {
      if (!row.present || row.terminologyInconsistencies.length === 0) continue;
      console.log(`\n**${row.displayName} terminology inconsistencies** (same English source text, differing translations - review for consistency, not necessarily a bug):\n`);
      for (const { sourceValue, entries } of row.terminologyInconsistencies) {
        const rendered = entries.map((e) => `${e.key}=${JSON.stringify(e.value)}`).join(', ');
        console.log(`- "${sourceValue}": ${rendered}`);
      }
    }
  }

  console.log('\n(Duplicates marked "⚠️differs" have occurrences with different text — the earlier one is silently');
  console.log('shadowed by the later one at runtime and is worth a human look. Duplicates without the marker have');
  console.log('identical text in every occurrence and are harmless dead weight.)');
  console.log('\n(Terminology inconsistencies: two or more keys share identical English source text but were');
  console.log('translated differently - often a sign one translation used a different word for the same concept.');
  console.log('See docs/i18n-style-guide.md\'s glossary section. Informational only, not part of --strict.)');
}

// --strict only asserts the regression guard that a single checkout can know
// on its own: a `supported: true` language must stay at 100% coverage in
// every layer. Whether a differing-value duplicate is *new* depends on two
// points in history (base vs. head), which this stateless per-checkout tool
// can't determine — that comparison is done in CI by running this script
// against both refs and diffing the `duplicates` lists (see
// .github/workflows/build-and-test.yml), not here.
let hasStrictFailure = false;
for (const layer of layers) {
  for (const row of layer.rows) {
    if (!row.present) continue;
    const lang = languages.find((l) => l.id === row.id);
    if (lang.supported && row.coverage < 100) hasStrictFailure = true;
  }
}

if (jsonOutput) {
  console.log(JSON.stringify({ layers, strictFailure: hasStrictFailure }, null, 2));
} else {
  printMarkdown();
}

if (strict && hasStrictFailure) {
  process.exitCode = 1;
}
