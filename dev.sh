#!/usr/bin/env bash
# Start the local editor+player dev environment.
#
# Usage:
#   ./dev.sh [--release] [--api-proxy <url>]
#
# Options:
#   --release           Build in Release mode (AOT-compiled WasmPlayer, ~15s extra)
#   --api-proxy <url>   Proxy /api requests to <url> (e.g. http://localhost:5043
#                       for a local textadventures.co.uk instance). Also switches
#                       the editor to server-save-only mode (textadventures.co.uk
#                       behaviour, PUBLIC_HAS_SERVER=true) and hides the
#                       Play/Create Home content (PUBLIC_SHOW_HOME=false) —
#                       textadventures.co.uk has its own site navigation and
#                       doesn't want a game-browser homepage. Default (no flag)
#                       is play.questviva.com behaviour: local-only, Home shown.
#   --port <n>          AppShell port (default 5174). WasmPlayer runs on <n>+1.
#                       Set this to a different value per checkout (e.g. when
#                       reviewing a PR in a git worktree alongside your main
#                       checkout) so the two dev.sh instances don't collide.
#
# Servers started (ports shift together with --port):
#   http://localhost:5174   AppShell (Vite / SvelteKit) — root shows Play/Create
#                           Home when nothing's loaded (unless --api-proxy);
#                           /open is the Create tab's content; proxies /player
#                           to the WasmPlayer server below (vite.config.ts)
#   http://localhost:5175   WasmPlayer (static dev server)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

API_PROXY=""
RELEASE=false
APPSHELL_PORT=5174
while [[ $# -gt 0 ]]; do
    case "$1" in
        --release) RELEASE=true; shift;;
        --api-proxy) API_PROXY="$2"; shift 2;;
        --port) APPSHELL_PORT="$2"; shift 2;;
        *) echo "Unknown argument: $1" >&2; exit 1;;
    esac
done
PLAYER_PORT=$((APPSHELL_PORT + 1))

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

# --api-proxy simulates textadventures.co.uk (server-save-only, no Home).
# Without it, this simulates play.questviva.com (local-only, Home shown).
SHOW_HOME=true
if [[ -n "$API_PROXY" ]]; then
    SHOW_HOME=false
fi

VITE_ENV=(
    "PORT=$APPSHELL_PORT"
    "WASM_PLAYER_PORT=$PLAYER_PORT"
    "PUBLIC_WASM_PLAYER_URL=http://localhost:$APPSHELL_PORT/player/"
    "PUBLIC_SHOW_HOME=$SHOW_HOME"
)
if [[ "$RELEASE" == true ]]; then
    VITE_ENV+=("WASM_CONFIG=Release")
fi
if [[ -n "$API_PROXY" ]]; then
    VITE_ENV+=("VITE_API_PROXY=$API_PROXY")
    VITE_ENV+=("PUBLIC_HAS_SERVER=true")
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
echo "  WasmPlayer:  http://localhost:$PLAYER_PORT/?game=/examples/simple.aslx"
[[ -n "$API_PROXY" ]] && echo "  API proxy →  $API_PROXY"
echo ""
echo "Ctrl+C to stop."
echo ""

wait "$PLAYER_PID" "$EDITOR_PID"
