#!/usr/bin/env node
// Packages the app via electron-builder's own Node API rather than its CLI,
// so injecting the repo's real VERSION doesn't rely on shell command
// substitution ($(cat ...)) — that's bash-only and breaks on Windows CI
// runners, where npm scripts run under cmd.exe by default.
//
// build({ config: {...} }) merges with package.json's "build" field the same
// way the CLI's -c/--config dot-path flags do (same underlying function).

import { copyFileSync, mkdirSync, readFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { build } from "electron-builder";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, "../../..");
const version = readFileSync(path.join(repoRoot, "VERSION"), "utf8").trim();

// Prerelease builds (RELEASE_CHANNEL=prerelease, set by electron-publish.yml
// from .github/scripts/release-channel.sh) are packaged as a separate app,
// "Quest Viva Beta", so installing one never replaces or shares data with the
// stable app. Each override covers a different place the identity leaks
// through:
// - appId: the macOS bundle ID, and the GUID of the Windows uninstall entry.
// - name: the Windows install directory (%LOCALAPPDATA%\Programs\<name>
//   for the default one-click installer), the .deb package name and the
//   Linux executable name.
// - productName: the app bundle, shortcut and artifact names, plus the
//   runtime app name, which also picks the userData directory (see main.ts).
// - desktopName: the Linux .desktop file and WM_CLASS, which must match each
//   other (see build/README.md), and must differ from the stable app's.
// - mac/win/linux icon: the purple "BETA" icon set in build/beta.
const isBeta = process.env.RELEASE_CHANNEL === "prerelease";
const channelConfig = isBeta
    ? {
          appId: "com.questviva.desktop.beta",
          mac: { icon: "build/beta/icon.icns" },
          win: { icon: "build/beta/icon.ico" },
          linux: { icon: "build/beta/icons" },
          extraMetadata: {
              name: "quest-viva-desktop-beta",
              productName: "Quest Viva Beta",
              desktopName: "quest-viva-beta.desktop",
          },
      }
    : {
          // Stable builds also produce a Microsoft Store package (.appx) on
          // Windows, alongside the NSIS installer. It's unsigned: the Store
          // signs it after certification, which is the point (no SmartScreen
          // warning, no code-signing certificate of our own). That also means
          // it can't be installed outside the Store, so electron-publish.yml
          // keeps it as a workflow artifact and never attaches it to the
          // GitHub Release. Its identity (package.json's "appx" block) belongs
          // to the reserved "Quest Viva" Store listing, so it's never built
          // for the beta, which has its own appId. electron-builder
          // concatenates arrays when merging config, so this adds to
          // package.json's nsis target rather than replacing it, and only
          // takes effect when packaging on Windows. The package version is
          // VERSION's major.minor.patch plus ".0", the form the Store needs.
          // x64 and arm64 make two .appx files with the same identity and
          // version, which Partner Center accepts as one submission, so
          // Windows on Arm gets a native build instead of x64 under
          // emulation. (The NSIS installer stays x64-only; see "Microsoft
          // Store" in docs/electron-desktop-app.md.)
          win: { target: [{ target: "appx", arch: ["x64", "arm64"] }] },
          extraMetadata: {},
      };

// The About panel (and, on Linux, the window) icon ships as an extraResource
// (see aboutIconPath() in main.ts). electron-builder concatenates array
// settings like extraResources when merging config rather than replacing them,
// so the beta can't swap that entry out here. Instead, package.json points it
// at this staging copy, and we fill it with the right channel's icon.
const iconDir = path.join(__dirname, "..", "build", ...(isBeta ? ["beta"] : []), "icons");
const stagedIconDir = path.join(__dirname, "..", "resources", "app-icon");
mkdirSync(stagedIconDir, { recursive: true });
copyFileSync(path.join(iconDir, "512x512.png"), path.join(stagedIconDir, "icon.png"));

try {
    // electron-builder defaults to implicit publishing when it detects CI
    // (deprecated in v27, warns without this) — the actual GitHub Release
    // upload is handled separately by electron-publish.yml's own
    // `gh release upload` step, so electron-builder itself should never
    // publish anything.
    await build({
        publish: "never",
        config: { ...channelConfig, extraMetadata: { ...channelConfig.extraMetadata, version } },
    });
} catch (err) {
    console.error(err);
    process.exit(1);
}
