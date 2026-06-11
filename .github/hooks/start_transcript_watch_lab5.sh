#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
PID_FILE="$REPO_ROOT/lab-5/.ai_transcript_watch.pid"

if [[ -f "$PID_FILE" ]]; then
  existing_pid="$(cat "$PID_FILE" || true)"
  if [[ -n "${existing_pid:-}" ]] && kill -0 "$existing_pid" 2>/dev/null; then
    echo "Transcript watch vec radi (PID: $existing_pid)."
    exit 0
  fi
fi

nohup bash "$REPO_ROOT/.github/hooks/watch_cursor_transcript_lab5.sh" 3 >/dev/null 2>&1 &
sleep 0.2

if [[ -f "$PID_FILE" ]]; then
  echo "Transcript watch pokrenut (PID: $(cat "$PID_FILE"))."
else
  echo "Ne mogu potvrditi pokretanje watch procesa." >&2
  exit 1
fi
