#!/usr/bin/env bash
# Postavi Google OAuth Client ID i Secret u dotnet user-secrets.
#
# Prije pokretanja:
# 1. Google Cloud Console → APIs & Services → Credentials → OAuth 2.0 Client ID (Web)
# 2. Authorized redirect URI: http://localhost:5000/signin-google
#
# Upotreba:
#   ./scripts/setup-google-secrets.sh "CLIENT_ID.apps.googleusercontent.com" "CLIENT_SECRET"

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/src/CarRent.Web/CarRent.Web.csproj"

CLIENT_ID="${1:?Navedi Google Client ID}"
CLIENT_SECRET="${2:?Navedi Google Client Secret}"

dotnet user-secrets set "Authentication:Google:ClientId" "$CLIENT_ID" --project "$PROJECT"
dotnet user-secrets set "Authentication:Google:ClientSecret" "$CLIENT_SECRET" --project "$PROJECT"

echo ""
echo "Google OAuth postavljen u user-secrets."
echo "Restart: ./scripts/run-local.sh"
echo "Login:   http://localhost:5000/Identity/Account/Login"
echo ""
echo "Redirect URI u Google Console mora biti:"
echo "  http://localhost:5000/signin-google"
