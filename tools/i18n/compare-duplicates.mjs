#!/usr/bin/env node
// Fails if `head.json` (this checkout's audit.mjs --json output) has a
// differing-value duplicate template/dynamictemplate key that isn't already
// present in `base.json` (the PR's base ref, audited with the *same*,
// current audit.mjs via --root - see that script's --root usage note).
// A harmless duplicate (identical text in every occurrence) never fails -
// only a *new* one where the shadowed text actually differs, since that's
// the bug class that matters (an earlier translation silently going dead).
//
// Usage: node tools/i18n/compare-duplicates.mjs <base.json> <head.json>

import { readFileSync } from 'node:fs';

const [baseFile, headFile] = process.argv.slice(2);
if (!baseFile || !headFile) {
  console.error('Usage: node tools/i18n/compare-duplicates.mjs <base.json> <head.json>');
  process.exit(2);
}

const base = JSON.parse(readFileSync(baseFile, 'utf8'));
const head = JSON.parse(readFileSync(headFile, 'utf8'));

function differingDupeKeys(report, layerName, langId) {
  const layer = report.layers.find((l) => l.layerName === layerName);
  const row = layer?.rows.find((r) => r.id === langId && r.present);
  if (!row) return new Set();
  return new Set(row.duplicates.filter((d) => !d.harmless).map((d) => d.key));
}

let newlyIntroduced = [];
for (const layer of head.layers) {
  for (const row of layer.rows) {
    if (!row.present) continue;
    const baseDupes = differingDupeKeys(base, layer.layerName, row.id);
    const headDupes = differingDupeKeys(head, layer.layerName, row.id);
    for (const key of headDupes) {
      if (!baseDupes.has(key)) {
        newlyIntroduced.push(`${layer.layerName} / ${row.displayName}: "${key}"`);
      }
    }
  }
}

if (newlyIntroduced.length > 0) {
  console.error('New differing-value duplicate template keys introduced by this change:');
  for (const line of newlyIntroduced) console.error(`  - ${line}`);
  console.error(
    '\nA duplicate <template>/<dynamictemplate> name in the same file resolves LAST wins ' +
    '(see audit.mjs header) - an earlier definition with different text is now silently dead. ' +
    'Either remove the shadowed one or rename so both are reachable.'
  );
  process.exit(1);
}

console.log('No newly introduced differing-value duplicate keys.');
