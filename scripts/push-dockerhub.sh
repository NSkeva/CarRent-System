#!/usr/bin/env bash
# Push Docker image na Docker Hub — deploy na Cloud Run preko web konzole (bez lokalnog gcloud).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

DOCKER_USER="${DOCKERHUB_USER:?Postavi DOCKERHUB_USER (tvoj Docker Hub username)}"
TAG="${DOCKERHUB_TAG:-latest}"
IMAGE="docker.io/${DOCKER_USER}/carrent-web:${TAG}"

echo "=== Build + push na Docker Hub ==="
echo "Image: $IMAGE"
echo ""

docker build -t "$IMAGE" "$ROOT"

if ! docker info 2>/dev/null | grep -q "Username"; then
  echo "Prijavi se na Docker Hub:"
  docker login
fi

docker push "$IMAGE"

echo ""
echo "============================================"
echo "  Image pushan: $IMAGE"
echo "============================================"
echo ""
echo "Sljedeće — u browseru (BEZ gcloud CLI):"
echo ""
echo "  1. https://console.cloud.google.com/ → novi projekt"
echo "  2. Omogući 'Cloud Run API' (pretraži u Enable APIs)"
echo "  3. Cloud Run → Create service"
echo "  4. Container image URL: $IMAGE"
echo "  5. Port: 8080"
echo "  6. Authentication: Allow unauthenticated invocations"
echo "  7. Create → kopiraj javni URL"
echo ""
echo "Zalijepi URL u FULL PROJECT/FULL-07-DEPLOY-CLOUD.md §5"
