#!/usr/bin/env bash
# Start the local editor+player dev environment.
#
# Usage:
#   ./dev.sh [--release] [--port <n>]
#
# Options:
#   --release           Build in Release mode (AOT-compiled WasmPlayer, ~15s extra)
#   --port <n>          AppShell port (default 5174). WasmPlayer runs on <n>+1.
#                       Set this to a different value per checkout (e.g. when
#                       reviewing a PR in a git worktree alongside your main
#                       checkout) so the two dev.sh instances don't collide.
#
# Servers started (ports shift together with --port):
#   http://localhost:5174   AppShell (Vite / SvelteKit) — root is the Play tab,
#                           /open the Create tab and /edit the editor; proxies /player
#                           to the WasmPlayer server below (vite.config.ts)
#   http://localhost:5175   WasmPlayer (static dev server)

set -euo pipefail

# The dotnet CLI can hang after a build on its workload-update and telemetry
# network checks. Neither is needed here; set either to 0 beforehand to opt back in.
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE="${DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE:-1}"
export DOTNET_CLI_TELEMETRY_OPTOUT="${DOTNET_CLI_TELEMETRY_OPTOUT:-1}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

RELEASE=false
APPSHELL_PORT=5174
while [[ $# -gt 0 ]]; do
    case "$1" in
        --release) RELEASE=true; shift;;
        --port) APPSHELL_PORT="$2"; shift 2;;
        *) echo "Unknown argument: $1" >&2; exit 1;;
    esac
done
PLAYER_PORT=$((APPSHELL_PORT + 1))

echo "Building WasmEditor..."
if [[ "$RELEASE" == true ]]; then
    dotnet publish src/WasmEditor/WasmEditor.csproj --configuration Release
else
    dotnet build src/WasmEditor/WasmEditor.csproj
fi

echo ""
echo "Building WasmPlayer..."
if [[ "$RELEASE" == true ]]; then
    dotnet publish src/WasmPlayer/WasmPlayer.csproj --configuration Release
else
    dotnet build src/WasmPlayer/WasmPlayer.csproj
fi

if [[ ! -d src/AppShell/node_modules ]]; then
    echo ""
    echo "src/AppShell/node_modules missing, installing (fresh checkout/worktree)..."
    npm --prefix src/AppShell install
fi

echo ""
echo "Starting dev servers..."
echo ""

if [[ "$RELEASE" == true ]]; then
    WASM_PLAYER_PORT="$PLAYER_PORT" node src/WasmPlayer/dev-server.mjs --release &
else
    WASM_PLAYER_PORT="$PLAYER_PORT" node src/WasmPlayer/dev-server.mjs &
fi
PLAYER_PID=$!

VITE_ENV=(
    "PORT=$APPSHELL_PORT"
    "WASM_PLAYER_PORT=$PLAYER_PORT"
    "PUBLIC_WASM_PLAYER_URL=http://localhost:$APPSHELL_PORT/player/"
)
if [[ "$RELEASE" == true ]]; then
    VITE_ENV+=("WASM_CONFIG=Release")
fi

env "${VITE_ENV[@]}" npm --prefix src/AppShell run dev &
EDITOR_PID=$!

cleanup() {
    echo ""
    echo "Shutting down..."
    kill "$PLAYER_PID" "$EDITOR_PID" 2>/dev/null || true
    wait "$PLAYER_PID" "$EDITOR_PID" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "  AppShell:    http://localhost:$APPSHELL_PORT"
echo "  WasmPlayer:  http://localhost:$PLAYER_PORT/?url=/examples/simple.aslx"
echo ""
echo "Ctrl+C to stop."
echo ""

wait "$PLAYER_PID" "$EDITOR_PID"
