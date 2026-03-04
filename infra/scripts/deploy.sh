#!/usr/bin/env bash
set -euo pipefail

IMAGE="${1:-}"
if [ -z "$IMAGE" ]; then
  echo "Usage: ./infra/scripts/deploy.sh <docker-image>"
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
INFRA_DIR="$ROOT_DIR/infra"
ENV_FILE="$INFRA_DIR/.env.prod"
COMPOSE_FILE="$INFRA_DIR/docker-compose.prod.yml"

if [ ! -f "$ENV_FILE" ]; then
  cp "$INFRA_DIR/docs/templates/prod.env.example" "$ENV_FILE"
  chmod 600 "$ENV_FILE"
  echo "[deploy] created $ENV_FILE from template. Update secrets before next deploy."
fi

if ! grep -q '^IMAGE_TAG=' "$ENV_FILE"; then
  echo "IMAGE_TAG=$IMAGE" >> "$ENV_FILE"
else
  sed -i "s|^IMAGE_TAG=.*$|IMAGE_TAG=$IMAGE|" "$ENV_FILE"
fi

if [ -n "${CI_REGISTRY:-}" ] && [ -n "${CI_REGISTRY_USER:-}" ] && [ -n "${CI_REGISTRY_PASSWORD:-}" ]; then
  echo "$CI_REGISTRY_PASSWORD" | docker login "$CI_REGISTRY" -u "$CI_REGISTRY_USER" --password-stdin
fi

cd "$INFRA_DIR"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" pull
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --remove-orphans

for i in $(seq 1 20); do
  if curl -fsS http://127.0.0.1/health >/dev/null; then
    echo "[deploy] health check passed"
    exit 0
  fi
  echo "[deploy] waiting for health... ($i/20)"
  sleep 2
done

echo "[deploy] health check failed"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" logs --tail 120
exit 1
