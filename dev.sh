#!/usr/bin/env bash
# Start the local editor+player dev environment.
#
# Usage:
#   ./dev.sh [--release] [--api-proxy <url>]
#
# Options:
#   --release           Build in Release mode (AOT-compiled WasmPlayer, ~15s extra)
#   --api-proxy <url>   Proxy /api requests to <url> (e.g. http://localhost:5043
#                       for a local textadventures.co.uk instance)
#
# Servers started:
#   http://localhost:5174   WasmEditor (Vite / SvelteKit)
#   http://localhost:5175   WasmPlayer (static dev server)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

API_PROXY=""
RELEASE=false
while [[ $# -gt 0 ]]; do
    case "$1" in
        --release) RELEASE=true; shift;;
        --api-proxy) API_PROXY="$2"; shift 2;;
        *) echo "Unknown argument: $1" >&2; exit 1;;
    esac
done

echo "Building WasmEditor..."
if [[ "$RELEASE" == true ]]; then
    dotnet build src/WasmEditor/WasmEditor.csproj --configuration Release
else
    dotnet build src/WasmEditor/WasmEditor.csproj
fi

echo ""
echo "Building WasmPlayer..."
if [[ "$RELEASE" == true ]]; then
    dotnet build src/WasmPlayer/WasmPlayer.csproj --configuration Release
else
    dotnet build src/WasmPlayer/WasmPlayer.csproj
fi

echo ""
echo "Starting dev servers..."
echo ""

if [[ "$RELEASE" == true ]]; then
    node src/WasmPlayer/dev-server.mjs --release &
else
    node src/WasmPlayer/dev-server.mjs &
fi
PLAYER_PID=$!

VITE_ENV=(
    "PUBLIC_WASM_PLAYER_URL=http://localhost:5174/player/"
)
if [[ -n "$API_PROXY" ]]; then
    VITE_ENV+=("VITE_API_PROXY=$API_PROXY")
fi

env "${VITE_ENV[@]}" npm --prefix src/WebEditor run dev &
EDITOR_PID=$!

cleanup() {
    echo ""
    echo "Shutting down..."
    kill "$PLAYER_PID" "$EDITOR_PID" 2>/dev/null || true
    wait "$PLAYER_PID" "$EDITOR_PID" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "  WasmEditor:  http://localhost:5174"
echo "  WasmPlayer:  http://localhost:5175/?game=/examples/simple.aslx"
[[ -n "$API_PROXY" ]] && echo "  API proxy →  $API_PROXY"
echo ""
echo "Ctrl+C to stop."
echo ""

wait "$PLAYER_PID" "$EDITOR_PID"
