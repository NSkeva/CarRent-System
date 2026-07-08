#!/usr/bin/env bash
# Build i deploy na Google Cloud Run (zahtijeva gcloud CLI + Docker).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT_ID="${GCP_PROJECT_ID:?Postavi GCP_PROJECT_ID}"
REGION="${GCP_REGION:-europe-west1}"
SERVICE="${GCP_SERVICE_NAME:-carrent-web}"
IMAGE="gcr.io/${PROJECT_ID}/${SERVICE}:latest"

echo "Build Docker image..."
docker build -t "$IMAGE" "$ROOT"

echo "Push..."
docker push "$IMAGE"

echo "Deploy Cloud Run..."
gcloud run deploy "$SERVICE" \
  --image "$IMAGE" \
  --platform managed \
  --region "$REGION" \
  --allow-unauthenticated \
  --port 8080 \
  --memory 512Mi

echo "Gotovo. URL:"
gcloud run services describe "$SERVICE" --region "$REGION" --format='value(status.url)'
