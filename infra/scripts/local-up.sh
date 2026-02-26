#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BE_DIR="$ROOT_DIR/BE"
ENV_FILE="$BE_DIR/.env"
ENV_TEMPLATE="$ROOT_DIR/infra/docs/templates/be.env.example"

if [ ! -f "$ENV_FILE" ]; then
  cp "$ENV_TEMPLATE" "$ENV_FILE"
  echo "[infra] BE/.env created from template: infra/docs/templates/be.env.example"
  echo "[infra] Update POSTGRES_PASSWORD in BE/.env if needed."
fi

cd "$BE_DIR"
docker compose up -d
docker compose ps
