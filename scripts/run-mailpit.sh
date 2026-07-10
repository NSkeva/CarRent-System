#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker nije instaliran. Instaliraj Docker ili koristi stvarni SMTP (vidi FULL-09-EMAIL-OBAVESTI.md)."
  exit 1
fi

docker compose -f docker-compose.mailpit.yml up -d
echo ""
echo "Mailpit pokrenut:"
echo "  SMTP (slanje):  127.0.0.1:1025"
echo "  Web UI (inbox): http://localhost:8025"
echo ""
echo "appsettings.Development.json je već podešen za Mailpit."
echo "Pokreni app: ./scripts/run-local.sh"
