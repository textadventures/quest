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

# Check both ports before building, so a clash fails in seconds rather than after
# the build. Otherwise the server that can't bind dies while the other carries on:
# Vite would then proxy /player to whatever already holds the player port, which
# is usually another checkout's or worktree's dev server, serving its own build.
port_holder() {
    # Prints "<command> (PID <pid>)" for the process listening on $1, if any.
    lsof -nP -iTCP:"$1" -sTCP:LISTEN 2>/dev/null | awk 'NR == 2 { print $1 " (PID " $2 ")" }'
}
port_in_use() {
    if command -v lsof >/dev/null 2>&1; then
        [[ -n "$(port_holder "$1")" ]]
    else
        (exec 3<>"/dev/tcp/127.0.0.1/$1") 2>/dev/null
    fi
}
PORTS_OK=true
for port in "$APPSHELL_PORT" "$PLAYER_PORT"; do
    if port_in_use "$port"; then
        holder="$(port_holder "$port" || true)"
        echo "Port $port is already in use${holder:+ by $holder}." >&2
        PORTS_OK=false
    fi
done
if [[ "$PORTS_OK" != true ]]; then
    echo "" >&2
    echo "Another dev server may be running, perhaps from another checkout or worktree." >&2
    echo "Stop it, or pick other ports with --port <n> (AppShell on <n>, WasmPlayer on <n>+1)," >&2
    echo "e.g. ./dev.sh --port $((APPSHELL_PORT + 10))" >&2
    exit 1
fi

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

# Kills a process and its descendants (npm runs Vite as a child, which a plain
# kill of npm's PID would leave running, still holding the port).
kill_tree() {
    local child
    for child in $(pgrep -P "$1" 2>/dev/null); do
        kill_tree "$child"
    done
    kill "$1" 2>/dev/null || true
}

cleanup() {
    trap - EXIT INT TERM
    echo ""
    echo "Shutting down..."
    kill_tree "$PLAYER_PID"
    kill_tree "$EDITOR_PID"
    wait "$PLAYER_PID" "$EDITOR_PID" 2>/dev/null || true
}
trap cleanup EXIT
trap 'cleanup; exit 130' INT
trap 'cleanup; exit 143' TERM

echo "  AppShell:    http://localhost:$APPSHELL_PORT"
echo "  WasmPlayer:  http://localhost:$PLAYER_PORT/?url=/examples/simple.aslx"
echo ""
echo "Ctrl+C to stop."
echo ""

# Run until either server exits, then stop the other one too, rather than carry on
# half-started. (`wait -n` would do this, but needs bash 4.3; macOS ships 3.2.)
while kill -0 "$PLAYER_PID" 2>/dev/null && kill -0 "$EDITOR_PID" 2>/dev/null; do
    sleep 1
done
if ! kill -0 "$PLAYER_PID" 2>/dev/null; then
    STOPPED="WasmPlayer dev server (port $PLAYER_PORT)"
    STATUS=0; wait "$PLAYER_PID" 2>/dev/null || STATUS=$?
else
    STOPPED="AppShell dev server (port $APPSHELL_PORT)"
    STATUS=0; wait "$EDITOR_PID" 2>/dev/null || STATUS=$?
fi
echo "" >&2
echo "The $STOPPED exited with status $STATUS; stopping the other server." >&2
exit 1
