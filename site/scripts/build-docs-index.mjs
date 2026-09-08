#!/usr/bin/env node
// Generates src/AppShell/src/lib/docs-index.generated.ts - a map from a script
// editor keyword (the <appliesto> value in src/Engine/Core/CoreEditor*.aslx) to
// the documentation page + anchor describing it, so the editor can deep-link
// each script command into questviva.com.
//
// Generated rather than hand-maintained because the mapping is mechanical: the
// docs already use "## Name" headings whose anchors match the keyword. Sibling
// script check-function-docs.mjs makes CI fail if a Core.aslx function has no
// such heading, so this index stays complete by construction.
//
// Run from repo root or from site/:
//   node site/scripts/build-docs-index.mjs           # rewrite the generated file
//   node site/scripts/build-docs-index.mjs --check   # fail if it's out of date

import { readFileSync, writeFileSync, readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(__dirname, "..", "..");

const coreDir = join(repoRoot, "src", "Engine", "Core");
const docsDir = join(repoRoot, "site", "src", "content", "docs");
const outPath = join(repoRoot, "src", "AppShell", "src", "lib", "docs-index.generated.ts");

// Keywords that name a script command but have no useful reference entry:
// syntax rather than a command (`=`, `//`, `@failed`), or internal
// pseudo-elements the editor uses to group the tree (`_objects`, ...).
// Applied to the bare name, after any "(function)" prefix has been stripped.
const isInternalName = (name) => name.startsWith("_") || !/^[A-Za-z]/.test(name);

// Element-type editors (<editor name="object">, etc.) rather than script
// commands - they belong to the element reference, not the function reference.
// Handled by Mechanism B (a curated <helpurl>), deliberately not indexed here.
const elementTypeKeywords = new Set([
  "command", "exit", "function", "game", "include", "javascript",
  "object", "timer", "turnscript", "type", "verb", "walkthrough",
]);

// Three function names appear as a "## " heading on two pages at once, because
// gamebook pages and Text Adventure dialogue pages have same-named functions.
// Which one is right depends on the game type, which the caller knows and this
// index doesn't - so both are recorded. Spelled out in both directions rather
// than letting one side fall out of readdir order, which would silently pick
// functions/gamebook for every one of them (g sorts before u).
const dualPageFunctions = {
  AddPageLink: { default: "functions/user-interface", gamebook: "functions/gamebook" },
  HasSeenPage: { default: "functions/user-interface", gamebook: "functions/gamebook" },
  RemovePageLink: { default: "functions/user-interface", gamebook: "functions/gamebook" },
};

function readScriptKeywords() {
  const keywords = new Set();
  for (const entry of readdirSync(coreDir, { withFileTypes: true })) {
    if (!entry.isFile() || !entry.name.endsWith(".aslx")) continue;
    const text = readFileSync(join(coreDir, entry.name), "utf8");
    for (const m of text.matchAll(/<appliesto>(.*?)<\/appliesto>/g)) {
      keywords.add(m[1].trim());
    }
  }
  return keywords;
}

// slug -> Set of "## " headings on that page.
function readHeadings() {
  const bySlug = new Map();
  const add = (slug, file) => {
    const text = readFileSync(file, "utf8");
    const headings = new Set();
    for (const m of text.matchAll(/^## (.+)$/gm)) headings.add(m[1].trim());
    bySlug.set(slug, headings);
  };
  for (const entry of readdirSync(join(docsDir, "functions"), { withFileTypes: true })) {
    if (!entry.isFile() || !entry.name.endsWith(".md")) continue;
    const slug = `functions/${entry.name.replace(/\.md$/, "")}`;
    add(slug === "functions/index" ? "functions" : slug, join(docsDir, "functions", entry.name));
  }
  add("scripts", join(docsDir, "scripts", "index.md"));
  return bySlug;
}

// Starlight derives an anchor from the heading text: lowercased, spaces to
// dashes. Every heading involved here is a single word, but normalise anyway.
const anchorFor = (heading) => heading.toLowerCase().replace(/\s+/g, "-");

function findPage(headingsBySlug, name, preferredSlugs) {
  const matches = [];
  for (const [slug, headings] of headingsBySlug) {
    for (const heading of headings) {
      if (heading.toLowerCase() === name.toLowerCase()) matches.push([slug, heading]);
    }
  }
  if (matches.length === 0) return null;
  for (const preferred of preferredSlugs) {
    const hit = matches.find(([slug]) => slug === preferred);
    if (hit) return hit;
  }
  // Prefer a functions/* page over scripts for a (function) keyword and vice
  // versa; beyond that, first match wins deterministically (readdir is sorted).
  return matches[0];
}

function build() {
  const headingsBySlug = readHeadings();
  const entries = [];
  const unmatched = [];

  for (const keyword of [...readScriptKeywords()].sort()) {
    const isFunction = keyword.startsWith("(function)");
    const name = isFunction ? keyword.slice("(function)".length) : keyword;
    if (isInternalName(name) || elementTypeKeywords.has(name)) continue;

    // A plain keyword is a script command (documented on the scripts page); a
    // (function) keyword is a function. This is what keeps `ask` the script
    // command and `Ask` the function pointing at different pages.
    const dual = isFunction ? dualPageFunctions[name] : undefined;
    const preferred = isFunction ? (dual ? [dual.default] : []) : ["scripts"];

    const match = findPage(headingsBySlug, name, preferred);
    if (!match) {
      unmatched.push(keyword);
      continue;
    }
    const [slug, heading] = match;
    const entry = { keyword, path: `/${slug}/#${anchorFor(heading)}` };
    if (dual) {
      if (slug !== dual.default) throw new Error(`${name}: expected ${dual.default}, resolved ${slug}`);
      if (!headingsBySlug.get(dual.gamebook)?.has(heading)) {
        throw new Error(`${name}: no "## ${heading}" heading on ${dual.gamebook} - is dualPageFunctions stale?`);
      }
      entry.gamebookPath = `/${dual.gamebook}/#${anchorFor(heading)}`;
    }
    entries.push(entry);
  }

  return { entries, unmatched };
}

const { entries, unmatched } = build();

const lines = [
  "// GENERATED FILE - do not edit by hand.",
  "// Regenerate with: node site/scripts/build-docs-index.mjs",
  "//",
  "// Maps a script editor keyword (<appliesto> in src/Engine/Core/CoreEditor*.aslx)",
  "// to the documentation page and anchor describing it. See that script for how",
  "// entries are derived and why a few keywords are deliberately absent.",
  "",
  "export interface DocsIndexEntry {",
  "    /** Site-relative path, including the anchor. */",
  "    path: string;",
  "    /** Same-named function on the gamebook page, when one exists. */",
  "    gamebookPath?: string;",
  "}",
  "",
  "export const DOCS_INDEX: Readonly<Record<string, DocsIndexEntry>> = {",
  ...entries.map(({ keyword, path, gamebookPath }) => {
    const value = gamebookPath
      ? `{ path: ${JSON.stringify(path)}, gamebookPath: ${JSON.stringify(gamebookPath)} }`
      : `{ path: ${JSON.stringify(path)} }`;
    return `    ${JSON.stringify(keyword)}: ${value},`;
  }),
  "};",
  "",
];
const output = lines.join("\n");

if (process.argv.includes("--check")) {
  let current = null;
  try {
    current = readFileSync(outPath, "utf8");
  } catch {
    // Missing file - reported as out of date below.
  }
  if (current !== output) {
    console.error(
      "\nsrc/AppShell/src/lib/docs-index.generated.ts is out of date with the docs.\n" +
      "Regenerate it with: node site/scripts/build-docs-index.mjs\n"
    );
    process.exit(1);
  }
  console.log(`OK: docs index is up to date (${entries.length} script keywords mapped).`);
} else {
  writeFileSync(outPath, output, "utf8");
  console.log(`Wrote ${entries.length} entries to src/AppShell/src/lib/docs-index.generated.ts`);
  if (unmatched.length > 0) {
    console.log(`\n${unmatched.length} keyword(s) had no matching "## " heading and were skipped:`);
    for (const k of unmatched) console.log(`  - ${k}`);
  }
}
