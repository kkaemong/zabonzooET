#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
INFRA_DIR="$ROOT_DIR/infra"
OUTPUT_FILE="${1:-$INFRA_DIR/.env.prod}"
AWS_SECRET_ID="${AWS_SECRET_ID:-}"
AWS_REGION="${AWS_REGION:-ap-northeast-2}"
SECRET_JSON=""

require_cmd() {
  local cmd="$1"
  if ! command -v "$cmd" >/dev/null 2>&1; then
    echo "[render-prod-env] Missing required command: $cmd"
    exit 1
  fi
}

if [ -n "$AWS_SECRET_ID" ]; then
  require_cmd aws
  require_cmd jq
  echo "[render-prod-env] Loading secret from AWS Secrets Manager: $AWS_SECRET_ID ($AWS_REGION)"
  SECRET_JSON="$(aws secretsmanager get-secret-value \
    --secret-id "$AWS_SECRET_ID" \
    --query SecretString \
    --output text \
    --region "$AWS_REGION")"
fi

secret_value() {
  local key="$1"
  if [ -z "$SECRET_JSON" ]; then
    return 0
  fi
  jq -er --arg k "$key" '.[$k]' <<<"$SECRET_JSON" 2>/dev/null || true
}

resolve_value() {
  local env_key="$1"
  local secret_key="$2"
  local default_value="${3:-}"
  local current="${!env_key:-}"
  local from_secret

  if [ -n "$current" ]; then
    printf '%s' "$current"
    return
  fi

  from_secret="$(secret_value "$secret_key")"
  if [ -n "$from_secret" ]; then
    printf '%s' "$from_secret"
    return
  fi

  printf '%s' "$default_value"
}

SPRING_PROFILES_ACTIVE="$(resolve_value SPRING_PROFILES_ACTIVE spring_profiles_active prod)"
POSTGRES_HOST="$(resolve_value POSTGRES_HOST db_host)"
POSTGRES_PORT="$(resolve_value POSTGRES_PORT db_port 5432)"
POSTGRES_DB="$(resolve_value POSTGRES_DB db_name amagetdon)"
POSTGRES_USER="$(resolve_value POSTGRES_USER db_username A507)"
POSTGRES_PASSWORD="$(resolve_value POSTGRES_PASSWORD db_password)"

SPRING_DATA_REDIS_HOST="$(resolve_value SPRING_DATA_REDIS_HOST redis_host redis)"
SPRING_DATA_REDIS_PORT="$(resolve_value SPRING_DATA_REDIS_PORT redis_port 6379)"
SPRING_REDIS_HOST="${SPRING_REDIS_HOST:-$SPRING_DATA_REDIS_HOST}"
SPRING_REDIS_PORT="${SPRING_REDIS_PORT:-$SPRING_DATA_REDIS_PORT}"

SPRING_DATASOURCE_URL="${SPRING_DATASOURCE_URL:-jdbc:postgresql://${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DB}}"
SPRING_DATASOURCE_USERNAME="${SPRING_DATASOURCE_USERNAME:-$POSTGRES_USER}"
SPRING_DATASOURCE_PASSWORD="${SPRING_DATASOURCE_PASSWORD:-$POSTGRES_PASSWORD}"
IMAGE_TAG="${IMAGE_TAG:-amagetdon-backend:latest}"

required_keys=(
  POSTGRES_HOST
  POSTGRES_DB
  POSTGRES_USER
  POSTGRES_PASSWORD
  SPRING_DATASOURCE_URL
  SPRING_DATASOURCE_USERNAME
  SPRING_DATASOURCE_PASSWORD
)

for key in "${required_keys[@]}"; do
  if [ -z "${!key:-}" ]; then
    echo "[render-prod-env] Missing required value: $key"
    echo "[render-prod-env] Provide it via environment variable or AWS secret."
    exit 1
  fi
done

cat > "$OUTPUT_FILE" <<EOF
SPRING_PROFILES_ACTIVE=$SPRING_PROFILES_ACTIVE
IMAGE_TAG=$IMAGE_TAG

POSTGRES_HOST=$POSTGRES_HOST
POSTGRES_PORT=$POSTGRES_PORT
POSTGRES_DB=$POSTGRES_DB
POSTGRES_USER=$POSTGRES_USER
POSTGRES_PASSWORD=$POSTGRES_PASSWORD

SPRING_DATASOURCE_URL=$SPRING_DATASOURCE_URL
SPRING_DATASOURCE_USERNAME=$SPRING_DATASOURCE_USERNAME
SPRING_DATASOURCE_PASSWORD=$SPRING_DATASOURCE_PASSWORD

SPRING_DATA_REDIS_HOST=$SPRING_DATA_REDIS_HOST
SPRING_DATA_REDIS_PORT=$SPRING_DATA_REDIS_PORT
SPRING_REDIS_HOST=$SPRING_REDIS_HOST
SPRING_REDIS_PORT=$SPRING_REDIS_PORT
EOF

chmod 600 "$OUTPUT_FILE"
echo "[render-prod-env] Wrote: $OUTPUT_FILE"
