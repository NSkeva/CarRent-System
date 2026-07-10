#!/usr/bin/env bash
# Lokalni test production Docker imagea (prije Cloud Run deploya).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
IMAGE="${CARRENT_IMAGE:-carrent-local:latest}"
PORT="${CARRENT_PORT:-8080}"

echo "Build image $IMAGE ..."
docker build -t "$IMAGE" "$ROOT"

docker rm -f carrent-local 2>/dev/null || true

echo "Pokretanje na http://localhost:$PORT ..."
docker run --name carrent-local -p "$PORT:8080" "$IMAGE"
