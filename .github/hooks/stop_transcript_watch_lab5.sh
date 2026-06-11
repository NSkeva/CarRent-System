#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
PID_FILE="$REPO_ROOT/lab-5/.ai_transcript_watch.pid"

if [[ ! -f "$PID_FILE" ]]; then
  echo "Transcript watch nije pokrenut."
  exit 0
fi

pid="$(cat "$PID_FILE" || true)"
if [[ -n "${pid:-}" ]] && kill -0 "$pid" 2>/dev/null; then
  kill "$pid"
  echo "Transcript watch zaustavljen (PID: $pid)."
else
  echo "Watch proces nije aktivan."
fi

rm -f "$PID_FILE"
