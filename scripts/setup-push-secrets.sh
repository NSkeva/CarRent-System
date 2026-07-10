#!/usr/bin/env bash
# Generira VAPID ključeve za Web Push i sprema privatni u user-secrets.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/src/CarRent.Web/CarRent.Web.csproj"
PATH="$ROOT/.dotnet:${PATH:-}"

if ! command -v npx >/dev/null 2>&1; then
  echo "Potreban je npx (Node.js) za generiranje VAPID ključeva."
  exit 1
fi

OUT=$(npx --yes web-push@3.6.7 generate-vapid-keys 2>/dev/null)
PUBLIC=$(echo "$OUT" | awk '/Public Key:/{getline; print; exit}')
PRIVATE=$(echo "$OUT" | awk '/Private Key:/{getline; print; exit}')

if [[ -z "$PUBLIC" || -z "$PRIVATE" ]]; then
  echo "Generiranje VAPID ključeva nije uspjelo."
  exit 1
fi

dotnet user-secrets set "FleetNotifications:PushEnabled" "true" --project "$PROJECT"
dotnet user-secrets set "FleetNotifications:WebPush:Subject" "mailto:admin@carrent.local" --project "$PROJECT"
dotnet user-secrets set "FleetNotifications:WebPush:PublicKey" "$PUBLIC" --project "$PROJECT"
dotnet user-secrets set "FleetNotifications:WebPush:PrivateKey" "$PRIVATE" --project "$PROJECT"

echo ""
echo "Web Push VAPID postavljen u user-secrets."
echo "Public key (već u appsettings.Development.json ako si syncao):"
echo "  $PUBLIC"
echo ""
echo "Sljedeće: restart app → Operativa → Obavijesti → Uključi push → Pošalji pending"
