#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
TRANSCRIPTS_ROOT="${CURSOR_TRANSCRIPTS_DIR:-$HOME/.cursor/projects/home-nskeva-Documents-Github-CarRent-System/agent-transcripts}"
OUT_FILE="$REPO_ROOT/lab2/ai_conversation.jsonl"

mkdir -p "$(dirname "$OUT_FILE")"

LATEST_FILE="$(ls -t "$TRANSCRIPTS_ROOT"/*/*.jsonl 2>/dev/null | head -n 1 || true)"

if [[ -z "$LATEST_FILE" ]]; then
  echo "No transcript file found in: $TRANSCRIPTS_ROOT" >&2
  exit 1
fi

cp "$LATEST_FILE" "$OUT_FILE"
echo "Exported transcript to: $OUT_FILE"
