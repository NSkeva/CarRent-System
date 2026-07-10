#!/usr/bin/env bash
# Postavi OpenAI API ključ u user-secrets (opcionalno — bez ključa radi rule-based AI).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/src/CarRent.Web"

if [[ -z "${OPENAI_API_KEY:-}" ]]; then
  echo "Unesi OpenAI API ključ (sk-...):"
  read -rs OPENAI_API_KEY
  echo
fi

if [[ -z "$OPENAI_API_KEY" ]]; then
  echo "Nema ključa — preskačem. Rule-based chat i dalje radi."
  exit 0
fi

dotnet user-secrets set "OpenAI:ApiKey" "$OPENAI_API_KEY" --project "$PROJECT"
dotnet user-secrets set "OpenAI:Model" "${OPENAI_MODEL:-gpt-4o-mini}" --project "$PROJECT"

echo "OpenAI postavljen u user-secrets za CarRent.Web."
