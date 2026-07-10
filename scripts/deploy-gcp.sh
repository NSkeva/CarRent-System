#!/usr/bin/env bash
# Build i deploy na Google Cloud Run (zahtijeva gcloud CLI + Docker).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

resolve_gcloud() {
  if command -v gcloud >/dev/null 2>&1; then
    command -v gcloud
    return
  fi
  if [[ -x "$HOME/google-cloud-sdk/bin/gcloud" ]]; then
    echo "$HOME/google-cloud-sdk/bin/gcloud"
    return
  fi
  echo ""
}

GCLOUD="$(resolve_gcloud)"
if [[ -z "$GCLOUD" ]]; then
  echo "gcloud nije pronađen."
  echo "  Instaliraj: ./scripts/install-gcloud.sh"
  echo "  Ili deploy bez gcloud: ./scripts/push-dockerhub.sh + GCP konzola (vidi FULL-07 §4B-alt)"
  exit 1
fi

PROJECT_ID="${GCP_PROJECT_ID:?Postavi GCP_PROJECT_ID}"
REGION="${GCP_REGION:-europe-west1}"
SERVICE="${GCP_SERVICE_NAME:-carrent-web}"
IMAGE="gcr.io/${PROJECT_ID}/${SERVICE}:latest"

echo "=== CarRent deploy → Cloud Run ==="
echo "Project: $PROJECT_ID"
echo "Region:  $REGION"
echo "Service: $SERVICE"
echo ""

echo "[1/3] Docker build..."
docker build -t "$IMAGE" "$ROOT"

echo "[2/3] Docker push (gcloud auth configure-docker ako prvi put)..."
"$GCLOUD" auth configure-docker gcr.io --quiet 2>/dev/null || true
docker push "$IMAGE"

echo "[3/3] Cloud Run deploy..."
"$GCLOUD" run deploy "$SERVICE" \
  --image "$IMAGE" \
  --platform managed \
  --region "$REGION" \
  --allow-unauthenticated \
  --port 8080 \
  --memory 512Mi \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,DatabaseProvider=Sqlite"

URL="$("$GCLOUD" run services describe "$SERVICE" --region "$REGION" --format='value(status.url)')"
echo ""
echo "============================================"
echo "  Deploy gotov!"
echo "  Javni URL: $URL"
echo "============================================"
echo ""
echo "Demo login (seed): admin@carrent.local / Admin123!"
echo "Javni AI asistent: $URL/asistent"
echo ""
echo "Zalijepi URL u FULL PROJECT/FULL-07-DEPLOY-CLOUD.md § Deploy URL"
