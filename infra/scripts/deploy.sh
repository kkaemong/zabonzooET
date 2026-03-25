#!/usr/bin/env bash
set -euo pipefail

BACKEND_IMAGE="${1:-}"
FRONTEND_IMAGE="${2:-${FRONTEND_IMAGE:-}}"

if [ -z "$BACKEND_IMAGE" ] || [ -z "$FRONTEND_IMAGE" ]; then
  cat <<USAGE
Usage: ./infra/scripts/deploy.sh <backend-image> <frontend-image>

or

  export FRONTEND_IMAGE=<frontend-image>
  ./infra/scripts/deploy.sh <backend-image>
USAGE
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
INFRA_DIR="$ROOT_DIR/infra"
ENV_FILE="$INFRA_DIR/.env.prod"
COMPOSE_FILE="$INFRA_DIR/docker-compose.prod.yml"
SSL_CONF="$INFRA_DIR/ssl.conf"

if [ ! -f "$ENV_FILE" ]; then
  cp "$INFRA_DIR/docs/templates/prod.env.example" "$ENV_FILE"
  chmod 600 "$ENV_FILE"
  echo "[deploy] created $ENV_FILE from template"
fi

if [ ! -f "$SSL_CONF" ]; then
  cat > "$SSL_CONF" <<'EOF'
# SSL not configured yet.
# If certbot is configured, replace this file with HTTPS server block.
EOF
  chmod 644 "$SSL_CONF"
  echo "[deploy] created $SSL_CONF placeholder"
fi

if ! grep -q '^BACKEND_IMAGE=' "$ENV_FILE"; then
  echo "BACKEND_IMAGE=$BACKEND_IMAGE" >> "$ENV_FILE"
else
  sed -i "s|^BACKEND_IMAGE=.*$|BACKEND_IMAGE=$BACKEND_IMAGE|" "$ENV_FILE"
fi

if ! grep -q '^FRONTEND_IMAGE=' "$ENV_FILE"; then
  echo "FRONTEND_IMAGE=$FRONTEND_IMAGE" >> "$ENV_FILE"
else
  sed -i "s|^FRONTEND_IMAGE=.*$|FRONTEND_IMAGE=$FRONTEND_IMAGE|" "$ENV_FILE"
fi

if [ -n "${CI_REGISTRY:-}" ] && [ -n "${CI_REGISTRY_USER:-}" ] && [ -n "${CI_REGISTRY_PASSWORD:-}" ]; then
  echo "$CI_REGISTRY_PASSWORD" | docker login "$CI_REGISTRY" -u "$CI_REGISTRY_USER" --password-stdin
fi

cd "$INFRA_DIR"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" pull
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --remove-orphans

check_backend_health() {
  curl -fsS http://127.0.0.1/api/health >/dev/null || \
    curl -fsS http://127.0.0.1/api/actuator/health >/dev/null
}

for i in $(seq 1 30); do
  if check_backend_health; then
    echo "[deploy] backend health check passed"
    exit 0
  fi
  echo "[deploy] waiting for backend health... ($i/30)"
  sleep 2
done

echo "[deploy] backend health check failed"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" logs --tail 120
exit 1
