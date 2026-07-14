#!/usr/bin/env node
// Assembles the three static directories main.ts serves (editor/, AppBundle/,
// player/) — the same layout .github/workflows/deploy-play.yml produces for
// play.questviva.com, just copied locally instead of deployed behind nginx.
//
// Prerequisites (build these first):
//   dotnet build -c ${WASM_CONFIG:-Debug} src/WasmEditor
//   dotnet build -c ${WASM_CONFIG:-Debug} src/WasmPlayer
//   npm run build   (in src/WebEditor)
//
// WASM_CONFIG follows the same convention as src/WebEditor/vite.config.ts —
// defaults to Debug (fast interpreter build) for local iteration; set
// WASM_CONFIG=Release to bundle the AOT build instead.

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, "../../..");
const wasmConfig = process.env.WASM_CONFIG === "Release" ? "Release" : "Debug";

const sources = {
    editor: path.join(repoRoot, "src/WebEditor/build"),
    AppBundle: path.join(repoRoot, `src/WasmEditor/bin/${wasmConfig}/net10.0/browser-wasm/AppBundle`),
    player: path.join(repoRoot, `src/WasmPlayer/bin/${wasmConfig}/net10.0/browser-wasm/AppBundle`),
};

const destRoot = path.join(__dirname, "../resources/app-static");

for (const [name, src] of Object.entries(sources)) {
    if (!fs.existsSync(src)) {
        console.error(`Missing build output for "${name}": ${src}`);
        console.error("Build it first (see docs/electron-desktop-app.md) before running npm run build here.");
        process.exit(1);
    }
    const dest = path.join(destRoot, name);
    fs.rmSync(dest, { recursive: true, force: true });
    fs.cpSync(src, dest, { recursive: true });
    console.log(`Copied ${name}: ${src} -> ${dest}`);
}
