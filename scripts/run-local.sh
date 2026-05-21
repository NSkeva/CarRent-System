#!/usr/bin/env bash
# Pokrece web app + lokalnu SQLite bazu (migracije + seed pri startu).
# Nije potreban Docker ni zasebni database server.

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ -x "$ROOT/.dotnet/dotnet" ]]; then
  export DOTNET_ROOT="$ROOT/.dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
  DOTNET=( "$ROOT/.dotnet/dotnet" )
else
  DOTNET=( dotnet )
fi

export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-$ROOT/.dotnet-home}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-$ROOT/.nuget/packages}"

echo "CarRent — pokretanje (SQLite lokalno, bez Dockera)..."
echo "Baza: src/CarRent.Web/Data/carrent.dev.db (kreira se / migrira pri startu)"
echo "URL:  http://localhost:5000 (ili port iz launchSettings)"
echo ""

exec "${DOTNET[@]}" run --project "$ROOT/src/CarRent.Web/CarRent.Web.csproj" "$@"
