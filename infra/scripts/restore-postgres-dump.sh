#!/usr/bin/env bash
set -euo pipefail

DUMP_FILE="${1:-}"

if [ -z "$DUMP_FILE" ]; then
  cat <<'USAGE'
Usage: ./infra/scripts/restore-postgres-dump.sh <dump-file>

Supported formats:
  - plain SQL (.sql)
  - custom pg_dump archive (.dump, .backup)
USAGE
  exit 1
fi

if [ ! -f "$DUMP_FILE" ]; then
  echo "[restore-db] dump file not found: $DUMP_FILE"
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
INFRA_DIR="$ROOT_DIR/infra"
ENV_FILE="$INFRA_DIR/.env.backend.prod"
COMPOSE_FILE="$INFRA_DIR/docker-compose.backend.yml"

if [ ! -f "$ENV_FILE" ]; then
  echo "[restore-db] missing env file: $ENV_FILE"
  exit 1
fi

read_env_value() {
  local key="$1"
  local default_value="${2:-}"
  local value
  value="$(grep -E "^${key}=" "$ENV_FILE" | head -n 1 | cut -d '=' -f 2- || true)"
  if [ -z "$value" ]; then
    value="$default_value"
  fi
  value="${value%\"}"
  value="${value#\"}"
  printf '%s' "$value"
}

POSTGRES_USER="$(read_env_value POSTGRES_USER A507)"
POSTGRES_DB="$(read_env_value POSTGRES_DB amagetdon)"

cd "$INFRA_DIR"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --wait local-postgres

CONTAINER_NAME="local-postgres"
CONTAINER_DUMP_PATH="/tmp/restore-input$(basename "$DUMP_FILE")"

docker cp "$DUMP_FILE" "${CONTAINER_NAME}:${CONTAINER_DUMP_PATH}"

case "$DUMP_FILE" in
  *.sql)
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T local-postgres \
      psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f "$CONTAINER_DUMP_PATH"
    ;;
  *.dump|*.backup)
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T local-postgres \
      pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists --no-owner --no-privileges "$CONTAINER_DUMP_PATH"
    ;;
  *)
    echo "[restore-db] unsupported dump format: $DUMP_FILE"
    exit 1
    ;;
esac

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T local-postgres rm -f "$CONTAINER_DUMP_PATH"

echo "[restore-db] restore completed"
