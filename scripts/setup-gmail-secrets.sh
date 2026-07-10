#!/usr/bin/env bash
# Postavi Gmail SMTP preko dotnet user-secrets (lozinka NE ide u git).
#
# Prije pokretanja:
# 1. Google račun → Sigurnost → 2FA uključena
# 2. App passwords → nova lozinka za "Mail" / "CarRent"
#
# Upotreba:
#   ./scripts/setup-gmail-secrets.sh tvoj@gmail.com "abcd efgh ijkl mnop"

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/src/CarRent.Web/CarRent.Web.csproj"
PATH="$ROOT/.dotnet:${PATH:-}"

GMAIL="${1:?Navedi Gmail adresu, npr. tvoj@gmail.com}"
APP_PASSWORD="${2:?Navedi Gmail App Password (16 znakova)}"
APP_PASSWORD="${APP_PASSWORD// /}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet nije pronađen. Koristi PATH s ./.dotnet ili instaliraj SDK."
  exit 1
fi

set_secret() {
  dotnet user-secrets set "$1" "$2" --project "$PROJECT"
}

set_secret "FleetNotifications:EmailEnabled" "true"
set_secret "FleetNotifications:DispatchEnabled" "true"
set_secret "FleetNotifications:DispatchIntervalSeconds" "30"
set_secret "FleetNotifications:DefaultRecipient" "$GMAIL"
set_secret "FleetNotifications:Smtp:Host" "smtp.gmail.com"
set_secret "FleetNotifications:Smtp:Port" "587"
set_secret "FleetNotifications:Smtp:UseSsl" "true"
set_secret "FleetNotifications:Smtp:Username" "$GMAIL"
set_secret "FleetNotifications:Smtp:Password" "$APP_PASSWORD"
set_secret "FleetNotifications:Smtp:FromAddress" "$GMAIL"
set_secret "FleetNotifications:Smtp:FromName" "CarRent"

echo ""
echo "Gmail SMTP postavljen u user-secrets (projekt: carrent-web-local-secrets)."
echo "Primatelj internih obavijesti (bez emaila kupca): $GMAIL"
echo ""
echo "Sljedeće:"
echo "  1. ./scripts/run-local.sh"
echo "  2. Prijava kao Admin/Manager"
echo "  3. Operativa → Obavijesti → Pošalji pending emailove"
echo "  4. Provjeri inbox na $GMAIL"
