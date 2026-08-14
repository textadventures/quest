#!/usr/bin/env node
// Post-processes the compiled Tailwind/Skeleton output so its rules cannot
// leak outside a `.qv-chrome` container. Tailwind's `@utility` declarations
// can't be nested inside `@scope` at source time (build fails), so instead
// we compile normally and wrap the *compiled* CSS afterwards:
//
//   1. `:root, :host` (where Tailwind/Skeleton hang their CSS custom
//      properties) is rewritten to `:scope, :host`. Plain selectors written
//      inside `@scope (.qv-chrome) { ... }` implicitly mean "descendant of
//      the scope root" — they never match the root element itself. `:scope`
//      is the one selector that *does* match the root, so this is how the
//      theme tokens end up defined on the `.qv-chrome` element (custom
//      properties then inherit normally to its descendants regardless of
//      the scope boundary, which only governs selector matching).
//   2. `@property` declarations are hoisted out to the top level. Chromium
//      silently drops an entire `@scope` block's sibling content if it
//      contains a nested `@property` rule — verified empirically, not
//      documented. `@property` only registers a custom-property's syntax; it
//      doesn't paint anything on its own, so hoisting it unscoped is safe.
//   3. The rest of the stylesheet is wrapped in `@scope (.qv-chrome) { ... }`,
//      so global-looking rules (`*`, `html`, `body`, `::selection`, etc. —
//      both Tailwind's preflight reset and Skeleton's base/globals.css) only
//      ever match elements inside a `.qv-chrome` subtree, never the rest of
//      the page (playercore.css, jQuery UI, game-authored HTML).
//
// Run via `npm run build`, not directly — it operates in place on whatever
// path the Tailwind CLI just wrote.

import fs from 'node:fs';

const target = process.argv[2];
if (!target) {
  console.error('Usage: node scope-css.mjs <path-to-compiled-css>');
  process.exit(1);
}

// Extracts every top-level-or-nested `@<atRuleName> ... { ... }` block found
// anywhere in `css` (brace-depth aware), returning the text with those blocks
// removed and the extracted blocks themselves (their old wrapper, e.g. a now
// possibly-empty `@layer`/`@supports`, is left in place and is harmless).
function extractAtRuleBlocks(css, atRuleName) {
  const needle = '@' + atRuleName;
  const blocks = [];
  let out = '';
  let i = 0;
  while (i < css.length) {
    const idx = css.indexOf(needle, i);
    if (idx === -1) {
      out += css.slice(i);
      break;
    }
    out += css.slice(i, idx);
    const open = css.indexOf('{', idx);
    let depth = 1;
    let k = open + 1;
    while (depth > 0 && k < css.length) {
      if (css[k] === '{') depth++;
      else if (css[k] === '}') depth--;
      k++;
    }
    blocks.push(css.slice(idx, k));
    i = k;
  }
  return { css: out, blocks };
}

let css = fs.readFileSync(target, 'utf8');
css = css.replace(/:root,\s*:host\s*\{/g, ':scope, :host {');

const { css: withoutProperties, blocks: propertyBlocks } = extractAtRuleBlocks(css, 'property');

const hoisted = propertyBlocks.join('\n');
const scoped = `@scope (.qv-chrome) {\n${withoutProperties}\n}\n`;
fs.writeFileSync(target, `${hoisted}\n${scoped}`);
