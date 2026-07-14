#!/usr/bin/env bash
# Build and launch the Quest Viva desktop app (Electron shell + WebEditor +
# WasmEditor/WasmPlayer), Phase 1 of docs/electron-desktop-app.md.
#
# Usage:
#   ./electron.sh [--release]
#
# Options:
#   --release   Build WasmEditor/WasmPlayer in Release mode (AOT-compiled,
#               slower build) instead of the default Debug interpreter build.
#
# Unlike dev.sh, this doesn't run dev servers — it builds the static bundles
# once, assembles them into src/ElectronApp/resources/app-static (same
# editor/AppBundle/player layout deploy-play.yml produces), and launches the
# packaged app. Re-run after changing WebEditor/WasmEditor/WasmPlayer source.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

RELEASE=false
while [[ $# -gt 0 ]]; do
    case "$1" in
        --release) RELEASE=true; shift;;
        *) echo "Unknown argument: $1" >&2; exit 1;;
    esac
done

DOTNET_CONFIG="Debug"
[[ "$RELEASE" == true ]] && DOTNET_CONFIG="Release"

echo "Building WasmEditor ($DOTNET_CONFIG)..."
dotnet build src/WasmEditor/WasmEditor.csproj --configuration "$DOTNET_CONFIG"

echo ""
echo "Building WasmPlayer ($DOTNET_CONFIG)..."
dotnet build src/WasmPlayer/WasmPlayer.csproj --configuration "$DOTNET_CONFIG"

echo ""
echo "Building WebEditor..."
[[ -d src/WebEditor/node_modules ]] || npm --prefix src/WebEditor install
npm --prefix src/WebEditor run build

echo ""
echo "Building ElectronApp..."
[[ -d src/ElectronApp/node_modules ]] || npm --prefix src/ElectronApp install
if [[ "$RELEASE" == true ]]; then
    WASM_CONFIG=Release npm --prefix src/ElectronApp run build
else
    npm --prefix src/ElectronApp run build
fi

echo ""
echo "Launching Quest Viva desktop app..."
echo ""
# `npm --prefix ... exec` doesn't chdir the invoked command (unlike `npm run`),
# and electron needs "." to resolve to src/ElectronApp/package.json — so cd directly.
(cd src/ElectronApp && npx electron .)
