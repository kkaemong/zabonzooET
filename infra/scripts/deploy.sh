#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage: ./infra/scripts/deploy.sh <docker-image>

Optional environment variables:
  AWS_SECRET_ID        AWS Secrets Manager secret id (for dynamic .env.prod generation)
  AWS_REGION           AWS region for secret lookup (default: ap-northeast-2)
  REGISTRY_HOST        Docker registry host for login
  REGISTRY_USERNAME    Docker registry username
  REGISTRY_PASSWORD    Docker registry password
EOF
}

IMAGE="${1:-}"
if [ -z "$IMAGE" ]; then
  usage
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
INFRA_DIR="$ROOT_DIR/infra"
COMPOSE_FILE="$INFRA_DIR/docker-compose.prod.yml"
ENV_FILE="$INFRA_DIR/.env.prod"

compose_prod() {
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" "$@"
}

echo "[deploy] image: $IMAGE"
echo "[deploy] infra dir: $INFRA_DIR"

echo "[deploy] Rendering $ENV_FILE ..."
IMAGE_TAG="$IMAGE" AWS_SECRET_ID="${AWS_SECRET_ID:-}" AWS_REGION="${AWS_REGION:-ap-northeast-2}" \
  "$INFRA_DIR/scripts/render-prod-env.sh" "$ENV_FILE"

if [ -n "${REGISTRY_HOST:-}" ] && [ -n "${REGISTRY_USERNAME:-}" ] && [ -n "${REGISTRY_PASSWORD:-}" ]; then
  echo "[deploy] Logging into custom registry: $REGISTRY_HOST"
  echo "$REGISTRY_PASSWORD" | docker login "$REGISTRY_HOST" -u "$REGISTRY_USERNAME" --password-stdin
elif [ -n "${CI_REGISTRY:-}" ] && [ -n "${CI_REGISTRY_USER:-}" ] && [ -n "${CI_REGISTRY_PASSWORD:-}" ]; then
  echo "[deploy] Logging into GitLab registry: $CI_REGISTRY"
  echo "$CI_REGISTRY_PASSWORD" | docker login "$CI_REGISTRY" -u "$CI_REGISTRY_USER" --password-stdin
else
  echo "[deploy] Registry login skipped (public image or pre-authenticated host)."
fi

echo "[deploy] Pulling images ..."
compose_prod pull

echo "[deploy] Applying compose stack ..."
compose_prod up -d --remove-orphans

echo "[deploy] Waiting for running containers ..."
BACKEND_RUNNING=false
for i in $(seq 1 12); do
  if compose_prod ps --services --status running | grep -qx "backend"; then
    BACKEND_RUNNING=true
    break
  fi
  echo "[deploy] backend not running yet ($i/12)"
  sleep 5
done

if [ "$BACKEND_RUNNING" != true ]; then
  echo "==================================="
  echo "ERROR: backend container is not running after deploy."
  echo "==================================="
  compose_prod ps
  compose_prod logs backend --tail 200
  exit 1
fi

if command -v curl >/dev/null 2>&1; then
  echo "[deploy] Checking HTTP health endpoint ..."
  HTTP_HEALTHY=false
  for i in $(seq 1 15); do
    if curl -fsS http://127.0.0.1/health >/dev/null; then
      HTTP_HEALTHY=true
      break
    fi
    echo "[deploy] /health not ready yet ($i/15)"
    sleep 2
  done

  if [ "$HTTP_HEALTHY" != true ]; then
    echo "==================================="
    echo "ERROR: nginx /health check failed."
    echo "==================================="
    compose_prod ps
    compose_prod logs nginx --tail 200
    compose_prod logs backend --tail 200
    exit 1
  fi
else
  echo "[deploy] curl is not installed. Skipping HTTP health check."
fi

docker image prune -f
echo "==================================="
echo "Deployment successful: $IMAGE"
echo "==================================="
