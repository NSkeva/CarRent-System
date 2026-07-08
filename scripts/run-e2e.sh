#!/usr/bin/env bash
# Instalira Playwright Chromium i pokreće E2E scenarij (13+ koraka).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ -x "$ROOT/.dotnet/dotnet" ]]; then
  export DOTNET_ROOT="$ROOT/.dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi

export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-$ROOT/.dotnet-home}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-$ROOT/.nuget/packages}"
export PLAYWRIGHT_BROWSERS_PATH="${PLAYWRIGHT_BROWSERS_PATH:-$ROOT/.playwright}"

if [[ ! -d "$PLAYWRIGHT_BROWSERS_PATH/chromium_headless_shell-"* ]]; then
  echo "Instalacija Playwright Chromium..."
  npx --yes playwright@1.50.0 install chromium
fi

dotnet test tests/CarRent.Web.E2E/CarRent.Web.E2E.csproj --filter "FullProject_Scenario" "$@"
