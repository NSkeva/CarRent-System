#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
TRANSCRIPTS_ROOT="${CURSOR_TRANSCRIPTS_DIR:-$HOME/.cursor/projects/home-nskeva-Documents-Github-CarRent-System/agent-transcripts}"
OUT_FILE="$REPO_ROOT/lab2/ai_conversation.jsonl"
PID_FILE="$REPO_ROOT/lab2/.ai_transcript_watch.pid"
INTERVAL_SECONDS="${1:-3}"

mkdir -p "$(dirname "$OUT_FILE")"

sync_once() {
  local latest_file=""
  latest_file="$(ls -t "$TRANSCRIPTS_ROOT"/*/*.jsonl 2>/dev/null | head -n 1 || true)"
  if [[ -n "$latest_file" ]]; then
    cp "$latest_file" "$OUT_FILE"
  fi
}

cleanup() {
  rm -f "$PID_FILE"
}

echo "$$" > "$PID_FILE"
trap cleanup EXIT INT TERM

while true; do
  sync_once
  sleep "$INTERVAL_SECONDS"
done
