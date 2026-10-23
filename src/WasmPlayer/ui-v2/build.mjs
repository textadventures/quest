#!/usr/bin/env node
// Builds player UI v2 (docs/ui-v2-plan.md) from src/ and styles/ into wwwroot/v2/v2.js and
// wwwroot/v2/v2.css, which wasm-player.js loads as the v2 bundle (playerUiBundles). Both files
// are generated and gitignored; WasmPlayer.csproj's BuildChromeCss target runs this through
// `npm run build` before the static web assets are collected.
//
// The script is a classic script, not an ES module, because the host loads every UI bundle's
// scripts with plain <script> tags. esbuild doesn't type-check, so `npm run build` runs tsc
// (tsconfig.json) first.

import * as esbuild from 'esbuild';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const outDir = path.resolve(here, '..', 'wwwroot', 'v2');

const common = {
    bundle: true,
    minify: true,
    logLevel: 'warning',
    // Every browser WasmPlayer runs in has WebAssembly and custom elements, so nothing older
    // than this can load the player anyway.
    target: ['chrome111', 'edge111', 'firefox114', 'safari16.4'],
    legalComments: 'none',
};

await Promise.all([
    esbuild.build({
        ...common,
        entryPoints: [path.join(here, 'src', 'main.ts')],
        outfile: path.join(outDir, 'v2.js'),
        format: 'iife',
    }),
    esbuild.build({
        ...common,
        entryPoints: [path.join(here, 'styles', 'v2.css')],
        outfile: path.join(outDir, 'v2.css'),
    }),
]);
