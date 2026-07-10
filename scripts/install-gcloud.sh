#!/usr/bin/env bash
# Instalira Google Cloud SDK (gcloud) u ~/google-cloud-sdk — bez pacmana/AUR.
# NAPOMENA: npm i gcloud NIJE ovo — to je stari npm paket, ne koristiti!
set -euo pipefail

INSTALL_DIR="${GCLOUD_HOME:-$HOME/google-cloud-sdk}"
ARCH="$(uname -m)"
case "$ARCH" in
  x86_64)  PKG="google-cloud-cli-linux-x86_64.tar.gz" ;;
  aarch64) PKG="google-cloud-cli-linux-arm.tar.gz" ;;
  *) echo "Nepodržana arhitektura: $ARCH"; exit 1 ;;
esac

URL="https://dl.google.com/dl/cloudsdk/channels/rapid/downloads/${PKG}"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

echo "Preuzimam Google Cloud SDK..."
curl -fsSL -o "$TMP/gcloud.tar.gz" "$URL"
tar -xf "$TMP/gcloud.tar.gz" -C "$TMP"

echo "Instalacija u $INSTALL_DIR ..."
rm -rf "$INSTALL_DIR"
mv "$TMP/google-cloud-sdk" "$INSTALL_DIR"

"$INSTALL_DIR/install.sh" \
  --quiet \
  --path-update false \
  --command-completion false \
  --usage-reporting false

GCLOUD="$INSTALL_DIR/bin/gcloud"
"$GCLOUD" version

echo ""
echo "============================================"
echo "  gcloud instaliran: $GCLOUD"
echo "============================================"
echo ""
echo "Dodaj u ~/.zshrc (jednom):"
echo "  export PATH=\"$INSTALL_DIR/bin:\$PATH\""
echo ""
echo "U NOVOM terminalu ili odmah:"
echo "  export PATH=\"$INSTALL_DIR/bin:\$PATH\""
echo "  gcloud auth login"
echo ""
echo "Deploy:"
echo "  ./scripts/setup-gcp-deploy.sh TVOJ_PROJECT_ID"
echo "  export GCP_PROJECT_ID=TVOJ_PROJECT_ID"
echo "  ./scripts/deploy-gcp.sh"
echo ""
echo "BEZ gcloud: ./scripts/push-dockerhub.sh + GCP web konzola (vidi FULL-07)"
