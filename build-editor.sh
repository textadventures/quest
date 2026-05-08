#!/usr/bin/env bash
set -e

echo "Building WasmEditor..."
dotnet build src/WasmEditor/WasmEditor.csproj

echo ""
echo "Done. Refresh http://localhost:5174 to pick up the new build."
