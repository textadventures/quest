#!/usr/bin/env node
// Checks every <helpurl> in the editor definitions (src/Engine/Core/*.aslx)
// against the built documentation. Run after `npm run build` (it inspects
// dist/):  node site/scripts/check-editor-help-urls.mjs
//
// A <helpurl> is the target of the "?" shown on an element editor tab, so a
// docs page that moves or is renamed would silently start 404ing for authors.
// Both sides live in this repo, so the mismatch can be a build failure instead.
//
// Also fails if a <helpurl> sits anywhere other than directly inside a <tab> -
// only tabs read it, so one on a <control> would be silently dead.

import { readFileSync, existsSync, readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(__dirname, "..", "..");
const coreDir = join(repoRoot, "src", "Engine", "Core");
const dist = join(repoRoot, "site", "dist");

if (!existsSync(dist)) {
  console.error("site/dist not found - run `npm run build` in site/ first.");
  process.exit(1);
}

const problems = [];
let checked = 0;

// Nearest unclosed <tab>/<control>/<editor>/<library> before the given offset.
function enclosingElement(text, offset) {
  const stack = [];
  const before = text.slice(0, offset);
  for (const m of before.matchAll(/<(\/?)(tab|control|editor|library)[ >/]/g)) {
    if (m[1]) stack.pop();
    else stack.push(m[2]);
  }
  return stack[stack.length - 1] ?? null;
}

for (const entry of readdirSync(coreDir, { withFileTypes: true })) {
  if (!entry.isFile() || !entry.name.endsWith(".aslx")) continue;
  const text = readFileSync(join(coreDir, entry.name), "utf8");

  for (const m of text.matchAll(/<helpurl>([^<]*)<\/helpurl>/g)) {
    checked++;
    const url = m[1].trim();
    const line = text.slice(0, m.index).split("\n").length;
    const where = `${entry.name}:${line}`;

    const parent = enclosingElement(text, m.index);
    if (parent !== "tab") {
      problems.push(`${where}: <helpurl> inside <${parent}> - only <tab> reads it, so this one is dead`);
      continue;
    }
    if (!url.startsWith("/") || !url.endsWith("/")) {
      problems.push(`${where}: "${url}" should be a site-relative path with a trailing slash (e.g. /howto/world/exits/)`);
      continue;
    }
    if (!existsSync(join(dist, url.replace(/^\/|\/$/g, ""), "index.html"))) {
      problems.push(`${where}: "${url}" does not resolve to a built page`);
    }
  }
}

if (problems.length > 0) {
  console.error(`\n${problems.length} editor <helpurl> problem(s):\n`);
  for (const p of problems) console.error(`  - ${p}`);
  console.error("");
  process.exit(1);
}

console.log(`OK: all ${checked} editor <helpurl> target(s) resolve to a built page.`);
