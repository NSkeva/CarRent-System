#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
LOG_FILE="$REPO_ROOT/lab-4/agent_log.txt"

mkdir -p "$(dirname "$LOG_FILE")"

{
  echo "----- $(date '+%Y-%m-%d %H:%M:%S') -----"
  cat
  echo
} >> "$LOG_FILE"
