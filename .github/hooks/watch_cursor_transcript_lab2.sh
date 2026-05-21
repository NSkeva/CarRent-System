#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
PID_FILE="$REPO_ROOT/lab2/.ai_transcript_watch.pid"
INTERVAL_SECONDS="${1:-3}"

sync_once() {
  bash "$REPO_ROOT/.github/hooks/export_cursor_transcript_lab2.sh" >/dev/null
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
