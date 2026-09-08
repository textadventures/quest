#!/usr/bin/env node
// Validates the redirect setup. Run after `npm run build` (it inspects dist/):
//   node site/scripts/check-redirects.mjs
//
// Fails if:
//   - public/_redirects has more than 100 path rules. Cloudflare Pages silently
//     honours only the first 100 and drops the rest with no error or warning,
//     which is how ~585 rules came to be dead in production unnoticed. Bulk
//     path-to-path redirects belong in src/redirects.mjs, which Astro turns
//     into static pages with no such limit.
//   - a redirect target doesn't resolve to a built page
//   - a target's #anchor doesn't exist on that page
//   - a redirect source shadows a real page (the page would win, silently
//     making the redirect dead)
//   - a redirect points at another redirect, which costs the reader an extra
//     hop and drops any #anchor on the way through

import { readFileSync, existsSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(__dirname, "..", "..");
const siteDir = join(repoRoot, "site");
const dist = join(siteDir, "dist");

const CLOUDFLARE_REDIRECTS_LIMIT = 100;

if (!existsSync(dist)) {
  console.error("site/dist not found - run `npm run build` in site/ first.");
  process.exit(1);
}

const problems = [];

// --- _redirects rule count ---
const redirectsFile = join(siteDir, "public", "_redirects");
const pathRules = readFileSync(redirectsFile, "utf8")
  .split("\n")
  .map((l) => l.trim())
  .filter((l) => l && !l.startsWith("#") && l.startsWith("/"));
if (pathRules.length > CLOUDFLARE_REDIRECTS_LIMIT) {
  problems.push(
    `public/_redirects has ${pathRules.length} path rules; Cloudflare Pages honours only the first ` +
    `${CLOUDFLARE_REDIRECTS_LIMIT}. Move them to src/redirects.mjs.`
  );
}

// --- redirect targets ---
const redirectsSrc = readFileSync(join(siteDir, "src", "redirects.mjs"), "utf8");
const entries = [...redirectsSrc.matchAll(/^\s*"([^"]+)":\s*"([^"]+)",$/gm)].map((m) => [m[1], m[2]]);
if (entries.length === 0) problems.push("src/redirects.mjs: parsed zero entries - has its shape changed?");

// A built route lives at <path>/index.html (directory build format).
const pageFor = (urlPath) => {
  const clean = urlPath.split("#")[0].replace(/^\/|\/$/g, "");
  const candidates = [join(dist, clean, "index.html"), join(dist, `${clean}.html`)];
  return candidates.find((c) => existsSync(c)) ?? null;
};

// Astro emits redirect stubs as real files too, so a source that also has a
// real page can't be told apart by existence alone - detect the stub's marker.
const isRedirectStub = (file) => readFileSync(file, "utf8").includes('http-equiv="refresh"');

for (const [source, target] of entries) {
  if (/^https?:/.test(target)) continue;

  const page = pageFor(target);
  if (!page) {
    problems.push(`redirect target does not resolve to a built page: ${source} -> ${target}`);
    continue;
  }
  if (isRedirectStub(page)) {
    problems.push(
      `redirect points at another redirect (extra hop; any #anchor is dropped): ${source} -> ${target}`
    );
    continue;
  }

  const [, anchor] = target.split("#");
  if (anchor && !readFileSync(page, "utf8").includes(`id="${anchor}"`)) {
    problems.push(`redirect target anchor not found on the page: ${source} -> ${target}`);
  }

  const sourcePage = pageFor(source);
  if (sourcePage && !isRedirectStub(sourcePage)) {
    problems.push(`redirect source shadows a real page (the redirect is dead): ${source}`);
  }
}

if (problems.length > 0) {
  console.error(`\n${problems.length} redirect problem(s):\n`);
  for (const p of problems) console.error(`  - ${p}`);
  console.error("");
  process.exit(1);
}

console.log(
  `OK: ${entries.length} redirects resolve, and public/_redirects has ${pathRules.length} ` +
  `path rule(s) (limit ${CLOUDFLARE_REDIRECTS_LIMIT}).`
);
