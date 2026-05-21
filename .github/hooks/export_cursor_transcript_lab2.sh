#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
OUT_FILE="$REPO_ROOT/lab2/ai_conversation.jsonl"
# Lab 2 razgovor u glavnom transkriptu: linije 1-143 (prije lab-3 upita).
LAB2_START_LINE="${LAB2_START_LINE:-1}"
LAB2_END_LINE="${LAB2_END_LINE:-143}"

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

cursor_export_transcript_slice "$TRANSCRIPT" "$OUT_FILE" "$LAB2_START_LINE" "$LAB2_END_LINE"
echo "Exported Lab 2 transcript slice to: $OUT_FILE ($(wc -l <"$OUT_FILE") lines)"
