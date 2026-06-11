#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
OUT_FILE="$REPO_ROOT/lab-5/ai_conversation.jsonl"

# shellcheck source=/dev/null
source "$REPO_ROOT/.github/hooks/cursor_transcript_lib.sh"

ROOT="$(cursor_transcripts_root || true)"
if [[ -z "$ROOT" ]]; then
  echo "Cursor transcript folder not found." >&2
  exit 1
fi

TRANSCRIPT="$(cursor_latest_main_transcript "$ROOT" || true)"
if [[ -z "$TRANSCRIPT" ]]; then
  echo "No main transcript file found in: $ROOT" >&2
  exit 1
fi

cursor_export_transcript_by_markers \
  "$TRANSCRIPT" \
  "$OUT_FILE" \
  "Dodao sam lab-5"
