#!/usr/bin/env node
// Cross-checks src/Engine/Core/*.aslx library functions against the function
// reference pages under site/src/content/docs/functions/. Fails if:
//   - a library function has no matching "## Name" heading anywhere in
//     site/src/content/docs/functions/*.md (a gap - added but undocumented)
//   - a documented function name doesn't correspond to any current library
//     function, hard-coded function (ExpressionOwner.cs), or explicit
//     allowlist entry (a removed/renamed function - a stale doc)
//
// Run from repo root or from site/: node site/scripts/check-function-docs.mjs

import { readFileSync, readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(__dirname, "..", "..");

const coreDir = join(repoRoot, "src", "Engine", "Core");
const functionsDir = join(repoRoot, "site", "src", "content", "docs", "functions");
const functionProvidersDir = join(repoRoot, "src", "Engine", "Functions");

// Function names that are known to have been removed from Core.aslx but are
// deliberately still documented (e.g. with a "removed in X" note) for anyone
// working on old content. Add to this list rather than deleting such an entry.
const staleAllowlist = new Set([
  "GetTaggedName", // internal-core.md notes this was removed in Quest 5.4
]);

// Hard-coded functions implemented as special cases inside the expression
// evaluator (e.g. because they need the raw, unevaluated parameter) rather
// than as an ordinary method on a class in src/Engine/Functions/.
const specialCasedHardcodedFunctions = new Set([
  "IsDefined", // NcalcExpressionEvaluator.cs - needs the raw parameter name
]);

function readCoreFunctionNames() {
  const names = new Set();
  const pattern = /<function\s+name="([A-Za-z0-9_]+)"/g;
  for (const entry of readdirSync(coreDir, { withFileTypes: true })) {
    if (!entry.isFile() || !entry.name.endsWith(".aslx")) continue;
    // CoreEditor*.aslx / GamebookCoreEditor*.aslx define editor-UI elements,
    // not author-callable library functions - skip them.
    if (/^(Core)?Editor/.test(entry.name) || entry.name.startsWith("CoreEditor") || entry.name.startsWith("GamebookCoreEditor")) {
      continue;
    }
    const text = readFileSync(join(coreDir, entry.name), "utf8");
    for (const m of text.matchAll(pattern)) names.add(m[1]);
  }
  // English.aslx (the default language library) also defines a handful of
  // functions (e.g. GetDefaultPrefix) that games rely on regardless of the
  // author's chosen language.
  const englishPath = join(coreDir, "Languages", "English.aslx");
  const englishText = readFileSync(englishPath, "utf8");
  for (const m of englishText.matchAll(pattern)) names.add(m[1]);
  return names;
}

function readDocumentedFunctionNames() {
  const names = new Set();
  for (const entry of readdirSync(functionsDir, { withFileTypes: true })) {
    if (!entry.isFile() || !entry.name.endsWith(".md")) continue;
    const text = readFileSync(join(functionsDir, entry.name), "utf8");
    for (const m of text.matchAll(/^## ([A-Za-z0-9_]+)/gm)) names.add(m[1]);
  }
  return names;
}

function readHardcodedFunctionNames() {
  const names = new Set();
  // Public methods on the function-provider classes in src/Engine/Functions/
  // (ExpressionOwner.cs, StringFunctions.cs, DateTimeFunctions.cs, ...) are
  // exposed as hard-coded Quest functions of the same name (see
  // functions/hardcoded.md).
  for (const entry of readdirSync(functionProvidersDir, { withFileTypes: true })) {
    if (!entry.isFile() || !entry.name.endsWith(".cs")) continue;
    const text = readFileSync(join(functionProvidersDir, entry.name), "utf8");
    for (const m of text.matchAll(/^\s{4}public\s+(?:static\s+)?(?:async\s+)?[\w<>?[\],. ]+?\s+([A-Za-z0-9_]+)\s*\(/gm)) {
      names.add(m[1]);
    }
  }
  return names;
}

const coreFunctions = readCoreFunctionNames();
const documented = readDocumentedFunctionNames();
const hardcoded = readHardcodedFunctionNames();

const undocumented = [...coreFunctions].filter((n) => !documented.has(n)).sort();
const stale = [...documented]
  .filter(
    (n) =>
      !coreFunctions.has(n) &&
      !hardcoded.has(n) &&
      !staleAllowlist.has(n) &&
      !specialCasedHardcodedFunctions.has(n)
  )
  .sort();

let failed = false;

if (undocumented.length > 0) {
  failed = true;
  console.error(
    `\n${undocumented.length} Core.aslx function(s) have no entry in site/src/content/docs/functions/*.md:\n`
  );
  for (const n of undocumented) console.error(`  - ${n}`);
  console.error(
    "\nAdd a \"## Name\" entry for each (a real doc entry for author-facing functions, or an\n" +
    "internal-core.md placeholder for internal ones)."
  );
}

if (stale.length > 0) {
  failed = true;
  console.error(
    `\n${stale.length} documented function(s) don't match any current Core.aslx function or hard-coded function:\n`
  );
  for (const n of stale) console.error(`  - ${n}`);
  console.error(
    "\nEither the function was renamed/removed (update or remove its doc entry, or add it to\n" +
    "staleAllowlist in this script with a \"removed in X\" note if it's worth keeping for legacy\n" +
    "content), or this script's hard-coded-function detection missed a real function - check\n" +
    "ExpressionOwner.cs."
  );
}

if (failed) {
  process.exit(1);
} else {
  console.log(
    `OK: ${coreFunctions.size} Core.aslx functions and ${hardcoded.size} hard-coded functions all have matching docs.`
  );
}
