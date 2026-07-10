#!/usr/bin/env bash
# Jednokratna priprema GCP projekta za deploy.
set -euo pipefail

GCLOUD="$(command -v gcloud 2>/dev/null || true)"
[[ -z "$GCLOUD" && -x "$HOME/google-cloud-sdk/bin/gcloud" ]] && GCLOUD="$HOME/google-cloud-sdk/bin/gcloud"

if [[ -z "$GCLOUD" ]]; then
  echo "gcloud nije pronađen. Pokreni: ./scripts/install-gcloud.sh"
  exit 1
fi

PROJECT_ID="${1:?Navedi GCP project ID, npr. ./scripts/setup-gcp-deploy.sh carrent-demo-123}"

"$GCLOUD" config set project "$PROJECT_ID"
"$GCLOUD" services enable run.googleapis.com containerregistry.googleapis.com artifactregistry.googleapis.com

echo ""
echo "GCP projekt $PROJECT_ID spreman za deploy."
echo ""
echo "Sljedeće:"
echo "  export GCP_PROJECT_ID=$PROJECT_ID"
echo "  export GCP_REGION=europe-west1"
echo "  ./scripts/deploy-gcp.sh"
