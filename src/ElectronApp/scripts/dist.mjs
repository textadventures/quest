#!/usr/bin/env node
// Packages the app via electron-builder's own Node API rather than its CLI,
// so injecting the repo's real VERSION doesn't rely on shell command
// substitution ($(cat ...)) — that's bash-only and breaks on Windows CI
// runners, where npm scripts run under cmd.exe by default.
//
// build({ config: {...} }) merges with package.json's "build" field the same
// way the CLI's -c/--config dot-path flags do (same underlying function).

import { readFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { build } from "electron-builder";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, "../../..");
const version = readFileSync(path.join(repoRoot, "VERSION"), "utf8").trim();

try {
    await build({ config: { extraMetadata: { version } } });
} catch (err) {
    console.error(err);
    process.exit(1);
}
