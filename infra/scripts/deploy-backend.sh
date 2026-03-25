#!/usr/bin/env bash
set -euo pipefail

BACKEND_IMAGE="${1:-${BACKEND_IMAGE:-}}"

if [ -z "$BACKEND_IMAGE" ]; then
  cat <<'USAGE'
Usage: ./infra/scripts/deploy-backend.sh <backend-image>

or

  export BACKEND_IMAGE=<backend-image>
  ./infra/scripts/deploy-backend.sh
USAGE
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
INFRA_DIR="$ROOT_DIR/infra"
ENV_FILE="$INFRA_DIR/.env.backend.prod"
ENV_TEMPLATE="$INFRA_DIR/docs/templates/backend.env.example"
COMPOSE_FILE="$INFRA_DIR/docker-compose.backend.yml"
SSL_CONF="$INFRA_DIR/ssl.conf"

if [ ! -f "$ENV_FILE" ]; then
  cp "$ENV_TEMPLATE" "$ENV_FILE"
  chmod 600 "$ENV_FILE"
  echo "[deploy-backend] created $ENV_FILE from template"
fi

if [ ! -f "$SSL_CONF" ]; then
  cat > "$SSL_CONF" <<'EOF'
# SSL is not configured yet.
# Replace this file after certbot or your TLS setup is ready.
EOF
  chmod 644 "$SSL_CONF"
  echo "[deploy-backend] created $SSL_CONF placeholder"
fi

if ! grep -q '^BACKEND_IMAGE=' "$ENV_FILE"; then
  echo "BACKEND_IMAGE=$BACKEND_IMAGE" >> "$ENV_FILE"
else
  sed -i "s|^BACKEND_IMAGE=.*$|BACKEND_IMAGE=$BACKEND_IMAGE|" "$ENV_FILE"
fi

if [ -n "${CI_REGISTRY:-}" ] && [ -n "${CI_REGISTRY_USER:-}" ] && [ -n "${CI_REGISTRY_PASSWORD:-}" ]; then
  echo "$CI_REGISTRY_PASSWORD" | docker login "$CI_REGISTRY" -u "$CI_REGISTRY_USER" --password-stdin
fi

cd "$INFRA_DIR"
if docker image inspect "$BACKEND_IMAGE" >/dev/null 2>&1; then
  echo "[deploy-backend] using local image $BACKEND_IMAGE"
else
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" pull
fi
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --remove-orphans

check_backend_health() {
  curl -fsS http://127.0.0.1/actuator/health >/dev/null || \
    curl -fsS http://127.0.0.1/api/actuator/health >/dev/null
}

for i in $(seq 1 30); do
  if check_backend_health; then
    echo "[deploy-backend] backend health check passed"
    exit 0
  fi
  echo "[deploy-backend] waiting for backend health... ($i/30)"
  sleep 2
done

echo "[deploy-backend] backend health check failed"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" logs --tail 120
exit 1
